using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Documents;
using Tefterly.Core;
using Tefterly.Core.Models;

namespace Tefterly.Services
{
    public class NoteService : INoteService
    {
        private static IList<Notebook> _notebooks = new List<Notebook>();
        private static IList<Note> _notes = new List<Note>();
       
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        // services
        private readonly ISettingsService _settingsService;

        public NoteService(ISettingsService settingsService)
        {
            // attach all required services
            _settingsService = settingsService;

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
            {
                categoryCount = _notes.Where(p => p.NotebookCategory == category).Count();

                if (category == NotebookCategories.Default) // summary of default + starred categories
                    categoryCount += _notes.Where(p => p.NotebookCategory == NotebookCategories.Starred).Count();
            }

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

        public Note GetNote(Guid noteId)
        {
            return _notes.Where(p => p.Id == noteId).FirstOrDefault<Note>();
        }

        public bool DuplicateNote(Guid noteId)
        {
            bool success = false;
            Note targetNote = _notes.Where(p => p.Id == noteId).FirstOrDefault();

            if (targetNote != null)
            {
                Note newNote = CreateNewNote();

                newNote.Title = "Duplicate - " + targetNote.Title;
                newNote.Document = targetNote.Document;
                newNote.NotebookCategory = targetNote.NotebookCategory;

                _notes.Add(newNote);

                success = true;
            }

            return success;
        }

        public bool DeleteNote(Guid noteId)
        {
            bool success = false;
            Note targetNote = _notes.Where(p => p.Id == noteId).FirstOrDefault();

            if (targetNote != null)
            {
                _notes.Remove(targetNote);

                DeleteNoteXaml(targetNote);

                SaveNoteCatalog();

                success = true;
            }

            return success;
        }

        public bool AddNote()
        {
            bool success = false;
            Note newNote = CreateNewNote();

            if (newNote != null)
            {
                newNote.Title = String.Format("New Note - {0:F}", DateTime.Now);
                newNote.Document = new FlowDocument();
                newNote.NotebookCategory = NotebookCategories.Default;

                _notes.Add(newNote);

                success = true;
            }

            return success;
        }

        public void SaveNotes()
        {
            bool changesSaved = false;

            try
            {
                foreach (Note note in _notes)
                {
                    if (note.IsChanged == true)
                    {
                        SaveNoteXaml(note);

                        System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay} - [Action] '{note.Title}' saved");

                        changesSaved = true;
                    }
                }

                if (changesSaved == true)
                    SaveNoteCatalog();
            }
            catch (Exception ex)
            {
                Log.Error("Error while saving notes: {EX}", ex);
            }
        }

        #region Private Methods

        private void LoadNotebooks()
        {
            _notebooks = new List<Notebook>
            {
                new Notebook() { Id = NotebookCategories.Default, Title = "Notes", IconFont = "\xE8F1", IsSystem = true },
                new Notebook() { Id = NotebookCategories.Starred, Title = "Starred", IconFont = "\xE734", IsSystem = true },
                new Notebook() { Id = NotebookCategories.Archived, Title = "Archived", IconFont = "\xE7B8", IsSystem = true },
                new Notebook() { Id = NotebookCategories.Deleted, Title = "Deleted", IconFont = "\xE74D", IsSystem = true }
            };
        }

        private void LoadNotes()
        {
            if (File.Exists(_settingsService.Settings.NotesFileLocation) == true)
            {
                _notes = JsonSerializer.Deserialize<IList<Note>>(File.ReadAllText(_settingsService.Settings.NotesFileLocation));

                foreach (Note note in _notes)
                {
                    note.Document = LoadNoteXaml(note.Id);

                    Utilities.FormatFlowDocument(note.Document);
                }
            }
            else
            {
                var demoNoteText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean sit amet tincidunt magna, ac lacinia mauris. Phasellus et mi eget orci pellentesque vehicula. Suspendisse scelerisque efficitur lectus et sodales. Cras nibh diam, varius quis odio in, varius dictum orci. Cras condimentum tellus magna, maximus volutpat nunc egestas in. Quisque id elementum dolor. Integer suscipit magna dolor, quis placerat quam blandit ac. Nam venenatis sem in lorem tincidunt dictum. Maecenas ac pharetra enim. Ut mollis eros ut neque luctus molestie. Nam tempor ipsum velit, ac lobortis dolor fringilla in. Vivamus et auctor risus. Pellentesque lobortis leo quis convallis dignissim. In enim justo, aliquet vitae consectetur aliquam, ullamcorper eget nisi.";

                _notes = new List<Note>
                {
                    // note #1 - new one
                    new Note()
                    {
                        TrackChanges = true,
                        Id = Guid.NewGuid(),
                        Title = "Demo Note #1",
                        Document = Utilities.GetFlowDocumentFromText("This is demo note #1 - just made. " + demoNoteText),
                        NotebookCategory = NotebookCategories.Default,
                        CreatedDateTime = DateTime.Now,
                        UpdatedDateTime = DateTime.Now
                    },
                    // note #2 - 5 days old, starred
                    new Note()
                    {
                        TrackChanges = true,
                        Id = Guid.NewGuid(),
                        Title = "Demo Note #2 With A Long Title",
                        Document = Utilities.GetFlowDocumentFromText("This is demo note #2 - 5 days old, starred. " + demoNoteText),
                        NotebookCategory = NotebookCategories.Starred,
                        CreatedDateTime = DateTime.Now.AddDays(-5),
                        UpdatedDateTime = DateTime.Now.AddDays(-5)
                    },
                    // note #3 - 15 days old
                    new Note()
                    {
                        TrackChanges = true,
                        Id = Guid.NewGuid(),
                        Title = "Demo Note #3",
                        Document = Utilities.GetFlowDocumentFromText("This is demo note #3 - 15 days old. " + demoNoteText),
                        NotebookCategory = NotebookCategories.Default,
                        CreatedDateTime = DateTime.Now.AddDays(-15),
                        UpdatedDateTime = DateTime.Now.AddDays(-15)
                    },
                    // note #4 - 20 days old, archived
                    new Note()
                    {
                        TrackChanges = true,
                        Id = Guid.NewGuid(),
                        Title = "Demo Note #4",
                        Document = Utilities.GetFlowDocumentFromText("This is demo note #4 - 20 days old - archived. " + demoNoteText),
                        NotebookCategory = NotebookCategories.Default,
                        CreatedDateTime = DateTime.Now.AddDays(-20),
                        UpdatedDateTime = DateTime.Now.AddDays(-20)
                    },
                    // note #5 - over an year old
                    new Note()
                    {
                        TrackChanges = true,
                        Id = Guid.NewGuid(),
                        Title = "Demo Note #5",
                        Document = Utilities.GetFlowDocumentFromText("This is demo note #5 - over an year old. " + demoNoteText),
                        NotebookCategory = NotebookCategories.Default,
                        CreatedDateTime = DateTime.Now.AddMonths(-15),
                        UpdatedDateTime = DateTime.Now.AddMonths(-15)
                    }
                };
            }
        }

        private void SaveNoteXaml(Note note)
        {
            Utilities.EnsureTargetFolderExists(_settingsService.Settings.NotesFileLocation);

            string noteFileName = GetNoteFileName(note.Id, _settingsService.Settings.NotesLocation);
            TextRange textRange = new TextRange(note.Document.ContentStart, note.Document.ContentEnd);

            using (FileStream fileStream = new FileStream(noteFileName, FileMode.Create))
            {
                textRange.Save(fileStream, System.Windows.DataFormats.XamlPackage);
            }
        
            note.AcceptChanges(); // marked as saved
        }

        private FlowDocument LoadNoteXaml(Guid noteId)
        {
            FlowDocument document = new FlowDocument();
            string noteFileName = Path.Combine(_settingsService.Settings.NotesLocation, noteId + ".xaml");

            if (File.Exists(noteFileName) == true)
            {
                TextRange textRange = new TextRange(document.ContentStart, document.ContentEnd);
                using (FileStream fileStream = new FileStream(noteFileName, FileMode.Open))
                {
                    textRange.Load(fileStream, System.Windows.DataFormats.XamlPackage);
                }
            }
            else
                document = Utilities.GetFlowDocumentFromText($"Xaml note file with id:{noteId} not found !");

            return document;
        }

        private void DeleteNoteXaml(Note note)
        {
            string noteFileName = GetNoteFileName(note.Id, _settingsService.Settings.NotesLocation);

            if (File.Exists(noteFileName) == true)
                File.Delete(noteFileName);
        }

        private void SaveNoteCatalog()
        {
            Utilities.EnsureTargetFolderExists(_settingsService.Settings.NotesFileLocation);

            string jsonNotes = JsonSerializer.Serialize(_notes, _jsonSerializerOptions);

            File.WriteAllText(_settingsService.Settings.NotesFileLocation, jsonNotes);
        }

        private string GetNoteFileName(Guid noteId, string filePath)
        {
            return Path.Combine(filePath, noteId + ".xaml");
        }

        private Note CreateNewNote()
        {
            Note note = new Note
            {
                TrackChanges = true,

                // load default fields
                Id = Guid.NewGuid(),
                CreatedDateTime = DateTime.Now,
                UpdatedDateTime = DateTime.Now
            };

            return note;
        }

        #endregion
    }
}
