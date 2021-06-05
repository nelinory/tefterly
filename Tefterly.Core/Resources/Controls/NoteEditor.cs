using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Tefterly.Core.Resources.Controls
{
    public partial class NoteEditor : RichTextBox
    {
        public NoteEditor()
        {
            // event handlers
            TextChanged += OnTextChanged;
            PreviewKeyDown += OnPreviewKeyDown;

            // Credit: http://social.msdn.microsoft.com/Forums/vstudio/en-US/0d672c70-d49d-4ebf-871d-420cc164f7d8/c-wpf-richtextbox-remove-formatting-and-line-spaces
            DataObject.AddPastingHandler(this, DataObjectPasting_EventHandler);

            // add custom binding for Ctrl+Shift+V for rich format pasting
            this.InputBindings.Add(new KeyBinding(ApplicationCommands.Paste, Key.V, ModifierKeys.Control | ModifierKeys.Shift));

            // search support
            _noteEditorSearchHighlightResults = new NoteEditorSearchHighlight(this);

            // register global commands
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(FontStylesCommand, OnFontStylesCommand, CanExecuteFontStylesCommand));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(ParagraphStylesCommand, OnParagraphStylesCommand, CanExecuteParagraphStylesCommand));
        }

        public NoteEditor(FlowDocument document) : base(document) { }

        #region Font Style Command

        public static readonly ICommand FontStylesCommand = new RoutedUICommand("FontStylesCommand", "FontStylesCommand", typeof(NoteEditor));

        private void CanExecuteFontStylesCommand(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }

        private void OnFontStylesCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Enum.TryParse(e.Parameter.ToString(), out NoteFontStyles fontStyle);
            ApplyFontStylesCommand(fontStyle);
        }

        private void ApplyFontStylesCommand(NoteFontStyles fontStyle)
        {
            switch (fontStyle)
            {
                case NoteFontStyles.Bold:
                    EditingCommands.ToggleBold.Execute(null, this);
                    break;
                case NoteFontStyles.Italic:
                    EditingCommands.ToggleItalic.Execute(null, this);
                    break;
                case NoteFontStyles.Underline:
                    EditingCommands.ToggleUnderline.Execute(null, this);
                    break;
                case NoteFontStyles.Strikethrough:
                    TextDecorationCollection textDecorationCollection = Selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
                    if (textDecorationCollection != null && textDecorationCollection.Count == 0)
                        Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough); // set the formatting
                    else
                        Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null); // clear the formatting
                    break;
                case NoteFontStyles.Highlight:
                    object backgroundProperty = Selection.GetPropertyValue(TextElement.BackgroundProperty);
                    if (backgroundProperty is SolidColorBrush && ((SolidColorBrush)backgroundProperty).Color.Equals(Colors.Yellow) == true)
                        Selection.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent); // clear the highlight from the selection
                    else
                        Selection.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow); // highlight the selection // TODO: Read from settings
                    break;
                default:
                    throw new Exception(String.Format("Illegal NoteFontStyles enumeration value {0}", fontStyle));
            }
        }

        #endregion

        #region Paragraph Style Command

        public static readonly ICommand ParagraphStylesCommand = new RoutedUICommand("ParagraphStylesCommand", "ParagraphStylesCommand", typeof(NoteEditor));

        private void CanExecuteParagraphStylesCommand(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }

        private void OnParagraphStylesCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Enum.TryParse(e.Parameter.ToString(), out NoteParagraphStyles paragraphStyle);
            ApplyParagraphStylesCommand(paragraphStyle);
        }

        private void ApplyParagraphStylesCommand(NoteParagraphStyles paragraphStyle)
        {
            switch (paragraphStyle)
            {
                case NoteParagraphStyles.Left:
                    EditingCommands.AlignLeft.Execute(null, this);
                    break;
                case NoteParagraphStyles.Center:
                    EditingCommands.AlignCenter.Execute(null, this);
                    break;
                case NoteParagraphStyles.Right:
                    EditingCommands.AlignRight.Execute(null, this);
                    break;
                case NoteParagraphStyles.Justify:
                    EditingCommands.AlignJustify.Execute(null, this);
                    break;
                case NoteParagraphStyles.IncreaseIndent:
                    EditingCommands.IncreaseIndentation.Execute(null, this);
                    break;
                case NoteParagraphStyles.DecreaseIndent:
                    EditingCommands.DecreaseIndentation.Execute(null, this);
                    break;
                case NoteParagraphStyles.List:
                    EditingCommands.ToggleBullets.Execute(null, this);
                    MoveCursorToNextContentPosition();
                    break;
                case NoteParagraphStyles.OrderedList:
                    EditingCommands.ToggleNumbering.Execute(null, this);
                    MoveCursorToNextContentPosition();
                    break;
                default:
                    throw new Exception(String.Format("Illegal NoteParagraphStyles enumeration value {0}", paragraphStyle));
            }
        }

        private void MoveCursorToNextContentPosition()
        {
            TextPointer movePosition = CaretPosition.GetNextContextPosition(LogicalDirection.Forward);

            if (movePosition != null)
                CaretPosition = movePosition;

            Focus();
        }

        #endregion

        #region BoundFlowDocument Property

        public static readonly DependencyProperty BoundFlowDocumentProperty = DependencyProperty.Register(
            nameof(BoundFlowDocument),
            typeof(FlowDocument),
            typeof(NoteEditor),
            new PropertyMetadata((sender, args) => ((NoteEditor)sender).OnBoundFlowDocumentChanged(args)));

        public FlowDocument BoundFlowDocument
        {
            get { return (FlowDocument)GetValue(BoundFlowDocumentProperty); }
            set { SetValue(BoundFlowDocumentProperty, value); }
        }

        private void OnBoundFlowDocumentChanged(DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (e == null || e.NewValue == null)
                    Document = new FlowDocument();
                else
                    Document = e.NewValue as FlowDocument;
            }
            catch (Exception)
            {
                Document = new FlowDocument();
            }
        }

        #endregion

        #region Note Search Support

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
        private NoteEditorSearchHighlight _noteEditorSearchHighlightResults;

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

        #endregion

        protected void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Document.IsLoaded == true prevents the TextChange event of firing continuously while we load the flowDocument
            if (Document != null && Document.IsLoaded == true)
            {
                SetValue(BoundFlowDocumentProperty, Document);
            }
        }

        protected void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.S: // font strike-through
                        ApplyFontStylesCommand(NoteFontStyles.Strikethrough);
                        e.Handled = true;
                        break;
                    case Key.H: // font highlight
                        ApplyFontStylesCommand(NoteFontStyles.Highlight);
                        e.Handled = true;
                        break;
                    case Key.OemOpenBrackets: // Font downscale
                    case Key.OemCloseBrackets: // Font upscale
                        e.Handled = true;
                        break;
                }
            }
        }

        protected void DataObjectPasting_EventHandler(object sender, DataObjectPastingEventArgs e)
        {
            bool richObjectPasteRequest = false;
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                richObjectPasteRequest = true;

            // pasting image from clipboard
            if (e.DataObject.GetDataPresent(DataFormats.Bitmap) == true)
            {
                DataObject dataObject = new DataObject();
                dataObject.SetData(DataFormats.Bitmap, e.DataObject.GetData(DataFormats.Bitmap));
                e.DataObject = dataObject;
            }
            // drag and drop image
            else if (e.DataObject.GetDataPresent(DataFormats.Dib) == true)
            {
                BitmapImage bitmapImage = null;
                string[] droppedFiles = (string[])e.DataObject.GetData(DataFormats.FileDrop);

                if (droppedFiles != null && droppedFiles.Length > 0)
                {
                    foreach (string droppedFile in droppedFiles)
                    {
                        try
                        {
                            bitmapImage = new BitmapImage(new Uri(droppedFile));
                            break; // just get the first image
                        }
                        catch (Exception)
                        {
                            // do nothing since dropped image cannot be loaded as bitmap
                        }
                    }
                }

                if (bitmapImage != null) // we got an image
                {
                    DataObject dataObject = new DataObject();
                    dataObject.SetData(DataFormats.Bitmap, bitmapImage);
                    e.DataObject = dataObject;
                }
            }
            // remove formatting from the pasted text or use formatted pasted text as is
            else if (e.DataObject.GetDataPresent(DataFormats.Text) == true)
            {
                if (e.SourceDataObject.GetDataPresent(DataFormats.Rtf, true) == false)
                    return;

                string rtfDocument = e.SourceDataObject.GetData(DataFormats.Rtf) as string;

                FlowDocument document = new FlowDocument();
                document.SetValue(FlowDocument.TextAlignmentProperty, TextAlignment.Right);

                TextRange textRange = Utilities.FormatFlowDocument(document);

                if (textRange.CanLoad(DataFormats.Rtf) && string.IsNullOrEmpty(rtfDocument) == false)
                {
                    using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(rtfDocument)))
                    {
                        textRange.Load(stream, DataFormats.Rtf);
                    }
                }

                DataObject dataObject = new DataObject();
                if (richObjectPasteRequest == true)
                    dataObject.SetData(DataFormats.Rtf, rtfDocument);
                else
                    dataObject.SetData(DataFormats.Text, Utilities.RemoveBulletsFromText(textRange.Text));

                e.DataObject = dataObject;
            }
        }
    }
}