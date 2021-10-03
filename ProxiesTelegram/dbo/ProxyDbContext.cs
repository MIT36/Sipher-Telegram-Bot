using Microsoft.EntityFrameworkCore;
using ProxiesTelegram.dbo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxiesTelegram.dbo
{
    public class ProxyDbContext : DbContext
    {
        public ProxyDbContext(DbContextOptions<ProxyDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Proxy> Proxies { get; set; }
    }
}
