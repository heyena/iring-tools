using System;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Logging;

namespace PrismContrib.Errors
{
    public interface IError
    {
        bool IsError { get; set; }
        Exception Exception { get; set; }
        List<ErrorDetail> Errors { get; set; }
        bool SetError(Exception error);
        bool SetError(Exception error, Priority priority);
        bool SetError(Exception error, string friendlyMessage);
        bool SetError(Exception error, string friendlyMessage, Category category, Priority priority);
    }
}
