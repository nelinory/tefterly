using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Windows.Documents;
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

        // services
        private readonly INoteService _noteService;

        public NoteViewModel(INoteService noteService)
        {
            // attach all required services
            _noteService = noteService;
        }

        private void LoadNote(Guid noteId)
        {
            Business.Models.Note note = _noteService.GetNote(noteId);
            
            NoteTitle = note.Title;

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(note.Content);
            FlowDocument tempNoteContent = new FlowDocument(paragraph);

            NoteContent = tempNoteContent;
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
