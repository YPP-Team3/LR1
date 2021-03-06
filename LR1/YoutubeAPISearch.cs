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
        private static string apiKey = /*"AIzaSyDEJSX2wpoSvjxHNvKuVa5u9o1QV4u4QVo";*/File.ReadAllText("apiKey.txt");
        private static string jsonqwery = "https://youtube.googleapis.com/youtube/v3/videos?part=statistics&id=";
        private static YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = apiKey,
            ApplicationName = "MyName"
        });
        private static int prevVideoPage = 0;
        private static int pageSize = 5;
        private static SearchResource.ListRequest searchListRequest = youtubeService.Search.List("snippet");
        public static List<string> channelId = new List<string>();
        public static List<string> videos;
        public static List<string> channel = new List<string>();
        public static List<string> result;
        static DateTime date = DateTime.Now.Subtract(new DateTime(1, 1, 4, 0, 0, 0) - DateTime.MinValue);// дата 3 дня назад
        static int viewCount = 1000, likeCount = 10;
        static string nextpagetoken = " ";
        static string prevPageToken = " ";
        static string nextpagetokenChannel = " ";
        static string nextpagetokenChannelVideo = " ";
        [STAThread]


        //static void Main(string[] args)
        //{
        //    Console.WriteLine("YouTube Data API: Search");
        //    Console.WriteLine("========================");

        //    try
        //    {
        //        //int l, v = GetViewAndLikeCount(jsonqwery, "PSsjsqeYX_Y", out l);
        //        //Console.WriteLine(v + " " + l);
        //        string qwery = Console.ReadLine();
        //        Console.WriteLine("Видео:");
        //        for (int i = 0; i < 3; i++)
        //        {
        //            SearchVideo(qwery, i).Wait();
        //            foreach (string v in videos)
        //            {
        //                Console.WriteLine(v);
        //            }
        //            Console.WriteLine();
        //            Console.WriteLine();
        //        }
        //        Console.WriteLine("Каналы:");
        //        for (int i = 0; i < 3; i++)
        //        {
        //            SearchChannel(qwery, i).Wait();
        //            Console.WriteLine(channel[i]);
        //            Console.WriteLine();
        //            Console.WriteLine();
        //        }
        //        for (int i = 0; i < 3; i++)
        //        {
        //            for (int j = 0; j < 2; j++)
        //            {
        //                GetVideosFromChannelAsync(channelId[i], j).Wait();
        //                for (int k = 0; k < result.Count; k++)
        //                    Console.WriteLine(result[k]);
        //                Console.WriteLine();
        //            }
        //            Console.WriteLine();
        //            Console.WriteLine();
        //        }
        //        Console.WriteLine();
        //        //string token = File.ReadAllText("token.txt");

        //        //BackgroundWorker bw = new BackgroundWorker();
        //        //bw.DoWork += new DoWorkEventHandler(TelegramBot.bw_DoWork);

        //        //if (bw.IsBusy != true)
        //        //{
        //        //    bw.RunWorkerAsync(token);
        //        //}
        //        //Console.ReadLine();
        //    }
        //    catch (AggregateException ex)
        //    {
        //        foreach (var e in ex.InnerExceptions)
        //        {
        //            Console.WriteLine("Error: " + e.Message);
        //        }
        //    }

        //    Console.WriteLine("Press any key to continue...");
        //    Console.ReadKey();
        //}

        public static async Task SearchVideo(string qwery,
            int ytVideoPage, long numOfLikes, long numOfViews)// поиск видео по запросу (результат в листе videos)
        {
            if (ytVideoPage == 0)
            {
                nextpagetoken = " ";
            }

            if (ytVideoPage < prevVideoPage)
                nextpagetoken = prevPageToken;
            int iterator = 0;
            for (int i = 0; i <= 0/*ytVideoPage*/; i++)
            {
                videos = new List<string>();
                searchListRequest.Q = qwery;
                searchListRequest.MaxResults = 5;
                searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
                searchListRequest.PageToken = nextpagetoken;
                searchListRequest.Type = "video";
                var searchListResponse = await searchListRequest.ExecuteAsync();
                foreach (var searchResult in searchListResponse.Items)
                {
                    if (searchResult.Snippet.PublishedAt >= date)
                    {
                        long l, v = GetViewAndLikeCount(jsonqwery, searchResult.Id.VideoId, out l);
                        if (numOfViews <= v && numOfLikes <= l)
                        {
                            videos.Add(String.Format("({1}), views = {2}, likes = {3} ", searchResult.Snippet.Title,
                                "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId, v, l));
                        }
                    }
                }
                prevPageToken = nextpagetoken;
                nextpagetoken = searchListResponse.NextPageToken;
            }

            prevVideoPage = ytVideoPage;
        }
        public static async Task SearchChannel(string qwery, int n)// поиск каналов по запросу (результат в листе channels, список id каналов для функции GetVideosFromChannelAsync находится в листе channelId)
        {
            if (n == 0)
            {
                nextpagetokenChannel = " ";
            }
            searchListRequest.Q = qwery;
            searchListRequest.MaxResults = 1;
            searchListRequest.Order = null;
            searchListRequest.PageToken = nextpagetokenChannel;
            searchListRequest.Type = "channel";
            var searchListResponse = await searchListRequest.ExecuteAsync();
            foreach (var searchResult in searchListResponse.Items)
            {
                channel.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, "https://www.youtube.com/channel/" + searchResult.Id.ChannelId));
                channelId.Add(searchResult.Id.ChannelId);
            }
            nextpagetokenChannel = searchListResponse.NextPageToken;
        }
        public static async Task GetVideosFromChannelAsync(string ytChannelId, int n) //получение последних видео с канала по его id (результат в листе result)
        {
            if (n == 0)
            {
                nextpagetokenChannelVideo = " ";
            }
            searchListRequest.ChannelId = ytChannelId;
            searchListRequest.MaxResults = 5;
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
            searchListRequest.PageToken = nextpagetokenChannelVideo;
            searchListRequest.Type = "video";
            var searchListResponse = await searchListRequest.ExecuteAsync();
            result = new List<string>();
            foreach (var searchResult in searchListResponse.Items)
            {
                //if (searchResult.Snippet.PublishedAt >= date)
                {
                    long l, v = GetViewAndLikeCount(jsonqwery, searchResult.Id.VideoId, out l);
                    if (viewCount <= v && likeCount <= l)
                        result.Add(String.Format("{0} - {1}, views = {2}, likes = {3}", searchResult.Snippet.Title, "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId, v, l));
                }
            }
            nextpagetokenChannelVideo = searchListResponse.NextPageToken;
        }
        public static int GetViewAndLikeCount(string qwery, string videoId, out long likeCount) //запрос на получение количества лайков и просмотров у видео
        {
            string sURL = qwery + videoId + "&key=" + apiKey;
            WebRequest wrGETURL = WebRequest.Create(sURL);
            Stream objStream = wrGETURL.GetResponse().GetResponseStream();
            StreamReader objReader = new StreamReader(objStream);
            string sLine = "";
            int i = 0;
            string view = "", like = "";
            int viewCount = 0; likeCount = 0;
            while (sLine != null)
            {
                i++;
                sLine = objReader.ReadLine();
                if (sLine != null && (sLine.Contains("viewCount")))
                {
                    view = sLine.Split(':')[1];
                    view = view.Replace(" ", "");
                    view = view.Replace("\"", "");
                    view = view.Replace(",", "");
                    sLine = objReader.ReadLine();
                    like = sLine.Split(':')[1];
                    like = like.Replace(" ", "");
                    like = like.Replace("\"", "");
                    like = like.Replace(",", "");
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
    }
}