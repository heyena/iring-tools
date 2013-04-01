using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;
using PrismContrib.Loggers;

namespace PrismContrib.Errors
{
    /// <summary>
    /// Single point of processing for Ontology Errors
    /// </summary>
    public class Error : IError
    {

        #region Logger (IOC)
        private ILoggerFacade logger;
        /// <summary>
        /// Gets the logger - can be set via IOC defaults to DefaultLogger()
        /// </summary>
        /// <value>The logger.</value>
        [Dependency]
        public ILoggerFacade Logger
        {
            get
            {
                // Lazy instantiation if no IOC
                if (logger == null)
                    logger = new DefaultLogger();
                return logger;
            }
            set
            {
                logger = value;
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets a value indicating whether this instance is error.
        /// </summary>
        /// <value><c>true</c> if this instance is error; otherwise, <c>false</c>.</value>
        public bool IsError { get; set; }


        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception
        {
            get
            {
                if (Errors == null || Errors.Count == 0)
                    return null;

                ErrorDetail error = Errors.FirstOrDefault<ErrorDetail>();
                return error.Error;
            }
            set
            {
                SetError(value, value.Message + value.StackTrace,
                    Category.Exception, Priority.None);
            }
        }

        public string FriendlyMessage
        {
            get {
                if (Errors == null)
                    return "";

                ErrorDetail error = Errors.FirstOrDefault<ErrorDetail>();
                if (error == null)
                    return "";

                return error.FriendlyMessage;
            }

        }


        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        /// <value>The errors.</value>
        public List<ErrorDetail> Errors { get; set; }


        /// <summary>
        /// Sets the error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="friendlyMessage">The friendly message.</param>
        /// <returns></returns>
        public bool SetError(
            Exception error, 
            string friendlyMessage,
            Category category, 
            Priority priority)
        {
            IsError = true;

            // Lazy Instantiate
            if (Errors == null)
                Errors = new List<ErrorDetail>();

            Errors.Add(new ErrorDetail
            {
                Error = error,
                FriendlyMessage = friendlyMessage
            });

            // Log the error
            Logger.Log(friendlyMessage, category, priority);

            // So developer can set and return in one line
            return false;
        }

        /// <summary>
        /// Sets the error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public bool SetError(Exception error)
        {
            return SetError(error, error.Message + error.StackTrace, Category.Exception, Priority.High);
        }

        /// <summary>
        /// Sets the error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="friendlyMessage">The friendly message.</param>
        /// <returns></returns>
        public bool SetError(Exception error, string friendlyMessage)
        {
            return SetError(error, friendlyMessage, Category.Exception, Priority.None);
        }

        /// <summary>
        /// Sets the error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="priority">The priority.</param>
        /// <returns></returns>
        public bool SetError(Exception error, Priority priority)
        {
            return SetError(error, error.Message + error.StackTrace, Category.Exception, priority);
        }
    }
}
