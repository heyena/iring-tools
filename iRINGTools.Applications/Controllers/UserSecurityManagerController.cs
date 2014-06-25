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
using org.iringtools.UserSecurity;
namespace org.iringtools.web.controllers
{
    public class UserSecurityManagerController : BaseController
    {
        //
        // GET: /UserSecurityManager/
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserSecurityManagerController));
        private SecurityRepository _repository;
        private CustomError _CustomError = null;
        private CustomErrorLog _CustomErrorLog = null;

        public UserSecurityManagerController() : this(new SecurityRepository()) { }

        public UserSecurityManagerController(SecurityRepository repository)
            : base()
        {
            _repository = repository;
            //_repository.AuthHeaders = _authHeaders;
        }

        public ActionResult Index()
        {
            return View();
            

        }

        public JsonResult getUsers()
        {
            Users user = _repository.GetAllUsers("json");
            return Json(user, JsonRequestBehavior.AllowGet);
        }

    }
}
