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
using org.iringtools.library;
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
            List<ScopeTreeNode> nodes = new List<ScopeTreeNode>();

            foreach (ScopeProject scope in scopes)
            {

              ScopeTreeNode nodeScope = new ScopeTreeNode
              {
                id = scope.Name,
                expanded = true,

                Name = scope.Name,
                Description = scope.Description                
              };

              nodes.Add(nodeScope);

              foreach (ScopeApplication app in scope.Applications)
              {
                ApplicationTreeNode nodeApp = new ApplicationTreeNode
                {
                  id = scope.Name + ":" + app.Name,
                  expanded = false,
                  leaf = true,

                  Name = app.Name,
                  Description = app.Description,
                  Mapping = "Need Mapping"
                };

                nodeScope.children.Add(nodeApp);
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

    // POST: /Service/Search?query={value}
    [AcceptVerbs(HttpVerbs.Post)]
    public JsonResult Search()
    {
      string query = Request.Form["query"];
      string start = Request.Form["start"];
      string limit = Request.Form["limit"];
      RefDataEntitiesContainer container = new RefDataEntitiesContainer();

      if (query != null && !query.Equals(String.Empty))
      {

        Uri address = new Uri(_refDataServiceURI + "/search/" + query);

        WebClient webClient = new WebClient();
        string result = webClient.DownloadString(address);

        RefDataEntities entities = result.DeserializeDataContract<RefDataEntities>();
        
        container.Entities = new List<Entity>();
        foreach (KeyValuePair<string, Entity> kvp in entities)
        { 
          container.Entities.Add(kvp.Value);
        }
        container.Count = entities.Count;        

      }

      return Json(container, JsonRequestBehavior.AllowGet);
    }

  }
}
