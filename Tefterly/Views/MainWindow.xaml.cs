using System.Windows;
using Tefterly.Services;

namespace Tefterly.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // services
        private INoteService _noteService;
        private ISettingsService _settingsService;

        public MainWindow(INoteService noteService, ISettingsService settingsService)
        {
            InitializeComponent();

            Closing += OnClosing;

            // attach all required services
            _noteService = noteService;
            _settingsService = settingsService;
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // save notes before exiting
            _noteService.SaveNotes();

            // save settings before exiting
            _settingsService.Save();
        }
    }
}