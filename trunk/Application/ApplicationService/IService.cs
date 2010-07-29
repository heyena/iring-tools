using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using org.iringtools.library;
using System;

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
        List<String> GetProviders();

        [OperationContract]
        [WebGet(UriTemplate = "/relationship")]
        List<String> GetRelationships();

        [OperationContract]
        [WebGet(UriTemplate = "/{project}/{application}/schemaObjects")]
        List<String> GetSchemaObjects(string project, string application);

        [OperationContract]
        [WebGet(UriTemplate = "/{project}/{application}/schemaObjects/{schemaObjectName}")]
        DatabaseDictionary GetSchemaObjectSchema(string project, string application, string schemaObjectName);
    }
}
