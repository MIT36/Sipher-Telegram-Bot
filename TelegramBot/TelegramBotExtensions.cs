using FeistelCipher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProxiesTelegram;
using TelegramBot.Services;
using TelegramBot.Services.Implementation;

namespace TelegramBot;

public static class TelegramBotExtensions
{
    public static IServiceCollection AddTelegramBotServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<TelegramServerOptions>(options => options.Token = config["TelegramBotToken"]);

        services.AddSingleton<ITelegramServiceOptions, TelegramServerOptions>(sp => sp.GetRequiredService<IOptions<TelegramServerOptions>>().Value);

        services.AddSingleton<TelegramServerOptions>();

        services.AddFeistelSipher();
        services.AddProxiesTelegram(config);
        services.AddSingleton<ITelegramServer, TelegramServer>();
        services.AddScoped<ITextCommand, TextCommand>();
        services.AddScoped<GeneratorBotService>();

        return services;
    }
}
