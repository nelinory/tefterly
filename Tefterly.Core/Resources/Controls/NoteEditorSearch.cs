using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;

namespace Tefterly.Core.Resources.Controls
{
    public partial class NoteEditor
    {
        #region SearchText Property

        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
            nameof(SearchTerm),
            typeof(string),
            typeof(NoteEditor),
            new PropertyMetadata((sender, args) => ((NoteEditor)sender).OnSearchTextChanged(args)));

        public string SearchTerm
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        private void OnSearchTextChanged(DependencyPropertyChangedEventArgs e)
        {
            Search(e.NewValue.ToString());
        }

        #endregion

        private MethodInfo _searchMethodAPI;
        private readonly NoteEditorSearchHighlight _noteEditorSearchHighlightResults;

        public void Search(string searchTerm)
        {
            _noteEditorSearchHighlightResults.Clear();

            if (String.IsNullOrEmpty(searchTerm) == true)
                return;

            TextRange textRange = FindText(Document.ContentStart, Document.ContentEnd, searchTerm);
            if (textRange == null)
                return;

            // bring the first search result in the view
            FrameworkContentElement fce = textRange.Start.Paragraph as FrameworkContentElement;
            if (fce != null)
                fce.BringIntoView();

            // search for the next matches until end of document
            while (textRange != null)
            {
                _noteEditorSearchHighlightResults.Add(textRange, false); // do not update adorner layout while adding search results
                textRange = FindText(textRange.End, Document.ContentEnd, searchTerm);
            }

            _noteEditorSearchHighlightResults.Update();
        }

        private TextRange FindText(TextPointer startPosition, TextPointer endPosition, string searchText)
        {
            // Credit: https://shevaspace.blogspot.com/2007/11/how-to-search-text-in-wpf-flowdocument.html
            TextRange textRange = null;
            if (startPosition.CompareTo(endPosition) < 0)
            {
                try
                {
                    if (_searchMethodAPI == null)
                        _searchMethodAPI = typeof(FrameworkElement).Assembly.GetType("System.Windows.Documents.TextFindEngine").GetMethod("Find", BindingFlags.Static | BindingFlags.Public);

                    object result = _searchMethodAPI.Invoke(null, new object[] { startPosition, endPosition, searchText, 0, CultureInfo.CurrentCulture });
                    textRange = result as TextRange;
                }
                catch (ApplicationException)
                {
                    textRange = null;
                }
            }

            return textRange;
        }
    }
}
