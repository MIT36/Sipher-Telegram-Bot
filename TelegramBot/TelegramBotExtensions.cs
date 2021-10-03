using FeistelCipher;
using Microsoft.Extensions.DependencyInjection;
using ProxiesTelegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Services;
using TelegramBot.Services.Interfaces;

namespace TelegramBot
{
    public static class TelegramBotExtensions
    {
        public static IServiceCollection AddTelegramBotServices(this IServiceCollection services, TelegramBotOptions telegramBotOptions)
        {
            services
                .AddFeistelSipher(telegramBotOptions.KeySipher)
                .AddProxiesTelegram(options =>
                {
                    options.ConnectionStringDb = telegramBotOptions.ConnectionStringProxyDb;
                    options.Site = telegramBotOptions.ProxySite;
                })
                .AddSingleton(sv => new TelegramServerOptions
                {
                    Token = telegramBotOptions.Token,
                    CallbackErrorMessage = telegramBotOptions.CallbackErrorMessage,
                    CallbackSuccessMessage = telegramBotOptions.CallbackSuccessMessage
                })
                .AddSingleton<ITelegramServer, TelegramServer>()
                .AddScoped<ITextCommand, TextCommand>();

            return services;
        }
    }
}
