using ModernWpf.Controls;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Windows.Documents;
using Tefterly.Core;
using Tefterly.Core.Events;
using Tefterly.Services;

namespace Tefterly.Modules.Note.ViewModels
{
    public class NoteViewModel : BindableBase, INavigationAware
    {
        private string _noteTitle;
        public string NoteTitle
        {
            get { return _noteTitle; }
            set { SetProperty(ref _noteTitle, value); }
        }

        private FlowDocument _noteContent;
        public FlowDocument NoteContent
        {
            get { return _noteContent; }
            set { SetProperty(ref _noteContent, value); }
        }

        private bool _showNoteComponents;
        public bool ShowNoteComponents
        {
            get { return _showNoteComponents; }
            set { SetProperty(ref _showNoteComponents, value); }
        }

        private Business.Models.Note _currentNote;
        public Business.Models.Note CurrentNote
        {
            get { return _currentNote; }
            set { SetProperty(ref _currentNote, value); }
        }

        // services
        private readonly INoteService _noteService;
        private readonly IEventAggregator _eventAggregator;

        // commands
        public DelegateCommand MarkNoteAsStarredCommand { get; set; }
        public DelegateCommand DuplicateNoteCommand { get; set; }
        public DelegateCommand MarkNoteAsArchivedCommand { get; set; }
        public DelegateCommand DeleteNoteCommand { get; set; }
        public DelegateCommand PermanentlyDeleteNoteCommand { get; set; }

        public NoteViewModel(INoteService noteService, IEventAggregator eventAggregator)
        {
            // attach all required services
            _noteService = noteService;
            _eventAggregator = eventAggregator;

            // attach all commands
            MarkNoteAsStarredCommand = new DelegateCommand(() => ExecuteChangeNotebookCategory(NotebookCategories.Starred));
            DuplicateNoteCommand = new DelegateCommand(() => ExecuteDuplicateNoteCommand());
            MarkNoteAsArchivedCommand = new DelegateCommand(() => ExecuteChangeNotebookCategory(NotebookCategories.Archived));
            DeleteNoteCommand = new DelegateCommand(() => ExecuteChangeNotebookCategory(NotebookCategories.Deleted));
            PermanentlyDeleteNoteCommand = new DelegateCommand(() => ExecutePermanentlyDeleteNoteCommand());
        }

        private void LoadNote(Guid noteId)
        {
            CurrentNote = _noteService.GetNote(noteId);

            if (CurrentNote != null)
            {
                NoteTitle = CurrentNote.Title;

                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(CurrentNote.Content);
                FlowDocument tempNoteContent = new FlowDocument(paragraph);

                NoteContent = tempNoteContent;

                ShowNoteComponents = true;
            }
            else
                ShowNoteComponents = false;
        }

        private void ExecuteChangeNotebookCategory(Guid notebookCategory)
        {
            if (CurrentNote == null || notebookCategory == null)
                return;

            if (CurrentNote.NotebookCategory == notebookCategory)
                CurrentNote.NotebookCategory = NotebookCategories.Default;
            else
                CurrentNote.NotebookCategory = notebookCategory;

            if (_noteService.UpdateNotebookCategory(CurrentNote.Id, CurrentNote.NotebookCategory) == true)
                _eventAggregator.GetEvent<NoteChangedEvent>().Publish(String.Empty);
        }

        private void ExecuteDuplicateNoteCommand()
        {
            if (CurrentNote == null)
                return;

            if (_noteService.DuplicateNote(CurrentNote.Id) == true)
                _eventAggregator.GetEvent<NoteChangedEvent>().Publish(String.Empty);
        }

        private async void ExecutePermanentlyDeleteNoteCommand()
        {
            if (CurrentNote == null)
                return;

            ContentDialog dialog = new ContentDialog
            {
                Title="Permanently delete note",
                PrimaryButtonText="Yes", 
                CloseButtonText ="No",
                DefaultButton = ContentDialogButton.Close,
                Content="Are you sure you want to delete this note?"
                            + Environment.NewLine
                            + Environment.NewLine
                            + "This action cannot be undone."
            };

            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (_noteService.DeleteNote(CurrentNote.Id) == true)
                    _eventAggregator.GetEvent<NoteChangedEvent>().Publish(String.Empty);
            }
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
