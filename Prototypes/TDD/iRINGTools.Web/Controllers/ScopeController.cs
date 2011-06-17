using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using iRINGTools.Data;
using iRINGTools.Services;

namespace iRINGTools.Web.Controllers
{
  public class ScopeController : Controller
  {
    #region constructors
    private IAdapterService _adapterService;

    public ScopeController(IAdapterService adapterService)
    {
      _adapterService = adapterService;
    }

    #endregion

    /// <summary>
    /// Main method for the Scopes
    /// </summary>
    public ActionResult Index(int? id)
    {
      int scopeId = id ?? 0;
      Scope scope = _adapterService.GetScope(scopeId) ??
      _adapterService.GetDefaultScope();

      //log the event
      //_personalizationService.SaveCategoryView(this.GetUserName(), Request.UserHostAddress, category);

      return View(scope);
    }

    public ActionResult Show(int id)
    {
      var result = _adapterService.GetScope(id);

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

    public ActionResult ScopeList()
    {
      var result = _adapterService.GetScopes();
      return View("_ScopeList", result);
    }

    public ActionResult ApplicationList(int scopeId)
    {
      var scope = _adapterService.GetScope(scopeId);
      return View("_ApplicationList", scope.Applications);
    }

    [Authorize(Roles = "Administrator")]
    public ActionResult Edit(int id)
    {
      var result = _adapterService.GetApplication(id);
      return View(result);
    }

  }
}
