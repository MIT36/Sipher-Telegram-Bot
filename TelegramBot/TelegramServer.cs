using Microsoft.Extensions.DependencyInjection;
using ProxiesTelegram;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using TelegramBot.Services.Interfaces;

namespace TelegramBot;

internal class TelegramServer : ITelegramServer
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly TelegramServerOptions _options;

    public TelegramServer(IServiceScopeFactory scopeFactory, TelegramServerOptions options)
    {
        _scopeFactory = scopeFactory;
        _options = options;
    }

    private ITelegramBotClient BotClient { set; get; }

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
        BotClient = new TelegramBotClient(_options.Token);

        User botUser = await BotClient.GetMeAsync();

        var connectMessage = proxy != null ? $"Proxy port is good: {proxy.Address.Host}:{proxy.Address.Port}" : "Сonnection success!";
        var message = $"Telegram Bot: {botUser.Username}\r\n{connectMessage}";

        InvokeSuccessEvent(message);
        BotClient.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync);
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

        // For Russia, when telegram was blocked in 2018
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

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message && update.Type != Telegram.Bot.Types.Enums.UpdateType.EditedMessage)
        {
            await botClient.SendTextMessageAsync(update.Message.Chat, "Я принимаю только текстовые сообщения для шифрования!");
            return;
        }

        var message = update.Message;
        var userFrom = message.From;

        var logMessage = GetUserFromMessage(userFrom?.Username, userFrom?.FirstName, userFrom?.LastName, message?.Text);

        InvokeSuccessEvent(logMessage);

        using var scope = _scopeFactory.CreateScope();
        var cmd = scope.ServiceProvider.GetRequiredService<ITextCommand>();
        await botClient.SendTextMessageAsync(message?.Chat, cmd.GetText(message?.Text));
    }

    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        InvokeErrorEvent(ErrorMessage);
        return Task.CompletedTask;
    }

    private void InvokeErrorEvent(string message)
    {
        _options.CallbackErrorMessage?.Invoke(this, message);
    }

    private void InvokeSuccessEvent(string message)
    {
        _options.CallbackSuccessMessage?.Invoke(this, message);
    }

    private string GetUserFromMessage(string userName, string firstName, string lastName, string text) =>
        $"Message from User.\r\nLogin: {userName}\r\nFirst Name {firstName}\r\nLast Name: {lastName}\r\nMessage: {text}";
}
