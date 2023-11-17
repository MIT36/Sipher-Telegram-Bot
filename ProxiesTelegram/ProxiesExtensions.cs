using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProxiesTelegram.dbo;
using System;

namespace ProxiesTelegram;

public static class ProxiesDependencyInjectionExtensions
{
    public static IServiceCollection AddProxiesTelegram(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<ProxyServiceOptions>(options =>
        {
            options.ConnectionStringDb = config.GetConnectionString("DefaultConnection");
            options.Site = config["Site"];
        });

        services.AddDbContext<ProxyDbContext>(builder => builder.UseSqlite(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IProxyStorageService, ProxyStorageService>();
        services.AddScoped<IProxyService, ProxySiteService>();
        
        return services;
    }
}
