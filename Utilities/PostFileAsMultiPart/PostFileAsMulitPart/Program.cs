using System;
using org.ids_adi.iring.utility;
using org.iringtools.utility;
using System.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace org.ids_adi.iring.utility.webclient
{
  class Program
  {
    private const string HELP_MESSAGE = @"
///// PostAsMultiPart.exe - iRING File POST Utility /////

USAGE  PostAsMultiPart baseUrl relativeUrl filePath mimeType

  baseUrl - Base URL of webservice.
  relativeUrl - Relative URL of webservice
  filePath - Path of file to POST.
  mimeType - mime-type of the file to POST.
";

     [STAThread] 
    static void Main(string[] args)
    {
      string baseUrl = String.Empty;
      string relativeUrl = String.Empty;
      string filePath = String.Empty;
      string mimeType = String.Empty;

      if (args.Length >= 4)
      {
        baseUrl = args[0];
        relativeUrl = args[1];
        filePath = args[2];
        mimeType = args[3];

        FileInfo info = new FileInfo(filePath);

        Stream stream = Utility.ReadStream(filePath);

        WebHttpClient webClient = new WebHttpClient(baseUrl);

        MultiPartMessage message = new MultiPartMessage
        {
          fileName = info.Name,
          message = stream,
          mimeType = mimeType,
          type = MultipartMessageType.File
        };

        List<MultiPartMessage> requestMessages = new List<MultiPartMessage>
        {
          message,
        };

        string responseTxt = webClient.PostMultipartMessage(relativeUrl, requestMessages, true);

        Console.WriteLine(responseTxt);

        Console.ReadKey();
      }
      else
      {
        Console.WriteLine(HELP_MESSAGE);
      }
    }
  }
}
