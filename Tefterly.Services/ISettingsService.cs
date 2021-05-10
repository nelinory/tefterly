using Tefterly.Business.Models;

namespace Tefterly.Services
{
    public interface ISettingsService
    {
        string SettingsFileLocation { get; }
        Settings Settings { get; }
        void Set<T>(string settingNamespace, string settingName, T value);
        void Get<T>(string settingNamespace, string settingName, ref T value);

        void Load();
        void Save();
    }
}
