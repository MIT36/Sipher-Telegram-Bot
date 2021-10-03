using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProxiesTelegram
{
    public interface IProxyService
    {
        IEnumerable<WebProxy> GetProxiesFromSite();

        Task<IEnumerable<WebProxy>> GetExistingProxies();

        Task SaveProxy(string host, int port);
    }
}
