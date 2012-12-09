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
using org.iringtools.refdata.response;
using org.iringtools.utility;
using org.iringtools.mapping;

using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;
using org.ids_adi.qmxf;
using VDS.RDF;
using System.Text;
using log4net;
using System.Text.RegularExpressions;
using org.iringtools.web.Models;

namespace org.iringtools.web.controllers
{
  public class MappingController : BaseController
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MappingController));
    NamespaceMapper _nsMap = null;
    private NameValueCollection _settings = null;
    private RefDataRepository _refdata = null;
    private IMappingRepository _repository { get; set; }
    private string _keyFormat = "Mapping.{0}.{1}";
    private const string _unMappedToken = "[unmapped]";
    private char[] _delimiters = new char[] { '/' };
    private bool _qn = false;
    private string _qName = string.Empty;
    private string _contextName = string.Empty;
    private string _endpoint = string.Empty;
    private string _baseUrl = string.Empty;    
  
    public MappingController() : this(new MappingRepository()) { }

    public MappingController(IMappingRepository repository)
    {
      _settings = ConfigurationManager.AppSettings;
      _repository = repository;
        
      _refdata = new RefDataRepository();
      _nsMap = new NamespaceMapper();
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

    private Mapping GetMapping(string baseUrl)
    {
      string key = adapter_PREFIX + string.Format(_keyFormat, _contextName, _endpoint, baseUrl);

      if (Session[key] == null)
      {
        Session[key] = _repository.GetMapping(_contextName, _endpoint, baseUrl);
      }

      return (Mapping)Session[key];
    }

    // added string baseUrl = form["baseUrl"];
    public JsonResult AddClassMap(FormCollection form)
    {
      TreeNode nodes = new TreeNode();
      nodes.children = new List<JsonTreeNode>();
      SetContextEndpoint(form);

      try
      {
        int index = Convert.ToInt32(form["index"]);
        string dataObject = form["dataObject"];
        if (string.IsNullOrEmpty(dataObject)) throw new Exception("Object Name/Property Name has no value");
        string propertyName = form["propertyName"];
        string graphName = form["graphName"];
        string roleName = form["roleName"];
        string classId = form["classID"];
        string classLabel = form["classLabel"];        
        string idents = string.Empty;

        if (string.IsNullOrEmpty(form["relation"]))
          idents = string.Format("{0}.{1}", dataObject, propertyName);
        else
        {
          string relation = form["relation"];          
          idents = string.Format("{0}.{1}.{2}", dataObject, relation, propertyName);
        }

        Mapping mapping = GetMapping(_baseUrl);
        GraphMap graphMap = mapping.FindGraphMap(graphName);

        string parentClassId = form["parentClassId"];
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(parentClassId);

        if (ctm != null)
        {
          ClassMap classMap = new ClassMap();
          TemplateMap templateMap = ctm.TemplateMaps[index];

          foreach (var role in templateMap.RoleMaps)
          {
            if (role.Name == roleName)
            {
              _qn = _nsMap.ReduceToQName(classId, out _qName);
              role.Type = RoleType.Reference;
              role.DataType = _qn ? _qName : classId;
              role.Value = classLabel;
              classMap.Name = classLabel;
              classMap.Id = _qn ? _qName : classId;
              classMap.Identifiers = new Identifiers();

              classMap.Identifiers.Add(idents);
              graphMap.AddClassMap(role, classMap);
              role.ClassMap = classMap;

              string context = _contextName + "/" + _endpoint + "/" + graphMap.Name + "/" + classMap.Name + "/" +
                templateMap.Name + "(" + index + ")" + role.Name;

              nodes.children.Add(CreateClassNode(context, classMap));

              break;
            }
          }
        }
      }

      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(nodes, JsonRequestBehavior.AllowGet);
      }

      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    // added string baseUrl = form["baseUrl"];
    public JsonResult MapReference(FormCollection form)
    {
      try
      {
        SetContextEndpoint(form);
        string templateName = string.Empty;
        string format = String.Empty;
        string reference = string.Empty;

        _qn = _nsMap.ReduceToQName(form["reference"], out reference);
        string classId = form["classId"];
        string label = form["label"];
        string roleName = form["roleName"];
        string roleId = form["roleId"];        
        string graph = form["graphName"];

        int index = Convert.ToInt32(form["index"]);

        Mapping mapping = GetMapping(_baseUrl);
        GraphMap graphMap = mapping.FindGraphMap(graph);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId);
        TemplateMap tMap = ctm.TemplateMaps[index];
        RoleMap rMap = tMap.RoleMaps.Find(c => c.Name == roleName);

        if (rMap != null)
        {
          rMap.Type = RoleType.Reference;
          rMap.DataType = reference;
          rMap.PropertyName = null;
          rMap.ValueListName = null;
        }
        else
        {
          throw new Exception("Error Creating Reference Map...");
        }
      }
      catch (Exception e)
      {
        String msg = e.ToString();
        _logger.Error(msg);
        return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    // added string baseUrl = form["baseUrl"];    
    public JsonResult AddTemplateMap(FormCollection form)
    {
      JsonTreeNode nodes = new JsonTreeNode();

      try
      {
        SetContextEndpoint(form);
        string format = String.Empty;
        string nodeType = form["nodetype"];
        string parentType = form["parentType"];
        string parentId = form["parentId"];
        string identifier = form["id"];
        string graph = form["graphName"];        
        string context = string.Format("{0}/{1}", _contextName, _endpoint);

        ClassMap selectedClassMap = null;
        Mapping mapping = GetMapping(_baseUrl);
        GraphMap graphMap = mapping.FindGraphMap(graph);
        ClassMap graphClassMap = graphMap.ClassTemplateMaps.FirstOrDefault().ClassMap;
        QMXF newtemplate = _refdata.GetTemplate(identifier);

        if (parentType == "GraphMapNode")
        {
          selectedClassMap = graphMap.ClassTemplateMaps.Find(c => c.ClassMap.Id.Equals(parentId)).ClassMap;
        }
        else if (parentType == "ClassMapNode")
        {
          foreach (var classTemplateMap in graphMap.ClassTemplateMaps)
          {
            if (classTemplateMap.ClassMap.Id == parentId)
            {
              selectedClassMap = classTemplateMap.ClassMap;
              break;
            }
          }
        }

        object template = null;
        TemplateMap templateMap = new TemplateMap();

        if (newtemplate.TemplateDefinitions.Count > 0)
        {
          foreach (var defs in newtemplate.TemplateDefinitions)
          {
            template = defs;
            templateMap.Id = defs.Identifier;
            templateMap.Name = defs.Name[0].Value;
            templateMap.Type = TemplateType.Definition;
            GetRoleMaps(selectedClassMap.Id, template, templateMap);
          }
        }
        else
        {
          foreach (var quals in newtemplate.TemplateQualifications)
          {
            template = quals;
            templateMap.Id = quals.Identifier;
            templateMap.Name = quals.Name[0].Value;
            templateMap.Type = TemplateType.Qualification;
            GetRoleMaps(selectedClassMap.Id, template, templateMap);
          }
        }

        graphMap.AddTemplateMap(selectedClassMap, templateMap);
        //_repository.UpdateMapping(mapping, _contextName, _endpoint, _baseUrl);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(nodes, JsonRequestBehavior.AllowGet);
      }

      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    // added string baseUrl = form["baseUrl"];    
    public JsonResult GetNode(FormCollection form)
    {
      SetContextEndpoint(form);
      GraphMap graphMap = null;
      ClassMap graphClassMap = null;
      string format = String.Empty;
      string context = form["node"];      

      //SetContextEndpoint(form);
      
      string[] variables = context.Split(_delimiters, StringSplitOptions.RemoveEmptyEntries);
      string graphName = form["graphName"];

      string key = adapter_PREFIX + string.Format(_keyFormat, _contextName, _endpoint, _baseUrl);
      Mapping mapping = GetMapping(_baseUrl);
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      if (!string.IsNullOrEmpty(graphName))
        graphMap = mapping.GraphMaps.FirstOrDefault<GraphMap>(o => o.Name == graphName);

      if (graphMap != null)
      {
        graphClassMap = graphMap.ClassTemplateMaps.FirstOrDefault().ClassMap;

        switch (form["type"])
        {
          case "MappingNode":
            foreach (var graph in mapping.GraphMaps)
            {
              if (graphMap != null && graphMap.Name != graph.Name) continue;
              JsonTreeNode graphNode = CreateGraphNode(context, graph, graphClassMap);
              nodes.Add(graphNode);
            }

            break;

          case "GraphMapNode":
            if (graphMap != null)
            {
              foreach (var templateMaps in graphMap.ClassTemplateMaps)
              {
                if (templateMaps.ClassMap.Name != graphClassMap.Name) continue;
                int templateIndex = 0;

                foreach (var templateMap in templateMaps.TemplateMaps)
                {
                  TreeNode templateNode = CreateTemplateNode(context, templateMap, templateIndex);
                  TreeNode roleNode = new TreeNode();

                  foreach (var role in templateMap.RoleMaps)
                  {
                    roleNode = new TreeNode
                    {
                      nodeType = "async",
                      type = "RoleMapNode",
                      iconCls = "treeRole",
                      id = templateNode.id + "/" + role.Name,
                      text = role.IsMapped() ? string.Format("{0}{1}", role.Name, "") :
                                                string.Format("{0}{1}", role.Name, _unMappedToken),
                      expanded = false,
                      leaf = false,
                      record = role,
                      property = new Dictionary<string, string>()
                    };

                    if (role.Type == RoleType.Reference)
                    {
                      // 
                      // resolve class label and store it in role value
                      //
                      string classId = role.DataType;

                      if (string.IsNullOrEmpty(classId) || !classId.StartsWith("rdl:"))
                        classId = role.Value;

                      if (!string.IsNullOrEmpty(classId) && !string.IsNullOrEmpty(role.Value) &&
                        role.Value.StartsWith("rdl:"))
                      {
                        string classLabel = GetClassLabel(classId);
                        role.DataType = classId;
                        role.Value = classLabel;
                      }
                    }

                    if (role.ClassMap != null && role.ClassMap.Id != graphClassMap.Id)
                    {
                      TreeNode classNode = CreateClassNode(context, role.ClassMap);

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
              foreach (var classTemplateMap in graphMap.ClassTemplateMaps)
              {
                if (classTemplateMap.ClassMap.Id == classMapId)
                {
                  int templateIndex = 0;

                  foreach (var templateMap in classTemplateMap.TemplateMaps)
                  {
                    TreeNode templateNode = CreateTemplateNode(context, templateMap, templateIndex);
                    TreeNode roleNode = new TreeNode();

                    foreach (var role in templateMap.RoleMaps)
                    {
                      roleNode = new TreeNode
                      {
                        nodeType = "async",
                        type = "RoleMapNode",
                        iconCls = "treeRole",
                        id = templateNode.id + "/" + role.Name,
                        text = role.IsMapped() ? string.Format("{0}{1}", role.Name, "") :
                                                 string.Format("{0}{1}", role.Name, _unMappedToken),
                        expanded = false,
                        leaf = false,
                        record = role,
                        property = new Dictionary<string, string>()
                      };

                      if (role.Type == RoleType.Reference)
                      {
                        // 
                        // resolve class label and store it in role value
                        //
                        string classId = role.DataType;

                        if (string.IsNullOrEmpty(classId) || !classId.StartsWith("rdl:"))
                          classId = role.Value;

                        if (!string.IsNullOrEmpty(classId) && !string.IsNullOrEmpty(role.Value) &&
                          role.Value.StartsWith("rdl:"))
                        {
                          string classLabel = GetClassLabel(classId);
                          role.DataType = classId;
                          role.Value = classLabel;
                        }
                      }

                      if (role.ClassMap != null && role.ClassMap.Id != graphClassMap.Id)
                      {
                        JsonTreeNode classNode = CreateClassNode(context, role.ClassMap);
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
              string className;
              if (string.IsNullOrEmpty(form["className"]))
                className = graphClassMap.Name;
              else
                className = form["className"];

              ClassTemplateMap classTemplateMap =
                graphMap.ClassTemplateMaps.Find(ctm => ctm.ClassMap.Name == className);

              if (classTemplateMap == null) break;
              TemplateMap templateMap =
                classTemplateMap.TemplateMaps.Find(tm => tm.Id == templateId);

              if (templateMap == null) break;
              foreach (var role in templateMap.RoleMaps)
              {
                TreeNode roleNode = CreateRoleNode(context, role);

                if (role.Type == RoleType.Reference)
                {
                  // 
                  // resolve class label and store it in role value
                  //
                  string classId = role.DataType;

                  if (string.IsNullOrEmpty(classId) || !classId.StartsWith("rdl:"))
                    classId = role.Value;

                  if (!string.IsNullOrEmpty(classId) && !string.IsNullOrEmpty(role.Value) &&
                    role.Value.StartsWith("rdl:"))
                  {
                    string classLabel = GetClassLabel(classId);
                    role.DataType = classId;
                    role.Value = classLabel;
                  }
                }

                if (role.ClassMap != null && role.ClassMap.Id != graphClassMap.Id)
                {
                  JsonTreeNode classNode = CreateClassNode(context, role.ClassMap);
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

    // added string baseUrl = form["baseUrl"];    
    public JsonResult DeleteClassMap(FormCollection form)
    {
      try
      {
        SetContextEndpoint(form);
        string graph = form["graphName"];
        string classId = form["classId"];
        string parentClassId = form["parentClass"];
        string parentTemplateId = form["parentTemplate"];
        string parentRoleId = form["parentRole"];
        int index = Convert.ToInt32(form["index"]);
        string className = form["className"];        

        Mapping mapping = GetMapping(_baseUrl);
        GraphMap graphMap = mapping.FindGraphMap(graph);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(parentClassId);
        TemplateMap tMap = ctm.TemplateMaps[index];

        RoleMap rMap = tMap.RoleMaps.Find(r => r.Id.Equals(parentRoleId));
        if (rMap != null)
          graphMap.DeleteRoleMap(tMap, rMap.Id);
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

    // added string baseUrl = form["baseUrl"];    
    public JsonResult ResetMapping(FormCollection form)
    {
      try
      {
        SetContextEndpoint(form);
        string roleId = form["roleId"];
        string templateId = form["templateId"];
        string classId = form["parentClassId"];
        string graphName = form["graphName"];        

        int index = Convert.ToInt32(form["index"]);
        Mapping mapping = GetMapping(_baseUrl);
        GraphMap graphMap = mapping.FindGraphMap(graphName);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId);
        TemplateMap tMap = ctm.TemplateMaps[index];
        RoleMap rMap = tMap.RoleMaps.Find(r => r.Id.Equals(roleId));

        if (rMap.ClassMap != null)
        {
          graphMap.DeleteRoleMap(tMap, rMap.Id);
        }

        if (rMap.DataType.StartsWith("xsd:"))
        {
          rMap.Type = RoleType.DataProperty;
          rMap.PropertyName = string.Empty;
        }
        else
        {
          rMap.Type = RoleType.Unknown;
          rMap.PropertyName = null;
        }

        rMap.Value = null;
        rMap.ValueListName = null;
        rMap.ClassMap = null;
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    // added string baseUrl = form["baseUrl"];    
    public JsonResult MakePossessor(FormCollection form)
    {
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      try
      {
        SetContextEndpoint(form);
        string graphName = form["graphName"];
        int index = Convert.ToInt32(form["index"]);
        string classId = form["classId"];
        string roleName = form["roleName"];        
        string context = string.Format("{0}/{1}/{2}/{3}", _contextName, _endpoint, graphName, roleName);

        Mapping mapping = GetMapping(_baseUrl);
        GraphMap graphMap = mapping.FindGraphMap(graphName);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId);
        TemplateMap tMap = ctm.TemplateMaps[index];
        RoleMap rMap = tMap.RoleMaps.FirstOrDefault(c => c.Name == roleName);

        if (rMap != null)
        {
          rMap.Type = RoleType.Possessor;
          rMap.PropertyName = null;
          rMap.ValueListName = null;
          rMap.Value = null;
          JsonTreeNode roleNode = CreateRoleNode(context, rMap);
          roleNode.text.Replace(_unMappedToken, "");
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

    public JsonResult MakeReference(FormCollection form)
    {
      try
      {
        SetContextEndpoint(form);
        //string mappingNode = form["mappingNode"];
        //string scope = form["scope"];
        //string app = form["app"];
        string graph = form["graph"];
        string classId = form["classId"];
        string roleName = form["roleName"];
        int index = Convert.ToInt16(form["index"]);

        Mapping mapping = GetMapping(_baseUrl);
        GraphMap graphMap = mapping.FindGraphMap(graph);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId);
        TemplateMap tMap = ctm.TemplateMaps[index];
        RoleMap rMap = tMap.RoleMaps.Find(c => c.Name == roleName);

        if (rMap != null)
        {
          rMap.Type = RoleType.Reference;
          rMap.PropertyName = null;
          rMap.ValueListName = null;
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

    private JsonTreeNode CreateGraphNode(string context, GraphMap graph, ClassMap classMap)
    {
      JsonTreeNode graphNode = new JsonTreeNode
      {
        nodeType = "async",
        identifier = classMap.Id,
        type = "GraphMapNode",
        iconCls = "treeGraph",
        id = context + "/" + graph.Name + "/" + classMap.Name,
        text = graph.Name,
        expanded = false,
        leaf = false,
        record = graph
      };

      return graphNode;
    }

    private TreeNode CreateClassNode(string context, ClassMap classMap)
    {
      TreeNode classNode = new TreeNode
      {
        identifier = classMap.Id,
        nodeType = "async",
        type = "ClassMapNode",
        iconCls = "treeClass",
        id = context + "/" + classMap.Name,
        text = classMap.Name,
        expanded = false,
        leaf = false,

        record = classMap
      };

      return classNode;
    }

    private TreeNode CreateTemplateNode(string context, TemplateMap templateMap, int index)
    {
      if (!templateMap.Id.Contains(":"))
        templateMap.Id = string.Format("tpl:{0}", templateMap.Id);

      TreeNode templateNode = new TreeNode
      {
        nodeType = "async",
        identifier = templateMap.Id,
        type = "TemplateMapNode",
        iconCls = "treeTemplate",
        id = context + "/" + templateMap.Name + "(" + index + ")",
        text = templateMap.Name,
        expanded = false,
        leaf = false,
        children = new List<JsonTreeNode>(),
        record = templateMap
      };

      return templateNode;
    }

    private TreeNode CreateRoleNode(string context, RoleMap role)
    {
      TreeNode roleNode = new TreeNode
      {
        nodeType = "async",
        type = "RoleMapNode",
        iconCls = "treeRole",
        id = context + "/" + role.Name,
        text = role.IsMapped() ? string.Format("{0}{1}", role.Name, "") :
                                 string.Format("{0}{1}", role.Name, _unMappedToken),
        expanded = false,
        leaf = false,
        record = role,
        property = new Dictionary<string, string>()
      };

      return roleNode;
    }

    // added string baseUrl = form["baseUrl"];    
    public ActionResult GraphMap(FormCollection form)
    {
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      try
      {
        SetContextEndpoint(form);
        string format = String.Empty;
        string oldGraphName = "";

        oldGraphName = form["oldGraphName"];
        string propertyCtx = form["objectName"];
        if (string.IsNullOrEmpty(propertyCtx)) throw new Exception("ObjectName has no value");

        Mapping mapping = GetMapping(_baseUrl);
        string context = string.Format("{0}/{1}", _contextName, _endpoint);
        string newGraphName = form["graphName"];
        string classLabel = form["classLabel"];

        string keyProperty = form["keyProperty"];
        string dataObject = form["objectName"];       

        string classId = form["classUrl"];
        string oldClassId = form["oldClassUrl"];
        string oldClassLabel = form["oldClassLabel"];

        _qn = _nsMap.ReduceToQName(classId, out _qName);

        if (oldGraphName == "")
        {
          if (mapping.GraphMaps == null)
            mapping.GraphMaps = new GraphMaps();

          GraphMap graphMap = new GraphMap
          {
            Name = newGraphName,
            DataObjectName = dataObject
          };

          ClassMap classMap = new ClassMap
          {
            Name = classLabel,
            Id = _qn ? _qName : classId
          };

          classMap.Identifiers.Add(string.Format("{0}.{1}", dataObject, keyProperty));
          graphMap.AddClassMap(null, classMap);
          mapping.GraphMaps.Add(graphMap);
          nodes.Add(CreateGraphNode(context, graphMap, classMap));
          _repository.UpdateMapping(mapping, _contextName, _endpoint, _baseUrl);
        }
        else
        {
          GraphMap graphMap = mapping.FindGraphMap(oldGraphName);
          if (graphMap == null)
            graphMap = new GraphMap();
          graphMap.Name = newGraphName;
          graphMap.DataObjectName = dataObject;
          ClassTemplateMap ctm = graphMap.ClassTemplateMaps.Find(c => c.ClassMap.Name.Equals(oldClassLabel));

          ctm.ClassMap.Name = classLabel;
          ctm.ClassMap.Id = _qn ? _qName : classId;
          ctm.ClassMap.Identifiers.Clear();
          ctm.ClassMap.Identifiers.Add(string.Format("{0}.{1}", dataObject, keyProperty));
          _repository.UpdateMapping(mapping, _contextName, _endpoint, _baseUrl);
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    private void SetContextEndpoint(FormCollection form)
    {
      _contextName = form["contextName"];
      _endpoint = form["endpoint"];
      _baseUrl = form["baseUrl"];
    }

    // added string baseUrl = form["baseUrl"];   
    public JsonResult UpdateMapping(FormCollection form)
    {
      SetContextEndpoint(form);      
      Mapping mapping = GetMapping(_baseUrl);

      try
      {
        _repository.UpdateMapping(mapping, _contextName, _endpoint, _baseUrl);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    // added string baseUrl = form["baseUrl"];   
    public JsonResult DeleteGraphMap(FormCollection form)
    {
      try
      {
        SetContextEndpoint(form);
        Mapping mapping = GetMapping(_baseUrl);
        string graphName = form["graphName"];
        GraphMap graphMap = mapping.FindGraphMap(graphName);

        if (graphMap != null)
        {
          mapping.GraphMaps.Remove(graphMap);
          _repository.UpdateMapping(mapping, _contextName, _endpoint, _baseUrl);
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    // added string baseUrl = form["baseUrl"];   
    public JsonResult MapProperty(FormCollection form)
    {
      try
      {
        SetContextEndpoint(form);
        string propertyName = form["propertyName"];
        string graphName = form["graphName"];
        string classId = form["classId"];
        string relatedObject = form["relatedObject"];
        string roleName = form["roleName"];
        int index = Convert.ToInt16(form["index"]);

        Mapping mapping = GetMapping(_baseUrl);
        GraphMap graphMap = mapping.FindGraphMap(graphName);
        ClassTemplateMap ctMap = graphMap.GetClassTemplateMap(classId);

        if (ctMap != null)
        {
          TemplateMap tMap = ctMap.TemplateMaps[index];
          RoleMap rMap = tMap.RoleMaps.Find(r => r.Name.Equals(roleName));

          if (!string.IsNullOrEmpty(rMap.DataType) && rMap.DataType.StartsWith("xsd"))
          {
            rMap.PropertyName = getPropertyName(relatedObject, propertyName, graphMap);
            rMap.Type = RoleType.DataProperty;
            rMap.ValueListName = null;
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

    private string getPropertyName(string relatedObject, string propertyName, GraphMap graphMap)
    {
      string rMapPropertyName;
      if (relatedObject != "undefined" && relatedObject != "")
      {
        rMapPropertyName = string.Format("{0}.{1}.{2}",
          graphMap.DataObjectName,
          relatedObject,
          propertyName);
      }
      else
      {
        rMapPropertyName = string.Format("{0}.{1}", graphMap.DataObjectName, propertyName);
      }
      return rMapPropertyName;
    }    

    // added string baseUrl = form["baseUrl"];   
    public JsonResult MapValueList(FormCollection form)
    {
      try
      {
        SetContextEndpoint(form);
        string propertyName = form["propertyName"];
        string graphName = form["graphName"];
        string classId = form["classId"];
        string roleName = form["roleName"];
        string valueListName = form["valueListName"];
        int index = Convert.ToInt16(form["index"]);
        string relatedObject = form["relatedObject"];

        Mapping mapping = GetMapping(_baseUrl);
        GraphMap graphMap = mapping.FindGraphMap(graphName);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId);

        if (ctm != null)
        {
          TemplateMap tMap = ctm.TemplateMaps[index];
          RoleMap rMap = tMap.RoleMaps.Find(rm => rm.Name.Equals(roleName));

          if (rMap != null)
          {
            rMap.PropertyName = getPropertyName(relatedObject, propertyName, graphMap);
            rMap.Type = RoleType.DataProperty;
            rMap.ValueListName = valueListName;
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

    // added string baseUrl = form["baseUrl"];   
    public JsonResult DeleteTemplateMap(FormCollection form)
    {
      try
      {
        SetContextEndpoint(form);
        string parentNode = form["mappingNode"].Split('/')[1];
        string templateId = form["identifier"];
        string parentClassId = form["parentIdentifier"];
        int index = Convert.ToInt16(form["index"]);

        Mapping mapping = GetMapping(_baseUrl);
        GraphMap graphMap = mapping.FindGraphMap(parentNode);
        ClassTemplateMap ctm = graphMap.GetClassTemplateMap(parentClassId);
        ctm.TemplateMaps.RemoveAt(index);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    // added string baseUrl = form["baseUrl"];   
    public JsonResult DeleteValueList(FormCollection form)
    {
      try
      {
        SetContextEndpoint(form);
        Mapping mapping = GetMapping(_baseUrl);
        string deleteValueList = form["valueList"];
        var valueListMap = mapping.ValueListMaps.Find(c => c.Name == deleteValueList);

        if (valueListMap != null)
          mapping.ValueListMaps.Remove(valueListMap);
        _repository.UpdateMapping(mapping, _contextName, _endpoint, _baseUrl);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    // added string baseUrl = form["baseUrl"];   
    public ActionResult ValueListMap(FormCollection form)
    {
      Mapping mapping = null;
      try
      {
        SetContextEndpoint(form);
        string valueList = form["valueList"];
        //?
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
              _qName = classUrl;
              break;
            }
          }

          if (!classUrlUsesPrefix)
          {
            _qn = _nsMap.ReduceToQName(classUrl, out _qName);
          }
        }

        mapping = GetMapping(_baseUrl);
        ValueListMap valuelistMap = null;

        if (mapping.ValueListMaps != null)
          valuelistMap = mapping.ValueListMaps.Find(c => c.Name == valueList);

        if (oldClassUrl == "")
        {
          ValueMap valueMap = new ValueMap
          {
            InternalValue = internalName,
            Uri = _qName,
            Label = classLabel
          };
          if (valuelistMap.ValueMaps == null)
            valuelistMap.ValueMaps = new ValueMaps();
          valuelistMap.ValueMaps.Add(valueMap);
        }
        else
        {
          ValueMap valueMap = valuelistMap.ValueMaps.Find(c => c.Uri.Equals(oldClassUrl));
          if (valueMap != null)
          {
            valueMap.InternalValue = internalName;
            valueMap.Uri = _qName;
            valueMap.Label = classLabel;
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      _repository.UpdateMapping(mapping, _contextName, _endpoint, _baseUrl);
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    // added string baseUrl = form["baseUrl"];   
    public JsonResult DeleteValueMap(FormCollection form)
    {
      Mapping mapping = null;
      try
      {
        SetContextEndpoint(form);
        string valueList = form["valueList"];
        //?
        string oldClassUrl = form["oldClassUrl"];
        mapping = GetMapping(_baseUrl);
        ValueListMap valuelistMap = null;

        if (mapping.ValueListMaps != null)
          valuelistMap = mapping.ValueListMaps.Find(c => c.Name == valueList);

        ValueMap valueMap = valuelistMap.ValueMaps.Find(c => c.Uri.Equals(oldClassUrl));
        if (valueMap != null)
          valuelistMap.ValueMaps.Remove(valueMap);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      _repository.UpdateMapping(mapping, _contextName, _endpoint, _baseUrl);
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    // added string baseUrl = form["baseUrl"];   
    public ActionResult ValueList(FormCollection form)
    {
      Mapping mapping = null;
      try
      {
        SetContextEndpoint(form);
        string oldValueList = "";
        ValueListMap valueListMap = null;

        oldValueList = form["oldValueList"];

        mapping = GetMapping(_baseUrl);
        string newvalueList = form["valueList"];

        if (mapping.ValueListMaps != null)
        {
          if (oldValueList != "")
            valueListMap = mapping.ValueListMaps.Find(c => c.Name == oldValueList);
          else
            valueListMap = mapping.ValueListMaps.Find(c => c.Name == newvalueList);
        }
        if (valueListMap == null)
        {
          ValueListMap valuelistMap = new ValueListMap
          {
            Name = newvalueList
          };

          mapping.ValueListMaps.Add(valuelistMap);
        }
        else
        {
          valueListMap.Name = newvalueList;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      _repository.UpdateMapping(mapping, _contextName, _endpoint, _baseUrl);
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

      if (currentTemplateMap.RoleMaps == null)
        currentTemplateMap.RoleMaps = new RoleMaps();

      if (template is TemplateDefinition)
      {
        TemplateDefinition templateDefinition = (TemplateDefinition)template;
        List<RoleDefinition> roleDefinitions = templateDefinition.RoleDefinition;

        foreach (RoleDefinition roleDefinition in roleDefinitions)
        {
          string range = roleDefinition.Range;
          _qn = _nsMap.ReduceToQName(range, out qRange);

          string id = roleDefinition.Identifier;
          _qn = _nsMap.ReduceToQName(id, out qId);

          RoleMap roleMap = new RoleMap()
          {
            Name = roleDefinition.Name.FirstOrDefault().Value,
            Id = qId
          };

          if (qRange == classId)    // possessor role
          {
            roleMap.Type = RoleType.Possessor;
            roleMap.DataType = qRange;
          }
          else if (qRange.StartsWith("xsd:"))  // data property role
          {
            roleMap.Type = RoleType.DataProperty;
            roleMap.DataType = qRange;
            roleMap.PropertyName = string.Empty;
          }
          else if (!qRange.StartsWith("dm:"))  // reference role
          {
            roleMap.Type = RoleType.Reference;
            roleMap.DataType = qRange;
            roleMap.Value = GetClassLabel(qRange);
          }
          else  // unknown
          {
            roleMap.Type = RoleType.Unknown;
            roleMap.DataType = qRange;
          }

          currentTemplateMap.RoleMaps.Add(roleMap);
        }
      }

      if (template is TemplateQualification)
      {
        TemplateQualification templateQualification = (TemplateQualification)template;
        List<RoleQualification> roleQualifications = templateQualification.RoleQualification;

        foreach (RoleQualification roleQualification in roleQualifications)
        {
          string range = roleQualification.Range;
          _qn = _nsMap.ReduceToQName(range, out qRange);

          string id = roleQualification.Qualifies;
          _qn = _nsMap.ReduceToQName(id, out qId);

          if (currentTemplateMap.RoleMaps.Find(x => x.Id == qId) == null)
          {
            RoleMap roleMap = new RoleMap()
            {
              Name = roleQualification.Name.FirstOrDefault().Value,
              Id = qId
            };

            if (roleQualification.Value != null)  // fixed role
            {
              if (!String.IsNullOrEmpty(roleQualification.Value.Reference))
              {
                roleMap.Type = RoleType.Reference;
                _qn = _nsMap.ReduceToQName(roleQualification.Value.Reference, out qRange);
                roleMap.DataType = _qn ? qRange : roleQualification.Value.Reference;
              }
              else if (!String.IsNullOrEmpty(roleQualification.Value.Text))  // fixed role is a literal
              {
                roleMap.Type = RoleType.FixedValue;
                roleMap.Value = roleQualification.Value.Text;
                roleMap.DataType = roleQualification.Value.As;
              }
            }
            else if (qRange == classId)    // possessor role
            {
              roleMap.Type = RoleType.Possessor;
              roleMap.DataType = qRange;
            }
            else if (qRange.StartsWith("xsd:"))  // data property role
            {
              roleMap.Type = RoleType.DataProperty;
              roleMap.DataType = qRange;
              roleMap.PropertyName = string.Empty;
            }
            else if (!qRange.StartsWith("dm:"))  // reference role
            {
              roleMap.Type = RoleType.Reference;
              roleMap.DataType = qRange;
              roleMap.Value = GetClassLabel(qRange);
            }
            else  // unknown
            {
              roleMap.Type = RoleType.Unknown;
              roleMap.DataType = qRange;
            }

            currentTemplateMap.RoleMaps.Add(roleMap);
          }
        }
      }
    }

    // added string baseUrl = form["baseUrl"];   
    public ActionResult Export(string scope, string application, string graphMap)
    {
      this._contextName = scope;
      this._endpoint = application;

      Mapping mapping = GetMapping(_baseUrl);
      Mapping export;
      if (!string.IsNullOrEmpty(graphMap))
      {
        export = new Mapping();
        export.GraphMaps = new GraphMaps();
        export.GraphMaps.Add(mapping.FindGraphMap(graphMap));
        export.ValueListMaps = mapping.ValueListMaps;
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
