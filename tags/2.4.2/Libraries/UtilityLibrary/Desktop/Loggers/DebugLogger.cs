using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;

#if SILVERLIGHT
using System.Windows.Browser;
#endif

namespace org.iringtools.utility.Loggers
{
  /// <summary>
  /// Debug Logger.  If Exception with High prior the message
  /// will be shown in dialog box
  /// </summary>
  public class DebugLogger : ILogger
  {
    public bool IsMessageBoxEnabled {get; set; }

    /// <summary>
    /// Logs the specified message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="category">The category.</param>
    /// <param name="priority">The priority.</param>
    public void Log(string message, Category category, Priority priority)
    {
      string catPri = string.Format("{0}({1})", category, priority);
      message = DateTime.Now.ToLongTimeString() + " " + message;
      Debug.WriteLine(message, catPri);

      if (category == Category.Exception && priority == Priority.High)
#if SILVERLIGHT
        HtmlPage.Window.Alert(message);
#else
        // By default we don't want a messagebox to display
        // for Exceptions (High Priority) because it is being
        // used by web services.  Enable during development
        if (IsMessageBoxEnabled)
          MessageBox.Show(message, catPri);
#endif

    }

  }
}
