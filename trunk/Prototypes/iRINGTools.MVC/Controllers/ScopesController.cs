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
      Uri address = new Uri(_adapterServiceURI + "/scopes");

      WebClient webClient = new WebClient();
      string result = webClient.DownloadString(address);

      List<ScopeProject> scopes = result.DeserializeDataContract<List<ScopeProject>>();

      ScopesContainer container = new ScopesContainer
      {
        Scopes = scopes,
        Total = scopes.Count
      };
    
      return Json(container, JsonRequestBehavior.AllowGet);
    
    }

    //
    // GET: /Scopes?scope=12345_000

    public JsonResult Scope(string id)
    {
      Uri address = new Uri(_adapterServiceURI + "/scopes");

      WebClient webClient = new WebClient();
      string result = webClient.DownloadString(address);

      List<ScopeProject> scopes = result.DeserializeDataContract<List<ScopeProject>>();

      ScopeProject scope = scopes.Find(o => o.Name == id);

      return Json(scope, JsonRequestBehavior.AllowGet);      
    }

    //
    // GET: /Scopes/Application/ABC

    public JsonResult Application(string id)
    {
      Uri address = new Uri(_adapterServiceURI + "/scopes");

      WebClient webClient = new WebClient();
      string result = webClient.DownloadString(address);

      List<ScopeProject> scopes = result.DeserializeDataContract<List<ScopeProject>>();

      ScopeProject scope = scopes.Find(o => o.Name == id);

      ApplicationContainer container = new ApplicationContainer();

      if (scope != null)
      {
        container.Applications = scope.Applications;
        container.Total = scope.Applications.Count;
      }

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    //
    // GET: /Scopes/Create
    
    public JsonResult Create()
    {
      return Json(null, JsonRequestBehavior.AllowGet);
    }

    //
    // POST: /Scopes/Create

    [HttpPost]
    public JsonResult Create(FormCollection collection)
    {
      try
      {
        // TODO: Add insert logic here

        //return RedirectToAction("Index");
        return Json(null, JsonRequestBehavior.AllowGet);
      }
      catch
      {
        return Json(null, JsonRequestBehavior.AllowGet);
      }
    }

    //
    // GET: /Scopes/Edit/5
    
    public JsonResult Edit(int id)
    {
      return Json(null, JsonRequestBehavior.AllowGet);
    }

    //
    // POST: /Scopes/Edit/5

    [HttpPost]
    public JsonResult Edit(int id, FormCollection collection)
    {
      try
      {
        // TODO: Add update logic here

        //return RedirectToAction("Index");
        return Json(null, JsonRequestBehavior.AllowGet);
      }
      catch
      {
        return Json(null, JsonRequestBehavior.AllowGet);
      }
    }

    //
    // GET: /Scopes/Delete/5

    public JsonResult Delete(int id)
    {
      return Json(null, JsonRequestBehavior.AllowGet);
    }

    //
    // POST: /Scopes/Delete/5

    [HttpPost]
    public JsonResult Delete(int id, FormCollection collection)
    {
      try
      {
        // TODO: Add delete logic here

        //return RedirectToAction("Index");
        return Json(null, JsonRequestBehavior.AllowGet);
      }
      catch
      {
        return Json(null, JsonRequestBehavior.AllowGet);
      }
    }

    //
    // POST: /Scopes/Navigation

    public JsonResult Navigation()
    {
      Uri address = new Uri(_adapterServiceURI + "/scopes");

      WebClient webClient = new WebClient();
      string result = webClient.DownloadString(address);

      List<ScopeProject> scopes = result.DeserializeDataContract<List<ScopeProject>>();

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
  }
}
