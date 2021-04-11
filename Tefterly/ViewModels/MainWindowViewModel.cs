using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using Tefterly.Business;
using Tefterly.Core;
using Tefterly.Core.Commands;

namespace Tefterly.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Tefterly";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private readonly IRegionManager _regionManager;

        private DelegateCommand<NavigationItem> _globalNavigateCommand;
        public DelegateCommand<NavigationItem> GlobalNavigateCommand => _globalNavigateCommand ?? (_globalNavigateCommand = new DelegateCommand<NavigationItem>(ExecuteGlobalNavigateCommand));

        public MainWindowViewModel(IRegionManager regionManager, IApplicationCommands applicationCommands)
        {
            _regionManager = regionManager;

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
