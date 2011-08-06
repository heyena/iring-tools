using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Security.Cryptography;

namespace org.iringtools.utility
{
  public static class EncryptionUtility
  {
    private static readonly ILog logger = LogManager.GetLogger(typeof(EncryptionUtility));
    private static String SECRET_KEY = "secret";  
    private static RijndaelManaged cipher;

    static EncryptionUtility()
    {
      try
      {
        byte[] keyBytes = new byte[0x10];
        byte[] buf = Encoding.UTF8.GetBytes(SECRET_KEY);
        int len = (buf.Length > keyBytes.Length) ? keyBytes.Length : buf.Length;
        Array.Copy(buf, keyBytes, len);

        cipher = new RijndaelManaged()
        {
          Mode = CipherMode.CBC,
          Padding = PaddingMode.PKCS7,
          KeySize = 0x80,
          BlockSize = 0x80,
          Key = keyBytes,
          IV = keyBytes
        };
      }
      catch (Exception e)
      {
        logger.Error("Error initializing cryptor: " + e);
      }
    }

    public static string Encrypt(string plainText)
    {
      try
      {
        ICryptoTransform encryptor = cipher.CreateEncryptor();
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length));
      }
      catch (Exception e)
      {
        String message = "Error encrypting [" + plainText + "]" + e;
        logger.Error(message);
        throw new Exception(message);
      }
    }

    public static string Decrypt(string cipherText)
    {
      try
      {
        ICryptoTransform decryptor = cipher.CreateDecryptor();
        byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
        byte[] plainText = decryptor.TransformFinalBlock(cipherTextBytes, 0, cipherTextBytes.Length);
        return Encoding.UTF8.GetString(plainText);
      }
      catch (Exception e)
      {
        String message = "Error decrypting [" + cipherText + "]" + e;
        logger.Error(message);
        throw new Exception(message);
      }
    }
  }
}
