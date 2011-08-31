using System;
using org.ids_adi.iring.utility;
using org.iringtools.utility;

namespace org.ids_adi.iring.utility.encryption
{
  class Program
  {
    private const string HELP_MESSAGE =@"
///// EncryptCredentials.exe - iRING Credential Encryption Utility /////

USAGE  EncryptCredentials username password [domain]

  username - name of user.
  passowrd - passowrd of user.
  domain   - (optional) domain od user.
";

    static void Main(string[] args)
    {

      string cred = "JbGWIKHtbRjgS7H88v+nyWB7XZvcvY5X0mBspo5msqR2NuFMbDS6gTRyERh77simqpUFYGvARAzBwjwYOH4wZQ==";
      WebCredentials wcred = new WebCredentials(cred);
      wcred.Decrypt();


      string userName = String.Empty;
      string password = String.Empty;
      string domain = String.Empty;

      if (args.Length >= 2)
      {
        userName = args[0];
        password = args[1];

        if (args.Length > 2)
        {
          domain = args[2];
        }

        WebCredentials credentials = new WebCredentials
        {
          userName = userName,
          password = password,
          domain = domain
        };
        credentials.Encrypt();

        Console.WriteLine(credentials.encryptedToken);
      }
      else
      {
        Console.WriteLine(HELP_MESSAGE);
      }
    }
  }
}
