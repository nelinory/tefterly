﻿using ModernWpf;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Tefterly.Core;
using Tefterly.Core.Commands;
using Tefterly.Core.Events;
using Tefterly.Core.Navigation;
using Tefterly.Services;

namespace Tefterly.Modules.Notebook.ViewModels
{
    public class NotebookViewModel : BindableBase
    {
        private ObservableCollection<Core.Models.Notebook> _notebookList;
        public ObservableCollection<Core.Models.Notebook> NotebookList
        {
            get { return _notebookList; }
            set { SetProperty(ref _notebookList, value); }
        }

        private Core.Models.Notebook _selectedNotebook;
        public Core.Models.Notebook SelectedNotebook
        {
            get { return _selectedNotebook; }
            set
            {
                SetProperty(ref _selectedNotebook, value);
                ExecuteNavigation(_selectedNotebook);
            }
        }

        public static string Version
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
        private readonly ISettingsService _settingsService;

        // commands
        public DelegateCommand ChangeThemeCommand { get; set; }
        public DelegateCommand ShowSettingsDialogCommand { get; set; }

        public NotebookViewModel(INoteService noteService, IApplicationCommands applicationCommands, IEventAggregator eventAggregator, ISettingsService settingsService)
        {
            // attach all required services
            _noteService = noteService;
            _eventAggregator = eventAggregator;
            _settingsService = settingsService;

            // attach all composite commands
            _applicationCommands = applicationCommands;

            // attach all commands
            ChangeThemeCommand = new DelegateCommand(() => ExecuteChangeThemeCommand());
            ShowSettingsDialogCommand = new DelegateCommand(() => _eventAggregator.GetEvent<ModifySettingsEvent>().Publish(String.Empty));

            // subscribe to important events
            _eventAggregator.GetEvent<NoteChangedEvent>().Subscribe(x => { RefreshCategoryCounts(); });
            _eventAggregator.GetEvent<ResetNotebookCategoryEvent>().Subscribe(x => { SelectedNotebook = NotebookList[0]; });

            LoadNotebookList();
        }

        private void LoadNotebookList()
        {
            NotebookList = new ObservableCollection<Core.Models.Notebook>(_noteService.GetAllNotebookCategories());

            if (NotebookList.Count > 0)
            {
                if (_settingsService.Settings.General.RememberLastUsedCategory == true)
                    SelectedNotebook = NotebookList.Where(m => m.Id == _settingsService.Settings.General.LastUsedCategory).FirstOrDefault();

                if (SelectedNotebook == null)
                    SelectedNotebook = NotebookList[0]; // select the first available category
            }

            RefreshCategoryCounts();
        }

        private void RefreshCategoryCounts()
        {
            foreach (Core.Models.Notebook notebook in NotebookList)
            {
                notebook.TotalItems = _noteService.GetCategoryCount(notebook.Id);
            }
        }

        private void ExecuteNavigation(Core.Models.Notebook selectedNotebook)
        {
            Guid id = (selectedNotebook == null) ? Guid.Empty : selectedNotebook.Id;

            NavigationItem navigationItem = new NavigationItem
            {
                NavigationPath = NavigationPaths.Notes,
                NavigationRegion = RegionNames.NotesRegion,
                NavigationParameters = new NavigationParameters { { "id", id } }
            };

            if (id != Guid.Empty
                && _settingsService.Settings.General.RememberLastUsedCategory == true
                && _settingsService.Settings.General.LastUsedCategory != id)
                _settingsService.Settings.General.LastUsedCategory = id;

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
