using System;
using Microsoft.Extensions.Configuration;

namespace TelegramBot.Services.Implementation;

internal class TelegramServerOptions : ITelegramServiceOptions
{
    public string Token { get; set; }

    public EventHandler<string> CallbackSuccessMessage { get; set; }

    public EventHandler<string> CallbackErrorMessage { get; set; }
}
