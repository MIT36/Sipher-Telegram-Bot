namespace TelegramBot.Services;

interface ITextCommand
{
    string GetText(string message);
}
