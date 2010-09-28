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
using org.iringtools.library.manifest;
using org.iringtools.utility;
using org.iringtools.client.Models;

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
      WebHttpClient client = new WebHttpClient(_adapterServiceURI);
      List<ScopeProject> scopes = client.Get<List<ScopeProject>>("/scopes");      

      List<ScopeTreeNode> nodes = new List<ScopeTreeNode>();

      foreach (ScopeProject scope in scopes)
      {

        ScopeTreeNode nodeScope = new ScopeTreeNode(scope);

        nodes.Add(nodeScope);

        foreach (ScopeApplication app in scope.Applications)
        {
          ApplicationTreeNode nodeApp = new ApplicationTreeNode(app);
          nodeScope.children.Add(nodeApp);
        }
      }

      return Json(nodes, JsonRequestBehavior.AllowGet);    
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
      string scope = Request.QueryString["scope"];
      string application = Request.QueryString["application"];

      JsonContainer<List<Graph>> container = new JsonContainer<List<Graph>>();

      WebHttpClient client = new WebHttpClient(_adapterServiceURI);
      Manifest manifest = client.Get<Manifest>(String.Format("/{0}/{1}/manifest", scope, application));
            
      container.Items = manifest.Graphs;
      container.Total = manifest.Graphs.Count;

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    //
    // Get: Scopes/Mapping?scope={scope}&application={application}

    public JsonResult Mapping()
    {
      string scope = Request.QueryString["scope"];
      string application = Request.QueryString["application"];

      JsonContainer<List<GraphMap>> container = new JsonContainer<List<GraphMap>>();

      WebHttpClient client = new WebHttpClient(_adapterServiceURI);
      Mapping mapping = client.Get<Mapping>(String.Format("/{0}/{1}/mapping", scope, application));
            
      container.Items = mapping.graphMaps;
      container.Total = mapping.graphMaps.Count;

      return Json(container, JsonRequestBehavior.AllowGet);
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
                        
      container.Items = dictionary.dataObjects;
      container.Total = dictionary.dataObjects.Count;

      return Json(container, JsonRequestBehavior.AllowGet);
    }
    
  }
}
