using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Onova;
using Onova.Services;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using DivaModManager.UI;
using Octokit;
using System.Windows.Media.Imaging;

namespace DivaModManager
{
    public class AutoUpdater
    {
        private static ProgressBox progressBox;
        private static GitHubClient client = new GitHubClient(new ProductHeaderValue("DivaModManager"));
        private static HttpClient httpClient = new();

        public static async Task<bool> CheckForDMMUpdate(CancellationTokenSource cancellationToken)
        {
            // Get Version Number
            var localVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            try
            {
                var owner = "TekkaGB";
                var repo = "DivaModManager";
                Release release = await client.Repository.Release.GetLatest(owner, repo);
                Match onlineVersionMatch = Regex.Match(release.TagName, @"(?<version>([0-9]+\.?)+)[^a-zA-Z]");
                string onlineVersion = null;
                if (onlineVersionMatch.Success)
                {
                    onlineVersion = onlineVersionMatch.Value;
                }
                if (UpdateAvailable(onlineVersion, localVersion))
                {
                    ChangelogBox notification = new ChangelogBox(release, "Diva Mod Manager", $"A new version of Diva Mod Manager is available (v{onlineVersion})!", null, false, true);
                    notification.ShowDialog();
                    notification.Activate();
                    if (notification.YesNo)
                    {
                        string downloadUrl = release.Assets.First().BrowserDownloadUrl;
                        string fileName = release.Assets.First().Name;
                        // Download the update
                        await DownloadDMM(downloadUrl, fileName, onlineVersion, new Progress<DownloadProgress>(ReportUpdateProgress), cancellationToken);
                        // Notify that the update is about to happen
                        MessageBox.Show($"Finished downloading {fileName}!\nDiva Mod Manager will now restart.", "Notification", MessageBoxButton.OK);
                        // Update DMM
                        UpdateManager updateManager = new UpdateManager(new LocalPackageResolver($"{Global.assemblyLocation}{Global.s}Downloads{Global.s}DMMUpdate"), new ZipExtractor());
                        if (!Version.TryParse(onlineVersion, out Version version))
                        {
                            MessageBox.Show($"Error parsing {onlineVersion}!\nCancelling update.", "Notification", MessageBoxButton.OK);
                            return false;
                        }
                        // Updates and restarts DMM
                        await updateManager.PrepareUpdateAsync(version);
                        updateManager.LaunchUpdater(version);
                        return true;
                    }
                    else
                        Global.logger.WriteLine("Update for Diva Mod Manager cancelled.", LoggerType.Info);
                }
                else
                    Global.logger.WriteLine("No update for Diva Mod Manager available.", LoggerType.Info);
            }
            catch (Exception)
            {

            }
            return false;
        }
        private static async Task DownloadDMM(string uri, string fileName, string version, Progress<DownloadProgress> progress, CancellationTokenSource cancellationToken)
        {
            try
            {
                // Create the downloads folder if necessary
                Directory.CreateDirectory(@$"{Global.assemblyLocation}{Global.s}Downloads{Global.s}DMMUpdate");
                progressBox = new ProgressBox(cancellationToken);
                progressBox.progressBar.Value = 0;
                progressBox.progressText.Text = $"Downloading {fileName}";
                progressBox.Title = "Diva Mod Manager Update Progress";
                progressBox.finished = false;
                progressBox.Show();
                progressBox.Activate();
                // Write and download the file
                using (var fs = new FileStream(
                    $@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}DMMUpdate{Global.s}{fileName}", System.IO.FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await httpClient.DownloadAsync(uri, fs, fileName, progress, cancellationToken.Token);
                }
                // Rename the file
                if (!File.Exists($@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}DMMUpdate{Global.s}{version}.7z"))
                {
                    File.Move($@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}DMMUpdate{Global.s}{fileName}", $@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}DMMUpdate{Global.s}{version}.7z");
                }
                progressBox.Close();
            }
            catch (OperationCanceledException)
            {
                // Remove the file is it will be a partially downloaded one and close up
                File.Delete(@$"{Global.assemblyLocation}{Global.s}Downloads{Global.s}DMMUpdate{Global.s}{fileName}");
                if (progressBox != null)
                {
                    progressBox.finished = true;
                    progressBox.Close();
                }
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Error whilst downloading {fileName} {e.Message}");
                if (progressBox != null)
                {
                    progressBox.finished = true;
                    progressBox.Close();
                }
            }
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
            if (onlineVersion is null || localVersion is null)
            {
                return false;
            }
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
    }
}
