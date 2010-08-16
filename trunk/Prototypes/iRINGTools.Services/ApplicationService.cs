using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web.Configuration;
using log4net;
using org.iringtools.application;
using org.iringtools.library;

namespace org.iringtools.services
{
  [ServiceContract]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class ApplicationService
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ApplicationService));
    private ApplicationProvider _applicationProvider = null;
    
    public ApplicationService()
    {
      _applicationProvider = new ApplicationProvider(WebConfigurationManager.AppSettings);
    }

    [WebGet(UriTemplate = "/{project}/{application}/dictionary")]
    public DatabaseDictionary GetDictionary(string project, string application)
    {
      return _applicationProvider.GetDictionary(project, application);
    }

    [WebGet(UriTemplate = "/{project}/{application}/schema")]
    public DatabaseDictionary GetDatabaseSchema(string project, string application)
    {
      return _applicationProvider.GetDatabaseSchema(project, application);
    }

    [WebInvoke(Method = "POST", UriTemplate = "/{project}/{application}/dictionary")]
    public Response PostDictionary(string project, string application, DatabaseDictionary dict)
    {
      return _applicationProvider.PostDictionary(project, application, dict);
    }

    [WebGet(UriTemplate = "/{project}/{application}/generate")]
    public Response Generate(string project, string application)
    {
      return _applicationProvider.Generate(project, application);
    }

    [WebGet(UriTemplate = "/providers")]
    public List<String> GetProviders()
    {
      return _applicationProvider.GetProviders();
    }

    [WebGet(UriTemplate = "/relationship")]
    public List<String> GetRelationships()
    {
      return _applicationProvider.GetRelationships();
    }

    [WebGet(UriTemplate = "/{project}/{application}/schemaObjects")]
    public List<String> GetSchemaObjects(string project, string application)
    {
      return _applicationProvider.GetSchemaObjects(project, application);
    }

    [WebGet(UriTemplate = "/{project}/{application}/schemaObjects/{schemaObjectName}")]
    public DataObject GetSchemaObjectSchema(string project, string application, string schemaObjectName)
    {
      return _applicationProvider.GetSchemaObjectSchema(project, application, schemaObjectName);
    }
        















    }
}
