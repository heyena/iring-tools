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

    private void GetMapping(string scope, string application)
    {
      string key = string.Format(_keyFormat, scope, application);

      if (Session[key] == null)
      {
        Session[key] = _repository.GetMapping(scope, application);
      }
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

      if (Session[key] == null)
        GetMapping(scope, application);
      Mapping mapping = (Mapping)Session[key];
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();
      if (variables.Count() > 2)
      {
        graphMap = mapping.graphMaps.FirstOrDefault(c => c.name == variables[2]);
        graphclassMap = graphMap.classTemplateMaps.FirstOrDefault().classMap;
      }
      switch (form["type"])
      {
        case "mapping":
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
          var classMapId = form["record"];
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
          var templateId = form["record"];
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
                }
              }
            }
          }
          break;
        case "RoleMapNode":
          break;
        default:
          foreach (var graph in mapping.graphMaps)
          {
            JsonTreeNode graphNode = GetGraphNode(graph, context);

            nodes.Add(graphNode);
          }
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
        text = role.name,
        expanded = false,
        leaf = false,
        children = null,
        record = role.id
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
        record = classMap.id
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
        record = templateMap.id
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
        record = graph.name

      };
      return graphNode;
    }
  }
}
