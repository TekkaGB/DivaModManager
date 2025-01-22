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
    public enum DMAFeedFilter
    {
        Latest,
        Popular
    }
    public static class DMAFeedGenerator
    {
        private static Dictionary<string, DivaModArchiveModList> feed;
        public static bool error;
        public static Exception exception;
        public static DivaModArchiveModList CurrentFeed;
        public static void ClearCache()
        {
            if (feed != null)
                feed.Clear();
        }
        public static async Task GetFeed(int page, DMAFeedFilter filter, string search, int limit)
        {
            error = false;
            if (feed == null)
                feed = new();
            // Remove oldest key if more than 15 pages are cached
            if (feed.Count > 15)
                feed.Remove(feed.Aggregate((l, r) => DateTime.Compare(l.Value.TimeFetched, r.Value.TimeFetched) < 0 ? l : r).Key);
            using (var httpClient = new HttpClient())
            {
                var requestUrl = GenerateUrl(page, filter, search, limit);
                if (feed.ContainsKey(requestUrl) && feed[requestUrl].IsValid)
                {
                    CurrentFeed = feed[requestUrl];
                    return;
                }
                CurrentFeed = new();
                try
                {
                    var response = await httpClient.GetAsync(requestUrl);
                    var posts = JsonSerializer.Deserialize<ObservableCollection<DivaModArchivePost>>(await response.Content.ReadAsStringAsync());
                    CurrentFeed.Posts = posts;
                    response = await httpClient.GetAsync($"https://divamodarchive.xyz/api/v1/posts/post_count?name={search}");
                    var numPosts = Double.Parse(await response.Content.ReadAsStringAsync());
                    var totalPages = Math.Ceiling(numPosts / limit);
                    if (totalPages == 0)
                        totalPages = 1;
                    CurrentFeed.TotalPages = totalPages;
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
        private static string GenerateUrl(int page, DMAFeedFilter filter, string search, int limit)
        {
            // Base
            var url = "https://divamodarchive.xyz/api/v1/posts/";
            switch (filter)
            {
                case DMAFeedFilter.Latest:
                    url += "latest";
                    break;
                case DMAFeedFilter.Popular:
                    url += "popular";
                    break;
            }
            url += $"?name={search}";
            var offset = (page - 1) * limit;
            url += $"&offset={offset}";
            url += $"&limit={limit}";
            return url;
        }
    }
}
