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
                children = new List<TreeNode>()
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
                    leaf = false
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

      WebHttpClient client = new WebHttpClient(adapterServiceURI);

      switch (form["type"])
      {
        case "ScopesNode":
          {

            List<TreeNode> nodes = new List<TreeNode>();
            ScopeProjects scopes = client.Get<ScopeProjects>("/scopes");

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
                children = null
              };

              nodes.Add(node);

            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
          }
        case "ScopeNode":
          {

            List<TreeNode> nodes = new List<TreeNode>();
            ScopeProjects scopes = client.Get<ScopeProjects>("/scopes");

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
                children = null                
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

            Mapping mapping = client.Get<Mapping>(String.Format("/{0}/{1}/mapping", context.Split('/')[0], context.Split('/')[1]), true);

            List<TreeNode> nodes = new List<TreeNode>();

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
                children = new List<TreeNode>()
              };

              nodes.Add(node);
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
          }
        case "DataObjectsNode":
          {

            string context = form["node"];

            DataDictionary dictionary = client.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", context.Split('/')[0], context.Split('/')[1]));

            List<TreeNode> nodes = new List<TreeNode>();

            foreach (DataObject dataObject in dictionary.DataObjects)
            {
              TreeNode node = new TreeNode
              {
                nodeType = "async",
                type = "DataObjectNode",
                icon = "Content/img/object.png",
                id = context + "/DataObject/" + dataObject.ObjectName,
                text = dataObject.ObjectName,
                expanded = false,
                leaf = false,
                children = null
              };

              nodes.Add(node);
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);

          }
        case "DataObjectNode":
          {
            string context = form["node"];

            DataDictionary dictionary = client.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", context.Split('/')[0], context.Split('/')[1]));

            DataObject dataObject = dictionary.DataObjects.FirstOrDefault(o => o.ObjectName == context.Split('/')[4]);

            List<TreeNode> nodes = new List<TreeNode>();

            foreach (DataProperty property in dataObject.DataProperties)
            {
              TreeNode node = new TreeNode
              { 
                nodeType = "async",
                type = "DataPropertyNode",
                icon = (dataObject.IsKeyProperty(property.PropertyName)) ? "Content/img/key.png" : "Content/img/property.png",
                id = context + "/" + property.PropertyName,
                text = property.PropertyName,
                expanded = true,
                leaf = true,
                children = new List<TreeNode>()
              };

              nodes.Add(node);
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);

          }
        case "GraphsNode": {

          string context = form["node"];
          Mapping mapping = client.Get<Mapping>(String.Format("/{0}/{1}/mapping", context.Split('/')[0], context.Split('/')[1]), true);

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
              children = new List<TreeNode>()
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

    public JsonResult Scope(FormCollection form)
    {
      string format = String.Empty;
      string adapterServiceURI = _adapterServiceURI;
      bool isEdit = false;

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
      if (form["formtype"].ToUpper() == "EDITFORM")
      {
        for (int i = 0; i < scopes.Count(); i++)
        {
          if (scopes[i].Name == form["nodeID"])
          {
            scopes[i].Name = _editApplication.Name;
            scopes[i].Description = _editApplication.Description;
            isEdit = true;
          }
        }
      }
      else
      {
        ScopeApplications app = new ScopeApplications();
        //app.Add(new ScopeApplication { Name = string.Empty, Description = string.Empty });
        scopes.Add(new ScopeProject { Applications = app, Name = _editApplication.Name.Trim(), Description = _editApplication.Description.Trim() });

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
      if (form["formtype"].ToUpper() == "EDITFORM")
      {
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
      }
      else
      {
        for (int i = 0; i < scopes.Count(); i++)
        {
          if (scopes[i].Name == form["nodeID"])
          {
            scopes[i].Applications.Add(new ScopeApplication { Name = _editApplication.Name, Description = _editApplication.Description });
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
