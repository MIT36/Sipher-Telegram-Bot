using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProxiesTelegram.dbo.Models;

namespace ProxiesTelegram.dbo;

internal class ProxyStorageService(ProxyDbContext dbContext) : IProxyStorageService
{
    public async Task<IEnumerable<WebProxy>> GetProxies()
    {
        var dbProxies = await dbContext.Proxies
                .OrderByDescending(pr => pr.LastConnection)
                .ToListAsync();
        return dbProxies.Select(pr => new WebProxy(pr.Host, pr.Port));
    }

    public async Task SaveProxy(string host, int port)
    {
        var dbProxy = await dbContext.Proxies.SingleOrDefaultAsync(pr => pr.Host == host && pr.Port == port);
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
