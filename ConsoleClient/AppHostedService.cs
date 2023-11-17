using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TelegramBot.Services;

namespace ConsoleClient;

class AppHostedService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly ITelegramServer _telegramServer;

    public AppHostedService(ILoggerFactory loggerFactory, ITelegramServer telegramServer, ITelegramServiceOptions options)
    {
        _telegramServer = telegramServer;
        _logger = loggerFactory.CreateLogger(GetType());
        options.CallbackSuccessMessage = OnSuccessMessage;
        options.CallbackErrorMessage = OnCallbackErrorMessage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Run App");
        try
        {
            await _telegramServer.StartAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex.Source}: {ex.Message}\r\n{ex.StackTrace}");
        }
    }

    private void OnCallbackErrorMessage(object sender, string message)
    {
        _logger.LogError(message);
    }

    private void OnSuccessMessage(object sender, string message)
    {
        _logger.LogInformation(message);
    }
}
