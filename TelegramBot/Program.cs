using System;
using System.Net.Http;
using Telegram.Bot;
using MihaZupan;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TelegramBot.dbo;
using TelegramBot.Services;
using TelegramBot.Services.Interfaces;

namespace TelegramBot
{
    class Program
    {
        
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                var serviceProvider = new ServiceCollection()
                    .AddDbContext<SQLiteContext>()
                    .AddSingleton<TelegramServer>()
                    .AddTransient<ProxyService>()
                    .AddTransient<ITextCommand, TextCommand>()
                    .AddTransient<IFeistelSipher, FeistelCipherClassic>(sv => new FeistelCipherClassic(Encoding.GetEncoding(1251).GetBytes(Secret.Key)))
                    .BuildServiceProvider();
                await serviceProvider.GetRequiredService<TelegramServer>().StartAsync();
                Console.ReadKey();
                //test commit
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
            }
        }
    }
}
