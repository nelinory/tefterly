using Prism.Mvvm;
using System;
using System.Windows.Documents;
using Tefterly.Core;

namespace Tefterly.Business.Models
{
    public class Note : ModelChangeTrackingBase
    {
        private Guid _id;
        public Guid Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public string Content
        {
            get { return Utilities.GetTextFromFlowDocument(Document); }
        }
        public FlowDocument _document;
        public FlowDocument Document
        {
            get { return _document; }
            set
            {
                SetProperty(ref _document, value);
                RaisePropertyChanged("Content");
            }
        }

        private Guid _notebookCategory;
        public Guid NotebookCategory
        {
            get { return _notebookCategory; }
            set
            {
                SetProperty(ref _notebookCategory, value);
                RaisePropertyChanged("IsStarred");
                RaisePropertyChanged("IsArchived");
                RaisePropertyChanged("IsDeleted");
            }
        }

        // state
        public bool IsStarred
        {
            get { return NotebookCategory == NotebookCategories.Starred; }
        }
        public bool IsArchived
        {
            get { return NotebookCategory == NotebookCategories.Archived; }
        }
        public bool IsDeleted
        {
            get { return NotebookCategory == NotebookCategories.Deleted; }
        }
    }
}