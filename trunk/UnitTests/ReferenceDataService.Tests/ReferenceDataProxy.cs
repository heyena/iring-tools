using System;
using System.Collections.Generic;
using org.ids_adi.qmxf;
using System.ComponentModel;
using System.Net;
using org.iringtools.utility;
using PrismContrib.Errors;
using PrismContrib.Loggers;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;
using org.iringtools.library;
using System.Configuration;
using System.Collections.Specialized;
using org.iringtools.refdata;

namespace ReferenceDataService.Tests
{
  class ReferenceDataProxy // : IService
  {
    private ReferenceDataProvider _referenceDataServiceProvider = null;

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
      NameValueCollection settings = ConfigurationManager.AppSettings;
      _referenceDataServiceProvider = new ReferenceDataProvider(settings);
    }

    public Repositories GetRepositories()
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

    public RefDataEntities SearchPage(string query, int start, int limit)
    {
      return _referenceDataServiceProvider.SearchPage(query, start, limit);
    }

    public RefDataEntities SearchPageReset(string query, int start, int limit)
    {
      return _referenceDataServiceProvider.SearchPageReset(query, start, limit);
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

    #region Part8

    public QMXF GetPart8Template(string id)
    {
        return _referenceDataServiceProvider.GetPart8Template(id);
    }

    public List<Classification> GetPart8TemplateClassif(string id)
    {
        return _referenceDataServiceProvider.GetPart8TemplateClassif(id);
    }

    public List<Specialization> GetPart8TemplateSpec(string id)
    {
        return _referenceDataServiceProvider.GetPart8TemplateSpec(id);
    }

    public Response PostPart8Template(QMXF qmxf)
    {
        return _referenceDataServiceProvider.PostPart8Template(qmxf);
    }

    public QMXF GetPart8Class(string id)
    {
        return _referenceDataServiceProvider.GetPart8Class(id);
    }

    public RefDataEntities Part8Search(string query)
    {
        return _referenceDataServiceProvider.Part8Search(query);
    }

    public Response PostPart8Class(QMXF qmxf)
    {
        return _referenceDataServiceProvider.PostPart8Class(qmxf);
    }

    #endregion Part8
  }
}
