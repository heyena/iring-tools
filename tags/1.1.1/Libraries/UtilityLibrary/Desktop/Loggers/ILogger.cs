using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.utility.Loggers
{
  public interface ILogger 
  {
    bool IsMessageBoxEnabled { get; set; }
    void Log(string message, Category category, Priority priority);
  }
}
