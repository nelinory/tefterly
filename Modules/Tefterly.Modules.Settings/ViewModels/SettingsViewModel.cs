using ModernWpf.Controls;
using Prism.Events;
using Prism.Mvvm;
using System;
using Tefterly.Core.Events;
using Tefterly.Services;

namespace Tefterly.Modules.Settings.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        // services
        private readonly IEventAggregator _eventAggregator;
        private  ISettingsService _settingsService;

        public SettingsViewModel(IEventAggregator eventAggregator, ISettingsService settingsService)
        {
            // attach all required services
            _eventAggregator = eventAggregator;
            _settingsService = settingsService;

            // subscribe to important events
            _eventAggregator.GetEvent<ModifySettingsEvent>().Subscribe(x => { ModifySettings(); });
        }

        private async void ModifySettings()
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Settings",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Save & Restart",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                Content = new Views.SettingsDialog()
            };

            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay} - Save");
                //_settingsService.Save();
            }
            else if (result == ContentDialogResult.Secondary)
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay} - Save & Restart");
            else
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay} - Escape");
                _settingsService.Load();
            }
        }
    }
}