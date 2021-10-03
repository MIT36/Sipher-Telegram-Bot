using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class TelegramServerOptions
    {
        public string Token { get; set; }

        public EventHandler<string> CallbackSuccessMessage { get; set; }

        public EventHandler<string> CallbackErrorMessage { get; set; }
    }
}
