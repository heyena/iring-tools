using System;
using System.Collections.Generic;
using System.Web.Mvc;
using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;
using log4net;
using org.iringtools.library;
using Newtonsoft.Json;
using System.Collections.Specialized;
using org.iringtools.utility;
using System.Web.Script.Serialization;

namespace org.iringtools.web.controllers
{
    public class AdapterManagerController : BaseController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterManagerController));
        private AdapterRepository _repository;
        private CustomError _CustomError = null;
        private CustomErrorLog _CustomErrorLog = null;

        public AdapterManagerController() : this(new AdapterRepository()) { }

        public AdapterManagerController(AdapterRepository repository)
            : base()
        {
            _repository = repository;
            _repository.AuthHeaders = _authHeaders;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CacheInfo(FormCollection form)
        {
            try
            {
                CacheInfo cacheInfo = _repository.GetCacheInfo(form["scope"], form["app"]);

                // NOTE: default ASP .NET serializer has issue with datetime fields, use JSON. NET library
                string json = JsonConvert.SerializeObject(cacheInfo);
                return Content(json);
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
                    _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errGetUICacheInfo, e, _logger);
                    return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                }
                //throw e;
            }
        }

        public ActionResult DBProviders()
        {
            NameValueList providers = new NameValueList();

            try
            {
                foreach (Provider provider in System.Enum.GetValues(typeof(Provider)))
                {
                    string value = provider.ToString();
                    providers.Add(new ListItem() { Name = value, Value = value });
                }

                return Json(providers, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                // return Json(new { success = false, message = e.ToString() });
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIDBProviders, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DBDictionary(FormCollection form)
        {
            try
            {
                DatabaseDictionary dbDict = _repository.GetDBDictionary(form["scope"], form["app"]);
                return Json(dbDict, JsonRequestBehavior.AllowGet);
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
                    _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIDBDictionary, e, _logger);
                    return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                }
                //throw e;
            }
        }

        public JsonResult RegenAll()
        {
            try
            {
                Response response = _repository.RegenAll();
                if (response.Level == StatusLevel.Success)
                {
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = response.Messages, stackTraceDescription = response.StatusText }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                return null;
                //throw e;
            }

        }

        public JsonResult Refresh(FormCollection form)
        {
            string context = form["nodeid"];
            string type = form["type"];
            string[] names = context.Split('/');
            string scope = names[0];
            string application = names[1];
            string dataObjectName = string.Empty;
            Response response = null;

            _repository.Session = Session;

            if (type == "one")
            {
                dataObjectName = names[2];
                response = _repository.Refresh(scope, application, dataObjectName);
            }
            else
                response = _repository.Refresh(scope, application);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SwitchDataMode(FormCollection form)
        {

            try
            {

                string context = form["nodeid"];
                string mode = form["mode"];
                string[] names = context.Split('/');
                string scopeName = names[0];
                string applicationName = names[1];

                Response response = _repository.SwitchDataMode(scopeName, applicationName, mode);

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
                nodes.Add(dataObjectsNode);

                if (response.Level == StatusLevel.Success)
                {
                    return Json(new { success = true, response, nodes }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = response.Messages, stackTraceDescription = response.StatusText }, JsonRequestBehavior.AllowGet);
                }
                //return Json(response, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUISwitchDataMode, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult RefreshCache(FormCollection form)
        {
            try
            {
                string scope = form["scope"];
                string app = form["app"];
                int timeout = int.Parse(form["timeout"]);

                Response response = _repository.RefreshCache(scope, app, timeout);

                //return Json(response, JsonRequestBehavior.AllowGet);
                return Json(new { success = true, response, JsonRequestBehavior.AllowGet });
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIRefreshCache, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult RefreshObjectCache(FormCollection form)
        {
            string context = form["nodeid"];
            string objectType = form["objectType"];
            string[] names = context.Split('/');
            string scope = names[0];
            string application = names[1];

            Response response = _repository.RefreshCache(scope, application, objectType);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ImportCache(FormCollection form)
        {
            try
            {
                string scope = form["scope"];
                string app = form["app"];
                string importURI = form["importURI"];
                int timeout = int.Parse(form["timeout"]);

                Response response = _repository.ImportCache(scope, app, importURI, timeout);

                return Json(response, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIImportCache, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteCache(FormCollection form)
        {
            string context = form["nodeid"];
            string cacheURI = form["cacheURI"];
            string[] names = context.Split('/');
            string scope = names[0];
            string application = names[1];

            Response response = _repository.DeleteCache(scope, application);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public class DBProvider
        {
            public string Provider { get; set; }
        }
    }
}
