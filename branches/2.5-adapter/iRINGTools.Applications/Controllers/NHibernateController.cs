using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using org.iringtools.web.controllers;
using org.iringtools.adapter.security;
using System.Collections;
using log4net;
using iRINGTools.Web.Helpers;
using org.iringtools.library;
using org.iringtools.web.Models;

namespace org.iringtools.web.Controllers
{
  public class NHibernateController : BaseController
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseController));
    private NHibernateRepository _repository;
    private string _keyFormat = "Datadictionary.{0}.{1}";

    public NHibernateController() : this(new NHibernateRepository()) { }

    public NHibernateController(NHibernateRepository repository)
        : base()
    {
      _repository = repository;
    }



    private DatabaseDictionary GetDbDictionary(string contextName, string endpoint)
    {
      var key = adapter_PREFIX + string.Format(_keyFormat, contextName, endpoint);
      DatabaseDictionary databaseDictionary = null;
      var getDbDict = false;

      if (Session[key] != null)
      {
        databaseDictionary = (DatabaseDictionary)Session[key];
        if (databaseDictionary.ConnectionString == null || databaseDictionary.dataObjects.Count == 0)
          getDbDict = true;          
      }
      else
        getDbDict = true;       
     
      if (getDbDict)
      {
        databaseDictionary = _repository.GetDbDictionary(contextName, endpoint);
      }

      Session[key] = databaseDictionary;

      return (DatabaseDictionary)Session[key];
    }

    public ActionResult DBObjects(FormCollection form)
    {
      try
      {
        var dbObjects = _repository.GetDbObjects(form["scope"], form["app"], form["dbProvider"], form["dbServer"], form["dbInstance"],
          form["dbName"], form["dbSchema"], form["dbUserName"], form["dbPassword"], form["tableNames"], form["portNumber"], form["serName"]);

        return Json(dbObjects, JsonRequestBehavior.AllowGet);
      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }
    }

    public ActionResult Trees(FormCollection form)
    {
      try
      {
        var response = string.Empty;

        response = _repository.SaveDbDictionary(form["scope"], form["app"], form["tree"]);

        if (response != null)
        {
          response = response.ToLower();
          if (response.Contains("error"))
          {
            var inds = response.IndexOf("<message>", System.StringComparison.Ordinal);
            var inde = response.IndexOf("</message>", System.StringComparison.Ordinal);
            var msg = response.Substring(inds + 9, inde - inds - 9);
            return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
          }
        }
        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }
    }

    public ActionResult DataType()
    {
      try
      {
        var dataTypeNames = Enum.GetValues(typeof (DataType)).Cast<DataType>().ToDictionary(dataType => ((int) dataType).ToString(), dataType => dataType.ToString());

        return Json(dataTypeNames, JsonRequestBehavior.AllowGet);
      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }
    }     

    public ActionResult DBProviders()
    {
      var container = new JsonContainer<List<DBProvider>>();

      try
      {
        var dataProviders = _repository.GetDbProviders();

        var providers = dataProviders.Select(dataProvider => new DBProvider() {Provider = dataProvider.ToString()}).ToList();

        container.items = providers;
        container.success = true;
        container.total = dataProviders.Count;

      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }
      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public ActionResult DBDictionary(FormCollection form)
    {
      try
      {
        var dbDict = _repository.GetDbDictionary(form["scope"], form["app"]);
        return Json(dbDict, JsonRequestBehavior.AllowGet);
      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }
    }

    public ActionResult TableNames(FormCollection form)
    {
      var container = new JsonContainer<List<string>>();

      try
      {
        var dataObjects = _repository.GetTableNames(
          form["scope"], form["app"], form["dbProvider"], form["dbServer"], form["dbInstance"],
          form["dbName"], form["dbSchema"], form["dbUserName"], form["dbPassword"], form["portNumber"], form["serName"]);

        container.items = dataObjects;
        container.success = true;
        container.total = dataObjects.Count;
      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    private string GetKeytype(string name, IEnumerable<DataProperty> properties)
    {
      var keyType = string.Empty;
      keyType = properties.FirstOrDefault(p => p.propertyName == name).keyType.ToString();
      return keyType;
    }
    private string GetDatatype(string name, IEnumerable<DataProperty> properties)
    {
      var dataType = string.Empty;
      dataType = properties.FirstOrDefault(p => p.propertyName == name).dataType.ToString();
      return dataType;
    }

    private void AddContextEndpointtoNode(JsonTreeNode node, FormCollection form)
    {
      if (form["contextName"] != null)
        node.properties.Add("context", form["contextName"]);
      if (form["endpoint"] != null)
        node.properties.Add("endpoint", form["endpoint"]);
    }
  }

  public class DBProvider
  {
    public string Provider { get; set; }
  }
}
