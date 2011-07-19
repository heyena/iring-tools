using System.Collections.Generic;
using System.Web.Mvc;
using iRINGTools.Web.Models;
using org.iringtools.library;
using org.iringtools.adapter.security;
using iRINGTools.Web.Helpers;
using System;
using System.IO;
using log4net;
using System.Configuration;
using System.Collections;
using org.iringtools.utility;

namespace org.iringtools.web.controllers
{
  public abstract class BaseController : Controller
  {
    protected IAuthenticationLayer _authenticationLayer = new OAuthProvider();
    protected IDictionary _allClaims = new Dictionary<string, string>();
    protected string _oAuthToken = String.Empty;
    protected IAuthorizationLayer _authorizationLayer = new LdapAuthorizationProvider();
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseController));

    public BaseController()
		{
      string enableOAuth = ConfigurationManager.AppSettings["EnableOAuth"];

      if (!String.IsNullOrEmpty(enableOAuth) && enableOAuth.ToUpper() == "TRUE")
      {
        try
        {
          _authenticationLayer.Authenticate(ref _allClaims, ref _oAuthToken);
        }
        catch (Exception e)
        {
          _logger.Error(e.ToString());
        }

        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string ldapConfigFilePath = baseDirectory + @"App_Data\ldap.conf";

        if (System.IO.File.Exists(ldapConfigFilePath))
        {
          Properties ldapConfig = new Properties();
          ldapConfig.Load(ldapConfigFilePath);
          ldapConfig["authorizedGroup"] = "adapterAdmins";
          _authorizationLayer.Init(ldapConfig);

          if (!_authorizationLayer.IsAuthorized(_allClaims))
          {
            throw new UnauthorizedAccessException("User not authorized to access AdapterManager.");
          }
        }
        else
        {
          _logger.Warn("LDAP Configuration is missing!");
        }
      }
		}
  }

	public class AdapterManagerController : BaseController 
	{
		private AdapterRepository _repository;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterManagerController));
    
		public AdapterManagerController() : this(new AdapterRepository()) { }

		public AdapterManagerController(AdapterRepository repository) : base()
		{
			_repository = repository;
		}

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult DBProviders()
		{
			DataProviders dataProviders = _repository.GetDBProviders();

			List<DBProvider> providers = new List<DBProvider>();
			foreach (Provider dataProvider in dataProviders)
			{
				providers.Add(new DBProvider() { Provider = dataProvider.ToString() });
			}

			JsonContainer<List<DBProvider>> container = new JsonContainer<List<DBProvider>>();
			container.items = providers;
			container.success = true;
			container.total = dataProviders.Count;

			return Json(container, JsonRequestBehavior.AllowGet);
		}

		public ActionResult DBDictionary(FormCollection form)
		{
			DatabaseDictionary dbDict = _repository.GetDBDictionary(form["scope"], form["app"]);
			return Json(dbDict, JsonRequestBehavior.AllowGet);
		}

		public ActionResult TableNames(FormCollection form)
		{
			List<string> dataObjects = _repository.GetTableNames(
				form["scope"], form["app"], form["dbProvider"], form["dbServer"], form["dbInstance"],
				form["dbName"], form["dbSchema"], form["dbUserName"], form["dbPassword"], form["portNumber"]);

			JsonContainer<List<string>> container = new JsonContainer<List<string>>();
			container.items = dataObjects;
			container.success = true;
			container.total = dataObjects.Count;

			return Json(container, JsonRequestBehavior.AllowGet);
		}

		public ActionResult DBObjects(FormCollection form)
		{
			List<JsonTreeNode> dbObjects = _repository.GetDBObjects(
				form["scope"], form["app"], form["dbProvider"], form["dbServer"], form["dbInstance"],
				form["dbName"], form["dbSchema"], form["dbUserName"], form["dbPassword"], form["tableNames"], form["portNumber"]);

			return Json(dbObjects, JsonRequestBehavior.AllowGet);
		}


		public ActionResult Trees(FormCollection form)
		{
			string response = string.Empty;
			
			response = _repository.SaveDBDictionary(form["scope"], form["app"], form["tree"]);

			if (response != null && response.ToUpper().Contains("ERROR"))
			{
				int inds = response.ToUpper().IndexOf("<MESSAGE>");
				int inde = response.ToUpper().IndexOf(";");
				string msg = response.Substring(inds + 9, inde - inds - 13);
				return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = true }, JsonRequestBehavior.AllowGet);
		}


		public class DBProvider
		{
			public string Provider { get; set; }
		}
	}
}
