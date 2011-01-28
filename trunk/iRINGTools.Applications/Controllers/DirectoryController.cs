using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;

using iRINGTools.Web.Models;

namespace iRINGTools.Web.Controllers
{
  public class DirectoryController : Controller
  {

    NameValueCollection _settings = null;
    string _adapterServiceURI = String.Empty;
    string _refDataServiceURI = String.Empty;

    public DirectoryController()
    {
      _settings = ConfigurationManager.AppSettings;
      _adapterServiceURI = _settings["AdapterServiceUri"];
      _refDataServiceURI = _settings["ReferenceDataServiceUri"];
    }

    private List<string> GetGraphs(string scope, string application)
    {
      List<string> graphs = new List<string>();
      WebHttpClient client = new WebHttpClient(_adapterServiceURI);
      Mapping mapping = client.Get<Mapping>("/" + scope + "/" + application + "/mapping", true);
      foreach (GraphMap graph in mapping.graphMaps)
      {
        graphs.Add(graph.name);
      }
      return graphs;
    }

    //
    // GET: /Directory/

    public JsonResult Index()
    {
      string format = String.Empty;
      string adapterServiceURI = _adapterServiceURI;

      if (Request.QueryString["format"] != null)
        format = Request.QueryString["format"].ToUpper();

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";

      WebHttpClient client = new WebHttpClient(adapterServiceURI);
      ScopeProjects scopes = client.Get<ScopeProjects>("/scopes");

      switch (format)
      {
        case "TREE":
          {
            List<ScopeTreeNode> nodes = new List<ScopeTreeNode>();

            foreach (ScopeProject scope in scopes)
            {

              ScopeTreeNode nodeScope = new ScopeTreeNode(scope);

              nodes.Add(nodeScope);

              foreach (ScopeApplication app in scope.Applications)
              {
                ApplicationTreeNode nodeApp = new ApplicationTreeNode(app);
                nodeScope.children.Add(nodeApp);
                
                List<string> graphs = GetGraphs(scope.Name, app.Name);

                foreach (string graph in graphs)
                {
                  GraphTreeNode nodeGraph = new GraphTreeNode(graph);
                  nodeApp.children.Add(nodeGraph);
                }
                
              }
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
          }        
        default:
          {
            JsonContainer<List<ScopeProject>> container = new JsonContainer<List<ScopeProject>>();
            container.items = scopes;
            container.total = scopes.Count;
            container.success = true;
            return Json(container, JsonRequestBehavior.AllowGet);
          }
      }
    }

    public JsonResult Scope(FormCollection form)
    {
      string format = String.Empty;
      string adapterServiceURI = _adapterServiceURI;

      if (Request.QueryString["format"] != null)
        format = Request.QueryString["format"].ToUpper();

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";

      WebHttpClient client = new WebHttpClient(adapterServiceURI);
      ScopeProjects scopes = client.Get<ScopeProjects>("/scopes");
      
      string relativeUri = String.Format("/scopes");
      Uri address = new Uri(adapterServiceURI + relativeUri);
      ScopeProject _editApplication = new ScopeProject();

      _editApplication.Name = form["appName"];
      _editApplication.Description = form["description"];

      //             var _scopesIndex = scopes.Where(x => x.Name == this.Request.Form["nodeID"]).FirstOrDefault();

      for (int i = 0; i < scopes.Count(); i++)
      {
        if (scopes[i].Name == form["nodeID"])
        {
          scopes[i].Name = _editApplication.Name;
          scopes[i].Description = _editApplication.Description;
        }
      }

      string data = Utility.SerializeDataContract<ScopeProjects>(scopes);
      string responseMessage = client.Post<ScopeProjects>(relativeUri, scopes, true);
      if (responseMessage.Contains("success"))
      {
        return Json(new
        {
          success = true
        }, JsonRequestBehavior.AllowGet);
      }
      else
      {
        return Json(new
        {
          success = false
        }, JsonRequestBehavior.AllowGet);
      }
    }

    public JsonResult Application(FormCollection form)
    {
      string format = String.Empty;
      string adapterServiceURI = _adapterServiceURI;

      if (Request.QueryString["format"] != null)
        format = Request.QueryString["format"].ToUpper();

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";

      WebHttpClient client = new WebHttpClient(adapterServiceURI);
      ScopeProjects scopes = client.Get<ScopeProjects>("/scopes");

      string relativeUri = String.Format("/scopes");
      Uri address = new Uri(adapterServiceURI + relativeUri);
      ScopeApplication _editApplication = new ScopeApplication();

      _editApplication.Name = form["appName"];
      _editApplication.Description = form["description"];

      // var _scopesIndex = scopes.Where(x => x.Name == this.Request.Form["parentNodeID"]).FirstOrDefault().Applications.ToList();

      for (int i = 0; i < scopes.Count(); i++)
      {
        if (scopes[i].Name == form["parentNodeID"])
        {
          for (int j = 0; j < scopes[i].Applications.Count(); j++)
          {
            if (scopes[i].Applications[j].Name == form["nodeID"])
            {
              scopes[i].Applications[j].Name = _editApplication.Name;
              scopes[i].Applications[j].Description = _editApplication.Description;
            }
          }

        }
      }

      string data = Utility.SerializeDataContract<ScopeProjects>(scopes);
      string responseMessage = client.Post<ScopeProjects>(relativeUri, scopes, true);
      if (responseMessage.Contains("success"))
      {
        return Json(new
        {
          success = true
        }, JsonRequestBehavior.AllowGet);
      }
      else
      {
        return Json(new
        {
          success = false
        }, JsonRequestBehavior.AllowGet);
      }
    }

  }
}
