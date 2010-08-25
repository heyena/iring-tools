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
  public class ServiceController : Controller
  {
    //
    // GET: /AdapterService/
    NameValueCollection _settings = null;
    string _adapterServiceURI = String.Empty;
    string _refDataServiceURI = String.Empty;

    public ServiceController()
    {
      _settings = ConfigurationManager.AppSettings;
      _adapterServiceURI = _settings["AdapterServiceUri"];
      _refDataServiceURI = _settings["ReferenceDataServiceUri"];      
    }

    public ActionResult Index()
    {
      return View();
    }    

    // GET: /Service/Scopes?format={tree|grid}
    public JsonResult Scopes()
    {
      string format = Server.HtmlEncode(Request.QueryString["format"]);
      if (format == null) { format = String.Empty; }

      Uri address = new Uri(_adapterServiceURI + "/scopes");

      WebClient webClient = new WebClient();
      string result = webClient.DownloadString(address);

      List<ScopeProject> scopes = result.DeserializeDataContract<List<ScopeProject>>();

      switch (format.ToUpper())
      {
        case "TREE":
          {
            List<ProjectTreeNode> nodes = new List<ProjectTreeNode>();

            foreach (ScopeProject prj in scopes)
            {

              ProjectTreeNode treePrj = new ProjectTreeNode
              {
                name = prj.Name,
                description = prj.Description,
                expanded = true
              };

              nodes.Add(treePrj);

              foreach (ScopeApplication app in prj.Applications)
              {
                ProjectTreeNode treeApp = new ProjectTreeNode
                {
                  name = app.Name,
                  description = app.Description,                  
                  leaf = true
                };

                treePrj.children.Add(treeApp);
              }
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
          }
        case "GRID":
          {
            ProjectContainer projects = new ProjectContainer
            {
              Projects = scopes,
              Count = scopes.Count
            };

            return Json(projects, JsonRequestBehavior.AllowGet);
          }
        default:
          {
            return Json(scopes, JsonRequestBehavior.AllowGet);
          }
      }
    }

    // GET: /Service/Search/{query}
    public JsonResult Search(string param)
    {
      Uri address = new Uri(_refDataServiceURI + "/search/" + param);

      WebClient webClient = new WebClient();
      string result = webClient.DownloadString(address);

      List<RefDataEntities> entities = result.DeserializeDataContract<List<RefDataEntities>>();

      RefDataEntitiesContainer container = new RefDataEntitiesContainer
      {
        RefDataEntities = entities,
        Count = entities.Count
      };

      return Json(container, JsonRequestBehavior.AllowGet);
    }

  }
}
