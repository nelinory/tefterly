using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using Tefterly.Core.Commands;
using Tefterly.Core.Events;
using Tefterly.Core.Navigation;

namespace Tefterly.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        // services
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;

        // commands
        public DelegateCommand<NavigationItem> GlobalNavigateCommand { get; set; }

        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IApplicationCommands applicationCommands)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            // attach all commands
            GlobalNavigateCommand = new DelegateCommand<NavigationItem>((NavigationItem) => ExecuteGlobalNavigateCommand(NavigationItem));

            applicationCommands.NavigateCommand.RegisterCommand(GlobalNavigateCommand);

            // subscribe to important events
            _eventAggregator.GetEvent<NoteChangedEvent>().Subscribe(x =>
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay} - [Event] NoteChangedEvent triggered");
            });
            _eventAggregator.GetEvent<ResetNotebookCategoryEvent>().Subscribe(x =>
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay} - [Event] ResetNotebookCategoryEvent triggered");
            });
            _eventAggregator.GetEvent<ThemeChangedEvent>().Subscribe(x =>
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay} - [Event] ThemeChangedEvent triggered");
            });
        }

        private void ExecuteGlobalNavigateCommand(NavigationItem navigationItem)
        {
            if (navigationItem == null)
                throw new ArgumentNullException();

            System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay} - [Navigation] /{navigationItem.NavigationRegion}/{navigationItem.NavigationPath}{navigationItem.NavigationParameters} executed");

            _regionManager.RequestNavigate(navigationItem.NavigationRegion, navigationItem.NavigationPath, navigationItem.NavigationParameters);
        }
    }
}
