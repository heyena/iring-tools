using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using org.iringtools.library;
using org.iringtools.utility;
using System.Configuration;
using System.Collections.Specialized;
using org.iringtools.client.Models;

namespace org.iringtools.client.Controllers
{
  public class AdapterServiceController : Controller
  {
    //
    // GET: /AdapterService/
    NameValueCollection _settings = null;
    string _adapterServiceURI = String.Empty;   

    public AdapterServiceController()
    {
      _settings = ConfigurationManager.AppSettings;
      _adapterServiceURI = _settings["AdapterServiceUri"];
    }

    public ActionResult Index()
    {
      return View();
    }

    public JsonResult Scopes(String format)
    {

      Uri address = new Uri(_adapterServiceURI + "/scopes");

      WebClient webClient = new WebClient();
      string result = webClient.DownloadString(address);

      Collection<ScopeProject> scopes = result.DeserializeDataContract<Collection<ScopeProject>>();

      switch (format) {
        case "tree":
          {
            List<ProjectTreeContainer> root = new List<ProjectTreeContainer>();

            foreach (ScopeProject project in scopes)
            {

              ProjectTreeContainer treePrj = new ProjectTreeContainer 
              {
                name = project.Name,
                description = project.Description                
              };

              root.Add(treePrj);

              foreach (ScopeApplication application in project.Applications)
              {
                ProjectTreeContainer treeApp = new ProjectTreeContainer
                {
                  name = project.Name,
                  description = project.Description
                };

                treePrj.children.Add(treeApp);
              }
            }

            return Json(scopes, JsonRequestBehavior.AllowGet);
          }
        default:
          {
            ProjectContainer projects = new ProjectContainer
            {
              Projects = scopes,
              Count = scopes.Count
            };

            return Json(scopes, JsonRequestBehavior.AllowGet);
          }
      }
    }

  }
}
