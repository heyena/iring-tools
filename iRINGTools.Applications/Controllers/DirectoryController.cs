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
using org.iringtools.applicationConfig;
using org.iringtools.UserSecurity;

namespace org.iringtools.web.controllers
{
    public class DirectoryController : BaseController
    {

        string userName = System.Web.HttpContext.Current.Session["userName"].ToString();
            int siteId = 4;
        int platformId = 2;

        private static readonly ILog _logger = LogManager.GetLogger(typeof(DirectoryController));
        private CustomError _CustomError = null;
        private CustomErrorLog _CustomErrorLog = null;
        private AdapterRepository _repository;
        private ApplicationConfigurationRepository _appConfigRepository = new ApplicationConfigurationRepository();
        private SecurityRepository _securityRepository = new SecurityRepository();
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
                    case "WorldNode":
                        {
                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                            Sites sites = _securityRepository.GetSitesbyUser(userName, siteId);

                            foreach (Site site in sites)
                            {
                                JsonTreeNode node = new JsonTreeNode
                                {
                                    nodeType = "async",
                                    type = "SiteNode",
                                    iconCls = "site",
                                    id = site.SiteId.ToString(),
                                    text = site.SiteName,
                                    expanded = false,
                                    leaf = false,
                                    children = null,
                                    record = Utility.SerializeJson<Site>(site, true)
                                };

                                nodes.Add(node);
                            }

                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }
                    case "SiteNode":
                        {
                            var record = Utility.DeserializeJson<Site>(form["record"].ToString(), true);

                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                            Folders folders = _appConfigRepository.GetFolders(userName, record.SiteId, platformId, Guid.Empty);

                            foreach (Folder folder in folders)
                            {
                                JsonTreeNode node = new JsonTreeNode
                                {
                                    nodeType = "async",
                                    type = "FolderNode",
                                    iconCls = "folder",
                                    id = folder.FolderId.ToString(),
                                    text = folder.FolderName,
                                    expanded = false,
                                    leaf = false,
                                    children = null,
                                    record = Utility.SerializeJson<Folder>(folder, true)
                                };

                                nodes.Add(node);
                            }

                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }
                    case "FolderNode":
                        {
                            var record = Utility.DeserializeJson<Folder>(form["record"].ToString(), true);

                            List<JsonTreeNode> nodes = PopulateFolderNode(record.SiteId, record.FolderId);

                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }
                    case "ContextNode":
                        {
                            var record = Utility.DeserializeJson<org.iringtools.applicationConfig.Context>(form["record"].ToString(), true);

                            List<JsonTreeNode> nodes = PopulateApplicationNodes(record.ContextId);

                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }
                    case "ApplicationNode":
                        {
                            var record = Utility.DeserializeJson<Application>(form["record"].ToString(), true);

                            List<JsonTreeNode> nodes = PopulateApplicationChildrenNodes(record.ApplicationId);

                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }
                    //case "ValueListsNode":
                    //    {
                    //        string context = form["node"];
                    //        string scopeName = context.Split('/')[0];
                    //        string applicationName = context.Split('/')[1];

                    //        Mapping mapping = GetMapping(scopeName, applicationName);

                    //        List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                    //        foreach (org.iringtools.mapping.ValueListMap valueList in mapping.valueListMaps)
                    //        {
                    //            JsonTreeNode node = new JsonTreeNode
                    //            {
                    //                nodeType = "async",
                    //                type = "ValueListNode",
                    //                iconCls = "valuemap",
                    //                id = context + "/ValueList/" + valueList.name,
                    //                text = valueList.name,
                    //                expanded = false,
                    //                leaf = false,
                    //                children = null,
                    //                record = valueList
                    //            };
                    //            node.property = new Dictionary<string, string>();
                    //            node.property.Add("Name", valueList.name);
                    //            nodes.Add(node);
                    //        }

                    //        return Json(nodes, JsonRequestBehavior.AllowGet);
                    //    }
                    //case "ValueListNode":
                    //    {
                    //        string context = form["node"];
                    //        string scopeName = context.Split('/')[0];
                    //        string applicationName = context.Split('/')[1];
                    //        string valueList = context.Split('/')[4];

                    //        List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                    //        Mapping mapping = GetMapping(scopeName, applicationName);
                    //        org.iringtools.mapping.ValueListMap valueListMap = mapping.valueListMaps.Find(c => c.name == valueList);

                    //        foreach (var valueMap in valueListMap.valueMaps)
                    //        {
                    //            string classLabel = String.Empty;

                    //            if (!String.IsNullOrEmpty(valueMap.uri))
                    //            {
                    //                string valueMapUri = valueMap.uri.Split(':')[1];

                    //                if (!String.IsNullOrEmpty(valueMap.label))
                    //                {
                    //                    classLabel = valueMap.label;
                    //                }
                    //                else if (Session[valueMapUri] != null)
                    //                {
                    //                    classLabel = (string)Session[valueMapUri];
                    //                }
                    //                else
                    //                {
                    //                    classLabel = GetClassLabel(valueMapUri);
                    //                    Session[valueMapUri] = classLabel;
                    //                }
                    //            }

                    //            JsonTreeNode node = new JsonTreeNode
                    //            {
                    //                nodeType = "async",
                    //                type = "ListMapNode",
                    //                iconCls = "valuelistmap",
                    //                id = context + "/ValueMap/" + valueMap.internalValue,
                    //                text = classLabel + " [" + valueMap.internalValue + "]",
                    //                expanded = false,
                    //                leaf = true,
                    //                children = null,
                    //                record = valueMap
                    //            };

                    //            node.property = new Dictionary<string, string>();
                    //            node.property.Add("Name", valueMap.internalValue);
                    //            node.property.Add("Class Label", classLabel);
                    //            nodes.Add(node);
                    //        }

                    //        return Json(nodes, JsonRequestBehavior.AllowGet);
                    //    }

                    //case "DataObjectsNode":
                    //    {
                    //        string context = form["node"];
                    //        string scopeName = context.Split('/')[0];
                    //        string applicationName = context.Split('/')[1];
                    //        string dataLayer = form["datalayer"];
                    //        string refresh = form["refresh"];

                    //        if (refresh == "true")
                    //        {
                    //            Response response = _repository.Refresh(scopeName, applicationName);
                    //            _logger.Info(Utility.Serialize<Response>(response, true));
                    //        }

                    //        List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                    //        org.iringtools.library.DataDictionary dictionary = _repository.GetDictionary(scopeName, applicationName);

                    //        if (dictionary != null && dictionary.dataObjects != null)
                    //        {
                    //            foreach (org.iringtools.library.DataObject dataObject in dictionary.dataObjects)
                    //            {
                    //                JsonTreeNode node = new JsonTreeNode
                    //                {
                    //                    nodeType = "async",
                    //                    type = "DataObjectNode",
                    //                    iconCls = "treeObject",
                    //                    id = context + "/DataObject/" + dataObject.objectName,
                    //                    text = dataObject.objectName,
                    //                    expanded = false,
                    //                    leaf = false,
                    //                    children = null,
                    //                    hidden = dataObject.isHidden,
                    //                    record = new
                    //                    {
                    //                        Name = dataObject.objectName,
                    //                        DataLayer = dataLayer
                    //                    }
                    //                };

                    //                if (dataObject.isRelatedOnly)
                    //                {
                    //                    node.hidden = true;
                    //                }

                    //                node.property = new Dictionary<string, string>();
                    //                node.property.Add("Object Name", dataObject.objectName);
                    //                node.property.Add("Is Readonly", dataObject.isReadOnly.ToString());
                    //                node.property.Add("Properties Count", dataObject.dataProperties.Count.ToString());
                    //                nodes.Add(node);

                    //            }
                    //        }

                    //        return Json(nodes, JsonRequestBehavior.AllowGet);
                    //    }
                    //case "DataObjectNode":
                    //    {
                    //        string keyType, dataType;
                    //        string context = form["node"];
                    //        string scopeName = context.Split('/')[0];
                    //        string applicationName = context.Split('/')[1];
                    //        string dataObjectName = context.Split('/')[4];

                    //        org.iringtools.library.DataDictionary dictionary = _repository.GetDictionary(scopeName, applicationName);
                    //        org.iringtools.library.DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName == dataObjectName);

                    //        List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                    //        foreach (org.iringtools.library.DataProperty property in dataObject.dataProperties)
                    //        {
                    //            keyType = getKeyType(property.propertyName, dataObject.dataProperties);
                    //            dataType = property.dataType.ToString();

                    //            bool isKeyProp = dataObject.isKeyProperty(property.propertyName);

                    //            JsonTreeNode node = new JsonTreeNode
                    //            {
                    //                nodeType = "async",
                    //                type = isKeyProp ? "KeyDataPropertyNode" : "DataPropertyNode",
                    //                iconCls = isKeyProp ? "treeKey" : "treeProperty",
                    //                id = context + "/" + dataObject.objectName + "/" + property.propertyName,
                    //                text = property.propertyName,
                    //                expanded = true,
                    //                leaf = true,
                    //                children = new List<JsonTreeNode>(),
                    //                record = new
                    //                {
                    //                    Name = property.propertyName,
                    //                    Keytype = keyType,
                    //                    Datatype = dataType
                    //                }
                    //            };
                    //            node.property = new Dictionary<string, string>();
                    //            node.property.Add("Name", property.propertyName);

                    //            node.property.Add("Datatype", dataType);
                    //            node.property.Add("Data Length", property.dataLength.ToString());
                    //            node.property.Add("Precison", property.precision.ToString());
                    //            node.property.Add("Scale", property.scale.ToString());

                    //            if (isKeyProp)
                    //            {
                    //                node.property.Add("Keytype", keyType);
                    //            }
                    //            nodes.Add(node);
                    //        }
                    //        if (dataObject.extensionProperties != null)
                    //        {
                    //            if (dataObject.extensionProperties.Count > 0)
                    //            {
                    //                foreach (org.iringtools.library.ExtensionProperty extension in dataObject.extensionProperties)
                    //                {
                    //                    JsonTreeNode node = new JsonTreeNode
                    //                    {
                    //                        nodeType = "async",
                    //                        type = "ExtensionNode",
                    //                        iconCls = "treeExtension",
                    //                        id = context + "/" + dataObject.objectName + "/" + extension.propertyName,
                    //                        text = extension.propertyName,
                    //                        expanded = false,
                    //                        leaf = true,
                    //                        children = null,
                    //                        record = new
                    //                        {
                    //                            Name = extension.propertyName,
                    //                        }
                    //                    };
                    //                    node.property = new Dictionary<string, string>();
                    //                    node.property.Add("Name", extension.propertyName);
                    //                    node.property.Add("Datatype", extension.dataType.ToString());
                    //                    node.property.Add("Data Length", extension.dataLength.ToString());
                    //                    node.property.Add("Precison", extension.precision.ToString());
                    //                    node.property.Add("Scale", extension.scale.ToString());
                    //                    nodes.Add(node);
                    //                }
                    //            }
                    //        }
                    //        // Ends]]
                    //        if (dataObject.dataRelationships.Count > 0)
                    //        {
                    //            foreach (org.iringtools.library.DataRelationship relation in dataObject.dataRelationships)
                    //            {
                    //                JsonTreeNode node = new JsonTreeNode
                    //                {
                    //                    nodeType = "async",
                    //                    type = "RelationshipNode",
                    //                    iconCls = "treeRelation",
                    //                    id = context + "/" + dataObject.objectName + "/" + relation.relationshipName,
                    //                    text = relation.relationshipName,
                    //                    expanded = false,
                    //                    leaf = false,
                    //                    children = null,
                    //                    record = new
                    //                    {
                    //                        Name = relation.relationshipName,
                    //                        Type = relation.relationshipType,
                    //                        Related = relation.relatedObjectName
                    //                    }
                    //                };
                    //                node.property = new Dictionary<string, string>();
                    //                node.property.Add("Name", relation.relationshipName);
                    //                node.property.Add("Type", relation.relationshipType.ToString());
                    //                node.property.Add("Related", relation.relatedObjectName);
                    //                nodes.Add(node);
                    //            }
                    //        }

                    //        return Json(nodes, JsonRequestBehavior.AllowGet);
                    //    }

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
                                org.iringtools.library.DataDictionary dictionary = _repository.GetDictionary(scopeName, applicationName);
                                org.iringtools.library.DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName.ToUpper() == related.ToUpper());

                                foreach (org.iringtools.library.DataProperty property in dataObject.dataProperties)
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

                    //case "GraphsNode":
                    //    {

                    //        string context = form["node"];
                    //        string scopeName = context.Split('/')[0];
                    //        string applicationName = context.Split('/')[1];

                    //        Mapping mapping = GetMapping(scopeName, applicationName);

                    //        List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                    //        foreach (GraphMap graph in mapping.graphMaps)
                    //        {
                    //            JsonTreeNode node = new JsonTreeNode
                    //            {
                    //                nodeType = "async",
                    //                type = "GraphNode",
                    //                iconCls = "treeGraph",
                    //                id = context + "/Graph/" + graph.name,
                    //                text = graph.name,
                    //                expanded = true,
                    //                leaf = true,
                    //                children = new List<JsonTreeNode>(),
                    //                record = graph

                    //            };

                    //            ClassMap classMap = graph.classTemplateMaps[0].classMap;

                    //            node.property = new Dictionary<string, string>();
                    //            node.property.Add("Data Object", graph.dataObjectName);
                    //            node.property.Add("Root Class", classMap.name);
                    //            nodes.Add(node);
                    //        }

                    //        return Json(nodes, JsonRequestBehavior.AllowGet);

                    //    }
                    default:
                        {
                            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                        }
                }
            }
            catch (Exception e)
            {
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

        public ActionResult GetDataLayers()
        {
            try
            {
                DataLayers dataLayers = _appConfigRepository.GetDataLayers(siteId, platformId);

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

                    //scopeName = form["displayName"];
                    scopeName = form["internalName"];
                    //  success = _repository.AddScope(form["displayName"], form["description"], form["cacheDBConnStr"], form["Permissions"]);
                    success = _repository.AddScope(form["internalName"], form["description"], form["cacheDBConnStr"], form["Permissions"], form["displayName"]);


                }
                else
                {
                    scopeName = form["contextName"];
                    success = _repository.UpdateScope(form["contextName"], form["displayName"], form["description"], form["cacheDBConnStr"], form["Permissions"]);
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

        public JsonResult Folder(FormCollection form)
        {
            try
            {
                Response response = null;

                dynamic record;
                Guid parentId;

                record = Utility.DeserializeJson<Site>(form["record"].ToString(), true);
                parentId = Guid.Empty;

                if (record.SiteName == null)
                {
                    record = Utility.DeserializeJson<Folder>(form["record"].ToString(), true);
                    parentId = record.FolderId;
                }

                Folder tempFolder = new Folder()
                {
                    FolderName = form["displayName"],
                    SiteId = record.SiteId,
                    PlatformId = platformId
                };

                if (form["state"] == "new")
                {
                    tempFolder.ParentFolderId = parentId;
                    tempFolder.Groups.AddRange(GetSelectedGroups(form["ResourceGroups"]));

                    response = _appConfigRepository.AddFolder(tempFolder);
                }
                else if (form["state"] == "edit")
                {
                    tempFolder.FolderId = record.FolderId;
                    tempFolder.ParentFolderId = record.ParentFolderId;
                    tempFolder.Groups.AddRange(GetSelectedGroups(form["ResourceGroups"]));

                    response = _appConfigRepository.UpdateFolder(tempFolder);
                }
                else if (form["state"] == "delete")
                {
                    tempFolder.FolderId = record.FolderId;
                    tempFolder.ParentFolderId = record.ParentFolderId;

                    response = _appConfigRepository.DeleteFolder(tempFolder);
                }

                if (response.Level == StatusLevel.Success)
                {
                    List<JsonTreeNode> nodes = PopulateFolderNode(tempFolder.SiteId, tempFolder.ParentFolderId);
                    return Json(new { success = true, message = response.StatusText, nodes }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = response.StatusText, stackTraceDescription = response.StatusText }, JsonRequestBehavior.AllowGet);
                }

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

        public JsonResult Context(FormCollection form)
        {
            try
            {
                int folderSiteId = 0;

                Response response = null;
                dynamic record = Utility.DeserializeJson<Folder>(form["record"].ToString(), true);

                if (record.FolderName == null)
                {
                    record = Utility.DeserializeJson<org.iringtools.applicationConfig.Context>(form["record"].ToString(), true);
                    folderSiteId = int.Parse(form["siteId"]);
                }

                applicationConfig.Context tempContext = new applicationConfig.Context()
                {
                    DisplayName = form["displayName"],
                    InternalName = form["internalName"],
                    Description = form["description"],
                    CacheConnStr = form["cacheDBConnStr"],
                    FolderId = record.FolderId
                };

                if (form["state"] == "new")
                {
                    folderSiteId = record.SiteId;
                    tempContext.Groups.AddRange(GetSelectedGroups(form["ResourceGroups"]));

                    response = _appConfigRepository.AddContext(tempContext);
                }
                else if (form["state"] == "edit")
                {
                    tempContext.ContextId = record.ContextId;
                    tempContext.Groups.AddRange(GetSelectedGroups(form["ResourceGroups"]));

                    response = _appConfigRepository.UpdateContext(tempContext);
                }
                else if (form["state"] == "delete")
                {
                    tempContext.ContextId = record.ContextId;

                    response = _appConfigRepository.DeleteContext(tempContext);
                }

                if (response.Level == StatusLevel.Success)
                {
                    List<JsonTreeNode> nodes = PopulateFolderNode(folderSiteId, tempContext.FolderId);

                    return Json(new { success = true, message = response.StatusText, nodes }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = response.StatusText, stackTraceDescription = response.StatusText }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
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
                    _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errGetUIScope, e, _logger);
                    return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult DragAndDropEntity(FormCollection form)
        {

            Response response = null;
            Guid destinationParentEntityId = Guid.Parse(form["parentEntityId"]);
            Guid droppedEntityId = Guid.Parse(form["entityId"]);
            string resourceType = form["nodeType"].Replace("Node", string.Empty);
            int platformId = int.Parse(form["platformId"]);
            int siteId = int.Parse(form["siteId"]);
            string displayName = form["displayName"];
            response = _appConfigRepository.DragAndDropEntity(resourceType, droppedEntityId, destinationParentEntityId, siteId, platformId);
            //return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            return Json(new { success = true, message = "nodeCopied" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Application(FormCollection form)
        {
            try
            {
                Response response = null;
                List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                dynamic record = Utility.DeserializeJson<org.iringtools.applicationConfig.Context>(form["record"].ToString(), true);

                string success = String.Empty;

                library.Configuration configuration = new Configuration
                {
                    AppSettings = new AppSettings
                    {
                        Settings = new List<Setting>()
                    }
                };

                // TODO: Need to change the name of property (and its attribute) DisplayName to ContextName in Context class
                if (record.CacheConnStr == null)
                {
                    record = Utility.DeserializeJson<org.iringtools.applicationConfig.Application>(form["record"].ToString(), true);
                }

                Application tempApplication = new Application()
                {
                    DisplayName = form["displayName"],//form["Name"],
                    InternalName = form["internalName"],
                    Description = form["description"],
                    Assembly = form["dataLayerCombo"],
                    DXFRUrl = "http://localhost:56789/dxfr",
                    ContextId = record.ContextId
                };

                if (form["state"] == "new")
                {
                    tempApplication.Groups.AddRange(GetSelectedGroups(form["ResourceGroups"]));

                    response = _appConfigRepository.AddApplication(tempApplication);
                    nodes = PopulateApplicationNodes(tempApplication.ContextId);
                }
                else if (form["state"] == "edit")
                {
                    tempApplication.ApplicationId = record.ApplicationId;
                    tempApplication.Groups.AddRange(GetSelectedGroups(form["ResourceGroups"]));

                    response = _appConfigRepository.UpdateApplication(tempApplication);
                    nodes = PopulateApplicationNodes(tempApplication.ContextId);
                }
                else if (form["State"] == "delete")
                {
                    tempApplication.ApplicationId = record.ApplicationId;

                    response = _appConfigRepository.DeleteApplication(tempApplication);
                    nodes = PopulateApplicationNodes(tempApplication.ContextId);
                }

                if (response.Level == StatusLevel.Success)
                {
                    return Json(new { success = true, message = response.StatusText, nodes }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = response.StatusText, stackTraceDescription = response.StatusText }, JsonRequestBehavior.AllowGet);
                }
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


        public JsonResult GetAppSettings(FormCollection form)
        {
            try
            {
                ApplicationSettings settings = _appConfigRepository.GetAppSettings();

                JsonContainer<ApplicationSettings> container = new JsonContainer<ApplicationSettings>();
                container.items = settings;
                container.success = true;
                container.total = settings.Count;

                return Json(container, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMGetGroupsInAUser, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
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

            org.iringtools.library.DataFilter filter = (org.iringtools.library.DataFilter)Session[keyName];

            if (filter == null)
            {
                filter = new org.iringtools.library.DataFilter();
                _repository.GetFilterFile(ref filter, keyName);
            }

            JsonContainer<org.iringtools.library.DataFilter> container = new JsonContainer<org.iringtools.library.DataFilter>();
            container.items = filter;
            container.success = true;
            container.total = filter.Expressions.Count + filter.OrderExpressions.Count;

            return Json(container, JsonRequestBehavior.AllowGet);
        }

        public JsonResult dataFilter(FormCollection form)
        {
            try
            {
                org.iringtools.library.DataFilter gridDataFilter = new org.iringtools.library.DataFilter();

                int expCount = Convert.ToInt16(form["exprCount"]);
                int oeExpCount = Convert.ToInt16(form["oeExprCount"]);

                int ec = 1;
                while (ec <= expCount)
                {
                    org.iringtools.library.Expression expression = new org.iringtools.library.Expression();

                    if (!string.IsNullOrEmpty(form["openGroupCount_" + ec]))
                        expression.OpenGroupCount = Convert.ToInt16(form["openGroupCount_" + ec]);

                    if (!string.IsNullOrEmpty(form["propertyName_" + ec]))
                        expression.PropertyName = form["propertyName_" + ec];

                    if (!string.IsNullOrEmpty(form["relationalOperator_" + ec]))
                        expression.RelationalOperator = (org.iringtools.library.RelationalOperator)Enum.Parse(typeof(org.iringtools.library.RelationalOperator), form["relationalOperator_" + ec]);

                    if (!string.IsNullOrEmpty(form["value_" + ec]))
                        expression.Values = new org.iringtools.library.Values() { form["value_" + ec] };
                    else
                        expression.Values = new org.iringtools.library.Values() { string.Empty };

                    if (!string.IsNullOrEmpty(form["logicalOperator_" + ec]))
                        expression.LogicalOperator = (org.iringtools.library.LogicalOperator)Enum.Parse(typeof(org.iringtools.library.LogicalOperator), form["logicalOperator_" + ec]);

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
                         new org.iringtools.library.OrderExpression()
                         {
                             PropertyName = form["OEProName_" + oec],
                             SortOrder = (org.iringtools.library.SortOrder)Enum.Parse(typeof(org.iringtools.library.SortOrder), form["OESortOrder_" + oec]),
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

        public ActionResult saveSchedularData(FormCollection form)
        {

            try
            {
                string success = String.Empty;
                string displayname = form["displayName"];
                string applications = form["applications"];
                string dataObjects = form["dataobj"];
                string startDate = form["startDate"];
                string occurence = form["occrad"];
                string startTime = form["startTime"];
                string endTime = form["endTime"];
                string endDate = form["endDate"];

                startDate = startDate + " " + startTime;
                endDate = endDate + " " + endTime;


                //   Guid guid = Guid.Parse("00000000-0000-0000-0000-000000000000");
                Guid guid = Guid.Empty;

                string schcontextName = form["displayName"];

                //  success = _repository.AddSchedular(guid, 0, displayname, applications, dataObjects, null, null, Convert.ToDateTime(startDate), Convert.ToDateTime(endDate), "daily", "sunday");
                success = _repository.AddSchedular(guid, 0, displayname, applications, dataObjects, null, null, startDate, endDate, occurence, 0, 0);

            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllJob()
        {
            {
                AgentLibrary.Agent.Jobs result = null;
                try
                {
                    result = _repository.getAllScheduleJob(2, 3);

                }
                catch (Exception e)
                {
                    _logger.Error(e.ToString());
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult getScheduleJob(FormCollection form)
        {
            {
                AgentLibrary.Agent.Job result = null;
                try
                {
                    result = _repository.getScheduleJob(Guid.Parse(form["jobId"]));

                }
                catch (Exception e)
                {
                    _logger.Error(e.ToString());
                }

                return Json(result, JsonRequestBehavior.AllowGet);
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

        private string getKeyType(string name, List<org.iringtools.library.DataProperty> properties)
        {
            string keyType = string.Empty;
            keyType = properties.FirstOrDefault(p => p.propertyName == name).keyType.ToString();

            return keyType;
        }

        private List<Group> GetSelectedGroups(string Groups)
        {
            List<Group> tempGroups = new List<Group>();

            string[] groupArray = Groups.Split(',');

            for (int i = 0; i < groupArray.Length; i++)
            {
                tempGroups.Add(new Group() { GroupId = int.Parse(groupArray[i]) });
            }

            return tempGroups;
        }

        private List<JsonTreeNode> PopulateFolderNode(int siteId, Guid folderId)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
            Folders folders = _appConfigRepository.GetFolders(userName, siteId, platformId, folderId);
            org.iringtools.applicationConfig.Contexts contexts = _appConfigRepository.GetContexts(userName, folderId);

            foreach (Folder folder in folders)
            {
                JsonTreeNode node = new JsonTreeNode
                {
                    nodeType = "async",
                    type = "FolderNode",
                    iconCls = "folder",
                    id = folder.FolderId.ToString(),
                    text = folder.FolderName,
                    expanded = false,
                    leaf = false,
                    children = null,
                    record = Utility.SerializeJson<Folder>(folder, true)
                };

                nodes.Add(node);
            }

            foreach (org.iringtools.applicationConfig.Context context in contexts)
            {
                JsonTreeNode node = new JsonTreeNode
                {
                    nodeType = "async",
                    type = "ContextNode",
                    iconCls = "context",
                    id = context.ContextId.ToString(),
                    text = context.DisplayName,
                    expanded = false,
                    leaf = false,
                    children = null,
                    record = Utility.SerializeJson<org.iringtools.applicationConfig.Context>(context, true)
                };

                node.property = new Dictionary<string, string>();
                node.property.Add("siteId", siteId.ToString());
                nodes.Add(node);
            }

            return nodes;
        }

        private List<JsonTreeNode> PopulateApplicationNodes(Guid contextId)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
            org.iringtools.applicationConfig.Applications applications = _appConfigRepository.GetApplications(userName, contextId);

            foreach (Application application in applications)
            {
                JsonTreeNode node = new JsonTreeNode
                {
                    nodeType = "async",
                    type = "ApplicationNode",
                    iconCls = "tabsApplication",
                    id = application.ApplicationId.ToString(),
                    text = application.DisplayName,
                    expanded = false,
                    leaf = false,
                    children = null,
                    record = Utility.SerializeJson<Application>(application, true)
                };

                nodes.Add(node);
            }

            return nodes;
        }

        private List<JsonTreeNode> PopulateApplicationChildrenNodes(Guid applicationId)
        {
            List<JsonTreeNode> applicationNodes = new List<JsonTreeNode>();

            org.iringtools.library.DataDictionary dataDictionary = _appConfigRepository.GetDictionary(applicationId);
            org.iringtools.applicationConfig.Graphs graphs = _appConfigRepository.GetGraphs(userName, applicationId);
            ValueListMaps valueListMaps = _appConfigRepository.GetValueListMaps(userName, applicationId);

            JsonTreeNode dataObjectsNode = new JsonTreeNode()
            {
                nodeType = "async",
                type = "DataObjectsNode",
                iconCls = "folder",
                id = "DataObjectsNode-" + applicationId.ToString(),
                text = "Data Objects",
                expanded = false,
                leaf = false,
                record = Utility.SerializeJson<org.iringtools.library.DataDictionary>(dataDictionary, true)
            };

            foreach (org.iringtools.library.DataObject dataObject in dataDictionary.dataObjects)
            {
                if (dataObjectsNode.children == null)
                {
                    dataObjectsNode.children = new List<JsonTreeNode>();
                }

                JsonTreeNode node = new JsonTreeNode
                {
                    nodeType = "async",
                    type = "DataObjectNode",
                    iconCls = "treeObject",
                    id = dataObject.dataObjectId.ToString(),
                    text = dataObject.objectName,
                    expanded = false,
                    leaf = false,
                    children = null,
                    record = Utility.SerializeJson<org.iringtools.library.DataObject>(dataObject, true)
                };

                dataObjectsNode.children.Add(node);

                PopulatePropertiesNodes(ref node, dataObject);
            }

            applicationNodes.Add(dataObjectsNode);

            JsonTreeNode graphsNode = new JsonTreeNode()
            {
                nodeType = "async",
                type = "GraphsNode",
                iconCls = "folder",
                id = "GraphsNode-" + applicationId.ToString(),
                text = "Graphs",
                expanded = false,
                leaf = false,
                record = Utility.SerializeJson<org.iringtools.applicationConfig.Graphs>(graphs, true)
            };

            foreach (org.iringtools.applicationConfig.Graph graph in graphs)
            {
                if (graphsNode.children == null)
                {
                    graphsNode.children = new List<JsonTreeNode>();
                }

                JsonTreeNode node = new JsonTreeNode
                {
                    nodeType = "async",
                    type = "GraphNode",
                    iconCls = "treeGraph",
                    id = graph.GraphId.ToString(),
                    text = graph.GraphName,
                    expanded = false,
                    leaf = false,
                    children = null,
                    record = Utility.SerializeJson<org.iringtools.applicationConfig.Graph>(graph, true)
                };

                graphsNode.children.Add(node);
            }

            applicationNodes.Add(graphsNode);

            JsonTreeNode valueListMapsNode = new JsonTreeNode()
            {
                nodeType = "async",
                type = "ValueListsNode",
                iconCls = "folder",
                id = "ValueListMapsNode-" + applicationId.ToString(),
                text = "ValueLists",
                expanded = false,
                leaf = false,
                record = Utility.SerializeJson<ValueListMaps>(valueListMaps, true)
            };


            foreach (ValueListMap valueListMap in valueListMaps)
            {
                if (valueListMapsNode.children == null)
                {
                    valueListMapsNode.children = new List<JsonTreeNode>();
                }

                JsonTreeNode node = new JsonTreeNode
                {
                    nodeType = "async",
                    type = "ValueListNode",
                    iconCls = "valuemap",
                    id = valueListMap.valueMaps.ToString(),
                    text = valueListMap.name,
                    expanded = false,
                    leaf = false,
                    children = null,
                    record = Utility.SerializeJson<ValueListMap>(valueListMap, true)
                };

                valueListMapsNode.children.Add(node);
            }

            applicationNodes.Add(valueListMapsNode);
            
            return applicationNodes;
        }

        private void PopulatePropertiesNodes(ref JsonTreeNode node, library.DataObject dataObject)
        {
            List<JsonTreeNode> propertiesNodes = new List<JsonTreeNode>();

            if (node.children == null)
            {
                node.children = new List<JsonTreeNode>();
            }

            foreach(library.DataProperty dataProperty in dataObject.dataProperties)
            {
                bool isKeyProp = dataObject.isKeyProperty(dataProperty.propertyName);

                JsonTreeNode propNode = new JsonTreeNode
                {
                    nodeType = "async",
                    type = isKeyProp ? "KeyDataPropertyNode" : "DataPropertyNode",
                    iconCls = isKeyProp ? "treeKey" : "treeProperty",
                    id = dataProperty.dataPropertyId.ToString(),
                    text = dataProperty.propertyName,
                    expanded = false,
                    leaf = true,
                    children = null,
                    record = Utility.SerializeJson<library.DataProperty>(dataProperty, true)
                };

                node.children.Add(propNode);
            }

            if (dataObject.extensionProperties != null)
            {
                foreach (library.ExtensionProperty extensionProp in dataObject.extensionProperties)
                {
                    JsonTreeNode extensionNode = new JsonTreeNode
                    {
                        nodeType = "async",
                        type = "ExtensionNode",
                        iconCls = "treeExtension",
                        id = extensionProp.extensionPropertyId.ToString(),
                        text = extensionProp.propertyName,
                        expanded = false,
                        leaf = true,
                        children = null,
                        record = Utility.SerializeJson<library.ExtensionProperty>(extensionProp, true)
                    };

                    node.children.Add(extensionNode);
                }
            }

            if (dataObject.dataRelationships != null)
            {
                foreach (org.iringtools.library.DataRelationship relation in dataObject.dataRelationships)
                {
                    JsonTreeNode relationshipNode = new JsonTreeNode
                    {
                        nodeType = "async",
                        type = "RelationshipNode",
                        iconCls = "treeRelation",
                        id = relation.relationshipId.ToString(),
                        text = relation.relationshipName,
                        expanded = false,
                        leaf = false,
                        children = null,
                        record = Utility.SerializeJson<library.DataRelationship>(relation, true)
                    };
                    //relationshipNode.property = new Dictionary<string, string>();
                    //relationshipNode.property.Add("Name", relation.relationshipName);
                    //relationshipNode.property.Add("Type", relation.relationshipType.ToString());
                    //relationshipNode.property.Add("Related", relation.relatedObjectName);

                    node.children.Add(relationshipNode);
                }
            }
        }

        #endregion
    }
}