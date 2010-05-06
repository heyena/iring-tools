using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using org.iringtools.library;
using System.Collections.ObjectModel;

namespace DbDictionaryService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/scopes")]
        Collection<ScopeProject> GetScopes();

        [OperationContract]
        [WebGet(UriTemplate = "/{project}/{application}/dbdictionary")]
        DatabaseDictionary GetDbDictionary(string project, string application);

        [OperationContract]
        [WebGet(UriTemplate = "/{connString}/{dbProvider}/dbschema")]
        DatabaseDictionary GetDatabaseSchema(string connString, string dbProvider);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/{project}/{application}/savedbdictionary")]
        Response SaveDatabaseDictionary(string project, string application, DatabaseDictionary dict);

        [OperationContract]
        [WebGet(UriTemplate = "/dbdictionaries")]
        List<string> GetExistingDbDictionaryFiles();

        [OperationContract]
        [WebGet(UriTemplate = "/providers")]
        string[] GetProviders();

        [OperationContract]
        [WebGet(UriTemplate = "/{projectName}/{applicationName}/postdbdictionary")]
        Response PostDictionaryToAdapterService(string projectName, string applicationName);

        [OperationContract]
        [WebGet(UriTemplate = "/{projectName}/{applicationName}/clear")]
        Response ClearTripleStore(string projectName, string applicationName);

        [OperationContract]
        [WebGet(UriTemplate = "/{projectName}/{applicationName}/delete")]
        Response DeleteApp(string ProjectName, string applicationName);
    }  
}
