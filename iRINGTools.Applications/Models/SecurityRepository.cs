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

                items = client.Get<Permissions>("/permissions?format=" + format);  

               
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

        public Group getGroupById(string iGroupId, string format)
        {
            Group item = null;
            _logger.Debug("In SecurityRepository getAllGroups");
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

        public void InsertUsers(FormCollection form)
        {

            _logger.Debug("In Security Repository Insert Users");
            try
            {
                User user = new User { UserName = form["UserName"], UserFirstName = form["UserFirstName"], UserLastName = form["UserLastName"], UserEmail = form["UserEmail"], UserPhone = form["UserPhone"], UserDesc = form["UserDesc"] };
                Users users = new Users();
                users.Add(user);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);

                // client.Post<Users>("/users?format=xml", users, true,"xml");
                client.Post<Users>("/users?format=xml", users, true);
                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;

            }

        }

        public void InsertGroup(FormCollection form)
        {

            _logger.Debug("In Security Repository Insert Group");
            try
            {
                Group group = new Group { GroupName = form["GroupName"], GroupDesc = form["GroupDesc"]};
                Groups groups = new Groups();
                groups.Add(group);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);

                // client.Post<Users>("/users?format=xml", users, true,"xml");
                client.Post<Groups>("/groups?format=xml", groups, true);
                _logger.Debug("Successfully called Security Service.");

            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;

            }

        }

        public void UpdateGroup(FormCollection form)
        {

            _logger.Debug("In Security Repository Update Group");
            try
            {
                Group group = new Group { GroupId = Convert.ToInt32(form["GroupId"]), GroupName = form["GroupName"], GroupDesc = form["GroupDesc"] };
                Groups groups = new Groups();
                groups.Add(group);
                WebHttpClient client = CreateWebClient(_adapterServiceUri);

                // client.Post<Users>("/users?format=xml", users, true,"xml");
                client.Put<Groups>("/groups?format=xml", groups, true);
                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;

            }

        }

        public Group deleteGroup(string groupId, string format)
        {
            Group group = null;
            Group group1 = new Group { GroupId = Convert.ToInt32(groupId) };
            Groups groups = new Groups();
            groups.Add(group1);

            _logger.Debug("In SecurityRepository getAllGroups");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                //client.Delete<Groups>("/groups?format=xml", groups, true);
                client.Delete<Groups>("/groups?groupId=" + groupId + "&format=" + format, groups, true);
                
                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;

            }
            return group;

        }

     }
}
