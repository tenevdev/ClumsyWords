using ClumsyWordsUniversal.Data;
using ClumsyWordsUniversal.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace ClumsyWordsUniversal
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ItemPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ItemPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
        }

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
            var item = await SampleDataSource.GetItemAsync((string)e.NavigationParameter);
            this.DefaultViewModel["Item"] = item;
        }

        void Header_Click(object sender, RoutedEventArgs e)
        {
            // Determine what group the Button instance represents
            var group = (sender as FrameworkElement).DataContext;

            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter

            Dictionary<string, object> navParameters = new Dictionary<string, object>();
            navParameters["ItemName"] = ((DefinitionsDataItem)DefaultViewModel["CurrentItem"]).Term;
            navParameters["Group"] = (CommonGroup<TermProperties>)group;


            this.Frame.Navigate(typeof(ItemPage), navParameters);
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

        #region Event Handlers

        private void OnGridViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region Maintain Selection Across View States
            foreach (var item in e.RemovedItems)
            {
                this.itemListView.SelectedItems.Remove(item);
            }

            foreach (var item in e.AddedItems)
            {
                this.itemListView.SelectedItems.Add(item);
            }
            #endregion

            if (this.itemGridView.SelectedItems.Count == 0)
            {
                //this.BottomAppBar.IsSticky = false;
                this.BottomAppBar.IsOpen = false;
            }
            else
            {
                this.BottomAppBar.IsOpen = true;
                //this.BottomAppBar.IsSticky = true;
            }
        }

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region Maintain Selection Across View States
            foreach (var item in e.RemovedItems)
            {
                this.itemGridView.SelectedItems.Remove(item);
            }

            foreach (var item in e.AddedItems)
            {
                this.itemGridView.SelectedItems.Add(item);
            }
            #endregion

            if (this.itemGridView.SelectedItems.Count == 0)
            {
                this.BottomAppBar.IsOpen = false;
            }
            else
            {
                this.BottomAppBar.IsOpen = true;
            }
        }

        private void OnAddToFavouritesClick(object sender, RoutedEventArgs e)
        {
            DefinitionsDataItem newItem = new DefinitionsDataItem();
            string currentTerm = ((DefinitionsDataItem)this.DefaultViewModel["CurrentItem"]).Term;
            List<TermProperties> selectedItems = new List<TermProperties>();
            TermProperties currentItem;

            foreach (var item in this.itemGridView.SelectedItems)
            {
                currentItem = (TermProperties)item;

                if (!selectedItems.Contains(currentItem))
                    selectedItems.Add(currentItem);
            }

            newItem.ComposeItemFromTermProperties(currentTerm, selectedItems);
            App.DataSource.GetGroup("Favourites").Items.Add(newItem);
            this.Frame.Navigate(typeof(HubPage), new string[] { "Recent", "Favourites" });
        }

        private void OnClearSelectionClick(object sender, RoutedEventArgs e)
        {
            if (this.itemGridView.SelectedItems.Count != 0)
                this.itemGridView.SelectedItems.Clear();
        }

        private void OnGoHomeClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HubPage), new string[] { "Recent", "Favourites" });
        }

        private void OnSemanticZoomViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.IsSourceZoomedInView == true)
                this.BottomAppBar.Visibility = Visibility.Collapsed;
            else
                this.BottomAppBar.Visibility = Visibility.Visible;
        }

        private async void SignInClick(object sender, RoutedEventArgs e)
        {
            await App.UpdateUserName(true);

            //this.userName.Text = App.UserName;

            if (App.UserName == "You are not signed in.")
                this.SignInBtn.Visibility = Visibility.Visible;
            else
                this.SignInBtn.Visibility = Visibility.Collapsed;
        }

        private void OnQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            // If the Window isn't already using Frame navigation, insert our own Frame
            var previousContent = Window.Current.Content;
            var frame = previousContent as Frame;

            // Display search results
            frame.Navigate(typeof(SearchResultsPage), args.QueryText);
            Window.Current.Content = frame;

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void OnSuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
        {
            // Extract the query
            string query = args.QueryText.ToLower();

            // Get all available terms from Recent and Favourites groups
            string[] categories = new string[] { "Recent", "Favourites" };
            List<string> terms = App.DataSource.GetTerms(categories);

            // Display a term as a suggestion if it starts with the current query
            foreach (var term in terms)
            {
                if (term.StartsWith(query))
                    args.Request.SearchSuggestionCollection.AppendQuerySuggestion(term);
            }
        }

        #endregion
    }
}