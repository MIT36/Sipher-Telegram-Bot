using Microsoft.Extensions.DependencyInjection;
using ProxiesTelegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Services.Interfaces;

namespace TelegramBot
{
    internal class TelegramServer : ITelegramServer
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private TelegramServerOptions _options;

        public TelegramServer(IServiceScopeFactory scopeFactory, TelegramServerOptions options)
        {
            _scopeFactory = scopeFactory;
            _options = options;
        }

        private ITelegramBotClient BotClient { set; get; }

        public event EventHandler<string> OnCallbackSuccessMessage;
        public event EventHandler<string> OnCallbackErrorMessage;

        private async Task<WebProxy> TryConnectWithProxies(IEnumerable<WebProxy> webProxies)
        {
            foreach (var proxy in webProxies)
            {
                InvokeSuccessEvent($"Connecting to telegram bot with proxy: {proxy.Address.Host}:{proxy.Address.Port}...");
                try
                {
                    await InitTelegramBot(proxy);
                    return proxy;
                }
                catch
                {
                    InvokeErrorEvent($"Proxy failed: {proxy.Address.Host}:{proxy.Address.Port}");
                }
            }
            return null;
        }

        private async Task<User> InitTelegramBot(WebProxy proxy = null)
        {
            using var scope = _scopeFactory.CreateScope();
            BotClient = new TelegramBotClient(_options.Token, proxy ?? null) { Timeout = TimeSpan.FromSeconds(5) };

            User botUser = await BotClient.GetMeAsync();

            var connectMessage = proxy != null ? $"Proxy port is good: {proxy.Address.Host}:{proxy.Address.Port}" : "Сonnection success!";
            var message = $"Telegram Bot: {botUser.Username}\r\n{connectMessage}";

            InvokeSuccessEvent(message);
            BotClient.OnMessage += TelegramBotClient_OnMessage;
            BotClient.StartReceiving();
            return botUser;
        }

        public async Task StartAsync()
        {
            var botUser = default(User);
            try
            {
                botUser = await InitTelegramBot();
            }
            catch (Exception ex)
            {
                InvokeErrorEvent($"{ex.Message}\r\n{ex.StackTrace}");
            }

            if (botUser == null)
            {
                using var scope = _scopeFactory.CreateScope();
                var proxyService = scope.ServiceProvider.GetRequiredService<IProxyService>();
                var goodProxy = await TryConnectWithProxies(await proxyService.GetExistingProxies()) ?? await TryConnectWithProxies(await proxyService.GetProxiesFromSite());
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

                var logMessage = GetUserFromMessage(userFrom?.Username, userFrom?.FirstName, userFrom?.LastName, e?.Message?.Text);

                InvokeSuccessEvent(logMessage);
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
            }
            catch (Exception ex)
            {
                InvokeErrorEvent($"{ex.Message}\r\n{ex.StackTrace}");
            }

        }

        public void StopAsync()
        {
            BotClient.StopReceiving();
        }

        private void InvokeErrorEvent(string message)
        {
            _options.CallbackErrorMessage?.Invoke(this, message);
            OnCallbackErrorMessage?.Invoke(this, message);
        }

        private void InvokeSuccessEvent(string message)
        {
            _options.CallbackSuccessMessage?.Invoke(this, message);
            OnCallbackSuccessMessage?.Invoke(this, message);
        }

        private string GetUserFromMessage(string userName, string firstName, string lastName, string text) =>
            $"Message from User.\r\nLogin: {userName}\r\nFirst Name {firstName}\r\nLast Name: {lastName}\r\nMessage: {text}";
    }
}
