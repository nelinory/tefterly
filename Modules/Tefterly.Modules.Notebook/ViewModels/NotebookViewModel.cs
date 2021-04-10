using ModernWpf;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using Tefterly.Core;
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

        public string Version
        {
            get
            {
                System.Version versionObject = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
                return $"v{versionObject.Major}.{versionObject.Minor}.{versionObject.Build}";
            }
        }

        // services
        private INoteService _noteService;

        // commands
        public DelegateCommand ChangeThemeCommand { get; set; }

        public NotebookViewModel(INoteService noteService)
        {
            // attach all required services
            _noteService = noteService;

            // attach all commands
            ChangeThemeCommand = new DelegateCommand(() => ExecuteChangeThemeCommand());

            NotebookList = new ObservableCollection<Business.Models.Notebook>(_noteService.GetAllNotebookCategories());

            RefreshCategoryCounts();
        }

        private void RefreshCategoryCounts()
        {
            foreach (Business.Models.Notebook notebook in NotebookList)
            {
                notebook.TotalItems = _noteService.GetCategoryCount(notebook.Id);
            }
        }

        private void ExecuteChangeThemeCommand()
        {
            if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            else
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
        }
    }
}
