using Prism.Mvvm;
using System.Collections.ObjectModel;
using Tefterly.Services;

namespace Tefterly.Modules.Notebook.ViewModels
{
    public class NotebookViewModel : BindableBase
    {
        private ObservableCollection<Business.Models.Notebook> _notebookList;
        public ObservableCollection<Business.Models.Notebook> NotebookList
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

        private INoteService _noteService;

        public NotebookViewModel(INoteService noteService)
        {
            _noteService = noteService;

            NotebookList = new ObservableCollection<Business.Models.Notebook>(_noteService.GetAllNotebookCategories());
        }
    }
}
