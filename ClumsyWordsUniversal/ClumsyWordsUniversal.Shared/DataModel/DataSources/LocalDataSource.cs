using Microsoft.Live;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace ClumsyWordsUniversal.Data
{
    public class LocalDataSource : DataSource
    {
        public LocalDataSource() { }

        private static readonly StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        private readonly string _fileName = "definitions.json";

        /// <summary>
        /// Initializes the data source trying to load data
        /// </summary>
        /// <returns></returns>
        public override async Task InitAsync()
        {
            await this.LoadDataAsync();
        }

        /// <summary>
        /// Loads data from local storage and populates the data source
        /// </summary>
        /// <returns></returns>
        public override async Task LoadDataAsync()
        {
            bool exists = true;
            StorageFile file = null;
            try
            {
                file = await LocalDataSource.localFolder.GetFileAsync(this._fileName);
            }
            catch (FileNotFoundException)
            {
                exists = false;
            }
            if (!exists)
            {
                // Create the file
                file = await LocalDataSource.localFolder.CreateFileAsync(this._fileName);

                // Get sample data designed for inital loading of the app
                file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///DataModel/DataSources/Sample/SampleDefinitions.json", UriKind.Absolute));
            }
            var result = await FileIO.ReadTextAsync(file);

            this.groupsMap = await JsonConvert.DeserializeObjectAsync<Dictionary<string, DefinitionsDataGroup>>(result);
            if (this.groupsMap == null)
                this.groupsMap = new Dictionary<string, DefinitionsDataGroup>();
        }

        /// <summary>
        /// Saves data to local storage
        /// </summary>
        /// <returns></returns>
        public override async Task SaveDataAsync()
        {
            var file = await LocalDataSource.localFolder.CreateFileAsync(this._fileName, CreationCollisionOption.ReplaceExisting);

            string jsonData = await JsonConvert.SerializeObjectAsync(this.groupsMap, Formatting.None);
            await FileIO.WriteTextAsync(file, jsonData);
        }

        /// <summary>
        /// Saves data to local storage
        /// </summary>
        /// <param name="filename">The name of the file to write in</param>
        /// <param name="data">The data object to be serialized and written in the file</param>
        /// <returns></returns>
        public override async Task SaveDataAsync(string filename, object data)
        {
            var file = await LocalDataSource.localFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            string jsonData = await JsonConvert.SerializeObjectAsync(data, Formatting.None);
            await FileIO.WriteTextAsync(file, jsonData);
        }
    }
}
