using System;
using System.IO;
using System.Text.Json;
using Tefterly.Business.Models;

namespace Tefterly.Services
{
    public class SettingsService : ISettingsService
    {
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        public SettingsService()
        {
            Load();
        }

        public string SettingsFileLocation
        {
            get { return Path.Combine(Environment.CurrentDirectory, "Tefterly.config"); }
        }

        public Settings Settings { get; private set; }

        public void Get<T>(string settingNamespace, string settingName, ref T value)
        {
            throw new NotImplementedException();
        }

        public void Set<T>(string settingNamespace, string settingName, T value)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            if (File.Exists(SettingsFileLocation) == true)
            {
                using (FileStream fs = File.OpenRead(SettingsFileLocation))
                {
                    Settings = JsonSerializer.DeserializeAsync<Settings>(fs).Result;
                }
            }
            else
                Settings = new Settings();
        }

        public void Save()
        {
            using (FileStream fs = File.Create(SettingsFileLocation))
            {
                JsonSerializer.SerializeAsync(fs, Settings, _jsonSerializerOptions);
            }
        }
    }
}
