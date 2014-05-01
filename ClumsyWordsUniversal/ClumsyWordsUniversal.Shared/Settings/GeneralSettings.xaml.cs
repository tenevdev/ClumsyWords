using ClumsyWordsUniversal.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace ClumsyWordsUniversal.Settings
{
    // Contains general settings with default values set

    public sealed partial class GeneralSettings : SettingsFlyout
    {
        public GeneralSettings()
        {
            this.InitializeComponent();

            // Set toggles
            if (roamingSettings.Values.ContainsKey("saveLocal"))
                this.localSwitch.IsOn = (bool)roamingSettings.Values["saveLocal"];
            else
                this.localSwitch.IsOn = true;

            if (roamingSettings.Values.ContainsKey("saveSkyDrive"))
            {
                this.skyDriveSwitch.IsOn = (bool)roamingSettings.Values["saveSkyDrive"];

                if (skyDriveSwitch.IsOn &&
                App.SecondaryDataSource == null &&
                (!roamingSettings.Values.ContainsKey("saveSkyDrive") || roamingSettings.Values.Contains(new KeyValuePair<string, object>("saveSkyDrive", false))))
                {

                    // There is no secondary data source so create it
                    App.SecondaryDataSource = new CloudDataSource("ClumsyWords");
                    //App.SecondaryDataSource.groupsMap = App.DataSource.groupsMap;

                    // Create a folder in the sky drive directory
                }
            }
            else
                this.skyDriveSwitch.IsOn = false;

            if (roamingSettings.Values.ContainsKey("displayTips"))
                this.tipsSwitch.IsOn = (bool)roamingSettings.Values["displayTips"];
            else
                this.tipsSwitch.IsOn = true;

            if (roamingSettings.Values.ContainsKey("displayTooltips"))
                this.tooltipsSwitch.IsOn = (bool)roamingSettings.Values["displayTooltips"];
            else
                this.tooltipsSwitch.IsOn = true;
        }

        Windows.Storage.ApplicationDataContainer roamingSettings =
                Windows.Storage.ApplicationData.Current.RoamingSettings;

        private void SaveLocalSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if ((!this.localSwitch.IsOn) && (!this.skyDriveSwitch.IsOn))
            {
                skyDriveSwitch.IsOn = true;
                roamingSettings.Values["saveSkyDrive"] = skyDriveSwitch.IsOn;
            }
            roamingSettings.Values["saveLocal"] = localSwitch.IsOn;

        }

        private void SaveSkyDriveSwitch_Toggled(object sender, RoutedEventArgs e)
        {

            //Testing
            //roamingSettings.Values.Remove("saveSkyDrive");

            // Check if there is a secondary data source
            if (skyDriveSwitch.IsOn && 
                App.SecondaryDataSource == null && 
                (!roamingSettings.Values.ContainsKey("saveSkyDrive") || roamingSettings.Values.Contains(new KeyValuePair<string, object>("saveSkyDrive", false))))
            {

                // There is no secondary data source so create it
                App.SecondaryDataSource = new CloudDataSource("ClumsyWords");
                //App.SecondaryDataSource.groupsMap = App.DataSource.groupsMap;

                // Create a folder in the sky drive directory
            }

            roamingSettings.Values["saveSkyDrive"] = skyDriveSwitch.IsOn;
        }

        private void DisplayTipsSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            roamingSettings.Values["displayTipsStarted"] = tipsSwitch.IsOn;
        }

        private void TooltipsSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            roamingSettings.Values["displayTooltips"] = tooltipsSwitch.IsOn;
        }
    }
}
