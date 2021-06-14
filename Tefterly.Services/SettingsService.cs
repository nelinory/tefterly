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

        public void Get<T>(string settingName, ref T value)
        {
            // TODO: Is it needed
            value = (T)Settings.GetType().GetProperty(settingName).GetValue(Settings, null);
        }

        public void Set<T>(string settingName, T value)
        {
            // TODO: Is it needed
            Settings.GetType().GetProperty(settingName).SetValue(Settings, value);
        }

        public void Load()
        {
            Settings = new Settings();

            if (File.Exists(SettingsFileLocation) == true)
            {
                using (FileStream fs = File.OpenRead(SettingsFileLocation))
                {
                    Settings = JsonSerializer.DeserializeAsync<Settings>(fs).Result;
                }
            }
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
