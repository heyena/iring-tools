
using Microsoft.Practices.Composite.Logging;
using System.Diagnostics;
using System.Windows;
using System.Windows.Browser;

namespace PrismContrib.Loggers
{
    /// <summary>
    /// Debug Logger.  If Exception with High prior the message
    /// will be shown in dialog box
    /// </summary>
    public class DebugLogger : ILoggerFacade
    {
        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="category">The category.</param>
        /// <param name="priority">The priority.</param>
        public void Log(string message, Category category, Priority priority)
        {
            string catPri = string.Format("{0}({1})", category, priority);
            Debug.WriteLine(message, catPri);

            if(category== Category.Exception && priority== Priority.High)
#if SILVERLIGHT
                HtmlPage.Window.Alert(message);
#else
                MessageBox.Show(message,catPri);
#endif
        
        }

    }
}
