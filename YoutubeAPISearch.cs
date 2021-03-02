﻿using System;
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
        private static string jsonqwery = "https://youtube.googleapis.com/youtube/v3/videos?part=statistics&id=";//Ys7-6_t7OEQ&key=+apiKey;
        private static YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = apiKey,
            ApplicationName = "MyName"
        });
        private static SearchResource.ListRequest searchListRequest = youtubeService.Search.List("snippet");
        public static List<string> channelId;
        public static List<string> videos;
        public static List<string> channels;
        public static List<string> rez;
        static DateTime date = DateTime.Now.Subtract(new DateTime(1, 1, 4, 0, 0, 0) - DateTime.MinValue);// дата 3 дня назад
        static int viewCount = 1000, likeCount = 10;
        [STAThread]


        static void Main(string[] args)
        {
            Console.WriteLine("YouTube Data API: Search");
            Console.WriteLine("========================");

            try
            {
                //int l, v = GetViewAndLikeCount(jsonqwery, "PSsjsqeYX_Y", out l);
                //Console.WriteLine(v + " " + l);
                string qwery = Console.ReadLine();
                SearchVideo(qwery).Wait();
                Console.WriteLine("Видео:");
                foreach (string v in videos)
                {
                    Console.WriteLine(v);
                }
                Console.WriteLine();
                SearchChannel(qwery).Wait();
                Console.WriteLine("Каналы:");
                for (int i = 0; i < channels.Count; i++)
                {
                    Console.WriteLine(i + " " + channels[i]);
                }
                GetVideosFromChannelAsync(channelId[0]).Wait();
                Console.WriteLine();
                Console.WriteLine("Видео с канала " + channels[0].Split('(')[0] + ":");//видео с нулевого по номеру канала
                for (int i = 0; i < rez.Count; i++)
                {
                    Console.WriteLine(rez[i]);
                }

                //string token = File.ReadAllText("token.txt");

                //BackgroundWorker bw = new BackgroundWorker();
                //bw.DoWork += new DoWorkEventHandler(TelegramBot.bw_DoWork);

                //if (bw.IsBusy != true)
                //{
                //    bw.RunWorkerAsync(token);
                //}
                //Console.ReadLine();
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

        public static async Task SearchVideo(string qwery)// поиск видео по запросу (результат в листе videos)
        {
            searchListRequest.Q = qwery;
            searchListRequest.MaxResults = 500;
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
            var searchListResponse = await searchListRequest.ExecuteAsync();
            videos = new List<string>();
            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Id.Kind == "youtube#video")
                {
                    if (searchResult.Snippet.PublishedAt >= date)
                    {
                        int l, v = GetViewAndLikeCount(jsonqwery, searchResult.Id.VideoId, out l);
                        if (viewCount <= v && likeCount <= l)
                            videos.Add(String.Format("{0} ({1}), views = {2}, likes = {3} ", searchResult.Snippet.Title, "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId, v, l));
                    }
                }
            }
        }
        public static async Task SearchChannel(string qwery)// поиск каналов по запросу (результат в листе channels, список id каналов для функции GetVideosFromChannelAsync находится в листе channelId)
        {
            searchListRequest.Q = qwery;
            searchListRequest.MaxResults = 50;
            searchListRequest.Order = null;
            var searchListResponse = await searchListRequest.ExecuteAsync();
            channelId = new List<string>();
            channels = new List<string>();
            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Id.Kind == "youtube#channel")
                {
                    channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, "https://www.youtube.com/channel/" + searchResult.Id.ChannelId));
                    channelId.Add(searchResult.Id.ChannelId);
                }
            }
        }
        public static async Task GetVideosFromChannelAsync(string ytChannelId) //получение последних видео с канала по его id (результат в листе rez)
        {
            searchListRequest.ChannelId = ytChannelId;
            searchListRequest.MaxResults = 100;
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
            var searchListResponse = await searchListRequest.ExecuteAsync();
            rez = new List<string>();
            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Id.Kind == "youtube#video")
                {
                    if (searchResult.Snippet.PublishedAt >= date)
                    {
                        int l, v = GetViewAndLikeCount(jsonqwery, searchResult.Id.VideoId, out l);
                        if (viewCount <= v && likeCount <= l)
                            rez.Add(String.Format("{0} - {1}, views = {2}, likes = {3}", searchResult.Snippet.Title, "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId, v, l));
                    }
                }
            }
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