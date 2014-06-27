using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.adapter;
using log4net;
using System.Collections.Specialized;
using Ninject;
using System.Runtime.Serialization;
using System.IO;
using org.iringtools.library;
using System.Xml.Linq;
using org.iringtools.utility;
using System.ServiceModel.Web;
using System.Data.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace org.iringtools.applicationConfig
{
    public class ApplicationConfigurationProvider : BaseProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));
        private string _connSecurityDb;
        private int _siteID;

        [Inject]
        public ApplicationConfigurationProvider(NameValueCollection settings)
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
                _logger.Error("Error initializing adapter provider: " + e.Message);
            }
        }

        public Contexts GetAllContexts()
        {
            List<Context> lstContext = new List<Context>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstContext = dc.ExecuteQuery<Context>("spgContext @SiteId = {0}",_siteID).ToList();
            }

            Contexts contexts = new Contexts();
            contexts.AddRange(lstContext);
            return contexts;
        }

        public Response InsertContext(XDocument xml)
        {
            Response response = new Response();

            try
            {
                Contexts contexts = Utility.DeserializeDataContract<Contexts>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    foreach (Context context in contexts)
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@DisplayName", Value = context.DisplayName });
                        nvl.Add(new ListItem() { Name = "@InternalName", Value = context.InternalName });
                        nvl.Add(new ListItem() { Name = "@Description", Value = context.Description });
                        nvl.Add(new ListItem() { Name = "@CacheConnStr", Value = context.CacheConnStr });
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });

                        DBManager.Instance.ExecuteNonQueryStoredProcedure(_connSecurityDb, "spiContext", nvl);
                    }
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Contexts added successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Contexts: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateContext(XDocument xml)
        {
            Response response = new Response();

            try
            {
                Contexts contexts = Utility.DeserializeDataContract<Contexts>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    foreach (Context context in contexts)
                    {
                        if (string.IsNullOrEmpty(context.InternalName))
                        {
                            throw new Exception("Please provide the internal name of the context in" + 
                                                 " the payload which you want to update.");
                        }

                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@DisplayName", Value = context.DisplayName });
                        nvl.Add(new ListItem() { Name = "@InternalName", Value = context.InternalName });
                        nvl.Add(new ListItem() { Name = "@Description", Value = context.Description });
                        nvl.Add(new ListItem() { Name = "@CacheConnStr", Value = context.CacheConnStr });
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(_siteID) });

                        DBManager.Instance.ExecuteNonQueryStoredProcedure(_connSecurityDb, "spuContext", nvl);
                    }
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Contexts updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating Contexts: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteContext(string internalName)
        {
            Response response = new Response();

            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    dc.ExecuteQuery<Context>("spdContext @InternalName = {0}, @SiteId = {1}", internalName, _siteID);
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Context deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Context: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Applications GetAllApplications(string scopeInternalName)
        {
            List<Application> lstApplication = new List<Application>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstApplication = dc.ExecuteQuery<Application>("spgApplication @ScopeInternalName = {0}, @SiteId = {1}",
                                                              scopeInternalName, _siteID).ToList();
            }

            Applications applications = new Applications();
            applications.AddRange(lstApplication);
            return applications;
        }

        public Response InsertApplications(string scopeInternalName, XDocument xml)
        {
            Response response = new Response();

            try
            {
                Applications applications = Utility.DeserializeDataContract<Applications>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    foreach (Application application in applications)
                    {
                        dc.ExecuteQuery<Application>("spiApplication @ScopeInternalName = {0}, @AppDisplayName = {1}, @AppInternalName = {2}, " +
                                                      "@Description = {3}, @DXFRUrl = {4}, @SiteId = {5}", scopeInternalName, application.DisplayName,
                                                      application.InternalName, application.Description, application.DXFRUrl, _siteID).ToList();
                    }
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Applications added successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Applications: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateApplications(string scopeInternalName, XDocument xml)
        {
            Response response = new Response();

            try
            {
                Applications applications = Utility.DeserializeDataContract<Applications>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    foreach (Application application in applications)
                    {
                        dc.ExecuteQuery<Application>("spuApplication @ScopeInternalName = {0}, @AppDisplayName = {1}, @AppInternalName = {2}, " +
                                                      "@Description = {3}, @DXFRUrl = {4}, @SiteId = {5}", scopeInternalName, application.DisplayName,
                                                      application.InternalName, application.Description, application.DXFRUrl, _siteID).ToList();
                    }
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Applications updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating Applications: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteApplication(string scopeInternalName, string appInternalName)
        {
            Response response = new Response();

            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    dc.ExecuteQuery<Context>("spdApplication @ScopeInternalName = {0}, @AppInternalName = {1}, @SiteId = {2}", scopeInternalName, appInternalName, _siteID);
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Application deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Application: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Graphs GetAllGraphs()
        {
            List<Graph> lstGraph = new List<Graph>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstGraph = dc.ExecuteQuery<Graph>("spgGraph").ToList();
            }

            Graphs graphs = new Graphs();
            graphs.AddRange(lstGraph);
            return graphs;
        }


        public Applications GetApplicationsForUser(string user)
        {
            List<Application> lstApplications = new List<Application>();

            // Fetch Application from DB as a list of string for that user.
            Application objApplication = new Application();
            //Fill object here.
            lstApplications.Add(objApplication);

            Applications applications = new Applications();
            applications.AddRange(lstApplications);
            return applications;
        }

        public Response InsertApplicationForUser(string user,string format, XDocument xml)
        {
            Response response = new Response();

            try
            {
                Applications applications = Utility.DeserializeDataContract<Applications>(xml.ToString());
                
                //To Do: Call your stored procedure to insert the application.

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Application added successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding application: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateApplicationForUser(string user, string format, XDocument xml)
        {
            Response response = new Response();

            try
            {
                Applications applications = Utility.DeserializeDataContract<Applications>(xml.ToString());

                //To Do: Call your stored procedure to update the application.

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Application updating successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating application: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteApplicationForUser(string user, string format)
        {
            Response response = new Response();

            try
            {
                //To Do: Call your stored procedure to update the application.

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Application updating successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating application: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
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
