using Microsoft.Practices.Composite.Logging;
using PrismContrib.Loggers;

namespace OntologyService.Interface.Loggers
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
            // TODO: Write logger - for now use DebugLogger
            defaultLogger.Log(message, category, priority);
        }

    }
}
