using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using org.iringtools.library;
using org.iringtools.utility;
using org.w3.sparql_results;

namespace DemoControlPanel.Web
{
    // NOTE: If you change the interface name "IDemoControlPanelService" here, you must also update the reference to "IDemoControlPanelService" in Web.config.
    [ServiceContract]
    public interface IDemoControlPanelService
    {
        [OperationContract]
        ConfiguredEndpoints GetConfiguredEndpoints();

        [OperationContract]
        Response GetManifest(iRINGEndpoint endpoint, string projectName, string applicationName);

        [OperationContract]
        Response GetSenderManifest(iRINGEndpoint endpoint, string projectName, string applicationName);
        
        [OperationContract]
        Response GetReceiverManifest(iRINGEndpoint endpoint, string projectName, string applicationName);

        [OperationContract]
        Response Refresh(iRINGEndpoint endpoint, string projectName, string applicationName, string graphName, string identifier);

        [OperationContract]
        Response Reset(iRINGEndpoint endpoint, string projectName, string applicationName);

        [OperationContract]
        Response Pull(Scenario scenario, iRINGEndpoint endpoint, iRINGEndpoint targetEnpoint, string graphName);

        [OperationContract]
        Response Export(iRINGEndpoint endpoint, string projectName, string applicationName, string graphName);

        [OperationContract]
        Response Import(iRINGEndpoint endpoint, string projectName, string applicationName);

        [OperationContract]
        Collection<Collection<SPARQLBinding>> Query(iRINGEndpoint endpoint, string projectName, string applicationName, string query);

        [OperationContract]
        Response Update(iRINGEndpoint endpoint, string projectName, string applicationName, string query);
        
        [OperationContract]
        Response GetQXF(Stream stream);

        [OperationContract]
        Collection<ScopeProject> GetScopes(iRINGEndpoint endpoint);

        [OperationContract]
        Collection<ScopeProject> GetInterfaceScopes(iRINGEndpoint endpoint);
    }
}
