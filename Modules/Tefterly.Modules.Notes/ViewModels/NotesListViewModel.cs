using Prism.Mvvm;

namespace Tefterly.Modules.Notes.ViewModels
{
    public class NotesListViewModel : BindableBase
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public NotesListViewModel()
        {
            Message = "NotesList View";
        }
    }
}
