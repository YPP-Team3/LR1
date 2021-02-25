using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBot
{
    class JsonReadWrite
    {
        private const int NumberOfRetries = 5;
        private const int DelayOnRetry = 100;
        public static async Task WriteJsonAsync(string fileName, List<UserFilter> userFilters)
        {
            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    using (FileStream fileStream = File.OpenWrite(fileName))
                        await JsonSerializer.SerializeAsync(fileStream, userFilters);
                    break;
                }
                catch (IOException e) when (i <= NumberOfRetries)
                {
                    Thread.Sleep(DelayOnRetry);
                }
            }
        }

        public static async Task<List<UserFilter>> ReadJsonAsync(string fileName)
        {
            List<UserFilter> userFilters = new List<UserFilter>();
            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    using (FileStream fileStream = File.OpenRead(fileName))
                        userFilters = await JsonSerializer.DeserializeAsync<List<UserFilter>>(fileStream);
                    break;
                }
                catch (IOException e) when (i <= NumberOfRetries)
                {
                    Thread.Sleep(DelayOnRetry);
                }
            }
            return userFilters;   
        }
    }
}
