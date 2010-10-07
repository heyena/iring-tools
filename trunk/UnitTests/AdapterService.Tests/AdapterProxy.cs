﻿using System;
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
using org.iringtools.exchange;

namespace AdapterService.Tests
{
  [TestClass]
  public class AdapterProxy
  {
    private AdapterProvider _adapterProvider = null;
    private ExchangeProvider _exchangeProvider = null;
    private DtoProvider _dtoProvider = null;

    [Dependency]
    public IError Error { get; set; }

    [Dependency]
    public ILoggerFacade Logger { get; set; }

    public AdapterProxy()
    {
      _dtoProvider = new DtoProvider(ConfigurationManager.AppSettings);
      _adapterProvider = new AdapterProvider(ConfigurationManager.AppSettings);
      _exchangeProvider = new ExchangeProvider(ConfigurationManager.AppSettings);
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

    public XDocument GetXml(string projectName, string applicationName, string graphName, string format)
    {
      XDocument xDocument = null;
      try
      {
        xDocument = _adapterProvider.GetProjection(projectName, applicationName, graphName, format);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return xDocument;
    }

    public IList<IDataObject> GetDataObject(string projectName, string applicationName, string graphName, string format, XDocument xml)
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

    public Response Pull(string projectName, string applicationName, string graphName, Request request)
    {
      Response response = null;
      try
      {
        response = _exchangeProvider.Pull(projectName, applicationName, graphName, request);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return response;
    }

    public Response PullDTO(string projectName, string applicationName, string graphName, Request request)
    {
      Response response = null;
      try
      {
        response = _exchangeProvider.PullDTO(projectName, applicationName, graphName, request);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return response;
    }

    public Response Push(string projectName, string applicationName, string graphName, PushRequest request)
    {
      Response response = null;
      try
      {
        response = _exchangeProvider.Push(projectName, applicationName, graphName, request);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return response;
    }

    public Response Post(string projectName, string applicationName, string graphName, string format, XDocument xDocument)
    {
      Response response = null;
      try
      {
        response = _adapterProvider.Post(projectName, applicationName, graphName, format, xDocument);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return response;
    }

    public Response PostDTO(string projectName, string applicationName, string graphName, DataTransferObjects dto)
    {
      Response response = null;
      try
      {
        response = _dtoProvider.PostDataTransferObjects(projectName, applicationName, graphName, dto);
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
  }
}
