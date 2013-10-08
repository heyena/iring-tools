using System.Web.Mvc;
using iRINGTools.Data;
using iRINGTools.Services;
using iRINGTools.Web.Controllers;
using System.Collections.Generic;

namespace iRINGTools.Web.Controllers
{
  public class PersonalizationController : Controller
  {
    //private IPersonalizationService _pService;

    //public PersonalizationController(IPersonalizationService pService)
    //{
    //  _pService = pService;
    //}

    public ActionResult Summary()
    {
      //UserProfile user = _pService.GetProfile(this.GetUserName());
      //return View(user);
      return null;
    }

    public ActionResult AddressBook()
    {
      //UserProfile user = _pService.GetProfile(this.GetUserName());
      //return View(user.AddressBook);
      return null;
    }

    public ActionResult Favorites()
    {
      //UserProfile user = _pService.GetProfile(this.GetUserName());
      //return View(user);
      return null;
    }

  }
}