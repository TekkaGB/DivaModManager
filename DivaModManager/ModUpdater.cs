using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Net.Http;
using System.Threading;
using DivaModManager.UI;
using System.Reflection;
using System.Windows;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives;
using Tomlyn;
using Tomlyn.Model;

namespace DivaModManager
{
    public static class ModUpdater
    {
        private static ProgressBox progressBox;
        private static int updateCounter;
        public async static Task CheckForUpdates(string path, MainWindow main)
        {
            updateCounter = 0;
            if (!Directory.Exists(path))
            {
                main.GameBox.IsEnabled = true;
                main.ModGrid.IsEnabled = true;
                main.ConfigButton.IsEnabled = true;
                main.LaunchButton.IsEnabled = true;
                main.OpenModsButton.IsEnabled = true;
                main.UpdateButton.IsEnabled = true;
                main.LauncherOptionsBox.IsEnabled = true;
                main.LoadoutBox.IsEnabled = true;
                main.EditLoadoutsButton.IsEnabled = true;
                main.Activate();
                return;
            }
            var cancellationToken = new CancellationTokenSource();
            var requestUrls = new Dictionary<string, List<string>>();
            var DMArequestUrl = "https://divamodarchive.xyz/api/v1/posts/posts?";
            var mods = Directory.GetDirectories(path).Where(x => File.Exists($"{x}{Global.s}mod.json")).ToList();
            var modList = new Dictionary<string, List<string>>();
            var DMAmodList = new List<string>();
            var urlCounts = new Dictionary<string, int>();
            foreach (var mod in mods)
            {
                if (!File.Exists($"{mod}{Global.s}mod.json"))
                    continue;
                Metadata metadata;
                try
                {
                    var metadataString = File.ReadAllText($"{mod}{Global.s}mod.json");
                    metadata = JsonSerializer.Deserialize<Metadata>(metadataString);
                }
                catch (Exception e)
                {
                    Global.logger.WriteLine($"Error occurred while getting metadata for {mod} ({e.Message})", LoggerType.Error);
                    continue;
                }
                Uri url = null;
                if (metadata.homepage != null)
                {
                    url = CreateUri(metadata.homepage.ToString());
                    if (url != null)
                    {
                        var MOD_TYPE = char.ToUpper(url.Segments[1][0]) + url.Segments[1].Substring(1, url.Segments[1].Length - 3);
                        var MOD_ID = url.Segments[2];
                        if (!urlCounts.ContainsKey(MOD_TYPE))
                            urlCounts.Add(MOD_TYPE, 0);
                        int index = urlCounts[MOD_TYPE];
                        if (!modList.ContainsKey(MOD_TYPE))
                            modList.Add(MOD_TYPE, new());
                        modList[MOD_TYPE].Add(mod);
                        if (!requestUrls.ContainsKey(MOD_TYPE))
                            requestUrls.Add(MOD_TYPE, new string[] { $"https://gamebanana.com/apiv6/{MOD_TYPE}/Multi?_csvProperties=_sName,_aSubmitter,_aCategory,_aSuperCategory,_sProfileUrl,_sDescription,_bHasUpdates,_aLatestUpdates,_aFiles,_aPreviewMedia,_aAlternateFileSources,_tsDateUpdated&_csvRowIds=" }.ToList());
                        else if (requestUrls[MOD_TYPE].Count == index)
                            requestUrls[MOD_TYPE].Add($"https://gamebanana.com/apiv6/{MOD_TYPE}/Multi?_csvProperties=_sName,_aSubmitter,_aCategory,_aSuperCategory,_sProfileUrl,_sDescription,_bHasUpdates,_aLatestUpdates,_aFiles,_aPreviewMedia,_aAlternateFileSources,_tsDateUpdated&_csvRowIds=");
                        requestUrls[MOD_TYPE][index] += $"{MOD_ID},";
                        if (requestUrls[MOD_TYPE][index].Length > 1990)
                            urlCounts[MOD_TYPE]++;
                    }
                    else if (metadata.id != null)
                    {
                        DMArequestUrl += $"post_id={metadata.id}&";
                        DMAmodList.Add(mod);
                    }
                }
            }
            // Remove extra comma
            foreach (var key in requestUrls.Keys)
            {
                var counter = 0;
                foreach (var requestUrl in requestUrls[key].ToList())
                {
                    if (requestUrl.EndsWith(","))
                        requestUrls[key][counter] = requestUrl.Substring(0, requestUrl.Length - 1);
                    counter++;
                }

            }
            if (requestUrls.Count == 0 && !DMArequestUrl.Contains("post_id"))
            {
                Global.logger.WriteLine("No mod updates available.", LoggerType.Info);
                main.GameBox.IsEnabled = true;
                main.ModGrid.IsEnabled = true;
                main.ConfigButton.IsEnabled = true;
                main.LaunchButton.IsEnabled = true;
                main.OpenModsButton.IsEnabled = true;
                main.UpdateButton.IsEnabled = true;
                main.LauncherOptionsBox.IsEnabled = true;
                main.LoadoutBox.IsEnabled = true;
                main.EditLoadoutsButton.IsEnabled = true;
                return;
            }
            List<GameBananaAPIV4> response = new();
            List<DivaModArchivePost> DMAresponse = new();
            using (var client = new HttpClient())
            {
                foreach (var type in requestUrls)
                {
                    foreach (var requestUrl in type.Value)
                    {
                        try
                        {
                            var responseString = await client.GetStringAsync(requestUrl);
                            var partialResponse = JsonSerializer.Deserialize<List<GameBananaAPIV4>>(responseString);
                            response = response.Concat(partialResponse).ToList();
                        }
                        catch (Exception e)
                        {
                            Global.logger.WriteLine(e.Message, LoggerType.Error);
                            main.GameBox.IsEnabled = true;
                            main.ModGrid.IsEnabled = true;
                            main.ConfigButton.IsEnabled = true;
                            main.LaunchButton.IsEnabled = true;
                            main.OpenModsButton.IsEnabled = true;
                            main.UpdateButton.IsEnabled = true;
                            main.LauncherOptionsBox.IsEnabled = true;
                            main.LoadoutBox.IsEnabled = true;
                            main.EditLoadoutsButton.IsEnabled = true;
                            return;
                        }
                    }
                }
                // DivaModArchive updates
                if (DMArequestUrl.Contains("post_id"))
                {
                    var responseString = await client.GetStringAsync(DMArequestUrl);
                    DMAresponse = JsonSerializer.Deserialize<List<DivaModArchivePost>>(responseString);
                }
            }
            var convertedModList = new List<string>();
            foreach (var type in modList)
                foreach (var mod in type.Value)
                    convertedModList.Add(mod);
            for (int i = 0; i < convertedModList.Count; i++)
            {
                Metadata metadata;
                try
                {
                    metadata = JsonSerializer.Deserialize<Metadata>(File.ReadAllText($"{convertedModList[i]}{Global.s}mod.json"));
                }
                catch (Exception e)
                {
                    Global.logger.WriteLine($"Error occurred while getting metadata for {convertedModList[i]} ({e.Message})", LoggerType.Error);
                    continue;
                }
                await ModUpdate(response[i], convertedModList[i], metadata, new Progress<DownloadProgress>(ReportUpdateProgress), CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Token));
            }

            if (DMAresponse.Count > 0)
            {
                foreach (var DMAmod in DMAmodList)
                {
                    Metadata metadata;
                    try
                    {
                        metadata = JsonSerializer.Deserialize<Metadata>(File.ReadAllText($"{DMAmod}{Global.s}mod.json"));
                    }
                    catch (Exception e)
                    {
                        Global.logger.WriteLine($"Error occurred while getting metadata for {DMAmod} ({e.Message})", LoggerType.Error);
                        continue;
                    }
                    var DMAresponseMod = DMAresponse.Find(x => x.ID == metadata.id);
                    await ModUpdate(DMAresponseMod, DMAmod, metadata, new Progress<DownloadProgress>(ReportUpdateProgress), CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Token));
                }
            }

            if (updateCounter == 0)
                Global.logger.WriteLine("No mod updates available.", LoggerType.Info);
            else
                Global.logger.WriteLine("Done checking for mod updates!", LoggerType.Info);

            main.GameBox.IsEnabled = true;
            main.ModGrid.IsEnabled = true;
            main.ConfigButton.IsEnabled = true;
            main.LaunchButton.IsEnabled = true;
            main.OpenModsButton.IsEnabled = true;
            main.UpdateButton.IsEnabled = true;
            main.LauncherOptionsBox.IsEnabled = true;
            main.LoadoutBox.IsEnabled = true;
            main.EditLoadoutsButton.IsEnabled = true;
            main.Activate();
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
        private static async Task ModUpdate(GameBananaAPIV4 item, string mod, Metadata metadata, Progress<DownloadProgress> progress, CancellationTokenSource cancellationToken)
        {
            // If lastupdate doesn't exist, add one
            if (metadata.lastupdate == null)
            {
                if (item.HasUpdates != null && (bool)item.HasUpdates)
                    metadata.lastupdate = item.Updates[0].DateAdded;
                else
                    metadata.lastupdate = new DateTime(1970, 1, 1);
                string metadataString = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText($@"{mod}{Global.s}mod.json", metadataString);
                return;
            }
            if (item.HasUpdates != null && (bool)item.HasUpdates)
            {
                var update = item.Updates[0];
                // Compares dates of last update to current
                if (DateTime.Compare((DateTime)metadata.lastupdate, update.DateAdded) < 0)
                {
                    ++updateCounter;
                    // Display the changelog and confirm they want to update
                    Global.logger.WriteLine($"An update is available for {Path.GetFileName(mod)}!", LoggerType.Info);
                    ChangelogBox changelogBox = new ChangelogBox(update, Path.GetFileName(mod), $"A new update is available for {Path.GetFileName(mod)}", item.Image, true);
                    changelogBox.Activate();
                    changelogBox.ShowDialog();
                    if (changelogBox.Skip)
                    {
                        if (File.Exists($@"{mod}{Global.s}mod.json"))
                        {
                            Global.logger.WriteLine($"Skipped update for {Path.GetFileName(mod)}...", LoggerType.Info);
                            metadata.lastupdate = update.DateAdded;
                            string metadataString = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText($@"{mod}{Global.s}mod.json", metadataString);
                        }
                        return;
                    }
                    if (!changelogBox.YesNo)
                    {
                        Global.logger.WriteLine($"Declined update for {Path.GetFileName(mod)}...", LoggerType.Info);
                        return;
                    }
                    // Download the update
                    var files = item.Files;
                    string downloadUrl = null, fileName = null;

                    if (files.Count > 1)
                    {
                        UpdateFileBox fileBox = new UpdateFileBox(files.ToList(), Path.GetFileName(mod));
                        fileBox.Activate();
                        fileBox.ShowDialog();
                        downloadUrl = fileBox.chosenFileUrl;
                        fileName = fileBox.chosenFileName;
                    }
                    else if (files.Count == 1)
                    {
                        downloadUrl = files.ElementAt(0).DownloadUrl;
                        fileName = files.ElementAt(0).FileName;
                    }
                    else
                    {
                        Global.logger.WriteLine($"An update is available for {Path.GetFileName(mod)} but no downloadable files are available directly from GameBanana.", LoggerType.Info);
                    }
                    if (item.AlternateFileSources != null)
                    {
                        var choice = MessageBox.Show($"Alternate file sources were found for {Path.GetFileName(mod)}! Would you like to manually update?", "Diva Mod Manager", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (choice == MessageBoxResult.Yes)
                        {
                            new AltLinkWindow(item.AlternateFileSources, Path.GetFileName(mod), Global.config.CurrentGame, metadata.homepage.AbsoluteUri, true).ShowDialog();
                            return;
                        }
                    }
                    if (downloadUrl != null && fileName != null)
                    {
                        await DownloadFile(downloadUrl, fileName, mod, item, progress, cancellationToken);
                    }
                    else
                    {
                        Global.logger.WriteLine($"Cancelled update for {Path.GetFileName(mod)}", LoggerType.Info);
                    }
                }
            }
            else if (item.HasUpdates == null)
                Global.logger.WriteLine($"{mod} was most likely trashed by the creator and cannot receive anymore updates", LoggerType.Warning);
        }
        private static async Task ModUpdate(DivaModArchivePost item, string mod, Metadata metadata, Progress<DownloadProgress> progress, CancellationTokenSource cancellationToken)
        {
            // If lastupdate doesn't exist, add one
            if (metadata.lastupdate == null)
            {
                metadata.lastupdate = item.Date;
                string metadataString = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText($@"{mod}{Global.s}mod.json", metadataString);
                return;
            }
            // Compares dates of last update to current
            if (DateTime.Compare((DateTime)metadata.lastupdate, item.Date) < 0)
            {
                ++updateCounter;
                // Display the changelog and confirm they want to update
                Global.logger.WriteLine($"An update is available for {Path.GetFileName(mod)}!", LoggerType.Info);
                ChangelogBox changelogBox = new ChangelogBox(item, Path.GetFileName(mod), $"A new update is available for {Path.GetFileName(mod)}", true);
                changelogBox.Activate();
                changelogBox.ShowDialog();
                if (changelogBox.Skip)
                {
                    if (File.Exists($@"{mod}{Global.s}mod.json"))
                    {
                        Global.logger.WriteLine($"Skipped update for {Path.GetFileName(mod)}...", LoggerType.Info);
                        metadata.lastupdate = item.Date;
                        string metadataString = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText($@"{mod}{Global.s}mod.json", metadataString);
                    }
                    return;
                }
                if (!changelogBox.YesNo)
                {
                    Global.logger.WriteLine($"Declined update for {Path.GetFileName(mod)}...", LoggerType.Info);
                    return;
                }
                // Download the update
                await DownloadFile(item.DownloadUrl.ToString(), item.DownloadUrl.ToString().Split('/').Last(), mod, item, progress, cancellationToken);
            }
        }
        private static async Task DownloadFile(string uri, string fileName, string mod, GameBananaAPIV4 item, Progress<DownloadProgress> progress, CancellationTokenSource cancellationToken)
        {
            try
            {
                // Create the downloads folder if necessary
                Directory.CreateDirectory($@"{Global.assemblyLocation}{Global.s}Downloads");
                // Download the file if it doesn't already exist
                if (File.Exists($@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName}"))
                {
                    try
                    {
                        File.Delete($@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName}");
                    }
                    catch (Exception e)
                    {
                        Global.logger.WriteLine($"Couldn't delete the already existing {Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName} ({e.Message})",
                            LoggerType.Error);
                        return;
                    }
                }
                progressBox = new ProgressBox(cancellationToken);
                progressBox.progressBar.Value = 0;
                progressBox.finished = false;
                progressBox.Title = $"Download Progress";
                progressBox.Show();
                progressBox.Activate();
                // Write and download the file
                using (var fs = new FileStream(
                    $@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName}", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var client = new HttpClient();
                    await client.DownloadAsync(uri, fs, fileName, progress, cancellationToken.Token);
                }
                progressBox.Close();
                await Task.Run(() =>
                {
                    ClearDirectory(mod);
                    ExtractFile(fileName, mod, item);
                });
            }
            catch (OperationCanceledException)
            {
                // Remove the file is it will be a partially downloaded one and close up
                File.Delete($@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName}");
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
                Global.logger.WriteLine($"Error whilst downloading {fileName} ({e.Message})", LoggerType.Error);
            }
        }
        private static async Task DownloadFile(string uri, string fileName, string mod, DivaModArchivePost item, Progress<DownloadProgress> progress, CancellationTokenSource cancellationToken)
        {
            try
            {
                // Create the downloads folder if necessary
                Directory.CreateDirectory($@"{Global.assemblyLocation}{Global.s}Downloads");
                // Download the file if it doesn't already exist
                if (File.Exists($@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName}"))
                {
                    try
                    {
                        File.Delete($@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName}");
                    }
                    catch (Exception e)
                    {
                        Global.logger.WriteLine($"Couldn't delete the already existing {Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName} ({e.Message})",
                            LoggerType.Error);
                        return;
                    }
                }
                progressBox = new ProgressBox(cancellationToken);
                progressBox.progressBar.Value = 0;
                progressBox.finished = false;
                progressBox.Title = $"Download Progress";
                progressBox.Show();
                progressBox.Activate();
                // Write and download the file
                using (var fs = new FileStream(
                    $@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName}", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var client = new HttpClient();
                    await client.DownloadAsync(uri, fs, fileName, progress, cancellationToken.Token);
                }
                progressBox.Close();
                await Task.Run(() =>
                {
                    ClearDirectory(mod);
                    ExtractFile(fileName, mod, item);
                });
            }
            catch (OperationCanceledException)
            {
                // Remove the file is it will be a partially downloaded one and close up
                File.Delete($@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName}");
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
                Global.logger.WriteLine($"Error whilst downloading {fileName} ({e.Message})", LoggerType.Error);
            }
        }

        private static void ClearDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (FileInfo fi in dir.GetFiles())
            {
                if (fi.Name.ToLowerInvariant() != "mod.json" && fi.Name.ToLowerInvariant() != "config.toml")
                    fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                ClearDirectory(di.FullName);
                di.Delete();
            }
        }
        private static void ExtractFile(string fileName, string output, GameBananaAPIV4 item)
        {
            string _ArchiveSource = $@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName}";
            string ArchiveDestination = $@"{Global.assemblyLocation}{Global.s}temp";
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
                                if (!reader.Entry.IsDirectory)
                                    reader.WriteEntryToDirectory(ArchiveDestination, new ExtractionOptions()
                                    {
                                        ExtractFullPath = true,
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
                                if (!reader.Entry.IsDirectory)
                                {
                                    reader.WriteEntryToDirectory(ArchiveDestination, new ExtractionOptions()
                                    {
                                        ExtractFullPath = true,
                                        Overwrite = true
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Global.logger.WriteLine($"Couldn't extract {fileName}. ({e.Message})", LoggerType.Error);
                    return;
                }
                TomlTable oldConfig = null;
                if (File.Exists($@"{output}{Global.s}config.toml"))
                    Toml.TryToModel(File.ReadAllText($@"{output}{Global.s}config.toml"), out oldConfig, out var diagnostics);
                foreach (var folder in Directory.GetDirectories(ArchiveDestination, "*", SearchOption.AllDirectories).Where(x => File.Exists($@"{x}{Global.s}config.toml")))
                {
                    MoveDirectory(folder, output);
                }
                if (File.Exists($@"{output}{Global.s}mod.json"))
                {
                    var metadata = JsonSerializer.Deserialize<Metadata>(File.ReadAllText($@"{output}{Global.s}mod.json"));
                    metadata.submitter = item.Owner.Name;
                    metadata.description = item.Description;
                    metadata.preview = item.Image;
                    metadata.homepage = item.Link;
                    metadata.avi = item.Owner.Avatar;
                    metadata.upic = item.Owner.Upic;
                    metadata.cat = item.CategoryName;
                    metadata.caticon = item.Category.Icon;
                    metadata.lastupdate = item.DateUpdated;
                    string metadataString = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText($@"{output}{Global.s}mod.json", metadataString);
                }
                // Use all old config values that aren't metadata to be shown
                if (oldConfig != null && File.Exists($@"{output}{Global.s}config.toml"))
                {
                    if (Toml.TryToModel(File.ReadAllText($@"{output}{Global.s}config.toml"), out TomlTable newConfig, out var diagnostics))
                        foreach (var key in oldConfig.Keys)
                        {
                            if (key.ToLowerInvariant() != "name" && key.ToLowerInvariant() != "author" && key.ToLowerInvariant() != "version" &&
                                key.ToLowerInvariant() != "date" && key.ToLowerInvariant() != "description" && newConfig.ContainsKey(key))
                                newConfig[key] = oldConfig[key];
                        }
                    else
                    {
                        Global.logger.WriteLine($"{diagnostics[0].Message} for {Path.GetFileName(output)}'s updated config.toml. Reusing former config.toml", LoggerType.Warning);
                        // Reuse old config if new config failed to parse
                        newConfig = oldConfig;
                    }
                    var configString = Toml.FromModel(newConfig);
                    File.WriteAllText($@"{output}{Global.s}config.toml", configString);
                }
                File.Delete(_ArchiveSource);
                Directory.Delete(ArchiveDestination, true);
            }
        }
        private static void ExtractFile(string fileName, string output, DivaModArchivePost item)
        {
            string _ArchiveSource = $@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName}";
            string ArchiveDestination = $@"{Global.assemblyLocation}{Global.s}temp";
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
                                if (!reader.Entry.IsDirectory)
                                    reader.WriteEntryToDirectory(ArchiveDestination, new ExtractionOptions()
                                    {
                                        ExtractFullPath = true,
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
                                if (!reader.Entry.IsDirectory)
                                {
                                    reader.WriteEntryToDirectory(ArchiveDestination, new ExtractionOptions()
                                    {
                                        ExtractFullPath = true,
                                        Overwrite = true
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Global.logger.WriteLine($"Couldn't extract {fileName}. ({e.Message})", LoggerType.Error);
                    return;
                }
                TomlTable oldConfig = null;
                if (File.Exists($@"{output}{Global.s}config.toml"))
                    Toml.TryToModel(File.ReadAllText($@"{output}{Global.s}config.toml"), out oldConfig, out var diagnostics);
                foreach (var folder in Directory.GetDirectories(ArchiveDestination, "*", SearchOption.AllDirectories).Where(x => File.Exists($@"{x}{Global.s}config.toml")))
                {
                    MoveDirectory(folder, output);
                }
                if (File.Exists($@"{output}{Global.s}mod.json"))
                {
                    var metadata = JsonSerializer.Deserialize<Metadata>(File.ReadAllText($@"{output}{Global.s}mod.json"));
                    metadata.id = item.ID;
                    metadata.submitter = item.User.Name;
                    metadata.description = item.ShortText;
                    metadata.preview = item.Image;
                    metadata.homepage = item.Link;
                    metadata.avi = item.User.Avatar;
                    metadata.cat = item.Type;
                    metadata.lastupdate = item.Date;
                    string metadataString = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText($@"{output}{Global.s}mod.json", metadataString);
                }
                // Use all old config values that aren't metadata to be shown
                if (oldConfig != null && File.Exists($@"{output}{Global.s}config.toml"))
                {
                    if (Toml.TryToModel(File.ReadAllText($@"{output}{Global.s}config.toml"), out TomlTable newConfig, out var diagnostics))
                        foreach (var key in oldConfig.Keys)
                        {
                            if (key.ToLowerInvariant() != "name" && key.ToLowerInvariant() != "author" && key.ToLowerInvariant() != "version" &&
                                key.ToLowerInvariant() != "date" && key.ToLowerInvariant() != "description" && newConfig.ContainsKey(key))
                                newConfig[key] = oldConfig[key];
                        }
                    else
                    {
                        Global.logger.WriteLine($"{diagnostics[0].Message} for {Path.GetFileName(output)}'s updated config.toml. Reusing former config.toml", LoggerType.Warning);
                        // Reuse old config if new config failed to parse
                        newConfig = oldConfig;
                    }
                    var configString = Toml.FromModel(newConfig);
                    File.WriteAllText($@"{output}{Global.s}config.toml", configString);
                }
                File.Delete(_ArchiveSource);
                Directory.Delete(ArchiveDestination, true);
            }
        }
        private static void MoveDirectory(string sourcePath, string targetPath)
        {
            //Copy all the files & Replaces any files with the same name
            foreach (var path in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                var newPath = path.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                File.Copy(path, newPath, true);
            }
        }
        private static Uri CreateUri(string url)
        {
            Uri uri;
            if ((Uri.TryCreate(url, UriKind.Absolute, out uri) || Uri.TryCreate("http://" + url, UriKind.Absolute, out uri)) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                // Use validated URI here
                string host = uri.DnsSafeHost;
                if (uri.Segments.Length != 3)
                    return null;
                switch (host)
                {
                    case "www.gamebanana.com":
                    case "gamebanana.com":
                        return uri;
                }
            }
            return null;
        }
    }
}
