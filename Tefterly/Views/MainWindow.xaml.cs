using System;
using System.Windows;
using System.Windows.Interop;
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
            Loaded += OnLoaded;

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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;

            HwndSource handleSource = HwndSource.FromHwnd(windowHandle);
            handleSource.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr handle, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            uint WM_SYSCOMMAND = 0x112;
            uint SC_KEYMENU = 0xf100;

            // do not allow the system menu to show up
            if ((message == WM_SYSCOMMAND) && (wParam.ToInt32() == SC_KEYMENU))
                handled = true;

            return IntPtr.Zero;
        }
    }
}