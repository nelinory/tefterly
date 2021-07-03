using Serilog;
using System;
using System.IO;
using System.IO.Compression;

namespace Tefterly.Core
{
    public static class BackupManager
    {
        private static readonly string _regularBackup = "Backup_r";
        private static readonly string _versionChangeBackup = "Backup_vc";

        public static void RegularBackup(string backupLocation, string notesLocation, int maxBackupsToKeep)
        {
            string currentDateTime = GetFormattedDateTime();
            string backupFileName = $"{_regularBackup}_{currentDateTime}.zip";

            if (ExecuteBackup(notesLocation, Path.Combine(backupLocation, backupFileName)) == true)
                PurgeOldBackups(backupLocation, _regularBackup, maxBackupsToKeep);
        }

        public static void VersionChangeBackup(string backupLocation, string notesLocation, int currentVersion, int latestVersion, int maxBackupsToKeep)
        {
            string currentDateTime = GetFormattedDateTime();
            string backupFileName = $"{_versionChangeBackup}_{currentVersion}-{latestVersion}_{currentDateTime}.zip";

            if (ExecuteBackup(notesLocation, Path.Combine(backupLocation, backupFileName)) == true)
                PurgeOldBackups(backupLocation, _versionChangeBackup, maxBackupsToKeep);
        }

        private static bool ExecuteBackup(string sourceFolder, string backupFileName)
        {
            bool success = false;

            try
            {
                if (Directory.Exists(sourceFolder) == true && Directory.GetFiles(sourceFolder).Length > 0)
                {
                    Utilities.EnsureTargetFolderExists(backupFileName);

                    ZipFile.CreateFromDirectory(sourceFolder, backupFileName, CompressionLevel.Fastest, false);

                    success = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while creating backup: {EX}", ex);
            }

            return success;
        }

        private static void PurgeOldBackups(string backupLocation, string backupFilePattern, int maxBackupsToKeep)
        {
            try
            {
                if (Directory.Exists(backupLocation) == true)
                {
                    string[] backupFileNames = Directory.GetFiles(backupLocation, $"{backupFilePattern}*");
                    if (backupFileNames.Length > maxBackupsToKeep)
                    {
                        Array.Sort(backupFileNames, StringComparer.InvariantCulture);

                        int filesToDelete = backupFileNames.Length - maxBackupsToKeep;
                        for (int i = 0; i < filesToDelete; i++)
                        {
                            if (File.Exists(backupFileNames[i]) == true)
                                File.Delete(backupFileNames[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while deleting backup: {EX}", ex);
            }
        }

        private static string GetFormattedDateTime()
        {
            return String.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
        }
    }
}