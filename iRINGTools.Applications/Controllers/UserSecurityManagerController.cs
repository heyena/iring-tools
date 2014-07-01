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
         //   Users user = _repository.GetAllUsers("json");
            Users user = _repository.GetAllUsers("xml");
            return Json(user, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getGroups()
        {
         //   Groups groups = _repository.GetAllGroups("json");
            Groups groups = _repository.GetAllGroups("xml");
            return Json(groups, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getPermissions()
        {
            //Permissions permissions = _repository.GetAllPermissions("json");
            Permissions permissions = _repository.GetAllPermissions("xml");
            return Json(permissions, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getRoles()
        {
          //  Roles roles = _repository.GetAllRoles("json");
            Roles roles = _repository.GetAllRoles("xml");
            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getGroupById(FormCollection form)
        {
            string iGroupId = form["GroupId"];
          //  Group group = _repository.getGroupById(iGroupId ,"json");
            Group group = _repository.getGroupById(iGroupId, "xml");
            return Json(group, JsonRequestBehavior.AllowGet);
        }

        public JsonResult saveGroup(FormCollection form)
        {
            Response response = null;
         
            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult deleteGroup(FormCollection form)
        {
            Response response = null;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult saveRole(FormCollection form)
        {
            Response response = null;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult deleteRole(FormCollection form)
        {
            Response response = null;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult savePermission(FormCollection form)
        {
            Response response = null;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult deletePermission(FormCollection form)
        {
            Response response = null;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult insertUsers(FormCollection form)
        {
            _repository.InsertUsers(form);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


    }
}
