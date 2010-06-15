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

    public Response UpdateMapping(string projectName, string applicationName, Mapping mapping)
    {
      Response response = null;
      try
      {
        response = _adapterProvider.UpdateMapping(projectName, applicationName, mapping);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }

    //public XElement Get(string projectName, string applicationName, string graphName, string identifier)
    //{
    //  XElement xml = null;
    //  try
    //  {
    //    xml = _adapterProvider.Get(projectName, applicationName, graphName, identifier, null);
    //  }
    //  catch (Exception ex)
    //  {
    //    Error.SetError(ex);
    //  }
    //  return xml;
    //}

    //public XElement GetList(string projectName, string applicationName, string graphName)
    //{
    //  XElement envelope = null;
    //  try
    //  {
    //    envelope = _adapterProvider.GetList(projectName, applicationName, graphName, null);
    //  }
    //  catch (Exception ex)
    //  {
    //    Error.SetError(ex);
    //  }
    //  return envelope;
    //}

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

    public Response Pull(string projectName, string applicationName, string graphName, Request request)
    {
      Response response = null;
      try
      {
        response = _adapterProvider.Pull(projectName, applicationName, graphName, request);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }

    //public Response PullDTO(string projectName, string applicationName, Request request)
    //{
    //  Response response = null;
    //  try
    //  {
    //    response = _adapterProvider.PullDTO(projectName, applicationName, request);
    //  }
    //  catch (Exception ex)
    //  {
    //    Error.SetError(ex);
    //  }
    //  return response;
    //}

    public Response UpdateDatabaseDictionary(string projectName, string applicationName, DatabaseDictionary databaseDictionary)
    {
      Response response = null;
      try
      {
        response = _adapterProvider.UpdateDatabaseDictionary(projectName, applicationName, databaseDictionary);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }

    //public List<DataTransferObject> GetDTOList(string projectName, string applicationName, string graphName)
    //{
    //  List<DataTransferObject> dtoList = null;
    //  try
    //  {
    //    dtoList = _adapterProvider.GetDTOList(projectName, applicationName, graphName);
    //  }
    //  catch (Exception ex)
    //  {
    //    Error.SetError(ex);
    //  }
    //  return dtoList;
    //}

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

    public Manifest GetManifest(string projectName, string applicationName)
    {
      Manifest manifest = null;
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
