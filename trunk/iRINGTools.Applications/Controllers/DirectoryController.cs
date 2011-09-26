using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;

using org.iringtools.library;
using org.iringtools.mapping;
using log4net;

namespace org.iringtools.web.controllers
{
    public class DirectoryController : BaseController
    {

        IAdapterRepository _repository;
        private string _keyFormat = "Mapping.{0}.{1}";
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DirectoryController));

        public DirectoryController()
            : this(new AdapterRepository())
        {
        }

        public DirectoryController(IAdapterRepository repository)
        {
            _repository = repository;
        }

        public ActionResult Index()
        {
            return View(_repository.GetScopes());
        }

        public ActionResult GetNode(FormCollection form)
        {
            try
            {
                _logger.Debug("Starting Switch block");
                _logger.Debug(form["type"]);

                switch (form["type"])
                {
                    case "ScopesNode":
                        {
                            _logger.Debug("In ScopesNode case block");

                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                            if (_repository.GetScopes() != null)
                            {
                                foreach (ScopeProject scope in _repository.GetScopes())
                                {
                                    _logger.Debug("Scope: " + scope.Name);

                                    JsonTreeNode node = new JsonTreeNode
                                    {
                                        type = "ScopeNode",
                iconCls = "scope",
                                        id = scope.Name,
                                        text = scope.Name,
                                      //  expanded = false,
                                        leaf = false,
                                        children = null,
                                        record = scope
                                    };

                                    node.property = new Dictionary<string, string>();
                                    node.property.Add("Name", scope.Name);
                                    node.property.Add("Description", scope.Description);
                                    nodes.Add(node);
                                }
                            }

                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }
                    case "ScopeNode":
                        {
                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                            ScopeProject scope = _repository.GetScope(form["node"]);

                            foreach (ScopeApplication application in scope.Applications)
                            {
                                DataLayer dataLayer = _repository.GetDataLayer(scope.Name, application.Name);

                                JsonTreeNode node = new JsonTreeNode
                                {
                                    type = "ApplicationNode",
                iconCls = "application",
                                    id = scope.Name + "/" + application.Name,
                                    text = application.Name,
                                 //   expanded = false,
                                    leaf = false,
                                    children = null,
                                    record = new
                                    {
                                        Name = application.Name,
                                        Description = application.Description,
                                        DataLayer = dataLayer.Name,
                                        Assembly = dataLayer.Assembly
                                    }
                                };
                                node.property = new Dictionary<string, string>();
                                node.property.Add("Name", application.Name);
                                node.property.Add("Description", application.Description);
                                node.property.Add("Data Layer", dataLayer.Name);
                                nodes.Add(node);

                            }

                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }
                    case "ApplicationNode":
                        {
                            string context = form["node"];

                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                            JsonTreeNode dataObjectsNode = new JsonTreeNode
                            {
                                type = "DataObjectsNode",
              iconCls = "folder",
                                id = context + "/DataObjects",
                                text = "Data Objects",
                             //   expanded = false,
                                leaf = false,
                                children = null
                            };


                            JsonTreeNode graphsNode = new JsonTreeNode
                            {
                                type = "GraphsNode",
              iconCls = "folder",
                                id = context + "/Graphs",
                                text = "Graphs",
                            //    expanded = false,
                                leaf = false,
                                children = null
                            };

                            JsonTreeNode ValueListsNode = new JsonTreeNode
                            {
                                type = "ValueListsNode",
              iconCls = "folder",
                                id = context + "/ValueLists",
                                text = "ValueLists",
                           //     expanded = false,
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
                            string scopeName = context.Split('/')[0];
                            string applicationName = context.Split('/')[1];

                            Mapping mapping = GetMapping(scopeName, applicationName);

                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                            foreach (ValueListMap valueList in mapping.valueListMaps)
                            {
                                JsonTreeNode node = new JsonTreeNode
                                {
                                    type = "ValueListNode",
                iconCls = "valuelistmap",
                                    id = context + "/ValueList/" + valueList.name,
                                    text = valueList.name,
                              //      expanded = false,
                                    leaf = false,
                                    children = null,
                                    record = valueList
                                };
                                node.property = new Dictionary<string, string>();
                                node.property.Add("Name", valueList.name);
                                nodes.Add(node);
                            }

                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }
                    case "ValueListNode":
                        {
                            string context = form["node"];
                            string scopeName = context.Split('/')[0];
                            string applicationName = context.Split('/')[1];
                            string valueList = context.Split('/')[4];

                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                            Mapping mapping = GetMapping(scopeName, applicationName);
                            ValueListMap valueListMap = mapping.valueListMaps.Find(c => c.name == valueList);

                            foreach (var valueMap in valueListMap.valueMaps)
                            {
                                string valueMapUri = valueMap.uri.Split(':')[1];
                                string classLabel = String.Empty;

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

                                JsonTreeNode node = new JsonTreeNode
                                {
                                    type = "ListMapNode",
                iconCls = "valuemap",
                                    id = context + "/ValueMap/" + valueMap.internalValue,
                                    text = classLabel + " [" + valueMap.internalValue + "]",
                                 //   expanded = false,
                                    leaf = true,
                                    children = null,
                                    record = valueMap
                                };

                                node.property = new Dictionary<string, string>();
                                node.property.Add("Name", valueMap.internalValue);
                                node.property.Add("Class Label", classLabel);
                                nodes.Add(node);
                            }

                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }

                    case "DataObjectsNode":
                        {
                            string context = form["node"];
                            string scopeName = context.Split('/')[0];
                            string applicationName = context.Split('/')[1];
                            DataDictionary dictionary = _repository.GetDictionary(scopeName, applicationName);

                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                            foreach (DataObject dataObject in dictionary.dataObjects)
                            {
                                JsonTreeNode node = new JsonTreeNode
                                {
                                    type = "DataObjectNode",
                iconCls = "object",
                                    id = context + "/DataObject/" + dataObject.objectName,
                                    text = dataObject.objectName,
                                //    expanded = false,
                                    leaf = false,
                                    children = null,
                                    record = new
                                    {
                                        Name = dataObject.objectName
                                    }
                                };
                                node.property = new Dictionary<string, string>();
                                node.property.Add("Name", dataObject.objectName);
                                nodes.Add(node);

                            }
                            return Json(nodes, JsonRequestBehavior.AllowGet);

                        }
                    case "DataObjectNode":
                        {
                            string datatype, keytype;
                            string context = form["node"];
                            string scopeName = context.Split('/')[0];
                            string applicationName = context.Split('/')[1];
                            string dataObjectName = context.Split('/')[4];

                            DataDictionary dictionary = _repository.GetDictionary(scopeName, applicationName);
                            DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName == dataObjectName);

                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                            foreach (DataProperty property in dataObject.dataProperties)
                            {
                                keytype = getKeytype(property.propertyName, dataObject.dataProperties);
                                datatype = getDatatype(property.propertyName, dataObject.dataProperties);
                                JsonTreeNode node = new JsonTreeNode
                                {
                                    type = (dataObject.isKeyProperty(property.propertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode",
                iconCls = (dataObject.isKeyProperty(property.propertyName)) ? "key" : "property",
                                    id = context + "/" + property.propertyName,
                                    text = property.propertyName,
                                //    expanded = true,
                                    leaf = true,
                                    children = new List<JsonTreeNode>(),
                                    record = new
                                    {
                                        Name = property.propertyName,
                                        Keytype = keytype,
                                        Datatype = datatype
                                    }
                                };
                                node.property = new Dictionary<string, string>();
                                node.property.Add("Name", property.propertyName);
                                node.property.Add("Keytype", keytype);
                                node.property.Add("Datatype", datatype);
                                nodes.Add(node);
                            }
                            if (dataObject.dataRelationships.Count > 0)
                            {
                                foreach (DataRelationship relation in dataObject.dataRelationships)
                                {
                                    JsonTreeNode node = new JsonTreeNode
                                    {
                                        type = "RelationshipNode",
                  iconCls = "relation",
                                        id = context + "/" + dataObject.objectName + "/" + relation.relationshipName,
                                        text = relation.relationshipName,
                                  //      expanded = false,
                                        leaf = false,
                                        children = null,
                                        record = new
                                        {
                                            Name = relation.relationshipName,
                                            Type = relation.relationshipType,
                                            Related = relation.relatedObjectName
                                        }
                                    };
                                    node.property = new Dictionary<string, string>();
                                    node.property.Add("Name", relation.relationshipName);
                                    node.property.Add("Type", relation.relationshipType.ToString());
                                    node.property.Add("Related", relation.relatedObjectName);
                                    nodes.Add(node);
                                }

                            }
                            return Json(nodes, JsonRequestBehavior.AllowGet);

                        }

                    case "RelationshipNode":
                        {
                            string keytype, datatype;
                            string context = form["node"];
                            string related = form["related"];
                            string scopeName = context.Split('/')[0];
                            string applicationName = context.Split('/')[1];
                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                            DataDictionary dictionary = _repository.GetDictionary(scopeName, applicationName);
                            DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName.ToUpper() == related.ToUpper());
                            foreach (DataProperty property in dataObject.dataProperties)
                            {
                                keytype = getKeytype(property.propertyName, dataObject.dataProperties);
                                datatype = getDatatype(property.propertyName, dataObject.dataProperties);
                                JsonTreeNode node = new JsonTreeNode
                                {
                                    type = (dataObject.isKeyProperty(property.propertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode",
                iconCls = (dataObject.isKeyProperty(property.propertyName)) ? "key" : "property",
                                    id = context + "/" + property.propertyName,
                                    text = property.propertyName,
                               //     expanded = true,
                                    leaf = true,
                                    children = new List<JsonTreeNode>(),
                                    record = new
                                    {
                                        Name = property.propertyName,
                                        Keytype = keytype,
                                        Datatype = datatype
                                    }
                                };
                                node.property = new Dictionary<string, string>();
                                node.property.Add("Name", property.propertyName);
                                node.property.Add("Type", keytype);
                                node.property.Add("Related", datatype);
                                nodes.Add(node);
                            }
                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }

                    case "GraphsNode":
                        {
                            string context = form["node"];
                            string scopeName = context.Split('/')[0];
                            string applicationName = context.Split('/')[1];

                            Mapping mapping = GetMapping(scopeName, applicationName);

                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                            foreach (GraphMap graph in mapping.graphMaps)
                            {
                                JsonTreeNode node = new JsonTreeNode
                                {
                                    type = "GraphNode",
                iconCls = "graphmap",
                                    id = context + "/Graph/" + graph.name,
                                    text = graph.name,
                              //      expanded = true,
                                    leaf = true,
                                    children = new List<JsonTreeNode>(),
                                    record = graph

                                };
                                node.property = new Dictionary<string, string>();
                                node.property.Add("Data Object Name", graph.dataObjectName);
                                node.property.Add("Name", graph.name);
                                node.property.Add("Identifier", graph.classTemplateMaps[0].classMap.identifiers[0].Split('.')[1]);
                                node.property.Add("Class Label", graph.classTemplateMaps[0].classMap.name);
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
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw e;
            }
        }

        public ActionResult DataLayers()
        {
            DataLayers dataLayers = _repository.GetDataLayers();

            JsonContainer<DataLayers> container = new JsonContainer<DataLayers>();
            container.items = dataLayers;
            container.success = true;
            container.total = dataLayers.Count;

            return Json(container, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Scope(FormCollection form)
        {
            string success = _repository.UpdateScope(form["scope"], form["name"], form["description"]);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Application(FormCollection form)
        {
            #region Works in case of New Application
            string name = string.Empty;
            if (form["Application"] == string.Empty)
            {
                name = form["Name"];
            }
            else
            {
                name = form["Application"];
            }
            #endregion

            string success = _repository.UpdateApplication(form["Scope"], name, form["Name"], form["Description"], form["assembly"]);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteScope(FormCollection form)
        {
            _repository.DeleteScope(form["nodeid"]);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteApplication(FormCollection form)
        {
            string context = form["nodeid"];
            string scope = context.Split('/')[0];
            string application = context.Split('/')[1];

            _repository.DeleteApplication(scope, application);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        #region Private Methods

        private Mapping GetMapping(string scope, string application)
        {
            string key = string.Format(_keyFormat, scope, application);

            if (Session[key] == null)
            {
                Session[key] = _repository.GetMapping(scope, application);
            }

            return (Mapping)Session[key];
        }

        private string GetClassLabel(string classId)
        {
            Entity dataEntity = _repository.GetClassLabel(classId);

            return Convert.ToString(dataEntity.Label);
        }

        private string getKeytype(string name, List<DataProperty> properties)
        {
            string keyType = string.Empty;
            keyType = properties.FirstOrDefault(p => p.propertyName == name).keyType.ToString();

            return keyType;
        }
        private string getDatatype(string name, List<DataProperty> properties)
        {
            string dataType = string.Empty;
            dataType = properties.FirstOrDefault(p => p.propertyName == name).dataType.ToString();

            return dataType;
        }
        #endregion
    }

}

