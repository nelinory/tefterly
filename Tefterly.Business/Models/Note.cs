using Prism.Mvvm;
using System;
using Tefterly.Core;

namespace Tefterly.Business.Models
{
    public class Note : BindableBase
    {
        private Guid _id;
        public Guid Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        private DateTime _createdDateTime;
        public DateTime CreatedDateTime
        {
            get { return _createdDateTime; }
            set { SetProperty(ref _createdDateTime, value); }
        }

        private DateTime _updatedDateTime;
        public DateTime UpdatedDateTime
        {
            get { return _updatedDateTime; }
            set { SetProperty(ref _updatedDateTime, value); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }

        private string _color;
        public string Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
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