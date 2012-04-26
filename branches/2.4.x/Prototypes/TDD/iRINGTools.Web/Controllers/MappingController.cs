using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using iRINGTools.Data;
using iRINGTools.Services;

namespace iRINGTools.Web.Controllers
{
  public class MappingController : Controller
  {
    #region constructors
    private IAdapterService _adapterService;

    public MappingController(IAdapterService adapterService)
    {
      _adapterService = adapterService;
    }

    #endregion

    /// <summary>
    /// Main method for the Mapping
    /// </summary>
    public ActionResult Index(int? id)
    {
      int configurationId = id ?? 0;
      Mapping configuration = _adapterService.GetMapping(configurationId) ?? 
        _adapterService.GetDefaultApplication().Mapping;

      //log the event
      //_personalizationService.SaveCategoryView(this.GetUserName(), Request.UserHostAddress, category);

      return View(configuration);
    }

    public ActionResult Show(int id)
    {
      Mapping result = _adapterService.GetMapping(id);

      if (result == null)
      {
        return RedirectToAction("Index");
      }
      else
      {
        ////log the event
        //_personalizationService.SaveProductView(this.GetUserName(), Request.UserHostAddress, product);
        return View("Show", result);
      }

    }

    [Authorize(Roles = "Administrator")]
    public ActionResult Edit(int id)
    {
      Mapping result = _adapterService.GetMapping(id);
      return View(result);
    }

  }
}
