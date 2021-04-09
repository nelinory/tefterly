using Prism.Mvvm;
using System.Collections.ObjectModel;
using Tefterly.Core;
using Tefterly.Services;

namespace Tefterly.Modules.Notes.ViewModels
{
    public class NotesViewModel : BindableBase
    {
        private ObservableCollection<Business.Models.Note> _notesList;
        public ObservableCollection<Business.Models.Note> NotesList
        {
            get { return _notesList; }
            set { SetProperty(ref _notesList, value); }
        }

        private INoteService _noteService;

        public NotesViewModel(INoteService noteService)
        {
            _noteService = noteService;

            NotesList = new ObservableCollection<Business.Models.Note>(_noteService.GetNotes(NotebookCategories.Default));
        }
    }
}
