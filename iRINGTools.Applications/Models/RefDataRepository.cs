﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;
using org.ids_adi.qmxf;
using System.Net;
using org.iringtools.refdata.federation;

namespace iRINGTools.Web.Models
{
  public class RefDataRepository : IRefDataRepository
  {
    private WebHttpClient _referenceDataServiceClient = null;
    private string _refDataServiceURI = string.Empty;
    private string relativeUri = string.Empty;

    public RefDataRepository()
    {
      NameValueCollection settings = ConfigurationManager.AppSettings;
      ServiceSettings _settings = new ServiceSettings();
      _settings.AppendSettings(settings);

      #region initialize webHttpClient for converting old mapping
      string proxyHost = _settings["ProxyHost"];
      string proxyPort = _settings["ProxyPort"];
      string referenceDataServiceUri = _settings["RefDataServiceUri"];

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        WebProxy webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
        _referenceDataServiceClient = new WebHttpClient(referenceDataServiceUri, null, webProxy);
      }
      else
      {
        _referenceDataServiceClient = new WebHttpClient(referenceDataServiceUri);

      }
      #endregion
    }

    public RefDataEntities Search(string query, int start, int limit)
    {
      relativeUri = string.Format("/search/{0}/{1}/{2}", query, start, limit);
      return _referenceDataServiceClient.Get<RefDataEntities>(relativeUri);
    }

    public List<Namespace> GetNamespaces()
    {
      return null;
    }

    public RefDataEntities Search(string query)
    {
      relativeUri = string.Format("/search/{0}/0/0", query);
      return _referenceDataServiceClient.Get<RefDataEntities>(relativeUri);
    }

    public RefDataEntities SearchReset(string query)
    {
      relativeUri = string.Format("/search/{0}/reset", query);
      return  _referenceDataServiceClient.Get<RefDataEntities>(relativeUri);
       
    }

    public Entity GetClassLabel(string classId)
    {
      relativeUri = string.Format("/classes/{0}/label", classId);
      return _referenceDataServiceClient.Get<Entity>(relativeUri);
    }

    public Entities GetSubClasses(string classId, Repository repository)
    {
      relativeUri = string.Format("/classes/{0}/subclasses", classId);
      if (repository != null)
        return _referenceDataServiceClient.Post<Repository, Entities>(relativeUri, repository);
      else
        return _referenceDataServiceClient.Get<Entities>(relativeUri);
    }

    public Entities GetSubClassesCount(string classId)
    {
        relativeUri = string.Format("/classes/{0}/subclasses/count", classId);
        return _referenceDataServiceClient.Get<Entities>(relativeUri);
    }

    public Federation GetFederation()
    {
      relativeUri = "/federation";
      return _referenceDataServiceClient.Get<Federation>(relativeUri);
    }

    public Entities GetSuperClasses(string classId, Repository repository)
    {
      relativeUri = string.Format("/classes/{0}/superclasses", classId);
      if(repository == null)
        return _referenceDataServiceClient.Get<Entities>(relativeUri);
      else
        return _referenceDataServiceClient.Post<Repository, Entities>(relativeUri, repository);
    }

    public Entities GetClassTemplates(string classId)
    {
      relativeUri = string.Format("/classes/{0}/templates", classId);
      return _referenceDataServiceClient.Get<Entities>(relativeUri);
    }

    public Entities GetClassTemplatesCount(string classId)
    {
        relativeUri = string.Format("/classes/{0}/templates/count", classId);
        return _referenceDataServiceClient.Get<Entities>(relativeUri);
    }

    //public QMXF GetClasses(string classId)
    //{
    //  relativeUri = string.Format("/classes/{0}", classId);
    //  return _referenceDataServiceClient.Get<QMXF>(relativeUri);
    //}

    public QMXF GetClasses(string classId, Repository repository)
    {
      relativeUri = string.Format("/classes/{0}", classId);
      if(repository != null)
        return _referenceDataServiceClient.Post<Repository, QMXF>(relativeUri, repository);
      else
        return _referenceDataServiceClient.Get<QMXF>(relativeUri);
    }

    public QMXF GetTemplate(string id)
    {
      relativeUri = string.Format("/templates/{0}", id);
      return _referenceDataServiceClient.Get<QMXF>(relativeUri);
    }                                                                  

    public Entities GetClassMembers(string classId, Repository repository)
    {
      relativeUri = string.Format("/classes/{0}/members", classId);
      if(repository != null)
        return _referenceDataServiceClient.Post<Repository, Entities>(relativeUri, repository);
      else
        return _referenceDataServiceClient.Get<Entities>(relativeUri);
    }

  }
}