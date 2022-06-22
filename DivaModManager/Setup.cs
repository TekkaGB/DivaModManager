using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Octokit;
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using DivaModManager.UI;
using Onova;
using Onova.Services;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers;
using SharpCompress.Common;

namespace DivaModManager
{
    public static class Setup
    {
        private static ProgressBox progressBox;
        private static GitHubClient client = new GitHubClient(new ProductHeaderValue("DivaModManager"));
        public static async Task<bool> CheckForDMLUpdate(CancellationTokenSource cancellationToken)
        {
            var gameFolder = Path.GetDirectoryName(Global.config.Configs[Global.config.CurrentGame].Launcher);
            if (!File.Exists($"{gameFolder}{Global.s}config.toml") || !File.Exists($"{gameFolder}{Global.s}dinput8.dll"))
            {
                Global.config.Configs[Global.config.CurrentGame].ModLoaderVersion = null;
                Global.UpdateConfig();
            }
            // Get Version Number
            var localVersion = Global.config.Configs[Global.config.CurrentGame].ModLoaderVersion;
            try
            {
                var owner = "blueskythlikesclouds";
                var repo = "DivaModLoader";
                Release release = await client.Repository.Release.GetLatest(owner, repo);
                Match onlineVersionMatch = Regex.Match(release.TagName, @"(?<version>([0-9]+\.?)+)[^a-zA-Z]");
                string onlineVersion = null;
                if (onlineVersionMatch.Success)
                {
                    onlineVersion = onlineVersionMatch.Value;
                }
                if (UpdateAvailable(onlineVersion, localVersion))
                {
                    string downloadUrl = release.Assets.First().BrowserDownloadUrl;
                    string fileName = release.Assets.First().Name;
                    if (localVersion != null)
                    {
                        ChangelogBox notification = new ChangelogBox(release, "DivaModLoader", $"A new version of DivaModLoader is available (v{onlineVersion})!", null);
                        notification.ShowDialog();
                        notification.Activate();
                        if (notification.YesNo)
                        {
                            await DownloadDML(downloadUrl, fileName, onlineVersion, new Progress<DownloadProgress>(ReportUpdateProgress), cancellationToken);
                            if (!File.Exists($"{gameFolder}{Global.s}config.toml") || !File.Exists($"{gameFolder}{Global.s}dinput8.dll")
                                || String.IsNullOrEmpty(Global.config.Configs[Global.config.CurrentGame].ModLoaderVersion))
                            {
                                Global.logger.WriteLine($"DivaModLoader failed to install, try setting up again.", LoggerType.Error);
                                return false;
                            }
                            else
                                return true;
                        }
                    }
                    // Download with no notification if it doesn't exist
                    else
                    {
                        await DownloadDML(downloadUrl, fileName, onlineVersion, new Progress<DownloadProgress>(ReportUpdateProgress), cancellationToken);
                        if (!File.Exists($"{gameFolder}{Global.s}config.toml") || !File.Exists($"{gameFolder}{Global.s}dinput8.dll")
                            || String.IsNullOrEmpty(Global.config.Configs[Global.config.CurrentGame].ModLoaderVersion))
                        {
                            Global.logger.WriteLine($"DivaModLoader failed to install, try setting up again.", LoggerType.Error);
                            return false;
                        }
                        else
                            return true;
                    }
                }
                else
                {
                    Global.logger.WriteLine("No update for DivaModLoader available.", LoggerType.Info);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Global.logger.WriteLine(ex.Message, LoggerType.Error);
                return false;
            }
            return true;
        }
        private static async Task DownloadDML(string uri, string fileName, string version, Progress<DownloadProgress> progress, CancellationTokenSource cancellationToken)
        {
            var downloadName = $@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}DML{Global.s}{version}{Path.GetExtension(fileName)}";
            try
            {
                // Create the downloads folder if necessary
                Directory.CreateDirectory($@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}DML");
                // Download the file if it doesn't already exist
                if (File.Exists(downloadName))
                {
                    try
                    {
                        File.Delete(downloadName);
                    }
                    catch (Exception e)
                    {
                        Global.logger.WriteLine($"Couldn't delete the already existing {downloadName} ({e.Message})",
                            LoggerType.Error);
                        return;
                    }
                }
                Global.logger.WriteLine("Downloading DivaModLoader...", LoggerType.Info);
                progressBox = new ProgressBox(cancellationToken);
                progressBox.progressBar.Value = 0;
                progressBox.finished = false;
                progressBox.Title = $"Download Progress";
                progressBox.Show();
                progressBox.Activate();
                // Write and download the file
                using (var fs = new FileStream(
                    downloadName, System.IO.FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var client = new HttpClient();
                    await client.DownloadAsync(uri, fs, fileName, progress, cancellationToken.Token);
                }
                progressBox.Close();
                var outputPath = Path.GetDirectoryName(Global.config.Configs[Global.config.CurrentGame].Launcher);
                await ExtractFile(downloadName, outputPath, version);
            }
            catch (OperationCanceledException)
            {
                // Remove the file is it will be a partially downloaded one and close up
                File.Delete(downloadName);
                if (progressBox != null)
                {
                    progressBox.finished = true;
                    progressBox.Close();
                }
                return;
            }
            catch (Exception e)
            {
                if (progressBox != null)
                {
                    progressBox.finished = true;
                    progressBox.Close();
                }
                Global.logger.WriteLine($"Error whilst downloading DivaModLoader ({e.Message})", LoggerType.Error);
            }
        }
        private static async Task ExtractFile(string fileName, string output, string version)
        {
            await Task.Run(() =>
            {
                string _ArchiveSource = fileName;
                string ArchiveDestination = output;
                Directory.CreateDirectory(ArchiveDestination);
                if (File.Exists(_ArchiveSource))
                {
                    try
                    {
                        if (Path.GetExtension(_ArchiveSource).Equals(".7z", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using (var archive = SevenZipArchive.Open(_ArchiveSource))
                            {
                                var reader = archive.ExtractAllEntries();
                                while (reader.MoveToNextEntry())
                                {
                                    if (!reader.Entry.IsDirectory && (reader.Entry.Key.ToLowerInvariant() == "dinput8.dll"
                                    || (reader.Entry.Key.ToLowerInvariant() == "config.toml") && !File.Exists($"{ArchiveDestination}{Global.s}config.toml")))
                                        reader.WriteEntryToDirectory(ArchiveDestination, new ExtractionOptions()
                                        {
                                            ExtractFullPath = false,
                                            Overwrite = true
                                        });
                                }
                            }
                        }
                        else
                        {
                            using (Stream stream = File.OpenRead(_ArchiveSource))
                            using (var reader = ReaderFactory.Open(stream))
                            {
                                while (reader.MoveToNextEntry())
                                {
                                    if (!reader.Entry.IsDirectory && (reader.Entry.Key.ToLowerInvariant() == "dinput8.dll"
                                    || (reader.Entry.Key.ToLowerInvariant() == "config.toml") && !File.Exists($"{ArchiveDestination}{Global.s}config.toml")))
                                        reader.WriteEntryToDirectory(ArchiveDestination, new ExtractionOptions()
                                        {
                                            ExtractFullPath = false,
                                            Overwrite = true
                                        });
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Global.logger.WriteLine($"Couldn't extract {fileName}. ({e.Message})", LoggerType.Error);
                    }
                    Global.config.Configs[Global.config.CurrentGame].ModLoaderVersion = version;
                    Global.UpdateConfig();
                    File.Delete(_ArchiveSource);
                    Global.logger.WriteLine($"Finished updating DivaModLoader.", LoggerType.Info);
                }
            });

        }
        private static void ReportUpdateProgress(DownloadProgress progress)
        {
            if (progress.Percentage == 1)
            {
                progressBox.finished = true;
            }
            progressBox.progressBar.Value = progress.Percentage * 100;
            progressBox.taskBarItem.ProgressValue = progress.Percentage;
            progressBox.progressTitle.Text = $"Downloading {progress.FileName}...";
            progressBox.progressText.Text = $"{Math.Round(progress.Percentage * 100, 2)}% " +
                $"({StringConverters.FormatSize(progress.DownloadedBytes)} of {StringConverters.FormatSize(progress.TotalBytes)})";
        }
        private static bool UpdateAvailable(string onlineVersion, string localVersion)
        {
            if (onlineVersion is null)
                return false;
            if (localVersion is null)
                return true;
            string[] onlineVersionParts = onlineVersion.Split('.');
            string[] localVersionParts = localVersion.Split('.');
            // Pad the version if one has more parts than another (e.g. 1.2.1 and 1.2)
            if (onlineVersionParts.Length > localVersionParts.Length)
            {
                for (int i = localVersionParts.Length; i < onlineVersionParts.Length; i++)
                {
                    localVersionParts = localVersionParts.Append("0").ToArray();
                }
            }
            else if (localVersionParts.Length > onlineVersionParts.Length)
            {
                for (int i = onlineVersionParts.Length; i < localVersionParts.Length; i++)
                {
                    onlineVersionParts = onlineVersionParts.Append("0").ToArray();
                }
            }
            // Decide whether the online version is new than local
            for (int i = 0; i < onlineVersionParts.Length; i++)
            {
                if (!int.TryParse(onlineVersionParts[i], out _))
                {
                    MessageBox.Show($"Couldn't parse {onlineVersion}");
                    return false;
                }
                if (!int.TryParse(localVersionParts[i], out _))
                {
                    MessageBox.Show($"Couldn't parse {localVersion}");
                    return false;
                }
                if (int.Parse(onlineVersionParts[i]) > int.Parse(localVersionParts[i]))
                {
                    return true;
                }
                else if (int.Parse(onlineVersionParts[i]) != int.Parse(localVersionParts[i]))
                {
                    return false;
                }
            }
            return false;
        }
        public static bool Generic(string exe, string defaultPath)
        {
            // Get install path from registry
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 1761390");
                if (!String.IsNullOrEmpty(key.GetValue("InstallLocation") as string))
                    defaultPath = $"{key.GetValue("InstallLocation") as string}{Global.s}DivaMegaMix.exe";
            }
            catch (Exception e)
            {
                Global.logger.WriteLine($"Couldn't find install path in registry ({e.Message})", LoggerType.Error);
            }
            if (!File.Exists(defaultPath))
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.DefaultExt = ".exe";
                dialog.Filter = $"Executable Files ({exe})|{exe}";
                dialog.Title = $"Select {exe} from your Steam Install folder";
                dialog.Multiselect = false;
                dialog.InitialDirectory = Global.assemblyLocation;
                dialog.ShowDialog();
                if (!String.IsNullOrEmpty(dialog.FileName)
                    && Path.GetFileName(dialog.FileName).Equals(exe, StringComparison.InvariantCultureIgnoreCase))
                    defaultPath = dialog.FileName;
                else if (!String.IsNullOrEmpty(dialog.FileName))
                {
                    Global.logger.WriteLine($"Invalid .exe chosen", LoggerType.Error);
                    return false;
                }
                else
                    return false;
            }
            var parent = Path.GetDirectoryName(defaultPath);
            Global.config.Configs[Global.config.CurrentGame].Launcher = defaultPath;
            // Check for DML update
            if (!File.Exists($"{parent}{Global.s}config.toml")
                || !File.Exists($"{parent}{Global.s}dinput8.dll"))
            {
                Global.config.Configs[Global.config.CurrentGame].ModLoaderVersion = null;
                Global.UpdateConfig();
            }
            Task<bool> task = null;
            App.Current.Dispatcher.Invoke(() =>
                {
                    task = CheckForDMLUpdate(new CancellationTokenSource());
                });
            if (!task.Result)
                return false;
            var ModsFolder = $"{parent}{Global.s}mods";
            Directory.CreateDirectory(ModsFolder);
            Global.config.Configs[Global.config.CurrentGame].ModsFolder = ModsFolder;
            Global.UpdateConfig();
            return true;
        }
    }
}
