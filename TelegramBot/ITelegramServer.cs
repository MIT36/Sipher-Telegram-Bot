using System;
using System.Threading.Tasks;

namespace TelegramBot;

public interface ITelegramServer
{
    Task StartAsync();
}
