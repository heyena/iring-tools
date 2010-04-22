using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using org.iringtools.library;
using System.Net;

namespace MappingPoster
{
  class Program
  {
    static void Main(string[] args)
    {
       
      if (args.Count() < 4)
      {
        Console.WriteLine("USAGE: mappingposter mappingFile baseUri project application [/p]");
        Console.WriteLine();
        Console.WriteLine("mappingFile - The mapping XML file");
        Console.WriteLine("baseUri - The base Uri to the AdapterService");
        Console.WriteLine("project - The project name");
        Console.WriteLine("application - The application name");
        Console.WriteLine("/p - use proxy.  For internal use.");
      }
      else
      {
        WebProxy proxy = null;
        if (args.Count() == 5 && args[4].ToUpper() == "/P")
        {
          proxy = new WebProxy("ashs-proxy", 8080);
          proxy.Credentials = CredentialCache.DefaultCredentials;
        }

        WebHttpClient client = new WebHttpClient(args[1], null, proxy);

        Mapping mapping = Utility.Read<Mapping>(args[0], false);

        Response response = client.Post<Mapping, Response>("/" + args[2] + "/" + args[3] + "/mapping", mapping, false);

        foreach (String msg in response)
        {
          Console.WriteLine(msg);
        }
      }

      Console.WriteLine("Press any key to close...");
      Console.ReadKey();
    }
  }
}
