using ClumsyWordsUniversal.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ClumsyWordsUniversal.Common
{
    /// <summary>
    /// Manages communication with the WebAPI sending requests and formatting responses
    /// Exposes static method for http request when searching for definitions on the server
    /// Parses the response from the server and returns a useful object in the context of the data model
    /// </summary>
    public class SearchService
    {
        const string NounString1 = "noun";
        const string NounString2 = "propernoun";
        const string PronounString = "pronoun";
        const string AdjectiveString1 = "adj";
        const string AdjectiveString2 = "adjective";
        const string VerbString = "verb";
        const string AdverbString = "adverb";
        const string PrepositionString = "preposition";
        const string ConjunctionString = "conjunction";
        const string InterjectionString = "interjection";
        const string SymbolString = "symbol";
        const string AbbreviationString = "abbreviation";
        const string PrefixString = "prefix";
        const string ProverbString = "proverb";
        const string NumeralString = "numeral";
        const string SuffixString = "suffix";
        const string ContractionString = "contraction";
        const string LetterString = "letter";


        public static bool TryParseXMLResponse(string responseText, Func<TermProperties, PartOfSpeech> getKeyFunc, string term, out DefinitionsDataItem results)
        {
            results = new DefinitionsDataItem();

            if (responseText == String.Empty) return false;

            XDocument doc = XDocument.Parse(responseText);
            if (doc.Element("results").Elements("results") != null)
            {
                List<TermProperties> termProperties = (from _props in doc.Element("results").Elements("result")
                                                       select new TermProperties
                                                       {
                                                           Term = _props.Element("term").Value.ToString(),
                                                           Definition = _props.Element("definition").Value.ToString(),
                                                           Example = _props.Element("example").Value.ToString(),
                                                           PartOfSpeech = SearchService.PartOfSpeechStringConversion(_props.Element("partofspeech").Value.ToString().ToLower())
                                                       }).ToList();

                var result = (from tp in termProperties
                              group tp by getKeyFunc(tp) into g
                              orderby g.Key
                              select new CommonGroup<TermProperties>(g.Key.ToString(), g.Key.ToString(), g, true)).ToList();

                if (result.Count == 0)
                    return false;

                results = new DefinitionsDataItem(term, new ObservableCollection<CommonGroup<TermProperties>>(result));

                return true;
            }
            return false;
        }

        public static async Task<DefinitionsDataItem> GetSearchResultAsync(string url, string term)
        {
            HttpClient client = new HttpClient();

            string searchQuery = url + term;
            try
            {
                HttpResponseMessage response = await client.GetAsync(searchQuery);

            string responseText = await response.Content.ReadAsStringAsync();

            DefinitionsDataItem termProps;

            if (SearchService.TryParseXMLResponse(responseText, p => p.PartOfSpeech, term, out termProps))
                return termProps;
            }
            catch (Exception ex)
            {
                // Maybe there is no connection to the Internet or the server is down
                return default(DefinitionsDataItem);
            }

            return default(DefinitionsDataItem);
        }

        private static PartOfSpeech PartOfSpeechStringConversion(string key)
        {

            switch (key.ToLower())
            {
                case NounString1:
                case NounString2:
                    return PartOfSpeech.noun;
                case PronounString:
                    return PartOfSpeech.pronoun;
                case AdjectiveString1:
                case AdjectiveString2:
                    return PartOfSpeech.adjective;
                case AdverbString:
                    return PartOfSpeech.adverb;
                case VerbString:
                    return PartOfSpeech.verb;
                case PrepositionString:
                    return PartOfSpeech.preposition;
                case ConjunctionString:
                    return PartOfSpeech.conjunction;
                case InterjectionString:
                    return PartOfSpeech.interjection;
                case SymbolString:
                    return PartOfSpeech.symbol;
                case AbbreviationString:
                    return PartOfSpeech.abbreviation;
                case PrefixString:
                    return PartOfSpeech.prefix;
                case ProverbString:
                    return PartOfSpeech.proverb;
                case NumeralString:
                    return PartOfSpeech.numeral;
                case SuffixString:
                    return PartOfSpeech.suffix;
                case ContractionString:
                    return PartOfSpeech.contraction;
                case LetterString:
                    return PartOfSpeech.letter;
                default:
                    return PartOfSpeech.other;
            }
        }
    }
}
