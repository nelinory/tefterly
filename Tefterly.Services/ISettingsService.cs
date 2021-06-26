using Tefterly.Core.Models;

namespace Tefterly.Services
{
    public interface ISettingsService
    {
        Settings Settings { get; }
        void Set<T>(string settingName, T value);
        void Get<T>(string settingName, ref T value);
        void Save();
    }
}
