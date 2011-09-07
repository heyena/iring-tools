using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DynamicGridDemo.Models;
using org.iringtools.library;

namespace DynamicGridDemo.Controllers
{
  [HandleError]
  public class DynamicGridController : Controller
  {
    private DynamicGridModel model;

    public DynamicGridController()
    {
      model = new DynamicGridModel();
    }

    public ActionResult Index()
    {
      return View();
    }

    public ActionResult GridDefinition(String scope, String app, String dataObject)
    {
      GridDefinition gridDef = model.GetGridDefinition(scope, app, dataObject);
      return Json(gridDef, JsonRequestBehavior.AllowGet);
    }

    public ActionResult GridData(String scope, String app, String dataObject, int start, int limit, 
      String sort, String dir, String filter)
    {
      GridData gridData = model.GetGridData(scope, app, dataObject, start, limit, sort, dir, filter);
      return Json(gridData, JsonRequestBehavior.AllowGet);
    }
  }
}
