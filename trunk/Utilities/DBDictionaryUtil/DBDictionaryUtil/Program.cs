using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using org.iringtools.utility;
using org.iringtools.library;

namespace DBDictionaryUtil
{
  class Program
  {
    static void Main(string[] args)
    {
      string adapterServiceUri = String.Empty;
      string dbDictionaryFilePath = String.Empty;
      string projectName = String.Empty;
      string applicationName = String.Empty;

      if (args.Length >= 4)
      {
        adapterServiceUri = args[0];
        projectName = args[1];
        applicationName = args[2];
        dbDictionaryFilePath = args[3];
      }
      else
      {
        adapterServiceUri = ConfigurationManager.AppSettings["AdapterServiceUri"];
        projectName = ConfigurationManager.AppSettings["ProjectName"];
        applicationName = ConfigurationManager.AppSettings["ApplicationName"];
        dbDictionaryFilePath = ConfigurationManager.AppSettings["dbDictionaryFilePath"];
      }

      if (!String.IsNullOrEmpty(adapterServiceUri) &&
          !String.IsNullOrEmpty(projectName) &&
          !String.IsNullOrEmpty(applicationName) && 
          !String.IsNullOrEmpty(dbDictionaryFilePath))
      {
        Console.WriteLine("Posting " + dbDictionaryFilePath + " to iRING Adapter Service...");

        try
        {
          string relativeUri = "/" + projectName + "/" + applicationName + "/dbdictionary";
          DatabaseDictionary dbDictionary = Utility.Read<DatabaseDictionary>(dbDictionaryFilePath);
          WebHttpClient httpClient = new WebHttpClient(adapterServiceUri, null);
          Response response = httpClient.Post<DatabaseDictionary, Response>(relativeUri, dbDictionary, true);
          
          foreach (string line in response)
          {
            Console.WriteLine(line);
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
        }
      }
      else
      {
        Console.WriteLine("Usage: \n\tDBDictionaryUtil.exe AdapterServiceUri ProjectName ApplicationName DatabaseDictionaryFilePath\n");
      }

      Console.WriteLine("Press any key to continue ...");
      Console.ReadKey();
    }
  }
}
