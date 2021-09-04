using ModernWpf;
using System;
using System.Windows;
using System.Windows.Interop;
using Tefterly.Core.Win32Api;
using Tefterly.Services;

namespace Tefterly.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // services
        private readonly INoteService _noteService;
        private readonly ISettingsService _settingsService;

        // main window handle
        private IntPtr _windowHandle;

        public MainWindow(INoteService noteService, ISettingsService settingsService)
        {
            InitializeComponent();

            Closing += OnClosing;
            Loaded += OnLoaded;

            // attach all required services
            _noteService = noteService;
            _settingsService = settingsService;

            // TODO: Read the app theme from settings
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // restore the window position
            if (_settingsService.Settings.General.RememberAppWindowPlacement == true)
            {
                WindowPlacement windowPlacement = _settingsService.Settings.General.AppWindowPlacement;
                NativeMethods.SetWindowPlacementEx(_windowHandle, ref windowPlacement);
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // remember the window position
            if (_settingsService.Settings.General.RememberAppWindowPlacement == true)
                _settingsService.Settings.General.AppWindowPlacement = NativeMethods.GetWindowPlacementEx(_windowHandle);

            // save notes before exiting
            _noteService.SaveNotes();

            // save settings before exiting
            _settingsService.Save();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // save the main window handle
            _windowHandle = new WindowInteropHelper(this).Handle;

            HwndSource handleSource = HwndSource.FromHwnd(_windowHandle);
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