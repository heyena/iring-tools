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
            try
            {
                Users user = _repository.GetAllUsers("xml");
                return Json(user, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMGetAllUsers, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getGroups()
        {
            try
            {
                Groups groups = _repository.GetAllGroups("xml");
                return Json(groups, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMGetAllGroups, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getPermissions()
        {
            try
            {
                Permissions permissions = _repository.GetAllPermissions("xml");
                return Json(permissions, JsonRequestBehavior.AllowGet);

            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMGetAllPermissions, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getRoles()
        {
            try
            {
                Roles roles = _repository.GetAllRoles("xml");
                return Json(roles, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMGetAllRoles, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getGroupUsers(FormCollection form)
        {
            try
            {
                string iGroupId = form["GroupId"];
                UserGroups userGroups = _repository.GetGroupUsers(iGroupId, "xml");
                return Json(userGroups, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMGetUsersInAGroup, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
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
            try
            {
                Response response = null;
                var actionType = form["ActionType"];
                if (actionType == "ADD")
                    response = _repository.InsertGroup(form);
                else
                    response = _repository.UpdateGroup(form);

                if (response.Level == StatusLevel.Success)
                    return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMSaveGroup, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult deleteGroup(FormCollection form)
        {
            try
            {
                Response response = null;
                string iGroupId = form["GroupId"];
                response = _repository.deleteGroup(iGroupId, "xml");

                if (response.Level == StatusLevel.Success)
                    return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMSaveGroup, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult saveRole(FormCollection form)
        {
            try
            {
                Response response = null;
                var actionType = form["ActionType"];
                if (actionType == "ADD")
                    response = _repository.InsertRole(form);
                else
                    response = _repository.UpdateRole(form);

                if (response.Level == StatusLevel.Success)
                    return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMSaveRole, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult deleteRole(FormCollection form)
        {

            try
            {
                Response response = null;
                string iRoleId = form["RoleId"];
                response = _repository.DeleteRole(iRoleId, "xml");

                if (response.Level == StatusLevel.Success)
                    return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMDeleteRole, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
            
        }

        public JsonResult savePermission(FormCollection form)
        {
            try
            {
                Response response = null;
                var actionType = form["ActionType"];
                if (actionType == "ADD")
                    response = _repository.InsertPermission(form);
                else
                    response = _repository.UpdatePermission(form);

                if (response.Level == StatusLevel.Success)
                    return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMSavePermission, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult deletePermission(FormCollection form)
        {
            try
            {
                string iPermissionId = form["PermissionId"];
                Response response = _repository.DeletePermission(iPermissionId, "xml");
                if (response.Level == StatusLevel.Success)
                    return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMDeletePermission, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult saveUser(FormCollection form)
        {

            try
            {
                Response response = null;
                var actionType = form["ActionType"];
                if (actionType == "ADD")
                    response = _repository.InsertUsers(form);
                else
                    response = _repository.UpdateUsers(form);

                if (response.Level == StatusLevel.Success)
                    return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMSaveUser, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult deleteUser(FormCollection form)
        {
            try
            {
                string userName = form["UserName"];
                Response response = _repository.DeleteUser(userName, "xml");
                if (response.Level == StatusLevel.Success)
                    return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMDeleteUser, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult saveGroupUsers(FormCollection form)
        {
            try
            {
                Response response = _repository.InsertGroupUsers(form);

                if (response.Level == StatusLevel.Success)
                    return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMSaveUsersInAGroup, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
            
        }

        public JsonResult saveUserGroups(FormCollection form)
        {
            try
            {
                Response response = _repository.InsertUserGroups(form);

                if (response.Level == StatusLevel.Success)
                    return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMSaveGroup, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
            
        }

        public ActionResult GetGroupsByUser()
        {
            try
            {
                string userName = System.Web.HttpContext.Current.Session["userName"].ToString();
                //string userName = "WorldTest";
                Groups userGroups = _repository.GetGroupsByUser(userName, "xml");

                JsonContainer<Groups> container = new JsonContainer<Groups>();
                container.items = userGroups;
                container.success = true;
                container.total = userGroups.Count;

                return Json(container, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMGetGroupsInAUser, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet); 
            }
        }

        public JsonResult saveRoleGroups(FormCollection form)
        {
            try
            {
                Response response = _repository.InsertRoleGroups(form);

                if (response.Level == StatusLevel.Success)
                    return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMSaveGroupsInARole, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult getRoleGroups(FormCollection form)
        {
            try
            {
                string iRoleId = form["RoleId"];
                Groups userGroups = _repository.GetRoleGroups(iRoleId, "xml");
                return Json(userGroups, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMGetGroupsInARole, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }

            
        }

        public JsonResult getGroupRoles(FormCollection form)
        {
            try
            {
                string iGroupId = form["GroupId"];
                Roles userGroups = _repository.GetGroupRoles(iGroupId, "xml");
                return Json(userGroups, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMGetRolesInAGroup, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }

            
        }

        public JsonResult getRolePermissions(FormCollection form)
        {
            try
            {
                string iGroupId = form["RoleId"];
                Permissions rolePermissions = _repository.GetRolePermissions(iGroupId, "xml");
                return Json(rolePermissions, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMGetPermissionsInARole, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
            
        }        

        public JsonResult saveGroupRoles(FormCollection form)
        {

            try
            {
                Response response = _repository.InsertGroupRoles(form);

                if (response.Level == StatusLevel.Success)
                    return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMSaveUsersInAGroup, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult saveRolePermissions(FormCollection form)
        {
            try
            {
                Response response = _repository.InsertRolePermissions(form);

                if (response.Level == StatusLevel.Success)
                    return Json(new { success = true, message = response.StatusText }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = response.StatusText }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUSMSavePermissionsInARole, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
            
        }

    }
}
