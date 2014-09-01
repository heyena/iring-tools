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

        public Contexts GetAllContexts(int siteId)
        {
            Contexts contexts = new Contexts();
            try
            {
                if (siteId == 0)
                {
                    siteId = _siteID;
                }

                List<Context> lstContext = new List<Context>();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    lstContext = dc.ExecuteQuery<Context>("spgContext @SiteId = {0}", siteId).ToList();
                }

                contexts.AddRange(lstContext);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Contexts: " + ex);
            }
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
                        int siteId = _siteID;
                        if (context.SiteId != 0)
                        {
                            siteId = context.SiteId;
                        }
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@DisplayName", Value = context.DisplayName });
                        nvl.Add(new ListItem() { Name = "@InternalName", Value = context.InternalName });
                        nvl.Add(new ListItem() { Name = "@Description", Value = context.Description });
                        nvl.Add(new ListItem() { Name = "@CacheConnStr", Value = context.CacheConnStr });
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(siteId) });
                        nvl.Add(new ListItem() { Name = "@FolderId", Value = Convert.ToString(context.FolderId) });

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

                        int siteId = _siteID;
                        if (context.SiteId != 0)
                        {
                            siteId = context.SiteId;
                        }

                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@DisplayName", Value = context.DisplayName });
                        nvl.Add(new ListItem() { Name = "@InternalName", Value = context.InternalName });
                        nvl.Add(new ListItem() { Name = "@Description", Value = context.Description });
                        nvl.Add(new ListItem() { Name = "@CacheConnStr", Value = context.CacheConnStr });
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(siteId) });
                        nvl.Add(new ListItem() { Name = "@FolderId", Value = Convert.ToString(context.FolderId) });

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

        public Response DeleteContext(string internalName, int siteId)
        {
            Response response = new Response();

            try
            {
                if (siteId == 0)
                {
                    siteId = _siteID;
                }

                using (var dc = new DataContext(_connSecurityDb))
                {
                    dc.ExecuteQuery<Context>("spdContext @InternalName = {0}, @SiteId = {1}", internalName, siteId);
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

        public Contexts GetContextsForUser(string userName, int siteId, Guid folderId)
        {
            Contexts contexts = new Contexts();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(siteId)});
                nvl.Add(new ListItem() { Name = "@FolderId", Value = Convert.ToString(folderId)});

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgContextByUser", nvl);
                contexts = utility.Utility.Deserialize<org.iringtools.applicationConfig.Contexts>(xmlString, true);    
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Contexts: " + ex);
            }
            return contexts;
        }

        public Applications GetApplicationsForUser(string userName, int siteId, Guid contextId)
        {
            Applications applications = new Applications();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(siteId) });
                nvl.Add(new ListItem() { Name = "@ContextId", Value = Convert.ToString(contextId) });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgApplicationByUser", nvl);
                applications = utility.Utility.Deserialize<Applications>(xmlString, true);    
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Applications: " + ex);
            }
            return applications;
        }

        public Folders GetFoldersForUser(string userName, int siteId, Guid parentFolderId)
        {
            Folders folders = new Folders();
            try
            {
                if (parentFolderId == Guid.Empty)
                {
                    parentFolderId = new Guid("00000000-1111-2222-3333-444444444444");
                }

                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(siteId) });
                nvl.Add(new ListItem() { Name = "@FolderId", Value = Convert.ToString(parentFolderId) });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgFolderByUser", nvl);
                folders = utility.Utility.Deserialize<Folders>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Folders: " + ex);
            }
            return folders;
        }

        
        public Graphs GetGraphsForUser(string userName, int siteId, Guid applicationId)
        {
            Graphs graphs = new Graphs();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(siteId) });
                nvl.Add(new ListItem() { Name = "@ApplicationId", Value = Convert.ToString(applicationId) });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgGraphByUser", nvl);
                graphs = utility.Utility.Deserialize<Graphs>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Graphs: " + ex);
            }
            return graphs;
        }

        public Graphs GetGraphMappingForUser(string userName, int siteId, Guid graphId)
        {
            Graphs graphs = new Graphs();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(siteId) });
                nvl.Add(new ListItem() { Name = "@GraphId", Value = Convert.ToString(graphId) });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgGraphMappingByUser", nvl);
                graphs = utility.Utility.Deserialize<Graphs>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Graphs: " + ex);
            }
            return graphs;
        }

        public Applications GetAllApplications(string scopeInternalName)
        {
            Applications applications = new Applications();

            try
            {
                List<Application> lstApplication = new List<Application>();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    lstApplication = dc.ExecuteQuery<Application>("spgApplication @ScopeInternalName = {0}, @SiteId = {1}",
                                                                  scopeInternalName, _siteID).ToList();
                }
   
                applications.AddRange(lstApplication);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Applications: " + ex);
            }

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
            Graphs graphs = new Graphs();
            try
            {
                List<Graph> lstGraph = new List<Graph>();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    lstGraph = dc.ExecuteQuery<Graph>("spgGraph").ToList();
                }

                graphs.AddRange(lstGraph);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Graphs: " + ex);
            }
            return graphs;
        }

        public Response InsertGraph(XDocument xml)
        {
            Response response = new Response();

            try
            {
                Graphs graphs = Utility.DeserializeDataContract<Graphs>(xml.ToString());
               
                using (var dc = new DataContext(_connSecurityDb))
                {
                    foreach (Graph graph in graphs)
                    {
                        //if (graph.graph == null)  ////For testing purpose.
                        //{
                        //    string grapthPath = @"C:\Branch3.0\iRINGTools.Services\App_Data\Mapping.1234_000.ABC.xml";
                        //    byte[] bytes = System.IO.File.ReadAllBytes(grapthPath);
                        //    graph.graph = bytes;
                        //}

                        dc.ExecuteQuery<Graph>("spiGraphs @ApplicationId = {0}, @GraphName = {1}, @Graph = {2}, @SiteId = {3}",
                                                         graph.ApplicationId, graph.GraphName, graph.graph, _siteID).ToList();
                    }
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Graphs added successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Graphs: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }


        public Response UpdateGraphs(XDocument xml)
        {
            Response response = new Response();

            try
            {
                Graphs graphs = Utility.DeserializeDataContract<Graphs>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    foreach (Graph graph in graphs)
                    {
                        if (graph.GraphId == Guid.Empty)
                        {
                            throw new Exception("Please provide the GraphId of the Graph in" +
                                                  " the payload which you want to update.");
                        }

                        dc.ExecuteQuery<Graph>("spuGraphs @GraphId = {0}, @GraphName = {1}, @Graph = {2}, " +
                                                "@SiteId = {3}", graph.GraphId, graph.GraphName, graph.graph, _siteID).ToList();
                    }
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Graphs updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating Graphs: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteGraph(string graphId)
        {
            Response response = new Response();

            try
            {
                Guid id = new Guid(graphId);
                using (var dc = new DataContext(_connSecurityDb))
                {
                    dc.ExecuteQuery<Graph>("spdGraph @GraphId = {0}, @SiteId = {1}", id, _siteID);
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Graph deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Graph: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
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

        public XElement FormatIncomingMessage<T>(Stream stream, string format, bool isBase64encoded = false)
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

                if (isBase64encoded)
                    xElement = dataItems.Serialize<T>();
                else
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

        public DataFilters GetDataFiltersForUser(string userName, int siteId, Guid resourceId)
        {
            DataFilters datafilters = new DataFilters();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(siteId) });
                nvl.Add(new ListItem() { Name = "@ResourceId", Value = Convert.ToString(resourceId) });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgDataFilterByUser", nvl);
                datafilters = utility.Utility.Deserialize<org.iringtools.applicationConfig.DataFilters>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Contexts: " + ex);
            }
            return datafilters;
        }

        public Exchanges GetExchangesForUser(string userName, int siteId, Guid commodityId)
        {
            Exchanges exchanges = new Exchanges();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(siteId) });
                nvl.Add(new ListItem() { Name = "@CommodityId", Value = Convert.ToString(commodityId) });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgExchangesByUser", nvl);
                exchanges = utility.Utility.Deserialize<org.iringtools.applicationConfig.Exchanges>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Contexts: " + ex);
            }
            return exchanges;
        }

        public Commodities GetCommoditiesForUser(string userName, int siteId, Guid contextId)
        {
            Commodities commodities = new Commodities();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(siteId) });
                nvl.Add(new ListItem() { Name = "@ContextId", Value = Convert.ToString(contextId) });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgCommoditiesByUser", nvl);
                commodities = utility.Utility.Deserialize<org.iringtools.applicationConfig.Commodities>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Contexts: " + ex);
            }
            return commodities;
        }

    }
}
