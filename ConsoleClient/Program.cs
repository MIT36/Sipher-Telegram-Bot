using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TelegramBot;

namespace ConsoleClient
{
    class Program
    {
        
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                await RunConsoleHost(args);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
            }
        }

        public static async Task RunConsoleHost(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<AppHostedService>();
            builder.Services.AddTelegramBotServices(builder.Configuration);

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
