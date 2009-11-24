using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.utility.Loggers
{
    public interface ILog
    {
        ILogger Logger { get; set; }
    }
}
