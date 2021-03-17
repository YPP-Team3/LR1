using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;
using YoutubeAPISearch;
using TextConstPack;
using LanguagesEnum;

namespace TelegramBot
{

    class Program
    {
        private static Telegram.Bot.TelegramBotClient Bot;

        /// <summary>
        /// Хранит текущие состояния бота
        /// </summary>
        private static Dictionary<long, BotStage> BotStages;

        private static Dictionary<string, UserFilter> UserFilters;
        private static TextPack[] UsedTextPacks = TextPackData.AvailablePacks;

        private static TextPack CurrentTextPack;

        private const string BotStagesJsonPath = "./BotStages.json";
        private const string UserFiltersJsonPath = "./UserFilters.json";

        private static int YTVideoPage = 0;
        private static int YTChannelPage = 0;
        private static int YTChannelVideoPage = 0;
        private static string YTSearchQuery = "";
        private static string YTSearchChannelQuery = "";
        private static bool CanChangeQuery;
        private static bool CanChangeChannelQuery;

        public static BotStage CurrentBotStage;
        public enum BotStage
        {
            ChoosingLanguage,
            ChoosingPlatform, ChoosingYTFunction, ChoosingYTFilters,
            SettingYTLikes, SettingYTViews,
            SearchingYTChannel, SearchingYTVideo,
            ChoosingYTVideos, ChoosingYTChannels, ChoosingYTChannelVideos
        }

        private static async Task ChangeBotStage(long chatId, BotStage nextBotStage)
        {
            if (BotStages.ContainsKey(chatId))
            {
                BotStages[chatId] = nextBotStage;
            }
            else
            {
                BotStages.Add(chatId, nextBotStage);
            }
            await JsonReadWrite.WriteJsonAsync(BotStagesJsonPath, BotStages);
        }
        private static async Task UpdateUserFilters()
        {
            await JsonReadWrite.WriteJsonAsync(UserFiltersJsonPath, UserFilters);
        }

        public static void Main()
        {
            MainTask().GetAwaiter().GetResult();
        }
        public static async Task MainTask()
        {
            CurrentBotStage = BotStage.ChoosingPlatform;
            if (!File.Exists(BotStagesJsonPath)) File.Create(BotStagesJsonPath);
            if ((await JsonReadWrite.ReadBotStagesJsonAsync(BotStagesJsonPath)).Count != 0)
                BotStages = await JsonReadWrite.ReadBotStagesJsonAsync(BotStagesJsonPath);
            else BotStages = new Dictionary<long, BotStage>();

            if (!File.Exists(UserFiltersJsonPath)) File.Create(UserFiltersJsonPath);
            if (File.ReadAllText(UserFiltersJsonPath).Length != 0)
                UserFilters = await JsonReadWrite.ReadUserFiltersJsonAsync(UserFiltersJsonPath);
            else UserFilters = new Dictionary<string, UserFilter>();


            CurrentTextPack = UsedTextPacks.First(x => x.Name == "English");

            string token = System.IO.File.ReadAllText($"./token.txt");
            Bot = new TelegramBotClient(token);
            User me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;


            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");

            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            Message message = messageEventArgs.Message;
            long chatId = message.Chat.Id;
            if (!UserFilters.ContainsKey(chatId.ToString()))
            {
                UserFilters.Add(chatId.ToString(), new UserFilter(chatId));
            }
            if (!BotStages.ContainsKey(chatId))
            {
                await ChangeBotStage(chatId, BotStage.ChoosingPlatform);
            }
            CurrentBotStage = BotStages[chatId];

            if (message == null || message.Type != MessageType.Text)
            {
                return;
            }


            if (UserFilters[chatId.ToString()].UserLanguage == "Undefined")
            {
                await ChooseLanguageAction(message);
                await ChangeBotStage(chatId, BotStage.ChoosingLanguage);
            }
            else
            {
                CurrentTextPack =
                    UsedTextPacks.First(x => x.Name == UserFilters[chatId.ToString()].UserLanguage);
            }
            switch (BotStages[message.Chat.Id])
            {
                case BotStage.ChoosingLanguage:
                    {
                        if (UsedTextPacks.Any(x => x.Name == message.Text))
                        {
                            UserFilters[chatId.ToString()].UserLanguage = message.Text;
                            await ChangeBotStage(chatId, BotStage.ChoosingPlatform);
                            CurrentTextPack = UsedTextPacks.First(x => x.Name == message.Text);
                            await UpdateUserFilters();
                            await Bot.SendTextMessageAsync(
                                chatId: chatId,
                                text: CurrentTextPack.PromptGreetings
                            );
                            await ChoosePlatformAction(message);
                        }
                        break;
                    }
                case BotStage.ChoosingPlatform:
                    {
                        var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                            {
                                new KeyboardButton[]
                                {
                                    CurrentTextPack.BtnChoosingPlatform_YT,
                                    CurrentTextPack.BtnChoosingPlatform_Insta
                                }
                            });
                        if (message.Text == CurrentTextPack.BtnChoosingPlatform_YT)
                        {
                            await SendYoutubeOptions(message);
                            await ChangeBotStage(chatId, BotStage.ChoosingYTFunction);
                            CurrentBotStage = BotStages[chatId];
                        }
                        else if (message.Text == CurrentTextPack.BtnChoosingPlatform_Insta)
                        {
                            await SendInstagramOptions(message);
                        }
                        break;
                    }
                case BotStage.ChoosingYTFunction:
                    {
                        if (message.Text == CurrentTextPack.BtnChoosingYTFunction_Filters)
                        {
                            await SendYTFilterOptions(message);
                            await ChangeBotStage(chatId, BotStage.ChoosingYTFilters);
                            CurrentBotStage = BotStages[chatId];
                        }
                        else
                        if (message.Text == CurrentTextPack.BtnChoosingYTFunction_Search)
                        {
                            await SendSearchYTVideoOptions(message);
                            await ChangeBotStage(chatId, BotStage.SearchingYTVideo);
                            CurrentBotStage = BotStages[chatId];
                        }
                        else if (message.Text == CurrentTextPack.BtnChoosingYTFunction_ChannelSearch)
                        {
                            await SendSearchYTChannelOptions(message);
                            await ChangeBotStage(chatId, BotStage.SearchingYTChannel);
                            CurrentBotStage = BotStages[chatId];
                        }
                        break;
                    }

                case BotStage.ChoosingYTFilters:
                    {
                        if (message.Text == CurrentTextPack.BtnChoosingPlatform_YT)
                        {
                            await SendYoutubeOptions(message);
                            await ChangeBotStage(chatId, BotStage.ChoosingYTFunction);
                            CurrentBotStage = BotStages[chatId];
                        }
                        else
                        if (message.Text == CurrentTextPack.BtnSettingYTViews)
                        {
                            await Bot.SendTextMessageAsync(
                                chatId: chatId,
                                text: CurrentTextPack.PromptSettingYTViews,
                                replyMarkup: new ReplyKeyboardRemove()
                            );
                            await ChangeBotStage(chatId, BotStage.SettingYTViews);
                        }
                        else
                        if (message.Text == CurrentTextPack.BtnSettingYTLikes)
                        {
                            await Bot.SendTextMessageAsync(
                                chatId: chatId,
                                text: CurrentTextPack.PromptSettingYTLikes,
                                replyMarkup: new ReplyKeyboardRemove()
                            );
                            await ChangeBotStage(chatId, BotStage.SettingYTLikes);
                        }
                        break;
                    }
                case BotStage.SettingYTLikes:
                    {   //могут быть косяки в создании новых фильтров
                        if (message.Text == CurrentTextPack.BtnBackToStart)
                        {
                            await SendYoutubeOptions(message);
                            await ChangeBotStage(chatId, BotStage.ChoosingYTFunction);
                        }
                        long views = 0;
                        try
                        {
                            long likes = Int32.Parse(Regex.Match(message.Text, @"[-\d]+").Value);
                            if (UserFilters.Count > 0)
                            {
                                if (UserFilters.ContainsKey(chatId.ToString()))
                                    views = UserFilters[chatId.ToString()].ViewsYT;
                                else UserFilters.Add(chatId.ToString(), new UserFilter(chatId, views, likes));
                            }
                            if (likes >= 0)
                            {
                                if (UserFilters.ContainsKey(chatId.ToString()))
                                    UserFilters[chatId.ToString()].LikesYT = likes;
                                else UserFilters.Add(chatId.ToString(), new UserFilter(chatId, views, likes));
                                await UpdateUserFilters();
                                await Bot.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: (likes > 0) ? CurrentTextPack.PromptSettingYTLikes_Done + $" ({likes})"
                                        : CurrentTextPack.PromptSettingYTLikes_Disabled);
                                await SendYTFilterOptions(message);
                                await ChangeBotStage(chatId, BotStage.ChoosingYTFilters);
                            }
                            else
                            {
                                await Bot.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: CurrentTextPack.PromptSettingYTFilter_Error
                                );
                                await SendYTFilterOptions(message);
                                await ChangeBotStage(chatId, BotStage.ChoosingYTFilters);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }

                        break;
                    }
                case BotStage.SettingYTViews:
                    {
                        if (message.Text == CurrentTextPack.BtnBackToStart)
                        {
                            await ChoosePlatformAction(message);
                            await ChangeBotStage(chatId, BotStage.ChoosingPlatform);
                        }

                        try
                        {
                            long likes = 0;
                            long views = Int32.Parse(Regex.Match(message.Text, @"[-\d]+").Value);
                            if (UserFilters.Count > 0)
                            {
                                if (UserFilters.ContainsKey(chatId.ToString()))
                                    likes = UserFilters[chatId.ToString()].LikesYT;
                            }
                            if (views >= 0)
                            {
                                if (UserFilters.ContainsKey(chatId.ToString()))
                                    UserFilters[chatId.ToString()].ViewsYT = views;
                                else UserFilters.Add(chatId.ToString(), new UserFilter(chatId, views, likes));
                                await UpdateUserFilters();
                                await Bot.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: (views > 0)
                                        ? CurrentTextPack.PromptSettingYTViews_Done + $" ({views})"
                                        : CurrentTextPack.PromptSettingYTViews_Disabled);
                                await SendYTFilterOptions(message);
                                await ChangeBotStage(chatId, BotStage.ChoosingYTFilters);
                            }
                            else
                            {
                                await Bot.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: CurrentTextPack.PromptSettingYTFilter_Error
                                );
                                await SendYTFilterOptions(message);
                                await ChangeBotStage(chatId, BotStage.ChoosingYTFilters);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }

                        break;
                    }
                case BotStage.SearchingYTVideo:
                    {
                        var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                        {
                                new KeyboardButton[]{ CurrentTextPack.BtnBackToStart },
                                new KeyboardButton[]
                                {CurrentTextPack.BtnSearchingYTVideo_Next5},
                                new KeyboardButton[]{ CurrentTextPack.BtnChoosingYTFunction_Search }
                            });
                        await Bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: CurrentTextPack.PromptSearchingYTVideo_Done,
                            replyMarkup: RequestReplyKeyboard
                        );

                        YTSearchQuery = (CanChangeQuery) ? message.Text : YTSearchQuery;
                        CanChangeQuery = false;
                        await SearchYTVideo(message, YTSearchQuery, YTVideoPage);
                        await ChangeBotStage(chatId, BotStage.ChoosingYTVideos);
                        break;
                    }
                case BotStage.ChoosingYTVideos:
                    {
                        if (message.Text == CurrentTextPack.BtnSearchingYTVideo_Next5)
                        {
                            ++YTVideoPage;
                            await SearchYTVideo(message, YTSearchQuery, YTVideoPage);
                            await ChangeBotStage(chatId, BotStage.ChoosingYTVideos);
                        }
                        else
                        if (message.Text == CurrentTextPack.BtnChoosingYTFunction_Search)
                        {
                            await SendSearchYTVideoOptions(message);
                            await ChangeBotStage(chatId, BotStage.SearchingYTVideo);
                        }

                        await ChangeBotStage(chatId, BotStage.SearchingYTVideo);
                        break;
                    }
                case BotStage.SearchingYTChannel:
                    {
                        await SendSearchYTChannelOptions(message);
                        var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                        {
                            new KeyboardButton[]{ CurrentTextPack.BtnBackToStart },
                            new KeyboardButton[]{CurrentTextPack.BtnNextChannel,
                                CurrentTextPack.BtnChooseYTChannel }
                        });
                        await Bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: CurrentTextPack.PromptSearchingYTChannel_Done,
                            replyMarkup: RequestReplyKeyboard
                        );
                        YTSearchChannelQuery = (CanChangeChannelQuery) ? message.Text : YTSearchChannelQuery;
                        CanChangeChannelQuery = false;
                        await SearchYTChannel(message,YTSearchChannelQuery);
                        await ChangeBotStage(chatId, BotStage.ChoosingYTChannels);
                        break;
                    }
                case BotStage.ChoosingYTChannels:
                    {
                        if (message.Text == CurrentTextPack.BtnNextChannel)
                        {
                            ++YTChannelPage;
                            await SearchYTChannel(message,YTSearchChannelQuery);
                            await ChangeBotStage(chatId, BotStage.ChoosingYTChannels);
                        }
                        else
                        if (message.Text == CurrentTextPack.BtnChooseYTChannel)
                        {
                            UserFilters[chatId.ToString()].ChosenChannelId = YoutubeAPISearch.Search.channelId.Last();
                            await UpdateUserFilters();
                            await SendSearchYTVideoOptions(message);
                            await ChangeBotStage(chatId, BotStage.SearchingYTChannel);
                        }
                        await ChangeBotStage(chatId, BotStage.ChoosingYTChannels);
                        break;
                    }
                default:
                    //await ChoosePlatformAction(message);
                    //await ChangeBotStage(chatId, BotStage.ChoosingPlatform);

                    break;
            }

            if (message.Text == CurrentTextPack.BtnBackToStart)
            {   //В начало
                await ChoosePlatformAction(message);
                await ChangeBotStage(chatId, BotStage.ChoosingPlatform);
                CurrentBotStage = BotStages[chatId];
            }
            else if (message.Text == CurrentTextPack.CommandStart)
            {   //-/start - выбор языка
                await ChooseLanguageAction(message);
                await ChangeBotStage(chatId, BotStage.ChoosingLanguage);
                CurrentBotStage = BotStages[chatId];
            }
            else if (message.Text == CurrentTextPack.CommandHelp)
            {   //-/help
                await Bot.SendTextMessageAsync(
                    chatId: chatId,
                    text: CurrentTextPack.PromptGreetings
                );
                await Bot.SendTextMessageAsync(
                    chatId: chatId,
                    text: CurrentTextPack.PromptHelp
                );
            }
            else if (message.Text == CurrentTextPack.CommandInfo)
            {   //-/info
                await Bot.SendTextMessageAsync(
                    chatId: chatId,
                    text: CurrentTextPack.PromptInfo
                );
            }

            else
            {
                //await Bot.SendTextMessageAsync(
                //    chatId: chatId,
                //    text: CurrentTextPack.PromptInvalidCommandResponse
                //);
            }

            async Task ChooseLanguageAction(Message msg)
            {
                KeyboardButton[] keyboardButtons = new KeyboardButton[UsedTextPacks.Length];
                for (int i = 0; i < UsedTextPacks.Length; i++)
                {
                    keyboardButtons[i] = new KeyboardButton(UsedTextPacks[i].Name);
                }
                var RequestReplyKeyboard = new ReplyKeyboardMarkup(keyboardButtons);

                await Bot.SendTextMessageAsync(
                    chatId: chatId,
                    text: CurrentTextPack.PromptChoosingLanguage,
                    replyMarkup: RequestReplyKeyboard
                );
                await UpdateUserFilters();
            }
            async Task SendYoutubeOptions(Message msg)
            {
                var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[]{ CurrentTextPack.BtnBackToStart },
                    new KeyboardButton[]{CurrentTextPack.BtnChoosingYTFunction_Filters,
                        CurrentTextPack.BtnChoosingYTFunction_Search },
                    new KeyboardButton[]{ CurrentTextPack.BtnChoosingYTFunction_ChannelSearch }
                });
                await Bot.SendTextMessageAsync(
                    chatId: msg.Chat.Id,
                    text: CurrentTextPack.PromptChoosingYTFunction,
                    replyMarkup: RequestReplyKeyboard
                );
            }

            async Task SendYTFilterOptions(Message msg)
            {
                var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[]{ CurrentTextPack.BtnChoosingPlatform_YT},
                    new KeyboardButton[]{ CurrentTextPack.BtnSettingYTViews,
                        CurrentTextPack.BtnSettingYTLikes}
                });
                await Bot.SendTextMessageAsync(
                    chatId: msg.Chat.Id,
                    text: CurrentTextPack.PromptChoosingYTFilters +
                          CurrentTextPack.PromptCurrentFilters(UserFilters[chatId.ToString()].ViewsYT,
                              UserFilters[chatId.ToString()].LikesYT),
                    replyMarkup: RequestReplyKeyboard
                );
            }
            async Task SendSearchYTVideoOptions(Message msg)
            {
                CanChangeQuery = true;
                await Bot.SendTextMessageAsync(
                    chatId: msg.Chat.Id,
                    text: CurrentTextPack.PromptSearchingYTVideo,
                    replyMarkup: new ReplyKeyboardRemove()
                );
            }
            async Task SendSearchYTChannelOptions(Message msg)
            {
                CanChangeChannelQuery = true;
                await Bot.SendTextMessageAsync(
                    chatId: msg.Chat.Id,
                    text: CurrentTextPack.PromptSearchingYTChannel,
                    replyMarkup: new ReplyKeyboardRemove()
                );
            }
            async Task SendInstagramOptions(Message msg)
            {
                await Bot.SendChatActionAsync(chatId, ChatAction.UploadPhoto);
                try
                {
                    await Bot.SendPhotoAsync(
                           chatId: chatId,
                           photo: new InputOnlineFile(new Uri("https://i.ytimg.com/vi/U5YssW0z9rY/hqdefault.jpg")),
                           caption: "Здесь что-то будет"
                       );
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            async Task ChoosePlatformAction(Message msg)
            {
                CanChangeQuery = true;
                var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[]{CurrentTextPack.BtnChoosingPlatform_YT,
                        CurrentTextPack.BtnChoosingPlatform_Insta}
                });
                await Bot.SendTextMessageAsync(
                    chatId: msg.Chat.Id,
                    text: CurrentTextPack.PromptChoosingPlatform,
                    replyMarkup: RequestReplyKeyboard
                );
            }

            async Task DefaultResponse(Message msg)
            {
                string usage = CurrentTextPack.PromptInvalidCommandResponse;
                await Bot.SendTextMessageAsync(
                    chatId: msg.Chat.Id,
                    text: usage,
                    replyMarkup: new ReplyKeyboardRemove()
                );
                await ChoosePlatformAction(msg);
            }
        }

        private static async Task SearchYTChannel(Message msg, string query)
        {
            if (await YoutubeAPISearch.Search.SearchChannel(query, YTChannelPage))
            {
                await Bot.SendTextMessageAsync(
                    chatId: msg.Chat.Id,
                    text: CurrentTextPack.PromptSearchingYTChannel_Done
                );
                await Bot.SendTextMessageAsync(
                    chatId: msg.Chat.Id,
                    text: YoutubeAPISearch.Search.channel.Last()
                );
            }
            else await Bot.SendTextMessageAsync(
                chatId: msg.Chat.Id,
                text: CurrentTextPack.PromptSearchingYTChannel_Failure
            );

        }

        private static async Task SearchYTVideo(Message message, string query, int ytVideoPage)
        {
            await YoutubeAPISearch.Search.SearchVideo(query, ytVideoPage,
                UserFilters[message.Chat.Id.ToString()].ViewsYT,
                UserFilters[message.Chat.Id.ToString()].LikesYT);
            foreach (string video in YoutubeAPISearch.Search.videos.Last())
            {
                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: video /*Regex.Match(video, @"[\w\W]+\(([\w\W]+)\)").Value*/
                );
            }
            if (YoutubeAPISearch.Search.videos.Count == 0)
                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: CurrentTextPack.PromptSearchingYTVideo_Failure
                );
        }


        // Process Inline Keyboard callback data
        //private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        //{
        //    var callbackQuery = callbackQueryEventArgs.CallbackQuery;

        //    await Bot.AnswerCallbackQueryAsync(
        //        callbackQueryId: callbackQuery.Id,
        //        text: $"Received {callbackQuery.Data}"
        //    );

        //    await Bot.SendTextMessageAsync(
        //        chatId: callbackQuery.chatId,
        //        text: $"Received {callbackQuery.Data}"
        //    );
        //}

        #region Inline Mode

        //private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        //{
        //    Console.WriteLine($"Received inline query from: {inlineQueryEventArgs.InlineQuery.From.Id}");

        //    InlineQueryResultBase[] results = {
        //        // displayed result
        //        new InlineQueryResultArticle(
        //            id: "3",
        //            title: "TgBots",
        //            inputMessageContent: new InputTextMessageContent(
        //                "hello"
        //            )
        //        )
        //    };
        //    await Bot.AnswerInlineQueryAsync(
        //        inlineQueryId: inlineQueryEventArgs.InlineQuery.Id,
        //        results: results,
        //        isPersonal: true,
        //        cacheTime: 0
        //    );
        //}

        //private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        //{
        //    Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        //}

        #endregion

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message
            );
        }
    }
}
