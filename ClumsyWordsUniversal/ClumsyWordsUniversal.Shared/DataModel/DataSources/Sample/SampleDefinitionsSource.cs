using ClumsyWordsUniversal.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Data.Json;
using Windows.UI.Xaml.Data;
using Newtonsoft.Json;

namespace ClumsyWordsUniversal.Data
{
    /// <summary>
    /// Universal sample data source
    /// </summary>
    public sealed class SampleDefinitionsSource
    {
        private static SampleDefinitionsSource _sampleDataSource = new SampleDefinitionsSource();

        public Dictionary<string, DefinitionsDataGroup> groupsMap = new Dictionary<string, DefinitionsDataGroup>();

        /// <summary>
        /// Replica for groupsMap in the real data source
        /// </summary>
        private Dictionary<string, DefinitionsDataGroup> _groups = new Dictionary<string, DefinitionsDataGroup>();
        public Dictionary<string, DefinitionsDataGroup> Groups
        {
            get { return this._groups; }
            set { this._groups = value; }
        }

        /// <summary>
        /// Used to generate a XAML file with sample data
        /// </summary>
        public ObservableCollection<DefinitionsDataGroup> GroupsCollection { get; set; }

        public SampleDefinitionsSource()
        {
            GetSampleDataAsync();
        }

        public static async Task<IEnumerable<DefinitionsDataGroup>> GetGroupsAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Groups.Values;
        }

        public static async Task<DefinitionsDataGroup> GetGroupAsync(string key)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.Values.Where((group) => group.Key.Equals(key));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<DefinitionsDataItem> GetItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.Values.SelectMany(group => group.Items).Where((item) => item.Id.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        /// <summary>
        /// Used for grouping
        /// </summary>
        /// <param name="itemList">Collection of items to be grouped</param>
        /// <param name="getKeyFunc">The filtering condition</param>
        /// <returns>A collection of groups</returns>
        private static ObservableCollection<CommonGroup<TermProperties>> GetGroupsList(List<TermProperties> itemList, Func<TermProperties, PartOfSpeech> getKeyFunc)
        {
            IEnumerable<CommonGroup<TermProperties>> groupsList = from item in itemList
                                                                  group item by getKeyFunc(item) into g
                                                                  orderby g.Key
                                                                  select new CommonGroup<TermProperties>(g.Key.ToString(), g.Key.ToString(), g.AsEnumerable<TermProperties>(), true);

            return new ObservableCollection<CommonGroup<TermProperties>>(groupsList);
        }

        /// <summary>
        /// Load async JSON data like in real load method
        /// </summary>
        /// <returns></returns>
        private async Task GetSampleDataAsync()
        {
            if (this._groups.Count != 0)
                return;

            Uri dataUri = new Uri("ms-appx:///DataModel/SampleDefinitions.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);

            this.Groups = await JsonConvert.DeserializeObjectAsync<Dictionary<string, DefinitionsDataGroup>>(jsonText);
            this.GroupsCollection = new ObservableCollection<DefinitionsDataGroup>(GetGroupsAsync().Result);

            //JsonObject jsonObject = JsonObject.Parse(jsonText);
            //JsonArray jsonArray = jsonObject["Groups"].GetArray();

            //foreach (JsonValue groupValue in jsonArray)
            //{
            //    JsonObject groupObject = groupValue.GetObject();
            //    DefinitionsDataGroup group = new DefinitionsDataGroup(groupObject["Key"].GetString(),
            //                                                groupObject["Title"].GetString());

            //    foreach (JsonValue itemValue in groupObject["Items"].GetArray())
            //    {
            //        JsonObject itemObject = itemValue.GetObject();
            //        ObservableCollection<CommonGroup<TermProperties>> groupedDefinitions = new ObservableCollection<CommonGroup<TermProperties>>();
            //        foreach(JsonValue groupByPartOfSpeechValue in itemObject["Items"].GetArray())
            //        {
            //            JsonObject commonGroupObject = groupByPartOfSpeechValue.GetObject();

            //            CommonGroup<TermProperties> definitionsList = new CommonGroup<TermProperties>();

            //            definitionsList.Key = commonGroupObject["Key"].GetString();
            //            definitionsList.Title = commonGroupObject["Title"].GetString();

            //            foreach (var propertyValue in commonGroupObject["Items"].GetArray())
            //            {
            //                JsonObject propertyObject = propertyValue.GetObject();

            //                definitionsList.Items.Add(new TermProperties(propertyObject["Term"].GetString(),
            //                propertyObject["Definition"].GetString(),
            //                (PartOfSpeech)propertyObject["PartOfSpeech"].GetNumber(),
            //                propertyObject["Example"].GetString()));
            //            }
            //            groupedDefinitions.Add(definitionsList);
                        
            //        }
            //        group.Items.Add(new DefinitionsDataItem(itemObject["Term"].GetString(),
            //            groupedDefinitions,
            //            Guid.NewGuid()));
            //    }
            //    this.Groups.Add(group.Key,group);
            //}
        }
    }
}
