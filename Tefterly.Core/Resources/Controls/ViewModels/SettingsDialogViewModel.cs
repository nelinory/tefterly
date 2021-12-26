﻿using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace Tefterly.Core.Resources.Controls.ViewModels
{
    public class SettingsDialogViewModel : BindableBase
    {
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

        public List<Tuple<string, Brush>> SearchResultsHighlightColors { get; } = new List<Tuple<string, Brush>>()
        {
            new Tuple<string, Brush>("Default Orange", (SolidColorBrush) new BrushConverter().ConvertFromString("#FFA500")),
            new Tuple<string, Brush>("Green", (SolidColorBrush) new BrushConverter().ConvertFromString("#10893E")),
            new Tuple<string, Brush>("Red", (SolidColorBrush) new BrushConverter().ConvertFromString("#E81123")),
            new Tuple<string, Brush>("Gold", (SolidColorBrush) new BrushConverter().ConvertFromString("#FF8C00")),
            new Tuple<string, Brush>("Purple", (SolidColorBrush) new BrushConverter().ConvertFromString("#6B69D6")),
            new Tuple<string, Brush>("Plum", (SolidColorBrush) new BrushConverter().ConvertFromString("#BF0077")),
            new Tuple<string, Brush>("Camouflage", (SolidColorBrush) new BrushConverter().ConvertFromString("#847545"))
        };

        private string _notesLocation;
        public string NotesLocation
        {
            get { return _notesLocation; }
            set { SetProperty(ref _notesLocation, value); }
        }

        private string _backupLocation;
        public string BackupLocation
        {
            get { return _backupLocation; }
            set { SetProperty(ref _backupLocation, value); }
        }

        private string _notesFileLocation;
        public string NotesFileLocation
        {
            get { return _notesFileLocation; }
            set { SetProperty(ref _notesFileLocation, value); }
        }

        public SettingsDialogViewModel()
        {
            NotesLocation = Path.Combine(Environment.CurrentDirectory, "Notes");
            BackupLocation = Path.Combine(Environment.CurrentDirectory, "Backup");
            NotesFileLocation = Path.Combine(NotesLocation, "Tefterly.notes");
        }
    }
}
