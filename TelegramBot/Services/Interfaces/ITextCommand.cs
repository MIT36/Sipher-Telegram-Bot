using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBot.Services.Interfaces
{
    interface ITextCommand
    {
        string GetText(string message);
    }
}
