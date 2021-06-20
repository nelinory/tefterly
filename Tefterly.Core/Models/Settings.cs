using System;
using System.IO;

namespace Tefterly.Core.Models
{
    public class Settings
    {
        public int LatestVersion { get { return 1; } }

        public int CurrentVersion { get; set; } = 1;
        public string NotesLocation { get; set; }
        public string NotesFileLocation { get; set; }

        public Search Search { get; set; }
        public Notes Notes { get; set; }

        public Settings()
        {
            CurrentVersion = LatestVersion;

            // default values
            NotesLocation = Path.Combine(Environment.CurrentDirectory, "Notes");
            NotesFileLocation = Path.Combine(NotesLocation, "Tefterly.notes");

            // sections
            Search = new Search();
            Notes = new Notes();
        }
    }

    public class Search
    {
        public string ResultsHighlightColor { get; set; } = "#FFFFA500";
        public double ResultsHighlightColorOpacity { get; set; } = 0.5;
        public int ResultsRefreshTimerIntervalMs { get; set; } = 10;
        public int TermMinimumLength { get; set; } = 2;
    }

    public class Notes
    {
        public double FontSize { get; set; } = 14;
        public string FontFamily { get; set; } = "Segoe UI";
        public int AutoSaveTimerIntervalSeconds { get; set; } = 7;
        public bool IsSpellCheckEnabled { get; set; } = false;
        public string HyperlinkForegroundColor { get; set; } = "#FF0173C7";
    }
}
