using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeAPISearch
{
    /// <summary>
    /// YouTube Data API v3 sample: search by keyword.
    /// Relies on the Google APIs Client Library for .NET, v1.7.0 or higher.
    /// See https://developers.google.com/api-client-library/dotnet/get_started
    ///
    /// Set ApiKey to the API key value from the APIs & auth > Registered apps tab of
    /// https://cloud.google.com/console
    /// Please ensure that you have enabled the YouTube Data API for your project.
    /// </summary>
    internal class Search
    {
        private static string apiKey = "AIzaSyDgeGRlSZ2Gub4tDxo3cE3tEn3KiYhDRCY";
        static List<string> chanelId = new List<string>();
        static List<string> rez = new List<string>();
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("YouTube Data API: Search");
            Console.WriteLine("========================");

            try
            {
                Search s = new Search();
                string qwery = Console.ReadLine();
                s.Run(qwery).Wait();
                s.GetVideosFromChannelAsync(chanelId[0]).Wait();
                Console.WriteLine("Videos from chanel "+qwery);
                for (int i = 0; i < rez.Count; i++)
                {
                    Console.WriteLine(rez[i]);
                }
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        public Task<List<SearchResult>> GetVideosFromChannelAsync(string ytChannelId) //показывает видео с канала, загруженные не позже трех дней назад
        {

            return Task.Run(() =>
            {
                List<SearchResult> res = new List<SearchResult>();
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = apiKey,
                    ApplicationName = this.GetType().ToString()
                });
                string nextpagetoken = " ";

                while (nextpagetoken != null)
                {
                    var searchListRequest = youtubeService.Search.List("snippet");
                    searchListRequest.MaxResults = 500;
                    searchListRequest.ChannelId = ytChannelId;
                    searchListRequest.PageToken = nextpagetoken;
                    searchListRequest.Type = "video";
                    
                    

                    // Call the search.list method to retrieve results matching the specified query term.
                    var searchListResponse = searchListRequest.Execute();
                    DateTime date = DateTime.Now.Subtract(new DateTime(1, 1, 4, 0, 0, 0) - DateTime.MinValue); // дата 3 дня назад
                    //Console.WriteLine(date);
                    // Process  the video responses 
                    res.AddRange(searchListResponse.Items);
                    foreach(var searchRez in searchListResponse.Items)
                    {
                        if(searchRez.Snippet.PublishedAt>date)
                        rez.Add(String.Format("{0} - {1} ", searchRez.Snippet.Title, "https://www.youtube.com/watch?v=" + searchRez.Id.VideoId));
                    }
                    nextpagetoken = searchListResponse.NextPageToken;

                }

                return res;

            });
        }


        private async Task Run(string qwery)// поиск каналов и видео по запросу
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = qwery; // Replace with your search term.
            searchListRequest.MaxResults = 500;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            List<string> videos = new List<string>();
            List<string> channels = new List<string>();
            List<string> playlists = new List<string>();
            DateTime date = DateTime.Now.Subtract(new DateTime(1, 1, 4, 0, 0, 0) - DateTime.MinValue);// дата 3 дня назад
            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        if (searchResult.Snippet.PublishedAt > date)
                            videos.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.VideoId));
                        break;

                    case "youtube#channel":
                        channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
                        chanelId.Add(searchResult.Id.ChannelId);
                        break;

                    //case "youtube#playlist":
                    //    playlists.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.PlaylistId));
                    //    break;
                }
            }
            
            Console.WriteLine(String.Format("Channels:\n{0}\n", string.Join("\n", channels)));
            Console.WriteLine(String.Format("Videos:\n{0}\n", string.Join("\n", videos)));
            
           // Console.WriteLine(String.Format("Playlists:\n{0}\n", string.Join("\n", playlists)));
        }

        private async Task Subscribe(string ytChanelId) //подписка (решено не использовать)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = this.GetType().ToString()
            });


            try
            {
                Subscription body = new Subscription();
                body.Snippet = new SubscriptionSnippet();
                body.Snippet.ResourceId = new ResourceId();
                body.Snippet.ResourceId.ChannelId = ytChanelId;  //replace with specified channel id

                var addSubscriptionRequest = youtubeService.Subscriptions.Insert(body, "snippet");
                var addSubscriptionResponse = await addSubscriptionRequest.ExecuteAsync();

            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}