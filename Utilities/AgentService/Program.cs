
using System.ServiceProcess;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using log4net;

namespace iRINGAgentService
{

    static class Program
    {
        private static ILog _logger = LogManager.GetLogger(typeof(Program));
       
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower().Equals("/debug")) 
            { 
                // run as console 
                try 
                { 
                    iRINGAgentService service = new iRINGAgentService(); 
                    service.RunConsole(args); 
                } 
                catch (Exception ex) 
                {
                    throw ex;
                } 
            } 
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
			    { 
				    new iRINGAgentService() 
			    };
                ServiceBase.Run(ServicesToRun);
            }
        }
       
    }
}
