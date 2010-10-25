using System;
using Microsoft.Practices.Unity;

namespace org.iringtools.library.presentation.configuration
{
    public interface IAppConfiguration
    {
        string GetParameter(string key);
    }
}
