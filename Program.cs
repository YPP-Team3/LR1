using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBot
{
    class Program
    {
        
        static void Main(string[] args)
        {
            string token = File.ReadAllText($"./token.txt");
            
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);

            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync(token);
            }
            Console.ReadLine();
            //UserFilter userFilter =
            //    new UserFilter("smth", 0, 1000, UserFilter.FilterType.Youtube);
            //List<UserFilter> userFilters = new List<UserFilter>();
            //userFilters.Add(userFilter);
            //userFilters.Add(userFilter);

            //string fileName = Console.ReadLine();
            //JsonReadWrite.WriteJsonAsync(fileName, userFilters);

            //Console.WriteLine(String.Join("\n", JsonReadWrite.ReadJsonAsync(fileName).Result));

        }

        static async void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string key = e.Argument as string;

            try
            {
                Telegram.Bot.TelegramBotClient bot = new Telegram.Bot.TelegramBotClient(key);
                await bot.SetWebhookAsync("");

                int offset = 0;
                while (true)
                {
                    var updates = await bot.GetUpdatesAsync(offset);
                    foreach (var update in updates)
                    {
                        var message = update.Message;
                        if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                        {
                            if (message.Text == "/setlikes")
                            {
                                await bot.SendTextMessageAsync(message.Chat.Id, message.Text,
                                    replyToMessageId: message.MessageId);
                            }
                        }

                        offset = update.Id + 1;
                        Console.WriteLine($"[{update.Id}]\t{update.Message}");
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}
