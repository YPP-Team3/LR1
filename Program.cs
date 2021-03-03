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

namespace TelegramBot
{

    class Program
    {
        private static Telegram.Bot.TelegramBotClient Bot;
        
        /// <summary>
        /// Хранит текущие состояния бота
        /// </summary>
        private static Dictionary<long,BotStage> BotStages;

        private static Dictionary<string, UserFilter> UserFilters;

        private const string BotStagesJsonPath = "./BotStages.json";
        private const string UserFiltersJsonPath = "./UserFilters.json";

        public static BotStage CurrentBotStage;
        public enum BotStage
        {
            ChoosingPlatform, ChoosingYTFunction, ChoosingYTFilters,
            SettingYTLikes, SettingYTViews,
            SearchingYTChannel, SearchingYTVideo
        }

        private static async Task ChangeBotStage(long chatId, BotStage nextBotStage)
        {
            if (BotStages.ContainsKey(chatId))
            {
                BotStages[chatId] = nextBotStage;
            }
            else
            {
                BotStages.Add(chatId,nextBotStage);
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
            if((await JsonReadWrite.ReadBotStagesJsonAsync(BotStagesJsonPath)).Count != 0)
                BotStages = await JsonReadWrite.ReadBotStagesJsonAsync(BotStagesJsonPath);
            else BotStages = new Dictionary<long, BotStage>();

            if (!File.Exists(UserFiltersJsonPath)) File.Create(UserFiltersJsonPath);
            if ((await JsonReadWrite.ReadUserFiltersJsonAsync(UserFiltersJsonPath)).Count != 0)
                UserFilters = await JsonReadWrite.ReadUserFiltersJsonAsync(UserFiltersJsonPath);
            else UserFilters = new Dictionary<string, UserFilter>();


            string token = System.IO.File.ReadAllText($"./token.txt");
            Bot = new TelegramBotClient(token);
            User me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;

            //Bot.OnMessage += BotOnMessageReceived2;
            
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");

            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static void BotOnMessageReceived2(object sender, MessageEventArgs e)
        {
            Console.WriteLine(Bot.GetChatAsync(e.Message.Chat.Id).Result.Id);
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            Message message = messageEventArgs.Message;
            long chatId = message.Chat.Id;
            if (true /*chatId == 430265734*/)
            {
                if (!BotStages.ContainsKey(chatId))
                {
                    await ChangeBotStage(chatId, BotStage.ChoosingPlatform);
                }
                CurrentBotStage = BotStages[chatId];

                if (message == null || message.Type != MessageType.Text)
                {
                    return;
                }

                switch (CurrentBotStage)
                {
                    case BotStage.ChoosingPlatform:
                        {
                            switch (message.Text)
                            {
                                case "YouTube":
                                    await SendYoutubeOptions(message);
                                    await ChangeBotStage(chatId, BotStage.ChoosingYTFunction);
                                    CurrentBotStage = BotStages[chatId];
                                    break;

                                case "Instagram":
                                    await SendInstagramOptions(message);
                                    break;
                            }
                            break;
                        }
                    case BotStage.ChoosingYTFunction:
                        {
                            switch (message.Text)
                            {
                                case "Фильтры YT":
                                    await SendYTFilterOptions(message);
                                    await ChangeBotStage(chatId, BotStage.ChoosingYTFilters);
                                    CurrentBotStage = BotStages[chatId];
                                    break;

                                case "Поиск YT":
                                    await SendSearchYTVideoOptions(message);
                                    await ChangeBotStage(chatId, BotStage.SearchingYTVideo);
                                    CurrentBotStage = BotStages[chatId];
                                    break;
                                case "Поиск Каналов YT":
                                    await SendSearchYTChannelOptions(message);
                                    await ChangeBotStage(chatId, BotStage.SearchingYTChannel);
                                    CurrentBotStage = BotStages[chatId];
                                    break;
                            }
                            break;
                        }

                    case BotStage.ChoosingYTFilters:
                        {
                            switch (message.Text)
                            {
                                case "Лайки":
                                    await Bot.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: "Введите число лайков"
                                    );
                                    await ChangeBotStage(chatId, BotStage.SettingYTLikes);
                                    break;
                                case "Просмотры":
                                    await Bot.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: "Введите число просмотров"
                                    );
                                    await ChangeBotStage(chatId, BotStage.SettingYTViews);
                                    break;
                            }
                            break;
                        }
                    case BotStage.SettingYTLikes:
                        {   //могут быть косяки в создании новых фильтров
                            if (message.Text == "В начало")
                            {
                                await ChoosePlatformAction(message);
                                await ChangeBotStage(chatId, BotStage.ChoosingPlatform);
                            }
                            long views = 0;
                            long likes = Int32.Parse(Regex.Match(message.Text, @"[-\d]+").Value);
                            if (UserFilters.Count > 0)
                            {
                                if (UserFilters.ContainsKey(chatId.ToString()))
                                    views = UserFilters[chatId.ToString()].ViewsYT;
                                else UserFilters.Add(chatId.ToString(), new UserFilter(chatId, views, likes));
                            }
                            if (likes > 0)
                            {
                                if (UserFilters.ContainsKey(chatId.ToString()))
                                    UserFilters[chatId.ToString()].LikesYT = likes;
                                else UserFilters.Add(chatId.ToString(), new UserFilter(chatId, views, likes));
                                await UpdateUserFilters();
                                await Bot.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: (likes >0) ? $"Фильтр лайков записан ({likes})"
                                        : "Фильтр лайков отключен");
                                await SendYTFilterOptions(message);
                                await ChangeBotStage(chatId, BotStage.ChoosingYTFilters);
                            }
                            else await Bot.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Введите положительное число, или 0 для сброса фильтра"
                            );
                            break;
                        }
                    case BotStage.SettingYTViews:
                    {
                        if (message.Text == "В начало")
                        {
                            await ChoosePlatformAction(message);
                            await ChangeBotStage(chatId, BotStage.ChoosingPlatform);
                        }
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
                                        ? $"Фильтр просмотров записан ({views})"
                                        : "Фильтр просмотров отключен");
                                await SendYTFilterOptions(message);
                                await ChangeBotStage(chatId, BotStage.ChoosingYTFilters);
                            }
                            else await Bot.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Введите положительное число, или 0 для сброса фильтра"
                            );
                            break;
                        }
                    case BotStage.SearchingYTVideo:
                        {
                            //делать дело, а потом
                            await ChoosePlatformAction(message);
                            await ChangeBotStage(chatId, BotStage.ChoosingPlatform);
                            break;
                        }
                    case BotStage.SearchingYTChannel:
                        {
                            //делать дело, а потом
                            await ChoosePlatformAction(message);
                            await ChangeBotStage(chatId, BotStage.ChoosingPlatform);
                            break;
                        }
                    default:
                        break;
                }
                switch (message.Text)
                {
                    case "В начало":
                        await ChoosePlatformAction(message);
                        await ChangeBotStage(chatId, BotStage.ChoosingPlatform);
                        CurrentBotStage = BotStages[chatId];
                        break;

                    default:
                        break;
                }

                async Task SendYoutubeOptions(Message msg)
                {
                    var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                    new KeyboardButton[]{ "В начало" },
                    new KeyboardButton[]{"Фильтры YT","Поиск YT"},
                    new KeyboardButton[]{ "Поиск каналов YT" }
                });
                    await Bot.SendTextMessageAsync(
                        chatId: msg.Chat.Id,
                        text: "Выберите функцию",
                        replyMarkup: RequestReplyKeyboard
                    );
                }

                async Task SendYTFilterOptions(Message msg)
                {
                    var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                    new KeyboardButton[]{ "В начало"},
                    new KeyboardButton[]{"Лайки","Просмотры"}
                });
                    await Bot.SendTextMessageAsync(
                        chatId: msg.Chat.Id,
                        text: "Выберите Фильтр",
                        replyMarkup: RequestReplyKeyboard
                    );
                }
                async Task SendSearchYTVideoOptions(Message msg)
                {
                    await Bot.SendTextMessageAsync(
                        chatId: msg.Chat.Id,
                        text: "Введите поисковый запрос",
                        replyMarkup: new ReplyKeyboardRemove()
                    );
                }
                async Task SendSearchYTChannelOptions(Message msg)
                {
                    await Bot.SendTextMessageAsync(
                        chatId: msg.Chat.Id,
                        text: "Введите название канала",
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
                    var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                    new KeyboardButton[]{"YouTube","Instagram"},
                });
                    await Bot.SendTextMessageAsync(
                        chatId: msg.Chat.Id,
                        text: "Выберите платформу",
                        replyMarkup: RequestReplyKeyboard
                    );
                }

                async Task DefaultResponse(Message msg)
                {
                    const string usage = "Такой команды нет, используйте кнопки меню";
                    await Bot.SendTextMessageAsync(
                        chatId: msg.Chat.Id,
                        text: usage,
                        replyMarkup: new ReplyKeyboardRemove()
                    );
                    await ChoosePlatformAction(msg);
                }
            }

            
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
