using ModernWpf;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using Tefterly.Business;
using Tefterly.Core;
using Tefterly.Core.Commands;
using Tefterly.Core.Events;
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
                ExecuteNavigation(_selectedNotebook);
            }
        }

        public string Version
        {
            get
            {
                Version versionObject = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
                return $"v{versionObject.Major}.{versionObject.Minor}.{versionObject.Build}";
            }
        }

        // services
        private readonly INoteService _noteService;
        private readonly IApplicationCommands _applicationCommands;
        private readonly IEventAggregator _eventAggregator;

        // commands
        public DelegateCommand ChangeThemeCommand { get; set; }

        public NotebookViewModel(INoteService noteService, IApplicationCommands applicationCommands, IEventAggregator eventAggregator)
        {
            // attach all required services
            _noteService = noteService;
            _eventAggregator = eventAggregator;

            // attach all composite commands
            _applicationCommands = applicationCommands;

            // attach all commands
            ChangeThemeCommand = new DelegateCommand(() => ExecuteChangeThemeCommand());

            // subscribe to important events
            _eventAggregator.GetEvent<NoteChangedEvent>().Subscribe(x => { RefreshCategoryCounts(); });
            _eventAggregator.GetEvent<ResetNotebookCategoryEvent>().Subscribe(x => { SelectedNotebook = NotebookList[0]; });

            LoadNotebookList();
        }

        private void LoadNotebookList()
        {
            NotebookList = new ObservableCollection<Business.Models.Notebook>(_noteService.GetAllNotebookCategories());

            if (NotebookList.Count > 0)
                SelectedNotebook = NotebookList[0]; // select the first item
            else
                SelectedNotebook = null; // no notebooks found

            RefreshCategoryCounts();
        }

        private void RefreshCategoryCounts()
        {
            foreach (Business.Models.Notebook notebook in NotebookList)
            {
                notebook.TotalItems = _noteService.GetCategoryCount(notebook.Id);
            }
        }

        private void ExecuteNavigation(Business.Models.Notebook selectedNotebook)
        {
            Guid id = (selectedNotebook == null) ? Guid.Empty : selectedNotebook.Id;

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

            _eventAggregator.GetEvent<ThemeChangedEvent>().Publish(String.Empty);
        }
    }
}
