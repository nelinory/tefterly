using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Tefterly.Core.Resources.Controls
{
    public class NoteEditorSearchHighlightAdorner : Adorner
    {
        private NoteEditor _noteEditor;
        private TextRange _textRange;
        private Rect _searchHighlightRect;

        public NoteEditorSearchHighlightAdorner(NoteEditor noteEditor, TextRange textRange) : base(noteEditor)
        {
            _noteEditor = noteEditor;
            _textRange = textRange;
            _searchHighlightRect = new Rect();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            try
            {
                Rect leftSide = _textRange.Start.GetCharacterRect(LogicalDirection.Forward);
                Rect rightSide = _textRange.End.GetCharacterRect(LogicalDirection.Backward);

                // when application window is resizing these may not have values
                if (leftSide.IsEmpty == true || rightSide.IsEmpty == true)
                    return;
                
                _searchHighlightRect.X = leftSide.X;
                _searchHighlightRect.Width = rightSide.Right - leftSide.Left;
                _searchHighlightRect.Height = rightSide.Bottom - leftSide.Top;
                _searchHighlightRect.Y = rightSide.Y;

                if ((_searchHighlightRect.X < 0
                    || _searchHighlightRect.Y < 0
                    || _searchHighlightRect.X > _noteEditor.ActualWidth
                    || _searchHighlightRect.Y > (_noteEditor.ActualHeight - _noteEditor.Margin.Bottom)))
                    return;

                // TODO: Read this from settings
                drawingContext.PushOpacity(0.5);
                drawingContext.DrawRectangle(new SolidColorBrush(Colors.Orange), null, _searchHighlightRect);
                drawingContext.Pop();
            }
            catch (Exception ex)
            {
                // TODO: Logging
            }
        }
    }
}