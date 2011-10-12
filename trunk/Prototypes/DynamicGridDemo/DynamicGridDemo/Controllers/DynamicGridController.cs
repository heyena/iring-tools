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

    public ActionResult GridDefinition(String dataObject)
    {
      GridDefinition gridDef = model.GetGridDefinition(dataObject);
      return Json(gridDef, JsonRequestBehavior.AllowGet);
    }

    public ActionResult GridData(String dataObject, int start, int limit, 
      String sort, String dir, String filter)
    {
      GridData gridData = model.GetGridData(dataObject, start, limit, sort, dir, filter);
      return Json(gridData, JsonRequestBehavior.AllowGet);
    }
  }
}
