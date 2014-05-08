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
using Windows.ApplicationModel.DataTransfer;
using ClumsyWordsUniversal.Common.Converters;
using ClumsyWordsUniversal.Views.ViewStateManagment;
using Windows.UI.Xaml.Media.Imaging;

namespace ClumsyWordsUniversal
{
    /// <summary>
    /// A page that displays an overview of a single group, including a preview of the items
    /// within the group.
    /// </summary>
    public sealed partial class SectionPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public SectionPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            // Display user name and profile picture or a sign in button
            this.DetermineUserPanelState();

            // Manage visual states
            var _viewsManager = new PageViewStateManager(this)
            {
                States = new List<CustomViewStates>
                {
                    new CustomViewStates {State = "Snapped", MatchState = (w,h) => this.SetSnappedState(w,h) },
                    new CustomViewStates { State = "Filled", MatchState = (w,h) => this.SetFilledState(w,h) },
                    new CustomViewStates { State = "FullScreenLandscape", MatchState = (w,h) => this.SetFullScreenState(w,h) }
                }

            };
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
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var group = App.DataSource.GetGroup((String)e.NavigationParameter);
            this.DefaultViewModel["Group"] = group;
            this.DefaultViewModel["Items"] = group.Items;

            DataTransferManager.GetForCurrentView().DataRequested += OnDataRequested;
            Window.Current.SizeChanged += Window_SizeChanged;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested -= OnDataRequested;
            Window.Current.SizeChanged += Window_SizeChanged;
        }

        #region Share Functionality Methods

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var request = args.Request;
            var group = (DefinitionsDataGroup)this.DefaultViewModel["Group"];

            DefinitionsToHTMLConverter htmlConverter = new DefinitionsToHTMLConverter();

            request.Data.Properties.Title = String.Empty;

            string definitionsString = "<style>body{word-wrap:break-word;} h2{text-align:center;}</style>";

            if (this.itemGridView.SelectedItems.Count == 0)
            {
                foreach (var item in group.Items)
                {
                    request.Data.Properties.Title += item.Term + ", ";
                    definitionsString += htmlConverter.Convert(item, typeof(string), null, string.Empty);
                }
            }

            else
            {
                CommonGroup<DefinitionsDataItem> selectedItems = new CommonGroup<DefinitionsDataItem>();
                DefinitionsDataItem currentItem;

                foreach (var selectedItem in this.itemGridView.SelectedItems)
                {
                    currentItem = (DefinitionsDataItem)selectedItem;

                    if (!selectedItems.Items.Contains(currentItem))
                        selectedItems.Items.Add(currentItem);
                }

                foreach (var item in selectedItems.Items)
                {
                    request.Data.Properties.Title += item.Term + ", ";
                    definitionsString += htmlConverter.Convert(item, typeof(string), null, string.Empty);
                }
            }

            definitionsString = HtmlFormatHelper.CreateHtmlFormat(definitionsString);
            request.Data.SetHtmlFormat(definitionsString);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((DefinitionsDataItem)e.ClickedItem).Id;
            this.Frame.Navigate(typeof(ItemPage), itemId);
        }

        private void OnAddToFavouritesClick(object sender, RoutedEventArgs e)
        {
            List<object> selectedItems = this.itemGridView.SelectedItems.ToList();
            foreach (var item in selectedItems)
            {
                if (!App.DataSource.GetGroup("Favourites").ContainsSimilar((DefinitionsDataItem)item))
                {
                    App.DataSource.GetGroup("Favourites").Items.Add(new DefinitionsDataItem((DefinitionsDataItem)item));
                }
            }
            this.itemGridView.SelectedItems.Clear();
        }

        private void OnClearSelectionClick(object sender, RoutedEventArgs e)
        {
            if (this.itemGridView.SelectedItems.Count != 0)
                this.itemGridView.SelectedItems.Clear();
        }

        private void OnDeleteSelectedItemsClick(object sender, RoutedEventArgs e)
        {
            List<object> selectedItems = this.itemGridView.SelectedItems.ToList();
            foreach (var selectedItem in selectedItems)
            {
                foreach (var group in (List<DefinitionsDataGroup>)this.DefaultViewModel["Groups"])
                {
                    for (int i = group.Items.Count - 1; i >= 0; i--)
                    {
                        if (group.Items[i].Id == ((DefinitionsDataItem)selectedItem).Id)
                        {
                            group.Items.Remove(group.Items[i]);
                            break;
                        }
                    }
                }
            }
        }

        private void OnGoHomeClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HubPage), new List<string>() { "Recent", "Favourites" });
        }

        private async void SignInClick(object sender, RoutedEventArgs e)
        {
            await App.UpdateUserName(false);

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

        private void DetermineUserPanelState()
        {
            if (App.UserName == "You're not signed in.")
            {
                this.userInfo.Visibility = Visibility.Collapsed;
                this.SignInBtn.Visibility = Visibility.Visible;
            }
            else if (App.UserName != "Couldn't sign in. Please, try again later." && App.UserName != "")
            {
                this.SignInBtn.Visibility = Visibility.Collapsed;
                this.userInfo.Visibility = Visibility.Visible;

                this.userInfo.FirstName = App.FirstName;
                this.userInfo.LastName = App.LastName;
                this.userInfo.ImageSource = App.ProfileImage;
            }
        }

        #region Visual State Switching Methods

        private bool SetSnappedState(double width, double height)
        {
            if (width < 675)
            {
                //VisualStateManager.GoToState(this, "Snapped", false);

                Grid.SetRow(this.searchBox, 1);
                Grid.SetColumn(this.searchBox, 1);

                Grid.SetRowSpan(this.userPanel, 2);
                Grid.SetColumn(this.userPanel, 2);
                Grid.SetColumnSpan(this.userPanel, 2);

                return true;
            }
            return false;
        }

        private bool SetFilledState(double width, double height)
        {
            if (width < height)
            {
                Grid.SetRow(this.searchBox, 1);
                Grid.SetColumn(this.searchBox, 1);

                Grid.SetRowSpan(this.userPanel, 2);
                Grid.SetColumn(this.userPanel, 2);
                Grid.SetColumnSpan(this.userPanel, 2);

                return true;
            }
            return false;
        }

        private bool SetFullScreenState(double width, double height)
        {
            Grid.SetRow(this.searchBox, 0);
            Grid.SetColumn(this.searchBox, 2);
            Grid.SetColumnSpan(this.searchBox, 1);

            Grid.SetColumn(this.userPanel, 3);
            Grid.SetColumnSpan(this.userPanel, 1);
            Grid.SetRowSpan(this.userPanel, 1);

            return true;
        }

        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            //ApplicationView currentState = ApplicationView.GetForCurrentView();

            // Set the initial visual state of the control
            //VisualStateManager.GoToState(this, "", false);
            if (e.Size.Width <= 675)
            {
                VisualStateManager.GoToState(this, "Snapped", false);

                Grid.SetRow(this.searchBox, 1);
                Grid.SetColumn(this.searchBox, 1);

                Grid.SetRowSpan(this.userPanel, 2);
                Grid.SetColumn(this.userPanel, 2);
                Grid.SetColumnSpan(this.userPanel, 2);
            }
            else if (e.Size.Height > e.Size.Width)
            {
                VisualStateManager.GoToState(this, "Filled", false);

                Grid.SetRow(this.searchBox, 1);
                Grid.SetColumn(this.searchBox, 1);

                Grid.SetRowSpan(this.userPanel, 2);
                Grid.SetColumn(this.userPanel, 2);
                Grid.SetColumnSpan(this.userPanel, 2);
            }
            else
            {
                VisualStateManager.GoToState(this, "FullScreenLandscape", false);

                Grid.SetRow(this.searchBox, 0);
                Grid.SetColumn(this.searchBox, 2);
                Grid.SetColumnSpan(this.searchBox, 1);

                Grid.SetColumn(this.userPanel, 3);
                Grid.SetColumnSpan(this.userPanel, 1);
                Grid.SetRowSpan(this.userPanel, 1);
            }
        }

        #endregion
    }
}