using Microsoft.Practices.Composite.Logging;
using PrismContrib.Loggers;

namespace org.iringtools.ontologyservice.presentation.loggers
{
    /// <summary>
    /// Ontology Default Logger
    /// </summary>
    public class DefaultLogger : ILoggerFacade
    {
        ILoggerFacade defaultLogger = new DebugLogger();

        /// <summary>
        /// Single entry point for Ontology Logging
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="category">The category.</param>
        /// <param name="priority">The priority.</param>
        public void Log(string message, Category category, Priority priority)
        {
            defaultLogger.Log(message, category, priority);
        }

    }
}
