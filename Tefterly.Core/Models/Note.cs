using System;
using System.Text.Json.Serialization;
using System.Windows.Documents;

namespace Tefterly.Core.Models
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

        private string _content;
        [JsonIgnore]
        public string Content
        {
            get { return _content; }
        }

        private FlowDocument _document;
        [JsonIgnore]
        public FlowDocument Document
        {
            get { return _document; }
            set
            {
                SetProperty(ref _document, value);
                SetProperty(ref _content, Utilities.GetTextFromFlowDocument(value));
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
        [JsonIgnore]
        public bool IsStarred
        {
            get { return NotebookCategory == NotebookCategories.Starred; }
        }
        [JsonIgnore]
        public bool IsArchived
        {
            get { return NotebookCategory == NotebookCategories.Archived; }
        }
        [JsonIgnore]
        public bool IsDeleted
        {
            get { return NotebookCategory == NotebookCategories.Deleted; }
        }
    }
}