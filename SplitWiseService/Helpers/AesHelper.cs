using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace SplitWiseService.Helpers;

public class AesHelper
{
    private readonly IConfiguration _configuration;
    public AesHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Encrypt
    public string Encrypt(string text)
    {
        IConfigurationSection AESDetails = _configuration.GetSection("AESDetails");

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(AESDetails["key"]);
            aes.IV = Encoding.UTF8.GetBytes(AESDetails["iv"]);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(text);
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    // Decrypt
    public string Decrypt(string cipherText)
    {
        IConfigurationSection AESDetails = _configuration.GetSection("AESDetails");

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(AESDetails["key"]);
            aes.IV = Encoding.UTF8.GetBytes(AESDetails["iv"]);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (StreamReader swDecrypt = new StreamReader(csDecrypt))
            {
                return swDecrypt.ReadToEnd();
            }
        }
    }

}
