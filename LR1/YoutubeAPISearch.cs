using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Net;

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
        private static string apiKey = File.ReadAllText("apiKey.txt");
        private static string jsonQwery = "https://youtube.googleapis.com/youtube/v3/videos?part=statistics&id=";
        private static YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = apiKey,
            ApplicationName = "MyName"
        });
        private static SearchResource.ListRequest searchListRequest = youtubeService.Search.List("snippet");
        public static List<string> channelId = new List<string>();
        public static List<List<string>> videos = new List<List<string>>();
        public static List<string> channel = new List<string>();
        public static List<string> LastVideosFromChannel;
        static DateTime date = DateTime.Now.Subtract(new DateTime(1, 1, 4, 0, 0, 0) - DateTime.MinValue);// дата 3 дня назад
        public static int viewCount = 1000, likeCount = 10;
        static string nextPageToken = " ";
        static string nextPageTokenChannel = " ";
        static string nextPageTokenChannelVideo = " ";
        [STAThread]


        /*static void Main(string[] args)
        {
            Console.WriteLine("YouTube Data API: Search");
            Console.WriteLine("========================");

            try
            {
                //int l, v = GetViewAndLikeCount(jsonqwery, "PSsjsqeYX_Y", out l);
                //Console.WriteLine(v + " " + l);
                string qwery = Console.ReadLine();
                videos.Add(new List<string>());
                Console.WriteLine("Видео:");
                for (int i = 0; i < 3; i++)
                {
                    SearchVideo(qwery, i).Wait();
                }
                foreach(List<string> v in videos)
                {
                    foreach(string s in v)
                        Console.WriteLine(s);
                    Console.WriteLine();
                    Console.WriteLine();
                }
                Console.WriteLine("Каналы:");
                for (int i = 0; i < 3; i++)
                {
                    SearchChannel(qwery, i).Wait();
                    Console.WriteLine(channel[i]);
                    //if (i < channel.Count)
                    //{


                    //}
                    Console.WriteLine();
                    Console.WriteLine();
                }
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        GetVideosFromChannelAsync(channelId[i], j).Wait();
                        for (int k = 0; k < rez.Count; k++)
                            Console.WriteLine(rez[k]);
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                }
                Console.WriteLine();
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
        }*/

        public static async Task SearchVideo(string qwery, int n,long viewCount, long likeCount)// поиск видео по запросу (результат в листе videos)
        {
            if (n == 0)
            {
                nextPageToken = " ";
            }
            videos.Add(new List<String>());
            searchListRequest.Q = qwery;
            searchListRequest.MaxResults = 5;
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
            //searchListRequest.PageToken = nextPageToken;
            searchListRequest.Type = "video";
            SearchListResponse searchListResponse;
            while (videos[videos.Count-1].Count <5)
            {
                searchListRequest.PageToken = nextPageToken;
                searchListResponse = await searchListRequest.ExecuteAsync();
                foreach (var searchResult in searchListResponse.Items)
                {
                    if (searchResult.Snippet.PublishedAt >= date)
                    {
                        int l, v = GetViewAndLikeCount(jsonQwery, searchResult.Id.VideoId, out l);
                        if (viewCount <= v && likeCount <= l && (!videos[videos.Count-1].Contains(String.Format
                                ("({1}), views = {2}, likes = {3} ", searchResult.Snippet.Title, 
                                "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId, v, l))))
                            videos[videos.Count - 1].Add(String.Format("({1}), views = {2}, likes = {3} ", searchResult.Snippet.Title, "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId, v, l));
                    }
                }
                nextPageToken = searchListResponse.NextPageToken;
            }
        }
        public static async Task<bool> SearchChannel(string qwery, int n)// поиск каналов по запросу (результат в листе channels, список id каналов для функции GetVideosFromChannelAsync находится в листе channelId)
        {
            bool hasFound = false;
            if (n == 0)
            {
                nextPageTokenChannel = " ";
            }
            searchListRequest.Q = qwery;
            searchListRequest.MaxResults = 1;
            searchListRequest.Order = null;
            searchListRequest.PageToken = nextPageTokenChannel;
            searchListRequest.Type = "channel";
            var searchListResponse = await searchListRequest.ExecuteAsync();
            foreach (var searchResult in searchListResponse.Items)
            {
                channel.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, "https://www.youtube.com/channel/" + searchResult.Id.ChannelId));
                channelId.Add(searchResult.Id.ChannelId);
                hasFound = true;
            }
            nextPageTokenChannel = searchListResponse.NextPageToken;
            return hasFound;
        }
        public static async Task GetVideosFromChannelAsync(string ytChannelId, int n, long views, long likes) //получение последних видео с канала по его id (результат в листе LastVideosFromChannel)
        {
            if (n == 0)
            {
                nextPageTokenChannelVideo = " ";
            }
            searchListRequest.ChannelId = ytChannelId;
            searchListRequest.MaxResults = 5;
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
            searchListRequest.PageToken = nextPageTokenChannelVideo;
            searchListRequest.Type = "video";
            var searchListResponse = await searchListRequest.ExecuteAsync();
            LastVideosFromChannel = new List<string>();
            foreach (var searchResult in searchListResponse.Items)
            {
                //if (searchResult.Snippet.PublishedAt >= date)
                {
                    int l, v = GetViewAndLikeCount(jsonQwery, searchResult.Id.VideoId, out l);
                    if (views <= v && likes <= l)
                        LastVideosFromChannel.Add(String.Format("{0} - {1}, views = {2}, likes = {3}", searchResult.Snippet.Title, "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId, v, l));
                }
            }
            nextPageTokenChannelVideo = searchListResponse.NextPageToken;
        }
        public static int GetViewAndLikeCount(string qwery, string videoId, out int likeCount) //запрос на получение количества лайков и просмотров у видео
        {
            string sURL = qwery + videoId + "&key=" + apiKey;
            WebRequest wrGETURL = WebRequest.Create(sURL);
            Stream objStream = wrGETURL.GetResponse().GetResponseStream();
            StreamReader objReader = new StreamReader(objStream);
            string sLine = "";
            int i = 0;
            string view = "", like = "";
            while (sLine != null)
            {
                i++;
                sLine = objReader.ReadLine();
                if (sLine != null && (sLine.Contains("viewCount")))
                {
                    view = sLine.Split(':')[1];
                    view = DeleteExcessCharacters(view);
                    sLine = objReader.ReadLine();
                    like = sLine.Split(':')[1];
                    like = DeleteExcessCharacters(like);
                    break;
                }
            }
            objReader.Close();
            objReader.Dispose();
            objStream.Close();
            objStream.Dispose();
            viewCount = int.Parse(view);
            likeCount = int.Parse(like);
            return viewCount;
        }
        public static string DeleteExcessCharacters(string s)
        {
            s = s.Replace(" ", "");
            s = s.Replace("\"", "");
            s = s.Replace(",", "");
            return s;
        }
    }
}