using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using org.iringtools.library;
using log4net;
using org.iringtools.UserSecurity;
using System.Configuration;
using System.ServiceModel.Web;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using System.Net;

namespace org.iringtools.services
{
    [ServiceContract(Namespace = "http://www.iringtools.org/service")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class UserSecurityService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserSecurityService));
        private UserSecurityProvider _userSecurityProvider = null;
        private CustomError _CustomError = null;

        public UserSecurityService()
        {
            _userSecurityProvider = new UserSecurityProvider(ConfigurationManager.AppSettings);
        }

        [Description("Get all users from the database.")]
        [WebGet(UriTemplate = "/users?format={format}")]
        public void GetAllUsers(string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Users users = _userSecurityProvider.GetAllUsers();
                _userSecurityProvider.FormatOutgoingMessage<Users>(users, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert users to the database.")]
        [WebInvoke(Method = "POST", UriTemplate = "/users?format={format}")]
        public void InsertUsers(string format,Stream stream)
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    XElement xElement = _userSecurityProvider.FormatIncomingMessage<Users>(stream, format);
                    response = _userSecurityProvider.InsertUsers(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Insert users to the database.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/users?format={format}")]
        public void UpdateUsers(string format, Stream stream)
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    XElement xElement = _userSecurityProvider.FormatIncomingMessage<Users>(stream, format);
                    response = _userSecurityProvider.UpdateUsers(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Insert users to the database.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/users?userId={userId}&format={format}")]
        public void DeleteUser(int userId, string format)
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    response = _userSecurityProvider.DeleteUser(userId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Get all sites from the database.")]
        [WebGet(UriTemplate = "/sites?format={format}")]
        public void GetSites(string format) 
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Sites sites = _userSecurityProvider.GetSites();
                _userSecurityProvider.FormatOutgoingMessage<Sites>(sites, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Get site by site ID from the database.")]
        [WebGet(UriTemplate = "/site?siteId={siteId}&format={format}")]
        public void GetSite(int siteId, string format) 
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Site site = _userSecurityProvider.GetSite(siteId);
                _userSecurityProvider.FormatOutgoingMessage<Site>(site, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert sites to the data base.")]
        [WebInvoke(Method = "POST", UriTemplate = "/sites?format={format}")]
        public void InsertSites(string format, Stream stream)  
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    XElement xElement = _userSecurityProvider.FormatIncomingMessage<Sites>(stream, format);
                    response = _userSecurityProvider.InsertSite(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Update sites to the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/sites?format={format}")]
        public void UpdateSites(string format, Stream stream)
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    XElement xElement = _userSecurityProvider.FormatIncomingMessage<Sites>(stream, format);
                    response = _userSecurityProvider.UpdateSites(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Update contexts to the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/site?siteId={siteId}&format={format}")]
        public void DeleteSite(int siteId, string format) 
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    response = _userSecurityProvider.DeleteSite(siteId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }
        	 
        [Description("Get groups by group id from the database.")]
        [WebGet(UriTemplate = "/group?groupId={iGroupId}&format={format}")]
        public void GetGroupById(int iGroupId, string format)  
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Group group = _userSecurityProvider.GetGroupById(iGroupId);
                _userSecurityProvider.FormatOutgoingMessage<Group>(group, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Get groups by group id from the database.")]
        [WebGet(UriTemplate = "/user?userId={userId}&format={format}")]
        public void GetUser(int userId, string format)    
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                User user = _userSecurityProvider.GetUserById(userId);
                _userSecurityProvider.FormatOutgoingMessage<User>(user, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Get all roles from the database.")]
        [WebGet(UriTemplate = "/roles?format={format}")]
        public void GetRoles(string format) 
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Roles roles = _userSecurityProvider.GetAllRoles();
                _userSecurityProvider.FormatOutgoingMessage<Roles>(roles, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert roles to the database.")]
        [WebInvoke(Method = "POST", UriTemplate = "/roles?format={format}")]
        public void InsertRoles(string format, Stream stream)
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    XElement xElement = _userSecurityProvider.FormatIncomingMessage<Roles>(stream, format);
                    response = _userSecurityProvider.InsertRole(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Update roles in the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/roles?format={format}")]
        public void UpdateRoles(string format, Stream stream)
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    XElement xElement = _userSecurityProvider.FormatIncomingMessage<Roles>(stream, format);
                    response = _userSecurityProvider.UpdateRoles(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Delete role from the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/roles?roleId={roleId}&format={format}")]
        public void DeleteRole(int roleId, string format) 
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    response = _userSecurityProvider.DeleteRole(roleId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }


        [Description("Insert permissions to the database.")]
        [WebInvoke(Method = "POST", UriTemplate = "/permissions?format={format}")]
        public void InsertPermissions(string format, Stream stream)  
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    XElement xElement = _userSecurityProvider.FormatIncomingMessage<Permissions>(stream, format);
                    response = _userSecurityProvider.InsertPermission(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }


        [Description("Update permissions in the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/permissions?format={format}")]
        public void UpdatePermissions(string format, Stream stream)
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    XElement xElement = _userSecurityProvider.FormatIncomingMessage<Permissions>(stream, format);
                    response = _userSecurityProvider.UpdatePermissions(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Delete permission from the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/permissions?permissionId={permissionId}&format={format}")]
        public void DeletePermission(int permissionId, string format)
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    response = _userSecurityProvider.DeletePermission(permissionId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Get all groups from the database.")]
        [WebGet(UriTemplate = "/groups?format={format}")]
        public void GetGroups(string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Groups groups = _userSecurityProvider.GetAllGroups();
                _userSecurityProvider.FormatOutgoingMessage<Groups>(groups, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert groups to the database.")]
        [WebInvoke(Method = "POST", UriTemplate = "/groups?format={format}")]
        public void InsertGroups(string format, Stream stream)
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    XElement xElement = _userSecurityProvider.FormatIncomingMessage<Groups>(stream, format);
                    response = _userSecurityProvider.InsertGroup(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Update groups in the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/groups?format={format}")]
        public void UpdateGroups(string format, Stream stream)
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    XElement xElement = _userSecurityProvider.FormatIncomingMessage<Groups>(stream, format);
                    response = _userSecurityProvider.UpdateGroups(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Delete group from the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/groups?groupId={groupId}&format={format}")]
        public void DeleteGroup(int groupId, string format)
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    response = _userSecurityProvider.DeleteGroup(groupId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _userSecurityProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Get groups by group id from the database.")]
        [WebGet(UriTemplate = "/role?roleId={iroleId}&format={format}")]
        public void GetRoleById(int iroleId, string format)  
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Role role = _userSecurityProvider.GetRoleById(iroleId);
                _userSecurityProvider.FormatOutgoingMessage<Role>(role, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Get roles by site ID from the database.")]
        [WebGet(UriTemplate = "/siteRole?siteId={siteId}&format={format}")]
        public void GetSiteRoles(int siteId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Roles roles = _userSecurityProvider.GetSiteRoles(siteId);
                _userSecurityProvider.FormatOutgoingMessage<Roles>(roles, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Get roles based on groupId and roleId from the database.")]
        [WebGet(UriTemplate = "/groupRole?groupId={groupId}&roleId={roleId}&format={format}")]
        public void GetGroupRole(int groupId, int roleId, string format) 
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                GroupRoles groupRoles = _userSecurityProvider.GetGroupRole(groupId, roleId);
                _userSecurityProvider.FormatOutgoingMessage<GroupRoles>(groupRoles, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Return groups based on site from the database.")]
        [WebGet(UriTemplate = "/siteGroups?siteId={siteId}&format={format}")]
        public void GetSiteGroups(int siteId, string format) 
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Groups groups = _userSecurityProvider.GetSiteGroups(siteId);
                _userSecurityProvider.FormatOutgoingMessage<Groups>(groups, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Return all users for group from the database.")]
        [WebGet(UriTemplate = "/groupUsers?groupId={groupId}&format={format}")]
        public void GetGroupUsers(int groupId, string format) 
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Users users = _userSecurityProvider.GetGroupUsers(groupId);
                _userSecurityProvider.FormatOutgoingMessage<Users>(users, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Return all users based on site from the database.")]
        [WebGet(UriTemplate = "/siteUsers?siteId={siteId}&format={format}")]
        public void GetSiteUsers(int siteId, string format) 
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Users users = _userSecurityProvider.GetSiteUsers(siteId);
                _userSecurityProvider.FormatOutgoingMessage<Users>(users, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Return all permissions based on site from the database.")]
        [WebGet(UriTemplate = "/sitePermissions?siteId={siteId}&format={format}")]
        public void GetSitePermissions(int siteId, string format) 
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Permissions permissions = _userSecurityProvider.GetSitePermissions(siteId);
                _userSecurityProvider.FormatOutgoingMessage<Permissions>(permissions, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Return all groups that the user belongs to from the database.")]
        [WebGet(UriTemplate = "/groupsUser?userId={userId}&format={format}")]
        public void GetGroupsUser(int userId, string format) 
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Groups groups = _userSecurityProvider.GetGroupsUser(userId);
                _userSecurityProvider.FormatOutgoingMessage<Groups>(groups, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Return all users based on user id and group id from the database.")]
        [WebGet(UriTemplate = "/groupUser?groupId={groupId}&userId={userId}&format={format}")]
        public void GetGroupUser(int groupId, int userId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Users users = _userSecurityProvider.GetGroupUser(groupId, userId);
                _userSecurityProvider.FormatOutgoingMessage<Users>(users, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Return all role based on group from the database.")]
        [WebGet(UriTemplate = "/groupRoles?groupId={groupId}&format={format}")]
        public void GetGroupRoles(int groupId, string format) 
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Roles roles = _userSecurityProvider.GetGroupRoles(groupId);
                _userSecurityProvider.FormatOutgoingMessage<Roles>(roles, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Return permissions based on role from the database.")]
        [WebGet(UriTemplate = "/rolePermissions?roleId={roleId}&format={format}")]
        public void GetRolePermissions(int roleId, string format) 
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Permissions permissions = _userSecurityProvider.GetRolePermissions(roleId);
                _userSecurityProvider.FormatOutgoingMessage<Permissions>(permissions, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Return permissions based on role id and Permission id from the database.")]
        [WebGet(UriTemplate = "/rolePermission?roleId={roleId}&permissionId={permissionId}&format={format}")]
        public void GetRolePermission(int roleId, int permissionId, string format)      
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Permissions permissions = _userSecurityProvider.GetRolePermission(roleId, permissionId);
                _userSecurityProvider.FormatOutgoingMessage<Permissions>(permissions, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }



        #region Private Methods
        private string MapContentType(string format)
        {
            IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
            string contentType = request.ContentType;

            // if it's a known format then return it
            if (format != null && (format.ToLower() == "raw" || format.ToLower() == "dto" || format.ToLower() == "rdf" ||
              format.ToLower().Contains("xml") || format.ToLower().Contains("json")))
            {
                return format;
            }

            if (string.IsNullOrEmpty(format))
            {
                format = "json";
            }

            if (contentType != null)
            {
                if (contentType.ToLower().Contains("application/xml"))
                {
                    format = "xml";
                }
                else if (contentType.ToLower().Contains("json"))
                {
                    format = "json";
                }
                else
                {
                    format = "raw";
                }
            }

            return format;
        }

        private void PrepareResponse(ref Response response)
        {
            if (response.Level == StatusLevel.Success)
            {
                response.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            if (response.Messages != null)
            {
                foreach (string msg in response.Messages)
                {
                    response.StatusText += msg;
                }
            }

            foreach (Status status in response.StatusList)
            {
                foreach (string msg in status.Messages)
                {
                    response.StatusText += msg;
                }
            }
        }

        #endregion
    }
}