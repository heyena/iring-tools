using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.utility.Loggers
{
    public class LoggerHelper : IDisposable
    {
        private string assembly;
        private string message;
        private ILogger logger;
        public Category Category { get; set; }
        public Priority Priority { get; set; }

        public LoggerHelper(object sender, string methodName, string para)
        {
            string newMessage = string.Format("{0}({1})", methodName,para);
            logIt(sender, newMessage);
        }

        public LoggerHelper(object sender, string message)
        {
            logIt(sender, message);
        }

        public void logIt(object sender, string message)
        {
            this.assembly = sender.GetType().FullName;
            this.message = message;
            this.logger = ((ILog)sender).Logger;

            logger.Log(string.Format("{1}: {0} STARTED",
                message, assembly), Category, Priority);
        }

        public void Dispose()
        {
            logger.Log(string.Format("{1}: {0} COMPLETED",
                message, assembly), Category, Priority);
        }

    }
}
