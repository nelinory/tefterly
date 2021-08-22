using System.Collections.Generic;
using System.Windows.Documents;

namespace Tefterly.Core.Resources.Controls
{
    public class NoteEditorSearchHighlight
    {
        private static NoteEditor _noteEditor;
        private static AdornerLayer _noteEditorAdornerLayer;
        private static List<NoteEditorSearchHighlightAdorner> _noteEditorSearchHighlightAdorners;

        public static void Init(NoteEditor noteEditor)
        {
            _noteEditor = noteEditor;
            _noteEditorAdornerLayer = AdornerLayer.GetAdornerLayer(_noteEditor);
            _noteEditorSearchHighlightAdorners = new List<NoteEditorSearchHighlightAdorner>();
        }

        public static void Add(TextRange textRange, bool forceAdornerLayerUpdate = true)
        {
            NoteEditorSearchHighlightAdorner noteEditorSearchHighlightAdorner = new NoteEditorSearchHighlightAdorner(_noteEditor, textRange);
            _noteEditorSearchHighlightAdorners.Add(noteEditorSearchHighlightAdorner);

            _noteEditorAdornerLayer.Add(noteEditorSearchHighlightAdorner);
            if (forceAdornerLayerUpdate == true)
                _noteEditorAdornerLayer.Update();
        }

        public static void Update()
        {
            if (_noteEditorAdornerLayer == null)
                _noteEditorAdornerLayer = AdornerLayer.GetAdornerLayer(_noteEditor);

            _noteEditorAdornerLayer.InvalidateVisual();
            _noteEditorAdornerLayer.Update();
        }

        public static void Clear()
        {
            if (_noteEditorAdornerLayer == null)
                _noteEditorAdornerLayer = AdornerLayer.GetAdornerLayer(_noteEditor);

            foreach (NoteEditorSearchHighlightAdorner noteEditorSearchHighlightAdorner in _noteEditorSearchHighlightAdorners)
            {
                _noteEditorAdornerLayer.Remove(noteEditorSearchHighlightAdorner);
            }

            _noteEditorSearchHighlightAdorners.Clear();
            _noteEditorAdornerLayer.Update();
        }
    }
}