using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Xml;
using org.ids_adi.iring;
using org.ids_adi.qxf;
using org.w3.sparql_results;

namespace org.ids_adi.camelot.demo
{
  [ServiceContract]
  public interface IDemoService
  {
    [OperationContract]
    Response Refresh(iRINGEndpoint endpoint, string graphName, string identifier);

    [OperationContract]
    Response Pull(iRINGEndpoint endpoint, iRINGEndpoint targetEndpoint, string graphName);

    [OperationContract]
    List<List<SPARQLBinding>> Query(iRINGEndpoint endpoint, string query);

    [OperationContract]
    Response Update(iRINGEndpoint endpoint, string query);

    [OperationContract]
    Response GetQXF(Stream stream);

    [OperationContract]
    Response Export(iRINGEndpoint endpoint, string graphName);

    [OperationContract]
    Response Import(iRINGEndpoint endpoint);

    [OperationContract]
    Response Reset(iRINGEndpoint endpoint);

    [OperationContract]
    Response Generate(iRINGEndpoint endpoint);

    [OperationContract]
    Config GetConfig();

    [OperationContract]
    Response GetDictionary(iRINGEndpoint endpoint);

    [OperationContract]
    Response GetSenderDictionary(iRINGEndpoint endpoint);

    [OperationContract]
    Response GetReceiverDictionary(iRINGEndpoint endpoint);
  }
}
