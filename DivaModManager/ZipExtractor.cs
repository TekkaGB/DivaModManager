using Onova.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Common;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers;

namespace DivaModManager
{
    public class ZipExtractor : IPackageExtractor
    {
        public async Task ExtractPackageAsync(string sourceFilePath, string destDirPath,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var archive = SevenZipArchive.Open(sourceFilePath))
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(destDirPath, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
            catch
            {
                using (Stream stream = File.OpenRead(sourceFilePath))
                using (var reader = ReaderFactory.Open(stream))
                {
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            reader.WriteEntryToDirectory(destDirPath, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }
            }
            File.Delete(@$"{sourceFilePath}");
            // Move the folders to the right place
            string parentPath = Directory.GetParent(destDirPath).FullName;
            Directory.Move(Directory.GetDirectories(destDirPath)[0], $@"{parentPath}{Global.s}DivaModManager");
            Directory.Delete(destDirPath);
            Directory.Move($@"{parentPath}{Global.s}DivaModManager", destDirPath);
        }

    }
}
