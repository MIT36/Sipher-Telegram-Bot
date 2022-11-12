using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProxiesTelegram
{
    /// <summary>
    /// Proxy service is used if telegram is blocked. 
    /// Parse Ip addressed from specified site and saves the correct once in database
    /// </summary>
    public interface IProxyService
    {
        Task<IEnumerable<WebProxy>> GetProxiesFromSite();

        Task<IEnumerable<WebProxy>> GetExistingProxies();

        Task SaveProxy(string host, int port);
    }
}
