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
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
namespace org.iringtools.web.controllers
{
    public class DirectoryController : BaseController
    {

        string userName = System.Web.HttpContext.Current.Session["userName"].ToString();
        //string userName = "WorldTest";

        int siteId;
        int platformId;

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
            platformId = int.Parse(ConfigurationManager.AppSettings["PlatformId"]);
            siteId = int.Parse(ConfigurationManager.AppSettings["SiteId"]);
            _repository = repository;
            _repository.AuthHeaders = _authHeaders;
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
            try
            {
                Response response = null;
                Guid destinationId = Guid.Parse(form["parentEntityId"]);
                Guid sourceId = Guid.Parse(form["entityId"]);
                string resourceType = form["nodeType"].Replace("Node", string.Empty);
                int platformId = int.Parse(form["platformId"]);
                int siteId = int.Parse(form["siteId"]);
                string displayName = form["displayName"];
                response = _appConfigRepository.DragAndDropEntity(resourceType, sourceId, destinationId, siteId, platformId);
                return Json(new { success = true, message = response.StatusList[0].Messages[0] }, JsonRequestBehavior.AllowGet);
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
                    _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errDragNDropEntity, e, _logger);
                    return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                }
            }

        }

        public JsonResult Application(FormCollection form)
        {
            try
            {
                Response response = null;
                List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                dynamic record = Utility.DeserializeJson<org.iringtools.applicationConfig.Context>(form["record"].ToString(), true);

                string success = String.Empty;

                AppSettings applicationSettings = new AppSettings
                {
                    Settings = new List<Setting>()
                };

                if (form["applicationSettings"] != null)
                {
                    applicationSettings.Settings.AddRange(Utility.DeserializeJson<List<Setting>>(form["applicationSettings"], true));
                }

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
                    ContextId = record.ContextId,
                    ApplicationSettings = applicationSettings
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

        public JsonResult GetNodesForCache(FormCollection form)
        {
            try
            { 
                Guid nodeId = Guid.Parse(form["guid"]);
                var nodeType = form["type"];

                string currentNodesChildren = HttpUtility.HtmlDecode(_appConfigRepository.GetNodesForCache(nodeType, nodeId, userName));
                XDocument cacheNodesDocument = XDocument.Parse(currentNodesChildren);
                
                Applications applicationList = new Applications();
                List<DatabaseDictionary> databaseDictionaryList = new List<DatabaseDictionary>();
                
                foreach (XElement xElement in cacheNodesDocument.Root.Elements().First().Elements())
                {
                    if (xElement.Name.LocalName == "application")
                    {
                         applicationList.Add(Utility.Deserialize<Application>(xElement.ToString(), true));
                    }
                    else
                    {
                         databaseDictionaryList.Add(Utility.Deserialize<DatabaseDictionary>(xElement.ToString(), true));
                    }
                }

                return Json(new { success = true, message = "nodesFetched", applicationList, databaseDictionaryList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                //TODO: NEED TO HANDLE EXCEPTIONS PROPERLY
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

        public ActionResult GetDataFilter(FormCollection form)
        {
            var record = Utility.DeserializeJson<DataObject>(form["record"].ToString(), true);

            DataFilter dataFilter = record.dataFilter;

            if (dataFilter == null)
            {
                dataFilter = _appConfigRepository.GetDataFilter(record.dataObjectId);
                record.dataFilter = dataFilter;
            }

            JsonContainer<DataFilter> container = new JsonContainer<DataFilter>();
            container.items = dataFilter;
            container.success = true;
            container.total = dataFilter.Expressions.Count + dataFilter.OrderExpressions.Count;

            return Json(container, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveDataFilter(FormCollection form)
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
            string response = null;
            try
            {
                
                //string success = String.Empty;
                string displayname = form["contextName"];
                string applications = form["applicationName"];
                string dataObjects = form["dataObjectName"]; ;
                string startDate = form["startDate"];
                string occurence = form["occuranceRadio"];
                string startTime = form["startTime"];
                string endTime = form["endTime"];
                string endDate = form["endDate"];

                startDate = startDate + " " + startTime;
                endDate = endDate + " " + endTime;
                Guid guid = Guid.Empty;
                response = _repository.AddSchedular(guid, 0, displayname, applications, dataObjects, null, null, startDate, endDate, occurence,platformId,siteId,userName);
                if (response.Contains("jobadded"))
                { response = "Job Added Successfuly"; }
                else
                { response = "Duplicate Job"; }


            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                _logger.Error("Error in saving Job for scheduler : " + e.ToString());
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIDataFilter, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + "Error in saving Job for scheduler", stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, message = response }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllJob()
        {
            AgentLibrary.Agent.Jobs result = null;
            try
            {
                result = _repository.getAllScheduleJob(platformId, siteId, userName);

            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
            }

            return Json(result, JsonRequestBehavior.AllowGet);
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


        public JsonResult SwitchDataMode(FormCollection form)
        {

            try
            {
                Response response = null;

                Application tempApplication = Utility.DeserializeJson<org.iringtools.applicationConfig.Application>(form["record"].ToString(), true);
                 
                tempApplication.ApplicationDataMode = tempApplication.ApplicationDataMode == applicationConfig.DataMode.Cache ? applicationConfig.DataMode.Live : applicationConfig.DataMode.Cache;
             
                response = _appConfigRepository.UpdateApplication(tempApplication);

                var record = Utility.SerializeJson<org.iringtools.applicationConfig.Application>(tempApplication, true);

                if (response.Level == StatusLevel.Success)
                {
                    return Json(new { success = true, message = response.StatusText, record }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = response.StatusText, stackTraceDescription = response.StatusText }, JsonRequestBehavior.AllowGet);
                }
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUISwitchDataMode, e, _logger);
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

        //     private Mapping GetMapping(string scope, string application)
        private Graphs GetMappingOnAppId(string userName, Guid applicationId)
        {
            string key = string.Format(_keyFormat, userName, applicationId);

            if (Session[key] == null)
            {
                Session[key] = _repository.GetMappingOnAppId(userName, applicationId);
            }

            return (Graphs)Session[key];
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
                    record = Utility.SerializeJson<org.iringtools.applicationConfig.Graph>(graph, true),
                    GraphMap = (GraphMap)DeserializeObject(graph.graph)
                };

                GraphMap graphMap = (GraphMap)DeserializeObject(graph.graph);
                ClassMap classMap = graphMap.classTemplateMaps[0].classMap;
                node.property = new Dictionary<string, string>();
                node.property.Add("Data Object", graphMap.dataObjectName);
                node.property.Add("Root Class", classMap.name);

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
                    //id = valueListMap.valueMaps.ToString(),
                    text = valueListMap.name,
                    id = "valuelistId-" + valueListMap.ValueListMapId.ToString(),
                    expanded = false,
                    leaf = false,
                    children = null,
                   record = Utility.SerializeJson<ValueListMap>(valueListMap, true)
                };

                valueListMapsNode.children.Add(node);


                //value Map
                
                List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                
                if (node.children == null)
                {
                    node.children = new List<JsonTreeNode>();
                }

                if (valueListMap.valueMaps != null)
                {
                    foreach (ValueMap valueMap in valueListMap.valueMaps)
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

                        JsonTreeNode node1 = new JsonTreeNode
                        {
                            nodeType = "async",
                            type = "ListMapNode",
                            iconCls = "valuelistmap",
                            id = "/ValueMap/" + valueMap.ValueMapId.ToString(),
                            text = classLabel + " [" + valueMap.internalValue + "]",
                            expanded = false,
                            leaf = true,
                            children = null,
                            record = valueMap
                        };

                        node1.property = new Dictionary<string, string>();
                        node1.property.Add("Name", valueMap.internalValue);
                        node1.property.Add("Class Label", classLabel);
                        node.children.Add(node1);



                    }
                }



                //end of value map





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

            foreach (library.DataProperty dataProperty in dataObject.dataProperties)
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

                    relationshipNode.children = new List<JsonTreeNode>();

                    foreach (library.PropertyMap propMap in relation.propertyMaps)
                    {
                        JsonTreeNode propMapNode = new JsonTreeNode
                        {
                            nodeType = "async",
                            type = (dataObject.isKeyProperty(propMap.relatedPropertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode",
                            iconCls = (dataObject.isKeyProperty(propMap.relatedPropertyName)) ? "treeKey" : "treeProperty",
                            id = propMap.relatedPropertyName + propMap.relationshipId.ToString(),
                            text = propMap.dataPropertyName,
                            expanded = true,
                            leaf = true,
                            children = new List<JsonTreeNode>(),
                            record = Utility.SerializeJson<library.PropertyMap>(propMap, true)
                        };

                        relationshipNode.children.Add(propMapNode);
                    }

                    node.children.Add(relationshipNode);
                }
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

        #endregion
    }
}