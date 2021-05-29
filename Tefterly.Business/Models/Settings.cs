using System;
using System.IO;

namespace Tefterly.Business.Models
{
    public class Settings
    {
        public string NotesLocation { get; set; }
        public string NotesFileLocation { get; set; }
        public string SettingsFileLocation { get; set; }

        public Settings()
        {
            // default values
            NotesLocation = Path.Combine(Environment.CurrentDirectory, "Notes");
            NotesFileLocation = Path.Combine(NotesLocation, "Tefterly.notes");
            SettingsFileLocation = Path.Combine(NotesLocation, "Tefterly.config");
        }
    }
}
