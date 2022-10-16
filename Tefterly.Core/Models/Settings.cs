using System;
using System.IO;
using System.Text.Json.Serialization;
using Tefterly.Core.Win32Api;

namespace Tefterly.Core.Models
{
    public class Settings
    {
        public int LatestVersion { get { return 1; } }
        public int CurrentVersion { get; set; } = 1;

        [JsonIgnore]
        public string NotesLocation { get { return Path.Combine(Environment.CurrentDirectory, "Notes"); } }
        [JsonIgnore]
        public string BackupLocation { get { return Path.Combine(Environment.CurrentDirectory, "Backup"); } }
        [JsonIgnore]
        public string NotesFileLocation { get { return Path.Combine(NotesLocation, "Tefterly.notes"); } }

        public Search Search { get; set; }
        public Notes Notes { get; set; }
        public Backup Backup { get; set; }
        public General General { get; set; }

        public Settings()
        {
            CurrentVersion = LatestVersion;

            // sections
            Search = new Search();
            Notes = new Notes();
            Backup = new Backup();
            General = new General();
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

    public class Backup
    {
        public bool IsEnabled { get; set; } = true;
        public int TimerIntervalMinutes { get; set; } = 30;
        public int MaxRegularBackups { get; set; } = 20;
        public int MaxVersionChangeBackups { get; set; } = 7;
    }

    public class General
    {
        public bool RememberLastUsedCategory { get; set; } = true;
        public Guid LastUsedCategory { get; set; } = NotebookCategories.Default;
        public bool RememberAppWindowPlacement { get; set; } = true;
        public WindowPlacement AppWindowPlacement { get; set; }
    }
}
