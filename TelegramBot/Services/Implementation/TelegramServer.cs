using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace TelegramBot.Services.Implementation;

internal class TelegramServer : ITelegramServer
{
    private readonly IServiceProvider _services;

    private readonly ITelegramServiceOptions _options;

    public TelegramServer(IServiceProvider serviceProvider)
    {
        _services = serviceProvider;
        _options = serviceProvider.GetRequiredService<ITelegramServiceOptions>();
    }

    public async Task StartAsync()
    {
        try
        {
            await using var scope = _services.CreateAsyncScope();
            var generator = scope.ServiceProvider.GetRequiredService<GeneratorBotService>();
            var data = await generator.ConnectAndGetTelegramBot(InvokeSuccessEvent, InvokeErrorEvent);
            if (data.IsConnected)
            {
                data.Bot.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync);
                InvokeSuccessEvent("Bot is running successfully!");
                return;
            }

            InvokeErrorEvent($"Bot is not running!");
        }
        catch (Exception ex)
        {
            InvokeErrorEvent($"{ex.Message}\r\n{ex.StackTrace}");
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

        using var scope = _services.CreateScope();
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
