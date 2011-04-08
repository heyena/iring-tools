using System.Collections.Generic;
using System.Web.Mvc;
using iRINGTools.Web.Models;
using org.iringtools.library;
using iRINGTools.Web.Helpers;

namespace iRINGTools.Web.Controllers
{
  public class AdapterManagerController : Controller
  {
    private IAdapterRepository _repository;

    public AdapterManagerController() : this(new AdapterRepository()) {}

    public AdapterManagerController(IAdapterRepository repository)
    {
      _repository = repository;
    }

    public ActionResult Index()
    {
      return View();
    }

    public ActionResult DataProviders()
    {
      DataProviders dataProviders = _repository.GetDataProviders();

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

    public ActionResult DatabaseDictionary(FormCollection form)
    {
      DatabaseDictionary dbDict = _repository.GetDatabaseDictionary(form["scope"], form["app"]);
      return Json(dbDict, JsonRequestBehavior.AllowGet);
    }

    public ActionResult SchemaObjects(FormCollection form)
    {
      DataObjects dataObjects = _repository.GetSchemaObjects(
        form["scope"], form["app"], form["dbProvider"], form["dbServer"], form["dbInstance"], 
        form["dbName"], form["dbSchema"], form["dbUserName"], form["dbPassword"]);

      JsonContainer<DataObjects> container = new JsonContainer<DataObjects>();
      container.items = dataObjects;
      container.success = true;
      container.total = dataObjects.Count;

      return Json(container, JsonRequestBehavior.AllowGet);
    }
  }

  public class DBProvider
  {
    public string Provider { get; set; }
  }
}
