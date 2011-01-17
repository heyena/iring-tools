using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;

using org.iringtools.library;
//using org.iringtools.library.manifest;
using org.iringtools.utility;
using org.iringtools.client.Models;
using System.Runtime.Serialization;
//using org.iringtools.client.Contrib;
using System.Xml.Serialization;
using System.Xml;
using System.ServiceModel;
using org.iringtools.mapping;

namespace org.iringtools.client.Controllers
{
  public class ScopesController : Controller
  {
    NameValueCollection _settings = null;
    string _adapterServiceURI = String.Empty;
    string _refDataServiceURI = String.Empty;

    public ScopesController()
    {
      _settings = ConfigurationManager.AppSettings;
      _adapterServiceURI = _settings["AdapterServiceUri"];
      _refDataServiceURI = _settings["ReferenceDataServiceUri"];
    }

    //
    // GET: /Scopes
    
    public JsonResult Index()
    {
      string format = String.Empty;
      string adapterServiceURI = _adapterServiceURI;

      if (Request.QueryString["format"] != null)
        format = Request.QueryString["format"].ToUpper();

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"]+"/adapter";

      WebHttpClient client = new WebHttpClient(adapterServiceURI);
      ScopeProjects scopes = client.Get<ScopeProjects>("/scopes");
      
      switch (format)
      {
        case "TREE":
          {
            List<ScopeTreeNode> nodes = new List<ScopeTreeNode>();

            foreach (ScopeProject scope in scopes)
            {

              ScopeTreeNode nodeScope = new ScopeTreeNode(scope,null,null);

              nodes.Add(nodeScope);

              foreach (ScopeApplication app in scope.Applications)
              {
                ApplicationTreeNode nodeApp = new ApplicationTreeNode(app, scope, null);
                nodeScope.children.Add(nodeApp);
                List<string> graphs = GetGraphs(scope.Name, app.Name);

                foreach (string graph in graphs)
                {
                  GraphTreeNode nodeGraph = new GraphTreeNode(graph, scope, app);
                  nodeApp.children.Add(nodeGraph);
                }
              }
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
          }
        default:
          {
            JsonContainer<List<ScopeProject>> container = new JsonContainer<List<ScopeProject>>();
            container.Items = scopes;
            container.Total = scopes.Count;
            container.success = true;
            return Json(container, JsonRequestBehavior.AllowGet);
          }
      }
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

    public JsonResult Applications()
    {
      string scope = String.Empty;
      string adapterServiceURI = _adapterServiceURI;

      if (Request.QueryString["scope"] != null)
        scope = Request.QueryString["scope"];

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"]+"/adapter";

      WebHttpClient client = new WebHttpClient(adapterServiceURI);
      List<ScopeProject> scopes = client.Get<List<ScopeProject>>("/scopes");
      ScopeProject scopePrj = scopes.FirstOrDefault<ScopeProject>(o=>o.Name == scope);
            
      JsonContainer<List<ScopeApplication>> container = new JsonContainer<List<ScopeApplication>>();

      if (scopePrj != null)
      {
        container.Items = scopePrj.Applications;
        container.Total = scopePrj.Applications.Count;
        container.success = true;
      }

      return Json(container, JsonRequestBehavior.AllowGet);            
    }

    //
    // Get: Scopes/Binding?scope={scope}&application={application}

    public ActionResult Binding()
    {
      string scope = Request.QueryString["scope"];
      string application = Request.QueryString["application"];
            
      WebClient client = new WebClient();
      string request = client.DownloadString(String.Format(_adapterServiceURI+"/{0}/{1}/binding", scope, application));

      return this.Content(request, "text/xml");
    }


    //
    // Get: Scopes/Manifest?scope={scope}&application={application}

    public JsonResult Manifest()
    {
      string adapterServiceURI = _adapterServiceURI;

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"]+"/adapter";

      string scope = Request.QueryString["scope"];
      string application = Request.QueryString["application"];

      JsonContainer<List<GraphMap>> container = new JsonContainer<List<GraphMap>>();

      WebHttpClient client = new WebHttpClient(adapterServiceURI);
      Mapping mapping = client.Get<Mapping>(String.Format("/{0}/{1}/mapping", scope, application));

      container.Items = mapping.graphMaps;
      container.Total = mapping.graphMaps.Count;

      return Json(container, JsonRequestBehavior.AllowGet);
    }


    public ActionResult Test()
    {
      string request = System.Net.Dns.GetHostEntry("adcrdlweb").HostName;

      return this.Content(request, "text/xml");
    }

    //
    // Get: Scopes/Mapping?scope={scope}&application={application}

    public ActionResult Mapping()
    {
      string adapterServiceURI = _adapterServiceURI;

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";

      string scope = Request.QueryString["scope"];
      string application = Request.QueryString["application"];

      WebHttpClient client = new WebHttpClient(adapterServiceURI);
      string request = Utility.Serialize<Mapping>(client.Get<Mapping>(string.Format("/{0}/{1}/mapping", scope, application)),true);

      return this.Content(request, "text/xml");
    }

    //
    // Get: Scopes/Dictionary?scope={scope}&application={application}

    public JsonResult Dictionary()
    {
      string scope = Request.QueryString["scope"];
      string application = Request.QueryString["application"];

      JsonContainer<List<DataObject>> container = new JsonContainer<List<DataObject>>();
      
      WebHttpClient client = new WebHttpClient(_adapterServiceURI);
      DataDictionary dictionary = client.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", scope, application));
                        
      container.Items = dictionary.DataObjects;
      container.Total = dictionary.DataObjects.Count;

      return Json(container, JsonRequestBehavior.AllowGet);
    }
    
  }

}
