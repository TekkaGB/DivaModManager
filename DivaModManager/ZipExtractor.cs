using Onova.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SevenZipExtractor;
using SharpCompress.Common;
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
                if (Path.GetExtension(sourceFilePath).Equals(".7z", StringComparison.InvariantCultureIgnoreCase))
                {
                    using (var archive = new ArchiveFile(sourceFilePath))
                    {
                        archive.Extract(destDirPath);
                    }
                }
                else
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
            }
            catch
            {
                Global.logger.WriteLine("Failed to extract update", LoggerType.Error);
            }
            File.Delete(@$"{sourceFilePath}");
        }

    }
}
