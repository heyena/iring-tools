﻿using System;
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

    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{application}/dictionary")]
    public Response PostDictionary(string scope, string application, DatabaseDictionary dictionary)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.PostDictionary(scope, application, dictionary);
    }

    [WebGet(UriTemplate = "/{scope}/{application}/generate")]
    public Response Generate(string scope, string application)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.Generate(scope, application);
    }

    [WebGet(UriTemplate = "/relationships")]
    public DataRelationships GetRelationships()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetRelationships();
    }

    [WebGet(UriTemplate = "/{scope}/{application}/objects")]
    public DataObjects GetSchemaObjects(string scope, string application)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetSchemaObjects(scope, application);
    }

    [WebGet(UriTemplate = "/{scope}/{application}/objects/{objectName}")]
    public DataObject GetSchemaObjectSchema(string scope, string application, string objectName)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetSchemaObjectSchema(scope, application, objectName);
    }
    
    #region NHibernate Config Wizard support URIs
    [WebGet(UriTemplate = "/providers")]
    public DataProviders GetProviders()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetProviders();
    }

    [WebGet(UriTemplate = "/{scope}/{application}/dictionary")]
    public DatabaseDictionary GetDictionary(string scope, string application)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetDictionary(scope, application);
    }

    ///TODO: create request object and do post or encrypt password
    [WebGet(UriTemplate = "/{scope}/{application}/{dbProvider}/{dbServer}/{portNumber}/{dbInstance}/{dbName}/{dbSchema}/{dbUserName}/{dbPassword}/tables")]
		public List<string> GetTableNames(string scope, string application, string dbProvider, string dbServer, string portNumber,
      string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

			return _NHibernateProvider.GetTableNames(scope, application, dbProvider, dbServer, portNumber, dbInstance,
        dbName, dbSchema, dbUserName, dbPassword);
    }

    [WebGet(UriTemplate = "/{scope}/{application}/{dbProvider}/{dbServer}/{portNumber}/{dbInstance}/{dbName}/{dbSchema}/{dbUserName}/{dbPassword}/{tableNames}/objects")]
		public List<DataObject> GetDBObjects(string scope, string application, string dbProvider, string dbServer, string portNumber,
      string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string tableNames)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

			return _NHibernateProvider.GetDBObjects(scope, application, dbProvider, dbServer, portNumber, dbInstance,
        dbName, dbSchema, dbUserName, dbPassword, tableNames);
    }

    #endregion
  }
}
