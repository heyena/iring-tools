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

namespace org.iringtools.UserSecurity
{
    public class UserSecurityProvider : BaseProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));
        private string _connSecurityDb;

        [Inject]
        public UserSecurityProvider(NameValueCollection settings)
            : base(settings)
        {
            try
            {
                // We have _settings collection available here.
                _connSecurityDb = settings["SecurityConnection"];
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
                lstUsers = dc.ExecuteQuery<User>(@"SELECT UserId, UserName, SiteId, 
                                    UserFirstName, UserLastName, UserEmail, UserPhone, UserDesc, Active
                                    FROM [Users]").ToList();
            }

            Users users = new Users();
            users.AddRange(lstUsers);
            return users;
        }

        public Sites GetSites()
        {
            List<Site> lstSite = new List<Site>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstSite = dc.ExecuteQuery<Site>("spgSite").ToList();
            }

            Sites sites = new Sites();
            sites.AddRange(lstSite);
            return sites;
        }

        public Site GetSite(int iSiteId)
        {
            Site site = new Site();
           
            using (var dc = new DataContext(_connSecurityDb))
            {
                //object[] myObjArray = { 2, 'b', "test", "again" };
                //site = dc.ExecuteQuery<Site>("spgSite", myObjArray).ToList().First();
                site = dc.ExecuteQuery<Site>("spgSite",iSiteId).ToList().First();
            }

            return site;
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

    }
}
