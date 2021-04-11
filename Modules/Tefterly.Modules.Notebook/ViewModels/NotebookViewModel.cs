using ModernWpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using Tefterly.Business;
using Tefterly.Core;
using Tefterly.Core.Commands;
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

        private Business.Models.Notebook _selectedNotebook;
        public Business.Models.Notebook SelectedNotebook
        {
            get { return _selectedNotebook; }
            set
            {
                SetProperty(ref _selectedNotebook, value);

                // signal selected notebook changed
                if (_selectedNotebook != null)
                    ExecuteNavigation(_selectedNotebook.Id);
            }
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
        private readonly INoteService _noteService;
        private readonly IApplicationCommands _applicationCommands;

        // commands
        public DelegateCommand ChangeThemeCommand { get; set; }

        public NotebookViewModel(INoteService noteService, IApplicationCommands applicationCommands)
        {
            // attach all required services
            _noteService = noteService;

            // attach all composite commands
            _applicationCommands = applicationCommands;

            // attach all commands
            ChangeThemeCommand = new DelegateCommand(() => ExecuteChangeThemeCommand());

            LoadNotebookList();

            RefreshCategoryCounts();
        }

        private void LoadNotebookList()
        {
            NotebookList = new ObservableCollection<Business.Models.Notebook>(_noteService.GetAllNotebookCategories());

            if (NotebookList.Count > 0)
                SelectedNotebook = NotebookList[0]; // select the first item
        }

        private void RefreshCategoryCounts()
        {
            foreach (Business.Models.Notebook notebook in NotebookList)
            {
                notebook.TotalItems = _noteService.GetCategoryCount(notebook.Id);
            }
        }

        private void ExecuteNavigation(Guid id)
        {
            NavigationItem navigationItem = new NavigationItem
            {
                NavigationPath = NavigationPaths.Notes,
                NavigationRegion = RegionNames.NotesRegion,
                NavigationParameters = new NavigationParameters { { "id", id } }
            };

            _applicationCommands.NavigateCommand.Execute(navigationItem);
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
