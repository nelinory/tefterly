using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using Tefterly.Business;
using Tefterly.Core.Commands;

namespace Tefterly.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        // services
        private readonly IRegionManager _regionManager;

        // commands
        public DelegateCommand<NavigationItem> GlobalNavigateCommand { get; set; }

        public MainWindowViewModel(IRegionManager regionManager, IApplicationCommands applicationCommands)
        {
            _regionManager = regionManager;

            // attach all commands
            GlobalNavigateCommand = new DelegateCommand<NavigationItem>((NavigationItem) => ExecuteGlobalNavigateCommand(NavigationItem));

            applicationCommands.NavigateCommand.RegisterCommand(GlobalNavigateCommand);
        }
        
        private void ExecuteGlobalNavigateCommand(NavigationItem navigationItem)
        {
            if (navigationItem == null)
                throw new ArgumentNullException();

            _regionManager.RequestNavigate(navigationItem.NavigationRegion, navigationItem.NavigationPath, navigationItem.NavigationParameters);
        }
    }
}
