using System;
using Microsoft.Practices.Unity;
namespace Library.Interface.Configuration
{
    public interface IServerConfiguration : IAppConfiguration
    {
        string WebServerURL { get; }
        string WebServerPath { get; }
        string WebSiteName { get; }

        string OntologyServiceWCF { get; }
        string AdapterProxy { get; }

        string AdapterServiceUri { get; }
        string ReferenceDataServiceUri { get; }

    }
}
