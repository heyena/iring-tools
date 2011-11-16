using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.utility.Loggers
{
  /// <summary>
  /// Unless logging requirements suggest otherwise
  /// this should be the logger pointed to (single
  /// entry point)
  /// </summary>
  public class DefaultLogger : ILogger
  {
    // Logger to use for system wide applications
    ILogger defaultLogger = new DebugLogger();

    // Set to true to receive messageboxes on
    // errors (for development)
    private bool IsMessagBoxEnabledOverride = false;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is message box enabled.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is message box enabled; otherwise, <c>false</c>.
    /// </value>
    public bool IsMessageBoxEnabled { get; set; }


    /// <summary>
    /// Logs the specified message.
    /// 
    /// Provides easy mechanism to change the logger
    /// application wide (assuming DefaultLogger is used)
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="category">The category.</param>
    /// <param name="priority">The priority.</param>
    public void Log(string message, Category category, Priority priority)
    {
      // Use defaultlogger setting for messagebox
      defaultLogger.IsMessageBoxEnabled = 
        IsMessageBoxEnabled || IsMessagBoxEnabledOverride;  

      // Use configured logger to process entry
      defaultLogger.Log(message, category, priority);
    }

  }
}
