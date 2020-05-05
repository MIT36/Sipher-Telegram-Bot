using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBot.Services.Interfaces
{
    interface IFeistelSipher
    {
        string CryptText(string plainText, bool isDecrypt = false);
    }
}
