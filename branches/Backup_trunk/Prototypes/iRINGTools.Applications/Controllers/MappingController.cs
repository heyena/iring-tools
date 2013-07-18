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

        private Mapping GetMapping()
        {
            string key = string.Format(_keyFormat, _repository.getScope(), _repository.getApplication());

            if (Session[key] == null)
            {
                Session[key] = _repository.GetMapping();
            }

            return (Mapping)Session[key];
        }

        public JsonResult AddClassMap(FormCollection form)
        {
            TreeNode nodes = new TreeNode();
            nodes.children = new List<JsonTreeNode>();

            try
            {
                int index = Convert.ToInt32(form["index"]);
                string baseUri = form["baseUri"];
                _repository.getAppScopeName(baseUri);
                string dataObject = form["dataObject"];
                string propertyName = form["propertyName"];
                
                string graphName = form["graphName"];
                //?
                
                string roleName = form["roleName"];
                string classId = form["classID"];
                string classLabel = form["classLabel"];
                string idents = string.Empty;

                if (string.IsNullOrEmpty(form["relation"]))               
                    idents = string.Format("{0}.{1}", dataObject, propertyName);                
                else 
                {
                    string relation = form["relation"];
                  //?
                    idents = string.Format("{0}.{1}.{2}", dataObject, relation, propertyName);
                }

                Mapping mapping = GetMapping();
                GraphMap graphMap = mapping.FindGraphMap(graphName);

                string parentClassId = form["parentClassId"];
                ClassTemplateMap ctm = graphMap.GetClassTemplateMap(parentClassId);

                if (ctm != null)
                {
                  ClassMap classMap = new ClassMap();
                  TemplateMap templateMap = ctm.templateMaps[index];

                  foreach (var role in templateMap.roleMaps)
                  {
                    if (role.name == roleName)
                    {
                      qn = _nsMap.ReduceToQName(classId, out qName);
                      role.type = RoleType.Reference;
                      role.value = qn ? qName : classId;
                      classMap.name = classLabel;
                      classMap.id = qn ? qName : classId;
                      classMap.identifiers = new Identifiers();

                      classMap.identifiers.Add(idents);
                      graphMap.AddClassMap(role, classMap);
                      role.classMap = classMap;

                      string context = _repository.getScope() + "/" + _repository.getApplication() + "/" + graphMap.name + "/" + classMap.name + "/" + 
                        templateMap.name + "(" + index + ")" + role.name;

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

        public JsonResult MapReference(FormCollection form)
        {
            try
            {
                string qName = string.Empty;
                string templateName = string.Empty;
                string format = String.Empty;
                string reference = string.Empty;
                string baseUri = form["baseUri"];
                _repository.getAppScopeName(baseUri);
                bool qn = _nsMap.ReduceToQName(form["reference"], out reference);
                string classId = form["classId"];
                string label = form["label"];
                string roleName = form["roleName"];
                string roleId = form["roleId"];
               
                string graph = form["graphName"];
                //?

                int index = Convert.ToInt32(form["index"]);

                Mapping mapping = GetMapping();
                GraphMap graphMap = mapping.FindGraphMap(graph);
                ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId);
                TemplateMap tMap = ctm.templateMaps[index];
                RoleMap rMap = tMap.roleMaps.Find(c => c.name == roleName);

                if (rMap != null)
                {
                    rMap.type = RoleType.Reference;
                    rMap.value = reference;
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
              String msg = e.ToString();
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
                string qName = string.Empty;
                string format = String.Empty;
                string nodeType = form["nodetype"];
                string parentType = form["parentType"];
                string parentId = form["parentId"];
                string identifier = form["id"];
                string baseUri = form["baseUri"];
                _repository.getAppScopeName(baseUri);                
                string graph = form["graphName"];
                string context = string.Format("{0}/{1}", _repository.getScope(), _repository.getApplication());

                ClassMap selectedClassMap = null;
                Mapping mapping = GetMapping();
                GraphMap graphMap = mapping.FindGraphMap(graph);
                ClassMap graphClassMap = graphMap.classTemplateMaps.FirstOrDefault().classMap;
                QMXF newtemplate = _refdata.GetTemplate(identifier);

                if (parentType == "GraphMapNode")
                {
                    selectedClassMap = graphMap.classTemplateMaps.Find(c => c.classMap.id.Equals(parentId)).classMap;
                }
                else if (parentType == "ClassMapNode")
                {
                    foreach (var classTemplateMap in graphMap.classTemplateMaps)
                    {
                        if (classTemplateMap.classMap.id == parentId)
                        {
                            selectedClassMap = classTemplateMap.classMap;
                            break;
                        }
                    }
                }

                object template = null;
                TemplateMap templateMap = new TemplateMap();

                if (newtemplate.templateDefinitions.Count > 0)
                {
                    foreach (var defs in newtemplate.templateDefinitions)
                    {
                        template = defs;
                        templateMap.id = defs.identifier;
                        templateMap.name = defs.name[0].value;
                        templateMap.type = TemplateType.Definition;
                        GetRoleMaps(selectedClassMap.id, template, templateMap);
                    }
                }
                else
                {
                    foreach (var quals in newtemplate.templateQualifications)
                    {
                        template = quals;
                        templateMap.id = quals.identifier;
                        templateMap.name = quals.name[0].value;
                        templateMap.type = TemplateType.Qualification;
                        GetRoleMaps(selectedClassMap.id, template, templateMap);
                    }
                }

                graphMap.AddTemplateMap(selectedClassMap, templateMap);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                return Json(nodes, JsonRequestBehavior.AllowGet);
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
        }

        

        public JsonResult GetNode(FormCollection form)
        {
            GraphMap graphMap = null;
            ClassMap graphClassMap = null;
            string format = String.Empty;
            string context = form["node"];
            string baseUri = form["baseUri"];
            _repository.getAppScopeName(baseUri);
            string[] variables = context.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
          
            string graphName = form["graphName"];
            //?

            string key = string.Format(_keyFormat, _repository.getScope(), _repository.getApplication());

            Mapping mapping = GetMapping();
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
                                  TreeNode templateNode = CreateTemplateNode(context, templateMap, templateIndex);
                                    TreeNode roleNode = new TreeNode();

                                    foreach (var role in templateMap.roleMaps)
                                    {
                                        roleNode = new TreeNode
                                        {
                                            nodeType = "async",
                                            type = "RoleMapNode",
                                            icon = "Content/img/role-map.png",
                                            id = templateNode.id + "/" + role.name,
                                            text = role.IsMapped() ? string.Format("{0}{1}", role.name, "") :
                                                                      string.Format("{0}{1}", role.name, unMappedToken),
                                            expanded = false,
                                            leaf = false,
                                            
                                            record = role
                                        };

                                        if (role.type == RoleType.Reference)
                                        {
                                          roleNode.properties = new Dictionary<string, string>();
                                          roleNode.properties.Add("value label", GetClassLabel(role.value));
                                        }

                                        if (role.classMap != null && role.classMap.id != graphClassMap.id)
                                        {
                                            TreeNode classNode = CreateClassNode(context, role.classMap);

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
                                      TreeNode templateNode = CreateTemplateNode(context, templateMap, templateIndex);
                                      TreeNode roleNode = new TreeNode();

                                        foreach (var role in templateMap.roleMaps)
                                        {
                                          roleNode = new TreeNode
                                            {
                                                nodeType = "async",
                                                type = "RoleMapNode",
                                                icon = "Content/img/role-map.png",
                                                id = templateNode.id + "/" + role.name,
                                                text = role.IsMapped() ? string.Format("{0}{1}", role.name, "") :
                                                                         string.Format("{0}{1}", role.name, unMappedToken),
                                                expanded = false,
                                                leaf = false,
                                                
                                                record = role
                                            };

                                            if (role.type == RoleType.Reference)
                                            {
                                              roleNode.properties = new Dictionary<string, string>();
                                              roleNode.properties.Add("value label", GetClassLabel(role.value));
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
                            string className;
                            if (string.IsNullOrEmpty(form["className"]))
                              className = graphClassMap.name;
                            else
                              className = form["className"];
                          

                            ClassTemplateMap classTemplateMap =
                              graphMap.classTemplateMaps.Find(ctm => ctm.classMap.name == className);

                            if (classTemplateMap == null) break;
                            TemplateMap templateMap =
                              classTemplateMap.templateMaps.Find(tm => tm.id == templateId);

                            if (templateMap == null) break;
                            foreach (var role in templateMap.roleMaps)
                            {
                              TreeNode roleNode = CreateRoleNode(context, role);

                                if (role.type == RoleType.Reference)
                                {
                                  roleNode.properties = new Dictionary<string, string>();
                                  roleNode.properties.Add("value label", GetClassLabel(role.value));
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
                string graph = form["graphName"];
                _repository.setScope(form["scope"]);
                _repository.setApplication(form["application"]);
                string classId = form["classId"];
                string parentClassId = form["parentClass"];
                string parentTemplateId = form["parentTemplate"];
                string parentRoleId = form["parentRole"];
                int index = Convert.ToInt32(form["index"]);
                string className = form["className"];
             
                Mapping mapping = GetMapping();
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
                string baseUri = form["baseUri"];
                _repository.getAppScopeName(baseUri);
                string graphName = form["graphName"];
                
                int index = Convert.ToInt32(form["index"]);
                Mapping mapping = GetMapping();
                GraphMap graphMap = mapping.FindGraphMap(graphName);
                ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId);
                TemplateMap tMap = ctm.templateMaps[index];
                RoleMap rMap = tMap.roleMaps.Find(r => r.id.Equals(roleId));

                if (rMap.classMap != null)
                {
                    graphMap.DeleteRoleMap(tMap, rMap.id);
                }

                if (!string.IsNullOrEmpty(rMap.propertyName) || rMap.dataType.StartsWith("xsd"))
                    rMap.type = RoleType.DataProperty;
                else
                    rMap.type = RoleType.Reference;

                rMap.propertyName = null;
                rMap.value = null;
                rMap.valueListName = null;
                rMap.classMap = null;

                rMap.type = RoleType.Reference;
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
                string graphName = form["graphName"];
                _repository.setScope(form["scope"]);
                _repository.setApplication(form["application"]);
                int index = Convert.ToInt32(form["index"]);
                string classId = form["classId"];
                string roleName = form["roleName"];
                string context = string.Format("{0}/{1}/{2}/{3}", _repository.getScope(), _repository.getApplication(), graphName, roleName);
    
                Mapping mapping = GetMapping();
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
                icon = "Content/img/class-map.png",
                id = context + "/" + graph.name + "/" + classMap.name,
                text = graph.name,
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
                identifier = classMap.id,
                nodeType = "async",
                type = "ClassMapNode",
                icon = "Content/img/class-map.png",
                id = context + "/" + classMap.name,
                text = classMap.name,
                expanded = false,
                leaf = false,
                
                record = classMap
            };

            return classNode;
        }

        private TreeNode CreateTemplateNode(string context, TemplateMap templateMap, int index)
        {
            if (!templateMap.id.Contains(":"))
                templateMap.id = string.Format("tpl:{0}", templateMap.id);

            TreeNode templateNode = new TreeNode
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

        private TreeNode CreateRoleNode(string context, RoleMap role)
        {
          TreeNode roleNode = new TreeNode
            {
                nodeType = "async",
                type = "RoleMapNode",
                icon = "Content/img/role-map.png",
                id = context + "/" + role.name,
                text = role.IsMapped() ? string.Format("{0}{1}", role.name, "") :
                                         string.Format("{0}{1}", role.name, unMappedToken),
                expanded = false,
                leaf = false,
                
                record = role
            };

            return roleNode;
        }

        public ActionResult GraphMap(FormCollection form)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            try
            {
                string qName = string.Empty;
                string format = String.Empty;
                string oldGraphName = "";

                oldGraphName = form["oldGraphName"];
                //?

                string propertyCtx = form["objectName"];
                if (string.IsNullOrEmpty(propertyCtx)) throw new Exception("ObjectName has no value");
                string baseUri = form["baseUri"];
                _repository.getAppScopeName(baseUri);

                Mapping mapping = GetMapping();
                string context = string.Format("{0}/{1}", _repository.getScope(), _repository.getApplication());
                string newGraphName = form["graphName"];
                string classLabel = form["classLabel"];
                
                string keyProperty = form["keyProperty"];
                string dataObject = form["dataObject"];
                //?

                string classId = form["classUrl"];
                string oldClassId = form["oldClassUrl"];
                string oldClassLabel = form["oldClassLabel"];

                bool qn = false;

                qn = _nsMap.ReduceToQName(classId, out qName);

                if (oldGraphName == "")
                {
                    if (mapping.graphMaps == null)
                        mapping.graphMaps = new GraphMaps();

                    GraphMap graphMap = new GraphMap
                    {
                        name = newGraphName,
                        dataObjectName = dataObject
                    };

                    ClassMap classMap = new ClassMap
                    {
                        name = classLabel,
                        id = qn ? qName : classId
                    };

                    classMap.identifiers.Add(string.Format("{0}.{1}", dataObject, keyProperty));
                    graphMap.AddClassMap(null, classMap);
                    mapping.graphMaps.Add(graphMap);
                    nodes.Add(CreateGraphNode(context, graphMap, classMap));
                }
                else
                {
                    GraphMap graphMap = mapping.FindGraphMap(oldGraphName);
                    if (graphMap == null)
                        graphMap = new GraphMap();
                    graphMap.name = newGraphName;
                    graphMap.dataObjectName = dataObject;
                    ClassTemplateMap ctm = graphMap.classTemplateMaps.Find(c => c.classMap.name.Equals(oldClassLabel));

                    ctm.classMap.name = classLabel;
                    ctm.classMap.id = qn ? qName : classId;
                    ctm.classMap.identifiers.Clear();
                    ctm.classMap.identifiers.Add(string.Format("{0}.{1}", dataObject, keyProperty));
                    _repository.UpdateMapping(mapping);
                }
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
            _repository.setScope(form["scope"]);
            _repository.setApplication(form["application"]);
            Mapping mapping = GetMapping();
            try
            {
                _repository.UpdateMapping(mapping);
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
                _repository.setScope(form["scope"]);
                _repository.setApplication(form["application"]);
                Mapping mapping = GetMapping();
                string graphName = form["mappingNode"].Split('/')[4];
                GraphMap graphMap = mapping.FindGraphMap(graphName);

                if (graphMap != null)
                {
                  mapping.graphMaps.Remove(graphMap);
                  _repository.UpdateMapping(mapping);
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
                string propertyName = form["propertyName"];
                string baseUri = form["baseUri"];
                _repository.getAppScopeName(baseUri);
                string graphName = form["graphName"];               
                string classId = form["classId"];
                string relatedObject = form["relatedObject"];
                string roleName = form["roleName"];
                int index = Convert.ToInt16(form["index"]);
                Mapping mapping = GetMapping();
                GraphMap graphMap = mapping.FindGraphMap(graphName);
                ClassTemplateMap ctMap = graphMap.GetClassTemplateMap(classId);

                if (ctMap != null)
                {
                  TemplateMap tMap = ctMap.templateMaps[index];
                  RoleMap rMap = tMap.roleMaps.Find(r => r.name.Equals(roleName));

                  if (!string.IsNullOrEmpty(rMap.dataType) && rMap.dataType.StartsWith("xsd"))
                  {
                    rMap.propertyName = getPropertyName(relatedObject, propertyName, graphMap);
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

        private string getPropertyName(string relatedObject, string propertyName, GraphMap graphMap)
        {
          string rMapPropertyName;
          if (relatedObject != "undefined" && relatedObject != "")
          {
            rMapPropertyName = string.Format("{0}.{1}.{2}",
              graphMap.dataObjectName,
              relatedObject,
              propertyName);
          }
          else
          {
            rMapPropertyName = string.Format("{0}.{1}", graphMap.dataObjectName, propertyName);
          }
          return rMapPropertyName;
        }

        public JsonResult MapValueList(FormCollection form)
        {
          try
          {
            string baseUri = form["baseUri"];
            _repository.getAppScopeName(baseUri);
            string propertyName = form["propertyName"];
            string graphName = form["graphName"];                
            string classId = form["classId"];
            string roleName = form["roleName"];
            string valueListName = form["valueListName"];              
            int index = Convert.ToInt16(form["index"]);
            string relatedObject = form["relatedObject"];

            Mapping mapping = GetMapping();
            GraphMap graphMap = mapping.FindGraphMap(graphName);
            ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId);

            if (ctm != null)
            {
              TemplateMap tMap = ctm.templateMaps[index];
              RoleMap rMap = tMap.roleMaps.Find(rm => rm.name.Equals(roleName));

              if (rMap != null)
              {
                rMap.propertyName = getPropertyName(relatedObject, propertyName, graphMap);                      
                rMap.type = RoleType.DataProperty;
                rMap.valueListName = valueListName;
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
            _repository.setScope(form["scope"]);
            _repository.setApplication(form["application"]);
            string parentNode = form["mappingNode"].Split('/')[2];
            string templateId = form["identifier"];
            string parentClassId = form["parentIdentifier"];              
            int index = Convert.ToInt16(form["index"]);

            Mapping mapping = GetMapping();
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
            string baseUri = form["baseUri"];
            _repository.getAppScopeName(baseUri);

            Mapping mapping = GetMapping();
            string deleteValueList = form["valueList"];
            //?
            var valueListMap = mapping.valueListMaps.Find(c => c.name == deleteValueList);
            if (valueListMap != null)
                mapping.valueListMaps.Remove(valueListMap);
            _repository.UpdateMapping(mapping);
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
                string baseUri = form["baseUri"];
                _repository.getAppScopeName(baseUri);

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
                            qName = classUrl;
                            break;
                        }
                    }

                    if (!classUrlUsesPrefix)
                    {
                        qn = _nsMap.ReduceToQName(classUrl, out qName);
                    }
                }

                Mapping mapping = GetMapping();
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
                    _repository.UpdateMapping(mapping);
                }
                else
                {
                    ValueMap valueMap = valuelistMap.valueMaps.Find(c => c.uri.Equals(oldClassUrl));
                    if (valueMap != null)
                    {
                        valueMap.internalValue = internalName;
                        valueMap.uri = qName;
                        valueMap.label = classLabel;
                        _repository.UpdateMapping(mapping);
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
                string baseUri = form["baseUri"];
                _repository.getAppScopeName(baseUri);

                string valueList = form["valueList"];
              //?
                string oldClassUrl = form["oldClassUrl"];
                Mapping mapping = GetMapping();
                ValueListMap valuelistMap = null;

                if (mapping.valueListMaps != null)
                    valuelistMap = mapping.valueListMaps.Find(c => c.name == valueList);

                ValueMap valueMap = valuelistMap.valueMaps.Find(c => c.uri.Equals(oldClassUrl));
                if (valueMap != null)
                    valuelistMap.valueMaps.Remove(valueMap);
                _repository.UpdateMapping(mapping);
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

                string baseUri = form["baseUri"];
                _repository.getAppScopeName(baseUri);

                oldValueList = form["oldValueList"];
                //?

                Mapping mapping = GetMapping();
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
                    _repository.UpdateMapping(mapping);
                }
                else
                {
                    valueListMap.name = newvalueList;
                    _repository.UpdateMapping(mapping);
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
                _repository.setScope(form["scope"]);
                _repository.setApplication(form["application"]);
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
            bool qn = false;
            string qName = string.Empty;

            if (currentTemplateMap.roleMaps == null)
                currentTemplateMap.roleMaps = new RoleMaps();

            if (template is TemplateDefinition)
            {
                TemplateDefinition templateDefinition = (TemplateDefinition)template;
                List<RoleDefinition> roleDefinitions = templateDefinition.roleDefinition;

                foreach (RoleDefinition roleDefinition in roleDefinitions)
                {
                    string range = roleDefinition.range;
                    RoleMap roleMap = new RoleMap();

                    if (range != classId && range.StartsWith("xsd:"))
                    {
                        qn = _nsMap.ReduceToQName(range, out qName);
                        roleMap.type = RoleType.DataProperty;
                        roleMap.name = roleDefinition.name.FirstOrDefault().value;
                        roleMap.dataType = qn ? qName : range;
                        roleMap.propertyName = string.Empty;
                        qn = _nsMap.ReduceToQName(roleDefinition.identifier, out qName);
                        roleMap.id = qn ? qName : roleDefinition.identifier;
                        currentTemplateMap.roleMaps.Add(roleMap);
                    }
                    else if (range != classId && !range.StartsWith("xsd:"))
                    {
                        roleMap.type = RoleType.ObjectProperty;
                        roleMap.name = roleDefinition.name.FirstOrDefault().value;
                        qn = _nsMap.ReduceToQName(range, out qName);
                        roleMap.dataType = qn ? qName : range;
                        roleMap.propertyName = string.Empty;
                        qn = _nsMap.ReduceToQName(roleDefinition.identifier, out qName);
                        roleMap.id = qn ? qName : roleDefinition.identifier;
                        currentTemplateMap.roleMaps.Add(roleMap);
                    }
                    else if (range == classId)
                    {
                        roleMap.type = RoleType.Possessor;
                        roleMap.name = roleDefinition.name.FirstOrDefault().value;
                        qn = _nsMap.ReduceToQName(range, out qName);
                        roleMap.dataType = qn ? qName : range;
                        roleMap.propertyName = string.Empty;
                        qn = _nsMap.ReduceToQName(roleDefinition.identifier, out qName);
                        roleMap.id = qn ? qName : roleDefinition.identifier;
                        currentTemplateMap.roleMaps.Add(roleMap);
                    }
                }
            }

            if (template is TemplateQualification)
            {
                TemplateQualification templateQualification = (TemplateQualification)template;
                List<RoleQualification> roleQualifications = templateQualification.roleQualification;

                foreach (RoleQualification roleQualification in roleQualifications)
                {
                    qn = _nsMap.ReduceToQName(roleQualification.qualifies, out qName);
                    string range = roleQualification.range;
                    RoleMap roleMap = new RoleMap();

                    roleMap.name = roleQualification.name.FirstOrDefault().value;
                    roleMap.id = qn ? qName : roleQualification.qualifies;

                    if (roleQualification.value != null)  // fixed role
                    {
                        if (!String.IsNullOrEmpty(roleQualification.value.reference))
                        {
                            roleMap.type = RoleType.Reference;
                            qn = _nsMap.ReduceToQName(roleQualification.value.reference, out qName);
                            roleMap.value = qn ? qName : roleQualification.value.reference;
                        }
                        else if (!String.IsNullOrEmpty(roleQualification.value.text))  // fixed role is a literal
                        {
                            roleMap.type = RoleType.FixedValue;
                            roleMap.value = roleQualification.value.text;
                            roleMap.dataType = roleQualification.value.As;
                        }

                        currentTemplateMap.roleMaps.Add(roleMap);
                    }
                    else if (range != classId && range.StartsWith("xsd:")) // property role
                    {
                        roleMap.type = RoleType.DataProperty;
                        qn = _nsMap.ReduceToQName(range, out qName);
                        roleMap.dataType = qn ? qName : range;
                        roleMap.propertyName = String.Empty;
                        currentTemplateMap.roleMaps.Add(roleMap);
                    }
                    else if (range != classId && !range.StartsWith("xsd:"))
                    {
                        roleMap.type = RoleType.ObjectProperty;
                        qn = _nsMap.ReduceToQName(range, out qName);
                        roleMap.dataType = qn ? qName : range;
                        roleMap.propertyName = String.Empty;
                        currentTemplateMap.roleMaps.Add(roleMap);
                    }
                    else if (range == classId)    // class role
                    {
                        roleMap.type = RoleType.Possessor;
                        qn = _nsMap.ReduceToQName(range, out qName);
                        roleMap.dataType = qn ? qName : range;
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

            Mapping mapping = GetMapping();
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

            return File(Encoding.UTF8.GetBytes(content), "application/xml", string.Format("Mapping.{0}.{1}.xml", _repository.getScope(), _repository.getApplication()));
        }

        public ActionResult Import(FormCollection form)
        {
            Mapping mapping = Utility.DeserializeDataContract<Mapping>(form["mapping"]);

            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
    }
}
