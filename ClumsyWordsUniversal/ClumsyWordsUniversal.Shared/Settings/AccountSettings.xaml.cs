using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Live;
using System.Threading.Tasks;
using ClumsyWordsUniversal;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace ClumsyWordsUniversal.Settings
{
    // Contains functionality for Microsoft account management
    // Login dialog
    // Logout button
    // Forget account
    // Upload data to SkyDrive

    public sealed partial class AccountSettings : SettingsFlyout
    {
        private Boolean userCanSignOut = true;

        public AccountSettings()
        {
            this.InitializeComponent();

            this.DataContext = App.UserName;

            SetNameField(false);
        }

        private async void SignInClick(object sender, RoutedEventArgs e)
        {
            // This call will sign the user in and update the Account flyout UI.
            await this.SetNameField(true);
        }

        private async void SignOutClick(object sender, RoutedEventArgs e)
        {
            if (userCanSignOut)
            {
                try
                {
                    // Initialize access to the Live Connect SDK.
                    LiveAuthClient LCAuth = new LiveAuthClient();
                    LiveLoginResult LCLoginResult = await LCAuth.InitializeAsync();
                    // Sign the user out, if he or she is connected;
                    //  if not connected, skip this and just update the UI
                    if (LCLoginResult.Status == LiveConnectSessionStatus.Connected)
                    {
                        LCAuth.Logout();
                    }

                    // At this point, the user should be disconnected and signed out, so
                    //  update the UI.
                    this.userName.Text = "You're not signed in.";

                    // Show sign-in button.
                    signInBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    signOutBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                catch (LiveConnectException x)
                {
                    this.userName.Text = "An error has occured : " + x.HelpLink;
                }
            }
            else
            {
                cannotSignOutMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }


        private async Task SetNameField(bool login)
        {
            // If login == false, just update the name field.
            this.cannotSignOutMessage.Text = "";
            await App.UpdateUserName(login);

            //this.permissions.Text = App.Permissions;
            try
            {
                LiveAuthClient LCAuth = new LiveAuthClient();
                LiveLoginResult LCLoginResult = await LCAuth.InitializeAsync();

                if (LCLoginResult.Status == LiveConnectSessionStatus.Connected)
                {
                    // Test to see if the user can sign out.
                    userCanSignOut = LCAuth.CanLogout;
                }

                if (App.UserName == "You are not signed in.")
                {
                    // Show sign-in button.
                    signInBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    signOutBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                else
                {
                    // Show sign-out button if they can sign out.
                    //signOutBtn.Visibility = (userCanSignOut ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed);

                    // Disable if user cannot sign out
                    //signOutBtn.IsEnabled = userCanSignOut;

                    signOutBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    signInBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }

            }
            catch(LiveAuthException) 
            {
                this.cannotSignOutMessage.Text = "Couldn't sign in. Please check your Internet connection and try again.";
                this.cannotSignOutMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

        }
    }
}
