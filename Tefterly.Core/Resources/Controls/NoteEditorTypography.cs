using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Tefterly.Core.Resources.Controls
{
    public partial class NoteEditor
    {
        #region Font Style Command

        public static readonly ICommand FontStylesCommand = new RoutedUICommand("FontStylesCommand", "FontStylesCommand", typeof(NoteEditor));

        private void CanExecuteFontStylesCommand(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }

        private void OnFontStylesCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (Enum.TryParse(e.Parameter.ToString(), out NoteFontStyles fontStyle) == true)
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
                    if (Selection.GetPropertyValue(Inline.TextDecorationsProperty) is TextDecorationCollection textDecorationCollection && textDecorationCollection.Count == 0)
                        Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough); // set the formatting
                    else
                        Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null); // clear the formatting
                    break;
                case NoteFontStyles.Highlight:
                    if (Selection.GetPropertyValue(TextElement.BackgroundProperty) is SolidColorBrush brush && brush.Color.Equals(Colors.Yellow) == true)
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
            if (Enum.TryParse(e.Parameter.ToString(), out NoteParagraphStyles paragraphStyle) == true)
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
    }
}
