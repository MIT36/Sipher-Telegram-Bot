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
using TelegramBot.Services;
using TelegramBot.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FeistelCipher;
using ProxiesTelegram;

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
                //await serviceProvider.GetRequiredService<TelegramServer>().StartAsync();

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
                var key = config["Key"];
                var site = config["Site"];
                services.AddProxiesTelegram(opt =>
                {
                    opt.ConnectionStringDb = connectionDb;
                    opt.Site = site;
                })
                    .AddHostedService<AppHostedService>()
                    .AddSingleton<TelegramServer>()
                    .AddTransient<ITextCommand, TextCommand>()
                    .AddFeistelSipher(key);
                    //.AddTransient<IFeistelSipher, FeistelCipherClassic>(sv => new FeistelCipherClassic(Encoding.GetEncoding(1251).GetBytes(key)));
            })
            .RunConsoleAsync();
        }
    }
}
