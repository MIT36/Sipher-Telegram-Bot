using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBot
{
    class AppHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly TelegramServer _telegramServer;
        private readonly IServiceProvider serviceProvider;

        public AppHostedService(ILoggerFactory loggerFactory, TelegramServer telegramServer, IServiceProvider serviceProvider)
        {
            _telegramServer = telegramServer;
            _logger = loggerFactory.CreateLogger(GetType());
            this.serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run App");
            await _telegramServer.StartAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stop App");
            return Task.CompletedTask;
        }
    }
}
