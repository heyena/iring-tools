using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using iRINGTools.Data;
using iRINGTools.Services;

namespace iRINGTools.Web.Controllers
{
  public class DictionaryController : Controller
  {
    #region constructors
    private IAdapterService _adapterService;

    public DictionaryController(IAdapterService adapterService)
    {
      _adapterService = adapterService;
    }

    #endregion

    /// <summary>
    /// Main method for the Dictionary
    /// </summary>
    public ActionResult Index(int? id)
    {
      int configurationId = id ?? 0;
      Dictionary configuration = _adapterService.GetDictionary(configurationId) ?? 
        _adapterService.GetDefaultApplication().Dictionary;

      //log the event
      //_personalizationService.SaveCategoryView(this.GetUserName(), Request.UserHostAddress, category);

      return View(configuration);
    }

    public ActionResult Show(int id)
    {
      Dictionary result = _adapterService.GetDictionary(id);

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
      Dictionary result = _adapterService.GetDictionary(id);
      return View(result);
    }

  }
}
