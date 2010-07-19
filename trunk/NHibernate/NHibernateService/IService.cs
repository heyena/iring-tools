using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using org.iringtools.library;

namespace org.iringtools.application
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/{project}/{application}/dictionary")]
        DatabaseDictionary GetDictionary(string project, string application);

        [OperationContract]
        [WebGet(UriTemplate = "/{project}/{application}/schema")]
        DatabaseDictionary GetDatabaseSchema(string project, string application);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/{project}/{application}/dictionary")]
        Response PostDictionary(string project, string application, DatabaseDictionary dict);

        [OperationContract]
        [WebGet(UriTemplate = "/{project}/{application}/generate")]
        Response Generate(string project, string application);

        [OperationContract]
        [WebGet(UriTemplate = "/providers")]
        string[] GetProviders();
    }
}
