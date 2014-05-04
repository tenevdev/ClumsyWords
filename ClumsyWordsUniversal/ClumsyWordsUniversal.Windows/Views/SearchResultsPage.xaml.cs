﻿using ClumsyWordsUniversal.Common;
using ClumsyWordsUniversal.Common.Converters;
using ClumsyWordsUniversal.Data;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace ClumsyWordsUniversal
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class SearchResultsPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private Dictionary<string, List<DefinitionsDataItem>> _results = new Dictionary<string, List<DefinitionsDataItem>>();

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


        public SearchResultsPage()
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
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested += OnDataRequested;

            this.progressPanel.Visibility = Visibility.Visible;
            this.progressRing.IsActive = true;

            var queryText = e.NavigationParameter as String;
            this.DefaultViewModel["QueryText"] = '\u201c' + queryText + '\u201d';

            // TODO: Application-specific searching logic.  The search process is responsible for
            //       creating a list of user-selectable result categories:
            //
            //       filterList.Add(new Filter("<filter name>", <result count>));
            //
            //       Only the first filter, typically "All", should pass true as a third argument in
            //       order to start in an active state.  Results for the active filter are provided
            //       in Filter_SelectionChanged below.

            var filterList = new List<Filter>();
            filterList.Add(new Filter("All", 0, true));

            string query = queryText.ToLower();
            if (!App.DataSource.ContainsTerm(new[] { "Recent", "Favourites" }, query))
            {
                Credentials accessCredentials = new Credentials();

                // This file contains sensititve data and is ignored in our public repository
                Uri dataUri = new Uri("ms-appx:///API/STANDS4.keys.json");

                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
                string jsonText = await FileIO.ReadTextAsync(file);

                accessCredentials = await JsonConvert.DeserializeObjectAsync<Credentials>(jsonText);

                string url = "http://www.stands4.com/services/v2/defs.php?uid=" + accessCredentials.DeveloperId + "&tokenid=" + accessCredentials.Token + "&word=";
                Guid itemId = await App.DataSource.SearchAsync(url, query);

                if (itemId != new Guid())
                {
                    var searchItem = App.DataSource.GetItem(itemId);
                    var searchList = new List<DefinitionsDataItem>() { searchItem };
                    filterList.Add(new Filter("New", 1, true));

                    _results.Add("New", searchList);
                }
                else
                {
                    filterList.Add(new Filter("New", 0, false));

                    _results.Add("New", new List<DefinitionsDataItem>());
                }
            }

            this.progressPanel.Visibility = Visibility.Collapsed;
            this.progressRing.IsActive = false;

            var groups = App.DataSource.GetGroups(new string[] { "Recent", "Favourites" });
            var all = new List<DefinitionsDataItem>();
            _results.Add("All", all);
            bool isAdded = false;

            foreach (var group in groups)
            {
                var items = new List<DefinitionsDataItem>();
                _results.Add(group.Title, items);

                foreach (var itemGroup in group.Items)
                {
                    isAdded = false;

                    foreach (var definitionGroup in itemGroup.Items)
                    {
                        foreach (var definition in definitionGroup.Items)
                        {
                            if (definition.Term.ToLower().StartsWith(query))
                            {
                                all.Add(itemGroup);
                                items.Add(itemGroup);
                                isAdded = true;
                                break;
                            }
                        }
                        if (isAdded == true) break;
                    }
                }
                filterList.Add(new Filter(group.Title, items.Count, false));
            }

            filterList[0].Count = all.Count;

            // Communicate results through the view model
            this.DefaultViewModel["Filters"] = filterList;
            this.DefaultViewModel["ShowFilters"] = filterList.Count > 1;
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

        #region Event Handlers

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var request = args.Request;
            var items = (List<DefinitionsDataItem>)this.DefaultViewModel["Results"];

            DefinitionsToHTMLConverter htmlConverter = new DefinitionsToHTMLConverter();

            request.Data.Properties.Title = String.Empty;

            string definitionsString = "<style>body{word-wrap:break-word;} h2{text-align:center;}</style>";

            if (this.resultsGridView.SelectedItems.Count == 0)
            {
                foreach (var item in items)
                {
                    request.Data.Properties.Title += item.Term + ", ";
                    definitionsString += htmlConverter.Convert(item, typeof(string), null, String.Empty);
                }
            }

            else
            {
                List<DefinitionsDataItem> selectedItems = new List<DefinitionsDataItem>();
                DefinitionsDataItem currentItem;

                foreach (var selectedItem in this.resultsGridView.SelectedItems)
                {
                    currentItem = (DefinitionsDataItem)selectedItem;

                    if (!selectedItems.Contains(currentItem))
                        selectedItems.Add(currentItem);
                }

                foreach (var item in selectedItems)
                {
                    request.Data.Properties.Title += item.Term + ", ";
                    definitionsString += htmlConverter.Convert(item, typeof(string), null, String.Empty);
                }
            }

            definitionsString = HtmlFormatHelper.CreateHtmlFormat(definitionsString);
            request.Data.SetHtmlFormat(definitionsString);
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(ItemPage), ((DefinitionsDataItem)e.ClickedItem).Id);
        }

        /// <summary>
        /// Invoked when a filter is selected using the ComboBox in snapped view state.
        /// </summary>
        /// <param name="sender">The ComboBox instance.</param>
        /// <param name="e">Event data describing how the selected filter was changed.</param>
        void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Determine what filter was selected
            var selectedFilter = e.AddedItems.FirstOrDefault() as Filter;
            if (selectedFilter != null)
            {
                // Mirror the results into the corresponding Filter object to allow the
                // RadioButton representation used when not snapped to reflect the change
                selectedFilter.Active = true;

                // TODO: Respond to the change in active filter by setting this.DefaultViewModel["Results"]
                //       to a collection of items with bindable Image, Title, Subtitle, and Description properties

                this.DefaultViewModel["Results"] = _results[selectedFilter.Name];

                // Ensure results are found
                object results;
                ICollection resultsCollection;
                if (this.DefaultViewModel.TryGetValue("Results", out results) &&
                    (resultsCollection = results as ICollection) != null &&
                    resultsCollection.Count != 0)
                {
                    VisualStateManager.GoToState(this, "ResultsFound", true);
                    return;
                }
            }

            // Display informational text when there are no search results.
            VisualStateManager.GoToState(this, "NoResultsFound", true);
        }

        private void OnGridViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region Maintain Selection Across View States
            foreach (var item in e.RemovedItems)
            {
                this.resultsListView.SelectedItems.Remove(item);
            }

            foreach (var item in e.AddedItems)
            {
                this.resultsListView.SelectedItems.Add(item);
            }
            #endregion

            if (this.resultsGridView.SelectedItems.Count == 0)
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
                this.resultsGridView.SelectedItems.Remove(item);
            }

            foreach (var item in e.AddedItems)
            {
                this.resultsGridView.SelectedItems.Add(item);
            }
            #endregion

            if (this.resultsGridView.SelectedItems.Count == 0)
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
            List<object> selectedItems = this.resultsGridView.SelectedItems.ToList();
            foreach (var item in selectedItems)
            {
                if (!App.DataSource.GetGroup("Favourites").ContainsSimilar((DefinitionsDataItem)item))
                {
                    App.DataSource.GetGroup("Favourites").Items.Add(new DefinitionsDataItem((DefinitionsDataItem)item));
                }
            }
            this.resultsGridView.SelectedItems.Clear();
        }

        private void OnClearSelectionClick(object sender, RoutedEventArgs e)
        {
            if (this.resultsGridView.SelectedItems.Count != 0)
                this.resultsGridView.SelectedItems.Clear();
        }

        private void OnGoHomeClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HubPage), new string[] { "Recent", "Favourites" });
        }

        /// <summary>
        /// Invoked when a filter is selected using a RadioButton when not snapped.
        /// </summary>
        /// <param name="sender">The selected RadioButton instance.</param>
        /// <param name="e">Event data describing how the RadioButton was selected.</param>
        void Filter_Checked(object sender, RoutedEventArgs e)
        {
            // Mirror the change into the CollectionViewSource used by the corresponding ComboBox
            // to ensure that the change is reflected when snapped
            if (filtersViewSource.View != null)
            {
                var filter = (sender as FrameworkElement).DataContext;
                filtersViewSource.View.MoveCurrentTo(filter);
            }
        }

        #endregion

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

        /// <summary>
        /// View model describing one of the filters available for viewing search results.
        /// </summary>
        private sealed class Filter : ClumsyWordsUniversal.Common.BindableBase
        {
            private String _name;
            private int _count;
            private bool _active;

            public Filter(String name, int count, bool active = false)
            {
                this.Name = name;
                this.Count = count;
                this.Active = active;
            }

            public override String ToString()
            {
                return Description;
            }

            public String Name
            {
                get { return _name; }
                set { if (this.SetProperty(ref _name, value)) this.OnPropertyChanged("Description"); }
            }

            public int Count
            {
                get { return _count; }
                set { if (this.SetProperty(ref _count, value)) this.OnPropertyChanged("Description"); }
            }

            public bool Active
            {
                get { return _active; }
                set { this.SetProperty(ref _active, value); }
            }

            public String Description
            {
                get { return String.Format("{0} ({1})", _name, _count); }
            }
        }

        private sealed class Credentials
        {
            public Credentials() { }

            [JsonConstructor]
            public Credentials(string token, string devId) 
            {
                this.Token = token;
                this.DeveloperId = devId;
            }

            public string Token { get; set; }
            public string DeveloperId { get; set; }
        }
    }
}