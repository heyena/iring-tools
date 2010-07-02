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
        [WebGet(UriTemplate = "/{project}/{application}/dbdictionary")]
        DatabaseDictionary GetDbDictionary(string project, string application);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/dbschema")]
        DatabaseDictionary GetDatabaseSchema(Request request);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/{project}/{application}/dbdictionary")]
        Response SaveDatabaseDictionary(string project, string application, DatabaseDictionary dict);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/{project}/{application}/dbdictionary/commit")]
        Response UpdateDatabaseDictionary(string project, string application, DatabaseDictionary dict);

        [OperationContract]
        [WebGet(UriTemplate = "/dbdictionaries")]
        List<string> GetExistingDbDictionaryFiles();

        [OperationContract]
        [WebGet(UriTemplate = "/providers")]
        string[] GetProviders();
    }  
}
