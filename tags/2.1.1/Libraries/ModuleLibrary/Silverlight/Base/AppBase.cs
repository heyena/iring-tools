using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.Composite.Logging;
using System.Diagnostics;
using org.iringtools.library.presentation.events;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Composite.Events;
using org.iringtools.library.presentation.loggers;
using PrismContrib.Loggers;

namespace org.iringtools.library.presentation
{
    public enum TraceTypes {None,Textfile, EventLog, Debug };

    /// <summary>
    /// Application Base Class - contains common components
    /// </summary>
    public class AppBase : ILoggerFacade, IAppBase
    {
        protected IUnityContainer Container { get; set; }
       
        private List<Exception> Errors = new List<Exception>();
        private StringBuilder ErrorMessages = new StringBuilder();
        private Exception _error;

        /// <summary>
        /// True if an error occurred
        /// </summary>
        public bool IsError { get; set; }
        /// <summary>
        /// Logger - logs as configured
        /// </summary>
        public ILoggerFacade Logger { get; set; }
       
        /// <summary>
        /// Error occurred - assign error to this property
        /// </summary>
        public Exception Error
        {
            private get { return _error; }
            set
            {
                IsError = true;
                _error = value;
                Errors.Add(value);
                ErrorMessages.AppendFormat("{0}\r\n{1}\r\n", _error.Message, _error.StackTrace);
                WriteErrorLog(_error, Priority.High);
            }
        }



        public AppBase() {
            // Default logger is Debug window
            Logger = new DebugLogger();
        }

 
        /// <summary>
        /// Constructor
        /// </summary>
        public AppBase(IUnityContainer container) : this()
        {
            Container = container;
        }


        /// <summary>
        /// Tracks number of services currently being executed async
        /// so that we'll know when we're done
        /// </summary>
        private int serviceCount;

        /// <summary>
        /// Executed when a service starts
        /// </summary>
        public void StartService(object sender, ServiceEventArgs e)
        {
            // This base class many not always be instantiated via dependency injection
            // it may have to be instantiated manually to access loggers
            if (Container == null)
                return;

            serviceCount++;

            LogDebug(string.Format("SPINNER: START {1} => ServiceCount={0}", serviceCount, e.ServiceName));

            if (e == null)
                e = new ServiceEventArgs();

            e.Process = ServiceProcessType.Starting;

            IEventAggregator aggregator = Container.Resolve<IEventAggregator>();
            aggregator.GetEvent<ServiceEvent>().Publish(e);
        }

        /// <summary>
        /// Executed when a service stops
        /// </summary>
        public void StopService(object sender, ServiceEventArgs e)
        {
            // This base class many not always be instantiated via dependency injection
            // it may have to be instantiated manually to access loggers
            if (Container == null)
                return;

            serviceCount--;

            // If there are still services running don't tell the spinner to stop
            if (serviceCount > 0)
            {
                LogDebug(string.Format("SPINNER: STOP {1} => ServiceCount={0}", serviceCount,e.ServiceName));
                return;
            }

            if (e == null)
                e = new ServiceEventArgs();

            e.Process = ServiceProcessType.Stopped;

            IEventAggregator aggregator = Container.Resolve<IEventAggregator>();
            aggregator.GetEvent<ServiceEvent>().Publish(e);

        }


        /// <summary>
        /// Get most recent error
        /// </summary>
        /// <returns></returns>
        public Exception GetCurrentError()
        {
            return Error;
        }

        /// <summary>
        /// Multiple errors could have resulted from first error;
        /// get first error
        /// </summary>
        /// <returns></returns>
        public Exception GetFirstError()
        {
            if(!IsError)
                return new Exception("No Errors");

            return Errors[0];
        }

        /// <summary>
        /// Get complete error list
        /// </summary>
        /// <returns></returns>
        public List<Exception> GetErrorList()
        {
            return Errors;
        }

        /// <summary>
        /// Get string value of all error messages
        /// </summary>
        /// <returns></returns>
        public string GetErrorMessages()
        {
            return ErrorMessages.ToString();
        }

        /// <summary>
        /// Write an error to the log
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="priority"></param>
        public void WriteErrorLog(Exception ex, Priority priority)
        {
            Logger.Log(ex.Message + ex.StackTrace, Category.Exception, priority);            
        }

        /// <summary>
        /// Separator types
        /// </summary>
        public enum SeparatorType {BlankLine, DashedLines, Arrows};

        /// <summary>
        /// Logs the separator.
        /// </summary>
        /// <param name="type">The type.</param>
        public void LogSeparator(SeparatorType type)
        {
            switch (type)
            {
                case SeparatorType.BlankLine:
                    Debug.WriteLine("");
                    break;
                case SeparatorType.DashedLines:
                    Debug.WriteLine("=".PadRight(20, '='));
                    break;
                case SeparatorType.Arrows:
                    Debug.WriteLine("<".PadRight(19, '<') + ">".PadRight(19, '>'));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Log messages to the configured logger
        /// </summary>
        /// <param name="message"></param>
        /// <param name="category"></param>
        /// <param name="priority"></param>
        public virtual void Log(string message, Category category, Priority priority)
        {
            if (Logger != null)
                Logger.Log(message, category, priority);
            else
                Debug.WriteLine(string.Format("{1}({2}): {0}", 
                    message, category, priority));
        }

        public virtual void Log(string message)
        {
            Logger.Log(message, Category.Info, Priority.None);
        }

        public virtual void LogWarning(string message, Priority priority)
        {
            Logger.Log(message, Category.Warn, priority);
        }

        public virtual void LogError(string message, Priority priority)
        {
            Logger.Log(message, Category.Exception, priority);
        }

        public virtual void LogInformation(string message, Priority priority)
        {
            Logger.Log(message, Category.Info, priority);
        }
        public virtual void LogDebug(string message)
        {
            Logger.Log(message, Category.Debug, Priority.None);
        }

        public virtual void LogDebug(string message, Priority priority)
        {
            Logger.Log(message, Category.Debug, priority);
        }

        #region ILogger Members

        public virtual void Log(string p, string category, string priority)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
