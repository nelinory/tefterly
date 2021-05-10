using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
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
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // register shared services
            containerRegistry.RegisterSingleton<INoteService, NoteService>();
            containerRegistry.RegisterSingleton<ISettingsService, SettingsService>();

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
    }
}
