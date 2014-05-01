using ClumsyWordsUniversal.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Windows.UI.Xaml.Data;

namespace ClumsyWordsUniversal.Common.Converters
{
    public sealed class DefinitionsToHTMLConverter : IValueConverter
    {
        private string FirstLetterToUppercase(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            return char.ToUpper(text[0]) + text.Substring(1);
        }

        private string GenerateHTMLForItem(DefinitionsDataItem item, string chunk)
        {
            foreach (var g in item.Items)
            {
                chunk += String.Format("<h3>{0}</h3><br>", g.Title);
                foreach (var d in g.Items)
                {
                    if (string.IsNullOrEmpty(d.Example))
                    {
                        chunk += String.Format(
                            "<pre><u>Definition</u>: {0}<br></pre>",
                            d.Definition);
                    }
                    else
                    {
                        chunk += String.Format(
                            "<pre><u>Definition</u>: {0}<br>"
                            + "<u>Example</u>: {1}</pre>",
                            d.Definition,
                            d.Example);
                    }
                }
            }

            return chunk;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DefinitionsDataItem item = (DefinitionsDataItem)value;


            if (item == null)
                return String.Empty;

            string chunk = String.Empty;
            chunk += String.Format("<h2>{0}</h2><br>", item.Term);
            FirstLetterToUppercase(item.Term);

            if (parameter != null)
            {
                ObservableCollection<CommonGroup<TermProperties>> props = (ObservableCollection<CommonGroup<TermProperties>>)parameter;
                foreach (var g in props)
                {
                    chunk += String.Format("<h3>{0}</h3><br>", g.Title);
                    chunk = GenerateHTMLForItem(item, chunk);
                }
            }
            else 
            {
                return GenerateHTMLForItem(item, chunk);
            }
                return chunk;    
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
