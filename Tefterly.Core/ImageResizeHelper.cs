using System;
using System.Windows.Controls;
using System.Windows.Documents;
using Tefterly.Core.Resources.Controls;

namespace Tefterly.Core
{
    public class ImageResizeHelper
    {
        private static NoteEditor _noteEditor;
        private static ResizingAdorner _currentResizingAdorner = null;
        private static bool _imageResized = false;

        public delegate void Notify();
        public static event Notify ImageResized;

        public static void Init(NoteEditor noteEditor)
        {
            _noteEditor = noteEditor;
        }

        public static void UpdateImageResizers(Image image)
        {
            AdornerLayer noteEditorAdornerLayer = AdornerLayer.GetAdornerLayer(_noteEditor);

            if (_currentResizingAdorner != null)
                ClearImageResizers();

            _currentResizingAdorner = new ResizingAdorner(image, _noteEditor.ActualWidth, _noteEditor.ActualHeight);
            _currentResizingAdorner.AdornerResized += () =>
            {
                if (_imageResized == false)
                {
                    _imageResized = true;
                    ImageResized?.Invoke();

                    System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay} - [Event] AdornerResized triggered");
                }
            };

            noteEditorAdornerLayer.Add(_currentResizingAdorner);
            noteEditorAdornerLayer.Update();
        }

        public static void ClearImageResizers()
        {
            if (_currentResizingAdorner != null)
            {
                AdornerLayer noteEditorAdornerLayer = AdornerLayer.GetAdornerLayer(_noteEditor);

                noteEditorAdornerLayer.Remove(_currentResizingAdorner);
                noteEditorAdornerLayer.Update();

                // clear all internal variables
                _currentResizingAdorner = null;
                _imageResized = false;
            }
        }
    }
}