using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using Tefterly.Core;

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

        private DelegateCommand<string> _navigateCommand;
        private readonly IRegionManager _regionManager;

        public DelegateCommand<string> NavigateCommand => _navigateCommand ?? (_navigateCommand = new DelegateCommand<string>(ExecuteCommandName));

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        void ExecuteCommandName(string navigationPath)
        {
            if (string.IsNullOrWhiteSpace(navigationPath) == true)
                throw new ArgumentNullException();

            _regionManager.RequestNavigate(RegionNames.NotesRegion, navigationPath);
        }
    }
}
