using ClumsyWordsUniversal.Settings;
using Microsoft.Live;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClumsyWordsUniversal.Data
{
    public class CloudDataSource : DataSource
    {
        private readonly string _fileName = "definitions.json";
        private readonly string _folderName = "ClumsyWords";

        public static LiveAuthClient authClient;

        private string _userName;
        public string UserName
        {
            get { return this._userName; }
            set { this.SetProperty(ref this._userName, value); }
        }

        private string _imageUrl;
        public string ImageUrl
        {
            get { return this._imageUrl; }
            set { this.SetProperty(ref this._imageUrl, value); }
        }

        /// <summary>
        /// Initializes the data source taking care of permissions, user profile and data
        /// </summary>
        /// <returns></returns>
        public override async Task InitAsync()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                try
                {
                    authClient = new LiveAuthClient();
                    //LiveLoginResult loginResult = await authClient.InitializeAsync();

                    // Asks for the permissions needed by the user
                    LiveLoginResult authResult = await authClient.LoginAsync(new List<string>() { "wl.signin", "wl.basic", "wl.skydrive", "wl.skydrive_update" });
                    // Same as above using static property
                    //LiveLoginResult authResult = await CloudDataSource.authClient.LoginAsync(new List<string>() { "wl.signin", "wl.basic", "wl.skydrive", "wl.skydrive_update" });
                    
                    HasAccess = true;

                    if (authResult.Status == LiveConnectSessionStatus.Connected)
                    {
                        // Save the current session object
                        App.Session = authResult.Session;
                        LoadProfile();
                    }
                }
                catch (LiveAuthException)
                {
                    //Could not authenticate
                    //Redirect user to account settings
                    AccountSettings settings = new AccountSettings();
                    settings.ShowIndependent();

                }
            }
        }

        /// <summary>
        /// Gets the username of the current user and tries to load their data asynchroniously
        /// </summary>
        private async void LoadProfile()
        {
            LiveConnectClient client = new LiveConnectClient(App.Session);
            LiveOperationResult operationResult = await client.GetAsync("me");
            dynamic result = operationResult.Result;
            App.UserName = result.name;
            if (HasAccess)
                await LoadDataAsync();
        }


        #region Constructors
        public CloudDataSource() { }

        /// <summary>
        /// Creates a OneDrive data source with a folder in the user's My Documents directory
        /// </summary>
        /// <param name="name">The name of the folder to create</param>
        public CloudDataSource(string name)
        {
            this.CreateSkyDriveFolderAsync(name);
        }

        #endregion

        public bool Initialized { get; private set; }
        public bool HasAccess { get; private set; }

        /// <summary>
        /// Checks if a data folder exists and creates one if it doesn't
        /// </summary>
        /// <param name="name">The name of the desired directory</param>
        /// <returns>Path to the folder with the given name</returns>
        public async Task<string> CreateSkyDriveFolderAsync(string name)
        {
            // Check if there is a data folder ID in the roaming settings storage
            if (!Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("folderId"))
            {
                // Try to create a new folder
                try
                {
                    var folderData = new Dictionary<string, object>();
                    // Set the desired name
                    folderData.Add("name", name);
                    LiveConnectClient liveClient = new LiveConnectClient(App.Session);
                    // Save the data folder in the user's MyDouments OneDrive directory
                    LiveOperationResult operationResult =
                        await liveClient.PostAsync("me/skydrive/my_documents", folderData);

                    // Contains information about the newly created folder
                    dynamic result = operationResult.Result;

                    // Save data folder ID to roaming settigns storage
                    Windows.Storage.ApplicationData.Current.RoamingSettings.Values["folderId"] = result.id;
                }
                catch (LiveConnectException ex)
                {
                    // Return an empty path if something went wrong
                    return String.Empty;
                }
            }

            // Return the data folder ID from roaming settings storage
            return Windows.Storage.ApplicationData.Current.RoamingSettings.Values["folderId"].ToString();
        }

        /// <summary>
        /// Loads data from the data file contained in the data directory on the user's OneDrive
        /// and sets the groupsMap property of this data source
        /// </summary>
        /// <returns></returns>
        public override async Task LoadDataAsync()
        {
            LiveConnectClient client = new LiveConnectClient(App.Session);

            // Checks if there is a data file ID in the roaming settings storage
            // It is assumed that if there is an ID the file exists but the user might have deleted it manually
            // or the file might not exist for other reasons
            if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("dataFileId"))
            {
                // Get the path to the data file
                string path = Windows.Storage.ApplicationData.Current.RoamingSettings.Values["dataFileId"].ToString();

                // Create a file in the local folder
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile storageFile = await localFolder.CreateFileAsync("definitions.json", CreationCollisionOption.ReplaceExisting);

                // Try to download the content of the file and save it in the storage file
                try
                {
                    // Get file content
                    await client.BackgroundDownloadAsync(path + "/content", storageFile);
                }
                catch (Exception ex)
                {
                    // The file might have been deleted
                    // TODO: Handle this scenario more gracefully
                    storageFile = null;
                }

                // Makes sure there was no exception
                if (storageFile != null)
                {
                    //IStorageFile storageFile = downloadOperationResult.File;

                    // Reads from the storage file
                    var result = await FileIO.ReadTextAsync(storageFile);

                    // Deserializes the JSON formatted string

                    // This one is obsolete
                    //this.groupsMap = await JsonConvert.DeserializeObjectAsync<Dictionary<string, DefinitionsDataGroup>>(result);

                    // This method is now recommended for deserialization
                    this.groupsMap = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Dictionary<string, DefinitionsDataGroup>>(result));
                    if (this.groupsMap == null)
                        this.groupsMap = new Dictionary<string, DefinitionsDataGroup>();

                    Initialized = true;
                }
                else
                {
                    // There probably was an exception
                    // Assume the data file has been deleted so there is no data to load
                    this.groupsMap = new Dictionary<string, DefinitionsDataGroup>();
                    // Try to create a new data file
                    await this.SaveDataAsync();
                }
            }
            else
            {
                // There is no data file ID in the roaming settings storage so there is no data to load
                this.groupsMap = new Dictionary<string, DefinitionsDataGroup>();
                // Try to create a new data file
                await this.SaveDataAsync();
            }
        }

        /// <summary>
        /// Shorthand method for saving definitions data on the user's OneDrive directory
        /// </summary>
        /// <returns></returns>
        public override async Task SaveDataAsync()
        {
            // Set default file and folder names
            string path = await CreateSkyDriveFolderAsync(_folderName);

            LiveConnectClient client = new LiveConnectClient(App.Session);

            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
            {
                // Serialize data from this data source
                string data = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(this.groupsMap));

                //This one is obsolete
                //string data = await JsonConvert.SerializeObjectAsync(this.groupsMap);


                // Write this data to a stream
                await writer.WriteAsync(data);
                await writer.FlushAsync();
                // Upload a file from this stream with an overwrite option
                LiveOperationResult operationResult = await client.BackgroundUploadAsync(path, _fileName, stream.AsInputStream(), OverwriteOption.Overwrite);
                // Contains information about the uploaded file
                dynamic result = operationResult.Result;
                // Add the file ID to roaming settings storage
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["dataFileId"] = result.id;
            }
        }


        /// <summary>
        /// Saves data to a file with a given name on the user's OneDrive directory
        /// </summary>
        /// <param name="fileName">The destination file name</param>
        /// <param name="data">Data to be serialized and saved to the file</param>
        /// <returns></returns>
        public override async Task SaveDataAsync(string fileName, object data)
        {
            // Get local folder
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            // Create storage file with the desired name
            StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            // Serialize data in JSON format
            string jsonData = await JsonConvert.SerializeObjectAsync(data, Formatting.None);
            // Write serialized string to the storage file
            await FileIO.WriteTextAsync(file, jsonData);

            // Make sure a directory exists and get its path
            string path = await CreateSkyDriveFolderAsync(_folderName);


            LiveConnectClient client = new LiveConnectClient(App.Session);
            if (path != String.Empty)
            {
                bool shouldRetry = false;
                try
                {
                    LiveOperationResult operationResult = await client.BackgroundUploadAsync(path, file.Name, file, OverwriteOption.Overwrite);

                    dynamic result = operationResult.Result;
                    Windows.Storage.ApplicationData.Current.RoamingSettings.Values["dataFileId"] = result.id;
                }
                catch (LiveAuthException)
                {
                    // maybe the existing key is old or the folder was deleted by the user
                    Windows.Storage.ApplicationData.Current.RoamingSettings.Values.Remove("folderId");
                    path = CreateSkyDriveFolderAsync(_folderName).ToString();
                    if (path != String.Empty)
                        shouldRetry = true;
                }
                finally
                {
                    if (shouldRetry)
                        SaveDataAsync(fileName, this.groupsMap).Wait();
                }
            }
        }

    }
}
