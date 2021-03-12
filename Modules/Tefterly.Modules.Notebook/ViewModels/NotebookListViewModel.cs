using Prism.Mvvm;

namespace Tefterly.Modules.Notebook.ViewModels
{
    public class NotebookListViewModel : BindableBase
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public NotebookListViewModel()
        {
            Message = "NotebookList View";
        }
    }
}
