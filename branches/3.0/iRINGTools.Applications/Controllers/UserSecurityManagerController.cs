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
using System.Net;
using System.IO;
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
            ServiceSettings _settings = new ServiceSettings();
            _settings.Get("UserName");
            return View();
        }

        public JsonResult getUsers()
        {
         //Users user = _repository.GetAllUsers("json");
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

        public JsonResult getGroupUsers(FormCollection form)
        {
            string iGroupId = form["GroupId"];
            UserGroups userGroups = _repository.GetGroupUsers(iGroupId,"xml");
            return Json(userGroups, JsonRequestBehavior.AllowGet);
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
            var actionType = form["ActionType"];
            if (actionType == "ADD") {
                _repository.InsertGroup(form);
            }
            else{
                _repository.UpdateGroup(form);
            }
         
            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult deleteGroup(FormCollection form)
        {
            string iGroupId = form["GroupId"];
            //  Group group = _repository.getGroupById(iGroupId ,"json");
            Group group = _repository.deleteGroup(iGroupId, "xml");
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult saveRole(FormCollection form)
        {
            var actionType = form["ActionType"];
            if (actionType == "ADD")
            {
                _repository.InsertRole(form);
            }
            else
            {
                _repository.UpdateRole(form);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult deleteRole(FormCollection form)
        {
            string iRoleId = form["RoleId"];
            Role role = _repository.DeleteRole(iRoleId, "xml");
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult savePermission(FormCollection form)
        {
            var actionType = form["ActionType"];
            if (actionType == "ADD")
            {
                _repository.InsertPermission(form);
            }
            else
            {
                _repository.UpdatePermission(form);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult deletePermission(FormCollection form)
        {
            string iPermissionId = form["PermissionId"];
            Permission permission = _repository.DeletePermission(iPermissionId, "xml");
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult saveUser(FormCollection form)
        {
            var actionType = form["ActionType"];
            if (actionType == "ADD")
            {
                _repository.InsertUsers(form);
            }
            else
            {
                _repository.UpdateUsers(form);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult deleteUser(FormCollection form)
        {
            string iUserId = form["UserId"];
            User user = _repository.DeleteUser(iUserId, "xml");
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult saveGroupUsers(FormCollection form)
        {
            _repository.InsertGroupUsers(form);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult saveUserGroups(FormCollection form)
        {
            _repository.InsertUserGroups(form);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getUserGroups(FormCollection form)
        {
            string iUserId = form["UserId"];
            Groups userGroups = _repository.GetUserGroups(iUserId, "xml");
            return Json(userGroups, JsonRequestBehavior.AllowGet);
        }

        public JsonResult saveRoleGroups(FormCollection form)
        {
            _repository.InsertRoleGroups(form);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getRoleGroups(FormCollection form)
        {
            string iRoleId = form["RoleId"];
            Groups userGroups = _repository.GetRoleGroups(iRoleId, "xml");
            return Json(userGroups, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getGroupRoles(FormCollection form)
        {
            string iGroupId = form["GroupId"];
            Roles userGroups = _repository.GetGroupRoles(iGroupId, "xml");
            return Json(userGroups, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getRolePermissions(FormCollection form)
        {
            string iGroupId = form["RoleId"];
            Permissions rolePermissions = _repository.GetRolePermissions(iGroupId, "xml");
            return Json(rolePermissions, JsonRequestBehavior.AllowGet);
        }

        

        public JsonResult saveGroupRoles(FormCollection form)
        {
            _repository.InsertGroupRoles(form);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult saveRolePermissions(FormCollection form)
        {
            _repository.InsertRolePermissions(form);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

    }
}
