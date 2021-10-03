using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProxiesTelegram;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Services;
using TelegramBot.Services.Interfaces;

namespace TelegramBot
{
    internal class TelegramServer
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public TelegramServer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private ITelegramBotClient BotClient { set; get; }
        private User botUser { get; set; }

        private async Task<WebProxy> TryConnectWithProxies(IEnumerable<WebProxy> webProxies)
        {
            foreach (var proxy in webProxies)
            {
                Console.WriteLine($"Connecting to telegram bot with proxy: {proxy.Address.Host}:{proxy.Address.Port}...");
                try
                {
                    await InitTelegramBot(proxy);
                    return proxy;
                }
                catch
                {
                    Console.WriteLine($"Proxy failed: {proxy.Address.Host}:{proxy.Address.Port}");
                }
            }
            return null;
        }

        private async Task InitTelegramBot(WebProxy proxy = null)
        {
            using var scope = _scopeFactory.CreateScope();
            var token = scope.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string>("TelegramBotToken");
            BotClient = new TelegramBotClient(token, proxy ?? null);
            BotClient.Timeout = TimeSpan.FromSeconds(5);
            botUser = await BotClient.GetMeAsync();
            Console.WriteLine($"Telegram Bot: {botUser.Username}");
            Console.WriteLine(proxy != null ? $"Proxy port is good: {proxy.Address.Host}:{proxy.Address.Port}" : "Сonnection success!");
            BotClient.OnMessage += TelegramBotClient_OnMessage;
            BotClient.StartReceiving();
        }

        public async Task StartAsync()
        {
            try
            {
                await InitTelegramBot();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection error. Perhaps the telegram is blocked in your location.");
                Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
            }

            if (botUser == null)
            {
                using var scope = _scopeFactory.CreateScope();
                var proxyService = scope.ServiceProvider.GetRequiredService<IProxyService>();
                var goodProxy = await TryConnectWithProxies(await proxyService.GetExistingProxies()) ?? await TryConnectWithProxies(proxyService.GetProxiesFromSite());
                if (goodProxy != null)
                {
                    await proxyService.SaveProxy(goodProxy.Address.Host, goodProxy.Address.Port);
                    return;
                }
            }
        }

        private async void TelegramBotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            try
            {
                User userFrom = e?.Message?.From;
                Console.WriteLine($@"Message from User.
Login: {userFrom?.Username} 
First Name {userFrom?.FirstName}
Last Name: {userFrom?.LastName}
Message: {e?.Message?.Text}");
                TelegramBotClient botClient = sender as TelegramBotClient;
                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var cmd = scope.ServiceProvider.GetRequiredService<ITextCommand>();
                    await botClient.SendTextMessageAsync(e.Message?.Chat, cmd.GetText(e.Message.Text));
                }
                else
                {
                    await botClient.SendTextMessageAsync(e.Message?.Chat, "Я принимаю только текстовые сообщения для шифрования!");
                }
                /*string message = e?.Message?.Text?.ToLower()?.Trim();
                if (message == @"/start")
                    await botClient.SendTextMessageAsync(e?.Message?.Chat, "test message");
                else if (message.Contains("hello") || message.Contains("привет"))
                    await botClient.SendTextMessageAsync(e?.Message?.Chat, "Привет, я кароч бот, который написан на .Net Core! Я слегка тупой, но здАроваться по русски можууу!");
                else
                {
                    var fc = new FeistelCipherClassic();
                    var result = fc.CryptText(e?.Message?.Text);
                    await botClient.SendTextMessageAsync(e?.Message?.Chat, result);
                }*/
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
            }

        }

        
    }
}
