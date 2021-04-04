using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Tefterly.Core;
using Tefterly.Modules.Notebook.ViewModels;
using Tefterly.Modules.Notebook.Views;
using Tefterly.Services;

namespace Tefterly.Modules.Notebook
{
    public class NotebookModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public NotebookModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion(RegionNames.NotebookRegion, typeof(NotebookList));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ViewModelLocationProvider.Register<NotebookList, NotebookListViewModel>();

            containerRegistry.RegisterSingleton<INoteService, NoteService>();
        }
    }
}