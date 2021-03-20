using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using File = System.IO.File;
using static TelegramBot.BotStage;

namespace TelegramBot
{
    class JsonReadWrite
    {
        private const int NumberOfRetries = 5;
        private const int DelayOnRetry = 100;
        public static async Task WriteJsonAsync<T>(string fileName, T type)
        {
            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    if (File.Exists(fileName)) File.Delete(fileName);
                    using (FileStream fileStream = File.OpenWrite(fileName))
                    {
                        await JsonSerializer.SerializeAsync(fileStream, type);
                    }
                    break;
                }
                catch (IOException e) when (i <= NumberOfRetries)
                {
                    Thread.Sleep(DelayOnRetry);
                }
            }
        }

        public static async Task<Dictionary<string, UserFilter>> ReadUserFiltersJsonAsync(string fileName)
        {
            Dictionary<string, UserFilter> userFilters = new Dictionary<string, UserFilter>();
            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    string text = File.ReadAllText(fileName);
                    userFilters = JsonSerializer.Deserialize<Dictionary<string, UserFilter>>(text);
                    //using (FileStream fileStream = File.OpenRead(fileName))
                    //    if (!JsonSerializer.DeserializeAsync<Dictionary<string, UserFilter>>(fileStream)
                    //        .IsCompletedSuccessfully)
                    //        throw new Exception();

                    //    else
                    //    {
                    //        userFilters = await JsonSerializer.
                    //            DeserializeAsync<Dictionary<string, UserFilter>>(fileStream);
                    //    }
                    break;
                }
                catch (IOException e) when (i <= NumberOfRetries)
                {
                    await Task.Delay(DelayOnRetry);
                }
            }
            return userFilters;
        }
        public static async Task<Dictionary<long, BotStage>> ReadBotStagesJsonAsync(string fileName)
        {
            Dictionary<long, BotStage> BotStages = new Dictionary<long, BotStage>();
            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    using (FileStream fileStream = File.OpenRead(fileName))
                        if (!JsonSerializer.DeserializeAsync<Dictionary<long, BotStage>>(fileStream)
                            .IsCompletedSuccessfully)
                            break;
                        else
                        {
                            BotStages = await JsonSerializer.
                          DeserializeAsync<Dictionary<long, BotStage>>(fileStream);
                        }
                    break;
                }
                catch (IOException e) when (i <= NumberOfRetries)
                {
                    await Task.Delay(DelayOnRetry);
                }
            }
            return BotStages;
        }
    }
}
