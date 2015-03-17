using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using iRINGTools.Web.Helpers;
using log4net;
using Microsoft.ServiceModel.Web;
using org.iringtools.library;
using org.iringtools.mapping;
using org.iringtools.utility;
using org.iringtools.UserSecurity;
using System.Web.Mvc;

namespace iRINGTools.Web.Models
{
    public class SecurityRepository : IsecurityRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SecurityRepository));
        private CustomError _CustomError = null;
        protected ServiceSettings _settings;
        protected string _proxyHost;
        protected string _proxyPort;
        protected string _adapterServiceUri = null;
        protected string _dataServiceUri = null;
        protected string _hibernateServiceUri = null;
        protected string _referenceDataServiceUri = null;
        protected string _servicesBasePath = string.Empty;

        public SecurityRepository()
        {
            NameValueCollection settings = ConfigurationManager.AppSettings;

            _settings = new ServiceSettings();
            _settings.AppendSettings(settings);

            _proxyHost = _settings["ProxyHost"];
            _proxyPort = _settings["ProxyPort"];

            _adapterServiceUri = _settings["SecurityUri"];
            if (_adapterServiceUri.EndsWith("/"))
                _adapterServiceUri = _adapterServiceUri.Remove(_adapterServiceUri.Length - 1);


        }

        protected WebHttpClient CreateWebClient(string baseUri)
        {
            WebHttpClient client = null;
            client = new WebHttpClient(baseUri);
            return client;
        }

        public Users GetAllUsers(string format)
        {
            Users items = null;
            _logger.Debug("In SecurityRepository getAllUsers");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                items = client.Get<Users>("/users?format=" + format);  

                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;

            }
            return items;
        }

        public Groups GetAllGroups(string format)
        {
            Groups items = null;
            _logger.Debug("In SecurityRepository getAllGroups");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                items = client.Get<Groups>("/groups?format=" + format);  ///users?format={format}

                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;

            }
            return items;
        }

        public Permissions GetAllPermissions(string format)
        {
            Permissions items = null;
            _logger.Debug("In SecurityRepository GetAllPermissions");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                //sitePermissions?siteId={siteId}&format={format}

                //items = client.Get<Permissions>("/permissions?format=" + format);
                items = client.Get<Permissions>("/sitePermissions?siteId=1&format="+ format);  
               
                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;

            }
            return items;
        }

        public Roles GetAllRoles(string format)
        {
            Roles items = null;
            _logger.Debug("In SecurityRepository GetAllRoles");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                items = client.Get<Roles>("/roles?format=" + format);

                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;

            }
            return items;
        }

        public UserGroups GetGroupUsers(string groupId,string format)
        {
            UserGroups items = null;
            _logger.Debug("In SecurityRepository GetGroupUsers");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                items = client.Get<UserGroups>("/groupUsers?groupId=" + groupId + "&format=" + format);

                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw ex;

            }
            return items;
        }

        public Group getGroupById(string iGroupId, string format)
        {
            Group item = null;
            _logger.Debug("In SecurityRepository getGroupById");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                item = client.Get<Group>("/group?groupId="+ iGroupId + "&format="+ format);
                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
                
            }
            return item;
           
        }

        public Response InsertUsers(FormCollection form)
        {
            Response response = null;

            _logger.Debug("In Security Repository InsertUsers");
            try
            {
                User user = new User { UserName = form["UserName"], UserFirstName = form["UserFirstName"], UserLastName = form["UserLastName"], UserEmail = form["UserEmail"], UserPhone = form["UserPhone"], UserDesc = form["UserDesc"] };
                Users users = new Users();
                users.Add(user);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);

                response = client.Post<Users, Response>("/users?format=xml", users, true);
                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMSaveUser);
            }

            return response;

        }

        public Response UpdateUsers(FormCollection form)
        {
            Response response = null;
            _logger.Debug("In Security Repository UpdateUsers");
            try
            {
                User user = new User { UserId = Convert.ToInt32(form["UserId"]), UserName = form["UserName"], UserFirstName = form["UserFirstName"], UserLastName = form["UserLastName"], UserEmail = form["UserEmail"], UserPhone = form["UserPhone"], UserDesc = form["UserDesc"] };
                Users users = new Users();
                users.Add(user);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);

                response = client.Put<Users, Response>("/users?format=xml", users, true);
                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMSaveUser);
            }

            return response;
        }

        public Response DeleteUser(string userName, string format)
        {
            Response response = null;
            User user = new User { UserName=userName };
            _logger.Debug("In SecurityRepository DeleteUser");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
              response =  client.Delete<User,Response>("/users?userName=" + userName + "&format=" + format, user, true);
                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMDeleteUser);
            }
            return response;
        }

        public Response InsertGroup(FormCollection form)
        {
            Response response = null;

            _logger.Debug("In Security Repository InsertGroup");
            try
            {
                Group group = new Group { GroupName = form["GroupName"], GroupDesc = form["GroupDesc"]};
                Groups groups = new Groups();
                groups.Add(group);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);

                response = client.Post<Groups,Response>("/groups?format=xml", groups, true);
                _logger.Debug("Successfully called Security Service.");
                
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMSaveGroup);
            }

            return response;
        }

        public Response UpdateGroup(FormCollection form)
        {
            Response response = null;
            _logger.Debug("In Security Repository UpdateGroup");
            try
            {
                Group group = new Group { GroupId = Convert.ToInt32(form["GroupId"]), GroupName = form["GroupName"], GroupDesc = form["GroupDesc"] };
                Groups groups = new Groups();
                groups.Add(group);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);

               response = client.Put<Groups,Response>("/groups?format=xml", groups, true);
                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMSaveGroup);
            }

            return response;

        }

        public Response deleteGroup(string groupId, string format)
        {
            Response response = null;

            Group group1 = new Group { GroupId = Convert.ToInt32(groupId) };
            Groups groups = new Groups();
            groups.Add(group1);

            _logger.Debug("In SecurityRepository deleteGroup");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                response = client.Delete<Groups, Response>("/groups?groupId=" + groupId + "&format=" + format, groups, true);

                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMDeleteGroup);
            }

            return response;

        }

        public Response InsertRole(FormCollection form)
        {
            Response response = null;
            _logger.Debug("In Security Repository InsertRole");
            try
            {
                Role role = new Role { RoleName = form["RoleName"], RoleDesc = form["RoleDesc"] };
                Roles roles = new Roles();
                roles.Add(role);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                response = client.Post<Roles,Response>("/roles?format=xml", roles, true);
                _logger.Debug("Successfully called Security Service.");

            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMSaveRole);
            }

            return response;

        }

        public Response UpdateRole(FormCollection form)
        {
            Response response = null;
            _logger.Debug("In Security Repository UpdateRole");
            try
            {
                Role role = new Role { RoleId = Convert.ToInt32(form["RoleId"]), RoleName = form["RoleName"], RoleDesc = form["RoleDesc"] };
                Roles roles = new Roles();
                roles.Add(role);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                response = client.Put<Roles,Response>("/roles?format=xml", roles, true);
                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMSaveRole);
            }

            return response;
        }

        public Response DeleteRole(string roleId, string format)
        {
            Response response = null;
            Role role = new Role { RoleId = Convert.ToInt32(roleId) };

            _logger.Debug("In SecurityRepository DeleteRole");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                 response = client.Delete<Role,Response>("/roles?roleId=" + roleId + "&format=" + format, role, true);
                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMDeleteRole);
            }

            return response;

        }

        public Response InsertPermission(FormCollection form)
        {
            Response response = null;
            _logger.Debug("In Security Repository InsertPermission");
            try
            {
                Permission premission = new Permission { PermissionName = form["PermissionName"], PermissionDesc = form["PermissionDesc"] };
                Permissions permissions = new Permissions();
                permissions.Add(premission);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);

                response = client.Post<Permissions,Response>("/permissions?format=xml", permissions, true);
                _logger.Debug("Successfully called Security Service.");

            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMSavePermission);
            }

            return response;

        }

        public Response UpdatePermission(FormCollection form)
        {
            Response response = null;
            _logger.Debug("In Security Repository UpdatePermission");
            try
            {
                Permission permission = new Permission { PermissionId = Convert.ToInt32(form["PermissionId"]), PermissionName = form["PermissionName"], PermissionDesc = form["PermissionDesc"] };
                Permissions permissions = new Permissions();
                permissions.Add(permission);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);

                response = client.Put<Permissions,Response>("/permissions?format=xml", permissions, true);
                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMSavePermission);
            }

            return response;
        }

        public Response DeletePermission(string permissionId, string format)
        {
            Response response = null;
            Permission permission = new Permission { PermissionId = Convert.ToInt32(permissionId) };

            _logger.Debug("In SecurityRepository DeletePermission");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
               response = client.Delete<Permission,Response>("/permissions?permissionId=" + permissionId + "&format=" + format, permission, true);
                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMDeleteRole);
            }

            return response;
        }

        public Response InsertGroupUsers(FormCollection form)
        {
            Response response = null;
            _logger.Debug("In Security Repository InsertGroupUsers, insert users in group");
            try
            {
                int groupId = Convert.ToInt32(form["GroupId"]);
                string[] userIds =  form["SelectedUsers"].Split(new string[]{","},StringSplitOptions.RemoveEmptyEntries);

                UserGroups userGroups = new UserGroups();
                if (userIds.Length == 0)//to delete
                {
                    UserGroup userGroup = new UserGroup { GroupId = groupId, UserId = -1 };
                    userGroups.Add(userGroup);
                }
                               
                foreach (string item in userIds)
                {
                    UserGroup userGroup = new UserGroup { GroupId = groupId, UserId = Convert.ToInt32(item) };
                    userGroups.Add(userGroup);
                }

                WebHttpClient client = CreateWebClient(_adapterServiceUri);

                response = client.Post<UserGroups,Response>("/insertGroupUsers?format=xml", userGroups, true);
                _logger.Debug("Successfully called Security Service.");

            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMSaveUsersInAGroup);
            }

            return response;

        }

        public Response InsertUserGroups(FormCollection form)
        {
            Response response = null;
            _logger.Debug("In Security Repository InsertUserGroups, insert groups in a user");
            try
            {
                int userId = Convert.ToInt32(form["UserId"]);
                string[] groupIds = form["SelectedGroups"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                UserGroups userGroups = new UserGroups();
                if (groupIds.Length == 0)
                {
                    UserGroup userGroup = new UserGroup { GroupId =-1, UserId = userId };
                    userGroups.Add(userGroup);
                }

                foreach (string item in groupIds)
                {
                    UserGroup userGroup = new UserGroup { GroupId = Convert.ToInt32(item), UserId = userId };
                    userGroups.Add(userGroup);
                }

                WebHttpClient client = CreateWebClient(_adapterServiceUri);

                response = client.Post<UserGroups,Response>("/insertUserGroups?format=xml", userGroups, true);
                _logger.Debug("Successfully called Security Service.");

            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMSavePermission);
            }

            return response;

        }

        public Groups GetUserGroups(string userName, string format)
        {
            Groups items = null;
            _logger.Debug("In SecurityRepository GetUserGroups");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                items = client.Get<Groups>("/groupsUser?userName=" + userName + "&format=" + format);

                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw ex;

            }
            return items;
        }

        public Response InsertRoleGroups(FormCollection form)
        {
            Response response = null;
            _logger.Debug("In Security Repository InsertRoleGroups, insert groups in a role");
            try
            {
                int roleId = Convert.ToInt32(form["RoleId"]);
                string[] groupIds = form["SelectedGroups"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                GroupRoles userGroups = new GroupRoles();
                if(groupIds.Length==0)
                   {
                    GroupRole userGroup = new GroupRole { GroupId = -1, RoleId = roleId };
                    userGroups.Add(userGroup);
                   }

                foreach (string item in groupIds)
                {
                    GroupRole userGroup = new GroupRole { GroupId = Convert.ToInt32(item), RoleId = roleId };
                    userGroups.Add(userGroup);
                }

                WebHttpClient client = CreateWebClient(_adapterServiceUri);

                response = client.Post<GroupRoles,Response>("/insertRoleGroups?format=xml", userGroups, true);
                _logger.Debug("Successfully called Security Service.");

            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMSaveGroupsInARole);
            }

            return response;

        }

        public Groups GetRoleGroups(string roleId, string format)
        {
            Groups items = null;
            _logger.Debug("In SecurityRepository GetUserGroups");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                items = client.Get<Groups>("/roleGroups?roleId=" + roleId + "&format=" + format);

                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;

            }
            return items;
        }

        public Response InsertGroupRoles(FormCollection form)
        {
            Response response = null;
            _logger.Debug("In Security Repository InsertGroupRoles, insert roles in group");
            try
            {
                int groupId = Convert.ToInt32(form["GroupId"]);
                string[] roleIds = form["SelectedRoles"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                GroupRoles userGroups = new GroupRoles();
                if (roleIds.Length == 0)//to delete
                {
                    GroupRole userGroup = new GroupRole { GroupId = groupId, RoleId = -1 };
                    userGroups.Add(userGroup);

                }

                foreach (string item in roleIds)
                {
                    GroupRole userGroup = new GroupRole { GroupId = groupId, RoleId = Convert.ToInt32(item) };
                    userGroups.Add(userGroup);
                }

                WebHttpClient client = CreateWebClient(_adapterServiceUri);

                response = client.Post<GroupRoles, Response>("/insertGroupRoles?format=xml", userGroups, true);
                _logger.Debug("Successfully called Security Service.");

            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMSaveRolesInAGroup);
            }

            return response;

        }

        public Roles GetGroupRoles(string groupId, string format)
        {
            Roles items = null;
            _logger.Debug("In SecurityRepository GetGroupRoles");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                items = client.Get<Roles>("/groupRoles?groupId=" + groupId + "&format=" + format);

                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;

            }
            return items;
        }

        public Permissions GetRolePermissions(string roleId, string format)
        {
            Permissions items = null;
            Permissions allPermissions = null;
            _logger.Debug("In SecurityRepository GetRolePermissions");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                items = client.Get<Permissions>("/rolePermissions?roleId=" + roleId + "&format=" + format);
                allPermissions = GetAllPermissions("xml");
                foreach(var permItem  in allPermissions){
                    Permission perm = (Permission)permItem;
                    var permId = perm.PermissionId;
                    foreach(var selPerms1  in items){
                        Permission selPerm = (Permission)selPerms1;
                        var selPermId = selPerm.PermissionId;
                        if (selPermId == permId)
                        {
                            permItem.Chk = true;
                        }
                    }
                }

                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }
            return allPermissions;
        }

        public Response InsertRolePermissions(FormCollection form)
        {
            Response response = null;
            _logger.Debug("In Security Repository InsertRolePermissions, insert permissions in role");
            try
            {
                int roleId = Convert.ToInt32(form["RoleId"]);
                string[] permissionIds = form["SelectedPermissions"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                RolePermissions userGroups = new RolePermissions();
                if (permissionIds.Length == 0)
                {
                    RolePermission userGroup = new RolePermission { RoleId = roleId, PermissionId =-1 };
                    userGroups.Add(userGroup);
                }

                foreach (string item in permissionIds)
                {
                    RolePermission userGroup = new RolePermission { RoleId = roleId, PermissionId = Convert.ToInt32(item) };
                    userGroups.Add(userGroup);
                }

                WebHttpClient client = CreateWebClient(_adapterServiceUri);

                response = client.Post<RolePermissions,Response>("/insertRolePermissions?format=xml", userGroups, true);
                _logger.Debug("Successfully called Security Service.");

            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex, ErrorMessages.errUSMSavePermissionsInARole);
            }

            return response;

        }

        internal Sites GetSitesbyUser(string userName, int siteId)
        {
            Sites sites = null;
            _logger.Debug("In SecurityRepository GetGroupRoles");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                sites = client.Get<Sites>(String.Format("/sitesbyuser?userName={0}&siteId={1}&format=xml", userName, siteId));

                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;

            }
            return sites;
        }

        #region Private Methods
        private Response PrepareErrorResponse(Exception ex, string errMsg)
        {
            _logger.Error(ex.ToString());
            CustomErrorLog objCustomErrorLog = new CustomErrorLog();
            _CustomError = objCustomErrorLog.customErrorLogger(errMsg, ex, _logger);
            Response response = new Response
            {
                Level = StatusLevel.Error,
                Messages = new Messages
                      {
                        //ex.Message
                         "[ " + _CustomError.msgId + "] " +errMsg 
                      },
                StatusText = _CustomError.stackTraceDescription,
                StatusCode = HttpStatusCode.InternalServerError,
                StatusList = null
            };
            return response;
        }
        #endregion
    }
}
