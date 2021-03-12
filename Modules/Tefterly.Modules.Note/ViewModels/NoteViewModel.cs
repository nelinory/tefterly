using Prism.Mvvm;

namespace Tefterly.Modules.Note.ViewModels
{
    public class NoteViewModel : BindableBase
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public NoteViewModel()
        {
            Message = "Note View";
        }
    }
}
