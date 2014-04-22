using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ClumsyWordsUniversal.Common;

namespace ClumsyWordsUniversal.Data
{
    /// <summary>
    /// The base for Groups that will act as a view model in the GroupedItemsPage
    /// Implements TopItems support for Windows 8 Apps
    /// </summary>
    /// <typeparam name="T">Defines the type of the items list</typeparam>
    public class CommonGroup<T> : BindableBase
    {
        
        public CommonGroup()
            :this(String.Empty, String.Empty, new ObservableCollection<T>(), true)
        { }

        public CommonGroup(string key, string title)
            : this(key, title, Enumerable.Empty<T>(), true)
        { }

        [JsonConstructor]
        public CommonGroup(string key, string title, IEnumerable<T> items, bool isTopItemsSupported)
        {
            this._key = key;
            this._title = title;
            this._items = new ObservableCollection<T>(items);
            this._items.CollectionChanged += Items_CollectionChanged;
            this._topItems = new ObservableCollection<T>(items.Take(12));
        }

        private string _key;
        public string Key
        {
            get { return this._key; }
            set { this.SetProperty(ref this._key, value); }
        }

        private string _title;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private ObservableCollection<T> _items;
        public ObservableCollection<T> Items 
        {
            get { return this._items; }
            private set { SetProperty(ref this._items, value); }
        }

        void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        [JsonIgnore]
        private ObservableCollection<T> _topItems = new ObservableCollection<T>();
        [JsonIgnore]
        public ObservableCollection<T> TopItems
        {
            get { return this._topItems; }
            private set { SetProperty(ref this._topItems, value); }
        }
    }
}
