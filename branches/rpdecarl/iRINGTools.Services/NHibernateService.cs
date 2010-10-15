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
  public class NHibernateService
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateService));
    private NHibernateProvider _NHibernateProvider = null;
    
    public NHibernateService()
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
    public string GetVersion()
    {
      return new Version _NHibernateProvider.GetType().Assembly.GetName().Version.ToString();
    }
    #endregion

    [WebGet(UriTemplate = "/{project}/{application}/dictionary")]
    public DatabaseDictionary GetDictionary(string project, string application)
    {
      return _NHibernateProvider.GetDictionary(project, application);
    }

    [WebInvoke(Method = "POST", UriTemplate = "/{project}/{application}/dictionary")]
    public Response PostDictionary(string project, string application, DatabaseDictionary dictionary)
    {
      return _NHibernateProvider.PostDictionary(project, application, dictionary);
    }

    [WebGet(UriTemplate = "/{project}/{application}/generate")]
    public Response Generate(string project, string application)
    {
      return _NHibernateProvider.Generate(project, application);
    }

    [WebGet(UriTemplate = "/providers")]
    public List<String> GetProviders()
    {
      return _NHibernateProvider.GetProviders();
    }

    [WebGet(UriTemplate = "/relationship")]
    public List<String> GetRelationships()
    {
      return _NHibernateProvider.GetRelationships();
    }

    [WebGet(UriTemplate = "/{project}/{application}/schemaObjects")]
    public List<String> GetSchemaObjects(string project, string application)
    {
      return _NHibernateProvider.GetSchemaObjects(project, application);
    }

    [WebGet(UriTemplate = "/{project}/{application}/schemaObjects/{schemaObjectName}")]
    public DataObject GetSchemaObjectSchema(string project, string application, string schemaObjectName)
    {
      return _NHibernateProvider.GetSchemaObjectSchema(project, application, schemaObjectName);
    }
        















    }
}
