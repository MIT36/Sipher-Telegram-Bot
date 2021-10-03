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
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TelegramBot;

namespace ConsoleClient
{
    class Program
    {
        
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                await RunConsoleHost(args);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
            }
        }

        public static async Task RunConsoleHost(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var config = hostContext.Configuration;
                var connectionDb = config.GetConnectionString("DefaultConnection");
                var token = config["TelegramBotToken"];
                var key = config["Key"];
                var site = config["Site"];
                services
                    .AddHostedService<AppHostedService>()
                    .AddTelegramBotServices(new TelegramBotOptions
                    {
                        ConnectionStringProxyDb = connectionDb,
                        Token = token,
                        KeySipher = key,
                        ProxySite = site
                    });
            })
            .RunConsoleAsync();
        }
    }
}
