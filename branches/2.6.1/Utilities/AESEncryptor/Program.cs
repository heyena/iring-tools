using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;

namespace AESEncryptor
{
    class Program
    {
        static void Main(string[] args)
        {
            string plainText = string.Empty;
            if (args.Length > 0)
            {
                plainText = args[0];
            }
            else
            {
                Console.Write("Enter text to encrypt: ");
                plainText = Console.ReadLine();
            }

            if (plainText == string.Empty)
            {
                Console.WriteLine("No text to encrypt");
            }
            else
            {
                string cipherText = EncryptionUtility.Encrypt(plainText);
                Console.WriteLine("\n" + cipherText + "\n");
            }

            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }
    }
}
