using System.Collections.Generic;
using System.Web.Mvc;
using iRINGTools.Web.Models;
using org.iringtools.library;
using iRINGTools.Web.Helpers;
using System;

namespace iRINGTools.Web.Controllers
{
  public class AdapterManagerController : Controller
  {
    private AdapterRepository _repository;

    public AdapterManagerController() : this(new AdapterRepository()) {}

    public AdapterManagerController(AdapterRepository repository)
    {
      _repository = repository;
    }

    public ActionResult Index()
    {
      return View();
    }

    public ActionResult DBProviders()
    {
      DataProviders dataProviders = _repository.GetDBProviders();

      List<DBProvider> providers = new List<DBProvider>();
      foreach (Provider dataProvider in dataProviders)
      {
        providers.Add(new DBProvider() { Provider = dataProvider.ToString() });
      }

      JsonContainer<List<DBProvider>> container = new JsonContainer<List<DBProvider>>();
      container.items = providers;
      container.success = true;
      container.total = dataProviders.Count;

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public ActionResult DBDictionary(FormCollection form)
    {
      DatabaseDictionary dbDict = _repository.GetDBDictionary(form["scope"], form["app"]);
      return Json(dbDict, JsonRequestBehavior.AllowGet);
    }

    public ActionResult TableNames(FormCollection form)
    {
      List<string> dataObjects = _repository.GetTableNames(
        form["scope"], form["app"], form["dbProvider"], form["dbServer"], form["dbInstance"],
        form["dbName"], form["dbSchema"], form["dbUserName"], form["dbPassword"]);

      JsonContainer<List<string>> container = new JsonContainer<List<string>>();
      container.items = dataObjects;
      container.success = true;
      container.total = dataObjects.Count;

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public ActionResult DBObjects(FormCollection form)
    {
      List<JsonTreeNode> dbObjects = _repository.GetDBObjects(
        form["scope"], form["app"], form["dbProvider"], form["dbServer"], form["dbInstance"],
        form["dbName"], form["dbSchema"], form["dbUserName"], form["dbPassword"], form["tableNames"]);

      return Json(dbObjects, JsonRequestBehavior.AllowGet);
    }
  }

  public class DBProvider
  {
    public string Provider { get; set; }
  }
}
