﻿using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using Tefterly.Business;
using Tefterly.Core;
using Tefterly.Core.Commands;
using Tefterly.Core.Events;
using Tefterly.Services;

namespace Tefterly.Modules.Notes.ViewModels
{
    public class NotesViewModel : BindableBase, INavigationAware
    {
        private ObservableCollection<Business.Models.Note> _noteList;
        public ObservableCollection<Business.Models.Note> NoteList
        {
            get { return _noteList; }
            set { SetProperty(ref _noteList, value); }
        }

        private Business.Models.Note _selectedNote;
        public Business.Models.Note SelectedNote
        {
            get { return _selectedNote; }
            set
            {
                SetProperty(ref _selectedNote, value);
                ExecuteNavigation(_selectedNote);
            }
        }

        private Guid _selectedNotebookCategoryId;
        public Guid SelectedNotebookCategoryId
        {
            get { return _selectedNotebookCategoryId; }
            set { SetProperty(ref _selectedNotebookCategoryId, value); }
        }

        private bool _showNotesNotFoundPanel = false;
        public bool ShowNotesNotFoundPanel
        {
            get { return _showNotesNotFoundPanel; }
            set { SetProperty(ref _showNotesNotFoundPanel, value); }
        }

        private bool _showAddNoteButton;
        public bool ShowAddNoteButton
        {
            get { return _showAddNoteButton; }
            set { SetProperty(ref _showAddNoteButton, value); }
        }

        // services
        private readonly INoteService _noteService;
        private readonly IApplicationCommands _applicationCommands;
        private readonly IEventAggregator _eventAggregator;

        public NotesViewModel(INoteService noteService, IApplicationCommands applicationCommands, IEventAggregator eventAggregator)
        {
            // attach all required services
            _noteService = noteService;
            _eventAggregator = eventAggregator;

            // attach all composite commands
            _applicationCommands = applicationCommands;

            // subscribe to important events
            _eventAggregator.GetEvent<NoteChangedEvent>().Subscribe(x => { LoadNoteList(SelectedNotebookCategoryId, isNavigationAction: false); });
        }

        private void LoadNoteList(Guid notebookGategory, bool isNavigationAction)
        {
            ObservableCollection<Business.Models.Note> tempNoteList = new ObservableCollection<Business.Models.Note>(_noteService.GetNotes(notebookGategory));

            if (tempNoteList.Count > 0)
            {
                int selectedNoteIndex = tempNoteList.IndexOf(SelectedNote);
                if (SelectedNote != null && selectedNoteIndex > -1 && isNavigationAction == false)
                    SelectedNote = tempNoteList[selectedNoteIndex]; // keep existing selection if note state changed
                else
                    SelectedNote = tempNoteList[0]; // select the first item
            }
            else
                SelectedNote = null; // no notes found in the selected category

            NoteList = tempNoteList; // bind it to the live NoteList
            ShowNotesNotFoundPanel = (NoteList.Count == 0);
            ShowAddNoteButton = (SelectedNotebookCategoryId == NotebookCategories.Default);
        }

        private void ExecuteNavigation(Business.Models.Note selectedNote)
        {
            Guid id = (selectedNote == null) ? Guid.Empty : selectedNote.Id;

            NavigationItem navigationItem = new NavigationItem
            {
                NavigationPath = NavigationPaths.Note,
                NavigationRegion = RegionNames.NoteRegion,
                NavigationParameters = new NavigationParameters { { "id", id } }
            };

            _applicationCommands.NavigateCommand.Execute(navigationItem);
        }

        #region Navigation Logic

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            SelectedNotebookCategoryId = navigationContext.Parameters.GetValue<Guid>("id");

            LoadNoteList(SelectedNotebookCategoryId, isNavigationAction: true);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        #endregion
    }
}
