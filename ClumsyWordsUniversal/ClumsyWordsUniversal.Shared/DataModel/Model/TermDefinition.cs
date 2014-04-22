using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClumsyWordsUniversal.Data
{
    public class TermDefinition
    {
        public TermDefinition()
            :this(String.Empty, new List<TermProperties>())
        {
        }

        public TermDefinition(List<TermProperties> items) 
            :this(String.Empty, items)
        {
        }

        public TermDefinition(string term, List<TermProperties> items)
        {
            this.Term = term;
            this.TermProperties = items;
        }

        //public TermDefinition(string term, List<Group<TermProperties, PartOfSpeech>> tpList)
        //{
        //    this.Term = term;
        //    this.TermPropertiesList = tpList;
        //}

        /// <summary>
        /// The actual word that is being defined.
        /// </summary>
        public string Term { get; set; }

        public List<TermProperties> TermProperties { get; set; }

        public List<Group<PartOfSpeech, TermProperties>> GroupedTermProperties
        {
            get { return Group<PartOfSpeech, TermProperties>.GetGroups(this.TermProperties, p => p.PartOfSpeech).ToList(); }
            private set { this.GroupedTermProperties = value; }
        }

        //public IEnumerable<Group<TermProperties, PartOfSpeech>> Items { get; set; }

        ///// <summary>
        ///// A collection of all term properties objects.
        ///// </summary>
        //public List<Group<TermProperties, PartOfSpeech>> TermPropertiesList { get; set; }

        //public bool IsEmpty()
        //{
        //    if (string.IsNullOrEmpty(this.Term)) return true;
        //    return false;
        //}
    }

    public class Group<U, T> : List<T>
    {
        public Group() 
            :base()
        {
        }

        public Group(U key, IEnumerable<T> items)
            : base(items)
        {
            this.Key = key;
        }

        public U Key
        {
            get;
            set;
        }

        public static IEnumerable<Group<U, T>> GetGroups(List<T> itemList, Func<T, U> getKeyFunc)
        {
            IEnumerable<Group<U, T>> groupList = from item in itemList
                                                 group item by getKeyFunc(item) into g
                                                 orderby g.Key
                                                 select new Group<U,T>(g.Key, g);

            return groupList;
        }
    }
}
