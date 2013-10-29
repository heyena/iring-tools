using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;

namespace iRINGTools.WebMVC.Controllers
{
  public class HomeController : Controller
  {
    //
    // GET: /Home/

    public ActionResult Index()
    {
      return View();
    }

    public ActionResult SPARQLQuery()
    {
      return View();
    }    

  }
}
