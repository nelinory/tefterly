using ModernWpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Interop;
using Tefterly.Core;
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

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TurnOffAutomationPeer(this);
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

            // backup all notes if backup is enabled
            if (_settingsService.Settings.Backup.IsEnabled == true)
                BackupManager.RegularBackup(_settingsService.Settings.BackupLocation, _settingsService.Settings.NotesLocation, _settingsService.Settings.Backup.MaxRegularBackups);
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
            // http://pinvoke.net/default.aspx/Constants/WM.html

            // Alt+Space key
            uint WM_SYSCOMMAND = 0x112;
            uint SC_KEYMENU = 0xf100;

            // Right mouse button up
            uint WM_NCRBUTTONUP = 0xa5;
            uint HT_CAPTION = 0x02;

            // do not allow the system menu to show up
            if ((message == WM_SYSCOMMAND && wParam.ToInt32() == SC_KEYMENU) || (message == WM_NCRBUTTONUP && wParam.ToInt32() == HT_CAPTION))
                handled = true;

            return IntPtr.Zero;
        }
    }

    // Disable AutomationPeer to avoid memory leaks from WPF components
    public class TurnOffAutomationPeer : WindowAutomationPeer
    {
        public TurnOffAutomationPeer(Window window) : base(window) { }
        protected override List<AutomationPeer> GetChildrenCore() { return null; }
    }
}