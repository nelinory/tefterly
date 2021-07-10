using Tefterly.Core;
using Tefterly.Core.Models;

namespace Tefterly.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly SettingsManager _settingsManager;

        public SettingsService()
        {
            _settingsManager = SettingsManager.Instance;

            Settings = _settingsManager.Settings;
        }

        public Settings Settings { get; private set; }

        public void Get<T>(string settingName, ref T value)
        {
            _settingsManager.Get<T>(settingName, ref value);
        }

        public void Set<T>(string settingName, T value)
        {
            _settingsManager.Set<T>(settingName, value);
        }

        public void Load()
        {
            _settingsManager.Load();
        }

        public void Save()
        {
            _settingsManager.Save();
        }
    }
}
