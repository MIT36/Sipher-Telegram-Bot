using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Services;

public interface ITelegramServiceOptions
{
    string Token { get; }

    EventHandler<string> CallbackSuccessMessage { get; set; }

    EventHandler<string> CallbackErrorMessage { get; set; }
}
