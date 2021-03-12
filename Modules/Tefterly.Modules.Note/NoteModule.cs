using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Tefterly.Core;
using Tefterly.Modules.Note.ViewModels;
using Tefterly.Modules.Note.Views;

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
            _regionManager.RegisterViewWithRegion(RegionNames.NoteRegion, typeof(NoteView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ViewModelLocationProvider.Register<NoteView, NoteViewModel>();
        }
    }
}