using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;

using org.iringtools.library;
using org.iringtools.mapping;
using log4net;
using System.Web;
using System.IO;
using org.iringtools.utility;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Web.Script.Serialization;
using System.Xml;

namespace org.iringtools.web.controllers
{
    public class DirectoryController : BaseController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DirectoryController));
        private CustomError _CustomError = null;
        private CustomErrorLog _CustomErrorLog = null;
        private AdapterRepository _repository;
        private string _keyFormat = "Mapping.{0}.{1}";

        public DirectoryController() : this(new AdapterRepository()) { }

        public DirectoryController(AdapterRepository repository)
            : base()
        {
            _repository = repository;
            _repository.AuthHeaders = _authHeaders;
        }

        public ActionResult Index()
        {
            return View(_repository.GetScopes());
        }

        public ActionResult GetNode(FormCollection form)
        {
            try
            {
                _logger.Debug("GetNode type: " + form["type"]);
                _repository.Session = Session;

                switch (form["type"])
                {
                    case "ScopesNode":
                        {
                            System.Collections.IEnumerator ie = Session.GetEnumerator();
                            while (ie.MoveNext())
                            {
                                Session.Remove(ie.Current.ToString());
                                ie = Session.GetEnumerator();
                            }

                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                            var contexts = _repository.GetScopes();

                            if (contexts != null)
                            {
                                foreach (ScopeProject scope in contexts)
                                {
                                    JsonTreeNode node = new JsonTreeNode
                                    {
                                        nodeType = "async",
                                        type = "ScopeNode",
                                        iconCls = "scope",
                                        id = scope.Name,
                                        text = scope.DisplayName,
                                        expanded = false,
                                        leaf = false,
                                        children = null,
                                        record = scope
                                    };

                                    node.property = new Dictionary<string, string>();
                                    node.property.Add("Internal Name", scope.Name);
                                    node.property.Add("Display Name", scope.DisplayName);
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
                                Configuration config = _repository.GetConfig(scope.Name, application.Name);
                                application.Configuration = config;

                                DataLayer dataLayer = _repository.GetDataLayer(scope.Name, application.Name);

                                if (dataLayer != null)
                                {
                                    JsonTreeNode node = new JsonTreeNode
                                    {
                                        nodeType = "async",
                                        type = "ApplicationNode",
                                        iconCls = "application",
                                        id = scope.Name + "/" + application.Name,
                                        text = application.DisplayName,
                                        expanded = false,
                                        leaf = false,
                                        children = null,
                                        record = new
                                        {
                                            Name = application.Name,
                                            DisplayName = application.DisplayName,
                                            Description = application.Description,
                                            DataLayer = dataLayer.Name,
                                            Assembly = dataLayer.Assembly,
                                            Configuration = application.Configuration,
                                            CacheImportURI = application.CacheInfo == null ? "" : application.CacheInfo.ImportURI,
                                            CacheTimeout = application.CacheInfo == null ? "" : Convert.ToString(application.CacheInfo.Timeout),
                                            PermissionGroups = application.PermissionGroup
                                        }
                                    };

                                    node.property = new Dictionary<string, string>();
                                    node.property.Add("Internal Name", application.Name);
                                    node.property.Add("Display Name", application.DisplayName);
                                    node.property.Add("Description", application.Description);
                                    node.property.Add("Data Layer", dataLayer.Name);
                                    node.property.Add("LightweightDataLayer", dataLayer.IsLightweight ? "Yes" : "No");

                                    nodes.Add(node);
                                }
                            }

                            ActionResult result = Json(nodes, JsonRequestBehavior.AllowGet);
                            return result;
                        }
                    case "ApplicationNode":
                        {
                            string context = form["node"];
                            string[] contextParts = context.Split(new char[] { '/' });
                            string scopeName = contextParts[0];
                            string applicationName = contextParts[1];
                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                            JsonTreeNode dataObjectsNode = new JsonTreeNode
                            {
                                nodeType = "async",
                                type = "DataObjectsNode",
                                iconCls = "folder",
                                id = context + "/DataObjects",
                                text = "Data Objects",
                                expanded = false,
                                leaf = false,
                                children = null,
                                property = new Dictionary<string, string>()
                            };

                            ScopeProject scope = _repository.GetScope(scopeName);

                            if (scope != null)
                            {
                                ScopeApplication application = scope.Applications.Find(x => x.Name.ToLower() == applicationName.ToLower());

                                if (application != null)
                                {
                                    dataObjectsNode.property.Add("Data Mode", application.DataMode.ToString());
                                }
                            }

                            JsonTreeNode graphsNode = new JsonTreeNode
                            {
                                nodeType = "async",
                                type = "GraphsNode",
                                iconCls = "folder",
                                id = context + "/Graphs",
                                text = "Graphs",
                                expanded = false,
                                leaf = false,
                                children = null
                            };

                            JsonTreeNode ValueListsNode = new JsonTreeNode
                            {
                                nodeType = "async",
                                type = "ValueListsNode",
                                iconCls = "folder",
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
                            string scopeName = context.Split('/')[0];
                            string applicationName = context.Split('/')[1];

                            Mapping mapping = GetMapping(scopeName, applicationName);

                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                            foreach (ValueListMap valueList in mapping.valueListMaps)
                            {
                                JsonTreeNode node = new JsonTreeNode
                                {
                                    nodeType = "async",
                                    type = "ValueListNode",
                                    iconCls = "valuemap",
                                    id = context + "/ValueList/" + valueList.name,
                                    text = valueList.name,
                                    expanded = false,
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
                                    id = context + "/ValueMap/" + valueMap.internalValue,
                                    text = classLabel + " [" + valueMap.internalValue + "]",
                                    expanded = false,
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
                            string dataLayer = form["datalayer"];
                            string refresh = form["refresh"];

                            if (refresh == "true")
                            {
                                Response response = _repository.Refresh(scopeName, applicationName);
                                _logger.Info(Utility.Serialize<Response>(response, true));
                            }

                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                            DataDictionary dictionary = _repository.GetDictionary(scopeName, applicationName);

                            if (dictionary != null && dictionary.dataObjects != null)
                            {
                                foreach (DataObject dataObject in dictionary.dataObjects)
                                {
                                    JsonTreeNode node = new JsonTreeNode
                                    {
                                        nodeType = "async",
                                        type = "DataObjectNode",
                                        iconCls = "treeObject",
                                        id = context + "/DataObject/" + dataObject.objectName,
                                        text = dataObject.objectName,
                                        expanded = false,
                                        leaf = false,
                                        children = null,
                                        hidden = dataObject.isHidden,
                                        record = new
                                        {
                                            Name = dataObject.objectName,
                                            DataLayer = dataLayer
                                        }
                                    };

                                    if (dataObject.isRelatedOnly)
                                    {
                                        node.hidden = true;
                                    }

                                    node.property = new Dictionary<string, string>();
                                    node.property.Add("Object Name", dataObject.objectName);
                                    node.property.Add("Is Readonly", dataObject.isReadOnly.ToString());
                                    node.property.Add("Properties Count", dataObject.dataProperties.Count.ToString());
                                    nodes.Add(node);

                                }
                            }

                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }
                    case "DataObjectNode":
                        {
                            string keyType, dataType;
                            string context = form["node"];
                            string scopeName = context.Split('/')[0];
                            string applicationName = context.Split('/')[1];
                            string dataObjectName = context.Split('/')[4];

                            DataDictionary dictionary = _repository.GetDictionary(scopeName, applicationName);
                            DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName == dataObjectName);

                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                            foreach (DataProperty property in dataObject.dataProperties)
                            {
                                keyType = getKeyType(property.propertyName, dataObject.dataProperties);
                                dataType = property.dataType.ToString();

                                bool isKeyProp = dataObject.isKeyProperty(property.propertyName);

                                JsonTreeNode node = new JsonTreeNode
                                {
                                    nodeType = "async",
                                    type = isKeyProp ? "KeyDataPropertyNode" : "DataPropertyNode",
                                    iconCls = isKeyProp ? "treeKey" : "treeProperty",
                                    id = context + "/" + dataObject.objectName + "/" + property.propertyName,
                                    text = property.propertyName,
                                    expanded = true,
                                    leaf = true,
                                    children = new List<JsonTreeNode>(),
                                    record = new
                                    {
                                        Name = property.propertyName,
                                        Keytype = keyType,
                                        Datatype = dataType
                                    }
                                };
                                node.property = new Dictionary<string, string>();
                                node.property.Add("Name", property.propertyName);

                                node.property.Add("Datatype", dataType);
                                node.property.Add("Data Length", property.dataLength.ToString());
                                //node.property.Add("isVirtual", property.isVirtual.ToString());
                                if (isKeyProp)
                                {
                                    node.property.Add("Keytype", keyType);
                                }
                                nodes.Add(node);
                            }

                            if (dataObject.dataRelationships.Count > 0)
                            {
                                foreach (DataRelationship relation in dataObject.dataRelationships)
                                {
                                    JsonTreeNode node = new JsonTreeNode
                                    {
                                        nodeType = "async",
                                        type = "RelationshipNode",
                                        iconCls = "treeRelation",
                                        id = context + "/" + dataObject.objectName + "/" + relation.relationshipName,
                                        text = relation.relationshipName,
                                        expanded = false,
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
                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                            if (!String.IsNullOrEmpty(related))
                            {
                                string scopeName = context.Split('/')[0];
                                string applicationName = context.Split('/')[1];
                                DataDictionary dictionary = _repository.GetDictionary(scopeName, applicationName);
                                DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName.ToUpper() == related.ToUpper());

                                foreach (DataProperty property in dataObject.dataProperties)
                                {
                                    keytype = getKeyType(property.propertyName, dataObject.dataProperties);
                                    datatype = property.dataType.ToString();

                                    JsonTreeNode node = new JsonTreeNode
                                    {
                                        nodeType = "async",
                                        type = (dataObject.isKeyProperty(property.propertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode",
                                        iconCls = (dataObject.isKeyProperty(property.propertyName)) ? "treeKey" : "treeProperty",
                                        id = context + "/" + property.propertyName,
                                        text = property.propertyName,
                                        expanded = true,
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
                                    nodeType = "async",
                                    type = "GraphNode",
                                    iconCls = "treeGraph",
                                    id = context + "/Graph/" + graph.name,
                                    text = graph.name,
                                    expanded = true,
                                    leaf = true,
                                    children = new List<JsonTreeNode>(),
                                    record = graph

                                };

                                ClassMap classMap = graph.classTemplateMaps[0].classMap;

                                node.property = new Dictionary<string, string>();
                                node.property.Add("Data Object", graph.dataObjectName);
                                node.property.Add("Root Class", classMap.name);
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
                //_logger.Error(e.ToString());
                //throw e;

                _logger.Error(e.ToString());
                if (e.InnerException != null)
                {
                    string description = ((System.Net.HttpWebResponse)(((System.Net.WebException)(e.InnerException)).Response)).StatusDescription;
                    var jsonSerialiser = new JavaScriptSerializer();
                    CustomError json = (CustomError)jsonSerialiser.Deserialize(description, typeof(CustomError));
                    return Json(new { success = false, message = "[ Message Id " + json.msgId + "] - " + json.errMessage, stackTraceDescription = json.stackTraceDescription }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _CustomErrorLog = new CustomErrorLog();
                    _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errGetUINode, e, _logger);
                    return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult DataLayers()
        {
            try
            {
                DataLayers dataLayers = _repository.GetDataLayers();

                JsonContainer<DataLayers> container = new JsonContainer<DataLayers>();
                container.items = dataLayers;
                container.success = true;
                container.total = dataLayers.Count;

                return Json(container, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                if (e.InnerException != null)
                {
                    string description = ((System.Net.HttpWebResponse)(((System.Net.WebException)(e.InnerException)).Response)).StatusDescription;//;
                    var jsonSerialiser = new JavaScriptSerializer();
                    CustomError json = (CustomError)jsonSerialiser.Deserialize(description, typeof(CustomError));
                    return Json(new { success = false, message = "[ Message Id " + json.msgId + "] - " + json.errMessage, stackTraceDescription = json.stackTraceDescription }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _CustomErrorLog = new CustomErrorLog();
                    _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errGetUIDataLayer, e, _logger);
                    return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                }

            }
        }

        public string DataLayer(JsonTreeNode node, FormCollection form)
        {
            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase hpf = files[0] as HttpPostedFileBase;

            string dataLayerName = string.Empty;

            if (string.IsNullOrEmpty(form["Name"]))
            {
                int lastDot = hpf.FileName.LastIndexOf(".");
                dataLayerName = hpf.FileName.Substring(0, lastDot);
            }
            else
            {
                dataLayerName = form["Name"];
            }

            DataLayer dataLayer = new DataLayer()
            {
                Name = dataLayerName,
                Package = Utility.ToMemoryStream(hpf.InputStream)
            };

            MemoryStream dataLayerStream = new MemoryStream();
            DataContractSerializer serializer = new DataContractSerializer(typeof(DataLayer));
            serializer.WriteObject(dataLayerStream, dataLayer);
            dataLayerStream.Position = 0;

            Response response = _repository.UpdateDataLayer(dataLayerStream);
            return Utility.ToJson<Response>(response);
        }

        public JsonResult Scope(FormCollection form)
        {
            try
            {
                string success = String.Empty;
                string scopeName = string.Empty;

                if (form["state"] == "new")//if (String.IsNullOrEmpty(form["scope"]))
                {
                    scopeName = form["displayName"];
                    success = _repository.AddScope(form["displayName"], form["description"], form["cacheDBConnStr"], form["permissions"]);
                }
                else
                {
                    scopeName = form["contextName"];
                    success = _repository.UpdateScope(form["contextName"], form["displayName"], form["description"], form["cacheDBConnStr"], form["permissions"]);
                }
                if (success.Trim().Contains("Error"))
                {
                    _CustomErrorLog = new CustomErrorLog();
                    _CustomError = _CustomErrorLog.getErrorResponse(success);

                    return Json(new { success = false, message = _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
                }

                List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                ScopeProject scope = _repository.GetScope(scopeName);
                JsonTreeNode node = new JsonTreeNode
                {
                    nodeType = "async",
                    type = "ScopeNode",
                    iconCls = "scope",
                    id = scope.Name,
                    text = scope.DisplayName,
                    expanded = false,
                    leaf = false,
                    children = null,
                    record = scope
                };

                node.property = new Dictionary<string, string>();
                node.property.Add("Internal Name", scope.Name);
                node.property.Add("Display Name", scope.DisplayName);
                node.property.Add("Description", scope.Description);
                nodes.Add(node);

                return Json(new { success = true, nodes }, JsonRequestBehavior.AllowGet);
                // return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                if (e.InnerException != null)
                {
                    string description = ((System.Net.HttpWebResponse)(((System.Net.WebException)(e.InnerException)).Response)).StatusDescription;//;
                    var jsonSerialiser = new JavaScriptSerializer();
                    CustomError json = (CustomError)jsonSerialiser.Deserialize(description, typeof(CustomError));
                    return Json(new { success = false, message = "[ Message Id " + json.msgId + "] - " + json.errMessage, stackTraceDescription = json.stackTraceDescription }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _CustomErrorLog = new CustomErrorLog();
                    _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errGetUIScope, e, _logger);
                    return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
                }

            }

        }

        public JsonResult Application(FormCollection form)
        {
            try
            {
                string success = String.Empty;
                string scopeName = form["Scope"];
                string cacheImportURI = form["cacheImportURI"];
                long cacheTimeout = String.IsNullOrWhiteSpace(form["cacheTimeout"]) ? 0 : Convert.ToInt64(form["cacheTimeout"]);
                string permissions = form["permissions"];

                library.Configuration configuration = new Configuration
                {
                    AppSettings = new AppSettings
                    {
                        Settings = new List<Setting>()
                    }
                };

                CacheInfo cacheInfo = new CacheInfo
                {
                    ImportURI = cacheImportURI,
                    Timeout = cacheTimeout
                };

                for (int i = 0; i < form.AllKeys.Length; i++)
                {
                    //if (form.GetKey(i).ToLower() != "scope" && form.GetKey(i).ToLower() != "name" && form.GetKey(i).ToLower() != "description" && 
                    //  form.GetKey(i).ToLower() != "assembly" && form.GetKey(i).ToLower() != "application" && form.GetKey(i).ToLower().Substring(0, 3) != "val"
                    //  && form.GetKey(i).ToLower() != "cacheimporturi" && form.GetKey(i).ToLower() != "cachetimeout")
                    if (form.GetKey(i).ToLower().StartsWith("key"))
                    {
                        String key = form[i];
                        if (i + 1 < form.AllKeys.Length)
                        {
                            String value = form[i + 1];
                            configuration.AppSettings.Settings.Add(new Setting()
                            {
                                Key = key,
                                Value = value
                            });
                        }
                    }
                }

                List<string> groups = new List<string>();
                if (permissions.Contains(","))
                {
                    string[] arrstring = permissions.Split(',');
                    groups = new List<string>(arrstring);
                }
                else
                {
                    groups.Add(permissions);
                }

                ScopeApplication application = new ScopeApplication()
                {
                    DisplayName = form["displayName"],//form["Name"],
                    Name = form["Name"],
                    Description = form["Description"],
                    Assembly = form["assembly"],
                    Configuration = configuration,
                    CacheInfo = cacheInfo,
                    PermissionGroup = new PermissionGroups()
                };
                if (!string.IsNullOrEmpty(permissions))
                    application.PermissionGroup.AddRange(groups);

                if (String.IsNullOrEmpty(form["Application"]))
                {
                    success = _repository.AddApplication(scopeName, application);
                }
                else
                {
                    success = _repository.UpdateApplication(scopeName, form["Application"], application);
                }
                if (success.Trim().Contains("Error"))
                {
                    _CustomErrorLog = new CustomErrorLog();
                    _CustomError = _CustomErrorLog.getErrorResponse(success);
                    return Json(new { success = false, message = _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
                }

                //string applicationName = form["Application"];
                List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                ScopeProject scope = _repository.GetScope(scopeName);
                ScopeApplication app = (from apps in scope.Applications
                                        where apps.Name == application.Name
                                        select apps).FirstOrDefault();

                Configuration config = _repository.GetConfig(scope.Name, app.Name);
                app.Configuration = config;

                DataLayer dataLayer = _repository.GetDataLayer(scope.Name, app.Name);

                if (dataLayer != null)
                {
                    JsonTreeNode node = new JsonTreeNode
                    {
                        nodeType = "async",
                        type = "ApplicationNode",//"appNode",
                        iconCls = "application",
                        id = scope.Name + "/" + app.Name,
                        text = app.DisplayName,
                        expanded = false,
                        leaf = false,
                        children = null,
                        record = new
                        {
                            Name = app.Name,
                            DisplayName = app.DisplayName,
                            Description = app.Description,
                            DataLayer = dataLayer.Name,
                            Assembly = dataLayer.Assembly,
                            Configuration = app.Configuration,
                            CacheImportURI = app.CacheInfo == null ? "" : app.CacheInfo.ImportURI,
                            CacheTimeout = app.CacheInfo == null ? "" : Convert.ToString(app.CacheInfo.Timeout),
                            PermissionGroups = app.PermissionGroup
                        }
                    };

                    node.property = new Dictionary<string, string>();
                    node.property.Add("Internal Name", app.Name);
                    node.property.Add("Display Name", app.DisplayName);
                    node.property.Add("Description", app.Description);
                    node.property.Add("Data Layer", dataLayer.Name);
                    node.property.Add("LightweightDataLayer", dataLayer.IsLightweight ? "Yes" : "No");


                    JsonTreeNode dataObjectsNode = new JsonTreeNode
                    {
                        nodeType = "async",
                        type = "DataObjectsNode",
                        iconCls = "folder",
                        id = scopeName + "/" + app.Name + "/DataObjects",
                        text = "Data Objects",
                        expanded = false,
                        leaf = false,
                        children = null,
                        property = new Dictionary<string, string>()
                    };

                    if (scope != null)
                    {
                        ScopeApplication scopeapplication = scope.Applications.Find(x => x.Name.ToLower() == app.Name.ToLower());

                        if (scopeapplication != null)
                        {
                            dataObjectsNode.property.Add("Data Mode", scopeapplication.DataMode.ToString());
                        }
                    }

                    JsonTreeNode graphsNode = new JsonTreeNode
                    {
                        nodeType = "async",
                        type = "GraphsNode",
                        iconCls = "folder",
                        id = scopeName + "/" + app.Name + "/Graphs",
                        text = "Graphs",
                        expanded = false,
                        leaf = false,
                        children = null
                    };

                    JsonTreeNode ValueListsNode = new JsonTreeNode
                    {
                        nodeType = "async",
                        type = "ValueListsNode",
                        iconCls = "folder",
                        id = scopeName + "/" + app.Name + "/ValueLists",
                        text = "ValueLists",
                        expanded = false,
                        leaf = false,
                        children = null
                    };

                    if (node.children == null)
                        node.children = new List<JsonTreeNode>();
                    node.children.Add(dataObjectsNode);
                    node.children.Add(graphsNode);
                    node.children.Add(ValueListsNode);

                    nodes.Add(node);
                }

                return Json(new { success = true, nodes }, JsonRequestBehavior.AllowGet);
                //JsonResult result = Json(new { success = true }, JsonRequestBehavior.AllowGet);
                //return result;
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                if (e.InnerException != null)
                {
                    string description = ((System.Net.HttpWebResponse)(((System.Net.WebException)(e.InnerException)).Response)).StatusDescription;//;
                    var jsonSerialiser = new JavaScriptSerializer();
                    CustomError json = (CustomError)jsonSerialiser.Deserialize(description, typeof(CustomError));
                    return Json(new { success = false, message = "[ Message Id " + json.msgId + "] - " + json.errMessage, stackTraceDescription = json.stackTraceDescription }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _CustomErrorLog = new CustomErrorLog();
                    _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errAddUIApplication, e, _logger);
                    return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                }
            }
        }

        public JsonResult DeleteScope(FormCollection form)
        {
            try
            {
                string success = String.Empty;
                success = _repository.DeleteScope(form["nodeid"]);
                if (success.Trim().Contains("Error"))
                {
                    _CustomErrorLog = new CustomErrorLog();
                    _CustomError = _CustomErrorLog.getErrorResponse(success);
                    return Json(new { success = false, message = _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errGetUIDeleteScope, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteApplication(FormCollection form)
        {
            try
            {
                string context = form["nodeid"];
                string scope = context.Split('/')[0];
                string application = context.Split('/')[1];
                string success = String.Empty;

                success = _repository.DeleteApplication(scope, application);
                if (success.Trim().Contains("Error"))
                {
                    _CustomErrorLog = new CustomErrorLog();
                    _CustomError = _CustomErrorLog.getErrorResponse(success);
                    return Json(new { success = false, message = _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errGetUIDeleteApplication, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SecurityGroups()
        {
            try
            {
                List<Dictionary<string, string>> lstgroups = _repository.GetSecurityGroups();
                JsonContainer<List<Dictionary<string, string>>> container = new JsonContainer<List<Dictionary<string, string>>>();
                container.items = lstgroups;
                container.success = true;
                container.total = lstgroups.Count;

                return Json(container, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                if (e.InnerException != null)
                {
                    string description = ((System.Net.HttpWebResponse)(((System.Net.WebException)(e.InnerException)).Response)).StatusDescription;//;
                    var jsonSerialiser = new JavaScriptSerializer();
                    CustomError json = (CustomError)jsonSerialiser.Deserialize(description, typeof(CustomError));
                    return Json(new { success = false, message = "[ Message Id " + json.msgId + "] - " + json.errMessage, stackTraceDescription = json.stackTraceDescription }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _CustomErrorLog = new CustomErrorLog();
                    _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errGetUISecurityGroup, e, _logger);
                    return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                }
            }
        }

        public ActionResult InitializeUISettings()
        {

            try
            {
                NameValueList nvlSettings = _repository.GetGlobalVariables();

                JsonContainer<NameValueList> container = new JsonContainer<NameValueList>();
                container.items = nvlSettings;
                container.success = true;
                container.total = nvlSettings.Count;

                return Json(container, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                if (e.InnerException != null)
                {
                    string description = ((System.Net.HttpWebResponse)(((System.Net.WebException)(e.InnerException)).Response)).StatusDescription;//;
                    var jsonSerialiser = new JavaScriptSerializer();
                    CustomError json = (CustomError)jsonSerialiser.Deserialize(description, typeof(CustomError));
                    return Json(new { success = false, message = "[ Message Id " + json.msgId + "] - " + json.errMessage, stackTraceDescription = json.stackTraceDescription }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _CustomErrorLog = new CustomErrorLog();
                    _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errGetUIInitializeUISettings, e, _logger);
                    return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                }
            }

        }

        public ActionResult getDataFilter(string scope, string app, string graph)
        {
            string keyName = string.Format("{0}.{1}.{2}", scope, app, graph);

            DataFilter filter = (DataFilter)Session[keyName];

            if (filter == null)
            {
                filter = new DataFilter();
                _repository.GetFilterFile(ref filter, keyName);
            }

            JsonContainer<DataFilter> container = new JsonContainer<DataFilter>();
            container.items = filter;
            container.success = true;
            container.total = filter.Expressions.Count + filter.OrderExpressions.Count;

            return Json(container, JsonRequestBehavior.AllowGet);
        }

        public JsonResult dataFilter(FormCollection form)
        {
            try
            {
                DataFilter gridDataFilter = new DataFilter();

                int expCount = Convert.ToInt16(form["exprCount"]);
                int oeExpCount = Convert.ToInt16(form["oeExprCount"]);

                int ec = 1;
                while (ec <= expCount)
                {
                    Expression expression = new Expression();

                    if (!string.IsNullOrEmpty(form["openGroupCount_" + ec]))
                        expression.OpenGroupCount = Convert.ToInt16(form["openGroupCount_" + ec]);

                    if (!string.IsNullOrEmpty(form["propertyName_" + ec]))
                        expression.PropertyName = form["propertyName_" + ec];

                    if (!string.IsNullOrEmpty(form["relationalOperator_" + ec]))
                        expression.RelationalOperator = (RelationalOperator)Enum.Parse(typeof(RelationalOperator), form["relationalOperator_" + ec]);

                    if (!string.IsNullOrEmpty(form["value_" + ec]))
                        expression.Values = new Values() { form["value_" + ec] };

                    if (!string.IsNullOrEmpty(form["logicalOperator_" + ec]))
                        expression.LogicalOperator = (LogicalOperator)Enum.Parse(typeof(LogicalOperator), form["logicalOperator_" + ec]);

                    if (!string.IsNullOrEmpty(form["closeGroupCount_" + ec]))
                        expression.CloseGroupCount = Convert.ToInt16(form["closeGroupCount_" + ec]);

                    gridDataFilter.Expressions.Add(expression);
                    ec++;
                }

                int oec = 1;
                while (oec <= oeExpCount)
                {
                    if (!(string.IsNullOrEmpty(form["OEProName_" + oec]) &&
                            string.IsNullOrEmpty(form["OESortOrder_" + oec])))
                    {
                        gridDataFilter.OrderExpressions.Add(
                         new OrderExpression()
                         {
                             PropertyName = form["OEProName_" + oec],
                             SortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), form["OESortOrder_" + oec]),
                         }
                       );
                    }
                    oec++;
                }

                if (form["isAdmin"].ToLower() == "false")
                {
                    gridDataFilter.isAdmin = false;
                }
                else if (form["isAdmin"].ToLower() == "on")
                {
                    gridDataFilter.isAdmin = true;
                }

                string keyName = form["reqParams"];
                if (gridDataFilter.isAdmin)
                {
                    _repository.SaveFilterFile(gridDataFilter, keyName);
                    Session.Remove(keyName);
                }
                else
                {
                    Session.Add(keyName, gridDataFilter);
                    _repository.DeleteFilterFile(keyName);
                }

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                //_logger.Error("Error in saving data Filter File: " + e.ToString());
                //throw e;
                _logger.Error("Error in saving data Filter File: " + e.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIDataFilter, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

            }
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

        private string getKeyType(string name, List<DataProperty> properties)
        {
            string keyType = string.Empty;
            keyType = properties.FirstOrDefault(p => p.propertyName == name).keyType.ToString();

            return keyType;
        }
        #endregion
    }

}

