using Microsoft.EntityFrameworkCore;
using ProxiesTelegram.dbo;
using ProxiesTelegram.dbo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProxiesTelegram;

internal class ProxyService : IProxyService
{
    private readonly ProxyDbContext _db;
    private readonly string _site;

    public ProxyService(string site, ProxyDbContext dbContext)
    {
        _site = site;
        _db = dbContext;
    }

    public async Task<IEnumerable<WebProxy>> GetProxiesFromSite()
    {
        string html;
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("1");
            html = await client.GetStringAsync(_site);
        }

        var list = new List<WebProxy>();

        var regex = new Regex(@"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b");
        var ipAddresses = regex.Matches(html).Select(m => m.Value).Distinct().ToList();
        if (ipAddresses.Count > 0)
        {
            foreach (var item in ipAddresses)
            {
                list.Add(new WebProxy(item));
                Console.WriteLine(item);
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
