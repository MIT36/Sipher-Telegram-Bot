using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProxiesTelegram;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using TelegramBot.Models;

namespace TelegramBot.Services.Implementation;

internal class GeneratorBotService
{
    private readonly ITelegramServiceOptions _options;

    private readonly IProxyStorageService _proxyStorage;

    private readonly IProxyService _proxyService;

    private readonly ILogger _logger;

    public GeneratorBotService(IServiceProvider services)
    {
        _options = services.GetRequiredService<ITelegramServiceOptions>();
        _proxyStorage = services.GetRequiredService<IProxyStorageService>();
        _proxyService = services.GetRequiredService<IProxyService>();
        _logger = services.GetRequiredService<ILogger<GeneratorBotService>>();
    }

    public async Task<TelegramBotData> ConnectAndGetTelegramBot(Action<string> sendMessage = null, Action<string> sendErrorMessage = null)
    {
        var bot = await GenerateBot();
        if (bot is not null)
            return new TelegramBotData(true, bot, null);

        var data = await TryConnectWithProxies(_proxyStorage, sendMessage, sendErrorMessage);
        if (data.IsConnected)
            return data;

        data = await TryConnectWithProxies(_proxyService, sendMessage, sendErrorMessage);
        if (data.IsConnected)
        {
            await _proxyStorage.SaveProxy(data.Proxy.Address.Host, data.Proxy.Address.Port);
            return data;
        }

        return new(false);
    }

    private async Task<TelegramBotData> TryConnectWithProxies<TProxyService>(TProxyService service, Action<string> sendMessage = null, Action<string> sendErrorMessage = null)
        where TProxyService : IProxyService
    {
        var proxiesFromSite = await service.GetProxies();
        var telegramBotDataWithSiteProxy = await TryConnectWithProxies(proxiesFromSite, sendMessage, sendErrorMessage);
        if (telegramBotDataWithSiteProxy.IsConnected)
            return telegramBotDataWithSiteProxy;
        return new(false);
    }

    private async Task<TelegramBotData> TryConnectWithProxies(IEnumerable<WebProxy> proxies, Action<string> sendMessage = null, Action<string> sendErrorMessage = null)
    {
        foreach (var proxy in proxies)
        {
            sendMessage?.Invoke($"Connecting to telegram bot with proxy: {proxy.Address.Host}:{proxy.Address.Port}...");
            try
            {
                var bot = await GenerateBot(proxy);
                if (bot is not null)
                    return new(true, bot, proxy);
            }
            catch (RequestException ex)
            {
                _logger.LogError($"Source: {ex.Source}:{ex.Message}/r/n{ex.StackTrace}");
                sendErrorMessage?.Invoke($"Proxy failed: {proxy.Address.Host}:{proxy.Address.Port}");
            }

        }
        return new(false);
    }

    private async Task<ITelegramBotClient> GenerateBot(WebProxy proxy = null)
    {
        var telegramBotClient = CreateBot(proxy);
        await telegramBotClient.GetMeAsync();
        return telegramBotClient;
    }

    private TelegramBotClient CreateBot(WebProxy proxy = null)
    {
        if (proxy is null)
            return new TelegramBotClient(_options.Token);

        var httpClientHandler = new HttpClientHandler
        {
            Proxy = proxy
        };

        return new TelegramBotClient(_options.Token, new HttpClient(httpClientHandler));
    }
}