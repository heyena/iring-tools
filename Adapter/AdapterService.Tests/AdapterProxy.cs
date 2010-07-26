using System;
using org.iringtools.adapter;
using org.iringtools.utility;
using org.iringtools.library;
using PrismContrib.Errors;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AdapterService.Tests
{
  [TestClass]
  public class AdapterProxy
  {
    private AdapterProvider _adapterProvider = null;

    [Dependency]
    public IError Error { get; set; }

    [Dependency]
    public ILoggerFacade Logger { get; set; }

    public AdapterProxy()
    {
      _adapterProvider = new AdapterProvider(ConfigurationManager.AppSettings);
    }

    public Mapping GetMapping(string projectName, string applicationName)
    {
      Mapping mapping = null;

      try
      {
        mapping = _adapterProvider.GetMapping(projectName, applicationName);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return mapping;
    }

    public DataDictionary GetDictionary(string projectName, string applicationName)
    {
      DataDictionary data = null;
      try
      {
        data = _adapterProvider.GetDictionary(projectName, applicationName);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return data;
    }

    public Response UpdateMapping(string projectName, string applicationName, XElement mappingXml)
    {
      Response response = null;
      try
      {
        response = _adapterProvider.UpdateMapping(projectName, applicationName, mappingXml);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return response;
    }

    public XElement GetXml(string projectName, string applicationName, string graphName, string format)
    {
      XElement xElement = null;
      try
      {
        xElement = _adapterProvider.GetProjection(projectName, applicationName, graphName, format);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return xElement;
    }

    public IList<IDataObject> GetDataObject(string projectName, string applicationName, string graphName, string format, XElement xml)
    {
      IList<IDataObject> dataObjects = null;
      try
      {
        dataObjects = _adapterProvider.GetDataObjects(projectName, applicationName, graphName, format, xml);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return dataObjects;
    }

    public Response ClearAll(string projectName, string applicationName)
    {
      Response response = null;
      try
      {
        response = _adapterProvider.DeleteAll(projectName, applicationName);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return response;
    }

    public Response RefreshGraph(string projectName, string applicationName, string graphName)
    {
      Response response = null;
      try
      {
        response = _adapterProvider.Refresh(projectName, applicationName, graphName);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return response;
    }

    public Response RefreshAll(string projectName, string applicationName)
    {
      Response response = null;
      try
      {
        response = _adapterProvider.RefreshAll(projectName, applicationName);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return response;
    }

    public Response Pull(string projectName, string applicationName, Request request)
    {
      Response response = null;
      try
      {
        response = _adapterProvider.Pull(projectName, applicationName, request);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return response;
    }

    public Response PullDTO(string projectName, string applicationName, Request request)
    {
      Response response = null;
      try
      {
        response = _adapterProvider.PullDTO(projectName, applicationName, request);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return response;
    }

    public Response Push(string projectName, string applicationName, Request request)
    {
      Response response = null;
      try
      {
        response = _adapterProvider.Push(projectName, applicationName, request);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return response;
    }

    public Response Put(string projectName, string applicationName, string graphName, string format, XElement xml)
    {
      Response response = null;
      try
      {
        response = _adapterProvider.Put(projectName, applicationName, graphName, format, xml);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return response;
    }

    public List<ScopeProject> GetScopes()
    {
      List<ScopeProject> scopes = null;
      try
      {
        scopes = _adapterProvider.GetScopes();
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return scopes;
    }

    public org.iringtools.library.manifest.Manifest GetManifest(string projectName, string applicationName)
    {
      org.iringtools.library.manifest.Manifest manifest = null;
      try
      {
        manifest = _adapterProvider.GetManifest(projectName, applicationName);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return manifest;
    }
  }
}
