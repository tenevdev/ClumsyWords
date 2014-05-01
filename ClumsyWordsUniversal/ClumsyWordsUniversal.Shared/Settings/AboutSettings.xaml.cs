﻿using System;
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

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace ClumsyWordsUniversal.Settings
{
    // Contains information about:
    //      App Version
    //      Support
    //      Privacy Statement
    //      Copyright Information

    public sealed partial class AboutSettings : SettingsFlyout
    {
        public AboutSettings()
        {
            this.InitializeComponent();
        }
    }
}