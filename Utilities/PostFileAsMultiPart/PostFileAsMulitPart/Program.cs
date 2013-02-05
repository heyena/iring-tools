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
      string Scope = string.Empty;
      string App = string.Empty;

      try
      {
          string fileName = string.Empty;
          if (args.Length >= 6)
          {
              baseUrl = args[0];
              relativeUrl = args[1];
              filePath = args[2];
              mimeType = args[3];
              Scope = args[4];
              App = args[5];

              FileInfo info = new FileInfo(filePath);

              Stream stream = Utility.ReadStream(filePath);

              WebHttpClient webClient = new WebHttpClient(baseUrl);

              fileName = Scope + "." + App + "." + info.Name;            // Saving file as {Scope}.{App}.fileName
              MultiPartMessage message = new MultiPartMessage            // so that It can be easily identified during download.
              {
                  fileName = fileName,            //info.Name,
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
      catch (Exception ex)
      { 
       
      }
    }
  }
}
