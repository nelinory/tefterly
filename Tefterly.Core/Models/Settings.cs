using System;
using System.IO;

namespace Tefterly.Core.Models
{
    public class Settings
    {
        public int LatestVersion { get { return 1; } }

        public int CurrentVersion { get; set; }
        public string NotesLocation { get; set; }
        public string NotesFileLocation { get; set; }

        public Search Search { get; set; }

        public Settings()
        {
            CurrentVersion = LatestVersion;

            // default values
            NotesLocation = Path.Combine(Environment.CurrentDirectory, "Notes");
            NotesFileLocation = Path.Combine(NotesLocation, "Tefterly.notes");

            // search
            Search = new Search
            {
                ResultsHighlightColor = "#FF0173C7",
                ResultsRefreshTimerIntervalMs = 10,
                TermMinimumLength = 2
            };
        }
    }

    public class Search
    {
        public string ResultsHighlightColor { get; set; }
        public int ResultsRefreshTimerIntervalMs { get; set; }
        public int TermMinimumLength { get; set; }
    }
}
