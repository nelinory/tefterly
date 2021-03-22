using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace Tefterly.Modules.Notes.ViewModels
{
    public class NotesListViewModel : BindableBase
    {
        private ObservableCollection<Core.Models.Note> _notesList;
        public ObservableCollection<Core.Models.Note> NotesList
        {
            get { return _notesList; }
            set { SetProperty(ref _notesList, value); }
        }

        public NotesListViewModel()
        {
            NotesList = new ObservableCollection<Core.Models.Note>();
            _notesList.Add(new Core.Models.Note()
            {
                Id = Guid.NewGuid(),
                Title = "Test Note #1",
                UpdatedDateTime = DateTime.Now,
                Content = "Mauris felis est, fermentum vel mauris vel, ultricies viverra nisl. Etiam lacus dolor, consequat in venenatis quis, gravida porta est",
                IsStarred = false,
                IsArchived = false,
                IsDeleted = false
            });
            _notesList.Add(new Core.Models.Note()
            {
                Id = Guid.NewGuid(),
                Title = "Test Note #2",
                UpdatedDateTime = DateTime.Now.AddDays(-1),
                Content = "Sed semper justo nec mi aliquam, et vehicula nunc pellentesque.",
                IsStarred = true,
                IsArchived = false,
                IsDeleted = false
            });
            _notesList.Add(new Core.Models.Note()
            {
                Id = Guid.NewGuid(),
                Title = "Long Test Note #3",
                UpdatedDateTime = DateTime.Parse("01/11/2021 10:15:43"),
                Content = "Phasellus pharetra fermentum mi, at fringilla dolor sollicitudin et. Sed dapibus ex ac ligula semper, sit amet dignissim sem vulputate.",
                IsStarred = false,
                IsArchived = false,
                IsDeleted = false
            });
            _notesList.Add(new Core.Models.Note()
            {
                Id = Guid.NewGuid(),
                Title = "Test Note #4",
                UpdatedDateTime = DateTime.Parse("03/01/2020 17:05:43"),
                Content = "Aenean aliquam rutrum purus, dapibus facilisis diam porta ornare. Vestibulum egestas augue nec nulla congue, non ultrices enim consequat.",
                IsStarred = false,
                IsArchived = false,
                IsDeleted = false
            });
        }
    }
}
