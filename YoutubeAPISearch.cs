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
        private static string apiKey = File.ReadAllText("apiKey.txt");//"AIzaSyDgeGRlSZ2Gub4tDxo3cE3tEn3KiYhDRCY";//"AIzaSyDEJSX2wpoSvjxHNvKuVa5u9o1QV4u4QVo";
        private static string jsonqwery = "https://youtube.googleapis.com/youtube/v3/videos?part=statistics&id=";//Ys7-6_t7OEQ&key=+apiKey;
        static List<string> chanelId = new List<string>();
        public static List<string> videos;
        public static List<string> channels;
        static List<string> rez = new List<string>();
        static int viewCount = 10, likeCount = 10;
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("YouTube Data API: Search");
            Console.WriteLine("========================");

            try
            {
                //int l, v = GetViewAndLikeCount(jsonqwery, "PSsjsqeYX_Y", out l);
                //Console.WriteLine(v + " " + l);

                Search s = new Search();
                string qwery = Console.ReadLine();
                s.Run(qwery).Wait();
                Console.WriteLine("Каналы:");
                foreach (string c in channels)
                {
                    Console.WriteLine(c);
                }
                Console.WriteLine();
                Console.WriteLine("Видео:");
                foreach (string v in videos)
                {
                    Console.WriteLine(v);
                }
                s.GetVideosFromChannelAsync(chanelId[0]).Wait();
                Console.WriteLine();
                Console.WriteLine("Видео с канала " + qwery + ":");
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
        public static int GetViewAndLikeCount(string qwery, string videoId, out int likeCount)
        {
            string sURL;
            sURL = qwery + videoId + "&key=" + apiKey;

            WebRequest wrGETURL;
            wrGETURL = WebRequest.Create(sURL);

            Stream objStream;
            objStream = wrGETURL.GetResponse().GetResponseStream();

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
        async Task GetVideosFromChannelAsync(string ytChannelId)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.ChannelId = ytChannelId;
            searchListRequest.MaxResults = 15;
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
            var searchListResponse = await searchListRequest.ExecuteAsync();
            DateTime date = DateTime.Now.Subtract(new DateTime(1, 1, 4, 0, 0, 0) - DateTime.MinValue); // дата 3 дня назад
            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":

                        if (searchResult.Snippet.PublishedAt > date)
                        {
                            int l, v = GetViewAndLikeCount(jsonqwery, searchResult.Id.VideoId, out l);
                            if (viewCount <= v && likeCount <= l)
                                rez.Add(String.Format("{0} - {1}, views = {2}, likes = {3}", searchResult.Snippet.Title, "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId, v, l));
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        public async Task Run(string qwery)// поиск каналов и видео по запросу
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = qwery; // Replace with your search term.
            searchListRequest.MaxResults = 50;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            chanelId = new List<string>();
            videos = new List<string>();
            channels = new List<string>();
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
                        {
                            int l, v = GetViewAndLikeCount(jsonqwery, searchResult.Id.VideoId, out l);
                            if (viewCount <= v && likeCount <= l)
                                videos.Add(String.Format("{0} ({1}), views = {2}, likes = {3} ", searchResult.Snippet.Title, "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId, v, l));


                        }
                        break;

                    case "youtube#channel":
                        channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, "https://www.youtube.com/channel/" + searchResult.Id.ChannelId));
                        chanelId.Add(searchResult.Id.ChannelId);
                        break;
                }
            }

            //Console.WriteLine(String.Format("Channels:\n{0}\n", string.Join("\n", channels)));
            //Console.WriteLine(String.Format("Videos:\n{0}\n", string.Join("\n", videos)));
        }
    }
}