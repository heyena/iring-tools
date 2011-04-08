using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web.Configuration;
using log4net;
using org.iringtools.nhibernate;
using org.iringtools.library;
using System.ComponentModel;

namespace org.iringtools.services
{
  [ServiceContract]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class HibernateService
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(HibernateService));
    private NHibernateProvider _NHibernateProvider = null;

    public HibernateService()
    {
      _NHibernateProvider = new NHibernateProvider(WebConfigurationManager.AppSettings);
    }

    #region GetVersion
    /// <summary>
    /// Gets the version of the service.
    /// </summary>
    /// <returns>Returns the version as a string.</returns>
    [Description("Gets the version of the service.")]
    [WebGet(UriTemplate = "/version")]
    public VersionInfo GetVersion()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
	  
	  VersionInfo version = new  VersionInfo();
      
      Type type = typeof(NHibernateProvider);
      version.Major = type.Assembly.GetName().Version.Major;
      version.Minor = type.Assembly.GetName().Version.Minor;
      version.Build = type.Assembly.GetName().Version.Build;
      version.Revision = type.Assembly.GetName().Version.Revision;

      return version;
    }
    #endregion

    [WebGet(UriTemplate = "/{project}/{application}/dictionary")]
    public DatabaseDictionary GetDictionary(string project, string application)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetDictionary(project, application);
    }

    [WebInvoke(Method = "POST", UriTemplate = "/{project}/{application}/dictionary")]
    public Response PostDictionary(string project, string application, DatabaseDictionary dictionary)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.PostDictionary(project, application, dictionary);
    }

    [WebGet(UriTemplate = "/{project}/{application}/generate")]
    public Response Generate(string project, string application)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.Generate(project, application);
    }

    [WebGet(UriTemplate = "/providers")]
    public DataProviders GetProviders()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetProviders();
    }

    [WebGet(UriTemplate = "/relationships")]
    public DataRelationships GetRelationships()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetRelationships();
    }

    [WebGet(UriTemplate = "/{project}/{application}/objects")]
    public DataObjects GetSchemaObjects(string project, string application)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetSchemaObjects(project, application);
    }

    //TODO: create request object and do post
    [WebGet(UriTemplate = "/{project}/{application}/{dbProvider}/{dbServer}/{dbInstance}/{dbName}/{dbSchema}/{dbUserName}/{dbPassword}/objects")]
    public DataObjects GetSchemaObjects2(string project, string application, string dbProvider, string dbServer,
      string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      
      return _NHibernateProvider.GetSchemaObjects(project, application, dbProvider, dbServer, dbInstance, 
        dbName, dbSchema, dbUserName, dbPassword);
    }

    [WebGet(UriTemplate = "/{project}/{application}/objects/{objectName}")]
    public DataObject GetSchemaObjectSchema(string project, string application, string objectName)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetSchemaObjectSchema(project, application, objectName);
    }
















  }
}
