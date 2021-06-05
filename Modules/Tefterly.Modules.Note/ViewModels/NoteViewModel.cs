﻿using ModernWpf.Controls;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Windows.Threading;
using Tefterly.Core;
using Tefterly.Core.Events;
using Tefterly.Core.Resources.Controls;
using Tefterly.Services;

namespace Tefterly.Modules.Note.ViewModels
{
    public class NoteViewModel : BindableBase, INavigationAware
    {
        private Business.Models.Note _currentNote;
        public Business.Models.Note CurrentNote
        {
            get { return _currentNote; }
            set { SetProperty(ref _currentNote, value); }
        }

        private bool _showNoteComponents;
        public bool ShowNoteComponents
        {
            get { return _showNoteComponents; }
            set { SetProperty(ref _showNoteComponents, value); }
        }

        private bool _isSpellCheckEnabled;
        public bool IsSpellCheckEnabled
        {
            get { return _isSpellCheckEnabled; }
            set { SetProperty(ref _isSpellCheckEnabled, value); }
        }

        private string _noteNotFoundMessage;
        public string NoteNotFoundMessage
        {
            get { return _noteNotFoundMessage; }
            set { SetProperty(ref _noteNotFoundMessage, value); }
        }

        private readonly DispatcherTimer _autoSaveNoteTimer;
        private readonly DispatcherTimer _searchNoteResultsTimer;

        // services
        private readonly INoteService _noteService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISearchService _searchService;

        // commands
        public DelegateCommand MarkNoteAsStarredCommand { get; set; }
        public DelegateCommand DuplicateNoteCommand { get; set; }
        public DelegateCommand MarkNoteAsArchivedCommand { get; set; }
        public DelegateCommand DeleteNoteCommand { get; set; }
        public DelegateCommand PermanentlyDeleteNoteCommand { get; set; }
        public DelegateCommand ToggleSpellCheckCommand { get; set; }

        public NoteViewModel(INoteService noteService, IEventAggregator eventAggregator, ISearchService searchService)
        {
            // attach all required services
            _noteService = noteService;
            _eventAggregator = eventAggregator;
            _searchService = searchService;

            // attach all commands
            MarkNoteAsStarredCommand = new DelegateCommand(() => ExecuteChangeNotebookCategory(NotebookCategories.Starred));
            DuplicateNoteCommand = new DelegateCommand(() => ExecuteDuplicateNoteCommand());
            MarkNoteAsArchivedCommand = new DelegateCommand(() => ExecuteChangeNotebookCategory(NotebookCategories.Archived));
            DeleteNoteCommand = new DelegateCommand(() => ExecuteChangeNotebookCategory(NotebookCategories.Deleted));
            PermanentlyDeleteNoteCommand = new DelegateCommand(() => ExecutePermanentlyDeleteNoteCommand());
            ToggleSpellCheckCommand = new DelegateCommand(() => ExecuteToggleSpellCheckCommand());

            // event handlers
            _searchService.Search += (sender, e) => _searchNoteResultsTimer.Start(); // search started

            // subscribe to important events
            _eventAggregator.GetEvent<ThemeChangedEvent>().Subscribe(x => { SearchNoteResultsHandler(null, null); });

            // autosave
            _autoSaveNoteTimer = new DispatcherTimer();
            _autoSaveNoteTimer.Interval = TimeSpan.FromSeconds(7); // TODO: Add to settings
            _autoSaveNoteTimer.Tick += AutoSaveNoteHandler;

            // search note
            _searchNoteResultsTimer = new DispatcherTimer();
            _searchNoteResultsTimer.Interval = TimeSpan.FromMilliseconds(10); // TODO: Add to settings
            _searchNoteResultsTimer.Tick += SearchNoteResultsHandler;

            IsSpellCheckEnabled = false; // TODO: Add to settings
        }

        private void LoadNote(Guid noteId)
        {
            CurrentNote = _noteService.GetNote(noteId);

            if (CurrentNote != null)
            {
                CurrentNote.TrackChanges = true;
                CurrentNote.ModelChanged += (sender, e) => SendNoteChangedEvent();
            }

            ShowNoteComponents = (CurrentNote != null);
            NoteNotFoundMessage = String.Empty;
            if (_searchService.IsSearchInProgress() == false && CurrentNote == null)
                NoteNotFoundMessage = "Please, select a category with notes or create a new note";

            // if the search is still active and we are switching between search results then activate the search note timer
            if (_searchService.IsSearchInProgress() == true)
                _searchNoteResultsTimer.Start();
        }

        private void ExecuteChangeNotebookCategory(Guid notebookCategory)
        {
            if (CurrentNote == null || notebookCategory == null)
                return;

            if (CurrentNote.NotebookCategory == notebookCategory)
                CurrentNote.NotebookCategory = NotebookCategories.Default;
            else
                CurrentNote.NotebookCategory = notebookCategory;

            SendNoteChangedEvent();
        }

        private void ExecuteDuplicateNoteCommand()
        {
            if (CurrentNote == null)
                return;

            if (_noteService.DuplicateNote(CurrentNote.Id) == true)
                SendNoteChangedEvent();
        }

        private async void ExecutePermanentlyDeleteNoteCommand()
        {
            if (CurrentNote == null)
                return;

            ContentDialog dialog = new ContentDialog
            {
                Title = "Permanently delete note",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Close,
                Content = "Are you sure you want to delete this note?"
                            + Environment.NewLine
                            + Environment.NewLine
                            + "This action cannot be undone."
            };

            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (_noteService.DeleteNote(CurrentNote.Id) == true)
                    SendNoteChangedEvent();
            }
        }

        private void ExecuteToggleSpellCheckCommand()
        {
            if (CurrentNote == null)
                return;

            // enable/disable spellcheck
            IsSpellCheckEnabled = (IsSpellCheckEnabled == false);
        }

        private void SendNoteChangedEvent()
        {
            _eventAggregator.GetEvent<NoteChangedEvent>().Publish(String.Empty);

            // note changed start autoSaveNoteTimer
            _autoSaveNoteTimer.Start();
        }

        private void AutoSaveNoteHandler(object sender, EventArgs e)
        {
            if (CurrentNote == null)
                return;

            if (CurrentNote.IsChanged == true)
            {
                _noteService.SaveNotes();

                // note saved stop autoSaveNoteTimer
                _autoSaveNoteTimer.Stop();
            }
        }

        private void SearchNoteResultsHandler(object sender, EventArgs e)
        {
            if (CurrentNote != null)
            {
                NoteEditor noteEditor = CurrentNote.Document.Parent as NoteEditor;

                noteEditor.SearchTerm = String.Empty; // clear the property first so we can trigger DependencyPropertyChangedEvent on dependency property
                if (_searchService.IsSearchInProgress() == true)
                    noteEditor.SearchTerm = _searchService.SearchTerm;
            }

            _searchNoteResultsTimer.Stop(); // search complete
        }

        #region Navigation Logic

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoadNote(navigationContext.Parameters.GetValue<Guid>("id"));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        #endregion
    }
}
