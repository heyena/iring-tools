using System;
using System.Collections.Generic;
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
        Response GetManifest(iRINGEndpoint endpoint);

        [OperationContract]
        Response GetSenderManifest(iRINGEndpoint endpoint);
        
        [OperationContract]
        Response GetReceiverManifest(iRINGEndpoint endpoint);

        [OperationContract]
        Response Refresh(iRINGEndpoint endpoint, string graphName, string identifier);

        [OperationContract]
        Response Reset(iRINGEndpoint endpoint);

        [OperationContract]
        Response Pull(iRINGEndpoint endpoint, iRINGEndpoint targetEnpoint, string graphName);

        [OperationContract]
        Response Export(iRINGEndpoint endpoint, string graphName);

        [OperationContract]
        Response Import(iRINGEndpoint endpoint);

        [OperationContract]
        List<List<SPARQLBinding>> Query(iRINGEndpoint endpoint, string query);

        [OperationContract]
        Response Update(iRINGEndpoint endpoint, string query);
        
        [OperationContract]
        Response GetQXF(Stream stream);
    }
}
