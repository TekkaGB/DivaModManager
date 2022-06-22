using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DivaModManager
{
    public class GameBananaItem
    {
        [JsonPropertyName("Game().name")]
        public string Game { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("views")]
        public int? Views { get; set; }
        [JsonPropertyName("downloads")]
        public int? Downloads { get; set; }
        [JsonPropertyName("likes")]
        public int? Likes { get; set; }
        [JsonPropertyName("Owner().name")]
        public string Owner { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("RootCategory().name")]
        public string RootCat { get; set; }
        [JsonPropertyName("Preview().sSubFeedImageUrl()")]
        public Uri SubFeedImage { get; set; }
        [JsonPropertyName("Preview().sStructuredDataFullsizeUrl()")]
        public Uri EmbedImage { get; set; }
        [JsonPropertyName("Updates().bSubmissionHasUpdates()")]
        public bool? HasUpdates { get; set; }

        [JsonPropertyName("Updates().aGetLatestUpdates()")]
        public GameBananaItemUpdate[] Updates { get; set; }
        [JsonPropertyName("Files().aFiles()")]
        public Dictionary<string, GameBananaItemFile> Files { get; set; }

    }
    public class GameBananaItemFile
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1);
        [JsonPropertyName("_idRow")]
        public string Id { get; set; }
        [JsonPropertyName("_sFile")]
        public string FileName { get; set; }

        [JsonPropertyName("_nFilesize")]
        public long Filesize { get; set; }
        [JsonIgnore]
        public string ConvertedFileSize => StringConverters.FormatSize(Filesize);

        [JsonPropertyName("_sDownloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonPropertyName("_sDescription")]
        public string Description { get; set; }
        [JsonPropertyName("_bContainsExe")]
        public bool ContainsExe { get; set; }
        [JsonPropertyName("_nDownloadCount")]
        public int Downloads { get; set; }
        [JsonIgnore]
        public string DownloadString => StringConverters.FormatNumber(Downloads);

        [JsonPropertyName("_tsDateAdded")]
        public long DateAddedLong { get; set; }

        [JsonIgnore]
        public DateTime DateAdded => Epoch.AddSeconds(DateAddedLong);

        [JsonIgnore]
        public string TimeSinceUpload => StringConverters.FormatTimeAgo(DateTime.UtcNow - DateAdded);
    }
    public class GameBananaGame
    {
        [JsonPropertyName("_sName")]
        public string Name { get; set; }
    }
    public class GameBananaAPIV4
    {
        [JsonPropertyName("_sName")]
        public string Title { get; set; }
        [JsonPropertyName("_aGame")]
        public GameBananaGame Game { get; set; }
        [JsonPropertyName("_sProfileUrl")]
        public Uri Link { get; set; }
        [JsonIgnore]
        public Uri Image => Media.Where(x => x.Type == "image").ToList().Count > 0 ? new Uri($"{Media[0].Base}/{Media[0].File}")
            : new Uri("https://images.gamebanana.com/static/img/DefaultEmbeddables/Sound.jpg");
        [JsonPropertyName("_aPreviewMedia")]
        public List<GameBananaImage> Media { get; set; }
        [JsonPropertyName("_sDescription")]
        public string Description { get; set; }
        [JsonPropertyName("_aSubmitter")]
        public GameBananaMember Owner { get; set; }
        [JsonPropertyName("_aCategory")]
        public GameBananaCategory Category { get; set; }
        [JsonPropertyName("_aSuperCategory")]
        public GameBananaCategory RootCategory { get; set; }
        [JsonIgnore]
        public string CategoryName => RootCategory == null ? StringConverters.FormatSingular(null, Category.Name)
            : StringConverters.FormatSingular(RootCategory.Name, Category.Name);
        [JsonPropertyName("_aFiles")]
        public List<GameBananaItemFile> Files { get; set; }
        [JsonPropertyName("_tsDateUpdated")]
        public long DateUpdatedLong { get; set; }
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1);

        [JsonIgnore]
        public DateTime DateUpdated => Epoch.AddSeconds(DateUpdatedLong);
        [JsonPropertyName("_aAlternateFileSources")]
        public List<GameBananaAlternateFileSource> AlternateFileSources { get; set; }
        [JsonPropertyName("_bHasUpdates")]
        public bool? HasUpdates { get; set; }
        [JsonPropertyName("_aLatestUpdates")]
        public GameBananaItemUpdate[] Updates { get; set; }
    }
    public class GameBananaInstallerIntegration
    {
        [JsonPropertyName("_sDownloadUrl")]
        public string Download { get; set; }
    }
    public class GameBananaCategory
    {
        [JsonPropertyName("_idRow")]
        public int? ID { get; set; }
        [JsonPropertyName("_idParentCategoryRow")]
        public int? RootID { get; set; }
        [JsonPropertyName("_sModelName")]
        public string Model { get; set; }
        [JsonPropertyName("_sName")]
        public string Name { get; set; }
        [JsonPropertyName("_sIconUrl")]
        public Uri Icon { get; set; }
        [JsonIgnore]
        public bool HasIcon => Icon.OriginalString.Length > 0;
    }
    public class GameBananaMember
    {
        [JsonPropertyName("_sName")]
        public string Name { get; set; }
        [JsonPropertyName("_sAvatarUrl")]
        public Uri Avatar { get; set; }
        [JsonPropertyName("_sUpicUrl")]
        public Uri Upic { get; set; }
        [JsonIgnore]
        public bool HasUpic => Upic.OriginalString.Length > 0;
    }
    public class GameBananaItemUpdate
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1);
        [JsonPropertyName("_sTitle")]
        public string Title { get; set; }
        [JsonPropertyName("_sVersion")]
        public string Version { get; set; }

        [JsonPropertyName("_aChangeLog")]
        public GameBananaItemUpdateChange[] Changes { get; set; }

        [JsonPropertyName("_sText")]
        public string Text { get; set; }

        [JsonPropertyName("_tsDateAdded")]
        public long DateAddedLong { get; set; }

        [JsonIgnore]
        public DateTime DateAdded => Epoch.AddSeconds(DateAddedLong);
    }
    public class GameBananaItemUpdateChange
    {
        [JsonPropertyName("cat")]
        public string Category { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
    public class GameBananaRecord
    {
        [JsonPropertyName("_sName")]
        public string Title { get; set; }
        [JsonIgnore]
        public bool IsSpoiler => Title.ToUpperInvariant().StartsWith("(SPOILER)");
        [JsonPropertyName("_sProfileUrl")]
        public Uri Link { get; set; }
        [JsonPropertyName("_aAlternateFileSources")]
        public List<GameBananaAlternateFileSource> AlternateFileSources { get; set; }
        [JsonIgnore]
        public bool HasAltLinks => AlternateFileSources != null;
        [JsonIgnore]
        public Uri Image => Media.Where(x => x.Type == "image").ToList().Count > 0 ? new Uri($"{Media[0].Base}/{Media[0].File}") 
            : new Uri("https://images.gamebanana.com/static/img/DefaultEmbeddables/Sound.jpg");
        [JsonPropertyName("_aPreviewMedia")]
        public List<GameBananaImage> Media { get; set; }
        [JsonPropertyName("_sDescription")]
        public string Description { get; set; }
        [JsonIgnore]
        public bool HasDescription => Description.Length > 100;
        [JsonPropertyName("_sText")]
        public string Text { get; set; }
        [JsonIgnore]
        public string ConvertedText => ConvertHtmlToText(Text);
        [JsonPropertyName("_nViewCount")]
        public int Views { get; set; }
        [JsonPropertyName("_nLikeCount")]
        public int Likes { get; set; }
        [JsonPropertyName("_nDownloadCount")]
        public int Downloads { get; set; }
        [JsonIgnore]
        public string DownloadString => StringConverters.FormatNumber(Downloads);
        [JsonIgnore]
        public string ViewString => StringConverters.FormatNumber(Views);
        [JsonIgnore]
        public string LikeString => StringConverters.FormatNumber(Likes);
        [JsonPropertyName("_aSubmitter")]
        public GameBananaMember Owner { get; set; }
        [JsonPropertyName("_aFiles")]
        public List<GameBananaItemFile> AllFiles { get; set; }
        [JsonPropertyName("_aCategory")]
        public GameBananaCategory Category { get; set; }
        [JsonPropertyName("_aRootCategory")]
        public GameBananaCategory RootCategory { get; set; }
        [JsonIgnore]
        public string CategoryName => StringConverters.FormatSingular(RootCategory.Name, Category.Name);
        [JsonIgnore]
        public bool HasLongCategoryName => CategoryName.Length > 30;
        [JsonIgnore]
        public bool Compatible => AllFiles.Count > 0;

        [JsonPropertyName("_tsDateUpdated")]
        public long DateUpdatedLong { get; set; }
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1);

        [JsonIgnore]
        public DateTime DateUpdated => Epoch.AddSeconds(DateUpdatedLong);
        [JsonPropertyName("_tsDateAdded")]
        public long DateAddedLong { get; set; }

        [JsonIgnore]
        public DateTime DateAdded => Epoch.AddSeconds(DateAddedLong);
        [JsonIgnore]
        public string DateAddedFormatted => $"Added {StringConverters.FormatTimeAgo(DateTime.UtcNow - DateAdded)}";
        [JsonIgnore]
        public bool HasUpdates => DateAdded.CompareTo(DateUpdated) != 0;
        [JsonIgnore]
        public string DateUpdatedAgo => $"Updated {StringConverters.FormatTimeAgo(DateTime.UtcNow - DateUpdated)}";
        private string ConvertHtmlToText(string html)
        {
            // Newlines
            html = html.Replace("<br>", "\n");
            html = html.Replace(@"</li>", "\n");
            html = html.Replace(@"</h3>", "\n");
            html = html.Replace(@"</h2>", "\n");
            html = html.Replace(@"</h1>", "\n");
            html = html.Replace("<ul>", "\n");
            // Bullet point
            html = html.Replace("<li>", "• ");
            // Unique spaces
            html = html.Replace("&nbsp;", " ");
            html = html.Replace(@"\u00a0", " ");
            // Unique characters
            html = html.Replace("&amp;", "&");
            html = html.Replace("&gt;", ">");
            // Remove tabs
            html = html.Replace("\t", string.Empty);
            // Remove all unaccounted html tags
            html = Regex.Replace(html, "<.*?>", string.Empty);
            // Convert newlines of 3 or more to 2 newlines
            html = Regex.Replace(html, "[\\r\\n]{3,}", "\n\n", RegexOptions.Multiline);
            // Trim extra whitespace at start and end
            return html.Trim();
        }
        [JsonPropertyName("_bIsNsfw")]
        public bool IsNsfw { get; set; }
    }
    public class GameBananaModList
    {
        public ObservableCollection<GameBananaRecord> Records { get; set; }
        public double TotalPages { get; set; }
        public DateTime TimeFetched = DateTime.UtcNow;
        public bool IsValid => (DateTime.UtcNow - TimeFetched).TotalMinutes < 15;
    }
    public class GameBananaAlternateFileSource
    {
        [JsonPropertyName("url")]
        public Uri Url { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; } = "Mirror";
    }
    public class GameBananaImage
    {
        [JsonPropertyName("_sType")]
        public string Type { get; set; }
        [JsonPropertyName("_sUrl")]
        public Uri Audio { get; set; }
        [JsonPropertyName("_sBaseUrl")]
        public Uri Base { get; set; }
        [JsonPropertyName("_sFile")]
        public Uri File { get; set; }
        [JsonPropertyName("_sCaption")]
        public string Caption { get; set; }
    }
}
