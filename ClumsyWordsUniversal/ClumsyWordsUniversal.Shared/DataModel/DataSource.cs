using ClumsyWordsUniversal.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace ClumsyWordsUniversal.Data
{

    /// <summary>
    /// Item data model
    /// </summary>
    public class DefinitionsDataItem
    {
        public DefinitionsDataItem()
            : this(String.Empty, new ObservableCollection<CommonGroup<TermProperties>>(), Guid.NewGuid())
        {
        }

        public DefinitionsDataItem(ObservableCollection<CommonGroup<TermProperties>> items)
            : this(String.Empty, items, Guid.NewGuid())
        {
        }

        public DefinitionsDataItem(string term, ObservableCollection<CommonGroup<TermProperties>> items)
            : this(term, items, Guid.NewGuid())
        {
        }

        [JsonConstructor]
        public DefinitionsDataItem(string term, ObservableCollection<CommonGroup<TermProperties>> items, Guid id)
        {
            this.Id = id;
            this.Items = items;

            if (String.IsNullOrWhiteSpace(term) || term == String.Empty)
                this.Term = term;
            else
                this.Term = this.FirstDefinition.Term;
        }

        public DefinitionsDataItem(DefinitionsDataItem item)
            : this(item.Term, item.Items)
        {
        }

        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public string Term { get; set; }

        [JsonIgnore]
        public TermProperties FirstDefinition
        {
            get
            {
                if (this.Items.Count != 0)
                    return this.Items.FirstOrDefault().Items.FirstOrDefault();
                return new TermProperties();
            }
        }

        [JsonIgnore]
        public DefinitionsDataGroup Group { get; set; }

        [JsonIgnore]
        public int DefinitionsCount
        {
            get
            {
                int count = 0;
                foreach (var item in this.Items)
                {
                    count += item.Items.Count;
                }

                return count;
            }
        }

        [JsonProperty]
        public ObservableCollection<CommonGroup<TermProperties>> Items { get; set; }

        public void ComposeItemFromTermProperties(string term, List<TermProperties> termProps)
        {
            this.Items = DefinitionsDataItem.GetGroupsList(termProps, p => p.PartOfSpeech);
            this.Term = term;
        }

        public static ObservableCollection<CommonGroup<TermProperties>> GetGroupsList(List<TermProperties> itemList, Func<TermProperties, PartOfSpeech> getKeyFunc)
        {
            IEnumerable<CommonGroup<TermProperties>> groupsList = from item in itemList
                                                                  group item by getKeyFunc(item) into g
                                                                  orderby g.Key
                                                                  select new CommonGroup<TermProperties>(g.Key.ToString(), g.Key.ToString(), g.AsEnumerable<TermProperties>(), true);

            return new ObservableCollection<CommonGroup<TermProperties>>(groupsList);
        }
    }

    /// <summary>
    /// Group data model
    /// </summary>
    public class DefinitionsDataGroup : CommonGroup<DefinitionsDataItem>
    {
        private static readonly string _colorCode = "FFFA6800";

        public DefinitionsDataGroup(string key)
            : this(key, key, key, _colorCode) { }

        public DefinitionsDataGroup(string key, string title)
            : this(key, title, title, _colorCode) { }

        public DefinitionsDataGroup(string key, string title, string subtitle)
            : this(key, title, subtitle, _colorCode) { }

        public DefinitionsDataGroup(string key, string title, string subtitle, string colorCode)
            : base(key, title)
        {
            this.ColorCode = colorCode;
            this.Subtitle = subtitle;
        }

        [JsonConstructor]
        public DefinitionsDataGroup(string key, string title, string subtitle, ObservableCollection<DefinitionsDataItem> items, bool isTopItemsSupported, string colorCode)
            : base(key, title, items, isTopItemsSupported)
        {
            this.ColorCode = colorCode;
            this.Subtitle = subtitle;
        }

        public string ColorCode { get; set; }

        /// <summary>
        /// Personalized message describing the group in more than one word
        /// </summary>
        public string Subtitle { get; set; }

        public bool ContainsSimilar(DefinitionsDataItem compare)
        {
            foreach (var item in this.Items)
            {
                if (item.Items == compare.Items && item.Term == compare.Term)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// An abstract base class to use for implementing view models in different project types
    /// Inherits BindableBase which supports INotifyPropertyChanged
    /// Provides a simple data container and standard methods for searching groups and items
    /// Implements simple async method for searching words online using the STANDS4 Definitions API
    /// </summary>
    public abstract class DataSource : BindableBase
    {
        /// <summary>
        /// Implement logic for loading data from different sources
        /// </summary>
        /// <returns></returns>
        public abstract Task LoadDataAsync();

        /// <summary>
        /// Implement logic for saving data on different providers
        /// </summary>
        /// <returns></returns>
        public abstract Task SaveDataAsync();
        public abstract Task SaveDataAsync(string filename, object data);

        /// <summary>
        /// Implement the initialization logic of the data source
        /// </summary>
        /// <returns></returns>
        public abstract Task InitAsync();

        /// <summary>
        /// A key-value pair dictionary which contains data about all terms
        /// </summary>
        public Dictionary<string, DefinitionsDataGroup> groupsMap = new Dictionary<string, DefinitionsDataGroup>();

        /// <summary>
        /// Get all existing groups
        /// </summary>
        /// <returns> All existing groups</returns>
        public virtual IEnumerable<DefinitionsDataGroup> GetGroups()
        {
            return this.groupsMap.Values;
        }


        /// <summary>
        /// Finds the a group for each of the given keys or creates a new one if it doesn't exist
        /// </summary>
        /// <param name="keys">A collection of group keys</param>
        /// <returns>A collection of all existing groups with new groups for the given keys if they do not exist</returns>
        public virtual IEnumerable<DefinitionsDataGroup> GetGroups(IList<string> keys)
        {
            foreach (var k in keys)
            {
                if (!groupsMap.Keys.Contains(k))
                    this.AddGroup(k, k, "#FFFA6800");
            }
            return groupsMap.Values;
        }

        /// <summary>
        /// Checks if the group with the given key exists and if it doesn't creates a new group with the given key as a title.
        /// </summary>
        /// <param name="key">A group key</param>
        /// <returns>A group with the given key</returns>
        public virtual DefinitionsDataGroup GetGroup(string key)
        {
            DefinitionsDataGroup ddg;
            if (!groupsMap.TryGetValue(key, out ddg))
            {
                //return;
                ddg = new DefinitionsDataGroup(key, key, "#FFFA6800");
                groupsMap.Add(key, ddg);
            }

            return ddg;
        }

        /// <summary>
        /// Creates a new group inside the groupsMap
        /// </summary>
        /// <param name="groupName">The name of the new group</param>
        /// <param name="details">Detailed information about the group to set the subtitle</param>
        /// <param name="colorCode">The color to set the theme color for the new group</param>
        public virtual void AddGroup(string groupName, string details, string colorCode)
        {
            this.groupsMap.Add(groupName, new DefinitionsDataGroup(groupName, groupName, details, colorCode));
        }

        /// <summary>
        /// Searches for an item with the given unique identifier in all present groups
        /// </summary>
        /// <param name="id">A unique identifier</param>
        /// <returns>The item with the matching id or null if an item with the given id does not exist</returns>
        public virtual DefinitionsDataItem GetItem(Guid id)
        {

            foreach (var group in groupsMap.Values)
            {
                var match = group.Items.FirstOrDefault(item => item.Id.Equals(id));
                if (match != null) return match;
            }
            // Simple linear search is acceptable for small data sets
            //var matches = groupsMap. .SelectMany(group => group.Items).Where((item) => item.Id.Equals(id));
            //if (matches.Count() == 1) return matches.First();
            return null;
        }

        /// <summary>
        /// Gets all available terms from the groups with the given keys
        /// </summary>
        /// <param name="keys">A collection of group keys</param>
        /// <returns>A collection of terms</returns>
        public virtual List<string> GetTerms(IList<string> keys)
        {
            List<string> terms = new List<string>();
            foreach (var group in this.GetGroups(keys))
            {
                foreach (var item in group.Items)
                {
                    terms.Add(item.Term);
                }
            }

            return terms;
        }

        /// <summary>
        /// Checks if the collection of groups with the given keys contains the given term
        /// </summary>
        /// <param name="keys">A collection of group keys</param>
        /// <param name="term">A string to be searched for</param>
        /// <returns>True if any of the groups contains the term. False if none of the groups contains the term.</returns>
        public virtual bool ContainsTerm(IList<string> keys, string term)
        {
            foreach (var t in this.GetTerms(keys))
            {
                if (t == term) return true;
            }
            return false;
        }


        /// <summary>
        /// Searches for an item using the SearchSevice and adds this item to the Recent group
        /// </summary>
        /// <param name="url">The url of the STANDS4 Definitions API with the necessary query parameters</param>
        /// <returns>The unique identifier of the new item as a string</returns>
        public virtual async Task<Guid> SearchAsync(string url, string term)
        {
            var item = await SearchService.GetSearchResultAsync(url, term);
            if (item != null)
            {
                // Insert the new element on first place
                this.GetGroup("Recent").Items.Insert(0, item);
                return item.Id;
            }

            return new Guid();
        }
    }
}
