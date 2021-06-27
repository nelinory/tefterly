﻿using System;
using System.IO;
using System.Text.Json;
using Tefterly.Core.Models;

namespace Tefterly.Core
{
    public sealed class SettingsManager
    {
        private static readonly Lazy<SettingsManager> _intance = new Lazy<SettingsManager>(() => new SettingsManager());

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true };
        private static readonly string _settingsFileLocation = Path.Combine(Environment.CurrentDirectory, "Tefterly.config");

        public static SettingsManager Instance { get { return _intance.Value; } }

        public Settings Settings { get; private set; }

        private SettingsManager()
        {
            Settings = new Settings();

            if (File.Exists(_settingsFileLocation) == true)
            {
                using (FileStream fs = File.OpenRead(_settingsFileLocation))
                {
                    Settings = JsonSerializer.DeserializeAsync<Settings>(fs).Result;
                }
            }

            if (Settings.CurrentVersion != Settings.LatestVersion)
            {
                // always backup on version change
                BackupManager.VersionChangeBackup(Settings.BackupLocation, Settings.NotesLocation, Settings.CurrentVersion, Settings.LatestVersion, Settings.Backup.MaxVersionChangeBackups);

                // settings migration started

                // settings migration completed
                Settings.CurrentVersion = Settings.LatestVersion;
                Save();
            }
        }

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

        public void Save()
        {
            using (FileStream fs = File.Create(_settingsFileLocation))
            {
                JsonSerializer.SerializeAsync(fs, Settings, _jsonSerializerOptions);
            }
        }
    }
}