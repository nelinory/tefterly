using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Tefterly.Core;
using Tefterly.Modules.Notes.ViewModels;
using Tefterly.Modules.Notes.Views;

namespace Tefterly.Modules.Notebook
{
    public class NotesModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public NotesModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion(RegionNames.NotesRegion, typeof(NotesList));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ViewModelLocationProvider.Register<NotesList, NotesListViewModel>();
        }
    }
}