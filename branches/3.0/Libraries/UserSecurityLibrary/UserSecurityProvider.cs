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

            try
            {
                Users users = Utility.DeserializeDataContract<Users>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    foreach (User user in users)
                    {
                        dc.ExecuteQuery<User>("spiUser @SiteId = {0}, @UserName = {1}, @UserFirstName = {2}, "+
                                              "@UserLastName = {3}, @UserEmail = {4}, @UserPhone = {5}, @UserDesc = {6}",
                                              _siteID, user.UserName, user.UserFirstName, user.UserLastName,
                                              user.UserEmail, user.UserPhone, user.UserDesc).ToList();
                    }
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Users added successfully.");
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

            try
            {
                Users users = Utility.DeserializeDataContract<Users>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    foreach (User user in users)
                    {
                        if (user.UserId == null)
                            throw new Exception("Please pass the user id in the json body which you want to  update.");


                        dc.ExecuteQuery<User>("spuUser @UserId = {0}, @SiteId = {1}, @UserName = {2}, @UserFirstName = {3}, " +
                                              "@UserLastName = {4}, @UserEmail = {5}, @UserPhone = {6}, @UserDesc = {7}",
                                              user.UserId, _siteID, user.UserName, user.UserFirstName, user.UserLastName,
                                              user.UserEmail, user.UserPhone, user.UserDesc).ToList();
                    }
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Users added successfully.");
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

        public Response DeleteUser(int userId)
        {
            Response response = new Response();

            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    dc.ExecuteQuery<Context>("spdUser @UserId = {0}, @SiteId = {1}", userId, _siteID);
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("User deleted successfully.");
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
                    dc.ExecuteQuery<Context>("spdSites @SiteId = {0}", _siteID);
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
    }
}
