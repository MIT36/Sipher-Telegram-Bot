using System.Net;
using Telegram.Bot;

namespace TelegramBot.Models;

internal record TelegramBotData(bool IsConnected, ITelegramBotClient Bot = null, WebProxy Proxy = null);