using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBot.dbo.Models
{
    public class Proxy
    {
        public Guid Id { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public DateTime LastConnection { get; set; }
    }
}
