using System;
using System.Threading.Tasks;

namespace TelegramBot.Services;

public interface ITelegramServer
{
    Task StartAsync();
}
