using Microsoft.EntityFrameworkCore;
using ProxiesTelegram.dbo.Models;

namespace ProxiesTelegram.dbo;

public class ProxyDbContext : DbContext
{
    public ProxyDbContext(DbContextOptions<ProxyDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Proxy> Proxies { get; set; }
}
