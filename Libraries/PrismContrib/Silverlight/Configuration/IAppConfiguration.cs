using System;
using Microsoft.Practices.Unity;

namespace Library.Interface.Configuration
{
    public interface IAppConfiguration
    {
        string GetParameter(string key);
    }
}
