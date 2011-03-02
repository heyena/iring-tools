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
using System.Xml.Linq;

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

    //
    // GET: /Directory/

    private ScopeProjects GetScopes(string adapterServiceURI, bool fetch)
    {
      string key = "Scopes";

      if (Session[key] == null || fetch) {
                
        WebHttpClient client = new WebHttpClient(adapterServiceURI);
        ScopeProjects scopes = client.Get<ScopeProjects>("/scopes");

        Session[key] = scopes;        
      }

      return (ScopeProjects)Session[key];
    }

    private DataLayers GetDataLayers(string adapterServiceURI, bool fetch)
    {
      string key = "DataLayers";

      if (Session[key] == null || fetch)
      {

        WebHttpClient client = new WebHttpClient(adapterServiceURI);
        DataLayers datalayers = client.Get<DataLayers>("/datalayers");

        Session[key] = datalayers;
      }

      return (DataLayers)Session[key];
    }

    private Mapping GetMapping(string adapterServiceURI, bool fetch, string scope, string application)
    {
      string key = String.Format("{0}.{1}.mapping", scope, application);

      if (Session[key] == null || fetch) {
                
        WebHttpClient client = new WebHttpClient(adapterServiceURI);
        Mapping mapping = client.Get<Mapping>(String.Format("/{0}/{1}/mapping", scope, application), true);

        Session[key] = mapping;        
      }

      return (Mapping)Session[key];      
    }

    private DataDictionary GetDictionary(string adapterServiceURI, bool fetch, string scope, string application)
    {
      string key = String.Format("{0}.{1}.dictionary", scope, application);

      if (Session[key] == null || fetch)
      {

        WebHttpClient client = new WebHttpClient(adapterServiceURI);
        DataDictionary dictionary = client.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", scope, application), true);

        Session[key] = dictionary;
      }

      return (DataDictionary)Session[key];
    }

    private JsonResult PostScopes(string adapterServiceURI, ScopeProjects scopes) 
    {
      WebHttpClient client = new WebHttpClient(adapterServiceURI);
            
      string responseMessage = client.Post<ScopeProjects>("/scopes", scopes, true);
            
      if (responseMessage.Contains("success"))
      {
        GetScopes(adapterServiceURI, true);

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

    public JsonResult Index()
    {
      string format = String.Empty;
      string adapterServiceURI = _adapterServiceURI;

      if (Request.QueryString["format"] != null)
        format = Request.QueryString["format"].ToUpper();

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";

      ScopeProjects scopes = GetScopes(adapterServiceURI, false);

      switch (format)
      {
        case "TREE":
          {
            List<TreeNode> nodes = new List<TreeNode>();

            foreach (ScopeProject scope in scopes)
            {

              TreeNode nodeScope = new TreeNode
              {
                type = "ScopeNode",
                icon = "Content/img/system-file-manager.png",
                id = scope.Name,
                text = scope.Name,
                expanded = true,
                leaf = false,
                children = new List<TreeNode>(),
                record = scope
              };

              nodes.Add(nodeScope);

              foreach (ScopeApplication app in scope.Applications)
              {
                if (app.Name != string.Empty)
                {
                  TreeNode nodeApp = new TreeNode
                  {
                    type = "ApplicaionNode",
                    icon = "Content/img/applications-internet.png",
                    id = app.Name,
                    text = app.Name,
                    expanded = true,
                    leaf = false,
                    record = app
                  };
                  nodeScope.children.Add(nodeApp);

                  //List<string> graphs = GetGraphs(scope.Name, app.Name);

                  //foreach (string graph in graphs)
                  //{
                  //  GraphTreeNode nodeGraph = new GraphTreeNode(graph);
                  //  nodeApp.children.Add(nodeGraph);
                  //}
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

    public JsonResult GetNode(FormCollection form)
    {
      string format = String.Empty;
      string adapterServiceURI = _adapterServiceURI;

      if (Request.QueryString["format"] != null)
        format = Request.QueryString["format"].ToUpper();

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";

      switch (form["type"])
      {
        case "ScopesNode":
          {

            List<TreeNode> nodes = new List<TreeNode>();
            ScopeProjects scopes = GetScopes(adapterServiceURI, false);

            foreach (ScopeProject scope in scopes)
            {

              TreeNode node = new TreeNode
              {
                nodeType = "async",
                type = "ScopeNode",
                icon = "Content/img/system-file-manager.png",
                id = scope.Name,
                text = scope.Name,
                expanded = false,
                leaf = false,
                children = null,
                record = scope
              };

              nodes.Add(node);

            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
          }
        case "ScopeNode":
          {

            List<TreeNode> nodes = new List<TreeNode>();
            ScopeProjects scopes = GetScopes(adapterServiceURI, false);

            ScopeProject scope = scopes.FirstOrDefault(o => o.Name == form["node"]);

            foreach (ScopeApplication app in scope.Applications)
            {

              TreeNode node = new TreeNode
              {
                nodeType = "async",
                type = "ApplicationNode",
                icon = "Content/img/applications-internet.png",
                id = scope.Name + "/" + app.Name,
                text = app.Name,
                expanded = false,
                leaf = false,
                children = null,
                record = app
              };

              nodes.Add(node);

            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
          }
        case "ApplicationNode":
          {
            string context = form["node"];
            
            List<TreeNode> nodes = new List<TreeNode>();

            TreeNode dataObjectsNode = new TreeNode
            {
              nodeType = "async",
              type = "DataObjectsNode",
              icon = "Content/img/folder.png",
              id = context + "/DataObjects",
              text = "Data Objects",
              expanded = false,
              leaf = false,
              children = null
            }; 

            TreeNode graphsNode = new TreeNode
            {
              nodeType = "async",
              type = "GraphsNode",
              icon = "Content/img/folder.png",
              id = context + "/Graphs",
              text = "Graphs",
              expanded = false,
              leaf = false,
              children = null
            };

            TreeNode ValueListsNode = new TreeNode
            {
              nodeType = "async",
              type = "ValueListsNode",
              icon = "Content/img/folder.png",
              id = context + "/ValueLists",
              text = "ValueLists",
              expanded = false,
              leaf = false,
              children = null
            };

            nodes.Add(dataObjectsNode);
            nodes.Add(graphsNode);
            nodes.Add(ValueListsNode);

            return Json(nodes, JsonRequestBehavior.AllowGet);
          }
        case "ValueListsNode":
          {
            string context = form["node"];

            Mapping mapping = GetMapping(adapterServiceURI, false, context.Split('/')[0], context.Split('/')[1]);

            List<TreeNode> nodes = new List<TreeNode>();

            if (mapping.valueListMaps != null)
            {
              foreach (ValueListMap valueList in mapping.valueListMaps)
              {
                TreeNode node = new TreeNode
                {
                  nodeType = "async",
                  type = "ValueListNode",
                  icon = "Content/img/valuelist.png",
                  id = context + "/ValueList/" + valueList.name,
                  text = valueList.name,
                  expanded = true,
                  leaf = true,
                  children = new List<TreeNode>(),
                  record = valueList
                };

                nodes.Add(node);
              }
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
          }
        case "DataObjectsNode":
          {

            string context = form["node"];

            DataDictionary dictionary = GetDictionary(adapterServiceURI, false, context.Split('/')[0], context.Split('/')[1]);

            List<TreeNode> nodes = new List<TreeNode>();

            foreach (DataObject dataObject in dictionary.dataObjects)
            {
              TreeNode node = new TreeNode
              {
                nodeType = "async",
                type = "DataObjectNode",
                icon = "Content/img/object.png",
                id = context + "/DataObject/" + dataObject.objectName,
                text = dataObject.objectName,
                expanded = false,
                leaf = false,
                children = null,
                record = dataObject
              };

              nodes.Add(node);
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);

          }
        case "DataObjectNode":
          {
            string context = form["node"];

            DataDictionary dictionary = GetDictionary(adapterServiceURI, false, context.Split('/')[0], context.Split('/')[1]);

            DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName == context.Split('/')[4]);

            List<TreeNode> nodes = new List<TreeNode>();

            foreach (DataProperty property in dataObject.dataProperties)
            {
              TreeNode node = new TreeNode
              { 
                nodeType = "async",
                type = "DataPropertyNode",
                icon = (dataObject.isKeyProperty(property.propertyName)) ? "Content/img/key.png" : "Content/img/property.png",
                id = context + "/" + property.propertyName,
                text = property.propertyName,
                expanded = true,
                leaf = true,
                children = new List<TreeNode>(),
                record = property
              };

              nodes.Add(node);
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);

          }
        case "GraphsNode": {

          string context = form["node"];
          Mapping mapping = GetMapping(adapterServiceURI, false, context.Split('/')[0], context.Split('/')[1]);

          List<TreeNode> nodes = new List<TreeNode>();

          foreach (GraphMap graph in mapping.graphMaps)
          {
            TreeNode node = new TreeNode
            {
              nodeType = "async",
              type = "GraphNode",
              icon = "Content/img/graph-map.png",
              id = context + "/Graph/" + graph.name,
              text = graph.name,
              expanded = true,
              leaf = true,
              children = new List<TreeNode>(),
              record = graph
            };

            nodes.Add(node);
          }

          return Json(nodes, JsonRequestBehavior.AllowGet);

        }
        default:
          {
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
          }
      }

    }

    public JsonResult DataLayers()
    {
      string adapterServiceURI = _adapterServiceURI;
            
      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";
      
      DataLayers dataLayers = GetDataLayers(adapterServiceURI, false);

      JsonContainer<DataLayers> container = new JsonContainer<DataLayers>();
      container.items = dataLayers;
      container.success = true;
      container.total = dataLayers.Count;

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Scope(FormCollection form)
    {
      string adapterServiceURI = _adapterServiceURI;
            
      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";      
            
      ScopeProjects scopes = GetScopes(adapterServiceURI, false);

      ScopeProject project = scopes.FirstOrDefault(o => o.Name == form["Name"]);

      if (project == null) 
      {
        project = new ScopeProject();        
        project.Applications = new ScopeApplications();
      }

      project.Name = form["Name"];
      project.Description = form["Description"];

      ScopeProjects projects = new ScopeProjects();
      scopes.Add(project);

      return PostScopes(adapterServiceURI, projects);
    }

    public JsonResult Application(FormCollection form)
    {
      string adapterServiceURI = _adapterServiceURI;

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";

      WebHttpClient client = new WebHttpClient(adapterServiceURI);
      ScopeProjects scopes = client.Get<ScopeProjects>("/scopes");
            
      ScopeApplication application = new ScopeApplication();

      application.Name = form["Name"];
      application.Description = form["Description"];

      return PostScopes(adapterServiceURI, scopes);
    }

    public JsonResult DeleteNode()
    {
      string format = String.Empty;
      string adapterServiceURI = _adapterServiceURI;
      bool isDelete = false;

      if (Request.QueryString["format"] != null)
        format = Request.QueryString["format"].ToUpper();

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";

      WebHttpClient client = new WebHttpClient(adapterServiceURI);
      ScopeProjects scopes = client.Get<ScopeProjects>("/scopes");

      string relativeUri = String.Format("/scopes");
      Uri address = new Uri(adapterServiceURI + relativeUri);

      for (int i = 0; i < scopes.Count(); i++)
      {
        if (scopes[i].Name == Request.QueryString["parentNodeID"])
        {
          for (int j = 0; j < scopes[i].Applications.Count(); j++)
          {
            if (scopes[i].Applications[j].Name == Request.QueryString["nodeID"])
            {
              //scopes[i].Applications[j].Name = _editApplication.Name;
              //scopes[i].Applications[j].Description = _editApplication.Description;
              scopes[i].Applications.Remove(scopes[i].Applications[j]);
              isDelete = true;
            }
          }
        }
      }
      if (!isDelete)
      {
        for (int i = 0; i < scopes.Count(); i++)
        {
          if (scopes[i].Name == Request.QueryString["nodeID"])
          {
            scopes.Remove(scopes[i]);
          }
        }
      }

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
