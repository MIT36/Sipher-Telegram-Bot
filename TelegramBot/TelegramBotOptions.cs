using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class TelegramBotOptions
    {
        public string Token { get; set; }

        public string KeySipher { get; set; }

        public string ConnectionStringProxyDb { get; set; }

        public string ProxySite { get; set; }

        public EventHandler<string> CallbackSuccessMessage { get; set; }

        public EventHandler<string> CallbackErrorMessage { get; set; }
    }
}
