using System;
using Microsoft.Practices.Composite.Logging;
namespace org.iringtools.library.presentation.loggers
{
    public interface IAppBase
    {
        Exception Error { set; }
        Exception GetCurrentError();
        System.Collections.Generic.List<Exception> GetErrorList();
        string GetErrorMessages();
        Exception GetFirstError();
        bool IsError { get; set; }
        void Log(string message);
        void Log(string message, Category category, Priority priority);
        void LogDebug(string message);
        void LogDebug(string message, Priority priority);
        void LogError(string message, Priority priority);
        ILoggerFacade Logger { get; set; }
        void LogInformation(string message, Priority priority);
        void LogWarning(string message, Priority priority);
        void WriteErrorLog(Exception ex, Priority priority);
    }
}
