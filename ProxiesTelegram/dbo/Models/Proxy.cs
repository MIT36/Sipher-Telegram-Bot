using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxiesTelegram.dbo.Models
{
    public class Proxy
    {
        public Guid Id { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public DateTime LastConnection { get; set; }
    }
}
