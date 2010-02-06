using System;
using System.Collections.Generic;
using org.ids_adi.iring;
using org.ids_adi.iring.referenceData;
using org.ids_adi.qmxf;
using System.ComponentModel;
using System.Net;
using org.iringtools.utility;
using PrismContrib.Errors;
using PrismContrib.Loggers;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;
using org.iringtools.library;

namespace ReferenceDataService.Tests
{
  class ReferenceDataProxy:IReferenceDataService
  {
    private ReferenceDataServiceProvider _referenceDataServiceProvider = null;

    /// <summary>
    /// Gets or sets the error.
    /// </summary>
    /// <value>The error.</value>
    [Dependency]
    public IError Error { get; set; }

    [Dependency]
    public ILoggerFacade Logger { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdapterProxyProvider"/> class.
    /// </summary>
    /// <param name="container">The container.</param>
    public ReferenceDataProxy()
    {
      ConfigSettings configSettings = new ConfigSettings();
      configSettings.BaseDirectoryPath = System.Configuration.ConfigurationManager.AppSettings["BasePath"];
      configSettings.SPARQLPath = System.Configuration.ConfigurationManager.AppSettings["SPARQLPath"];
      configSettings.XMLPath = System.Configuration.ConfigurationManager.AppSettings["XMLPath"];
      configSettings.PageSize = System.Configuration.ConfigurationManager.AppSettings["PageSize"];
      configSettings.ClassRegistryBase = System.Configuration.ConfigurationManager.AppSettings["ClassRegistryBase"];
      configSettings.TemplateRegistryBase = System.Configuration.ConfigurationManager.AppSettings["TemplateRegistryBase"];
      configSettings.ExampleRegistryBase = System.Configuration.ConfigurationManager.AppSettings["ExampleRegistryBase"];
      configSettings.RegistryCredentialToken = System.Configuration.ConfigurationManager.AppSettings["RegistryCredentialToken"];
      configSettings.ProxyCredentialToken = System.Configuration.ConfigurationManager.AppSettings["ProxyCredentialToken"];
      configSettings.ProxyHost = System.Configuration.ConfigurationManager.AppSettings["ProxyHost"];
      configSettings.ProxyPort = System.Configuration.ConfigurationManager.AppSettings["ProxyPort"];
      _referenceDataServiceProvider = new ReferenceDataServiceProvider(configSettings);
    }

    public List<Repository> GetRepositories()
    {
      return _referenceDataServiceProvider.GetRepositories();
    }

    public RefDataEntities Search(string query)
    {
      return _referenceDataServiceProvider.Search(query);
    }

    public RefDataEntities SearchReset(string query)
    {
      return _referenceDataServiceProvider.SearchReset(query);
    }

    public RefDataEntities SearchPage(string query, string page)
    {
      return _referenceDataServiceProvider.SearchPage(query, page);
    }

    public RefDataEntities SearchPageReset(string query, string page)
    {
      return _referenceDataServiceProvider.SearchPageReset(query, page);
    }

    public List<Entity> Find(string query)
    {
      return _referenceDataServiceProvider.Find(query);
    }

    public QMXF GetClass(string id)
    {
      return _referenceDataServiceProvider.GetClass(id);
    }

    public string GetClassLabel(string id)
    {
      return _referenceDataServiceProvider.GetClassLabel(id);
    }

    public List<Entity> GetSuperClasses(string id)
    {
      return _referenceDataServiceProvider.GetSuperClasses(id);
    }

    public List<Entity> GetAllSuperClasses(string id)
    {
        return _referenceDataServiceProvider.GetAllSuperClasses(id);
    }


    public List<Entity> GetSubClasses(string id)
    {
      return _referenceDataServiceProvider.GetSubClasses(id);
    }

    public List<Entity> GetClassTemplates(string id)
    {
      return _referenceDataServiceProvider.GetClassTemplates(id);
    }

    public QMXF GetTemplate(string id)
    {
      return _referenceDataServiceProvider.GetTemplate(id);
    }

    public Response PostTemplate(QMXF template)
    {
      throw new NotImplementedException();
    }

    public Response PostClass(QMXF @class)
    {
      throw new NotImplementedException();
    }
  }
}
