namespace DivaModManager
{
    public class DownloadProgress
    {
        public DownloadProgress(float percentage, long downloadedBytes, long totalBytes, string fileName)
        {
            DownloadedBytes = downloadedBytes;
            TotalBytes = totalBytes;
            Percentage = percentage;
            FileName = fileName;
        }

        public float Percentage { get; set; }
        public long DownloadedBytes { get; set; }
        public long TotalBytes { get; set; }
        public string FileName { get; set; }
    }
}
