using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Tefterly.Core.Resources.Controls
{
    public partial class NoteEditor : RichTextBox
    {
        public NoteEditor()
        {
            // event handlers
            TextChanged += OnTextChanged;
            PreviewKeyDown += OnPreviewKeyDown;

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
                        Selection.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow); // highlight the selection
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

        #region BoundFlowDocument property

        public static readonly DependencyProperty BoundFlowDocumentProperty = DependencyProperty.Register(
            nameof(BoundFlowDocument),
            typeof(FlowDocument),
            typeof(NoteEditor),
            new PropertyMetadata((sender, args) => ((NoteEditor)sender).OnBoundFlowDocumentChanged(args)));

        public FlowDocument BoundFlowDocument
        {
            get
            {
                return (FlowDocument)GetValue(BoundFlowDocumentProperty);
            }
            set
            {
                SetValue(BoundFlowDocumentProperty, value);
            }
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
                }
            }
        }
    }
}