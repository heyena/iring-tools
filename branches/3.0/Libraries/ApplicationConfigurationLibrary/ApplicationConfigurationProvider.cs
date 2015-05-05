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
using System.Globalization;
using org.iringtools.mapping;
using System.Net;
using org.iringtools.adapter.projection;
using org.iringtools.library.tip;

namespace org.iringtools.applicationConfig
{
    public class ApplicationConfigurationProvider : BaseProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ApplicationConfigurationProvider));
        private static readonly int DEFAULT_PAGE_SIZE = 25;
        //private int _siteID;
        private mapping.GraphMap _graphMap = null;
        private DataObject _dataObjDef = null;
        private IProjectionLayer _projectionEngine = null;
        private bool _isResourceGraph = false;
        private List<IDataObject> _dataObjects = new List<IDataObject>();
        private bool _isProjectionPart7 = false;
        private bool _isFormatExpected = true;

        private string[] arrSpecialcharlist;
        private string[] arrSpecialcharValue;

        [Inject]
        public ApplicationConfigurationProvider(NameValueCollection settings)
            : base(settings)
        {
            try
            {
                if (_settings["SpCharList"] != null && _settings["SpCharValue"] != null)
                {
                    arrSpecialcharlist = _settings["SpCharList"].ToString().Split(',');
                    arrSpecialcharValue = _settings["SpCharValue"].ToString().Split(',');
                }

                if (_settings["LdapConfiguration"] != null && _settings["LdapConfiguration"].ToLower() == "true")
                {
                    utility.Utility.isLdapConfigured = true;
                    utility.Utility.InitializeConfigurationRepository(new Type[] { typeof(DataDictionary), typeof(DatabaseDictionary), typeof(XElementClone), typeof(AuthorizedUsers), typeof(Mapping) });
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error initializing application configuration provider: " + e.Message);
            }
        }

        public Contexts GetAllContexts(int siteId)
        {
            Contexts contexts = new Contexts();
            try
            {
                //if (siteId == 0)
                //{
                //    siteId = _siteID;
                //}

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
            response.Messages = new Messages();

            try
            {
                Context context = Utility.DeserializeDataContract<Context>(xml.ToString());

                string rawXml = context.Groups.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (context == null || string.IsNullOrEmpty(context.DisplayName))
                        PrepareErrorResponse(response, "Please enter Context DisplayName!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@DisplayName", Value = context.DisplayName });
                        nvl.Add(new ListItem() { Name = "@InternalName", Value = context.InternalName });
                        nvl.Add(new ListItem() { Name = "@Description", Value = context.Description });
                        nvl.Add(new ListItem() { Name = "@CacheConnStr", Value = context.CacheConnStr });
                        nvl.Add(new ListItem() { Name = "@FolderId", Value = context.FolderId });
                        nvl.Add(new ListItem() { Name = "@GroupList", Value = rawXml });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiContext", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "contextadded");
                                break;
                            case "0":
                                PrepareSuccessResponse(response, "duplicatecontext");
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
            response.Messages = new Messages();

            try
            {
                Context context = Utility.DeserializeDataContract<Context>(xml.ToString());

                string rawXml = context.Groups.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (context == null || string.IsNullOrEmpty(context.DisplayName))
                        PrepareErrorResponse(response, "Please enter context DisplayName!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@DisplayName", Value = context.DisplayName });
                        nvl.Add(new ListItem() { Name = "@Description", Value = context.Description });
                        nvl.Add(new ListItem() { Name = "@CacheConnStr", Value = context.CacheConnStr });
                        nvl.Add(new ListItem() { Name = "@ContextId", Value = context.ContextId });
                        nvl.Add(new ListItem() { Name = "@ParentFolderId", Value = context.FolderId });
                        nvl.Add(new ListItem() { Name = "@GroupList", Value = rawXml });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuContext", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "contextupdated");
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
                _logger.Error("Error updating Contexts: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteContext(string contextId)
        {
            Response response = new Response();
            response.Messages = new Messages();


            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (string.IsNullOrEmpty(contextId))
                        PrepareErrorResponse(response, "Please enter contextid!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@ContextId", Value = contextId });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdContext", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "contextdeleted");
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
                _logger.Error("Error deleting Context: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Contexts GetContextsForUser(string userName, Guid folderId)
        {
            Contexts contexts = new Contexts();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@FolderId", Value = folderId});

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgContextByUser", nvl);
                contexts = utility.Utility.Deserialize<org.iringtools.applicationConfig.Contexts>(xmlString, true);    
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Contexts: " + ex);
            }
            return contexts;
        }

        public Applications GetApplicationsForUser(string userName, Guid contextId)
        {
            Applications applications = new Applications();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@ContextId", Value = contextId });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgApplicationByUser", nvl);
                applications = utility.Utility.Deserialize<Applications>(xmlString, true);    
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Applications: " + ex);
            }
            return applications;
        }

        public Response DragAndDropEntity(string resourceType, Guid SourceId, Guid destinationId, int siteId, int platformId)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@ResourceType ", Value = resourceType });
                nvl.Add(new ListItem() { Name = "@SourceId", Value = SourceId });
                nvl.Add(new ListItem() { Name = "@DestinationId", Value = destinationId });
                nvl.Add(new ListItem() { Name = "@SiteId ", Value = siteId });
                nvl.Add(new ListItem() { Name = "@PlatFormId", Value = platformId });
                string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiDragAndDropEntity", nvl);

                switch (output)
                {
                    case "1":
                        PrepareSuccessResponse(response, "nodeCopied");
                        break;
                    case "-1":
                        PrepareSuccessResponse(response, "destinationFolderDeleted");
                        break;
                    case "-2":
                        PrepareSuccessResponse(response, "sourceFolderDeleted");
                        break;
                    case "-3":
                        PrepareSuccessResponse(response, "duplicateFolder");
                        break;
                    case "-4":
                        PrepareSuccessResponse(response, "sourceContextDeleted");
                        break;
                    case "-5":
                        PrepareSuccessResponse(response, "duplicateContext");
                        break;
                    case "-6":
                        PrepareSuccessResponse(response, "destinationContextDeleted");
                        break;
                    case "-7":
                        PrepareSuccessResponse(response, "sourceApplicationDeleted");
                        break;
                    case "-8":
                        PrepareSuccessResponse(response, "duplicateApplication");
                        break;
                    default:
                        PrepareErrorResponse(response, output);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Folder: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }
            return response;
        }

        public Folders GetFoldersForUser(string userName, int siteId, int platformId, Guid parentFolderId)
        {
            Folders folders = new Folders();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@SiteId", Value = siteId });
                nvl.Add(new ListItem() { Name = "@PlatformId", Value = platformId });
                nvl.Add(new ListItem() { Name = "@ParentFolderOrSiteId", Value = parentFolderId });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgFolderByUser", nvl);
                folders = utility.Utility.Deserialize<Folders>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Folders: " + ex);
            }
            return folders;
        }
        
        public Response InsertFolder(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                Folder folder = Utility.DeserializeDataContract<Folder>(xml.ToString());

                string rawXml = folder.Groups.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (folder == null || string.IsNullOrEmpty(folder.FolderName))
                        PrepareErrorResponse(response, "Please enter FolderName!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = folder.SiteId });
                        nvl.Add(new ListItem() { Name = "@PlatformId", Value = folder.PlatformId });
                        nvl.Add(new ListItem() { Name = "@ParentFolderId", Value = folder.ParentFolderId });
                        nvl.Add(new ListItem() { Name = "@FolderName", Value = folder.FolderName });
                        nvl.Add(new ListItem() { Name = "@GroupList", Value = rawXml });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiFolder", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "folderadded");
                                break;
                            case "0":
                                PrepareSuccessResponse(response, "duplicatefolder");
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
                _logger.Error("Error adding Folder: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateFolder(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                Folder folder = Utility.DeserializeDataContract<Folder>(xml.ToString());
                string rawXml = folder.Groups.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (folder == null || string.IsNullOrEmpty(folder.FolderName))
                        PrepareErrorResponse(response, "Please enter FolderName!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = folder.SiteId });
                        nvl.Add(new ListItem() { Name = "@PlatformId", Value = folder.PlatformId });
                        nvl.Add(new ListItem() { Name = "@FolderId", Value = folder.FolderId.ToString() });
                        nvl.Add(new ListItem() { Name = "@ParentFolderId", Value = folder.ParentFolderId.ToString() });
                        nvl.Add(new ListItem() { Name = "@FolderName", Value = folder.FolderName });
                        nvl.Add(new ListItem() { Name = "@GroupList", Value = rawXml });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuFolder", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "folderupdated");
                                break;
                            case "0":
                                PrepareSuccessResponse(response, "duplicatefolder");
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
                _logger.Error("Error updating Folder: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteFolder(string folderId)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (string.IsNullOrEmpty(folderId))
                        PrepareErrorResponse(response, "Please enter folderid!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@FolderId", Value = folderId });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdFolder", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "folderdeleted");
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
                _logger.Error("Error deleting Folder: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Graphs GetGraphsForUser(string userName,  Guid applicationId)
        {
            Graphs graphs = null;// = new Graphs();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });              
                nvl.Add(new ListItem() { Name = "@ApplicationId", Value = Convert.ToString(applicationId) });

                
                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgGraphByUser", nvl);
                if(string.IsNullOrEmpty(xmlString))
                     graphs = new Graphs();
                else
                graphs = utility.Utility.Deserialize<Graphs>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Graphs: " + ex);
            }
            return graphs;
        }

        public Graphs GetGraphMappingForUser(string userName, Guid graphId)
        {
            Graphs graphs = new Graphs();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
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
                    //TODO: Need to identify the usage of commented old code as the _siteID will not be present in the provider and rather has to be send from application if required.
                    //lstApplication = dc.ExecuteQuery<Application>("spgApplication @ScopeInternalName = {0}, @SiteId = {1}",
                    //                                              scopeInternalName, _siteID).ToList();
                    lstApplication = dc.ExecuteQuery<Application>("spgApplication @ScopeInternalName = {0}", scopeInternalName).ToList();
                }
   
                applications.AddRange(lstApplication);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Applications: " + ex);
            }

            return applications;
        }

        public Response InsertApplication(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                Application application = Utility.DeserializeDataContract<Application>(xml.ToString());

                ApplicationDataModeAndBindingHandling(ref application);


                string groupXml = application.Groups.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                string appSettingsXml = application.ApplicationSettings.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                string bindingXml = application.Binding.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (application == null || string.IsNullOrEmpty(application.DisplayName))
                        PrepareErrorResponse(response, "Please enter Application DisplayName!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@ContextId", Value = application.ContextId });
                        nvl.Add(new ListItem() { Name = "@InternalName", Value = application.InternalName });
                        nvl.Add(new ListItem() { Name = "@DisplayName", Value = application.DisplayName });
                        nvl.Add(new ListItem() { Name = "@Description", Value = application.Description });
                        nvl.Add(new ListItem() { Name = "@DXFRUrl", Value = application.DXFRUrl });
                        nvl.Add(new ListItem() { Name = "@Assembly", Value = application.Assembly });
                        nvl.Add(new ListItem() { Name = "@GroupList", Value = groupXml });
                        nvl.Add(new ListItem() { Name = "@AppSettingsList", Value = appSettingsXml });
                        nvl.Add(new ListItem() { Name = "@DataMode", Value = application.ApplicationDataMode.ToString() });
                        nvl.Add(new ListItem() { Name = "@Binding", Value = bindingXml });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiApplication", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "applicationAdded");
                                break;
                            case "0":
                                PrepareSuccessResponse(response, "duplicateApplication");
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
                _logger.Error("Error adding Applications: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateApplication(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                Application application = Utility.DeserializeDataContract<Application>(xml.ToString());

                ApplicationDataModeAndBindingHandling(ref application);

                string groupXml = application.Groups.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                string appSettingsXml = application.ApplicationSettings.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                string bindingXml = application.Binding.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (application == null || string.IsNullOrEmpty(application.DisplayName))
                        PrepareErrorResponse(response, "Please enter application DisplayName!");
                    else
                    {
                        NameValueList nvl = new NameValueList();

                        nvl.Add(new ListItem() { Name = "@ContextId", Value = application.ContextId });
                        nvl.Add(new ListItem() { Name = "@ApplicationId", Value = application.ApplicationId });
                        nvl.Add(new ListItem() { Name = "@DisplayName", Value = application.DisplayName });
                        nvl.Add(new ListItem() { Name = "@Description", Value = application.Description });
                        nvl.Add(new ListItem() { Name = "@DXFRUrl", Value = application.DXFRUrl });
                        nvl.Add(new ListItem() { Name = "@Assembly", Value = application.Assembly });
                        nvl.Add(new ListItem() { Name = "@GroupList", Value = groupXml });
                        nvl.Add(new ListItem() { Name = "@AppSettingsList", Value = appSettingsXml });
                        nvl.Add(new ListItem() { Name = "@DataMode", Value = application.ApplicationDataMode.ToString() });
                        nvl.Add(new ListItem() { Name = "@Binding", Value = bindingXml });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuApplication", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "applicationUpdated");
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
                _logger.Error("Error updating Applications: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteApplication(string applicationId)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (string.IsNullOrEmpty(applicationId))
                        PrepareErrorResponse(response, "Please enter applicationId!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@ApplicationId", Value = applicationId });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdApplication", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "applicationdeleted");
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

        public Response InsertGraph(string userName, XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {

                Graph graph = Utility.DeserializeDataContract<Graph>(xml.ToString());

                string rawXml = graph.Groups.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (graph == null || string.IsNullOrEmpty(graph.GraphName))
                        PrepareErrorResponse(response, "Please enter GraphName!");
                    else
                    {
                        //if (graph.graph == null)  ////For testing purpose.
                        //{
                        //    string grapthPath = @"C:\Branch3.0\iRINGTools.Services\App_Data\Mapping.1234_000.ABC.xml";
                        //    byte[] bytes = System.IO.File.ReadAllBytes(grapthPath);
                        //    graph.graph = bytes;
                        //}


                        NameValueList nvl = new NameValueList();

                        nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                        nvl.Add(new ListItem() { Name = "@ApplicationId", Value = Convert.ToString(graph.ApplicationId) });
                        nvl.Add(new ListItem() { Name = "@GraphName", Value = graph.GraphName });
                        nvl.Add(new ListItem() { Name = "@GroupList", Value = rawXml });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedureWithExtraParam_ByteArray(_connSecurityDb, "spiGraph", nvl, "@Graph", graph.graph);

                        
                        switch (output.ToString())
                        {
                            case "1":
                                PrepareSuccessResponse(response, "graphadded");
                                break;
                            case "0":
                                PrepareSuccessResponse(response, "duplicategraph");
                                break;
                            default:
                                PrepareErrorResponse(response, output.ToString());
                                break;
                        }
                    }

                }

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

        public Response UpdateGraph(string userName, XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                Graph graph = Utility.DeserializeDataContract<Graph>(xml.ToString());

                string rawXml = graph.Groups.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (graph == null || string.IsNullOrEmpty(graph.GraphName))
                        PrepareErrorResponse(response, "Please enter GraphName!");
                    else
                    {
                        //if (graph.graph == null)  ////For testing purpose.
                        //{
                        //    string grapthPath = @"C:\Branch3.0\iRINGTools.Services\App_Data\Mapping.1234_000.ABC.xml";
                        //    byte[] bytes = System.IO.File.ReadAllBytes(grapthPath);
                        //    graph.graph = bytes;
                        //}


                        NameValueList nvl = new NameValueList();

                        nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                        nvl.Add(new ListItem() { Name = "@GraphId", Value = Convert.ToString(graph.GraphId) });
                        nvl.Add(new ListItem() { Name = "@GraphName", Value = graph.GraphName });
                        nvl.Add(new ListItem() { Name = "@GroupList", Value = rawXml });
                        nvl.Add(new ListItem() { Name = "@ApplicationId", Value = Convert.ToString(graph.ApplicationId) });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedureWithExtraParam_ByteArray(_connSecurityDb, "spuGraph", nvl, "@Graph", graph.graph);


                        switch (output.ToString())
                        {
                            case "1":
                                PrepareSuccessResponse(response, "graphupdated");
                                break;
                            case "0":
                                PrepareSuccessResponse(response, "duplicategraph");
                                break;
                            default:
                                PrepareErrorResponse(response, output.ToString());
                                break;
                        }
                    }

                }
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
            response.Messages = new Messages();

            try
            {
                //Guid id = new Guid(graphId);
                //using (var dc = new DataContext(_connSecurityDb))
                //{
                //    dc.ExecuteQuery<Graph>("spdGraph @GraphId = {0}", id);
                //}

                //response.DateTimeStamp = DateTime.Now;
                //response.Messages = new Messages();
                //response.Messages.Add("Graph deleted successfully.");

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (string.IsNullOrEmpty(graphId))
                        PrepareErrorResponse(response, "Please enter graphId!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@GraphId", Value = graphId });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdGraph", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "graphdeleted");
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

        public DataFilters GetDataFilters(Guid resourceId)
        {
            DataFilters datafilters = new DataFilters();

            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@ResourceId", Value = resourceId });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgDataFilter", nvl);
                datafilters = utility.Utility.Deserialize<DataFilters>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Contexts: " + ex);
            }

            return datafilters;
        }

        public Response InsertDataFilter(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                string rawXml = xml.ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                using (var dc = new DataContext(_connSecurityDb))
                {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@rawXml", Value = rawXml });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiDataFilter", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "datafiltersadded");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    

                }

                //using (var dc = new DataContext(_connSecurityDb))
                //{
                //    NameValueList nvl = new NameValueList();
                //    nvl.Add(new ListItem() { Name = "@rawXml", Value = rawXml });
                //    DBManager.Instance.ExecuteNonQueryStoredProcedure(_connSecurityDb, "spiDataFilter", nvl);

                //}

                //response.DateTimeStamp = DateTime.Now;
                //response.Messages = new Messages();
                //response.Messages.Add("DataFilter added successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding DataFilter: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        //public Response UpdateDataFilter(string resourceId, string siteId, string dataFilterTypeId, XDocument xml)
        //{
        //    Response response = new Response();

        //    try
        //    {
        //        //Exchange exchange = Utility.DeserializeDataContract<Exchange>(xml.ToString());

        //        string rawXml = xml.ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

        //        using (var dc = new DataContext(_connSecurityDb))
        //        {
        //            NameValueList nvl = new NameValueList();
        //            nvl.Add(new ListItem() { Name = "@ResourceId", Value = resourceId });
        //            nvl.Add(new ListItem() { Name = "@DataFilterTypeId", Value = dataFilterTypeId });
        //            nvl.Add(new ListItem() { Name = "@SiteId", Value = siteId });
        //            nvl.Add(new ListItem() { Name = "@RawXml", Value = rawXml });
        //            DBManager.Instance.ExecuteNonQueryStoredProcedure(_connSecurityDb, "spuDataFilter", nvl);

        //        }

        //        response.DateTimeStamp = DateTime.Now;
        //        response.Messages = new Messages();
        //        response.Messages.Add("DataFilter updated successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error("Error udpating DataFilter: " + ex);

        //        Status status = new Status { Level = StatusLevel.Error };
        //        status.Messages = new Messages { ex.Message };

        //        response.DateTimeStamp = DateTime.Now;
        //        response.Level = StatusLevel.Error;
        //        response.StatusList.Add(status);
        //    }

        //    return response;
        //}

        public Response DeleteDataFilter(string resourceId)
        {
            Response response = new Response();

            try
            {

                using (var dc = new DataContext(_connSecurityDb))
                {
                    NameValueList nvl = new NameValueList();
                    nvl.Add(new ListItem() { Name = "@ResourceId", Value = resourceId });
                    DBManager.Instance.ExecuteNonQueryStoredProcedure(_connSecurityDb, "spdDataFilter", nvl);
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("DataFilter deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting DataFilter: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Exchanges GetExchangesForUser(string userName, Guid commodityId)
        {
            Exchanges exchanges = new Exchanges();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
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

        public Response InsertExchange(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                Exchange exchange = Utility.DeserializeDataContract<Exchange>(xml.ToString());

                string rawXml = exchange.Groups.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (exchange == null || string.IsNullOrEmpty(exchange.Name))
                        PrepareErrorResponse(response, "Please enter Exchange Name!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@CommodityId", Value = Convert.ToString(exchange.CommodityId) });
                        nvl.Add(new ListItem() { Name = "@SourceGraphId", Value = Convert.ToString(exchange.SourceGraphId) });
                        nvl.Add(new ListItem() { Name = "@DestinationGraphId", Value = Convert.ToString(exchange.DestinationGraphId) });
                        nvl.Add(new ListItem() { Name = "@Name", Value = exchange.Name });
                        nvl.Add(new ListItem() { Name = "@Description", Value = exchange.Description });
                        nvl.Add(new ListItem() { Name = "@PoolSize", Value = Convert.ToString(exchange.PoolSize) });
                        nvl.Add(new ListItem() { Name = "@XTypeAdd", Value = exchange.XTypeAdd });
                        nvl.Add(new ListItem() { Name = "@XTypeChange", Value = exchange.XTypeChange });
                        nvl.Add(new ListItem() { Name = "@XTypeSync", Value = exchange.XTypeSync });
                        nvl.Add(new ListItem() { Name = "@XTypeDelete", Value = exchange.XTypeDelete });
                        nvl.Add(new ListItem() { Name = "@XTypeSetNull", Value = exchange.XTypeSetNull });                    
                        nvl.Add(new ListItem() { Name = "@GroupList", Value = rawXml });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiExchange", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "exchangeadded");
                                break;
                            case "0":
                                PrepareWarningResponse(response, "duplicateexchange");
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
                _logger.Error("Error adding Exchange: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateExchange(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                Exchange exchange = Utility.DeserializeDataContract<Exchange>(xml.ToString());

                string rawXml = exchange.Groups.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (exchange == null || string.IsNullOrEmpty(exchange.Name))
                        PrepareErrorResponse(response, "Please enter Exchange Name!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@ExchangeId", Value = Convert.ToString(exchange.ExchangeId) });
                        nvl.Add(new ListItem() { Name = "@SourceGraphId", Value = Convert.ToString(exchange.SourceGraphId) });
                        nvl.Add(new ListItem() { Name = "@DestinationGraphId", Value = Convert.ToString(exchange.DestinationGraphId) });
                        nvl.Add(new ListItem() { Name = "@Name", Value = exchange.Name });
                        nvl.Add(new ListItem() { Name = "@Description", Value = exchange.Description });
                        nvl.Add(new ListItem() { Name = "@PoolSize", Value = Convert.ToString(exchange.PoolSize) });
                        nvl.Add(new ListItem() { Name = "@XTypeAdd", Value = exchange.XTypeAdd });
                        nvl.Add(new ListItem() { Name = "@XTypeChange", Value = exchange.XTypeChange });
                        nvl.Add(new ListItem() { Name = "@XTypeSync", Value = exchange.XTypeSync });
                        nvl.Add(new ListItem() { Name = "@XTypeDelete", Value = exchange.XTypeDelete });
                        nvl.Add(new ListItem() { Name = "@XTypeSetNull", Value = exchange.XTypeSetNull });
                        nvl.Add(new ListItem() { Name = "@GroupList", Value = rawXml });
                        nvl.Add(new ListItem() { Name = "@CommodityId", Value = exchange.CommodityId.ToString() });


                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuExchange", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "exchangeupdated");
                                break;
                            case "0":
                                PrepareWarningResponse(response, "duplicateexchange");
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
                _logger.Error("Error updating Exchange: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteExchange(string exchangeId)
        {
            Response response = new Response();

            try
            {

                using (var dc = new DataContext(_connSecurityDb))
                {
                    dc.ExecuteCommand("spdExchange @ExchangeId = {0} ", exchangeId);
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Exchange deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Exchange: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Commodities GetCommoditiesForUser(string userName,  Guid contextId)
        {
            Commodities commodities = new Commodities();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@ContextId", Value = Convert.ToString(contextId) });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgCommoditiesByUser", nvl);
                commodities = utility.Utility.Deserialize<org.iringtools.applicationConfig.Commodities>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Commodity: " + ex);
            }
            return commodities;
        }

        public Response InsertCommodity(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                Commodity commodity = Utility.DeserializeDataContract<Commodity>(xml.ToString());

                string rawXml = commodity.Groups.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (commodity == null || string.IsNullOrEmpty(commodity.CommodityName))
                        PrepareErrorResponse(response, "Please enter CommodityName!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@ContextId", Value = commodity.ContextId.ToString() });
                        nvl.Add(new ListItem() { Name = "@CommodityName", Value = commodity.CommodityName });
                        nvl.Add(new ListItem() { Name = "@GroupList", Value = rawXml });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiCommodity", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "commodityadded");
                                break;
                            case "0":
                                PrepareWarningResponse(response, "duplicatecommodity");
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
                _logger.Error("Error adding Commodity: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateCommodity(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                Commodity commodity = Utility.DeserializeDataContract<Commodity>(xml.ToString());
                string rawXml = commodity.Groups.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (commodity == null || string.IsNullOrEmpty(commodity.CommodityName))
                        PrepareErrorResponse(response, "Please enter CommodityName!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@CommodityId", Value = commodity.CommodityId.ToString() });
                        nvl.Add(new ListItem() { Name = "@CommodityName", Value = commodity.CommodityName });
                        nvl.Add(new ListItem() { Name = "@GroupList", Value = rawXml });
                        nvl.Add(new ListItem() { Name = "@ContextId", Value = commodity.ContextId.ToString() });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuCommodity", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "commodityupdated");
                                break;
                            case "0":
                                PrepareWarningResponse(response, "duplicatecommodity");
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
                _logger.Error("Error updating Commodity: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteCommodity(string comodityId)
        {
            Response response = new Response();

            try
            {

                using (var dc = new DataContext(_connSecurityDb))
                {
                    dc.ExecuteCommand("spdCommodity @CommodityId = {0} ", comodityId);
                }

                response.DateTimeStamp = DateTime.Now;
                response.Messages = new Messages();
                response.Messages.Add("Commodity deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Commodity: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public ValueListMaps GetValueListForUser(string userName, Guid applicationId)
        {
            ValueListMaps valueListMaps = new ValueListMaps();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@ApplicationId", Value = Convert.ToString(applicationId) });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgValuelist", nvl);
                valueListMaps = utility.Utility.Deserialize<ValueListMaps>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  valueListMaps: " + ex);
            }
            return valueListMaps;
        }

        public Graph GetGraphByGraphID(string userName,Guid graphID)
        {
            Graph graphs = new Graph();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@GraphId", Value = Convert.ToString(graphID) });
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgGraphByGraphID", nvl);
                graphs = utility.Utility.Deserialize<Graph>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Graph by GraphID: " + ex);
            }
            return graphs;
        }

        public Application GetApplicationByApplicationID(string userName, Guid applicationID)
        {
            Application application = new Application();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@ApplicationId", Value = Convert.ToString(applicationID) });
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgApplicationByApplicationID", nvl);
                application = utility.Utility.Deserialize<Application>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Application By ApplicationID: " + ex);
            }
            return application;
        }

        public DataLayers GetDataLayers(int siteId, int platformId)
        {
            DataLayers dataLayers = new DataLayers();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@SiteId", Value = siteId });
                nvl.Add(new ListItem() { Name = "@PlatformId", Value = platformId });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgDataLayers", nvl);
                dataLayers = utility.Utility.Deserialize<DataLayers>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  valueListMaps: " + ex);
            }
            return dataLayers;
        }

        public ApplicationSettings GetAppSettings()
        {
            ApplicationSettings settings = new ApplicationSettings();

            try
            {
                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgAppSettings", null);
                settings = utility.Utility.Deserialize<ApplicationSettings>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting Settings mapped with a user: " + ex);
                throw ex;
            }

            return settings;
        }

        public string GetNodesForCache(string nodeType, Guid nodeId, string userName)
        {
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@NodeType", Value = nodeType });
                nvl.Add(new ListItem() { Name = "@NodeId", Value = nodeId });
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgNodesForCache", nvl);

                return xmlString;
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  nodesForCache: " + ex);
                throw ex;
            }
        }

        public XDocument GetDataForDataObject(DataObject dataObject, ref string format, int start, int limit, bool fullIndex)
        {
            try
            {
                Application parentApplicationOfDataObject = GetApplicationForDataObject(dataObject.dataObjectId);
                Context parentContextOfDataObject = GetContextForDataObject(dataObject.dataObjectId);
                DatabaseDictionary tempDatabaseDictionary = GetDictionary(parentApplicationOfDataObject.ApplicationId);

                _settings["ProjectName"] = parentApplicationOfDataObject.InternalName;
                _settings["ApplicationName"] = parentContextOfDataObject.InternalName;

                DataDictionary dictionaryFromDB = (DataDictionary)tempDatabaseDictionary;

                AddURIsInSettingCollection(dataObject);

                _mapping = new mapping.Mapping();

                _logger.Debug("Initializing DataLayer.");
                InitializeDataLayer(parentApplicationOfDataObject, ref dictionaryFromDB);

                UpdateDictionary(XDocument.Parse(Utility.Serialize<DatabaseDictionary>((DatabaseDictionary)dictionaryFromDB, true)));

                _logger.DebugFormat("Initializing Projection: {0} as {1}", dataObject.objectName, format);
                InitializeProjection(dataObject, ref format, false);

                _projectionEngine.Start = start;
                _projectionEngine.Limit = limit;

                IList<string> index = new List<string>();

                if (limit == 0)
                {
                    limit = (_settings["DefaultPageSize"] != null) ? int.Parse(_settings["DefaultPageSize"]) : DEFAULT_PAGE_SIZE;
                }

                _logger.DebugFormat("Getting DataObjects Page: {0} {1}", start, limit);
                _dataObjects = _dataLayerGateway.Get(dataObject, parentApplicationOfDataObject.ApplicationDataMode, dataObject.dataFilter, start, limit);

                _projectionEngine.Count = _dataLayerGateway.GetCount(dataObject, parentApplicationOfDataObject.ApplicationDataMode, dataObject.dataFilter);
                _logger.DebugFormat("DataObjects Total Count: {0}", _projectionEngine.Count);

                _projectionEngine.FullIndex = fullIndex;

                if (_isProjectionPart7)
                {
                    return _projectionEngine.ToXml(_graphMap.name, ref _dataObjects);
                }
                else
                {
                    return _projectionEngine.ToXml(dataObject.objectName, ref _dataObjects);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetProjection: {0}", ex));
                throw ex;
            }
        }

        public Exchange GetExchangeByExchangeID(string userName, Guid exchangeID)
        {
            Exchange exchange = new Exchange();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@ExchangeId", Value = Convert.ToString(exchangeID) });
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgExchangeByExchangeID", nvl);
                exchange = utility.Utility.Deserialize<Exchange>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Exchange By ExchangeID: " + ex);
            }
            return exchange;
        }

        public string GetDataModeByDataObjectId(Guid dataObjectId)
        {
            string output = string.Empty;

            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    NameValueList nvl = new NameValueList();
                    nvl.Add(new ListItem() { Name = "@DataObjectId", Value = dataObjectId });

                    output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spgDataModeByDataObjectId", nvl);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error Getting DataMode By DataObjectId: " + ex);
            
            }

            return output;
        }

        /// <summary>
        /// insert job
        /// </summary>
        /// <param name="response"></param>
        /// <param name="errMsg"></param>

        //  public Response InsertJob(Job job )
        //public Response InsertJob(XDocument xml)
        //{
        //    Response response = new Response();
        //    response.Messages = new Messages();

        //    try
        //    {
        //        Job job = Utility.DeserializeDataContract<Job>(xml.ToString());

        //        using (var dc = new DataContext(_connSecurityDb))
        //        {
        //            NameValueList nvl = new NameValueList();

        //            nvl.Add(new ListItem() { Name = "@Is_Exchange", Value = Convert.ToString(job.Is_Exchange) });
        //            nvl.Add(new ListItem() { Name = "@Scope", Value = job.scope });
        //            nvl.Add(new ListItem() { Name = "@App", Value = job.app });
        //            nvl.Add(new ListItem() { Name = "@DataObject", Value = job.dataobject });
        //            nvl.Add(new ListItem() { Name = "@Xid", Value = job.xid });
        //            nvl.Add(new ListItem() { Name = "@Exchange_Url", Value = job.Exchange_Url });
        //            nvl.Add(new ListItem() { Name = "@Cache_Page_Size", Value = job.cache_page_size });

        //            string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiJob", nvl);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error("Error adding Job: " + ex);

        //        Status status = new Status { Level = StatusLevel.Error };
        //        status.Messages = new Messages { ex.Message };

        //        response.DateTimeStamp = DateTime.Now;
        //        response.Level = StatusLevel.Error;
        //        response.StatusList.Add(status);
        //    }

        //    return response;
        //}

        #region Private Methods

        private static void ApplicationDataModeAndBindingHandling(ref Application application)
        {
            //Handling DataLayer binding
            if (!string.IsNullOrEmpty(application.Assembly))
            {
                string typeName = application.Assembly.Split(new string[] { ", " }, StringSplitOptions.None)[0];
                string dataLayerName = application.Assembly.Split(new string[] { ", " }, StringSplitOptions.None)[1];

                application.Binding = new ApplicationBinding();
                application.Binding.ModuleName = "DataLayerBinding";
                application.Binding.BindName = "DataLayer";
                application.Binding.To = application.Assembly;
                application.Binding.Service = (typeof(IDataLayer)).AssemblyQualifiedName;

                System.Reflection.Assembly dataLayerAssembly = System.Reflection.Assembly.Load(dataLayerName);

                if (dataLayerAssembly.GetType(typeName).BaseType.ToString() == "org.iringtools.library.BaseLightweightDataLayer") //added for ilightweight2 datalayer
                {
                    if (typeof(ILightweightDataLayer).IsAssignableFrom(dataLayerAssembly.GetType(typeName)))
                    {
                        application.Binding.Service = (typeof(ILightweightDataLayer)).AssemblyQualifiedName;
                    }
                }
                else if (typeof(ILightweightDataLayer2).IsAssignableFrom(dataLayerAssembly.GetType(typeName)))
                {
                    application.Binding.Service = (typeof(ILightweightDataLayer2)).AssemblyQualifiedName;
                    application.ApplicationDataMode = applicationConfig.DataMode.Cache;
                }

                application.Binding.Service = application.Binding.Service.Remove(application.Binding.Service.IndexOf(", Version"));
            }
        }

        private void AddURIsInSettingCollection(DataObject dataObject, string resourceIdentifier = null, string relatedresource = null, string relatedId = null)
        {
            try
            {
                _logger.Debug("Adding URI in setting Collection.");

                if (dataObject.keyProperties.Count > 0)
                {
                    string keyPropertyName = dataObject.keyProperties[0].keyPropertyName;

                    string genericURI = "/" + dataObject.objectName;
                    string specificURI = "/" + dataObject.objectName;

                    if (resourceIdentifier != null)
                    {
                        genericURI = dataObject + "/{" + keyPropertyName + "}";
                        specificURI = dataObject + "/" + resourceIdentifier;
                    }

                    if (relatedresource != null)
                    {
                        genericURI = dataObject.objectName + "/{" + keyPropertyName + "}/" + relatedresource;
                        specificURI = dataObject.objectName + "/" + resourceIdentifier + "/" + relatedresource;
                    }
                    if (relatedId != null)
                    {
                        DataObject releteddataObject = _dictionary.dataObjects.Find(x => x.objectName.ToUpper() == relatedresource.ToUpper());

                        if (releteddataObject != null)
                        {
                            string reletedKeyPropertyName = releteddataObject.keyProperties[0].keyPropertyName;

                            genericURI = dataObject.objectName + "/{" + keyPropertyName + "}/" + reletedKeyPropertyName + "/{" + reletedKeyPropertyName + "}";
                            specificURI = dataObject.objectName + "/" + resourceIdentifier + "/" + reletedKeyPropertyName + "/" + relatedId;
                        }
                    }

                    _settings["GenericURI"] = genericURI;
                    _settings["SpecificURI"] = specificURI;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("Exception in Adding URI in setting Collection: {0}", ex));
                throw ex;
            }
        }

        private void InitializeProjection(DataObject dataObject, ref string format, bool isIndividual)
        {
            try
            {
                string[] expectedFormats = { 
              "rdf", 
              "dto",
              "xml",
              "json"
            };

                //_graphMap = _mapping.FindGraphMap(dataObject.objectName);

                //if (_graphMap != null)
                //{
                //    _isResourceGraph = true;
                //    dataObject = _dictionary.dataObjects.Find(o => o.objectName.ToUpper() == _graphMap.dataObjectName.ToUpper());

                //    if (dataObject == null || dataObject.isRelatedOnly)
                //    {
                //        _logger.Warn("Data object [" + _graphMap.dataObjectName + "] not found.");
                //        throw new WebFaultException(HttpStatusCode.NotFound);
                //    }
                //}
                //else
                //{
                    //dataObject = _dictionary.dataObjects.Find(o => o.objectName.ToUpper() == dataObject.objectName.ToUpper());

                    //if (dataObject == null || dataObject.isRelatedOnly)
                    //{
                    //    _logger.Warn("Resource [" + dataObject + "] not found.");
                    //    throw new WebFaultException(HttpStatusCode.NotFound);
                    //}
                //}

                if (format == null)
                {
                    if (isIndividual && !String.IsNullOrEmpty(dataObject.defaultProjectionFormat))
                    {
                        format = dataObject.defaultProjectionFormat;
                    }
                    else if (!String.IsNullOrEmpty(dataObject.defaultListProjectionFormat))
                    {
                        format = dataObject.defaultListProjectionFormat;
                    }
                    else if (!String.IsNullOrEmpty(_settings["DefaultProjectionFormat"]))
                    {
                        format = _settings["DefaultProjectionFormat"];
                    }
                    else
                    {
                        format = "json";
                    }
                }
                _isFormatExpected = expectedFormats.Contains(format.ToLower());

                if (format != null && _isFormatExpected)
                {
                    _projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());

                    if (_projectionEngine.GetType().BaseType == typeof(BasePart7ProjectionEngine))
                    {
                        _isProjectionPart7 = true;
                        if (_graphMap == null)
                        {
                            throw new FileNotFoundException("Requested resource [" + dataObject + "] cannot be rendered as Part7.");
                        }
                    }
                }
                else if (format == _settings["DefaultProjectionFormat"] && _isResourceGraph)
                {
                    format = "dto";
                    _projectionEngine = _kernel.Get<IProjectionLayer>("dto");
                    _isProjectionPart7 = true;
                }

                if (_projectionEngine != null && typeof(BasePart7ProjectionEngine).IsAssignableFrom(_projectionEngine.GetType()))
                    ((BasePart7ProjectionEngine)_projectionEngine).dataLayerGateway = _dataLayerGateway;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error initializing application: {0}", ex));
                throw ex;
            }
        }

        #endregion
    }
}