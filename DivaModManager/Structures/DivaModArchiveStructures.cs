using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DivaModManager
{
    public class DivaModArchivePost
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("images")]
        public List<Uri> Images { get; set; }
        [JsonPropertyName("files")]
        public List<Uri> Files { get; set; }
        [JsonPropertyName("file_names")]
        public List<String> FileNames { get; set; }
        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
        [JsonIgnore]
        public string DateUpdatedAgo => $"Updated {StringConverters.FormatTimeAgo(DateTime.UtcNow - Time)}";
        [JsonPropertyName("post_type")]
        public string PostType { get; set; }
        [JsonIgnore]
        public Uri Link => new Uri($"https://divamodarchive.com/posts/{ID}");
        [JsonPropertyName("like_count")]
        public int Likes { get; set; }
        [JsonPropertyName("download_count")]
        public int Downloads { get; set; }
        [JsonIgnore]
        public string DownloadString => StringConverters.FormatNumber(Downloads);
        [JsonIgnore]
        public string LikeString => StringConverters.FormatNumber(Likes);

        [JsonPropertyName("authors")]
        public List<DivaModArchiveUser> Authors { get; set; }
    }
    public class DivaModArchiveUser
    {
        [JsonPropertyName("id")]
        public double ID { get; set; }
        [JsonPropertyName("display_name")]
        public string Name { get; set; }
        [JsonPropertyName("avatar")]
        public Uri Avatar { get; set; }
    }
    public class DivaModArchiveModList
    {
        public ObservableCollection<DivaModArchivePost> Posts { get; set; }
        public double TotalPages { get; set; }
        public DateTime TimeFetched = DateTime.UtcNow;
        public bool IsValid => (DateTime.UtcNow - TimeFetched).TotalMinutes < 15;
    }
}
