using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Net.Http;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DivaModManager
{
    public enum GameFilter
    {
        MMP
    }
    public enum FeedFilter
    {
        Featured,
        Recent,
        Popular,
        None
    }
    public enum TypeFilter
    {
        Mods,
        WiPs,
        Sounds
    }
    public static class FeedGenerator
    {
        private static Dictionary<string, GameBananaModList> feed;
        public static bool error;
        public static Exception exception;
        public static GameBananaModList CurrentFeed;
        public static double GetHeader(this HttpResponseMessage request, string key)
        {
            IEnumerable<string> keys = null;
            if (!request.Headers.TryGetValues(key, out keys))
                return -1;
            return Double.Parse(keys.First());
        }
        public static void ClearCache()
        {
            if (feed != null)
                feed.Clear();
        }
        public static async Task GetFeed(int page, GameFilter game, TypeFilter type, FeedFilter filter, GameBananaCategory category, GameBananaCategory subcategory, int perPage, bool nsfw, string search)
        {
            error = false;
            if (feed == null)
                feed = new Dictionary<string, GameBananaModList>();
            // Remove oldest key if more than 15 pages are cached
            if (feed.Count > 15)
                feed.Remove(feed.Aggregate((l, r) => DateTime.Compare(l.Value.TimeFetched, r.Value.TimeFetched) < 0 ? l : r).Key);
            using (var httpClient = new HttpClient())
            {
                var requestUrl = GenerateUrl(page, game, type, filter, category, subcategory, perPage, nsfw, search);
                if (feed.ContainsKey(requestUrl) && feed[requestUrl].IsValid)
                {
                    CurrentFeed = feed[requestUrl];
                    return;
                }
                CurrentFeed = new();
                try
                {
                    var response = await httpClient.GetAsync(requestUrl);
                    var records = JsonSerializer.Deserialize<ObservableCollection<GameBananaRecord>>(await response.Content.ReadAsStringAsync());
                    CurrentFeed.Records = records;
                    // Get record count from header
                    var numRecords = response.GetHeader("X-GbApi-Metadata_nRecordCount");
                    if (numRecords != -1)
                    {
                        var totalPages = Math.Ceiling(numRecords / Convert.ToDouble(perPage));
                        if (totalPages == 0)
                            totalPages = 1;
                        CurrentFeed.TotalPages = totalPages;
                    }
                }
                catch (Exception e)
                {
                    error = true;
                    exception = e;
                    return;
                }
                if (!feed.ContainsKey(requestUrl))
                    feed.Add(requestUrl, CurrentFeed);
                else
                    feed[requestUrl] = CurrentFeed;
            }
        }
        private static string GenerateUrl(int page, GameFilter game, TypeFilter type, FeedFilter filter, GameBananaCategory category, GameBananaCategory subcategory, int perPage, bool nsfw, string search)
        {
            // Base
            var url = "https://gamebanana.com/apiv6/";
            switch (type)
            {
                case TypeFilter.Mods:
                    url += "Mod/";
                    break;
                case TypeFilter.Sounds:
                    url += "Sound/";
                    break;
                case TypeFilter.WiPs:
                    url += "Wip/";
                    break;
            }
            // Different starting endpoint if requesting all mods instead of specific category
            if (search != null)
            {
                url += $"ByName?_sName=*{search}*&_idGameRow=";
                switch (game)
                {
                    case GameFilter.MMP:
                        url += "16522&";
                        break;
                }
            }
            else if (category.ID != null)
                url += "ByCategory?";
            else
            {
                url += $"ByGame?_aGameRowIds[]=";
                switch (game)
                {
                    case GameFilter.MMP:
                        url += "16522&";
                        break;
                }
            }
            // Consistent args
            url += $"_csvProperties=_sName,_sModelName,_sProfileUrl,_aSubmitter,_tsDateUpdated,_tsDateAdded,_aPreviewMedia,_sText,_sDescription,_aCategory,_aRootCategory,_aGame,_nViewCount," +
                $"_nLikeCount,_nDownloadCount,_aFiles,_aModManagerIntegrations,_bIsNsfw,_aAlternateFileSources&_nPerpage={perPage}";
            if (!nsfw)
                url += "&_aArgs[]=_sbIsNsfw = false";
            // Sorting filter
            switch (filter)
            {
                case FeedFilter.Recent:
                    url += "&_sOrderBy=_tsDateUpdated,DESC";
                    break;
                case FeedFilter.Featured:
                    url += "&_aArgs[]=_sbWasFeatured = true& _sOrderBy=_tsDateAdded,DESC";
                    break;
                case FeedFilter.Popular:
                    url += "&_sOrderBy=_nDownloadCount,DESC";
                    break;
            }
            // Choose subcategory or category
            if (subcategory.ID != null)
                url += $"&_aCategoryRowIds[]={subcategory.ID}";
            else if (category.ID != null)
                url += $"&_aCategoryRowIds[]={category.ID}";
            
            // Get page number
            url += $"&_nPage={page}";
            return url;
        }
    }
}
