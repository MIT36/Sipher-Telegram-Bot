using System;

namespace TelegramBot;

public class TelegramServerOptions
{
    public string Token { get; set; }

    public EventHandler<string> CallbackSuccessMessage { get; set; }

    public EventHandler<string> CallbackErrorMessage { get; set; }
}
