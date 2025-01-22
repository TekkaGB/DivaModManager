using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Reflection;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Input;

namespace DivaModManager
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class FetchWindow : Window
    {
        public bool success;
        public Mod _mod;
        public FetchWindow(Mod mod)
        {
            InitializeComponent();
            _mod = mod;
            Title = $"Fetch Metadata for {_mod.name}";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private Uri CreateUri(string url)
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
        private Uri DMACreateUri(string url)
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
                    case "www.divamodarchive.com":
                    case "divamodarchive.com":
                        return uri;
                }
            }
            return null;
        }

        private async void Fetch()
        {
            Uri url = CreateUri(UrlBox.Text);
            if (url != null)
            {
                try
                {
                    var MOD_TYPE = char.ToUpper(url.Segments[1][0]) + url.Segments[1].Substring(1, url.Segments[1].Length - 3);
                    var MOD_ID = url.Segments[2];
                    var client = new HttpClient();
                    var requestUrl = $"https://gamebanana.com/apiv6/{MOD_TYPE}/{MOD_ID}?_csvProperties=_aSubmitter,_sDescription,_aPreviewMedia,_sProfileUrl," +
                        $"_aSuperCategory,_aCategory,_tsDateUpdated";
                    string responseString = await client.GetStringAsync(requestUrl);
                    var record = JsonSerializer.Deserialize<GameBananaAPIV4>(responseString);
                    var metadata = new Metadata();
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
                    File.WriteAllText($@"{Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{_mod.name}{Global.s}mod.json", metadataString);
                    success = true;
                    Close();
                    return;
                }
                catch (Exception ex)
                {
                    Global.logger.WriteLine(ex.Message, LoggerType.Error);
                }
            }
            url = DMACreateUri(UrlBox.Text);
            if (url != null)
            {
                var post_id = url.ToString().Split('/').Last();
                var DMAclient = new HttpClient();
                var requestUrl = $"https://divamodarchive.com/api/v1/posts/{post_id}";
                string responseString = await DMAclient.GetStringAsync(requestUrl);
                var post = JsonSerializer.Deserialize<DivaModArchivePost>(responseString);
                var metadata = new Metadata();
                metadata.id = post.ID;
                metadata.submitter = post.Authors[0].Name;
                metadata.description = post.Text;
                metadata.preview = post.Images[0];
                metadata.homepage = post.Link;
                metadata.avi = post.Authors[0].Avatar;
                metadata.cat = post.PostType;
                metadata.lastupdate = post.Time;
                string metadataString = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText($@"{Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{_mod.name}{Global.s}mod.json", metadataString);
                success = true;
                Close();
                return;
            }
            else
                Global.logger.WriteLine($"{UrlBox.Text} is invalid. The url should have the following format: https://gamebanana.com/<Mod Category>/<Mod ID> or https://divamodarchive.com/post/<Post ID>", LoggerType.Error);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Fetch();
        }

        private void UrlBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                Fetch();
        }
    }
}
