using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
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
            InputBindings.Add(new KeyBinding(ApplicationCommands.Paste, Key.V, ModifierKeys.Control | ModifierKeys.Shift));

            // search results highlight support
            NoteEditorSearchHighlight.Init(this);

            // register global commands
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(FontStylesCommand, OnFontStylesCommand, CanExecuteFontStylesCommand));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(ParagraphStylesCommand, OnParagraphStylesCommand, CanExecuteParagraphStylesCommand));
        }

        public NoteEditor(FlowDocument document) : base(document) { }

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
                if (e.NewValue == null)
                    Document = new FlowDocument();
                else
                    Document = e.NewValue as FlowDocument;
            }
            catch (Exception)
            {
                Document = new FlowDocument();
            }

            // scrollviewer appears to remember scroll position between different notes
            // this is a hack to reset the scrollviewer to top every time we load switch between notes
            if (this.Parent is ScrollViewer parent)
                parent.ScrollToTop();

            SubscribeToAllHyperlinks(Document);
        }

        #endregion

        protected void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Document.IsLoaded == true prevents the TextChange event of firing continuously while we load the flowDocument
            if (Document != null && Document.IsLoaded == true)
                SetValue(BoundFlowDocumentProperty, Document);
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
            else if (e.Key == Key.Space || e.Key == Key.Enter)
                ApplyHyperlinkFormat();
            else if (e.Key == Key.Back)
                RemoveHyperlinkFormat();
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
                    try
                    {
                        using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(rtfDocument)))
                        {
                            textRange.Load(stream, DataFormats.Rtf);
                        }
                    }
                    catch (Exception)
                    {
                        // something happen while loading the rtf object, most likely incompatible format
                        return;
                    }
                }

                DataObject dataObject = new DataObject();
                if (richObjectPasteRequest == true)
                    dataObject.SetData(DataFormats.Rtf, rtfDocument);
                else
                    dataObject.SetData(DataFormats.Text, Utilities.ConvertBulletsInText(textRange.Text));

                e.DataObject = dataObject;
            }
        }
    }
}