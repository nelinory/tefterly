﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tefterly.Business.Models;
using Tefterly.Core;

namespace Tefterly.Services
{
    public class NoteService : INoteService
    {
        private static IList<Notebook> _notebooks = new List<Notebook>();
        private static IList<Note> _notes = new List<Note>();

        public NoteService()
        {
            LoadNotebooks();
            LoadNotes();
        }

        public IList<Notebook> GetAllNotebookCategories()
        {
            return _notebooks;
        }

        public int GetCategoryCount(Guid category)
        {
            int categoryCount = 0;

            if (String.IsNullOrEmpty(category.ToString()) == false)
                categoryCount = _notes.Where(p => p.NotebookCategory == category).Count();

            return categoryCount;
        }

        public IList<Note> GetNotes(Guid category)
        {
            List<Note> notes = new List<Note>();

            if (String.IsNullOrEmpty(category.ToString()) == false)
            {
                // Default/All Notes -> not archived/deleted
                if (category == NotebookCategories.Default)
                    notes = _notes.Where(p => p.NotebookCategory == category || p.NotebookCategory == NotebookCategories.Starred).ToList<Note>();
                else
                    notes = _notes.Where(p => p.NotebookCategory == category).ToList<Note>();
            }

            return notes;
        }

        #region Private Methods

        private void LoadNotebooks()
        {
            _notebooks = new List<Notebook>
            {
                new Notebook() { Id = NotebookCategories.Default, Title = "Notes", IconFont = "\xE8F1", IsSystem = true },
                new Notebook() { Id = NotebookCategories.Starred, Title = "Starred", IconFont = "\xE734", IsSystem = true },
                new Notebook() { Id = NotebookCategories.Archived, Title = "Archived", IconFont = "\xF12B", IsSystem = true },
                new Notebook() { Id = NotebookCategories.Deleted, Title = "Deleted", IconFont = "\xE74D", IsSystem = true }
            };
        }

        private void LoadNotes()
        {
            // TODO: Load the real notes
            var demoNoteText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean sit amet tincidunt magna, ac lacinia mauris. Phasellus et mi eget orci pellentesque vehicula. Suspendisse scelerisque efficitur lectus et sodales. Cras nibh diam, varius quis odio in, varius dictum orci. Cras condimentum tellus magna, maximus volutpat nunc egestas in. Quisque id elementum dolor. Integer suscipit magna dolor, quis placerat quam blandit ac. Nam venenatis sem in lorem tincidunt dictum. Maecenas ac pharetra enim. Ut mollis eros ut neque luctus molestie. Nam tempor ipsum velit, ac lobortis dolor fringilla in. Vivamus et auctor risus. Pellentesque lobortis leo quis convallis dignissim. In enim justo, aliquet vitae consectetur aliquam, ullamcorper eget nisi.";

            _notes = new List<Note>
            {
                // note #1 - new one
                new Note()
                {
                    Id = Guid.NewGuid(),
                    CreatedDateTime = DateTime.Now,
                    UpdatedDateTime = DateTime.Now,
                    Title = "Demo Note #1",
                    Content = "This is demo note #1 - just made. " + demoNoteText,
                    Color = String.Empty,
                    NotebookCategory = NotebookCategories.Default
                },
                // note #2 - 5 days old, starred
                new Note()
                {
                    Id = Guid.NewGuid(),
                    CreatedDateTime = DateTime.Now.AddDays(-5),
                    UpdatedDateTime = DateTime.Now.AddDays(-5),
                    Title = "Demo Note #2 With A Long Title",
                    Content = "This is demo note #2 - 5 days old, starred. " + demoNoteText,
                    Color = String.Empty,
                    NotebookCategory = NotebookCategories.Starred
                },
                // note #3 - 15 days old
                new Note()
                {
                    Id = Guid.NewGuid(),
                    CreatedDateTime = DateTime.Now.AddDays(-15),
                    UpdatedDateTime = DateTime.Now.AddDays(-15),
                    Title = "Demo Note #3",
                    Content = "This is demo note #3 - 15 days old. " + demoNoteText,
                    Color = String.Empty,
                    NotebookCategory = NotebookCategories.Default
                },
                // note #4 - 20 days old, archived
                new Note()
                {
                    Id = Guid.NewGuid(),
                    CreatedDateTime = DateTime.Now.AddDays(-20),
                    UpdatedDateTime = DateTime.Now.AddDays(-20),
                    Title = "Demo Note #4",
                    Content = "This is demo note #4 - 20 days old - archived. " + demoNoteText,
                    Color = String.Empty,
                    NotebookCategory = NotebookCategories.Default
                },
                // note #5 - over an year old
                new Note()
                {
                    Id = Guid.NewGuid(),
                    CreatedDateTime = DateTime.Now.AddMonths(-15),
                    UpdatedDateTime = DateTime.Now.AddMonths(-15),
                    Title = "Demo Note #5",
                    Content = "This is demo note #5 - over an year old. " + demoNoteText,
                    Color = String.Empty,
                    NotebookCategory = NotebookCategories.Default
                }
            };
        }

        #endregion
    }
}
