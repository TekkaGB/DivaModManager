using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace DivaModManager
{
    public static class HttpClientExtensions
    {
        public static long GetDirectorySize(this DirectoryInfo directoryInfo, bool recursive = true)
        {
            var startDirectorySize = default(long);
            if (directoryInfo == null || !directoryInfo.Exists)
                return startDirectorySize; //Return 0 while Directory does not exist.

            //Add size of files in the Current Directory to main size.
            foreach (var fileInfo in directoryInfo.GetFiles())
                System.Threading.Interlocked.Add(ref startDirectorySize, fileInfo.Length);

            if (recursive) //Loop on Sub Direcotries in the Current Directory and Calculate it's files size.
                System.Threading.Tasks.Parallel.ForEach(directoryInfo.GetDirectories(), (subDirectory) =>
            System.Threading.Interlocked.Add(ref startDirectorySize, GetDirectorySize(subDirectory, recursive)));

            return startDirectorySize;  //Return full Size of this Directory.
        }
        public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, string fileName, IProgress<DownloadProgress> progress = null, CancellationToken cancellationToken = default)
        {
            // Get the http headers first to examine the content length
            using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead))
            {
                var contentLength = response.Content.Headers.ContentLength;

                using (var download = await response.Content.ReadAsStreamAsync())
                {

                    // Ignore progress reporting when no progress reporter was 
                    // passed or when the content length is unknown
                    if (progress == null || !contentLength.HasValue)
                    {
                        await download.CopyToAsync(destination);
                        return;
                    }

                    // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
                    var relativeProgress = new Progress<long>(totalBytes => progress.Report(new DownloadProgress((float)totalBytes / contentLength.Value, totalBytes, contentLength.Value, fileName)));
                    // Use extension method to report progress while downloading
                    await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
                    progress.Report(new DownloadProgress(1, contentLength.Value, contentLength.Value, fileName));
                }
            }
        }
    }
    public static class StreamExtensions
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (!destination.CanWrite)
                throw new ArgumentException("Has to be writable", nameof(destination));
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }
    }
}
