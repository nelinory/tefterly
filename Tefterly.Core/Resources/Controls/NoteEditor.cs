using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Tefterly.Core.Resources.Controls
{
    public partial class NoteEditor : RichTextBox
    {
        public NoteEditor()
        {
            // event handlers
            TextChanged += OnTextChanged;
        }

        public NoteEditor(FlowDocument document) : base(document) { }

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
    }
}