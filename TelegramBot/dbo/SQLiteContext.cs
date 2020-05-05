using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TelegramBot.dbo.Models;

namespace TelegramBot.dbo
{
    public class SQLiteContext : DbContext
    {
        public SQLiteContext()
        {
            Database.EnsureCreated();
        }

        public DbSet<Proxy> Proxies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(Secret.ConnectionString);
        }
    }
}
