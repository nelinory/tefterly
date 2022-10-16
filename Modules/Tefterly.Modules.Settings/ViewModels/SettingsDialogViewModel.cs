using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Tefterly.Services;

namespace Tefterly.Modules.Settings.ViewModels
{
    public class SettingsDialogViewModel : BindableBase
    {
        #region Drop-downs Data

        public List<int> SearchTermMinimumLengthItemSource { get; } = new List<int>() { 2, 3, 4, 5 };

        public List<Tuple<string, Brush>> SearchResultsHighlightColorsItemSource { get; } = new List<Tuple<string, Brush>>()
        {
            new Tuple<string, Brush>("Default Orange", (SolidColorBrush) new BrushConverter().ConvertFromString("#FFFFA500")),
            new Tuple<string, Brush>("Green", (SolidColorBrush) new BrushConverter().ConvertFromString("#FF10893E")),
            new Tuple<string, Brush>("Red", (SolidColorBrush) new BrushConverter().ConvertFromString("#FFE81123")),
            new Tuple<string, Brush>("Gold", (SolidColorBrush) new BrushConverter().ConvertFromString("#FFFF8C00")),
            new Tuple<string, Brush>("Purple", (SolidColorBrush) new BrushConverter().ConvertFromString("#FF6B69D6")),
            new Tuple<string, Brush>("Plum", (SolidColorBrush) new BrushConverter().ConvertFromString("#FFBF0077")),
            new Tuple<string, Brush>("Camouflage", (SolidColorBrush) new BrushConverter().ConvertFromString("#FF847545"))
        };

        public List<double> SearchResultsHighlightColorOpacityItemSource { get; } = new List<double>() { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9 };

        public List<Tuple<string, FontFamily>> FontFamily { get; } = new List<Tuple<string, FontFamily>>()
        {
            new Tuple<string, FontFamily>("Agency FB", new FontFamily("Agency FB")),
            new Tuple<string, FontFamily>("Arial", new FontFamily("Arial")),
            new Tuple<string, FontFamily>("Bell MT", new FontFamily("Bell MT")),
            new Tuple<string, FontFamily>("Calibri", new FontFamily("Calibri")),
            new Tuple<string, FontFamily>("Comic Sans MS", new FontFamily("Comic Sans MS")),
            new Tuple<string, FontFamily>("Courier New", new FontFamily("Courier New")),
            new Tuple<string, FontFamily>("Segoe UI", new FontFamily("Segoe UI")),
            new Tuple<string, FontFamily>("Tahoma", new FontFamily("Tahoma"))
        };

        public List<double> FontSizes { get; } = new List<double>()
        {
            8,
            9,
            10,
            11,
            12,
            14,
            16,
            18,
            20,
            24,
            28,
            36,
            48,
            72
        };

        public List<Tuple<string, Brush>> HyperlinkColors { get; } = new List<Tuple<string, Brush>>()
        {
            new Tuple<string, Brush>("Default Blue", (SolidColorBrush) new BrushConverter().ConvertFromString("#0173C7")),
            new Tuple<string, Brush>("Green", (SolidColorBrush) new BrushConverter().ConvertFromString("#10893E")),
            new Tuple<string, Brush>("Red", (SolidColorBrush) new BrushConverter().ConvertFromString("#E81123")),
            new Tuple<string, Brush>("Gold", (SolidColorBrush) new BrushConverter().ConvertFromString("#FF8C00")),
            new Tuple<string, Brush>("Purple", (SolidColorBrush) new BrushConverter().ConvertFromString("#6B69D6")),
            new Tuple<string, Brush>("Plum", (SolidColorBrush) new BrushConverter().ConvertFromString("#BF0077")),
            new Tuple<string, Brush>("Camouflage", (SolidColorBrush) new BrushConverter().ConvertFromString("#847545"))
        };

        #endregion

        #region Main Settings

        public string NotesLocation
        {
            get { return _settingsService.Settings.NotesLocation; }
        }

        public string BackupLocation
        {
            get { return _settingsService.Settings.BackupLocation; }
        }

        public string NotesFileLocation
        {
            get { return _settingsService.Settings.NotesFileLocation; }
        }

        #endregion

        #region General Settings

        public bool RememberLastUsedCategorySelectedItem
        {
            get { return _settingsService.Settings.General.RememberLastUsedCategory; }
            set { _settingsService.Settings.General.RememberLastUsedCategory = value; }
        }

        public bool RememberAppWindowPlacementSelectedItem
        {
            get { return _settingsService.Settings.General.RememberAppWindowPlacement; }
            set { _settingsService.Settings.General.RememberAppWindowPlacement = value; }
        }

        public int SearchTermMinimumLengthSelectedItem
        {
            get { return _settingsService.Settings.Search.TermMinimumLength; }
            set { _settingsService.Settings.Search.TermMinimumLength = value; }
        }

        public Tuple<string, Brush> SearchResultsHighlightColorSelectedItem
        {
            get
            {
                Tuple<string, Brush> brush = SearchResultsHighlightColorsItemSource.Where(m => m.Item2.ToString() == _settingsService.Settings.Search.ResultsHighlightColor).FirstOrDefault();
                return brush ?? SearchResultsHighlightColorsItemSource[0];
            }
            set { _settingsService.Settings.Search.ResultsHighlightColor = value.Item2.ToString(); }
        }

        public double SearchResultsHighlightColorOpacitySelectedItem
        {
            get { return _settingsService.Settings.Search.ResultsHighlightColorOpacity; }
            set { _settingsService.Settings.Search.ResultsHighlightColorOpacity = value; }
        }

        #endregion

        // services
        private readonly ISettingsService _settingsService;

        public SettingsDialogViewModel(ISettingsService settingsService)
        {
            // attach all required services
            _settingsService = settingsService;
        }
    }
}