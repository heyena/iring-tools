using System;
using Microsoft.Practices.Unity;
namespace org.iringtools.library.presentation.configuration
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
        string FacadeServiceUri { get; }

        string BaseAddress { get; }
    }
}
