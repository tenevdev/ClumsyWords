using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using ClumsyWordsUniversal.Common;
using Windows.Storage;
using System.Threading.Tasks;
using Microsoft.Live;
using ClumsyWordsUniversal.Data;
using ClumsyWordsUniversal.Settings;

#if WINDOWS_APP
using Windows.UI.ApplicationSettings;
#endif

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace ClumsyWordsUniversal
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        /// <summary>
        /// Initializes the singleton instance of the <see cref="App"/> class. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        #region Data

        /// <summary>
        /// Application settings container
        /// </summary>
        Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

        /// <summary>
        /// Static container which stores app content and provides it across the app
        /// </summary>
        public static DataSource DataSource { get; set; }

        /// <summary>
        /// An auxiliary container which is used when a user selects to use both local and cloud storage to store app content for the first time
        /// or the user changes the storage option 
        /// </summary>
        public static DataSource SecondaryDataSource { get; set; }

        private void GetDataSource()
        {
            if (this.roamingSettings.Values.Contains(new KeyValuePair<string, object>("saveSkyDrive", true)))
            {
                App.DataSource = new CloudDataSource();
            }

            else if (this.roamingSettings.Values.Contains(new KeyValuePair<string, object>("saveLocal", true)))
            {
                App.DataSource = new LocalDataSource();
            }
            else
            {
                App.DataSource = new LocalDataSource();
            }

        }

        public class GroupsCollection : List<string>
        {
            public GroupsCollection() : base() { }
            public GroupsCollection(IEnumerable<string> items) : base(items) { }

            public override string ToString()
            {
                string result = string.Empty;

                for (int i = 0; i < this.Count - 1; i++)
                {
                    result += this[i] + ",";
                }

                result += this.Last();
                return result;
            }
        }

        private List<string> GetGroupsListFromRoaming()
        {
            // Configure the parameter to be passed to the initial page
            // It should show what data should be loaded and displayed

            GroupsCollection groups = new GroupsCollection() { "Recent", "Favourites" };

            if (roamingSettings.Values.ContainsKey("Groups"))
                groups = new GroupsCollection(roamingSettings.Values["Groups"].ToString().Split(new char[]{','}));
            else
                roamingSettings.Values["Groups"] = groups.ToString();

            // The groups list is set with this values initially so we don't need to set it here once again
            return groups;
        }

        #endregion

        #region LiveAccount

        private static LiveConnectSession _session;
        /// <summary>
        /// A static property which provides access to the current session across the app
        /// </summary>
        public static LiveConnectSession Session
        {
            get
            {
                return _session;
            }
            set
            {
                _session = value;

            }
        }

        //private static string _permissions = "none";
        public static string Permissions { get; set; }

        private static string _userName = "You're not signed in.";
        /// <summary>
        /// The user's Live account user name
        /// </summary>
        public static string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;

            }
        }

        private static string _accountState = "signedOut";
        public static string AccountState
        {
            get { return _accountState; }
            set { _accountState = value; }
        }

        public static string FirstName { get; set; }
        public static string LastName { get; set; }
        public static string ProfilePictureSource { get; set; }

        private static IEnumerable<string> DetermineLoginScopes()
        {
            List<string> scopes = new List<string>() { "wl.basic" };

            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;

            // Check if user has given consent to save data into their SkyDrive storage.
            if (roamingSettings.Values.ContainsKey("saveSkyDrive"))
            {
                if ((bool)roamingSettings.Values["saveSkyDrive"])
                {
                    scopes.Add("wl.skydrive");
                    scopes.Add("wl.skydrive_upload");
                }
            }

            return scopes;
        }

        /// <summary>
        /// Tries signing in the user with their Microsoft account
        /// </summary>
        /// <param name="signIn">Shows wether the user should try to sign in or just load the current session state.</param>
        /// <returns></returns>
        public static async Task UpdateUserName(Boolean signIn)
        {
            try
            {
                // Open Live Connect SDK client.
                LiveAuthClient LCAuth = new LiveAuthClient();
                LiveLoginResult LCLoginResult = await LCAuth.InitializeAsync();
                try
                {
                    LiveLoginResult loginResult = null;
                    if (signIn)
                    {
                        // Sign in to the user's Microsoft account with the required scope.
                        //  
                        //  This call will display the Microsoft account sign-in screen if 
                        //   the user is not already signed in to their Microsoft account 
                        //   through Windows 8.
                        // 
                        //  This call will also display the consent dialog, if the user has 
                        //   has not already given consent to this app to access the data 
                        //   described by the scope.

                        List<string> scopes = DetermineLoginScopes().ToList();

                        // Sign in the user with the given scopes
                        try 
                        {
                            loginResult = await LCAuth.LoginAsync(scopes);
                        }
                        catch(Exception ex)
                        {
                            string m = ex.Message;
                        }
                    }
                    else
                    {
                        // If we don't want the user to sign in, continue with the current 
                        //  sign-in state.
                        loginResult = LCLoginResult;
                    }
                    if (loginResult.Status == LiveConnectSessionStatus.Connected)
                    {
                        // Create a client session to get the profile data.
                        LiveConnectClient connect = new LiveConnectClient(LCAuth.Session);

                        // Get the profile info of the user.
                        LiveOperationResult operationResult = await connect.GetAsync("me");
                        dynamic result = operationResult.Result;
                        App.UserName = result.name;
                        App.FirstName = result.first_name;
                        App.LastName = result.last_name;
                        App.Session = LCAuth.Session;

                        LiveOperationResult pictureOperationResult = await connect.GetAsync("me/picture");
                        dynamic pictureResult = pictureOperationResult.Result;
                        App.ProfilePictureSource = pictureResult.location;

                        if (result != null)
                        {
                            // Create a personalised hello-message using the user's name.
                            App.UserName = string.Join(" ", "Hello,", result.name, "!");
                        }
                        else
                        {
                            // Handle the case where the user name was not returned.
                            App.UserName = "Couldn't get user name.";
                        }

                        // Get the permissions given by the user to the app.
                        LiveOperationResult permissionsResult = await connect.GetAsync("me/permissions");
                        dynamic res = permissionsResult.RawResult;
                        App.Permissions = res;

                    }
                    else
                    {
                        // The user hasn't signed in so display this text 
                        //  in place of his or her name.
                        App.UserName = "You're not signed in.";
                    }

                }
                catch (LiveAuthException ex)
                {
                    // Handle the exception
                    string errorMessage = ex.Message;
                    App.UserName = "Couldn't sign in. Please, try again later.";
                }
            }
            catch (LiveAuthException ex)
            {
                // Handle the exception. 
                string errorMessage = ex.Message;
                App.UserName = "Couldn't sign in. Please, try again later.";

            }
            catch (LiveConnectException ex)
            {
                // Handle the exception.
                string errorMessage = ex.Message;
                App.UserName = "Couldn't sign in. Please, try again later.";

            }
        }

        #endregion

        #region Settings

#if WINDOWS_APP
        void App_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            // General Settings
            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "general", "General", (handler) => ShowGeneralSettings()));

            // Account Settings
            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "account", "Account", (handler) => ShowAccountSettings()));

            // Privacy Statement Settings
            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "privacy", "Privacy", (handler) => ShowPrivacyStatement()));

            // About Settings
            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "about", "About", (handler) => ShowAboutSettings()));

            // Help Settings
            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "help", "Help", (handler) => ShowHelpSettings()));
        }

        public void ShowGeneralSettings()
        {
            GeneralSettings settings = new GeneralSettings();
            settings.Show();
        }

        public void ShowAccountSettings()
        {
            AccountSettings settings = new AccountSettings();
            settings.Show();
        }

        public void ShowPrivacyStatement()
        {
            PrivacyStatement settings = new PrivacyStatement();
            settings.Show();
        }

        public void ShowAboutSettings()
        {
            AboutSettings settings = new AboutSettings();
            settings.Show();
        }

        public void ShowHelpSettings()
        {
            HelpSettings settings = new HelpSettings();
            settings.Show();
        }
#endif

        #endregion

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
#if WINDOWS_APP
            SettingsPane.GetForCurrentView().CommandsRequested += App_CommandsRequested;
#endif
            base.OnWindowCreated(args);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        // Something went wrong restoring state.
                        // Assume there is no state and continue
                    }
                }

                if (e.PreviousExecutionState == ApplicationExecutionState.Running)
                {
                    Window.Current.Activate();
                    return;
                }

                // Try signing in
                //await UpdateUserName(true);

                roamingSettings.Values.Clear();

                // Load data
                this.GetDataSource();
                await App.DataSource.InitAsync();

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif



                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(HubPage), this.GetGroupsListFromRoaming()))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }
    }
}