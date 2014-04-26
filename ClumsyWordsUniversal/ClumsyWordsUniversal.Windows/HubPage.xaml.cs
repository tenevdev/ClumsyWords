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
using ClumsyWordsUniversal.Data;
using ClumsyWordsUniversal.Common;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace ClumsyWordsUniversal
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// Gets the NavigationHelper used to aid in navigation and process lifetime management.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the DefaultViewModel. This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public HubPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroups = await SampleDefinitionsSource.GetGroupsAsync();
            this.DefaultViewModel["GroupsCollection"] = sampleDataGroups;
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        #region EventHandlers

        /// <summary>
        /// Invoked when a HubSection header is clicked.
        /// </summary>
        /// <param name="sender">The Hub that contains the HubSection whose header was clicked.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        void Hub_SectionHeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            HubSection section = e.Section;
            var group = section.DataContext;
            this.Frame.Navigate(typeof(SectionPage), ((SampleDataGroup)group).UniqueId);
        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        /// <param name="sender">The GridView or ListView
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(ItemPage), itemId);
        }

        #region SemanticZoom

        //private void OnSemanticZoomViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        //{
        //    if (e.IsSourceZoomedInView == true)
        //        this.BottomAppBar.Visibility = Visibility.Collapsed;
        //    else
        //        this.BottomAppBar.Visibility = Visibility.Visible;
        //}

        #endregion
        #region Selection

        //private void OnGridViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    #region Maintain Selection Across View States
        //    foreach (var item in e.RemovedItems)
        //    {
        //        this.itemListView.SelectedItems.Remove(item);
        //    }

        //    foreach (var item in e.AddedItems)
        //    {
        //        this.itemListView.SelectedItems.Add(item);
        //    }
        //    #endregion

        //    if (this.itemGridView.SelectedItems.Count == 0)
        //    {
        //        //this.BottomAppBar.IsSticky = false;
        //        this.BottomAppBar.IsOpen = false;
        //    }
        //    else
        //    {
        //        this.BottomAppBar.IsOpen = true;
        //        //this.BottomAppBar.IsSticky = true;
        //    }
        //}

        //private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    #region Maintain Selection Across View States
        //    foreach (var item in e.RemovedItems)
        //    {
        //        this.itemGridView.SelectedItems.Remove(item);
        //    }

        //    foreach (var item in e.AddedItems)
        //    {
        //        this.itemGridView.SelectedItems.Add(item);
        //    }
        //    #endregion

        //    if (this.itemGridView.SelectedItems.Count == 0)
        //    {
        //        this.BottomAppBar.IsOpen = false;
        //    }
        //    else
        //    {
        //        this.BottomAppBar.IsOpen = true;
        //    }
        //}

        #endregion
        #region Click

        /// <summary>
        /// Invoked when a group header is clicked.
        /// </summary>
        /// <param name="sender">The Button used as a group header for the selected group.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        void Header_Click(object sender, RoutedEventArgs e)
        {
            // Determine what group the Button instance represents
            var group = (sender as FrameworkElement).DataContext;

            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            this.Frame.Navigate(typeof(SectionPage), ((DefinitionsDataGroup)group).Key);
        }
        #endregion

        #endregion
    }
}
