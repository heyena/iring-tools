
using Microsoft.Practices.Composite.Logging;
namespace PrismContrib.Loggers
{
    public class DefaultLogger : ILoggerFacade
    {
        ILoggerFacade logger = new DebugLogger();

        public void Log(string message, Category category, Priority priority)
        {
            logger.Log(message, category, priority);
        }

    }
}
