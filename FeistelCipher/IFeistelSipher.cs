using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeistelCipher
{
    public interface IFeistelSipher
    {
        string CryptText(string plainText, bool isDecrypt = false);
    }
}
