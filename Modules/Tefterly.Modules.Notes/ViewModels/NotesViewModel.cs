using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tefterly.Core;
using Tefterly.Core.Commands;
using Tefterly.Core.Events;
using Tefterly.Core.Navigation;
using Tefterly.Services;

namespace Tefterly.Modules.Notes.ViewModels
{
    public class NotesViewModel : BindableBase, INavigationAware
    {
        private ObservableCollection<Core.Models.Note> _noteList;
        public ObservableCollection<Core.Models.Note> NoteList
        {
            get { return _noteList; }
            set { SetProperty(ref _noteList, value); }
        }

        private Core.Models.Note _selectedNote;
        public Core.Models.Note SelectedNote
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

        private string _searchTerm;
        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                SetProperty(ref _searchTerm, value);
                _searchService.ExecuteSearch(SearchTerm);
            }
        }

        private static Guid _selectedNoteFromSearch = Guid.Empty;

        // services
        private readonly INoteService _noteService;
        private readonly IApplicationCommands _applicationCommands;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISearchService _searchService;

        // commands
        public DelegateCommand AddNoteCommand { get; set; }
        public DelegateCommand ClearSearchTermCommand { get; set; }

        public NotesViewModel(INoteService noteService, IApplicationCommands applicationCommands, IEventAggregator eventAggregator, ISearchService searchService)
        {
            // attach all required services
            _noteService = noteService;
            _eventAggregator = eventAggregator;
            _searchService = searchService;

            // attach all commands
            AddNoteCommand = new DelegateCommand(() => ExecuteAddNoteCommand());
            ClearSearchTermCommand = new DelegateCommand(() => { SearchTerm = String.Empty; });

            // attach all composite commands
            _applicationCommands = applicationCommands;

            // subscribe to important events
            _eventAggregator.GetEvent<NoteChangedEvent>().Subscribe(x => { LoadNoteList(SelectedNotebookCategoryId); });

            // event handlers
            _searchService.Search += (sender, e) => LoadNoteList(SelectedNotebookCategoryId);
        }

        private void LoadNoteList(Guid notebookGategory)
        {
            List<Core.Models.Note> notes = new List<Core.Models.Note>(_noteService.GetNotes(notebookGategory).OrderByDescending(p => p.UpdatedDateTime));

            // apply search filter
            if (_searchService.IsSearchInProgress() == true)
                notes = notes.AsParallel().Where(p => p.Title.IndexOf(_searchService.SearchTerm, StringComparison.InvariantCultureIgnoreCase) != -1
                                                || p.Content.IndexOf(_searchService.SearchTerm, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();

            if (notes.Count > 0)
            {
                if (_selectedNoteFromSearch != Guid.Empty)
                    RestoreSelectedNoteOnSearch(notes);
                else
                    SelectedNote = notes[0]; // select the first available note

                StoreSelectedNoteOnSearch(SelectedNote.Id);
            }
            else
                SelectedNote = null; // no notes found in the selected category

            NoteList = new ObservableCollection<Core.Models.Note>(notes); // bind it to the live NoteList
            ShowNotesNotFoundPanel = (NoteList.Count == 0);

            if (String.IsNullOrEmpty(_searchService.SearchTerm) == true)
                ShowAddNoteButton = (SelectedNotebookCategoryId == NotebookCategories.Default);
            else
                ShowAddNoteButton = false;
        }

        private void ExecuteNavigation(Core.Models.Note selectedNote)
        {
            Guid id = (selectedNote == null) ? Guid.Empty : selectedNote.Id;

            NavigationItem navigationItem = new NavigationItem
            {
                NavigationPath = NavigationPaths.Note,
                NavigationRegion = RegionNames.NoteRegion,
                NavigationParameters = new NavigationParameters { { "id", id } }
            };

            StoreSelectedNoteOnSearch(id);

            _applicationCommands.NavigateCommand.Execute(navigationItem);
        }

        private void ExecuteAddNoteCommand()
        {
            if (_noteService.AddNote() == true)
                _eventAggregator.GetEvent<NoteChangedEvent>().Publish(String.Empty);

            if (SelectedNotebookCategoryId != NotebookCategories.Default)
                _eventAggregator.GetEvent<ResetNotebookCategoryEvent>().Publish(String.Empty);

            // save newly created note
            _noteService.SaveNotes();
        }

        private void StoreSelectedNoteOnSearch(Guid id)
        {
            // store the last selected note from the search results
            if (_searchService.IsSearchInProgress() == true)
                _selectedNoteFromSearch = id;
            else
                _selectedNoteFromSearch = Guid.Empty;
        }

        private void RestoreSelectedNoteOnSearch(List<Core.Models.Note> notes)
        {
            // restore the last selected note from the search results
            SelectedNote = notes.Where(m => m.Id == _selectedNoteFromSearch).FirstOrDefault();

            if (SelectedNote == null)
                SelectedNote = notes[0];
        }

        #region Navigation Logic

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            SelectedNotebookCategoryId = navigationContext.Parameters.GetValue<Guid>("id");

            LoadNoteList(SelectedNotebookCategoryId);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        #endregion
    }
}
