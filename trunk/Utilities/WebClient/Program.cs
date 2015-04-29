using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using log4net;
using System.Net;

namespace Bechtel.iRING.WebClient
{
    class Program
    {
        private static ILog _logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            if (args.Count() > 0)
            {
                string url = args[0];

                string[] seperator = 
                {
                    "://",
                    "/",
                    "?",
                    "&",
                    "="
                };

                string[] urlparts = url.Split(seperator, StringSplitOptions.None);

                int partCount = urlparts.Count();

                if (partCount == 0)
                {
                    Console.WriteLine("Invalid Arguments!");
                }
                else
                {
                    //StringBuilder baseUrl = new StringBuilder();
                    //StringBuilder relativeUrl = new StringBuilder(); 
                        
                    //baseUrl.Append(urlparts[0]);
                    
                    //if (partCount > 1)
                    //{
                    //    baseUrl.Append(urlparts[1]);

                    //    for (int i = 2; i <= partCount - 1; i++)
                    //    {
                    //        relativeUrl.Append(urlparts[i]);
                    //    }
                    //}

                    _logger.Debug("Calling: GET " + url);

                    WebHttpClient webClient = new WebHttpClient(url);

                    string msg = webClient.GetMessage(String.Empty);

                    Console.WriteLine(msg);
                    Console.ReadKey();
                }
            }
        }
    }
}
