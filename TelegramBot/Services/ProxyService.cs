using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TelegramBot.dbo;
using TelegramBot.dbo.Models;
using TelegramBot.Services.Interfaces;

namespace TelegramBot.Services
{
    class ProxyService
    {
        private readonly SQLiteContext dbContext;
        public ProxyService(SQLiteContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public IEnumerable<WebProxy> GetProxiesFromSite()
        {
            string html;
            using (WebClient client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.UserAgent, "1");
                html = client.DownloadString(Secret.Site);
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
            var dbProxies = await dbContext.Proxies
                    .OrderByDescending(pr => pr.LastConnection)
                    .ToListAsync();
            return dbProxies.Select(pr => new WebProxy(pr.Host, pr.Port));
        }

        public async Task SaveProxy(string host, int port)
        {
            var dbProxy = await dbContext.Proxies.FirstOrDefaultAsync(pr => pr.Host == host && pr.Port == port);
            if (dbProxy != null)
            {
                dbProxy.LastConnection = DateTime.Now;
                dbContext.Update(dbProxy);
            }
            else
            {
                await dbContext.Proxies.AddAsync(new Proxy
                {
                    Id = Guid.NewGuid(),
                    Host = host,
                    Port = port,
                    LastConnection = DateTime.Now
                });
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
