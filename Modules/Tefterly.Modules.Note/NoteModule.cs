using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Tefterly.Core;

namespace Tefterly.Modules.Note
{
    public class NoteModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public NoteModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion(RegionNames.NoteRegion, typeof(Views.Note));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ViewModelLocationProvider.Register<Views.Note, ViewModels.NoteViewModel>();

            containerRegistry.RegisterForNavigation<Views.Note, ViewModels.NoteViewModel>();
        }
    }
}