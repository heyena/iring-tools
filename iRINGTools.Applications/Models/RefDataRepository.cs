using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;

using org.iringtools.library;
using org.iringtools.refdata.federation;
using org.iringtools.refdata.response;
using org.iringtools.utility;
using org.iringtools.mapping;
using org.ids_adi.qmxf;
using Ninject;
using org.iringtools.refdata;
using org.iringtools.web.Models;
using org.iringtools.refdata.response;

namespace iRINGTools.Web.Models
{
  public class RefDataRepository : IRefDataRepository
  {
    private NameValueCollection _settings = null;
    private WebHttpClient _referenceDataServiceClient = null;
    private string relativeUri = string.Empty;

    [Inject]
    public RefDataRepository()
    {
      _settings = ConfigurationManager.AppSettings;
      _referenceDataServiceClient = new WebHttpClient(_settings["ReferenceDataServiceUri"]);
    }

    public RefdataResponse Search(string query, int start, int limit)
    {
      relativeUri = string.Format("/search/{0}/{1}/{2}", query, start, limit);
      return _referenceDataServiceClient.Get<RefdataResponse>(relativeUri);
    }

    public RefdataResponse Search(string query)
    {
      relativeUri = string.Format("/search/{0}/0/0", query);
      return _referenceDataServiceClient.Get<RefdataResponse>(relativeUri);
    }

    public List<Namespace> GetNamespaces()
    {
      return null;
    }

    public RefdataResponse SearchReset(string query)
    {
      relativeUri = string.Format("/search/{0}/reset", query);
      return  _referenceDataServiceClient.Get<RefdataResponse>(relativeUri);
       
    }

    public Entity GetClassLabel(string classId)
    {
      relativeUri = string.Format("/classes/{0}/label", classId);
      return _referenceDataServiceClient.Get<Entity>(relativeUri);
    }

    public org.iringtools.refdata.response.Entities GetSubClasses(string classId)
    {
      relativeUri = string.Format("/classes/{0}/subclasses", classId);
      return _referenceDataServiceClient.Get<Entities>(relativeUri);
    }

    public org.iringtools.refdata.response.Entities GetSubClasses(string classId, Repository repository)
    {
      relativeUri = string.Format("/classes/{0}/subclasses", classId);
      if (repository != null)
        return _referenceDataServiceClient.Post<Repository, Entities>(relativeUri, repository);
      else
        return _referenceDataServiceClient.Get<Entities>(relativeUri);
    }

    public org.iringtools.refdata.response.Entities GetSubClassesCount(string classId)
    {
        relativeUri = string.Format("/classes/{0}/subclasses/count", classId);
        return _referenceDataServiceClient.Get<Entities>(relativeUri);
    }  

    public org.iringtools.refdata.response.Entities GetSuperClasses(string classId)
    {
      relativeUri = string.Format("/classes/{0}/superclasses", classId);
      return _referenceDataServiceClient.Get<Entities>(relativeUri);
    }

    public org.iringtools.refdata.response.Entities GetSuperClasses(string classId, Repository repository)
    {
      relativeUri = string.Format("/classes/{0}/superclasses", classId);
      if (repository == null)
        return _referenceDataServiceClient.Get<Entities>(relativeUri);
      else
        return _referenceDataServiceClient.Post<Repository, Entities>(relativeUri, repository);
    }

    public org.iringtools.refdata.response.Entities GetClassTemplates(string classId)
    {
      relativeUri = string.Format("/classes/{0}/templates", classId);
      return _referenceDataServiceClient.Get<Entities>(relativeUri);
    }

    public org.iringtools.refdata.response.Entities GetClassTemplatesCount(string classId)
    {
        relativeUri = string.Format("/classes/{0}/templates/count", classId);
        return _referenceDataServiceClient.Get<Entities>(relativeUri);
    }

    public QMXF GetClasses(string classId)
    {
      relativeUri = string.Format("/classes/{0}", classId);
      return _referenceDataServiceClient.Get<QMXF>(relativeUri);
    }

    public QMXF GetClasses(string classId, Repository repository)
    {

        if (repository != null)
        {
            relativeUri = string.Format("/classes/{0}/{1}", classId, repository.Name);
            return _referenceDataServiceClient.Get<QMXF>(relativeUri);
        }
        else
        {
            relativeUri = string.Format("/classes/{0}", classId);
            return _referenceDataServiceClient.Get<QMXF>(relativeUri);
        }
    }

    public QMXF GetTemplate(string id)
    {
      relativeUri = string.Format("/templates/{0}", id);
      return _referenceDataServiceClient.Get<QMXF>(relativeUri);
    }

    public Federation GetFederation()
    {
      relativeUri = "/federation";
      return _referenceDataServiceClient.Get<Federation>(relativeUri);
    }

    public RefdataResponse GetClassMembers(string classId)
    {
      relativeUri = string.Format("/classes/{0}/members", classId);
      return _referenceDataServiceClient.Get<RefdataResponse>(relativeUri);
    }

    public RefdataResponse GetClassMembers(string classId, Repository repository)
    {

        if (repository != null)
        {
            relativeUri = string.Format("/classes/{0}/members/{1}", classId, repository.Name);
            return _referenceDataServiceClient.Get<RefdataResponse>(relativeUri);
        }
        else
        {
            relativeUri = string.Format("/classes/{0}/members", classId);
            return _referenceDataServiceClient.Get<RefdataResponse>(relativeUri);
        }
    }

  }
}