using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

internal class ProxySiteService : IProxyService
{
    private readonly ProxyServiceOptions _options;

    public ProxySiteService(IOptions<ProxyServiceOptions> options) => _options = options?.Value;

    public async Task<IEnumerable<WebProxy>> GetProxies()
    {
        string html;
        using var client = new HttpClient();

        client.DefaultRequestHeaders.UserAgent.ParseAdd("1");
        html = await client.GetStringAsync(_options.Site);

        var regex = new Regex(@"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b");
        return regex.Matches(html).Select(x => new WebProxy(x.Value));
    }
}
