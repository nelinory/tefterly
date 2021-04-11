using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using Tefterly.Business;
using Tefterly.Core;
using Tefterly.Core.Commands;
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

                // signal selected note changed
                if (_selectedNote != null)
                    ExecuteNavigation(_selectedNote.Id);
            }
        }

        // services
        private readonly INoteService _noteService;
        private readonly IApplicationCommands _applicationCommands;

        public NotesViewModel(INoteService noteService, IApplicationCommands applicationCommands)
        {
            // attach all required services
            _noteService = noteService;

            // attach all composite commands
            _applicationCommands = applicationCommands;

            LoadNoteList(NotebookCategories.Default);
        }

        private void LoadNoteList(Guid notebookGategory)
        {
            NoteList = new ObservableCollection<Business.Models.Note>(_noteService.GetNotes(notebookGategory));
            if (NoteList.Count > 0)
                SelectedNote = NoteList[0]; // select the first item
        }

        private void ExecuteNavigation(Guid id)
        {
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
            LoadNoteList(navigationContext.Parameters.GetValue<Guid>("id"));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        #endregion
    }
}
