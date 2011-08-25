using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using iRINGTools.Data;
using iRINGTools.Services;

namespace iRINGTools.Web.Controllers
{
  public class ApplicationController : Controller
  {
    #region constructors
    IAdapterService _adapterService;

    public ApplicationController(IAdapterService adapterService)
    {
      _adapterService = adapterService;
    }

    #endregion

    /// <summary>
    /// Main method for the Applications
    /// </summary>
    public ActionResult Index(int? id)
    {
      int applicationId = id ?? 0;
      Application application = _adapterService.GetApplication(applicationId) ??
      _adapterService.GetDefaultApplication();

      //log the event
      //_personalizationService.SaveCategoryView(this.GetUserName(), Request.UserHostAddress, category);

      return View(application);
    }

    public ActionResult Show(int id)
    {

      Application application = _adapterService.GetApplication(id);


      if (application == null)
      {
        return RedirectToAction("Index");
      }
      else
      {
        ////log the event
        //_personalizationService.SaveProductView(this.GetUserName(), Request.UserHostAddress, product);
        return View("Show", application);
      }

    }

    public ActionResult ApplicationList()
    {
      return View("_ApplicationList", _adapterService.GetApplications());
    }

    [Authorize(Roles = "Administrator")]
    public ActionResult Edit(int id)
    {
      Application p = _adapterService.GetApplication(id);
      return View(p);
    }

  }
}
