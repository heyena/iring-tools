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
using System.Web.Script.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using org.iringtools.UserSecurity;
using System.IO;
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
        private CustomError _CustomError = null;
        private CustomErrorLog _CustomErrorLog = null;

        private ApplicationConfigurationRepository _appConfigRepository = new ApplicationConfigurationRepository();
        string userName = System.Web.HttpContext.Current.Session["userName"].ToString();

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
            JsonTreeNode roleMapeNode = null;

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
                string newEdit = form["newEdit"];
                int parentClassIndex = int.Parse(form["parentClassIndex"]);
                int classIndex = int.Parse(form["classIndex"]);
                GraphMap graphMap = null;
                Guid graphId = Guid.Parse(form["graphId"]);
                //Guid graphId = Guid.Parse("1162f454-700e-4766-83c0-c36b2bfb43c1");
                if (System.Web.HttpContext.Current.Session["graphMap"] != null)
                {
                    graphMap = System.Web.HttpContext.Current.Session["graphMap"] as GraphMap;
                }

                else
                {
                    org.iringtools.applicationConfig.Graph Objgraph = _repository.GetGraphByGrapgId(userName, graphId);
                    graphMap = (GraphMap)DeserializeObject(Objgraph.graph);
                }

                ClassTemplateMap ctm = graphMap.GetClassTemplateMap(parentClassId, parentClassIndex);

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

                            if (newEdit == "NEW")
                                classMap.index = graphMap.GetClassMapMaxIndex(classMap.id) + 1;
                            else
                                classMap.index = classIndex;

                            classMap.path = graphMap.BuildClassPath(ctm.classMap, templateMap, role);

                            classMap.identifiers.AddRange(identifier.Split(','));
                            graphMap.AddClassMap(role, classMap);

                            string context = scope + "/" + app + "/" + graphMap.name + "/" + classMap.name + "/" +
                              templateMap.name + "(" + templateIndex + ")" + role.name;

                            roleMapeNode = CreateRoleNode(role);

                            roleMapeNode.children = new List<JsonTreeNode>();
                            classMapNode = CreateClassNode(classMap);
                            roleMapeNode.children.Add(classMapNode);
                            CreateClassMapNode(graphMap, classMap, classMapNode);
                            System.Web.HttpContext.Current.Session["graphMap"] = graphMap;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIAddClassMap, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { success = true, node = roleMapeNode }, JsonRequestBehavior.AllowGet);
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
                int classMapIndex = int.Parse(form["classMapIndex"]);
                string scope = form["contextName"];//dataObjectVars[0];
                string application = form["endpoint"];//dataObjectVars[1];
                string context = string.Format("{0}/{1}", scope, application);
                Guid graph = Guid.Parse(form["graphId"]);//dataObjectVars[2];
                ClassTemplateMap selectedCtm = null;
                org.iringtools.applicationConfig.Graph Objgraph = _repository.GetGraphByGrapgId(userName, graph);
                GraphMap graphMap = (GraphMap)DeserializeObject(Objgraph.graph);

                ClassMap graphClassMap = graphMap.classTemplateMaps.FirstOrDefault().classMap;
                QMXF templateQmxf = _refdata.GetTemplate(identifier);

                if (parentType == "GraphMapNode")
                {
                    selectedCtm = graphMap.classTemplateMaps.Find(c => (c.classMap.id.Equals(parentId) && c.classMap.index == classMapIndex));
                }
                else if (parentType == "ClassMapNode")
                {
                    foreach (var classTemplateMap in graphMap.classTemplateMaps)
                    {
                        if (classTemplateMap.classMap.id == parentId && classTemplateMap.classMap.index == classMapIndex)
                        {
                            selectedCtm = classTemplateMap;
                            break;
                        }
                    }
                }
                else
                {
                    throw new Exception("This operation is not allowed.");
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

                    //saving in session new code

                    System.Web.HttpContext.Current.Session["graphMap"] = graphMap;

                    // end of saving


                    nodes = CreateTemplateNode(newTemplateMap);
                    foreach (RoleMap roleMap in newTemplateMap.roleMaps)
                    {
                        JsonTreeNode roleNode = CreateRoleNode(roleMap);
                        nodes.children.Add(roleNode);
                        roleNode.leaf = true;
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIAddTemplateMap, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, node = nodes }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNode(FormCollection form)
        {
            try
            {
                GraphMap graphMap = null;
                ClassMap graphClassMap = null;
                string format = String.Empty;
                string context = form["node"];
                Guid graphId = Guid.Parse(form["graphId"]);
                string[] formgraph = form["graph"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string[] variables = context.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string scope = form["context"];//= variables[0];
                string application = form["endpoint"]; //= variables[1];
                string graphName = formgraph[formgraph.Count() - 1];
                string key = string.Format(_keyFormat, scope, application);

                Mapping mapping = GetMapping(scope, application);
                List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                org.iringtools.applicationConfig.Graph graph = _repository.GetGraphByGrapgId(userName, graphId);

                graphMap = (GraphMap)DeserializeObject(graph.graph);
                if (System.Web.HttpContext.Current.Session["graphMap"] != null)
                {
                    System.Web.HttpContext.Current.Session["graphMap"] = null;
                }

                if (graphMap != null)
                {
                    graphClassMap = graphMap.classTemplateMaps.FirstOrDefault().classMap;

                    if (form["type"] == "MappingNode")
                    {
                        JsonTreeNode graphNode = CreateGraphNode(context, graphMap, graphClassMap);
                        nodes.Add(graphNode);
                        graphNode.children = new List<JsonTreeNode>();
                        CreateGraphMapNode(graphMap, graphClassMap, graphNode.id, graphNode.children);
                    }

                }
                return Json(nodes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIGetMappingNode, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

            }
        }

        private void CreateTemplateMapNode(FormCollection form, GraphMap graphMap, ClassMap graphClassMap, string context, string[] variables, List<JsonTreeNode> nodes)
        {
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

                if (classTemplateMap == null) { return; }
                TemplateMap templateMap =
                  classTemplateMap.templateMaps.Find(tm => tm.id == templateId);

                if (templateMap == null) { return; }
                foreach (var role in templateMap.roleMaps)
                {
                    JsonTreeNode roleNode = CreateRoleNode(role);

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
                        JsonTreeNode classNode = CreateClassNode(role.classMap);
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

        private void CreateClassMapNode(GraphMap graphMap, ClassMap graphClassMap, JsonTreeNode classMapNode)
        {

            string context = classMapNode.id;

            var classMapId = classMapNode.identifier;// form["id"];
            int classMapIndex = classMapNode.identifierIndex;

            classMapNode.children = new List<JsonTreeNode>();
            List<JsonTreeNode> nodes = classMapNode.children;

            if (graphMap != null)
            {
                foreach (var classTemplateMap in graphMap.classTemplateMaps)
                {
                    if (classTemplateMap.classMap.id == classMapId && classTemplateMap.classMap.index == classMapIndex)
                    {
                        int templateIndex = 0;

                        foreach (var templateMap in classTemplateMap.templateMaps)
                        {
                            JsonTreeNode templateNode = CreateTemplateNode(templateMap);

                            foreach (var role in templateMap.roleMaps)
                            {
                                JsonTreeNode roleNode = CreateRoleNode(role);
                                if (role.type == RoleType.Reference)
                                {
                                    ResolveClassLabel(role);
                                }

                                if (role.classMap != null && role.classMap.id != graphClassMap.id)
                                {
                                    JsonTreeNode classNode = CreateClassNode(role.classMap);
                                    if (roleNode.children == null)
                                        roleNode.children = new List<JsonTreeNode>();
                                    roleNode.children.Add(classNode);
                                    CreateClassMapNode(graphMap, graphClassMap, classNode);
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
        }

        private void ResolveClassLabel(RoleMap role)
        {
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

        private void CreateGraphMapNode(GraphMap graphMap, ClassMap graphClassMap, string context, List<JsonTreeNode> nodes)
        {
            if (graphMap != null)
            {
                foreach (var templateMaps in graphMap.classTemplateMaps)
                {
                    if (templateMaps.classMap.name != graphClassMap.name) continue;
                    int templateIndex = 0;

                    foreach (var templateMap in templateMaps.templateMaps)
                    {
                        JsonTreeNode templateNode = CreateTemplateNode(templateMap);
                        JsonTreeNode roleNode = null;

                        IList<String> mappedProperties = graphMap.GetMappedProperties(templateMap.id, templateMap.index, templateMaps.classMap.id, templateMaps.classMap.index);
                        var count = 0;
                        for (int i = 0; i < mappedProperties.Count; i++)
                        {
                            templateNode.properties.Add("propertyName_" + i, mappedProperties[i]);
                            count++;
                        }
                        //templateNode.propertiesCount = count;
                        templateNode.properties.Add("propertiesCount", count.ToString());
                        foreach (var role in templateMap.roleMaps)
                        {
                            roleNode = CreateRoleNode(role);

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
                                //JsonTreeNode classNode = CreateClassNode(templateNode.id + "/" + role.name, role.classMap);
                                JsonTreeNode classNode = CreateClassNode(role.classMap);

                                if (roleNode.children == null)
                                    roleNode.children = new List<JsonTreeNode>();

                                roleNode.children.Add(classNode);
                                CreateClassMapNode(graphMap, graphClassMap, classNode);
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
        }

        public JsonResult DeleteClassMap(FormCollection form)
        {
            JsonTreeNode roleMapNode = null;
            try
            {
                string mappingNode = form["mappingNode"];

                // string[] dataObjectVars = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string scope = form["contextName"];//dataObjectVars[0];
                string application = form["endpoint"];//dataObjectVars[1];
                string graph = form["graph"];//dataObjectVars[2];
                string classId = form["classId"];
                string parentClassId = form["parentClass"];
                string parentTemplateId = form["parentTemplate"];
                string parentRoleId = form["parentRole"];
                int index = Convert.ToInt32(form["index"]);
                int parentClassIndex = Convert.ToInt32(form["parentClassIndex"]);
                string templateNodeId = form["templateNodeId"];


                // string className = dataObjectVars[dataObjectVars.Count() - 1];
                //Mapping mapping = GetMapping(scope, application);
                //GraphMap graphMap = mapping.FindGraphMap(graph);

                Guid graphId = Guid.Parse(form["graphId"]);

                GraphMap graphMap = null;
                if (System.Web.HttpContext.Current.Session["graphMap"] != null)
                {
                    graphMap = System.Web.HttpContext.Current.Session["graphMap"] as GraphMap;
                }

                else
                {
                    org.iringtools.applicationConfig.Graph Objgraph = _repository.GetGraphByGrapgId(userName, graphId);
                    graphMap = (GraphMap)DeserializeObject(Objgraph.graph);
                }


                ClassTemplateMap ctm = graphMap.GetClassTemplateMap(parentClassId, parentClassIndex);
                TemplateMap tMap = ctm.templateMaps[index];

                RoleMap rMap = tMap.roleMaps.Find(r => r.id.Equals(parentRoleId));
                if (rMap != null)
                    graphMap.DeleteRoleMap(tMap, rMap.id);
                else
                    throw new Exception("Error deleting ClassMap...");

                roleMapNode = CreateRoleNode(rMap);
                roleMapNode.leaf = true;
                System.Web.HttpContext.Current.Session["graphMap"] = graphMap;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIDeleteClassMap, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                // return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, node = roleMapNode }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ResetMapping(FormCollection form)
        {
            JsonTreeNode roleNode = null;
            try
            {
                string roleId = form["roleId"];
                string templateId = form["templateId"];
                string classId = form["parentClassId"];
                int parentClassIndex = Convert.ToInt32(form["parentClassIndex"]);
                string mappingNode = form["mappingNode"];
                //  string[] dataObjectVars = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string scope = form["contextName"];//dataObjectVars[0];
                string application = form["endpoint"];//dataObjectVars[1];
                string graphName = form["graphName"];//dataObjectVars[2];
                int index = Convert.ToInt32(form["index"]);
                // string parentNodeId = form["parentNodeId"];


                Mapping mapping = GetMapping(scope, application);
                GraphMap graphMap = mapping.FindGraphMap(graphName);
                ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId, parentClassIndex);
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
                roleNode = CreateRoleNode(rMap);
                roleNode.leaf = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                //return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIResetMapping, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { success = true, node = roleNode }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MakePossessor(FormCollection form)
        {
            //List<JsonTreeNode> nodes = new List<JsonTreeNode>();
            JsonTreeNode roleNode = null;

            try
            {
                string mappingNode = form["mappingNode"];

                object selectedNode = form["node"];
                int index = Convert.ToInt32(form["index"]);
                int classIndex = Convert.ToInt32(form["classIndex"]);

                //string[] dataObjectVars = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string scope = form["contextName"];//dataObjectVars[0];
                string application = form["endpoint"];//dataObjectVars[1];
                string graphName = form["graph"];//dataObjectVars[2];
                string classId = form["classId"];
                // string parentNodeId = form["parentNodeId"];
                string roleName = form["roleName"];
                Guid graphId = Guid.Parse(form["graphId"]);
                //string roleName = dataObjectVars[dataObjectVars.Length - 1];
                //  string context = string.Format("{0}/{1}/{2}/{3}", scope, application, graphName, dataObjectVars[dataObjectVars.Count() - 2]);
                //Mapping mapping = GetMapping(scope, application);
                //GraphMap graphMap = mapping.FindGraphMap(graphName);
                GraphMap graphMap = null;
                if (System.Web.HttpContext.Current.Session["graphMap"] != null)
                {
                    graphMap = System.Web.HttpContext.Current.Session["graphMap"] as GraphMap;
                }

                else
                {
                    org.iringtools.applicationConfig.Graph Objgraph = _repository.GetGraphByGrapgId(userName, graphId);
                    graphMap = (GraphMap)DeserializeObject(Objgraph.graph);
                }
                ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId, classIndex);
                TemplateMap tMap = ctm.templateMaps[index];
                RoleMap rMap = tMap.roleMaps.FirstOrDefault(c => c.name == roleName);

                if (rMap != null)
                {
                    rMap.type = RoleType.Possessor;
                    rMap.propertyName = null;
                    rMap.valueListName = null;
                    rMap.value = null;
                    roleNode = CreateRoleNode(rMap);
                    roleNode.text.Replace(unMappedToken, "");
                    roleNode.leaf = true;
                    //nodes.Add(roleNode);
                }
                else
                {
                    throw new Exception("Error Making Possessor Role...");
                }
                System.Web.HttpContext.Current.Session["graphMap"] = graphMap;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIMakeProcessor, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                //return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, node = roleNode }, JsonRequestBehavior.AllowGet);
            
        }

        private JsonTreeNode CreateGraphNode(string context, GraphMap graph, ClassMap classMap)
        {
            JsonTreeNode node = new JsonTreeNode
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
          record = CreateClassNodeRecord(classMap),
          identifierIndex = classMap.index
      };

            return node;
        }

        private JsonTreeNode CreateClassNode(ClassMap classMap)
        {
            JsonTreeNode node = new JsonTreeNode
      {
          identifier = classMap.id,
          nodeType = "async",
          type = "ClassMapNode",
          icon = "Content/img/class-map.png",
          //id = context + "/" + classMap.name,
          id = Guid.NewGuid().ToString("N"),
          text = classMap.name,
          expanded = false,
          leaf = false,
          children = null,
          record = CreateClassNodeRecord(classMap),
          identifierIndex = classMap.index
      };

            return node;
        }
        private Dictionary<string, string> CreateClassNodeRecord(ClassMap classMap)
        {
            Dictionary<string, string> record = new Dictionary<string, string>();
            record["class name"] = classMap.name;
            record["class id"] = classMap.id;
            record["identifier delimiter"] = classMap.identifierDelimiter;
            record["identifier"] = string.Join(",", classMap.identifiers);
            if (classMap.identifiers.Count > 1)
            {
                record["identifierDelimiter"] = classMap.identifierDelimiter;
            }
            return record;
        }

        private JsonTreeNode CreateTemplateNode(TemplateMap templateMap)
        {
            if (!templateMap.id.Contains(":"))
                templateMap.id = string.Format("tpl:{0}", templateMap.id);

            JsonTreeNode node = new JsonTreeNode
      {
          nodeType = "async",
          identifier = templateMap.id,
          type = "TemplateMapNode",
          icon = "Content/img/template-map.png",
          //id = context + "/" + templateMap.name + "(" + templateMap.index + ")",
          id = Guid.NewGuid().ToString("N"),
          text = templateMap.name,
          expanded = false,
          leaf = false,
          children = new List<JsonTreeNode>(),
          //record = templateMap,
          properties = new Dictionary<string, string>(),
          //propertiesCount = 0
      };

            Dictionary<string, string> record = new Dictionary<string, string>();
            record["name"] = templateMap.name;
            record["id"] = templateMap.id;
            record["type"] = templateMap.type.ToString();
            record["index"] = templateMap.index.ToString();
            node.record = record;
            return node;
        }

        private JsonTreeNode CreateRoleNode(RoleMap role)
        {
            JsonTreeNode node = new JsonTreeNode
            {
                nodeType = "async",
                type = "RoleMapNode",
                icon = "Content/img/role-map.png",
                //id = context + "/" + role.name,
                id = Guid.NewGuid().ToString("N") + "/" + role.name,
                text = role.IsMapped() ? string.Format("{0}{1}", role.name, "") :
                                        string.Format("{0}{1}", role.name, unMappedToken),
                expanded = false,
                leaf = false,
                children = null,
                //record = role,
                //properties = new Dictionary<string, string>()
            };

            Dictionary<string, string> record = new Dictionary<string, string>();
            record["name"] = role.name;
            record["id"] = role.id;
            record["type"] = role.type.ToString();

            string dataType = role.dataType;
            record["dataType"] = dataType;

            if (dataType != null)
            {
                if (!dataType.ToLower().StartsWith("xsd:"))
                {
                    if (!string.IsNullOrEmpty(role.value))
                    {
                        record["value"] = role.value;
                    }
                    else
                    {
                        string label = GetClassLabel(role.dataType);
                        if (!string.IsNullOrEmpty(label))
                            record["value"] = label;
                    }
                }

                if (role.dataType.ToLower().StartsWith("xsd:") || role.type == RoleType.ObjectProperty
                    || !string.IsNullOrEmpty(role.valueListName))
                {
                    record["propertyName"] = role.propertyName;
                    if (!string.IsNullOrEmpty(role.valueListName))
                    {
                        record["valueListName"] = role.valueListName;
                    }
                }
            }

            node.record = record;

            return node;
        }

        public ActionResult GraphMap(FormCollection form)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
            JsonTreeNode graphNode = new JsonTreeNode();

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
                Guid applicationId = Guid.Empty;
                Guid contextId = Guid.Empty; ;



                string context = string.Format("{0}/{1}/Graphs", scope, app);

                GraphMap graphMap = null;
                string groups = form["ResourceGroups"];

                bool qn = false;
                qn = _nsMap.ReduceToQName(classId, out qName);


                if (!string.IsNullOrEmpty(form["applicationId"].ToString().Trim()) || !string.IsNullOrWhiteSpace(form["applicationId"].ToString().Trim()))
                {
                    applicationId = Guid.Parse(form["applicationId"]);
                    contextId = Guid.Parse(form["applicationId"]);



                    graphMap = new GraphMap
                      {
                          name = graphName,
                          dataObjectName = objectName
                      };

                    ClassMap classMap = new ClassMap
                      {
                          name = className,
                          id = qn ? qName : classId,
                          identifierDelimiter = delimiter,
                          identifiers = new Identifiers(),
                          path = ""
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


                    insertGraph(scope, app, graphMap, applicationId, groups);
                }
                else // Edit existing graph
                {

                    Guid graphId = Guid.Parse(form["graphId"]);

                    org.iringtools.applicationConfig.Graph graph = _repository.GetGraphByGrapgId(userName, graphId);

                    graphMap = (GraphMap)DeserializeObject(graph.graph);


                    if (graphMap == null)
                        graphMap = new GraphMap();

                    graphMap.name = graphName;
                    graphMap.dataObjectName = objectName;

                    if (graphMap.classTemplateMaps.Count > 0)
                    {
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
                    else
                    {
                        ClassMap classMap = new ClassMap
                        {
                            name = className,
                            id = qn ? qName : classId,
                            identifierDelimiter = delimiter,
                            identifiers = new Identifiers(),
                            path = ""
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

                    }
                    UpdatetGraph(scope, app, graphMap, applicationId, groups, graphId.ToString());



                }


                graphNode = new JsonTreeNode
                {
                    nodeType = "async",
                    type = "GraphNode",
                    iconCls = "treeGraph",
                    id = context + "/Graph/" + graphMap.name,
                    text = graphMap.name,
                    expanded = true,
                    leaf = true,
                    children = new List<JsonTreeNode>(),
                    record = graphMap

                };

                graphNode.property = new Dictionary<string, string>();
                graphNode.property.Add("Data Object", graphMap.dataObjectName);
                if (graphMap.classTemplateMaps.Count > 0)
                    graphNode.property.Add("Root Class", graphMap.classTemplateMaps[0].classMap.name);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIGraphMapping, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);


            }
            return Json(new { success = true, node = graphNode }, JsonRequestBehavior.AllowGet);
        }




        public JsonResult UpdateMapping(FormCollection form)
        {
            string scope = form["scope"];
            string application = form["application"];
            Guid graphId = Guid.Parse(form["graphId"]);
            org.iringtools.applicationConfig.Graph graph = _repository.GetGraphByGrapgId(userName, graphId);
            GraphMap graphMap = null;
            if (System.Web.HttpContext.Current.Session["graphMap"] != null)
            {
                graphMap = System.Web.HttpContext.Current.Session["graphMap"] as GraphMap;
            }

            else
            {
                org.iringtools.applicationConfig.Graph Objgraph = _repository.GetGraphByGrapgId(userName, graphId);
                graphMap = (GraphMap)DeserializeObject(Objgraph.graph);
            }



            graphMap.RearrangeIndexAndPath();
            string groups = string.Empty;
            foreach (org.iringtools.UserSecurity.Group group in graph.Groups)
            {
                groups = groups + group.GroupId + ",";
            }
            int index = groups.LastIndexOf(",");
            groups = groups.Substring(0, (index - 1));
            return UpdatetGraph(scope, application, graphMap, Guid.Empty, groups, graphId.ToString());


        }

        public JsonResult insertGraph(string scope, string application, GraphMap mapping, Guid applicationId, string groups)
        {
            try
            {


                Groups selectedGroups = new Groups();

                string[] groupArray = groups.Split(',');
                for (int i = 0; i < groupArray.Length; i++)
                {
                    org.iringtools.UserSecurity.Group objGroup = new UserSecurity.Group();
                    objGroup.GroupId = Convert.ToInt16(groupArray[i].ToString());
                    selectedGroups.Add(objGroup);
                }
                org.iringtools.applicationConfig.Graph graph = new applicationConfig.Graph()
                {

                    ApplicationId = applicationId,
                    graph = ObjectToByteArray(mapping),
                    GraphId = Guid.Empty,
                    GraphName = mapping.name.ToString(),
                    Groups = selectedGroups

                };

                //  _repository.UpdateMapping(scope, application, mapping)

                _repository.UpdateMapping(scope, application, graph, userName, true);


                string key = string.Format(_keyFormat, scope, application);
                Session.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                //return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIUpdateMap, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                //return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DoUpdateMapping(string scope, string application, Mapping mapping)
        {
            try
            {
                _repository.UpdateMapping(scope, application, mapping);

                string key = string.Format(_keyFormat, scope, application);
                Session.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                //return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIUpdateMap, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                //return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }



        public JsonResult DeleteGraphMap(FormCollection form)
        {
            try
            {
                Guid graphId = Guid.Parse(form["graphId"]);
                if (graphId != null)
                {
                    _repository.DeleteGraphByGrapgId(userName, graphId);
                    //mapping.graphMaps.Remove(graphMap);
                    // DoUpdateMapping(scope, application, mapping);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                // return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIDeletegraphMap, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MapProperty(FormCollection form)
        {
            JsonTreeNode roleMapNode = null;
            try
            {
                string mappingNode = form["mappingNode"];
                string propertyName = form["propertyName"];
                string[] mappingCtx = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string scope = form["contextName"];//mappingCtx[0];
                string application = form["endpoint"];//mappingCtx[1];
                string graphName = form["graphName"];//mappingCtx[2];


                string classId = form["classId"];
                string[] classCtx = classId.Split(',');
                classId = classCtx[0];
                int classIndex = Convert.ToInt16(form["classIndex"]);
                string roleName = mappingCtx[mappingCtx.Length - 1];
                int index = Convert.ToInt16(form["index"]);
                Guid graphId = Guid.Parse(form["graphId"]);

                GraphMap graphMap = null;

                if (System.Web.HttpContext.Current.Session["graphMap"] != null)
                {
                    graphMap = System.Web.HttpContext.Current.Session["graphMap"] as GraphMap;
                }

                else
                {
                    org.iringtools.applicationConfig.Graph Objgraph = _repository.GetGraphByGrapgId(userName, graphId);
                    graphMap = (GraphMap)DeserializeObject(Objgraph.graph);
                }

                ClassTemplateMap ctMap = graphMap.GetClassTemplateMap(classId, classIndex);

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

                    roleMapNode = CreateRoleNode(rMap);
                    roleMapNode.leaf = true;
                }
                System.Web.HttpContext.Current.Session["graphMap"] = graphMap;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIMapProperty, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
            
            return Json(new { success = true, node = roleMapNode }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MapConstant(FormCollection form)
        {
            JsonTreeNode roleMapNode = null;
            try
            {
                string mappingNode = form["mappingNode"];
                string constantValue = form["constantValue"];
                string[] mappingCtx = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string scope = form["contextName"];//mappingCtx[0];
                string application = form["endpoint"];//mappingCtx[1];
                string graphName = form["graph"];//mappingCtx[2];
                string classId = form["classId"];
                int classIndex = Convert.ToInt16(form["classIndex"]);
                //string parentNodeId = form["parentNodeId"];

                string roleName = mappingCtx[mappingCtx.Length - 1];
                int index = Convert.ToInt16(form["index"]);

                //Guid graphId = Guid.Parse(form["graphId"]);
                //GraphMap graphMap = null;

                //if (System.Web.HttpContext.Current.Session["graphMap"] != null)
                //{
                //    graphMap = System.Web.HttpContext.Current.Session["graphMap"] as GraphMap;
                //}
                //else
                //{
                //    org.iringtools.applicationConfig.Graph Objgraph = _repository.GetGraphByGrapgId(userName, graphId);
                //    graphMap = (GraphMap)DeserializeObject(Objgraph.graph);
                //}
                Mapping mapping = GetMapping(scope, application);
                GraphMap graphMap = mapping.FindGraphMap(graphName);
                ClassTemplateMap ctMap = graphMap.GetClassTemplateMap(classId, classIndex);

                if (ctMap != null)
                {
                    TemplateMap tMap = ctMap.templateMaps[index];
                    RoleMap rMap = tMap.roleMaps.Find(r => r.name.Equals(roleName));

                    if (!string.IsNullOrEmpty(rMap.dataType) && rMap.dataType.StartsWith("xsd"))
                    {
                        if (!Utility.ValidateValueWithXsdType(rMap.dataType, constantValue))
                            throw new Exception("Invalid Data type of literal");

                        rMap.propertyName = null;
                        rMap.type = RoleType.FixedValue;
                        rMap.value = constantValue;
                        rMap.valueListName = null;
                    }
                    else
                    {
                        throw new Exception("Invalid constant map.");
                    }
                    roleMapNode = CreateRoleNode(rMap);
                    roleMapNode.leaf = true;
                }
            }
            catch (Exception ex)
            {
                //string msg = ex.ToString();
                //_logger.Error(msg);
                //return Json(new { success = false, message = msg }, JsonRequestBehavior.AllowGet);
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIMapConstant, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { success = true, node = roleMapNode }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult MakeReference(FormCollection form)
        {
            JsonTreeNode roleNode = null;
            try
            {
                string scope = form["scope"];
                string app = form["app"];
                //string graph = form["graph"];
                string classId = form["classId"];
                int classIndex = Convert.ToInt16(form["classIndex"]);

                int templateIndex = Convert.ToInt16(form["templateIndex"]);
                string roleName = form["roleName"];
                string refClassId = form["refClassId"];
                string refClassLabel = form["refClassLabel"];
                //string parentNodeId = form["parentNodeId"];

                Guid graphId = Guid.Parse(form["graphId"]);

                GraphMap graphMap = null;

                if (System.Web.HttpContext.Current.Session["graphMap"] != null)
                {
                    graphMap = System.Web.HttpContext.Current.Session["graphMap"] as GraphMap;
                }

                else
                {
                    org.iringtools.applicationConfig.Graph Objgraph = _repository.GetGraphByGrapgId(userName, graphId);
                    graphMap = (GraphMap)DeserializeObject(Objgraph.graph);
                }


                //Mapping mapping = GetMapping(scope, app);
                //GraphMap graphMap = mapping.FindGraphMap(graph);
                
                ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId, classIndex);
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

                roleNode = CreateRoleNode(rMap);
                roleNode.leaf = true;
                System.Web.HttpContext.Current.Session["graphMap"] = graphMap;
            }
            catch (Exception ex)
            {
                //string msg = ex.ToString();
                //_logger.Error(msg);
                //return Json(new { success = false, message = msg }, JsonRequestBehavior.AllowGet);
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIMakeReference, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

            }
            //return Json(roleNode, JsonRequestBehavior.AllowGet);
            return Json(new { success = true, node = roleNode }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MapValueList(FormCollection form)
        {
            JsonTreeNode roleMapNode = null;
            try
            {
                string mappingNode = form["mappingNode"];
                string propertyName = form["propertyName"];
                string objectNames = form["objectNames"];
                string[] propertyCtx = objectNames.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string[] mappingCtx = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string classId = form["classId"];
                string scope = form["contextName"];//propertyCtx[0];
                string application = form["endpoint"];//propertyCtx[1];
                string graphName = form["graphName"];//mappingCtx[2]graphId;
                int classIndex = Convert.ToInt16(form["classIndex"]);
               


                //string parentNodeId = form["parentNodeId"];//mappingCtx[2];


                string roleName = mappingCtx[mappingCtx.Length - 1];
                //string valueListName = propertyCtx[propertyCtx.Length - 1];
                string valueListName = objectNames;
                int index = Convert.ToInt16(form["index"]);

                //////Mapping mapping = GetMapping(scope, application);
                //////GraphMap graphMap = mapping.FindGraphMap(graphName);
                
                Guid graphId = Guid.Parse(form["graphId"]);

                GraphMap graphMap = null;

                if (System.Web.HttpContext.Current.Session["graphMap"] != null)
                {
                    graphMap = System.Web.HttpContext.Current.Session["graphMap"] as GraphMap;
                }

                else
                {
                    org.iringtools.applicationConfig.Graph Objgraph = _repository.GetGraphByGrapgId(userName, graphId);
                    graphMap = (GraphMap)DeserializeObject(Objgraph.graph);
                }
                ClassTemplateMap ctm = graphMap.GetClassTemplateMap(classId, classIndex);

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

                    roleMapNode = CreateRoleNode(rMap);
                    roleMapNode.leaf = true;
                }

                System.Web.HttpContext.Current.Session["graphMap"] = graphMap;
            }
            catch (Exception ex)
            {
                //string msg = ex.ToString();
                //_logger.Error(msg);
                //return Json(new { success = false, message = msg }, JsonRequestBehavior.AllowGet);
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIMapValueList, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, node = roleMapNode }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteTemplateMap(FormCollection form)
        {
            try
            {
                string scope = form["scope"];
                string application = form["application"];
                string graphName = form["rootNodeId"].Split('/')[1];//form["mappingNode"].Split('/')[2];
                string templateId = form["identifier"];
                string parentClassId = form["parentIdentifier"];
                int parentClassIndex = Convert.ToInt32(form["parentClassIndex"]);
                int index = Convert.ToInt16(form["index"]);

                //Mapping mapping = GetMapping(scope, application);
                //GraphMap graphMap = mapping.FindGraphMap(graphName);
                Guid graphId = Guid.Parse(form["graphId"]);
                GraphMap graphMap = null;

                if (System.Web.HttpContext.Current.Session["graphMap"] != null)
                {
                    graphMap = System.Web.HttpContext.Current.Session["graphMap"] as GraphMap;
                }

                else
                {
                    org.iringtools.applicationConfig.Graph Objgraph = _repository.GetGraphByGrapgId(userName, graphId);
                    graphMap = (GraphMap)DeserializeObject(Objgraph.graph);
                }
                
                
                //ClassTemplateMap ctm = graphMap.GetClassTemplateMap(parentClassId, parentClassIndex);
                //TemplateMap tm = ctm.templateMaps[index];
                //ctm.templateMaps.RemoveAt(index);
                graphMap.DeleteTemplateMap(parentClassId, parentClassIndex, index);
                System.Web.HttpContext.Current.Session["graphMap"] = graphMap;

            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIDeleteMapTemplate, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                //return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
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
                Guid applicationId = Guid.Parse(form["applicationId"]);
                var valueListMap = mapping.valueListMaps.Find(c => c.name == deleteValueList);
                if (valueListMap != null)
                    mapping.valueListMaps.Remove(valueListMap);
                DoUpdateMapping(scope, application, mapping);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIDeleteValueList, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                // return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult valueListMap(FormCollection form)
        {
            //List<JsonTreeNode> nodes = new List<JsonTreeNode>();
            JsonTreeNode valueListMapNode = new JsonTreeNode();
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
                Guid applicationId = Guid.Parse(form["applicationId"]);
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
                    DoUpdateMapping(scope, application, mapping);
                }
                else
                {
                    ValueMap valueMap = valuelistMap.valueMaps.Find(c => c.uri.Equals(oldClassUrl));
                    if (valueMap != null)
                    {
                        valueMap.internalValue = internalName;
                        valueMap.uri = qName;
                        valueMap.label = classLabel;
                        DoUpdateMapping(scope, application, mapping);
                    }
                }

                foreach (var valueMap in valuelistMap.valueMaps)
                {
                    if (form["internalName"] == valueMap.internalValue)
                    {
                        JsonTreeNode node = new JsonTreeNode
                        {
                            nodeType = "async",
                            type = "ListMapNode",
                            iconCls = "valuelistmap",
                            id = context[0] + "/" + context[1] + "/" + context[2] + "/" + context[3] + "/" + context[4] + "/ValueMap/" + valueMap.internalValue,//context + "/ValueMap/" + valueMap.internalValue,
                            text = valueMap.label + " [" + valueMap.internalValue + "]",
                            expanded = false,
                            leaf = true,
                            children = null,
                            record = valueMap
                        };

                        node.property = new Dictionary<string, string>();
                        node.property.Add("Name", valueMap.internalValue);
                        node.property.Add("Class Label", valueMap.label);
                        valueListMapNode = node;
                    }
                    //nodes.Add(node);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIValueListMap, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                // return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, node = valueListMapNode }, JsonRequestBehavior.AllowGet);
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
                Guid applicationId = Guid.Parse(form["applicationId"]);
                if (mapping.valueListMaps != null)
                    valuelistMap = mapping.valueListMaps.Find(c => c.name == valueList);

                ValueMap valueMap = valuelistMap.valueMaps.Find(c => c.uri.Equals(oldClassUrl));
                if (valueMap != null)
                    valuelistMap.valueMaps.Remove(valueMap);
                DoUpdateMapping(scope, application, mapping);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIDeleteValueMap, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                //  return Json(new { success = false }, JsonRequestBehavior.AllowGet);
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
                Guid applicationId = Guid.Empty;  //Guid.Parse(form["applicationId"]);
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
                DoUpdateMapping(targetScope, targetApplication, targetMapping);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUICopyValueList, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                // return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult valueList(FormCollection form)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
            JsonTreeNode valueListNode = new JsonTreeNode();
            try
            {
                string oldValueList = "";
                ValueListMap valueListMap = null;
                Guid applicationId = Guid.Parse(form["applicationId"]);

                string[] context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string scope = context[0];

                if (context.Count() >= 1)
                    oldValueList = context[context.Count() - 1];

                /////////string application = context[1];
               ///////// Mapping mapping = GetMapping(scope, application);
                string newvalueList = form["valueList"];

                //if (mapping.valueListMaps != null)
                //{
                if (form["valueListId"] != "")
                {
                    Guid valueListId = Guid.Parse(form["valueListId"]);
                    _repository.updateValueListMap(applicationId, valueListId, newvalueList);
                }
                ////////  valueListMap = mapping.valueListMaps.Find(c => c.name == oldValueList);
                else
                ///////////  valueListMap = mapping.valueListMaps.Find(c => c.name == newvalueList);
                {
                    _repository.InsertValueListMap(newvalueList, applicationId);
                }
                    // }

              


                    if (valueListMap == null)
                    {
                        ValueListMap valuelistMap = new ValueListMap
                        {
                            name = newvalueList
                        };

                     /////   mapping.valueListMaps.Add(valuelistMap);
                      //////  DoUpdateMapping(scope, application, mapping);
                    }
                    else
                    {
                        valueListMap.name = newvalueList;
                       /////// DoUpdateMapping(scope, application, mapping);
                    }

                    ValueListMaps valueListMaps = _appConfigRepository.GetValueListMaps(userName, applicationId);
                
                //foreach (ValueListMap valueList in mapping.valueListMaps)
               
                foreach (ValueListMap valueList in valueListMaps)
                {
                    if (form["valueList"] == valueList.name)
                    {
                        valueListNode = new JsonTreeNode
                        {
                            nodeType = "async",
                            type = "ValueListNode",
                            iconCls = "valuemap",
                          //  id = scope + "/" + application + "/ValueLists" + "/ValueList/" + valueList.name,//context + "/ValueList/" + valueList.name,
                            id = "valuelistId-" + valueList.ValueListMapId.ToString(),
                            text = valueList.name,
                            expanded = false,
                            leaf = false,
                            children = null,
                            record = valueList
                        };
                        valueListNode.property = new Dictionary<string, string>();
                        valueListNode.property.Add("Name", valueList.name);

                        if (valueList.valueMaps != null)
                        {
                            foreach (var valueMap in valueList.valueMaps)
                            {
                                string classLabel = String.Empty;

                                if (!String.IsNullOrEmpty(valueMap.uri))
                                {
                                    string valueMapUri = valueMap.uri.Split(':')[1];

                                    if (!String.IsNullOrEmpty(valueMap.label))
                                    {
                                        classLabel = valueMap.label;
                                    }
                                    else if (Session[valueMapUri] != null)
                                    {
                                        classLabel = (string)Session[valueMapUri];
                                    }
                                    else
                                    {
                                        classLabel = GetClassLabel(valueMapUri);
                                        Session[valueMapUri] = classLabel;
                                    }
                                }

                                JsonTreeNode node = new JsonTreeNode
                                {
                                    nodeType = "async",
                                    type = "ListMapNode",
                                    iconCls = "valuelistmap",
                                    id = context[0] + "/" + context[1] + "/" + context[2] + "/" + context[3] + "/" + form["valueList"] + "/ValueMap/" + valueMap.internalValue,//context + "/ValueMap/" + valueMap.internalValue,
                                    text = classLabel + " [" + valueMap.internalValue + "]",
                                    expanded = false,
                                    leaf = true,
                                    children = null,
                                    record = valueMap
                                };

                                node.property = new Dictionary<string, string>();
                                node.property.Add("Name", valueMap.internalValue);
                                node.property.Add("Class Label", classLabel);
                                if (valueListNode.children == null)
                                    valueListNode.children = new List<JsonTreeNode>();
                                valueListNode.children.Add(node);
                            }
                        }
                        nodes.Add(valueListNode);
                    }

                }
                
               
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIValueList, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                //  return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, node = nodes }, JsonRequestBehavior.AllowGet);
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
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIGetLabels, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                //return Json(jsonArray, JsonRequestBehavior.AllowGet);
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


        private byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public object DeserializeObject(byte[] bytes)
        {
            //byte[] bytes = Convert.FromBase64String(str);

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return new BinaryFormatter().Deserialize(stream);
            }
        }


        private List<org.iringtools.UserSecurity.Group> GetSelectedGroups(string Groups)
        {
            List<org.iringtools.UserSecurity.Group> tempGroups = new List<org.iringtools.UserSecurity.Group>();

            string[] groupArray = Groups.Split(',');

            for (int i = 0; i < groupArray.Length; i++)
            {
                tempGroups.Add(new org.iringtools.UserSecurity.Group() { GroupId = int.Parse(groupArray[i]) });
            }

            return tempGroups;
        }


        public JsonResult UpdatetGraph(string scope, string application, GraphMap mapping, Guid applicationId, string groups, string graphId)
        {
            try
            {


                Groups selectedGroups = new Groups();

                string[] groupArray = groups.Split(',');
                for (int i = 0; i < groupArray.Length; i++)
                {
                    org.iringtools.UserSecurity.Group objGroup = new UserSecurity.Group();
                    objGroup.GroupId = Convert.ToInt16(groupArray[i].ToString());
                    selectedGroups.Add(objGroup);
                }
                org.iringtools.applicationConfig.Graph graph = new applicationConfig.Graph()
                {

                    ApplicationId = applicationId,
                    graph = ObjectToByteArray(mapping),
                    GraphId = Guid.Parse(graphId),
                    GraphName = mapping.name.ToString(),
                    Groups = selectedGroups

                };

                //  _repository.UpdateMapping(scope, application, mapping)

                _repository.UpdateMapping(scope, application, graph, userName, false, graphId);


                string key = string.Format(_keyFormat, scope, application);
                Session.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                //return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIUpdateMap, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                //return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }









    }
}
