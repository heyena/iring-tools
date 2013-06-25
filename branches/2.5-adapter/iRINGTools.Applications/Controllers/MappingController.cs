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
  public class MappingController : Controller
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
      _nsMap.AddNamespace("eg", new Uri("http://example.org/data#"));
      _nsMap.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
      _nsMap.AddNamespace("rdl", new Uri("http://rdl.rdlfacade.org/data#"));
      _nsMap.AddNamespace("tpl", new Uri("http://tpl.rdlfacade.org/data#"));
      _nsMap.AddNamespace("dm", new Uri("http://dm.rdlfacade.org/data#"));
      _nsMap.AddNamespace("p8dm", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/data-model#"));
      _nsMap.AddNamespace("owl2xml", new Uri("http://www.w3.org/2006/12/owl2-xml#"));
      _nsMap.AddNamespace("p8", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/template-model#"));
      _nsMap.AddNamespace("templates", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/templates#"));
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
        string scope = form["scope"];
        string app = form["app"];
        string graph = form["graph"];
        string objectName = form["objectName"];
        string parentClassId = form["parentClassId"];
        int templateIndex = int.Parse(form["templateIndex"]);
        string roleName = form["roleName"];
        string identifier = form["identifier"];
        string delimiter = form["delimeter"];//form["delimiter"];
        string className = form["className"];
        string classId = form["classId"];

        Mapping mapping = GetMapping(scope, app);
        GraphMap graphMap = mapping.FindGraphMap(graph);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(parentClassId);

        if (ctm != null)
        {
          TemplateMap templateMap = ctm.templateMaps[templateIndex];

          foreach (var role in templateMap.roleMaps)
          {
            if (role.name == roleName)
            {
              qn = _nsMap.ReduceToQName(classId, out qName);
              role.type = RoleType.Reference;
              role.dataType = qn ? qName : classId;
              role.value = className;

              ClassMap classMap = new ClassMap()
              {
                name = className,
                id = qn ? qName : classId,
                identifierDelimiter = delimiter,
                identifiers = new Identifiers()
              };

              classMap.identifiers.AddRange(identifier.Split(','));
              graphMap.AddClassMap(role, classMap);

              string context = scope + "/" + app + "/" + graphMap.name + "/" + classMap.name + "/" +
                templateMap.name + "(" + templateIndex + ")" + role.name;

              classMapNode = CreateClassNode(context, classMap); 
              
              break;
            }
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
    
    public JsonResult AddTemplateMap(FormCollection form)
    {
      JsonTreeNode nodes = new JsonTreeNode();

      try
      {
        string qName = string.Empty;

        string format = String.Empty;
        string propertyCtx = form["ctx"];
        string nodeType = form["nodetype"];
        string parentType = form["parentType"];
        string parentId = form["parentId"];
        string identifier = form["id"];

        if (string.IsNullOrEmpty(propertyCtx)) throw new Exception("ObjectName has no value");

        string[] dataObjectVars = propertyCtx.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = dataObjectVars[0];
        string application = dataObjectVars[1];
        string graph = form["graphName"];//dataObjectVars[2];
        string context = string.Format("{0}/{1}", scope, application);

        ClassTemplateMap selectedCtm = null;
        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graph);
        ClassMap graphClassMap = graphMap.classTemplateMaps.FirstOrDefault().classMap;
        QMXF templateQmxf = _refdata.GetTemplate(identifier);

        if (parentType == "GraphMapNode")
        {
          selectedCtm = graphMap.classTemplateMaps.Find(c => c.classMap.id.Equals(parentId));
        }
        else if (parentType == "ClassMapNode")
        {
          foreach (var classTemplateMap in graphMap.classTemplateMaps)
          {
            if (classTemplateMap.classMap.id == parentId)
            {
              selectedCtm = classTemplateMap;
              break;
            }
          }
        }

        if (selectedCtm != null)
        {
          ClassMap selectedClassMap = selectedCtm.classMap;
          TemplateMap newTemplateMap = new TemplateMap();
          object template = null;

          if (templateQmxf.templateDefinitions.Count > 0)
          {
            foreach (var templateDef in templateQmxf.templateDefinitions)
            {
              template = templateDef;
              newTemplateMap.id = templateDef.identifier;
              newTemplateMap.name = templateDef.name[0].value;
              newTemplateMap.type = TemplateType.Definition;
              GetRoleMaps(selectedClassMap.id, template, newTemplateMap);
            }
          }
          else
          {
            foreach (var templateQual in templateQmxf.templateQualifications)
            {
              template = templateQual;
              newTemplateMap.id = templateQual.identifier;
              newTemplateMap.name = templateQual.name[0].value;
              newTemplateMap.type = TemplateType.Qualification;
              GetRoleMaps(selectedClassMap.id, template, newTemplateMap);
            }
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
        string msg = ex.ToString();
        _logger.Error(msg);
        return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
      }

      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetNode(FormCollection form)
    {
      GraphMap graphMap = null;
      ClassMap graphClassMap = null;
      string format = String.Empty;
      string context;
      //string context = form["node"];
      if (form["tempNode"]!=null && form["tempNode"]!="" )
          context = form["tempNode"];
      else
          context = form["node"];
      string[] formgraph = form["graph"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
      string[] variables = context.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
      string scope = variables[0];
      string application = variables[1];
      string graphName = formgraph[formgraph.Count() - 1];
      string key = string.Format(_keyFormat, scope, application);

      Mapping mapping = GetMapping(scope, application);
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      if (!string.IsNullOrEmpty(graphName))
        graphMap = mapping.graphMaps.FirstOrDefault<GraphMap>(o => o.name == graphName);

      if (graphMap != null)
      {
        graphClassMap = graphMap.classTemplateMaps.FirstOrDefault().classMap;

        switch (form["type"])
        {
          case "MappingNode":
            foreach (var graph in mapping.graphMaps)
            {
              if (graphMap != null && graphMap.name != graph.name) continue;
              JsonTreeNode graphNode = CreateGraphNode(context, graph, graphClassMap);
              nodes.Add(graphNode);
            }

            break;

          case "GraphMapNode":
            if (graphMap != null)
            {
              foreach (var templateMaps in graphMap.classTemplateMaps)
              {
                if (templateMaps.classMap.name != graphClassMap.name) continue;
                int templateIndex = 0;

                foreach (var templateMap in templateMaps.templateMaps)
                {
                  JsonTreeNode templateNode = CreateTemplateNode(context, templateMap, templateIndex);
                  JsonTreeNode roleNode = new JsonTreeNode();

                  foreach (var role in templateMap.roleMaps)
                  {
                    roleNode = new JsonTreeNode
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
                      string classId = role.dataType;

                      if (string.IsNullOrEmpty(classId) || !classId.StartsWith("rdl:"))
                        classId = role.value;

                      if (!string.IsNullOrEmpty(classId) && !string.IsNullOrEmpty(role.value) &&
                        role.value.StartsWith("rdl:"))
                      {
                        string classLabel = GetClassLabel(classId);
                        role.dataType = classId;
                        role.value = classLabel;
                      }
                    }

                    if (role.classMap != null && role.classMap.id != graphClassMap.id)
                    {
                      JsonTreeNode classNode = CreateClassNode(context, role.classMap);

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
            }

            break;

          case "ClassMapNode":
            var classMapId = form["id"];
            if (graphMap != null)
            {
              foreach (var classTemplateMap in graphMap.classTemplateMaps)
              {
                if (classTemplateMap.classMap.id == classMapId)
                {
                  int templateIndex = 0;

                  foreach (var templateMap in classTemplateMap.templateMaps)
                  {
                    JsonTreeNode templateNode = CreateTemplateNode(context, templateMap, templateIndex);
                    JsonTreeNode roleNode = new JsonTreeNode();

                    foreach (var role in templateMap.roleMaps)
                    {
                      roleNode = new JsonTreeNode
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
                        string classId = role.dataType;

                        if (string.IsNullOrEmpty(classId) || !classId.StartsWith("rdl:"))
                          classId = role.value;

                        if (!string.IsNullOrEmpty(classId) && !string.IsNullOrEmpty(role.value) &&
                          role.value.StartsWith("rdl:"))
                        {
                          string classLabel = GetClassLabel(classId);
                          role.dataType = classId;
                          role.value = classLabel;
                        }
                      }

                      if (role.classMap != null && role.classMap.id != graphClassMap.id)
                      {
                        JsonTreeNode classNode = CreateClassNode(context, role.classMap);
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
              }
            }

            break;

          case "TemplateMapNode":
            var templateId = form["id"];
            if (graphMap != null)
            {
              string className = graphClassMap.name;
              if (variables.Count() > 4)
              {
                className = variables[variables.Count() - 2];
              }

              ClassTemplateMap classTemplateMap =
                graphMap.classTemplateMaps.Find(ctm => ctm.classMap.name == className);

              if (classTemplateMap == null) break;
              TemplateMap templateMap =
                classTemplateMap.templateMaps.Find(tm => tm.id == templateId);

              if (templateMap == null) break;
              foreach (var role in templateMap.roleMaps)
              {
                JsonTreeNode roleNode = CreateRoleNode(context, role);

                if (role.type == RoleType.Reference)
                {
                  // 
                  // resolve class label and store it in role value
                  //
                  string classId = role.dataType;

                  if (string.IsNullOrEmpty(classId) || !classId.StartsWith("rdl:"))
                    classId = role.value;

                  if (!string.IsNullOrEmpty(classId) && !string.IsNullOrEmpty(role.value) &&
                    role.value.StartsWith("rdl:"))
                  {
                    string classLabel = GetClassLabel(classId);
                    role.dataType = classId;
                    role.value = classLabel;
                  }
                }

                if (role.classMap != null && role.classMap.id != graphClassMap.id)
                {
                  JsonTreeNode classNode = CreateClassNode(context, role.classMap);
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
        string mappingNode = form["mappingNode"];

        string[] dataObjectVars = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = dataObjectVars[0];
        string application = dataObjectVars[1];
        string graph = dataObjectVars[2];
        string classId = form["classId"];
        string parentClassId = form["parentClass"];
        string parentTemplateId = form["parentTemplate"];
        string parentRoleId = form["parentRole"];
        int index = Convert.ToInt32(form["index"]);
        string className = dataObjectVars[dataObjectVars.Count() - 1];
        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graph);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(parentClassId);
        TemplateMap tMap = ctm.templateMaps[index];

        RoleMap rMap = tMap.roleMaps.Find(r => r.id.Equals(parentRoleId));
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
        string roleId = form["roleId"];
        string templateId = form["templateId"];
        string classId = form["parentClassId"];
        string mappingNode = form["mappingNode"];
        string[] dataObjectVars = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = dataObjectVars[0];
        string application = dataObjectVars[1];
        string graphName = dataObjectVars[2];

        int index = Convert.ToInt32(form["index"]);
        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graphName);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId);
        TemplateMap tMap = ctm.templateMaps[index];
        RoleMap rMap = tMap.roleMaps.Find(r => r.id.Equals(roleId));

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
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      try
      {
        string mappingNode = form["mappingNode"];

        object selectedNode = form["node"];
        int index = Convert.ToInt32(form["index"]);

        string[] dataObjectVars = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = dataObjectVars[0];
        string application = dataObjectVars[1];
        string graphName = dataObjectVars[2];

        string classId = form["classId"];
        string roleName = dataObjectVars[dataObjectVars.Length - 1];
        string context = string.Format("{0}/{1}/{2}/{3}", scope, application, dataObjectVars[2], dataObjectVars[dataObjectVars.Count() - 2]);
        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graphName);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId);
        TemplateMap tMap = ctm.templateMaps[index];
        RoleMap rMap = tMap.roleMaps.FirstOrDefault(c => c.name == roleName);

        if (rMap != null)
        {
          rMap.type = RoleType.Possessor;
          rMap.propertyName = null;
          rMap.valueListName = null;
          rMap.value = null;
          JsonTreeNode roleNode = CreateRoleNode(context, rMap);
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
      JsonTreeNode graphNode = new JsonTreeNode
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
      JsonTreeNode classNode = new JsonTreeNode
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

      JsonTreeNode templateNode = new JsonTreeNode
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
      JsonTreeNode roleNode = new JsonTreeNode
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
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      try
      {
        string scope = form["scope"];
        string app = form["app"];
        string oldGraphName = form["oldGraphName"];
        string graphName = form["graphName"];
        string objectName = form["objectName"];
        string identifier = form["identifier"];
        string delimiter = form["delimiter"];
        string className = form["className"];
        string classId = form["classId"];

        string context = string.Format("{0}/{1}", scope, app);
        Mapping mapping = GetMapping(scope, app);

        bool qn = false;
        qn = _nsMap.ReduceToQName(classId, out qName);

        if (string.IsNullOrEmpty(oldGraphName))
        {
          if (mapping.graphMaps == null)
            mapping.graphMaps = new GraphMaps();

          GraphMap graphMap = new GraphMap
            {
              name = graphName,
              dataObjectName = objectName
            };

          ClassMap classMap = new ClassMap
            {
              name = className,
              id = qn ? qName : classId,
              identifierDelimiter = delimiter,
              identifiers = new Identifiers()
            };

          if (identifier.Contains(','))
          {
            string[] identifierParts = identifier.Split(',');

            foreach (string part in identifierParts)
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
        else // Edit existing graph
        {
          GraphMap graphMap = mapping.FindGraphMap(oldGraphName);//Get graph with old name

          if (graphMap == null)
            graphMap = new GraphMap();

          graphMap.name = graphName;
          graphMap.dataObjectName = objectName;

          ClassMap classMap = graphMap.classTemplateMaps[0].classMap;
          classMap.name = className;
          classMap.id = qn ? qName : classId;
          classMap.identifierDelimiter = delimiter;
          classMap.identifiers = new Identifiers();

          if (identifier.Contains(','))
          {
            string[] identifierParts = identifier.Split(',');

            foreach (string part in identifierParts)
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

      return Json(new {success = true}, JsonRequestBehavior.AllowGet);
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
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteGraphMap(FormCollection form)
    {
      try
      {
        string scope = form["scope"];
        string application = form["application"];
        Mapping mapping = GetMapping(scope, application);
        string graphName = form["mappingNode"].Split('/')[4];
        GraphMap graphMap = mapping.FindGraphMap(graphName);

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
        string mappingNode = form["mappingNode"];
        string propertyName = form["propertyName"];
        string[] mappingCtx = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = mappingCtx[0];
        string application = mappingCtx[1];
        string graphName = mappingCtx[2];

        string classId = form["classId"];
        string[] classCtx = classId.Split(',');
        classId = classCtx[0];
        string roleName = mappingCtx[mappingCtx.Length - 1];
        int index = Convert.ToInt16(form["index"]);
        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graphName);
        ClassTemplateMap ctMap = graphMap.GetClassTemplateMap(classId);

        if (ctMap != null)
        {
          TemplateMap tMap = ctMap.templateMaps[index];
          RoleMap rMap = tMap.roleMaps.Find(r => r.name.Equals(roleName));

          if (!string.IsNullOrEmpty(rMap.dataType) && rMap.dataType.StartsWith("xsd"))
          {
            rMap.propertyName = propertyName;
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
        string msg = ex.ToString();
        _logger.Error(msg);
        return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult MakeReference(FormCollection form)
    {
      try
      {
        string scope = form["scope"];
        string app = form["app"];
        string graph = form["graph"];
        string classId = form["classId"];
        int templateIndex = Convert.ToInt16(form["templateIndex"]);
        string roleName = form["roleName"];
        string refClassId = form["refClassId"];
        string refClassLabel = form["refClassLabel"];   

        Mapping mapping = GetMapping(scope, app);
        GraphMap graphMap = mapping.FindGraphMap(graph);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId);
        TemplateMap tMap = ctm.templateMaps[templateIndex];
        RoleMap rMap = tMap.roleMaps.Find(c => c.name == roleName);

        if (rMap != null)
        {
          rMap.type = RoleType.Reference;

          if (string.IsNullOrEmpty(refClassLabel))
          {
            rMap.value = GetClassLabel(rMap.dataType);
          }
          else
          {
            rMap.value = refClassLabel;
          }

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
        string msg = ex.ToString();
        _logger.Error(msg);
        return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult MapValueList(FormCollection form)
    {
      try
      {
        string mappingNode = form["mappingNode"];
        string propertyName = form["propertyName"];
        string objectNames = form["objectNames"];
        string[] propertyCtx = objectNames.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string[] mappingCtx = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = propertyCtx[0];
        string classId = form["classId"];
        string application = propertyCtx[1];
        string graphName = mappingCtx[2];

        string roleName = mappingCtx[mappingCtx.Length - 1];
        string valueListName = propertyCtx[propertyCtx.Length - 1];
        int index = Convert.ToInt16(form["index"]);

        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graphName);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId);

        if (ctm != null)
        {
          TemplateMap tMap = ctm.templateMaps[index];
          RoleMap rMap = tMap.roleMaps.Find(rm => rm.name.Equals(roleName));

          if (rMap != null)
          {
            rMap.valueListName = valueListName;
            rMap.propertyName = propertyName;
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
        string scope = form["scope"];
        string application = form["application"];
        string parentNode = form["mappingNode"].Split('/')[2];
        string templateId = form["identifier"];
        string parentClassId = form["parentIdentifier"];
        int index = Convert.ToInt16(form["index"]);

        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(parentNode);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(parentClassId);
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
        string[] context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = context[0];
        string application = context[1];
        Mapping mapping = GetMapping(scope, application);
        string deleteValueList = form["valueList"];
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

    public ActionResult valueListMap(FormCollection form)
    {
      try
      {
        string[] context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = context[0];
        string valueList = context[4];
        string application = context[1];
        string oldClassUrl = form["oldClassUrl"];
        string internalName = form["internalName"];
        string classUrl = form["classUrl"];
        string classLabel = form["classLabel"];

        bool classUrlUsesPrefix = false;

        if (!String.IsNullOrEmpty(classUrl))
        {
          foreach (string prefix in _nsMap.Prefixes)
          {
            if (classUrl.ToLower().StartsWith(prefix + ":"))
            {
              classUrlUsesPrefix = true;
              qName = classUrl;
              break;
            }
          }

          if (!classUrlUsesPrefix)
          {
            qn = _nsMap.ReduceToQName(classUrl, out qName);
          }
        }

        Mapping mapping = GetMapping(scope, application);
        ValueListMap valuelistMap = null;

        if (mapping.valueListMaps != null)
          valuelistMap = mapping.valueListMaps.Find(c => c.name == valueList);

        if (oldClassUrl == "")
        {
          ValueMap valueMap = new ValueMap
          {
            internalValue = internalName,
            uri = qName,
            label = classLabel
          };
          if (valuelistMap.valueMaps == null)
            valuelistMap.valueMaps = new ValueMaps();
          valuelistMap.valueMaps.Add(valueMap);
          _repository.UpdateMapping(scope, application, mapping);
        }
        else
        {
          ValueMap valueMap = valuelistMap.valueMaps.Find(c => c.uri.Equals(oldClassUrl));
          if (valueMap != null)
          {
            valueMap.internalValue = internalName;
            valueMap.uri = qName;
            valueMap.label = classLabel;
            _repository.UpdateMapping(scope, application, mapping);
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
        string[] context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = context[0];
        string valueList = context[4];
        string application = context[1];
        string oldClassUrl = form["oldClassUrl"];
        Mapping mapping = GetMapping(scope, application);
        ValueListMap valuelistMap = null;

        if (mapping.valueListMaps != null)
          valuelistMap = mapping.valueListMaps.Find(c => c.name == valueList);

        ValueMap valueMap = valuelistMap.valueMaps.Find(c => c.uri.Equals(oldClassUrl));
        if (valueMap != null)
          valuelistMap.valueMaps.Remove(valueMap);
        _repository.UpdateMapping(scope, application, mapping);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult CopyValueLists(string targetScope, string targetApplication, 
          string sourceScope, string sourceApplication, string valueList)
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
    
    public ActionResult valueList(FormCollection form)
    {
      try
      {
        string oldValueList = "";
        ValueListMap valueListMap = null;

        string[] context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = context[0];

        if (context.Count() >= 1)
          oldValueList = context[context.Count() - 1];

        string application = context[1];
        Mapping mapping = GetMapping(scope, application);
        string newvalueList = form["valueList"];

        if (mapping.valueListMaps != null)
        {
          if (oldValueList != "")
            valueListMap = mapping.valueListMaps.Find(c => c.name == oldValueList);
          else
            valueListMap = mapping.valueListMaps.Find(c => c.name == newvalueList);
        }
        if (valueListMap == null)
        {
          ValueListMap valuelistMap = new ValueListMap
          {
            name = newvalueList
          };

          mapping.valueListMaps.Add(valuelistMap);
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
      string classLabel = String.Empty;

      if (!String.IsNullOrEmpty(classId))
      {
        if (classId.Contains(":"))
          classId = classId.Substring(classId.IndexOf(":") + 1);

        string key = "class-label-" + classId;

        if (Session[key] != null)
        {
          return (string)Session[key];
        }

        try
        {
          Entity entity = _refdata.GetClassLabel(classId);

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
      JsonArray jsonArray = new JsonArray();

      try
      {
        string scope = form["Scope"];
        string application = form["Application"];
        string recordId = form["recordId"];
        string roleType = form["roleType"];
        string roleValue = form["roleValue"];

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
      string qRange = string.Empty;
      string qId = string.Empty;

      if (currentTemplateMap.roleMaps == null)
        currentTemplateMap.roleMaps = new RoleMaps();

      if (template is TemplateDefinition)
      {
        TemplateDefinition templateDefinition = (TemplateDefinition)template;
        List<RoleDefinition> roleDefinitions = templateDefinition.roleDefinition;

        foreach (RoleDefinition roleDefinition in roleDefinitions)
        {
          string range = roleDefinition.range;
          qn = _nsMap.ReduceToQName(range, out qRange);

          string id = roleDefinition.identifier;
          qn = _nsMap.ReduceToQName(id, out qId);

          RoleMap roleMap = new RoleMap()
          {
            name = roleDefinition.name.FirstOrDefault().value,
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

      if (template is TemplateQualification)
      {
        TemplateQualification templateQualification = (TemplateQualification)template;
        List<RoleQualification> roleQualifications = templateQualification.roleQualification;

        foreach (RoleQualification roleQualification in roleQualifications)
        {
          string range = roleQualification.range;
          qn = _nsMap.ReduceToQName(range, out qRange);

          string id = roleQualification.qualifies;
          qn = _nsMap.ReduceToQName(id, out qId);

          if (currentTemplateMap.roleMaps.Find(x => x.id == qId) == null)
          {
            RoleMap roleMap = new RoleMap()
            {
              name = roleQualification.name.FirstOrDefault().value,
              id = qId
            };

            if (roleQualification.value != null)  // fixed role
            {
              if (!String.IsNullOrEmpty(roleQualification.value.reference))
              {
                roleMap.type = RoleType.Reference;
                qn = _nsMap.ReduceToQName(roleQualification.value.reference, out qRange);
                roleMap.dataType = qn ? qRange : roleQualification.value.reference;
                roleMap.value = GetClassLabel(qRange);
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
            //else if (!qRange.StartsWith("dm:"))  // reference role
            //{
            //  roleMap.type = RoleType.Reference;
            //  roleMap.dataType = qRange;
            //  roleMap.value = GetClassLabel(qRange);
            //}
            else  // unknown
            {
              roleMap.type = RoleType.Unknown;
              roleMap.dataType = qRange;
            }

            currentTemplateMap.roleMaps.Add(roleMap);
          }
        }
      }
    }

    public ActionResult Export(string scope, string application, string graphMap)
    {
      //string scope = form["scope"];
      //string application = form["application"];
      //string graphMap = form["graphMap"];

      Mapping mapping = GetMapping(scope, application);
      Mapping export;
      if (!string.IsNullOrEmpty(graphMap))
      {
        export = new Mapping();
        export.graphMaps = new GraphMaps();
        export.graphMaps.Add(mapping.FindGraphMap(graphMap));
        export.valueListMaps = mapping.valueListMaps;
      }
      else
      {
        export = mapping;
      }

      string content = Utility.SerializeDataContract<Mapping>(export);

      return File(Encoding.UTF8.GetBytes(content), "application/xml", string.Format("Mapping.{0}.{1}.xml", scope, application));
    }

    public ActionResult Import(FormCollection form)
    {
      Mapping mapping = Utility.DeserializeDataContract<Mapping>(form["mapping"]);

      return Json(new { success = false }, JsonRequestBehavior.AllowGet);
    }
  }
}
