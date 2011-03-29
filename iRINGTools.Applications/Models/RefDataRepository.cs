using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;

namespace iRINGTools.Web.Models
{
  public class RefDataRepository : IRefDataRepository
  {
    private NameValueCollection _settings = null;
    private WebHttpClient _client = null;
    private string _refDataServiceURI = string.Empty;
    private string relativeUri = string.Empty;

    public RefDataRepository()
    {
      _settings = ConfigurationManager.AppSettings;
      _client = new WebHttpClient(_settings["ReferenceDataServiceUri"]);
    }

    public RefDataEntities Search(string query, string start, string limit)
    {
      relativeUri = string.Format("/search/{0}/{1}/{2}", query, start, limit);
      return _client.Get<RefDataEntities>(relativeUri);
    }

    public RefDataEntities Search(string query)
    {
      relativeUri = string.Format("/search/{0}/0/0", query);
      return _client.Get<RefDataEntities>(relativeUri);
    }

    public RefDataEntities SearchReset(string query)
    {
      relativeUri = string.Format("search/{0}/reset", query);
      return  _client.Get<RefDataEntities>(relativeUri);
       
    }

    public Entity GetClassLabel(string classId)
    {
      relativeUri = string.Format("/classes/{0}/label", classId);
      return _client.Get<Entity>(relativeUri);
    }

    public Entities GetSubClasses(string classId)
    {
      relativeUri = string.Format("/classes/{0}/subclasses", classId);
      return _client.Get<Entities>(relativeUri);
    }

    public Entities GetSuperClasses(string classId)
    {
      relativeUri = string.Format("/classes/{0}/superclasses", classId);
      return _client.Get<Entities>(relativeUri);
    }

    public Entities GetClassTemplates(string classId)
    {
      relativeUri = string.Format("/classes/{0}/templates", classId);
      return _client.Get<Entities>(relativeUri);
    }
  }
}