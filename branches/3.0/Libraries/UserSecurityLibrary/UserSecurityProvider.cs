using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using org.iringtools.adapter;
using log4net;
using System.Collections.Specialized;
using Ninject;
using System.Data.Linq;
using org.iringtools.utility;
using System.Web;
using System.IO;
using System.Xml.Linq;
using org.iringtools.library;

namespace org.iringtools.UserSecurity
{
    public class UserSecurityProvider : BaseProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));
        private string _connSecurityDb;
        private int _siteID;

        [Inject]
        public UserSecurityProvider(NameValueCollection settings)
            : base(settings)
        {
            try
            {
                // We have _settings collection available here.
                _connSecurityDb = settings["SecurityConnection"];
                _siteID = Convert.ToInt32(settings["SiteId"]);
            }
            catch (Exception e)
            {
                _logger.Error("Error initializing UserSecurity provider: " + e.Message);
            }
        }

        public Users GetAllUsers()
        {
            List<User> lstUsers = new List<User>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstUsers = dc.ExecuteQuery<User>("spgAllUser").ToList();
            }

            Users users = new Users();
            users.AddRange(lstUsers);
            return users;
        }
        
        public Response InsertUsers(XDocument xml)
        {

            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                User user = Utility.DeserializeDataContract<Users>(xml.ToString()).FirstOrDefault();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (user == null || string.IsNullOrEmpty(user.UserName))
                        PrepareErrorResponse(response, "Please enter UserName!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                        nvl.Add(new ListItem() { Name = "@UserName", Value = user.UserName });
                        nvl.Add(new ListItem() { Name = "@UserFirstName", Value = user.UserFirstName });
                        nvl.Add(new ListItem() { Name = "@UserLastName", Value = user.UserLastName });
                        nvl.Add(new ListItem() { Name = "@UserEmail", Value = user.UserEmail });
                        nvl.Add(new ListItem() { Name = "@UserPhone", Value = user.UserPhone });
                        nvl.Add(new ListItem() { Name = "@UserDesc", Value = user.UserDesc });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiUser", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "User added successfully!");
                                break;
                            case "0":
                                PrepareErrorResponse(response, "User with this name already exists!");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Users: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateUsers(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                User user = Utility.DeserializeDataContract<Users>(xml.ToString()).FirstOrDefault();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (user == null || string.IsNullOrEmpty(user.UserName))
                        PrepareErrorResponse(response, "Please enter UserName!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@UserId", Value = Convert.ToString(user.UserId) });
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                        nvl.Add(new ListItem() { Name = "@UserName", Value = user.UserName });
                        nvl.Add(new ListItem() { Name = "@UserFirstName", Value = user.UserFirstName });
                        nvl.Add(new ListItem() { Name = "@UserLastName", Value = user.UserLastName });
                        nvl.Add(new ListItem() { Name = "@UserEmail", Value = user.UserEmail });
                        nvl.Add(new ListItem() { Name = "@UserPhone", Value = user.UserPhone });
                        nvl.Add(new ListItem() { Name = "@UserDesc", Value = user.UserDesc });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuUser", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "User udpated successfully!");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error updating User: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;

           
        }

        public Response DeleteUser(string userName)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (string.IsNullOrEmpty(userName))
                        PrepareErrorResponse(response, "Please enter UserName!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                        nvl.Add(new ListItem() { Name = "@UserName", Value = userName });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdUser", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "User deleted successfully!");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting User: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public User GetUserById(int iUserId)
        {
            List<User> lstUsers = new List<User>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstUsers = dc.ExecuteQuery<User>("spgUserById @UserId = {0}", iUserId).ToList();
            }

            User user = new User();
            if (lstUsers.Count > 0)
                user = lstUsers.First();
            return user;
        }

        public Sites GetSites()
        {
            List<Site> lstSite = new List<Site>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstSite = dc.ExecuteQuery<Site>("spgSites").ToList();
            }

            Sites sites = new Sites();
            sites.AddRange(lstSite);
            return sites;
        }

        public Sites GetSitesByUser(string userName, int siteId)
        {
            List<Site> listSites = new List<Site>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                listSites = dc.ExecuteQuery<Site>("spgSitesByUser @UserName = {0}, @SiteId = {1}", userName, siteId).ToList();
            }

            Sites sites = new Sites();
            sites.AddRange(listSites);
            return sites;
        }

        public Site GetSite(int iSiteId)
        {
            List<Site> lstSite = new List<Site>();
           
            using (var dc = new DataContext(_connSecurityDb))
            {
                lstSite = dc.ExecuteQuery<Site>("spgSitesById @SiteId = {0}", _siteID).ToList();
            }

            Site site = new Site();
            if (lstSite.Count > 0)
                site = lstSite.First();

            return site;
        }

        public Response InsertRole(XDocument xml)
        {
            
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                Role role = Utility.DeserializeDataContract<Roles>(xml.ToString()).FirstOrDefault();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (role == null || string.IsNullOrEmpty(role.RoleName))
                        response.Messages.Add("Please enter RoleName.");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                        nvl.Add(new ListItem() { Name = "@RoleName", Value = role.RoleName });
                        nvl.Add(new ListItem() { Name = "@RoleDesc", Value = role.RoleDesc });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiRole", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "Role added successfully!");
                                break;
                            case "0":
                                PrepareErrorResponse(response, "Role with this name already exists!");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Roles: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateRoles(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                Role role = Utility.DeserializeDataContract<Roles>(xml.ToString()).FirstOrDefault();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (role == null || string.IsNullOrEmpty(role.RoleName))
                        response.Messages.Add("Please enter RoleName.");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@RoleId", Value = Convert.ToString(role.RoleId) });
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                        nvl.Add(new ListItem() { Name = "@RoleName", Value = role.RoleName });
                        nvl.Add(new ListItem() { Name = "@RoleDesc", Value = role.RoleDesc });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuRoles", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "Role updated successfully!");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error updating Roles: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
            
        }

        public Response DeleteRole(int roleId)
        {

            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (roleId == 0)
                        PrepareErrorResponse(response, "Please enter RoleId!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                        nvl.Add(new ListItem() { Name = "@RoleId", Value = Convert.ToString(roleId) });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdRoles", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "Role deleted successfully!");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Role: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response InsertPermission(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                Permission permission = Utility.DeserializeDataContract<Permissions>(xml.ToString()).FirstOrDefault();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (permission == null || string.IsNullOrEmpty(permission.PermissionName))
                        response.Messages.Add("Please enter PermissionName.");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                        nvl.Add(new ListItem() { Name = "@PermissionName", Value = permission.PermissionName });
                        nvl.Add(new ListItem() { Name = "@PermissionDesc", Value = permission.PermissionDesc });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiPermissions", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "Permission added successfully!");
                                break;
                            case "0":
                                PrepareErrorResponse(response, "Permission with this name already exists!");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Permissions: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdatePermissions(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                Permission permission = Utility.DeserializeDataContract<Permissions>(xml.ToString()).FirstOrDefault();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (permission == null || string.IsNullOrEmpty(permission.PermissionName))
                        response.Messages.Add("Please enter PermissionName.");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                        nvl.Add(new ListItem() { Name = "@PermissionId", Value = Convert.ToString(permission.PermissionId) });
                        nvl.Add(new ListItem() { Name = "@PermissionName", Value = permission.PermissionName });
                        nvl.Add(new ListItem() { Name = "@PermissionDesc", Value = permission.PermissionDesc });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuPermissions", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "Permission updated successfully!");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error updating Permissions: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;

        }

        public Response DeletePermission(int permissionId)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (permissionId == 0)
                        PrepareErrorResponse(response, "Please enter PermissionId!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                        nvl.Add(new ListItem() { Name = "@PermissionId", Value = Convert.ToString(permissionId) });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdPermissions", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "Permission deleted successfully!");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Permission: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response InsertGroup(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                Group group = Utility.DeserializeDataContract<Groups>(xml.ToString()).FirstOrDefault();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (group == null || string.IsNullOrEmpty(group.GroupName))
                        response.Messages.Add("Please enter GroupName.");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                        nvl.Add(new ListItem() { Name = "@GroupName", Value = group.GroupName });
                        nvl.Add(new ListItem() { Name = "@GroupDesc", Value = group.GroupDesc });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiGroups", nvl);

                        switch (output)
                        { 
                            case "1":
                                PrepareSuccessResponse(response, "Group added successfully!");
                                break;
                            case "0":
                                PrepareErrorResponse(response, "Group with this name already exists!");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                        
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Groups: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateGroups(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                Group group = Utility.DeserializeDataContract<Groups>(xml.ToString()).FirstOrDefault();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (group == null || string.IsNullOrEmpty(group.GroupName))
                        response.Messages.Add("Please enter GroupName.");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                        nvl.Add(new ListItem() { Name = "@GroupId", Value = Convert.ToString(group.GroupId) });
                        nvl.Add(new ListItem() { Name = "@GroupName", Value = group.GroupName });
                        nvl.Add(new ListItem() { Name = "@GroupDesc", Value = group.GroupDesc });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuGroups", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "Group updated successfully!");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error updating Groups: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
           
        }

        public Response DeleteGroup(int groupId)
        {

            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (groupId == 0)
                        PrepareErrorResponse(response, "Please enter GroupId!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                        nvl.Add(new ListItem() { Name = "@GroupId", Value = Convert.ToString(groupId) });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdGroups", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "Group deleted successfully!");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Group: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response InsertSite(XDocument xml)
        {
            Response response = new Response();

            try
            {
                Sites sites = Utility.DeserializeDataContract<Sites>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    foreach (Site site in sites)
                    {
                        dc.ExecuteQuery<Site>("spiSites @SiteName = {0}, @SiteDesc = {1}", site.SiteName , site.SiteDesc).ToList();
                    }
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Sites added successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Sites: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateSites(XDocument xml)
        {
            Response response = new Response();

            try
            {
                Sites sites = Utility.DeserializeDataContract<Sites>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    foreach (Site site in sites)
                    {
                        dc.ExecuteQuery<Site>("spuSites @SiteId = {0}, @SiteName = {1}, @SiteDesc = {2}", _siteID, site.SiteName, site.SiteDesc).ToList();
                    }
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Sites updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating Sites: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteSite(int siteId)
        {
            Response response = new Response();

            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    dc.ExecuteQuery<Site>("spdSites @SiteId = {0}", _siteID);
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Site deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Site: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Roles GetSiteRoles(int iSiteId)
        {
            List<Role> lstRoles = new List<Role>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstRoles = dc.ExecuteQuery<Role>("spgSiteRoles @SiteId = {0}", _siteID).ToList();
            }

            Roles roles = new Roles();
            roles.AddRange(lstRoles);
            return roles;
        }

        public GroupRoles GetGroupRole(int groupId, int roleId)
        {
            List<GroupRole> lstgroupRoles = new List<GroupRole>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstgroupRoles = dc.ExecuteQuery<GroupRole>("spgGroupRoles @GroupId = {0}, @RoleId = {1}, @SiteId = {2}",
                                                  groupId, roleId, _siteID).ToList();
            }

            GroupRoles groupRoles = new GroupRoles();
            groupRoles.AddRange(lstgroupRoles);
            return groupRoles;
        }

        public Groups GetAllGroups()
        {
            List<Group> lstGroup = new List<Group>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstGroup = dc.ExecuteQuery<Group>("spgAllGroups").ToList();
            }

            Groups groups = new Groups();
            groups.AddRange(lstGroup);
            return groups;
        }

        public Group GetGroupById(int iGroupId)
        {
            List<Group> lstGroup = new List<Group>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstGroup = dc.ExecuteQuery<Group>("spgGroups @GroupId = {0}", iGroupId).ToList();
            }

            Group group = new Group();
            if (lstGroup.Count > 0)
                group = lstGroup.First();

            return group;
        }

        public Roles GetAllRoles()
        {
            List<Role> lstRole = new List<Role>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstRole = dc.ExecuteQuery<Role>("spgRoles").ToList();
            }

            Roles roles = new Roles();
            roles.AddRange(lstRole);
            return roles;
        }

        public Role GetRoleById(int iRoleId)
        {
            List<Role> lstRole = new List<Role>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstRole = dc.ExecuteQuery<Role>("spgRolesById @RoleId = {0}", iRoleId).ToList();
            }

            Role role = new Role();
            if (lstRole.Count > 0)
                role = lstRole.First();

            return role;
        }

        public Groups GetSiteGroups(int iSiteId)
        {
            List<Group> lstGroup = new List<Group>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstGroup = dc.ExecuteQuery<Group>("spgSiteGroups @SiteId = {0}", _siteID).ToList();
            }

            Groups groups = new Groups();
            groups.AddRange(lstGroup);
            return groups;
        }

        public UserGroups GetGroupUsers(int iGroupId)
        {
            try
            {
                List<UserGroup> lstUser = new List<UserGroup>();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    lstUser = dc.ExecuteQuery<UserGroup>("spgGroupUsers @GroupId = {0}", iGroupId).ToList();
                }

                UserGroups usersG = new UserGroups();
                usersG.AddRange(lstUser);
                return usersG;
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting Group: " + ex);
                throw ex;
            }
        }

        public Users GetSiteUsers(int iSiteId)
        {
            List<User> lstUser = new List<User>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstUser = dc.ExecuteQuery<User>("spgSiteUsers @SiteId = {0}", _siteID).ToList();
            }

            Users users = new Users();
            users.AddRange(lstUser);
            return users;
        }

        public Permissions GetSitePermissions(int iSiteId)
        {
            List<Permission> lstPermission = new List<Permission>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstPermission = dc.ExecuteQuery<Permission>("spgPermissions @SiteId = {0}", _siteID).ToList();
            }

            Permissions permissions = new Permissions();
            permissions.AddRange(lstPermission);
            return permissions;
        }

        public Groups GetGroupsUser(string userName)
        {
            try
            {
                List<Group> lstGroup = new List<Group>();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    lstGroup = dc.ExecuteQuery<Group>("spgGroupUser @UserName = {0}, @SiteId = {1}", userName, _siteID).ToList();
                }

                Groups groups = new Groups();
                groups.AddRange(lstGroup);
                return groups;
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting Groups mapped with a user: " + ex);
                throw ex;
            }
        }

        public Users GetGroupUser(int iGroupId, int iUserId)
        {
            List<User> lstGroup = new List<User>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstGroup = dc.ExecuteQuery<User>("spgGroupUserIdGroupId @UserId = {0}, @GroupId = {1}, @SiteId = {2}",
                                                   iUserId, iGroupId, _siteID).ToList();
            }

            Users users = new Users();
            users.AddRange(lstGroup);
            return users;
        }

        public Roles GetGroupRoles(int iGroupId)
        {
            List<Role> lstRole = new List<Role>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstRole = dc.ExecuteQuery<Role>("spgRolesGroup @GroupId = {0}, @SiteId = {1}",
                                                                    iGroupId, _siteID).ToList();
            }

            Roles roles = new Roles();
            roles.AddRange(lstRole);
            return roles;
        }

        public Permissions GetRolePermissions(int iRoleId)
        {
            List<Permission> lstPermission = new List<Permission>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstPermission = dc.ExecuteQuery<Permission>("spgRolePermissions @RoleId = {0}, @SiteId = {1}",
                                                                    iRoleId, _siteID).ToList();
            }

            Permissions permissions = new Permissions();
            permissions.AddRange(lstPermission);
            return permissions;
        }

        public Permissions GetRolePermission(int iRoleId, int iPermissionId) 
        {
            List<Permission> lstPermission = new List<Permission>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstPermission = dc.ExecuteQuery<Permission>("spgPermissionRoles @RoleId = {0}, @PermissionId = {1}, @SiteId = {2}",
                                                                    iRoleId, iPermissionId, _siteID).ToList();
            }

            Permissions permissions = new Permissions();
            permissions.AddRange(lstPermission);
            return permissions;
        }

        public UserGroups GetUserGroups(string userName)
        {
            List<UserGroup> lstUserGroup = new List<UserGroup>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstUserGroup = dc.ExecuteQuery<UserGroup>("spgUserGroups @userName = {0}, @siteId = {1}",
                                                      userName, _siteID).ToList();
            }

            UserGroups userGroups = new UserGroups();
            userGroups.AddRange(lstUserGroup);
            return userGroups;
        }

        public void FormatOutgoingMessage<T>(T graph, string format, bool useDataContractSerializer)
        {
            if (format.ToUpper() == "JSON")
            {
                string json = Utility.SerializeJson<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
                HttpContext.Current.Response.Write(json);
            }
            else
            {
                string xml = Utility.Serialize<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/xml";
                HttpContext.Current.Response.Write(xml);
            }
        }

        public T FormatIncomingMessage<T>(Stream stream)
        {
            T dataItems;

            DataItemSerializer serializer = new DataItemSerializer(
                _settings["JsonIdField"], _settings["JsonLinksField"], bool.Parse(_settings["DisplayLinks"]));
            string json = Utility.ReadString(stream);
            dataItems = serializer.Deserialize<T>(json, true);
            stream.Close();

            return dataItems;
        }

        public XElement FormatIncomingMessage<T>(Stream stream, string format)
        {
            XElement xElement = null;

            if (format != null && (format.ToLower().Contains("xml") || format.ToLower().Contains("rdf") ||
              format.ToLower().Contains("dto")))
            {
                xElement = XElement.Load(stream);
            }
            else
            {
                T dataItems = FormatIncomingMessage<T>(stream);

                xElement = dataItems.ToXElement<T>();
            }

            return xElement;
        }

        public Response InsertGroupUsers(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {

                string rawXml = xml.ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                nvl.Add(new ListItem() { Name = "@rawXML", Value = rawXml });

                string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiGroupUsers", nvl);

                switch (output)
                {
                    case "1":
                        PrepareSuccessResponse(response, "Users mapped with the group successfully!");
                        break;
                    case "0":
                        PrepareSuccessResponse(response, "All users unmapped with the group successfully!");
                        break;
                    default:
                        PrepareErrorResponse(response, output);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Users to Group: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response InsertUserGroups(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                
                    string rawXml = xml.ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                    NameValueList nvl = new NameValueList();
                    nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                    nvl.Add(new ListItem() { Name = "@rawXML", Value = rawXml });

                    string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiUserGroups", nvl);

                    switch (output)
                    {
                        case "1":
                            PrepareSuccessResponse(response, "Groups mapped with the user successfully!");
                            break;
                        case "0":
                            PrepareSuccessResponse(response, "All groups unmapped with the user successfully!");
                            break;
                        default:
                            PrepareErrorResponse(response, output);
                            break;
                    }
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Groups to User: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Groups GetRoleGroups(int iRoleId)
        {
            List<Group> lstGroup = new List<Group>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstGroup = dc.ExecuteQuery<Group>("spgRoleGroups @RoleId = {0}, @SiteId = {1}", iRoleId, _siteID).ToList();
            }

            Groups groups = new Groups();
            groups.AddRange(lstGroup);
            return groups;
        }

        public Response InsertRoleGroups(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                string rawXml = xml.ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                nvl.Add(new ListItem() { Name = "@rawXML", Value = rawXml });

                string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiRoleGroups", nvl);

                switch (output)
                {
                    case "1":
                        PrepareSuccessResponse(response, "Groups mapped with the role successfully!");
                        break;
                    case "0":
                        PrepareSuccessResponse(response, "All groups unmapped with the role successfully!");
                        break;
                    default:
                        PrepareErrorResponse(response, output);
                        break;
                }

                //using (var dc = new DataContext(_connSecurityDb))
                //{
                //    dc.ExecuteCommand("spiRoleGroups @rawXML = {0},@SiteId = {1}", rawXml, _siteID);
                //}

                //response.DateTimeStamp = DateTime.Now;
                //response.Messages = new Messages();
                //response.Messages.Add("Groups added successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Groups to Role: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response InsertGroupRoles(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                string rawXml = xml.ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                nvl.Add(new ListItem() { Name = "@rawXML", Value = rawXml });

                string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiGroupRoles", nvl);
                
                switch (output)
                {
                    case "1":
                        PrepareSuccessResponse(response, "Roles mapped with the group successfully!");
                        break;
                    case "0":
                        PrepareSuccessResponse(response, "All roles unmapped with the group successfully!");
                        break;
                    default:
                        PrepareErrorResponse(response, output);
                        break;
                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Roles to Group: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response InsertRolePermissions(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                string rawXml = xml.ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem


                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });
                nvl.Add(new ListItem() { Name = "@rawXML", Value = rawXml });

                string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiRolePermissions", nvl);

                switch (output)
                {
                    case "1":
                        PrepareSuccessResponse(response, "Permissions mapped with the role successfully!");
                        break;
                    case "0":
                        PrepareSuccessResponse(response, "All permissions unmapped with the role successfully!");
                        break;
                    default:
                        PrepareErrorResponse(response, output);
                        break;
                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Permissions to Role : " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        #region Private Methods
        private void PrepareErrorResponse(Response response, string errMsg)
        {
            Status status = new Status { Level = StatusLevel.Error };
            status.Messages = new Messages { errMsg };
            response.DateTimeStamp = DateTime.Now;
            response.Level = StatusLevel.Error;
            response.StatusList.Add(status);
            
        }
        private void PrepareSuccessResponse(Response response, string errMsg)
        {
            Status status = new Status { Level = StatusLevel.Success };
            status.Messages = new Messages { errMsg };
            response.DateTimeStamp = DateTime.Now;
            response.Level = StatusLevel.Success;
            response.StatusList.Add(status);
        }
        #endregion
    }
}
