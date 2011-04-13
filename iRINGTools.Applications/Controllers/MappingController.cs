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

using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;

namespace iRINGTools.Web.Controllers
{
  public class MappingController : Controller
  {
    NamespaceMapper _nsMap = new NamespaceMapper();
    private NameValueCollection _settings = null;
    private IMappingRepository _repository { get; set; }
    private string _keyFormat = "Mapping.{0}.{1}";

    public MappingController()
      : this(new MappingRepository())
    { }
    public MappingController(IMappingRepository repository)
    {
      _settings = ConfigurationManager.AppSettings;
      _repository = repository;
    }

    //
    // GET: /Mapping/

    public ActionResult Index()
    {
      return View();
    }

    private Mapping GetMapping(string scope, string application)
    {
      string key = string.Format(_keyFormat, scope, application);

      if (Session[key] == null)
      {
        Session[key] = _repository.GetMapping(scope, application);
      }

      return (Mapping)Session[key];
    }

    public JsonResult AddClassMap(FormCollection form)
    {
      JsonTreeNode nodes = new JsonTreeNode();
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AddTemplateMap(FormCollection form)
    {
      JsonTreeNode nodes = new JsonTreeNode();
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }


    public JsonResult GetNode(FormCollection form)
    {
      GraphMap graphMap = null;
      ClassMap graphclassMap = null;
      char[] delimiters = new char[] { '/' };
      string format = String.Empty;
      string context = form["node"];

      string[] variables = context.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
      string scope = variables[0];
      string application = variables[1];
      string key = string.Format(_keyFormat, scope, application);
        
      Mapping mapping = GetMapping(scope, application);

      List<JsonTreeNode> nodes = new List<JsonTreeNode>();
     // if (variables.Count() > 2)
      //{
        graphMap = mapping.graphMaps.FirstOrDefault();
        if (graphMap != null)
        graphclassMap = graphMap.classTemplateMaps.FirstOrDefault().classMap;
      //}
        switch (form["type"])
        {
          case "MappingNode":
            foreach (var graph in mapping.graphMaps)
            {
              JsonTreeNode graphNode = GetGraphNode(graph, context);

              nodes.Add(graphNode);
            }
            break;
          case "GraphMapNode":
            if (graphMap != null)
            {

              foreach (var templateMaps in graphMap.classTemplateMaps)
              {
                if (templateMaps.classMap.name != graphclassMap.name) continue;
                foreach (var templateMap in templateMaps.templateMaps)
                {

                  JsonTreeNode templateNode = GetTemplateNode(templateMap, context);
                  nodes.Add(templateNode);
                }
              }
            }
            break;
          case "ClassMapNode":
            var classMapId = form["id"];
            if (graphMap != null)
            {
              foreach (var templateMaps in graphMap.classTemplateMaps)
              {
                if (templateMaps.classMap.id == graphclassMap.id) continue;
                foreach (var templateMap in templateMaps.templateMaps)
                {

                  JsonTreeNode templateNode = GetTemplateNode(templateMap, context);
                  nodes.Add(templateNode);
                }
              }
            }
            break;
          case "TemplateMapNode":
            var templateId = form["id"];
            if (graphMap != null)
            {

              foreach (var templateMaps in graphMap.classTemplateMaps)
              {
                if (templateMaps.classMap.name != graphclassMap.name) continue;
                foreach (var templateMap in templateMaps.templateMaps)
                {
                  if (templateMap.id != templateId) continue;
                  foreach (var role in templateMap.roleMaps)
                  {
                    JsonTreeNode roleNode = GetRoleNode(role, context);
                    nodes.Add(roleNode);
                    if (role.classMap != null && role.classMap.id != graphclassMap.id)
                    {
                      JsonTreeNode classNode = GetClassNode(role.classMap, context);
                      if (roleNode.children == null)
                        roleNode.children = new List<JsonTreeNode>();
                      roleNode.children.Add(classNode);
                    }
                    else
                    {
                      roleNode.leaf = true;
                    }
                  }
                }
              }
            }
            break;
          case "RoleMapNode":
            break;
      }
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    private JsonTreeNode GetRoleNode(RoleMap role, string context)
    {
      JsonTreeNode templateNode = new JsonTreeNode
      {
        nodeType = "async",
        type = "RoleMapNode",
        icon = "Content/img/role-map.png",
        id = context + "/" + role.name,
        text = role.IsMapped() ? string.Format("{0}{1}", 
                                 role.name, "") : 
                                 string.Format("{0}{1}", role.name, "[unmapped]"),
        expanded = false,
        leaf = false,
        children = null,
        record = role
      };
      return templateNode;
    }

    private JsonTreeNode GetClassNode(ClassMap classMap, string context)
    {
      JsonTreeNode classNode = new JsonTreeNode
      {
        nodeType = "async",
        type = "ClassMapNode",
        icon = "Content/img/class-map.png",
        id = context + "/" + classMap.name,
        text = classMap.name,
        expanded = false,
        leaf = false,
        children = null,
        record = classMap
      };
      return classNode;
    }

    private JsonTreeNode GetTemplateNode(TemplateMap templateMap, string context)
    {
      JsonTreeNode templateNode = new JsonTreeNode
      {
        nodeType = "async",
        type = "TemplateMapNode",
        icon = "Content/img/template-map.png",
        id = context + "/" + templateMap.name,
        text = templateMap.name,
        expanded = false,
        leaf = false,
        children = null,
        record = templateMap
      };
      return templateNode;
    }

    private JsonTreeNode GetGraphNode(GraphMap graph, string context)
    {
      JsonTreeNode graphNode = new JsonTreeNode
      {
        nodeType = "async",
        type = "GraphMapNode",
        icon = "Content/img/class-map.png",
        id = context + "/" + graph.name,
        text = graph.name,
        expanded = false,
        leaf = false,
        children = null,
        record = graph

      };
      return graphNode;
    }

    public JsonResult AddGraphMap(FormCollection form)
    {
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();
      try
      {
        string qName = string.Empty;

        char[] delimiters = new char[] { '/' };
        string format = String.Empty;
        string propertyCtx = form["objectName"];
        if (string.IsNullOrEmpty(propertyCtx)) throw new Exception("ObjectName has no value");
        string[] dataObjectVars = propertyCtx.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = dataObjectVars[0];
        string application = dataObjectVars[1];
        Mapping mapping = GetMapping(scope, application);

        string graphName = form["graphName"];
        string classLabel = form["classLabel"];
        string keyProperty = dataObjectVars[5];
        string dataObject = dataObjectVars[4];
        string classId = form["classUrl"];
        bool qn = false;

        qn = _nsMap.ReduceToQName(classId, out qName);

        GraphMap graphMap = new GraphMap
        {
          name = graphName,
          dataObjectName = dataObject
        };
        ClassMap classMap = new ClassMap
        {
          name = classLabel,
          id = qName
        };

        classMap.identifiers.Add(string.Format("{0}.{1}", dataObject, keyProperty));

        graphMap.AddClassMap(null, classMap);
        if (mapping.graphMaps == null)
          mapping.graphMaps = new GraphMaps();
        mapping.graphMaps.Add(graphMap);
        nodes.Add(GetGraphNode(graphMap, "MappingNode"));
      }
      catch (Exception ex)
      {
        return Json(nodes, JsonRequestBehavior.AllowGet);
      }
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult UpdateMapping(FormCollection form)
    {
      string scope = form["scope"];
      string application = form["application"];
      Mapping mapping = GetMapping(scope, application);
      try
      {
        _repository.UpdateMapping(scope, application, mapping);
      }
      catch (Exception ex)
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteGraph(FormCollection form)
    {
      
      try
      {
        string scope = form["scope"];
        string application = form["application"];
        Mapping mapping = GetMapping(scope, application);
        string graphName = form["mappingNode"].Split('/')[2];
        GraphMap graphMap = mapping.FindGraphMap(graphName);
        if (graphMap != null)
          mapping.graphMaps.Remove(graphMap);
      }
      catch (Exception ex)
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }
  }
}
