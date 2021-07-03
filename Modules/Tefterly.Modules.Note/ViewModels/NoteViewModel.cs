using ModernWpf.Controls;
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
        private Core.Models.Note _currentNote;
        public Core.Models.Note CurrentNote
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
        private readonly DispatcherTimer _notesBackupTimer;

        // services
        private readonly INoteService _noteService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISearchService _searchService;
        private readonly ISettingsService _settingsService;

        // commands
        public DelegateCommand MarkNoteAsStarredCommand { get; set; }
        public DelegateCommand DuplicateNoteCommand { get; set; }
        public DelegateCommand MarkNoteAsArchivedCommand { get; set; }
        public DelegateCommand DeleteNoteCommand { get; set; }
        public DelegateCommand PermanentlyDeleteNoteCommand { get; set; }
        public DelegateCommand ToggleSpellCheckCommand { get; set; }
        public DelegateCommand PrintNoteCommand { get; set; }

        public NoteViewModel(INoteService noteService, IEventAggregator eventAggregator, ISearchService searchService, ISettingsService settingsService)
        {
            // attach all required services
            _noteService = noteService;
            _eventAggregator = eventAggregator;
            _searchService = searchService;
            _settingsService = settingsService;

            // attach all commands
            MarkNoteAsStarredCommand = new DelegateCommand(() => ExecuteChangeNotebookCategory(NotebookCategories.Starred));
            DuplicateNoteCommand = new DelegateCommand(() => ExecuteDuplicateNoteCommand());
            MarkNoteAsArchivedCommand = new DelegateCommand(() => ExecuteChangeNotebookCategory(NotebookCategories.Archived));
            DeleteNoteCommand = new DelegateCommand(() => ExecuteChangeNotebookCategory(NotebookCategories.Deleted));
            PermanentlyDeleteNoteCommand = new DelegateCommand(() => ExecutePermanentlyDeleteNoteCommand());
            ToggleSpellCheckCommand = new DelegateCommand(() => ExecuteToggleSpellCheckCommand());
            PrintNoteCommand = new DelegateCommand(() => ExecutePrintNoteCommand());

            // event handlers
            _searchService.Search += (sender, e) => _searchNoteResultsTimer.Start(); // search started

            // subscribe to important events
            _eventAggregator.GetEvent<ThemeChangedEvent>().Subscribe(x => { SearchNoteResultsHandler(null, null); });

            // autosave
            _autoSaveNoteTimer = new DispatcherTimer();
            _autoSaveNoteTimer.Interval = TimeSpan.FromSeconds(_settingsService.Settings.Notes.AutoSaveTimerIntervalSeconds);
            _autoSaveNoteTimer.Tick += AutoSaveNoteHandler;

            // search note
            _searchNoteResultsTimer = new DispatcherTimer();
            _searchNoteResultsTimer.Interval = TimeSpan.FromMilliseconds(_settingsService.Settings.Search.ResultsRefreshTimerIntervalMs);
            _searchNoteResultsTimer.Tick += SearchNoteResultsHandler;

            // notes backup
            _notesBackupTimer = new DispatcherTimer();
            _notesBackupTimer.Interval = TimeSpan.FromMinutes(_settingsService.Settings.Backup.TimerIntervalMinutes);
            _notesBackupTimer.Tick += NotesBackupHandler;
            if (_settingsService.Settings.Backup.IsEnabled == true)
                _notesBackupTimer.Start();

            IsSpellCheckEnabled = _settingsService.Settings.Notes.IsSpellCheckEnabled;
        }

        private void LoadNote(Guid noteId)
        {
            CurrentNote = _noteService.GetNote(noteId);

            if (CurrentNote != null)
            {
                CurrentNote.TrackChanges = true;
                CurrentNote.ModelChanged += (sender, e) => _autoSaveNoteTimer.Start();
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

            NoteHasChanged();
        }

        private void ExecuteDuplicateNoteCommand()
        {
            if (CurrentNote == null)
                return;

            if (_noteService.DuplicateNote(CurrentNote.Id) == true)
                NoteHasChanged();
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
                    NoteHasChanged();
            }
        }

        private void ExecuteToggleSpellCheckCommand()
        {
            if (CurrentNote == null)
                return;

            // enable/disable spellcheck
            IsSpellCheckEnabled = (IsSpellCheckEnabled == false);
            _settingsService.Settings.Notes.IsSpellCheckEnabled = IsSpellCheckEnabled;
        }

        private void ExecutePrintNoteCommand()
        {
            if (CurrentNote == null)
                return;

            PrintManager.PrintNote(CurrentNote.Title, CurrentNote.Document);
        }

        private void NoteHasChanged()
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

        private void NotesBackupHandler(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay} - [Action] Regular notes backup initiated");

            BackupManager.RegularBackup(_settingsService.Settings.BackupLocation, _settingsService.Settings.NotesLocation, _settingsService.Settings.Backup.MaxRegularBackups);
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
