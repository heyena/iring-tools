using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using iRINGTools.Data;
using iRINGTools.Services;

namespace iRINGTools.Web.Controllers
{
  public class ConfigurationController : Controller
  {
    #region constructors
    private IAdapterService _adapterService;

    public ConfigurationController(IAdapterService adapterService)
    {
      _adapterService = adapterService;
    }

    #endregion

    /// <summary>
    /// Main method for the Configuration
    /// </summary>
    public ActionResult Index(int? id)
    {
      int configurationId = id ?? 0;
      Configuration configuration = _adapterService.GetConfiguration(configurationId) ?? 
        _adapterService.GetDefaultApplication().Configuration;

      //log the event
      //_personalizationService.SaveCategoryView(this.GetUserName(), Request.UserHostAddress, category);

      return View(configuration);
    }

    public ActionResult Show(int id)
    {
      Configuration result = _adapterService.GetConfiguration(id);

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
      Configuration result = _adapterService.GetConfiguration(id);
      return View(result);
    }

  }
}
