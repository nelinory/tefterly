using Prism.Ioc;
using Prism.Modularity;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Tefterly.Core.Commands;
using Tefterly.Modules.Note;
using Tefterly.Modules.Notebook;
using Tefterly.Modules.Notes;
using Tefterly.Services;
using Tefterly.Views;

namespace Tefterly
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE_WINDOW = 9;
        private static readonly Mutex _appMutex = new Mutex(true, "5DC91344-9EA8-4E15-9396-ED4CCBA8B152");

        protected override void OnStartup(StartupEventArgs e)
        {
            // configure logging
            Log.Logger = new LoggerConfiguration()
               .WriteTo.File(Path.Combine(Environment.CurrentDirectory, "Logs\\Tefterly-.log"),
                                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                                outputTemplate: "{Timestamp:MM/dd/yyyy HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                                rollingInterval: RollingInterval.Day,
                                retainedFileCountLimit: 7)
               .CreateLogger();

            // handler for Dispatcher unhandled exceptions
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            base.OnStartup(e);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell(Window shell)
        {
            if (_appMutex.WaitOne(TimeSpan.Zero, true) == true)
            {
                _appMutex.ReleaseMutex();

                // handler for unhandled exceptions
                AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e)
                {
                    LogFatalAndExit(e.ExceptionObject as Exception);
                };

                // show splash window
                //Window splashWin = new Splash();
                //splashWin.Show();
            }
            else
            {
                Process[] processes = Process.GetProcessesByName(Assembly.GetEntryAssembly().GetName().Name);
                {
                    if (processes.Length > 0)
                    {
                        ShowWindow(processes[0].MainWindowHandle, SW_RESTORE_WINDOW);
                        SetForegroundWindow(processes[0].MainWindowHandle);
                    }
                }
                Application.Current.Shutdown();
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // register shared services
            containerRegistry.RegisterSingleton<INoteService, NoteService>();
            containerRegistry.RegisterSingleton<ISettingsService, SettingsService>();
            containerRegistry.RegisterSingleton<ISearchService, SearchService>();

            // register composite commands
            containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            // register modules
            moduleCatalog.AddModule<NoteModule>();
            moduleCatalog.AddModule<NotesModule>();
            moduleCatalog.AddModule<NotebookModule>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // flush all log items before exit
            Log.CloseAndFlush();

            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // prevent default unhandled exception processing
            e.Handled = true;

            LogFatalAndExit(e.Exception);
        }

        private void LogFatalAndExit(Exception ex)
        {
            Log.Error("Fatal application exception: {EX}", ex);

            // save settings before exiting
            SettingsService settingsService = Container.Resolve<SettingsService>();
            settingsService.Save();

            // save notes before exiting
            NoteService noteService = Container.Resolve<NoteService>();
            noteService.SaveNotes();

            Application.Current.Shutdown();
        }
    }
}
