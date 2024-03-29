﻿using System;
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
            PreviewMouseDown += OnPreviewMouseDown; // PreviewMouseDown for image resize adorner & hyperlink copy link context menu

            // Credit: http://social.msdn.microsoft.com/Forums/vstudio/en-US/0d672c70-d49d-4ebf-871d-420cc164f7d8/c-wpf-richtextbox-remove-formatting-and-line-spaces
            DataObject.AddPastingHandler(this, DataObjectPastingEventHandler);

            // search results highlight support
            NoteEditorSearchHighlight.Init(this);

            // image resize support
            ImageResizeHelper.Init(this);

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
            if (Parent is ScrollViewer parent)
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
                    default:
                        break;
                }
            }
            else if (e.Key == Key.Space || e.Key == Key.Enter)
                ApplyHyperlinkFormat();
            else if (e.Key == Key.Back)
                RemoveHyperlinkFormat();
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextPointer textPointer = GetPositionFromPoint(e.GetPosition(this), false);

            // image in BlockUIContainer element
            if (e.ChangedButton == MouseButton.Left && textPointer != null && textPointer.Parent is BlockUIContainer)
            {
                UIElement uiElement = (textPointer.Parent as BlockUIContainer).Child;
                if (uiElement == null)
                    return;

                if (uiElement is Image)
                {
                    ImageResizeHelper.UpdateImageResizers(uiElement as Image);

                    // update the flowdocument when the image is resized
                    ImageResizeHelper.ImageResized += () => SetValue(BoundFlowDocumentProperty, Document);
                    return;
                }
            }

            // hyperlink support
            if (e.ChangedButton == MouseButton.Right)
            {
                Hyperlink hyperlink = (Mouse.DirectlyOver as Run)?.Parent as Hyperlink;
                foreach (MenuItem item in ContextMenu.Items)
                {
                    if (item.Tag != null && item.Tag.ToString() == "miCopyLink")
                    {
                        item.CommandParameter = (hyperlink == null) ? String.Empty : hyperlink.NavigateUri.ToString();
                        item.Visibility = (hyperlink == null) ? Visibility.Collapsed : Visibility.Visible;
                        break;
                    }
                }
            }

            ImageResizeHelper.ClearImageResizers();
        }

        protected void DataObjectPastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            // pasting image from clipboard
            if (e.DataObject.GetDataPresent(DataFormats.Bitmap) == true)
            {
                e.DataObject = new DataObject(DataFormats.Bitmap, e.DataObject.GetData(DataFormats.Bitmap));
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
                    e.DataObject = new DataObject(DataFormats.Bitmap, bitmapImage);
            }
            // remove formatting from the pasted text
            else if (e.DataObject.GetDataPresent(DataFormats.Text) == true)
            {
                string clipboardText = e.DataObject.GetData(DataFormats.UnicodeText) as string;

                if (IsValidUrl(clipboardText) == true)
                {
                    CaretPosition.InsertTextInRun(clipboardText);
                    ApplyHyperlinkFormat();

                    e.CancelCommand();
                    e.Handled = true;
                }
                else
                    e.DataObject = new DataObject(DataFormats.Text, Utilities.ConvertBulletsInText(clipboardText));
            }
        }
    }
}