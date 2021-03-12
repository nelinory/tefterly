using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using Tefterly.Modules.Note;
using Tefterly.Modules.Notebook;
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

        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<NotebookModule>();
            moduleCatalog.AddModule<NotesModule>();
            moduleCatalog.AddModule<NoteModule>();
        }
    }
}
