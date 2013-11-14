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

namespace org.iringtools.web.controllers
{
    public class AdapterManagerController : BaseController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterManagerController));
        private AdapterRepository _repository;

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
                throw e;
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
                return Json(new { success = false, message = e.ToString() });
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
                throw e;
            }
        }

        public JsonResult RegenAll()
        {
            Response response = _repository.RegenAll();
            return Json(response, JsonRequestBehavior.AllowGet);
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
            string context = form["nodeid"];
            string mode = form["mode"];
            string[] names = context.Split('/');
            string scope = names[0];
            string application = names[1];

            Response response = _repository.SwitchDataMode(scope, application, mode);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RefreshCache(FormCollection form)
        {
            string scope = form["scope"];
            string app = form["app"];
            int timeout = int.Parse(form["timeout"]);

            Response response = _repository.RefreshCache(scope, app, timeout);

            return Json(response, JsonRequestBehavior.AllowGet);
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
            string scope = form["scope"];
            string app = form["app"];
            string importURI = form["importURI"];
            int timeout = int.Parse(form["timeout"]);

            Response response = _repository.ImportCache(scope, app, importURI, timeout);

            return Json(response, JsonRequestBehavior.AllowGet);
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
