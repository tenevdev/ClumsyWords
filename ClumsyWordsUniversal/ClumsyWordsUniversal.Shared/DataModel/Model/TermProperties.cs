using ClumsyWordsUniversal.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ClumsyWordsUniversal.Data
{
    public class TermProperties : BindableBase
    {
        public TermProperties() : this(string.Empty, string.Empty, PartOfSpeech.other, string.Empty) { }

        public TermProperties(string term, string definition, PartOfSpeech partOfSpeech, string example)
        {
            this._term = term;
            this._definition = definition;
            this.PartOfSpeech = partOfSpeech;
            this.Example = example;
        }

        /// <summary>
        /// The term itself.
        /// </summary>
        private string _term;
        public string Term 
        {
            get { return this._term; }
            set { this.SetProperty(ref this._term, value); } 
        }

        /// <summary>
        /// The meaning of the term.
        /// </summary>
        private string _definition;
        public string Definition 
        {
            get { return this._definition; }
            set { this.SetProperty(ref this._definition, value); }
        }

        /// <summary>
        /// Shows what part of speech the term is.
        /// </summary>
        public PartOfSpeech PartOfSpeech { get; set; }

        /// <summary>
        /// Provides an example use of the term if available.
        /// </summary>
        public string Example { get; set; }
    }
}
