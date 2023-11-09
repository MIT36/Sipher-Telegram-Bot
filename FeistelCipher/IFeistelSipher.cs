namespace FeistelCipher;

public interface IFeistelSipher
{
    string CryptText(string plainText, bool isDecrypt = false);
}
