﻿using ClumsyWordsUniversal.Common;
using ClumsyWordsUniversal.Common.Converters;
using ClumsyWordsUniversal.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace ClumsyWordsUniversal.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ItemSectionPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public ItemSectionPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        #region Share Functionality Methods

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var request = args.Request;
            var items = (CommonGroup<TermProperties>)this.DefaultViewModel["Group"];

            DefinitionsToHTMLConverter htmlConverter = new DefinitionsToHTMLConverter();

            request.Data.Properties.Title = String.Empty;

            string definitionsString = "<style>body{word-wrap:break-word;} h2{text-align:center;}</style>";
            string title = items.Items.FirstOrDefault().Term;

            request.Data.Properties.Title = title;

            if (this.itemGridView.SelectedItems.Count == 0)
            {
                definitionsString += htmlConverter.Convert(new DefinitionsDataItem(title, new ObservableCollection<CommonGroup<TermProperties>>()),
                                                           typeof(string),
                                                           new ObservableCollection<CommonGroup<TermProperties>>() { items },
                                                           String.Empty);
            }

            else
            {
                CommonGroup<TermProperties> selectedItems = new CommonGroup<TermProperties>();
                selectedItems.Title = items.Title;
                TermProperties currentItem;

                foreach (var selectedItem in this.itemGridView.SelectedItems)
                {
                    currentItem = (TermProperties)selectedItem;

                    if (!selectedItems.Items.Contains(currentItem))
                        selectedItems.Items.Add(currentItem);
                }

                definitionsString += htmlConverter.Convert(new DefinitionsDataItem(title, new ObservableCollection<CommonGroup<TermProperties>>()),
                                                           typeof(string),
                                                           new ObservableCollection<CommonGroup<TermProperties>>() { selectedItems },
                                                           String.Empty);
            }

            definitionsString = HtmlFormatHelper.CreateHtmlFormat(definitionsString);
            request.Data.SetHtmlFormat(definitionsString);
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
            string currentTerm = this.DefaultViewModel["CurrentTerm"].ToString();
            List<TermProperties> selectedItems = new List<TermProperties>();

            foreach (var item in this.itemGridView.SelectedItems)
            {
                selectedItems.Add((TermProperties)item);
            }

            newItem.ComposeItemFromTermProperties(currentTerm, selectedItems);
            App.DataSource.GetGroup("Favourites").Items.Add(newItem);
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