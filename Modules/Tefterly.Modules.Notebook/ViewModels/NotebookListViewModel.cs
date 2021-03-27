using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace Tefterly.Modules.Notebook.ViewModels
{
    public class NotebookListViewModel : BindableBase
    {
        private ObservableCollection<Core.Models.Notebook> _notebookList;
        public ObservableCollection<Core.Models.Notebook> NotebookList
        {
            get { return _notebookList; }
            set { SetProperty(ref _notebookList, value); }
        }

        private string _version = "v1.0";
        public string Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
        }

        public NotebookListViewModel()
        {
            NotebookList = new ObservableCollection<Core.Models.Notebook>();
            _notebookList.Add(new Core.Models.Notebook() { Id = Guid.NewGuid(), Title = "Notes", IconFont = "\xE8F1", TotalItems = 4 });
            _notebookList.Add(new Core.Models.Notebook() { Id = Guid.NewGuid(), Title = "Starred", IconFont = "\xE734", TotalItems = 1 });
            _notebookList.Add(new Core.Models.Notebook() { Id = Guid.NewGuid(), Title = "Archived", IconFont = "\xF12B", TotalItems = 2 });
            _notebookList.Add(new Core.Models.Notebook() { Id = Guid.NewGuid(), Title = "Deleted", IconFont = "\xE74D", TotalItems = 1 });
        }
    }
}
