using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
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

        // commands
        public DelegateCommand AddNoteCommand { get; set; }

        public NotesViewModel(INoteService noteService, IApplicationCommands applicationCommands, IEventAggregator eventAggregator)
        {
            // attach all required services
            _noteService = noteService;
            _eventAggregator = eventAggregator;

            // attach all commands
            AddNoteCommand = new DelegateCommand(() => ExecuteAddNoteCommand());

            // attach all composite commands
            _applicationCommands = applicationCommands;

            // subscribe to important events
            _eventAggregator.GetEvent<NoteChangedEvent>().Subscribe(x => { LoadNoteList(SelectedNotebookCategoryId, isNavigationAction: false); });
        }

        private void LoadNoteList(Guid notebookGategory, bool isNavigationAction)
        {
            List<Business.Models.Note> notes = new List<Business.Models.Note>(_noteService.GetNotes(notebookGategory));

            if (notes.Count > 0)
            {
                int selectedNoteIndex = notes.IndexOf(SelectedNote);
                if (SelectedNote != null && selectedNoteIndex > -1 && isNavigationAction == false)
                    SelectedNote = notes[selectedNoteIndex]; // keep existing selection if note state changed
                else
                    SelectedNote = notes[0]; // select the first item
            }
            else
                SelectedNote = null; // no notes found in the selected category

            NoteList = new ObservableCollection<Business.Models.Note>(notes); // bind it to the live NoteList
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

        private void ExecuteAddNoteCommand()
        {
            // add some simple text for content
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add("Please, type your note content here...");

            Business.Models.Note newNote = new Business.Models.Note()
            {
                Id = Guid.NewGuid(),
                CreatedDateTime = DateTime.Now,
                UpdatedDateTime = DateTime.Now,
                Title = String.Format("New Note - {0:F}", DateTime.Now),
                Document = new FlowDocument(paragraph),
                NotebookCategory = NotebookCategories.Default
            };

            if (_noteService.AddNote(newNote) == true)
                _eventAggregator.GetEvent<NoteChangedEvent>().Publish(String.Empty);
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
