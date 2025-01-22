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
        [JsonPropertyName("text_short")]
        public string ShortText { get; set; }
        [JsonIgnore]
        public bool IsShortTextLong => ShortText.Length > 40;
        [JsonPropertyName("image")]
        public Uri Image { get; set; }
        [JsonPropertyName("images_extra")]
        public List<Uri> ExtraImages { get; set; }
        [JsonIgnore]
        public List<Uri> AllImages => GetAllImages(Image, ExtraImages);
        [JsonPropertyName("link")]
        public Uri DownloadUrl { get; set; }
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        [JsonIgnore]
        public string DateUpdatedAgo => $"Updated {StringConverters.FormatTimeAgo(DateTime.UtcNow - Date)}";
        [JsonPropertyName("type_tag")]
        public int TypeTag { get; set; }
        [JsonIgnore]
        public string Type => StringConverters.TypeToString(TypeTag);
        [JsonIgnore]
        public Uri Link => new Uri($"https://divamodarchive.xyz/posts/{ID}");
        [JsonPropertyName("likes")]
        public int Likes { get; set; }
        [JsonPropertyName("dislikes")]
        public int Dislikes { get; set; }
        [JsonPropertyName("downloads")]
        public int Downloads { get; set; }
        [JsonIgnore]
        public string DownloadString => StringConverters.FormatNumber(Downloads);
        [JsonIgnore]
        public string LikeString => StringConverters.FormatNumber(Likes);
        [JsonIgnore]
        public string DislikeString => StringConverters.FormatNumber(Dislikes);

        [JsonPropertyName("user")]
        public DivaModArchiveUser User { get; set; }
        private List<Uri> GetAllImages(Uri MainImage, List<Uri> ImageList)
        {
            List<Uri> Images = new();
            Images.Add(MainImage);
            Images.AddRange(ImageList);
            return Images;
        }
    }
    public class DivaModArchiveUser
    {
        [JsonPropertyName("id")]
        public double ID { get; set; }
        [JsonPropertyName("name")]
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
