using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public interface ITelegramServer
    {
        Task StartAsync();
        void StopAsync();
        event EventHandler<string> OnCallbackSuccessMessage;
        event EventHandler<string> OnCallbackErrorMessage;
    }
}
