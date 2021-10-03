using Microsoft.EntityFrameworkCore;
using ProxiesTelegram.dbo;
using ProxiesTelegram.dbo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProxiesTelegram
{
    internal class ProxyService : IProxyService
    {
        private readonly ProxyDbContext _db;
        private readonly string _site;
        public ProxyService(string site, ProxyDbContext dbContext)
        {
            _site = site;
            _db = dbContext;
        }
        public IEnumerable<WebProxy> GetProxiesFromSite()
        {
            string html;
            using (WebClient client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.UserAgent, "1");
                html = client.DownloadString(_site);
            }

            var list = new List<WebProxy>();

            Regex regex = new Regex(@"\d+(.)\d+(.)\d+(.)\d+(\<\/td><td>)\d+");
            MatchCollection matches = regex.Matches(html);
            if (matches.Count > 0)
            {
                foreach (Match item in matches)
                {
                    string hostPort = item.Value.Replace(@"</td><td>", ":");
                    if (hostPort.Contains('.'))
                    {
                        list.Add(new WebProxy(hostPort));
                        Console.WriteLine(hostPort);
                    }
                }
            }
            return list;
        }

        public async Task<IEnumerable<WebProxy>> GetExistingProxies()
        {
            var dbProxies = await _db.Proxies
                    .OrderByDescending(pr => pr.LastConnection)
                    .ToListAsync();
            return dbProxies.Select(pr => new WebProxy(pr.Host, pr.Port));
        }

        public async Task SaveProxy(string host, int port)
        {
            var dbProxy = await _db.Proxies.SingleOrDefaultAsync(pr => pr.Host == host && pr.Port == port);
            if (dbProxy != null)
            {
                dbProxy.LastConnection = DateTime.Now;
                _db.Update(dbProxy);
            }
            else
            {
                await _db.Proxies.AddAsync(new Proxy
                {
                    Id = Guid.NewGuid(),
                    Host = host,
                    Port = port,
                    LastConnection = DateTime.Now
                });
            }
            await _db.SaveChangesAsync();
        }
    }
}
