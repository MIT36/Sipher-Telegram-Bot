using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelegramBot;

namespace ConsoleClient
{
    class AppHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly ITelegramServer _telegramServer;

        public AppHostedService(ILoggerFactory loggerFactory, ITelegramServer telegramServer, TelegramServerOptions options)
        {
            _telegramServer = telegramServer;
            _logger = loggerFactory.CreateLogger(GetType());
            options.CallbackSuccessMessage = OnSuccessMessage;
            options.CallbackErrorMessage = OnCallbackErrorMessage;
        }

        private void OnCallbackErrorMessage(object sender, string message)
        {
            _logger.LogError(message);
        }

        private void OnSuccessMessage(object sender, string message)
        {
            _logger.LogInformation(message);
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
