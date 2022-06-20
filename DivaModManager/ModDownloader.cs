using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using System.Threading;
using System.Text.Json;
using SharpCompress.Common;
using System.Text.RegularExpressions;
using SharpCompress.Readers;
using DivaModManager.UI;
using SharpCompress.Archives.SevenZip;
using System.Linq;

namespace DivaModManager
{
    public class ModDownloader
    {
        private string URL_TO_ARCHIVE;
        private string URL;
        private string DL_ID;
        private string MOD_TYPE;
        private string MOD_ID;
        private string fileName;
        private bool cancelled;
        private HttpClient client = new();
        private CancellationTokenSource cancellationToken = new();
        private GameBananaAPIV4 response = new();
        private ProgressBox progressBox;
        public async void BrowserDownload(string game, GameBananaRecord record)
        {
            if (String.IsNullOrEmpty(Global.config.Configs[Global.config.CurrentGame].ModsFolder)
                || !Directory.Exists(Global.config.Configs[Global.config.CurrentGame].ModsFolder))
            {
                MessageBox.Show($"Please click Setup before installing mods!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Global.logger.WriteLine("Please click Setup before installing mods!", LoggerType.Warning);
                return;
            }
            DownloadWindow downloadWindow = new DownloadWindow(record);
            downloadWindow.ShowDialog();
            if (downloadWindow.YesNo)
            {
                string downloadUrl = null;
                string fileName = null;
                if (record.AllFiles.Count == 1)
                {
                    downloadUrl = record.AllFiles[0].DownloadUrl;
                    fileName = record.AllFiles[0].FileName;
                }
                else if (record.AllFiles.Count > 1)
                {
                    UpdateFileBox fileBox = new UpdateFileBox(record.AllFiles, record.Title);
                    fileBox.Activate();
                    fileBox.ShowDialog();
                    downloadUrl = fileBox.chosenFileUrl;
                    fileName = fileBox.chosenFileName;
                }
                if (downloadUrl != null && fileName != null)
                {
                    await Task.Run(() => DownloadFile(downloadUrl, fileName, new Progress<DownloadProgress>(ReportUpdateProgress),
                        CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Token)));
                    if (!cancelled)
                        await Task.Run(() => ExtractFile(fileName, game, record));
                }
            }
        }
        public async void Download(string line, bool running)
        {
            if (String.IsNullOrEmpty(Global.config.Configs[Global.config.CurrentGame].ModsFolder)
                || !Directory.Exists(Global.config.Configs[Global.config.CurrentGame].ModsFolder))
            {
                MessageBox.Show($"Please click Setup before installing mods!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Global.logger.WriteLine("Please click Setup before installing mods!", LoggerType.Warning);
                return;
            }
            if (ParseProtocol(line))
            {
                if (await GetData())
                {
                    DownloadWindow downloadWindow = new DownloadWindow(response);
                    downloadWindow.ShowDialog();
                    if (downloadWindow.YesNo)
                    {
                        await Task.Run(() => DownloadFile(URL_TO_ARCHIVE, fileName, new Progress<DownloadProgress>(ReportUpdateProgress),
                            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Token)));
                        if (!cancelled)
                            await Task.Run(() => ExtractFile(fileName, response.Game.Name.Replace(":", String.Empty), response));
                    }
                }
            }
            if (running)
                Environment.Exit(0);
        }

        private async Task<bool> GetData()
        {
            try
            {
                string responseString = await client.GetStringAsync(URL);
                response = JsonSerializer.Deserialize<GameBananaAPIV4>(responseString);
                fileName = response.Files.Where(x => x.Id == DL_ID).ToArray()[0].FileName;
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error while fetching data {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }
        private void ReportUpdateProgress(DownloadProgress progress)
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

        private bool ParseProtocol(string line)
        {
            try
            {
                line = line.Replace("dmm:", "");
                string[] data = line.Split(',');
                URL_TO_ARCHIVE = data[0];
                // Used to grab file info from dictionary
                var match = Regex.Match(URL_TO_ARCHIVE, @"\d*$");
                DL_ID = match.Value;
                MOD_TYPE = data[1];
                MOD_ID = data[2];
                URL = $"https://gamebanana.com/apiv6/{MOD_TYPE}/{MOD_ID}?_csvProperties=_sName,_aGame,_sProfileUrl,_aPreviewMedia,_sDescription,_aSubmitter,_aCategory,_aSuperCategory,_aFiles,_tsDateUpdated,_aAlternateFileSources,_bHasUpdates,_aLatestUpdates";
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error while parsing {line}: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }
        private void ExtractFile(string fileName, string game, GameBananaRecord record)
        {
            switch (game)
            {
                case "Hatsune Miku: Project DIVA Mega Mix+":
                    game = "Project DIVA Mega Mix+";
                    break;
            }
            string _ArchiveSource = $@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName}";
            string _ArchiveType = Path.GetExtension(fileName);
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
                    MessageBox.Show($"Couldn't extract {fileName}: {e.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            foreach (var folder in Directory.GetDirectories(ArchiveDestination, "*", SearchOption.AllDirectories).Where(x => File.Exists($@"{x}{Global.s}config.toml")))
            {
                string path = $@"{Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{Path.GetFileName(folder)}";
                int index = 2;
                while (Directory.Exists(path))
                {
                    path = $@"{Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{Path.GetFileName(folder)} ({index})";
                    index += 1;
                }
                MoveDirectory(folder, path);
                if (!File.Exists($@"{ArchiveDestination}{Global.s}mod.json"))
                {
                    Metadata metadata = new Metadata();
                    metadata.submitter = record.Owner.Name;
                    metadata.description = record.Description;
                    metadata.preview = record.Image;
                    metadata.homepage = record.Link;
                    metadata.avi = record.Owner.Avatar;
                    metadata.upic = record.Owner.Upic;
                    metadata.cat = record.CategoryName;
                    metadata.caticon = record.Category.Icon;
                    metadata.lastupdate = record.DateUpdated;
                    string metadataString = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText($@"{path}{Global.s}mod.json", metadataString);
                }
            }
            // Check if folder output folder exists, if not nothing was extracted
            if (!Directory.Exists(ArchiveDestination))
            {
                MessageBox.Show($"Didn't extract {fileName} due to improper format", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                // Only delete if successfully extracted
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
        private void ExtractFile(string fileName, string game, GameBananaAPIV4 record)
        {
            switch (game)
            {
                case "Hatsune Miku: Project DIVA Mega Mix+":
                    game = "Project DIVA Mega Mix+";
                    break;
            }
            string _ArchiveSource = $@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName}";
            string _ArchiveType = Path.GetExtension(fileName);
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
                    MessageBox.Show($"Couldn't extract {fileName}: {e.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            foreach (var folder in Directory.GetDirectories(ArchiveDestination, "*", SearchOption.AllDirectories).Where(x => File.Exists($@"{x}{Global.s}config.toml")))
            {
                string path = $@"{Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{Path.GetFileName(folder)}";
                int index = 2;
                while (Directory.Exists(path))
                {
                    path = $@"{Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{Path.GetFileName(folder)} ({index})";
                    index += 1;
                }
                MoveDirectory(folder, path);
                if (!File.Exists($@"{ArchiveDestination}{Global.s}mod.json"))
                {
                    Metadata metadata = new Metadata();
                    metadata.submitter = record.Owner.Name;
                    metadata.description = record.Description;
                    metadata.preview = record.Image;
                    metadata.homepage = record.Link;
                    metadata.avi = record.Owner.Avatar;
                    metadata.upic = record.Owner.Upic;
                    metadata.cat = record.CategoryName;
                    metadata.caticon = record.Category.Icon;
                    metadata.lastupdate = record.DateUpdated;
                    string metadataString = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText($@"{path}{Global.s}mod.json", metadataString);
                }
            }
            // Check if folder output folder exists, if not nothing was extracted
            if (!Directory.Exists(ArchiveDestination))
            {
                MessageBox.Show($"Didn't extract {fileName} due to improper format", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                // Only delete if successfully extracted
                File.Delete(_ArchiveSource);
                Directory.Delete(ArchiveDestination, true);
            }
        }
        private async void DownloadFile(string uri, string fileName, Progress<DownloadProgress> progress, CancellationTokenSource cancellationToken)
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
                        MessageBox.Show($"Couldn't delete the already existing {Global.assemblyLocation}/Downloads/{fileName} ({e.Message})", 
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    await client.DownloadAsync(uri, fs, fileName, progress, cancellationToken.Token);
                }
                progressBox.Close();
            }
            catch (OperationCanceledException)
            {
                // Remove the file is it will be a partially downloaded one and close up
                File.Delete($@"{Global.assemblyLocation}{Global.s}Downloads{Global.s}{fileName}");
                if (progressBox != null)
                {
                    progressBox.finished = true;
                    progressBox.Close();
                    cancelled = true;
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
                MessageBox.Show($"Error whilst downloading {fileName}. {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                cancelled = true;
            }
        }

    }
}
