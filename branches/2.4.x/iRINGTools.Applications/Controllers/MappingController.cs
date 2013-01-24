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
using org.ids_adi.qmxf;
using VDS.RDF;
using System.Text;
using log4net;
using System.Text.RegularExpressions;

namespace org.iringtools.web.controllers
{
  public class MappingController : BaseController
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MappingController));
    NamespaceMapper _nsMap = new NamespaceMapper();
    private NameValueCollection _settings = null;
    private RefDataRepository _refdata = null;
    private IMappingRepository _repository { get; set; }
    private string _keyFormat = "Mapping.{0}.{1}";
    private const string unMappedToken = "[unmapped]";
    private char[] delimiters = new char[] { '/' };
    private bool qn = false;
    private string qName = string.Empty;

    public MappingController() : this(new MappingRepository()) { }

    public MappingController(IMappingRepository repository)
    {
      _settings = ConfigurationManager.AppSettings;
      _repository = repository;
      _refdata = new RefDataRepository();
      foreach(var ns in _refdata.GetFederation().Namespaces)
      {
        _nsMap.AddNamespace(ns.Prefix, new Uri(ns.Uri));
      }
    }

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

    public ActionResult AddClassMap(FormCollection form)
    {
      JsonTreeNode classMapNode = null;
      
      try
      {
        var scope = form["scope"];
        var app = form["app"];
        var graph = form["graph"];
        var objectName = form["objectName"];
        var parentClassId = form["parentClassId"];
        var templateIndex = int.Parse(form["templateIndex"]);
        var roleName = form["roleName"];
        var identifier = form["identifier"];
        var delimiter = form["delimiter"];
        var className = form["className"];
        var classId = form["classId"];

        var mapping = GetMapping(scope, app);
        var graphMap = mapping.FindGraphMap(graph);
        var ctm = graphMap.GetClassTemplateMap(parentClassId);

        if (ctm != null)
        {
          var templateMap = ctm.templateMaps[templateIndex];

          foreach (var role in templateMap.roleMaps.Where(role => role.name == roleName))
          {
            qn = _nsMap.ReduceToQName(classId, out qName);
            role.type = RoleType.Reference;
            role.dataType = qn ? qName : classId;
            role.value = className;

            var classMap = new ClassMap()
              {
                name = className,
                id = qn ? qName : classId,
                identifierDelimiter = delimiter,
                identifiers = new Identifiers()
              };

            classMap.identifiers.AddRange(identifier.Split(','));
            graphMap.AddClassMap(role, classMap);

            var context = scope + "/" + app + "/" + graphMap.name + "/" + classMap.name + "/" +
                             templateMap.name + "(" + templateIndex + ")" + role.name;

            classMapNode = CreateClassNode(context, classMap); 
              
            break;
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false } + ex.ToString(), JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true, node = classMapNode }, JsonRequestBehavior.AllowGet);
    }

    [Obsolete("Use MakeReference instead", true)]
    public JsonResult MapReference(FormCollection form)
    {
      try
      {
        var qName = string.Empty;
        var templateName = string.Empty;
        var format = String.Empty;
        var propertyCtx = form["ctx"];
        var reference = string.Empty;
        var qn = _nsMap.ReduceToQName(form["reference"], out reference);
        var classId = form["classId"];
        var label = form["label"];
        var roleName = form["roleName"];
        var roleId = form["roleId"];
        var dataObjectVars = propertyCtx.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        var scope = dataObjectVars[0];
        var application = dataObjectVars[1];
        var graph = dataObjectVars[2];
        var index = Convert.ToInt32(form["index"]);

        var mapping = GetMapping(scope, application);
        var graphMap = mapping.FindGraphMap(graph);
        var ctm = graphMap.GetClassTemplateMap(classId);
        var tMap = ctm.templateMaps[index];
        var rMap = tMap.roleMaps.Find(c => c.name == roleName);

        if (rMap != null)
        {
          rMap.type = RoleType.Reference;
          rMap.dataType = reference;
          rMap.propertyName = null;
          rMap.valueListName = null;
        }
        else
        {
          throw new Exception("Error Creating Reference Map...");
        }
      }
      catch (Exception e)
      {
        var msg = e.ToString();
        _logger.Error(msg);
        return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AddTemplateMap(FormCollection form)
    {
      JsonTreeNode nodes = new JsonTreeNode();

      try
      {
        var qName = string.Empty;

        var format = String.Empty;
        var propertyCtx = form["ctx"];
        var nodeType = form["nodetype"];
        var parentType = form["parentType"];
        var parentId = form["parentId"];
        var identifier = form["id"];

        if (string.IsNullOrEmpty(propertyCtx)) throw new Exception("ObjectName has no value");

        var dataObjectVars = propertyCtx.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        var scope = dataObjectVars[0];
        var application = dataObjectVars[1];
        var graph = dataObjectVars[2];
        var context = string.Format("{0}/{1}", scope, application);

        ClassTemplateMap selectedCtm = null;
        var mapping = GetMapping(scope, application);
        var graphMap = mapping.FindGraphMap(graph);
        var graphClassMap = graphMap.classTemplateMaps.FirstOrDefault().classMap;
        var templateQmxf = _refdata.GetTemplate(identifier);

        if (parentType == "GraphMapNode")
        {
          selectedCtm = graphMap.classTemplateMaps.Find(c => c.classMap.id.Equals(parentId));
        }
        else if (parentType == "ClassMapNode")
        {
          foreach (var classTemplateMap in graphMap.classTemplateMaps.Where(classTemplateMap => classTemplateMap.classMap.id == parentId))
          {
            selectedCtm = classTemplateMap;
            break;
          }
        }

        if (selectedCtm != null)
        {
          var selectedClassMap = selectedCtm.classMap;
          var newTemplateMap = new TemplateMap();
          object template = null;

          if (templateQmxf.templateDefinitions.Count > 0)
          {
            var templateDef = templateQmxf.templateDefinitions.FirstOrDefault();
            
              template = templateDef;
            if (templateDef != null)
            {
              newTemplateMap.id = templateDef.identifier;
              newTemplateMap.name = templateDef.name[0].value;
            }
            newTemplateMap.type = TemplateType.Definition;
              GetRoleMaps(selectedClassMap.id, template, newTemplateMap);
            
          }
          else
          {
            var templateQual = templateQmxf.templateQualifications[0];
            
              template = templateQual;
              newTemplateMap.id = templateQual.identifier;
              newTemplateMap.name = templateQual.name[0].value;
              newTemplateMap.type = TemplateType.Qualification;
              GetRoleMaps(selectedClassMap.id, template, newTemplateMap);
            
          }

          #region DO NOT DELETE THIS CODE BLOCK, PENDING FOR MODELER'S CONFIRMATION
          // duplicate templates in the same class are not allowed
          //foreach (TemplateMap templateMap in selectedCtm.templateMaps)
          //{
          //  if (templateMap.id.Substring(templateMap.id.IndexOf(":") + 1) == identifier)
          //  {
          //    foreach (RoleMap roleMap in templateMap.roleMaps)
          //    {
          //      if (roleMap.type == RoleType.Reference)
          //      {
          //        RoleMap newRoleMap = newTemplateMap.roleMaps.Find(x => x.id == roleMap.id);

          //        if (newRoleMap.value != null && newRoleMap.value.ToLower() == roleMap.value.ToLower())
          //        {
          //          throw new Exception("Duplicate templates in the same class are not allowed!");
          //        }
          //      }
          //    }
          //  }
          //}
          #endregion

          graphMap.AddTemplateMap(selectedClassMap, newTemplateMap);
        }
      }
      catch (Exception ex)
      {
        var msg = ex.ToString();
        _logger.Error(msg);
        return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
      }

      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetNode(FormCollection form)
    {
      GraphMap graphMap = null;
      ClassMap graphClassMap = null;
      var format = String.Empty;
      var context = form["node"];
      var formgraph = form["graph"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
      var variables = context.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
      var scope = variables[0];
      var application = variables[1];
      var graphName = formgraph[formgraph.Count() - 1];
      var key = string.Format(_keyFormat, scope, application);

      var mapping = GetMapping(scope, application);
      var nodes = new List<JsonTreeNode>();

      if (!string.IsNullOrEmpty(graphName))
        graphMap = mapping.graphMaps.FirstOrDefault<GraphMap>(o => o.name == graphName);

      if (graphMap != null)
      {
        var firstOrDefault = graphMap.classTemplateMaps.FirstOrDefault();
        if (firstOrDefault != null)
          graphClassMap = firstOrDefault.classMap;

        switch (form["type"])
        {
          case "MappingNode":
            nodes.AddRange(from graph in mapping.graphMaps where graphMap.name == graph.name select CreateGraphNode(context, graph, graphClassMap));

            break;

          case "GraphMapNode":
            foreach (var templateMaps in graphMap.classTemplateMaps)
            {
              if (graphClassMap != null && templateMaps.classMap.name != graphClassMap.name) continue;
              var templateIndex = 0;

              foreach (var templateMap in templateMaps.templateMaps)
              {
                var templateNode = CreateTemplateNode(context, templateMap, templateIndex);

                foreach (var role in templateMap.roleMaps)
                {
                  var roleNode = new JsonTreeNode
                    {
                      nodeType = "async",
                      type = "RoleMapNode",
                      icon = "Content/img/role-map.png",
                      id = templateNode.id + "/" + role.name,
                      text = role.IsMapped() ? string.Format("{0}{1}", role.name, "") :
                                                                                        string.Format("{0}{1}", role.name, unMappedToken),
                      expanded = false,
                      leaf = false,
                      children = null,
                      record = role,
                      properties = new Dictionary<string, string>()
                    };

                  if (role.type == RoleType.Reference)
                  {
                    // 
                    // resolve class label and store it in role value
                    //
                    var classId = role.dataType;

                    if (string.IsNullOrEmpty(classId) || !classId.StartsWith("rdl:"))
                      classId = role.value;

                    if (!string.IsNullOrEmpty(classId) && !string.IsNullOrEmpty(role.value) &&
                        role.value.StartsWith("rdl:"))
                    {
                      var classLabel = GetClassLabel(classId);
                      role.dataType = classId;
                      role.value = classLabel;
                    }
                  }

                  if (graphClassMap != null && (role.classMap != null && role.classMap.id != graphClassMap.id))
                  {
                    var classNode = CreateClassNode(context, role.classMap);

                    if (roleNode.children == null)
                      roleNode.children = new List<JsonTreeNode>();

                    roleNode.children.Add(classNode);
                  }
                  else
                  {
                    roleNode.leaf = true;
                  }

                  templateNode.children.Add(roleNode);
                }

                nodes.Add(templateNode);
                templateIndex++;
              }
            }

            break;

          case "ClassMapNode":
            var classMapId = form["id"];
            foreach (var classTemplateMap in graphMap.classTemplateMaps)
            {
              if (classTemplateMap.classMap.id != classMapId) continue;
              var templateIndex = 0;

              foreach (var templateMap in classTemplateMap.templateMaps)
              {
                var templateNode = CreateTemplateNode(context, templateMap, templateIndex);

                foreach (var role in templateMap.roleMaps)
                {
                  var roleNode = new JsonTreeNode
                    {
                      nodeType = "async",
                      type = "RoleMapNode",
                      icon = "Content/img/role-map.png",
                      id = templateNode.id + "/" + role.name,
                      text = role.IsMapped() ? string.Format("{0}{1}", role.name, "") :
                                                                                        string.Format("{0}{1}", role.name, unMappedToken),
                      expanded = false,
                      leaf = false,
                      children = null,
                      record = role,
                      properties = new Dictionary<string, string>()
                    };

                  if (role.type == RoleType.Reference)
                  {
                    // 
                    // resolve class label and store it in role value
                    //
                    var classId = role.dataType;

                    if (string.IsNullOrEmpty(classId) || !classId.StartsWith("rdl:"))
                      classId = role.value;

                    if (!string.IsNullOrEmpty(classId) && !string.IsNullOrEmpty(role.value) &&
                        role.value.StartsWith("rdl:"))
                    {
                      var classLabel = GetClassLabel(classId);
                      role.dataType = classId;
                      role.value = classLabel;
                    }
                  }

                  if (graphClassMap != null && (role.classMap != null && role.classMap.id != graphClassMap.id))
                  {
                    var classNode = CreateClassNode(context, role.classMap);
                    if (roleNode.children == null)
                      roleNode.children = new List<JsonTreeNode>();
                    roleNode.children.Add(classNode);
                  }
                  else
                  {
                    roleNode.leaf = true;
                  }

                  templateNode.children.Add(roleNode);
                }

                nodes.Add(templateNode);
                templateIndex++;
              }

              break;
            }

            break;

          case "TemplateMapNode":
            var templateId = form["id"];
            {
              if (graphClassMap != null)
              {
                var className = graphClassMap.name;
                if (variables.Count() > 4)
                {
                  className = variables[variables.Count() - 2];
                }

                var classTemplateMap =
                  graphMap.classTemplateMaps.Find(ctm => ctm.classMap.name == className);

                if (classTemplateMap == null) break;
                var templateMap =
                  classTemplateMap.templateMaps.Find(tm => tm.id == templateId);

                if (templateMap == null) break;
                foreach (var role in templateMap.roleMaps)
                {
                  var roleNode = CreateRoleNode(context, role);

                  if (role.type == RoleType.Reference)
                  {
                    // 
                    // resolve class label and store it in role value
                    //
                    var classId = role.dataType;

                    if (string.IsNullOrEmpty(classId) || !classId.StartsWith("rdl:"))
                      classId = role.value;

                    if (!string.IsNullOrEmpty(classId) && !string.IsNullOrEmpty(role.value) &&
                        role.value.StartsWith("rdl:"))
                    {
                      var classLabel = GetClassLabel(classId);
                      role.dataType = classId;
                      role.value = classLabel;
                    }
                  }

                  if (role.classMap != null && role.classMap.id != graphClassMap.id)
                  {
                    var classNode = CreateClassNode(context, role.classMap);
                    if (roleNode.children == null)
                      roleNode.children = new List<JsonTreeNode>();
                    roleNode.children.Add(classNode);
                  }
                  else
                  {
                    roleNode.leaf = true;
                  }

                  nodes.Add(roleNode);
                }
              }
            }

            break;

          case "RoleMapNode":
            break;
        }
      }

      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteClassMap(FormCollection form)
    {
      try
      {
        var mappingNode = form["mappingNode"];

        var dataObjectVars = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        var scope = dataObjectVars[0];
        var application = dataObjectVars[1];
        var graph = dataObjectVars[2];
        var classId = form["classId"];
        var parentClassId = form["parentClass"];
        var parentTemplateId = form["parentTemplate"];
        var parentRoleId = form["parentRole"];
        var index = Convert.ToInt32(form["index"]);
        var className = dataObjectVars[dataObjectVars.Count() - 1];
        var mapping = GetMapping(scope, application);
        var graphMap = mapping.FindGraphMap(graph);
        var ctm = graphMap.GetClassTemplateMap(parentClassId);
        var tMap = ctm.templateMaps[index];

        var rMap = tMap.roleMaps.Find(r => r.id.Equals(parentRoleId));
        if (rMap != null)
          graphMap.DeleteRoleMap(tMap, rMap.id);
        else
          throw new Exception("Error deleting ClassMap...");

      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false } + ex.Message, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult ResetMapping(FormCollection form)
    {
      try
      {
        var roleId = form["roleId"];
        var templateId = form["templateId"];
        var classId = form["parentClassId"];
        var mappingNode = form["mappingNode"];
        var dataObjectVars = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        var scope = dataObjectVars[0];
        var application = dataObjectVars[1];
        var graphName = dataObjectVars[2];

        var index = Convert.ToInt32(form["index"]);
        var mapping = GetMapping(scope, application);
        var graphMap = mapping.FindGraphMap(graphName);
        var ctm = graphMap.GetClassTemplateMap(classId);
        var tMap = ctm.templateMaps[index];
        var rMap = tMap.roleMaps.Find(r => r.id.Equals(roleId));

        if (rMap.classMap != null)
        {
          graphMap.DeleteRoleMap(tMap, rMap.id);
        }

        if (rMap.dataType.StartsWith("xsd:"))
        {
          rMap.type = RoleType.DataProperty;
          rMap.propertyName = string.Empty;
        }
        else
        {
          rMap.type = RoleType.Unknown;
          rMap.propertyName = null;
        }

        rMap.value = null;
        rMap.valueListName = null;
        rMap.classMap = null;
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult MakePossessor(FormCollection form)
    {
      var nodes = new List<JsonTreeNode>();

      try
      {
        var mappingNode = form["mappingNode"];

        object selectedNode = form["node"];
        var index = Convert.ToInt32(form["index"]);

        var dataObjectVars = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        var scope = dataObjectVars[0];
        var application = dataObjectVars[1];
        var graphName = dataObjectVars[2];

        var classId = form["classId"];
        var roleName = dataObjectVars[dataObjectVars.Length - 1];
        var context = string.Format("{0}/{1}/{2}/{3}", scope, application, dataObjectVars[2], dataObjectVars[dataObjectVars.Count() - 2]);
        var mapping = GetMapping(scope, application);
        var graphMap = mapping.FindGraphMap(graphName);
        var ctm = graphMap.GetClassTemplateMap(classId);
        var tMap = ctm.templateMaps[index];
        var rMap = tMap.roleMaps.FirstOrDefault(c => c.name == roleName);

        if (rMap != null)
        {
          rMap.type = RoleType.Possessor;
          rMap.propertyName = null;
          rMap.valueListName = null;
          rMap.value = null;
          var roleNode = CreateRoleNode(context, rMap);
          roleNode.text.Replace(unMappedToken, "");
          nodes.Add(roleNode);
        }
        else
        {
          throw new Exception("Error Making Possessor Role...");
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(nodes, JsonRequestBehavior.AllowGet);
      }

      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    private JsonTreeNode CreateGraphNode(string context, GraphMap graph, ClassMap classMap)
    {
      var graphNode = new JsonTreeNode
      {
        nodeType = "async",
        identifier = classMap.id,
        type = "GraphMapNode",
        icon = "Content/img/graph-map.png",
        id = context + "/" + graph.name + "/" + classMap.name,
        text = graph.name,
        expanded = false,
        leaf = false,
        children = null,
        record = graph
      };

      return graphNode;
    }

    private JsonTreeNode CreateClassNode(string context, ClassMap classMap)
    {
      var classNode = new JsonTreeNode
      {
        identifier = classMap.id,
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

    private JsonTreeNode CreateTemplateNode(string context, TemplateMap templateMap, int index)
    {
      if (!templateMap.id.Contains(":"))
        templateMap.id = string.Format("tpl:{0}", templateMap.id);

      var templateNode = new JsonTreeNode
      {
        nodeType = "async",
        identifier = templateMap.id,
        type = "TemplateMapNode",
        icon = "Content/img/template-map.png",
        id = context + "/" + templateMap.name + "(" + index + ")",
        text = templateMap.name,
        expanded = false,
        leaf = false,
        children = new List<JsonTreeNode>(),
        record = templateMap
      };

      return templateNode;
    }

    private JsonTreeNode CreateRoleNode(string context, RoleMap role)
    {
      var roleNode = new JsonTreeNode
      {
        nodeType = "async",
        type = "RoleMapNode",
        icon = "Content/img/role-map.png",
        id = context + "/" + role.name,
        text = role.IsMapped() ? string.Format("{0}{1}", role.name, "") :
                                 string.Format("{0}{1}", role.name, unMappedToken),
        expanded = false,
        leaf = false,
        children = null,
        record = role,
        properties = new Dictionary<string, string>()
      };

      return roleNode;
    }

    public ActionResult GraphMap(FormCollection form)
    {
      var nodes = new List<JsonTreeNode>();

      try
      {
        var scope = form["scope"];
        var app = form["app"];
        var oldGraphName = form["oldGraphName"];
        var graphName = form["graphName"];
        var objectName = form["objectName"];
        var identifier = form["identifier"];
        var delimiter = form["delimiter"];
        var className = form["className"];
        var classId = form["classId"];

        var context = string.Format("{0}/{1}", scope, app);
        var mapping = GetMapping(scope, app);

        var qn = false;
        qn = _nsMap.ReduceToQName(classId, out qName);

        if (string.IsNullOrEmpty(oldGraphName))
        {
          if (mapping.graphMaps == null)
            mapping.graphMaps = new GraphMaps();

          var graphMap = new GraphMap
          {
            name = graphName,
            dataObjectName = objectName
          };

          var classMap = new ClassMap
          {
            name = className,
            id = qn ? qName : classId,
            identifierDelimiter = delimiter,
            identifiers = new Identifiers()
          };

          if (identifier.Contains(','))
          {
            var identifierParts = identifier.Split(',');

            foreach (var part in identifierParts)
            {
              classMap.identifiers.Add(part);
            }
          }
          else
          {
            classMap.identifiers.Add(identifier);
          }

          graphMap.AddClassMap(null, classMap);
          mapping.graphMaps.Add(graphMap);
          nodes.Add(CreateGraphNode(context, graphMap, classMap));
        }
        else
        {
          var graphMap = mapping.FindGraphMap(graphName) ?? new GraphMap();

          graphMap.name = graphName;
          graphMap.dataObjectName = objectName;
          
          var classMap = graphMap.classTemplateMaps[0].classMap;
          classMap.name = className;
          classMap.id = qn ? qName : classId;
          classMap.identifierDelimiter = delimiter;
          classMap.identifiers = new Identifiers();

          if (identifier.Contains(','))
          {
            var identifierParts = identifier.Split(',');

            foreach (var part in identifierParts)
            {
              classMap.identifiers.Add(part);
            }
          }
          else
          {
            classMap.identifiers.Add(identifier);
          }
        }

        _repository.UpdateMapping(scope, app, mapping);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(nodes, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult UpdateMapping(FormCollection form)
    {
      var scope = form["scope"];
      var application = form["application"];
      var mapping = GetMapping(scope, application);
      try
      {
        _repository.UpdateMapping(scope, application, mapping);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteGraphMap(FormCollection form)
    {
      try
      {
        var scope = form["scope"];
        var application = form["application"];
        var mapping = GetMapping(scope, application);
        var graphName = form["mappingNode"].Split('/')[4];
        var graphMap = mapping.FindGraphMap(graphName);

        if (graphMap != null)
        {
          mapping.graphMaps.Remove(graphMap);
          _repository.UpdateMapping(scope, application, mapping);
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult MapProperty(FormCollection form)
    {
      try
      {
        var mappingNode = form["mappingNode"];
        var propertyName = form["propertyName"];
        var mappingCtx = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        var scope = mappingCtx[0];
        var application = mappingCtx[1];
        var graphName = mappingCtx[2];

        var classId = form["classId"];
        var relatedObject = form["relatedObject"];
        var roleName = mappingCtx[mappingCtx.Length - 1];
        int index = Convert.ToInt16(form["index"]);
        var mapping = GetMapping(scope, application);
        var graphMap = mapping.FindGraphMap(graphName);
        var ctMap = graphMap.GetClassTemplateMap(classId);

        if (ctMap != null)
        {
          var tMap = ctMap.templateMaps[index];
          var rMap = tMap.roleMaps.Find(r => r.name.Equals(roleName));

          if (!string.IsNullOrEmpty(rMap.dataType) && rMap.dataType.StartsWith("xsd"))
          {
            if (relatedObject != "undefined" && relatedObject != "")
            {
              rMap.propertyName = string.Format("{0}.{1}.{2}",
                graphMap.dataObjectName,
                relatedObject,
                propertyName);
            }
            else
            {
              rMap.propertyName =
                  string.Format("{0}.{1}", graphMap.dataObjectName, propertyName);
            }

            rMap.type = RoleType.DataProperty;
            rMap.valueListName = null;
          }
          else
          {
            throw new Exception("Invalid property map.");
          }
        }
      }
      catch (Exception ex)
      {
        var msg = ex.ToString();
        _logger.Error(msg);
        return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult MakeReference(FormCollection form)
    {
      try
      {
        var mappingNode = form["mappingNode"];
        var scope = form["scope"];
        var app = form["app"];
        var graph = form["graph"];
        var classId = form["classId"];
        var roleName = form["roleName"];
        int index = Convert.ToInt16(form["index"]);

        var mapping = GetMapping(scope, app);
        var graphMap = mapping.FindGraphMap(graph);
        var ctm = graphMap.GetClassTemplateMap(classId);
        var tMap = ctm.templateMaps[index];
        var rMap = tMap.roleMaps.Find(c => c.name == roleName);

        if (rMap != null)
        {
          rMap.type = RoleType.Reference;
          rMap.value = GetClassLabel(rMap.dataType);
          rMap.propertyName = null;
          rMap.valueListName = null;
        }
        else
        {
          throw new Exception("Error creating Reference RoleMap...");
        }
      }
      catch (Exception ex)
      {
        var msg = ex.ToString();
        _logger.Error(msg);
        return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult MapValueList(FormCollection form)
    {
      try
      {
        var mappingNode = form["mappingNode"];
        var propertyName = form["propertyName"];
        var objectNames = form["objectNames"];
        var propertyCtx = objectNames.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        var mappingCtx = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        var scope = propertyCtx[0];
        var classId = form["classId"];
        var application = propertyCtx[1];
        var graphName = mappingCtx[2];

        var roleName = mappingCtx[mappingCtx.Length - 1];
        var valueListName = propertyCtx[propertyCtx.Length - 1];
        int index = Convert.ToInt16(form["index"]);

        var mapping = GetMapping(scope, application);
        var graphMap = mapping.FindGraphMap(graphName);
        var ctm = graphMap.GetClassTemplateMap(classId);

        if (ctm != null)
        {
          var tMap = ctm.templateMaps[index];
          var rMap = tMap.roleMaps.Find(rm => rm.name.Equals(roleName));

          if (rMap != null)
          {
            rMap.valueListName = valueListName;
            rMap.propertyName = string.Format("{0}.{1}", propertyName.Split(delimiters)[4], propertyName.Split(delimiters)[5]);
            rMap.type = RoleType.ObjectProperty;
            rMap.value = null;
          }
          else
          {
            throw new Exception("Error mapping ValueList...");
          }
        }
      }
      catch (Exception ex)
      {
        string msg = ex.ToString();
        _logger.Error(msg);
        return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteTemplateMap(FormCollection form)
    {
      try
      {
        var scope = form["scope"];
        var application = form["application"];
        var parentNode = form["mappingNode"].Split('/')[2];
        var templateId = form["identifier"];
        var parentClassId = form["parentIdentifier"];
        int index = Convert.ToInt16(form["index"]);

        var mapping = GetMapping(scope, application);
        var graphMap = mapping.FindGraphMap(parentNode);
        var ctm = graphMap.GetClassTemplateMap(parentClassId);
        ctm.templateMaps.RemoveAt(index);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteValueList(FormCollection form)
    {
      try
      {
        var context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        var scope = context[0];
        var application = context[1];
        var mapping = GetMapping(scope, application);
        var deleteValueList = form["valueList"];
        var valueListMap = mapping.valueListMaps.Find(c => c.name == deleteValueList);
        if (valueListMap != null)
          mapping.valueListMaps.Remove(valueListMap);
        _repository.UpdateMapping(scope, application, mapping);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult ValueListMap(FormCollection form)
    {
      try
      {
        var context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        var scope = context[0];
        var valueList = context[4];
        var application = context[1];
        var oldClassUrl = form["oldClassUrl"];
        var internalName = form["internalName"];
        var classUrl = form["classUrl"];
        var classLabel = form["classLabel"];

        var classUrlUsesPrefix = false;

        if (!String.IsNullOrEmpty(classUrl))
        {
          if (_nsMap.Prefixes.Any(prefix => classUrl.ToLower().StartsWith(prefix + ":")))
          {
            classUrlUsesPrefix = true;
            qName = classUrl;
          }

          if (!classUrlUsesPrefix)
          {
            qn = _nsMap.ReduceToQName(classUrl, out qName);
          }
        }

        var mapping = GetMapping(scope, application);
        ValueListMap valuelistMap = null;

        if (mapping.valueListMaps != null)
          valuelistMap = mapping.valueListMaps.Find(c => c.name == valueList);

        if (oldClassUrl == "")
        {
          var valueMap = new ValueMap
          {
            internalValue = internalName,
            uri = qName,
            label = classLabel
          };
          if (valuelistMap != null && valuelistMap.valueMaps == null)
            valuelistMap.valueMaps = new ValueMaps();
          if (valuelistMap != null) valuelistMap.valueMaps.Add(valueMap);
          _repository.UpdateMapping(scope, application, mapping);
        }
        else
        {
          if (valuelistMap != null)
          {
            var valueMap = valuelistMap.valueMaps.Find(c => c.uri.Equals(oldClassUrl));
            if (valueMap != null)
            {
              valueMap.internalValue = internalName;
              valueMap.uri = qName;
              valueMap.label = classLabel;
              _repository.UpdateMapping(scope, application, mapping);
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteValueMap(FormCollection form)
    {
      try
      {
        var context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        var scope = context[0];
        var valueList = context[4];
        var application = context[1];
        var oldClassUrl = form["oldClassUrl"];
        var mapping = GetMapping(scope, application);
        ValueListMap valuelistMap = null;

        if (mapping.valueListMaps != null)
          valuelistMap = mapping.valueListMaps.Find(c => c.name == valueList);

        if (valuelistMap != null)
        {
          var valueMap = valuelistMap.valueMaps.Find(c => c.uri.Equals(oldClassUrl));
          if (valueMap != null)
            valuelistMap.valueMaps.Remove(valueMap);
        }
        _repository.UpdateMapping(scope, application, mapping);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult CopyValueLists(string targetScope, string targetApplication, string sourceScope, string sourceApplication, string valueList)
    {
        try
        {
            Mapping sourceMapping = GetMapping(sourceScope, sourceApplication);
            Mapping targetMapping = GetMapping(targetScope, targetApplication);
            ValueListMap valuelistMap = null;
            // copy complete valueList having multiples valueList items.
            if (sourceMapping.valueListMaps != null)
            {
                if (valueList != "" && valueList == "ValueListToValueList")
                {
                    //foreach(ValueListMap valueListmap in sourceMapping.valueListMaps){
                    //targetMapping.valueListMaps.Add(Utility.CloneSerializableObject(valueListmap));
                    //}
                    targetMapping.valueListMaps = Utility.CloneSerializableObject(sourceMapping.valueListMaps);

                }
                else
                { // copy single valueList.
                    if (valueList != "")
                        valuelistMap = sourceMapping.valueListMaps.Find(c => c.name == valueList);
                    targetMapping.valueListMaps.Add(Utility.CloneSerializableObject(valuelistMap));
                }
            }
            _repository.UpdateMapping(targetScope, targetApplication, targetMapping);
        }
        catch (Exception ex)
        {
            _logger.Error(ex.ToString());
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }
		
    public ActionResult ValueList(FormCollection form)
    {
      try
      {
        var oldValueList = "";
        ValueListMap valueListMap = null;
        var context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        var scope = context[0];

        if (context.Any())
          oldValueList = context[context.Count() - 1];

        var application = context[1];
        var mapping = GetMapping(scope, application);
        var newvalueList = form["valueList"];

        if (mapping.valueListMaps != null)
        {
          valueListMap = oldValueList != "" ? mapping.valueListMaps.Find(c => c.name == oldValueList) : mapping.valueListMaps.Find(c => c.name == newvalueList);
        }
        if (valueListMap == null)
        {
          var valuelistMap = new ValueListMap
          {
            name = newvalueList
          };

          if (mapping.valueListMaps != null) mapping.valueListMaps.Add(valuelistMap);
          _repository.UpdateMapping(scope, application, mapping);
        }
        else
        {
          valueListMap.name = newvalueList;
          _repository.UpdateMapping(scope, application, mapping);
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public string GetClassLabel(string classId)
    {
      var classLabel = String.Empty;

      if (!String.IsNullOrEmpty(classId))
      {
        if (classId.Contains(":"))
          classId = classId.Substring(classId.IndexOf(":", System.StringComparison.Ordinal) + 1);

        var key = "class-label-" + classId;

        if (Session[key] != null)
        {
          return (string)Session[key];
        }

        try
        {
          var entity = _refdata.GetClassLabel(classId);

          classLabel = entity.Label;
          Session[key] = classLabel;
        }
        catch (Exception ex)
        {
          _logger.Error("Error getting class label for class id [" + classId + "]: " + ex);
          throw ex;
        }
      }

      return classLabel;
    }

    public JsonResult GetLabels(FormCollection form)
    {
      var jsonArray = new JsonArray();

      try
      {
        var scope = form["Scope"];
        var application = form["Application"];
        var recordId = form["recordId"];
        var roleType = form["roleType"];
        var roleValue = form["roleValue"];

        if (!string.IsNullOrEmpty(recordId))
        {
          //   jsonArray.Add( "recordId", _refdata.GetClassLabel(recordId.Split(':')[1]));
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(jsonArray, JsonRequestBehavior.AllowGet);
      }

      return Json(jsonArray, JsonRequestBehavior.AllowGet);
    }

    private void GetRoleMaps(string classId, object template, TemplateMap currentTemplateMap)
    {
      string qRange;
      string qId;

      if (currentTemplateMap.roleMaps == null)
        currentTemplateMap.roleMaps = new RoleMaps();

      var templateDefinition = template as TemplateDefinition;
      if (templateDefinition != null)
      {
        var roleDefinitions = templateDefinition.roleDefinition;

        foreach (var roleDefinition in roleDefinitions)
        {
          var range = roleDefinition.range;
          qn = _nsMap.ReduceToQName(range, out qRange);

          var id = roleDefinition.identifier;
          qn = _nsMap.ReduceToQName(id, out qId);

          var firstOrDefault = roleDefinition.name.FirstOrDefault();
          if (firstOrDefault != null)
          {
            var roleMap = new RoleMap()
              {
                name = firstOrDefault.value,
                id = qId
              };

            if (qRange == classId)    // possessor role
            {
              roleMap.type = RoleType.Possessor;
              roleMap.dataType = qRange;
            }
            else if (qRange.StartsWith("xsd:"))  // data property role
            {
              roleMap.type = RoleType.DataProperty;
              roleMap.dataType = qRange;
              roleMap.propertyName = string.Empty;
            }
            else if (!qRange.StartsWith("dm:"))  // reference role
            {
              roleMap.type = RoleType.Reference;
              roleMap.dataType = qRange;
              roleMap.value = GetClassLabel(qRange);
            }
            else  // unknown
            {
              roleMap.type = RoleType.Unknown;
              roleMap.dataType = qRange;
            }

            currentTemplateMap.roleMaps.Add(roleMap);
          }
        }
      }

      if (template is TemplateQualification)
      {
        var templateQualification = (TemplateQualification)template;
        var roleQualifications = templateQualification.roleQualification;

        foreach (var roleQualification in roleQualifications)
        {
          var range = roleQualification.range;
          qn = _nsMap.ReduceToQName(range, out qRange);

          var id = roleQualification.qualifies;
          qn = _nsMap.ReduceToQName(id, out qId);

          if (currentTemplateMap.roleMaps.Find(x => x.id == qId) != null) continue;
          var firstOrDefault = roleQualification.name.FirstOrDefault();
          if (firstOrDefault == null) continue;
          var roleMap = new RoleMap()
            {
              name = firstOrDefault.value,
              id = qId
            };

          if (roleQualification.value != null)  // fixed role
          {
            if (!String.IsNullOrEmpty(roleQualification.value.reference))
            {
              roleMap.type = RoleType.Reference;
              qn = _nsMap.ReduceToQName(roleQualification.value.reference, out qRange);
              roleMap.dataType = qn ? qRange : roleQualification.value.reference;
            }
            else if (!String.IsNullOrEmpty(roleQualification.value.text))  // fixed role is a literal
            {
              roleMap.type = RoleType.FixedValue;
              roleMap.value = roleQualification.value.text;
              roleMap.dataType = roleQualification.value.As;
            }
          }
          else if (qRange == classId)    // possessor role
          {
            roleMap.type = RoleType.Possessor;
            roleMap.dataType = qRange;
          }
          else if (qRange.StartsWith("xsd:"))  // data property role
          {
            roleMap.type = RoleType.DataProperty;
            roleMap.dataType = qRange;
            roleMap.propertyName = string.Empty;
          }
          else if (!qRange.StartsWith("dm:"))  // reference role
          {
            roleMap.type = RoleType.Reference;
            roleMap.dataType = qRange;
            roleMap.value = GetClassLabel(qRange);
          }
          else  // unknown
          {
            roleMap.type = RoleType.Unknown;
            roleMap.dataType = qRange;
          }

          currentTemplateMap.roleMaps.Add(roleMap);
        }
      }
    }

    public ActionResult Export(string scope, string application, string graphMap)
    {
      //string scope = form["scope"];
      //string application = form["application"];
      //string graphMap = form["graphMap"];

      var mapping = GetMapping(scope, application);
      Mapping export;
      if (!string.IsNullOrEmpty(graphMap))
      {
        export = new Mapping
          {graphMaps = new GraphMaps {mapping.FindGraphMap(graphMap)}, valueListMaps = mapping.valueListMaps};
      }
      else
      {
        export = mapping;
      }

      var content = Utility.SerializeDataContract<Mapping>(export);

      return File(Encoding.UTF8.GetBytes(content), "application/xml", string.Format("Mapping.{0}.{1}.xml", scope, application));
    }

    public ActionResult Import(FormCollection form)
    {
      var mapping = form["mapping"].DeserializeDataContract<Mapping>();

      return Json(new { success = false }, JsonRequestBehavior.AllowGet);
    }
  }
}
