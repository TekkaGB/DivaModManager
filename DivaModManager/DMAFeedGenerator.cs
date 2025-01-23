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
    public enum DMAFeedSort
    {
        Latest,
        Downloads,
        Likes,
    }
    public enum DMAFeedFilter
    {
        None,
        Song,
        Cover,
        Module,
        Ui,
        Plugin,
        Other
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
        public static async Task GetFeed(int page, DMAFeedSort sort, DMAFeedFilter filter, string search, int limit)
        {
            error = false;
            if (feed == null)
                feed = new();
            // Remove oldest key if more than 15 pages are cached
            if (feed.Count > 15)
                feed.Remove(feed.Aggregate((l, r) => DateTime.Compare(l.Value.TimeFetched, r.Value.TimeFetched) < 0 ? l : r).Key);
            using (var httpClient = new HttpClient())
            {
                var requestUrl = GenerateUrl(page, sort, filter, search, limit);
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
                    response = await httpClient.GetAsync($"https://divamodarchive.com/api/v1/posts/count?query={search}&limit={limit}");
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
        private static string GenerateUrl(int page, DMAFeedSort sort, DMAFeedFilter filter, string search, int limit)
        {
            // Base
            var url = "https://divamodarchive.com/api/v1/posts?sort=";
            switch (sort)
            {
                case DMAFeedSort.Latest:
                    url += "time:desc";
                    break;
                case DMAFeedSort.Downloads:
                    url += "download_count:desc";
                    break;
                case DMAFeedSort.Likes:
                    url += "like_count:desc";
                    break;
            }
            switch (filter)
            {
                case DMAFeedFilter.Song:
                    url += "&filter=post_type=Song";
                    break;
                case DMAFeedFilter.Cover:
                    url += "&filter=post_type=Cover";
                    break;
                case DMAFeedFilter.Module:
                    url += "&filter=post_type=Module";
                    break;
                case DMAFeedFilter.Ui:
                    url += "&filter=post_type=UI";
                    break;
                case DMAFeedFilter.Plugin:
                    url += "&filter=post_type=Plugin";
                    break;
                case DMAFeedFilter.Other:
                    url += "&filter=post_type=Other";
                    break;
            }
            url += $"&query={search}";
            var offset = (page - 1) * limit;
            url += $"&offset={offset}";
            url += $"&limit={limit}";
            return url;
        }
    }
}
