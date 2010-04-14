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

namespace AdapterService.Tests
{
  [TestClass]
  public class AdapterProxy
  {
    private AdapterProvider _adapterServiceProvider = null;   

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
      _adapterServiceProvider = new AdapterProvider(ConfigurationManager.AppSettings);
    }

    public Response Generate(string projectName, string applicationName)
    {
      Response response = null;
      try
      {
        response = _adapterServiceProvider.Generate(projectName, applicationName);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }

    public Response RefreshDictionary(string projectName, string applicationName)
    {
      Response response = null;
      try
      {
        response = _adapterServiceProvider.RefreshDictionary(projectName, applicationName);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }

    public Mapping GetMapping(string projectName, string applicationName)
    {
      Mapping mapping = null;

      try
      {
        mapping = _adapterServiceProvider.GetMapping(projectName, applicationName);
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
        data = _adapterServiceProvider.GetDictionary(projectName, applicationName);
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
        response = _adapterServiceProvider.UpdateMapping(projectName, applicationName, mapping);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }

    public Envelope Get(string projectName, string applicationName, string graphName, string identifier)
    {
      Envelope envelope = null;
      try
      {
        envelope = _adapterServiceProvider.Get(projectName, applicationName, graphName, identifier);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return envelope;
    }

    public Envelope GetList(string projectName, string applicationName, string graphName)
    {
      Envelope envelope = null;
      try
      {
        envelope = _adapterServiceProvider.GetList(projectName, applicationName, graphName);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return envelope;
    }

    public Response ClearStore(string projectName, string applicationName)
    {
      Response response = null;
      try
      {
        response = _adapterServiceProvider.ClearStore(projectName, applicationName);
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
        response = _adapterServiceProvider.RefreshGraph(projectName, applicationName, graphName);
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
        response = _adapterServiceProvider.RefreshAll(projectName, applicationName);
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
        response = _adapterServiceProvider.Pull(projectName, applicationName, request);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }

    public Response UpdateDatabaseDictionary(DatabaseDictionary databaseDictionary, string projectName, string applicationName)
    {
      Response response = null;
      try
      {
        response = _adapterServiceProvider.UpdateDatabaseDictionary(databaseDictionary, projectName, applicationName);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }

    public List<DataTransferObject> GetDTOList(string projectName, string applicationName, string graphName)
    {
      List<DataTransferObject> dtoList = null;
      try
      {
        dtoList = _adapterServiceProvider.GetDTOList(projectName, applicationName, graphName);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return dtoList;
    }
  }
}
