using Prism.Ioc;
using Prism.Modularity;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Tefterly.Core.Commands;
using Tefterly.Core.Win32Api;
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
        private static readonly Mutex _appMutex = new Mutex(true, "5DC91344-9EA8-4E15-9396-ED4CCBA8B152");
        private static readonly string _logsPath = Path.Combine(Environment.CurrentDirectory, "Logs");

        protected override void OnStartup(StartupEventArgs e)
        {
            ForceSingleInstance();

            ConfigureLogging();

            SetupUnhandledExceptionHandling();

            base.OnStartup(e);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
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

        #region Private methods

        private static void ForceSingleInstance()
        {
            if (_appMutex.WaitOne(TimeSpan.Zero, true) == true)
            {
                _appMutex.ReleaseMutex();

                // show splash window
                SplashScreen splash = new SplashScreen("Spash.png");
                splash.Show(true, true);
            }
            else
            {
                Process[] processes = Process.GetProcessesByName(Assembly.GetEntryAssembly().GetName().Name);
                {
                    if (processes.Length > 0)
                    {
                        NativeMethods.ShowWindowEx(processes[0].MainWindowHandle, NativeMethods.SW_RESTORE_WINDOW);
                        NativeMethods.SetForegroundWindowEx(processes[0].MainWindowHandle);
                    }
                }

                Application.Current.Shutdown();
            }
        }

        private static void ConfigureLogging()
        {
            // configure logging
            Log.Logger = new LoggerConfiguration()
               .WriteTo.File(Path.Combine(_logsPath, "Tefterly-.log"),
                                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                                outputTemplate: "{Timestamp:MM/dd/yyyy HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                                rollingInterval: RollingInterval.Day,
                                retainedFileCountLimit: 7)
               .CreateLogger();
        }

        private void SetupUnhandledExceptionHandling()
        {
            // handler for all exceptions from all threads
            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e)
            {
                LogFatalAndExit(e.ExceptionObject as Exception);
            };

            // handler for all exceptions from a single dispatcher thread
            Application.Current.DispatcherUnhandledException += delegate (object sender, DispatcherUnhandledExceptionEventArgs e)
            {
                // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it
                if (Debugger.IsAttached == false)
                {
                    e.Handled = true;
                    LogFatalAndExit(e.Exception);
                }
            };
        }

        private void LogFatalAndExit(Exception ex)
        {
            Log.Error("Fatal application exception: {EX}", ex);

            string errorMessage = $"A fatal application error occurred.\n\nPlease, check error logs at:\n{_logsPath} for more details.\n\nApplication will close now.";
            MessageBox.Show(errorMessage, "Tefterly - Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);

            try
            {
                // save settings before exiting
                SettingsService settingsService = Container.Resolve<SettingsService>();
                settingsService.Save();

                // save notes before exiting
                NoteService noteService = Container.Resolve<NoteService>();
                noteService.SaveNotes();
            }
            catch (Exception)
            {
                // ignore we are exiting anyway
            }

            Application.Current.Shutdown();
        }

        #endregion
    }
}
