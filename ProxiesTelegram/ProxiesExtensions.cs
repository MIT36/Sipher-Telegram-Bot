using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProxiesTelegram.dbo;
using System;

namespace ProxiesTelegram;

public static class ProxiesDependencyInjectionExtensions
{
    public static IServiceCollection AddProxiesTelegram(this IServiceCollection services, Action<ProxyServiceOptions> action)
    {
        var options = new ProxyServiceOptions();
        action.Invoke(options);

        services.AddDbContext<ProxyDbContext>(builder => builder.UseSqlite(options.ConnectionStringDb));

        services.AddScoped<IProxyService>(sv => new ProxyService(options.Site, sv.GetRequiredService<ProxyDbContext>()));

        return services;
    }
}
