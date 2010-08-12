﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using org.iringtools.utility;
using org.ids_adi.qmxf;
using org.iringtools.library;

namespace TestPost
{
    class Program
    {
        static void Main(string[] args)
        {
          /*string dataDictionaryXmlPath = @"..\..\DataDictionary.xml";

          DatabaseDictionary dataDictionary = Utility.Read<DatabaseDictionary>(dataDictionaryXmlPath, true);

          string baseUri = "http://localhost:62451/Service.svc";
          string relativeUri = "/dictionary/generate";
          WebHttpClient client = new WebHttpClient(baseUri);
          Response response = client.Post<DatabaseDictionary, Response>(relativeUri, dataDictionary, false);

          foreach (string message in response)
          {
            Console.Write(message);
          }

          Console.ReadKey();
          */

          
          string qmxfPath = @"..\..\testing-QMXF.xml";
          QMXF qmxf = Utility.Read<QMXF>(qmxfPath, false);

          string baseUri = "http://localhost:62460/";
          string relativeUri = "part8/templates";
          WebHttpClient client = new WebHttpClient(baseUri);
          Response response = client.Post<QMXF, Response>(relativeUri, qmxf, false);

          foreach (Status status in response.StatusList)
          {
              foreach (String message in status.Messages)
              {
                  Console.Write(message);
              }
          }
          
          Console.ReadKey();
          
        }
    }
}
