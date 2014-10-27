using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using log4net;
using org.iringtools.library;
using System.ServiceModel.Web;
using System.ComponentModel;
using org.iringtools.applicationConfig;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Net;

namespace org.iringtools.services
{
    [ServiceContract(Namespace = "http://www.iringtools.org/service")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class ApplicationConfigurationService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserSecurityService));
        private ApplicationConfigurationProvider _applicationConfigurationProvider = null;
        private CustomError _CustomError = null;

        public ApplicationConfigurationService()
        {
            _applicationConfigurationProvider = new ApplicationConfigurationProvider(ConfigurationManager.AppSettings);
        }


        [Description("Gets the scopes available from the data base.")]
        [WebGet(UriTemplate = "/contexts?siteId={siteId}&format={format}")]
        public void GetContexts(int siteId, string format)  // Completed.
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }


                org.iringtools.applicationConfig.Contexts contexts = _applicationConfigurationProvider.GetAllContexts(siteId);
                _applicationConfigurationProvider.FormatOutgoingMessage<org.iringtools.applicationConfig.Contexts>(contexts, format, true);
            }

            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetScope, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert contexts to the data base.")]
        [WebInvoke(Method = "POST", UriTemplate = "/insertContext/{userName}?format={format}")]
        public void InsertContext(string userName, string format, Stream stream) 
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<org.iringtools.applicationConfig.Contexts>(stream, format);
                    response = _applicationConfigurationProvider.InsertContext(userName,new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Update contexts to the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/updateContext/{userName}?format={format}")]
        public void UpdateContext(string userName,string format, Stream stream) // Completed.
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<org.iringtools.applicationConfig.Contexts>(stream, format);
                    response = _applicationConfigurationProvider.UpdateContext(userName,new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Update contexts to the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/deleteContext/{contextId}?format={format}")]
        public void DeleteContext(string contextId, string format) // Completed.
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
                    response = _applicationConfigurationProvider.DeleteContext(contextId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Gets all applications available from the data base.")]
        [WebGet(UriTemplate = "/apps/{scopeInternalName}?format={format}")]
        public void GetApplications(string scopeInternalName ,string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Applications applications = _applicationConfigurationProvider.GetAllApplications(scopeInternalName);
                _applicationConfigurationProvider.FormatOutgoingMessage<Applications>(applications, format, true);
            }

            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetScope, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert applications to the data base.")]
        [WebInvoke(Method = "POST", UriTemplate = "/apps/{scopeInternalName}?format={format}")]
        public void InsertApplications(string scopeInternalName, string format, Stream stream) 
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<Applications>(stream, format);
                    response = _applicationConfigurationProvider.InsertApplications(scopeInternalName, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Update applications to the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/apps/{scopeInternalName}?format={format}")]
        public void UpdateApplications(string scopeInternalName, string format, Stream stream) // Completed.
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<Applications>(stream, format);
                    response = _applicationConfigurationProvider.UpdateApplications(scopeInternalName, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Update contexts to the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/apps/{scopeInternalName}/{appInternalName}?format={format}")]
        public void DeleteApplication(string scopeInternalName, string appInternalName, string format) // Completed.
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
                    response = _applicationConfigurationProvider.DeleteApplication(scopeInternalName, appInternalName);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Gets all graphs available from the data base.")]
        [WebGet(UriTemplate = "/graphs?format={format}")]
        public void GetGraphs(string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Graphs graphs = _applicationConfigurationProvider.GetAllGraphs();
                _applicationConfigurationProvider.FormatOutgoingMessage<Graphs>(graphs, format, true);
            }

            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetScope, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert graphs to the data base.")]
        [WebInvoke(Method = "POST", UriTemplate = "/graphs?format={format}")]
        public void InsertGraphs(string format, Stream stream)
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<Graphs>(stream, format, true);
                    response = _applicationConfigurationProvider.InsertGraph(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Update graphs to the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/graphs?format={format}")]
        public void UpdateGraphs(string format, Stream stream) // Completed.
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<Graphs>(stream, format, true);
                    response = _applicationConfigurationProvider.UpdateGraphs(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Delete Graphs from the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/graphs/{graphId}?format={format}")]
        public void DeleteGraph(string graphId, string format) // Completed.
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
                    response = _applicationConfigurationProvider.DeleteGraph(graphId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, false);
        }

        [Description("Insert application for user")]
        [WebInvoke(Method = "POST", UriTemplate = "/app/{user}?format={format}")]
        public void AddApplicationForUser(string user, string format, Stream stream)
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<Applications>(stream, format);
                    response = _applicationConfigurationProvider.InsertApplicationForUser(user, format, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Update application for user")]
        [WebInvoke(Method = "PUT", UriTemplate = "/app/{user}?format={format}")]
        public void UpdateApplicationForUser(string user, string format, Stream stream)
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<Application>(stream, format);
                    response = _applicationConfigurationProvider.UpdateApplicationForUser(user, format, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("delete application for user")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/app/{user}?format={format}")]
        public void DeleteApplicationForUser(string user, string format)
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Get context collection for user")]
        [WebGet(UriTemplate = "/contexts/{userName}?siteId={siteId}&folderId={folderId}&format={format}")]
        public void GetContextsForUser(string userName, int siteId, Guid folderId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                org.iringtools.applicationConfig.Contexts contexts = _applicationConfigurationProvider.GetContextsForUser(userName, siteId, folderId);
                _applicationConfigurationProvider.FormatOutgoingMessage<org.iringtools.applicationConfig.Contexts>(contexts, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Get application collection for user")]
        [WebGet(UriTemplate = "/applications/{userName}?siteId={siteId}&contextId={contextId}&format={format}")]
        public void GetApplicationsForUser(string userName, int siteId, Guid contextId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Applications applications = _applicationConfigurationProvider.GetApplicationsForUser(userName, siteId, contextId);
                _applicationConfigurationProvider.FormatOutgoingMessage<Applications>(applications, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Get graph collection for user")]
        [WebGet(UriTemplate = "/graphs/{userName}?siteId={siteId}&applicationId={applicationId}&format={format}")]
        public void GetGraphsForUser(string userName, int siteId, Guid applicationId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Graphs graphs = _applicationConfigurationProvider.GetGraphsForUser(userName, siteId, applicationId);
                _applicationConfigurationProvider.FormatOutgoingMessage<Graphs>(graphs, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Get graph collection for user")]
        [WebGet(UriTemplate = "/graphMapping/{userName}?siteId={siteId}&graphId={graphId}&format={format}")]
        public void GetGraphMappingForUser(string userName, int siteId, Guid graphId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Graphs graphs = _applicationConfigurationProvider.GetGraphMappingForUser(userName, siteId, graphId);
                _applicationConfigurationProvider.FormatOutgoingMessage<Graphs>(graphs, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Get application collection for user")]
        [WebGet(UriTemplate = "/folders/{userName}?siteId={siteId}&parentFolderId={parentFolderId}&format={format}")]
        public void GetFoldersForUser(string userName, int siteId, Guid parentFolderId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                Folders folders = _applicationConfigurationProvider.GetFoldersForUser(userName, siteId, parentFolderId);
                _applicationConfigurationProvider.FormatOutgoingMessage<Folders>(folders, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert folder to the data base.")]
        [WebInvoke(Method = "POST", UriTemplate = "/insertFolder/{userName}?format={format}")]
        public void InsertFolder(string userName, string format, Stream stream)
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<Folder>(stream, format);
                    response = _applicationConfigurationProvider.InsertFolder(userName, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("update Folder to the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/updateFolder/{userName}?format={format}")]
        public void UpdateFolder(string userName, string format, Stream stream)
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<Folder>(stream, format);
                    response = _applicationConfigurationProvider.UpdateFolder(userName, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("delete Folder to the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/deleteFolder/{folderId}?format={format}")]
        public void DeleteFolder(string folderId, string format)
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
                    response = _applicationConfigurationProvider.DeleteFolder(folderId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Get datafilter collection for user")]
        [WebGet(UriTemplate = "/datafilters/{userName}?siteId={siteId}&resourceId={resourceId}&format={format}")]
        public void GetDataFiltersForUser(string userName, int siteId, Guid resourceId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                org.iringtools.applicationConfig.DataFilters dataFilters = _applicationConfigurationProvider.GetDataFiltersForUser(userName, siteId, resourceId);
                _applicationConfigurationProvider.FormatOutgoingMessage<org.iringtools.applicationConfig.DataFilters>(dataFilters, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert DataFilter to the data base.")]
        [WebInvoke(Method = "POST", UriTemplate = "/insertDataFilter?resourceId={resourceId}&siteId={siteId}&dataFilterTypeId={dataFilterTypeId}&format={format}")]
        public void InsertDataFilter(string resourceId, string siteId, string dataFilterTypeId, string format, Stream stream)
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<org.iringtools.applicationConfig.DataFilters>(stream, format);
                    response = _applicationConfigurationProvider.InsertDataFilter(resourceId, siteId,dataFilterTypeId, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Update DataFilter to the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/deleteDataFilter?resourceId={resourceId}&format={format}")]
        public void DeleteDataFilter(string resourceId, string format)
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
                    //XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<org.iringtools.applicationConfig.DataFilter>(stream, format);
                    response = _applicationConfigurationProvider.DeleteDataFilter(resourceId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Update DataFilter to the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/udpateDataFilter?resourceId={resourceId}&siteId={siteId}&dataFilterTypeId={dataFilterTypeId}&format={format}")]
        public void UpdateDataFilter(string resourceId, string siteId, string dataFilterTypeId, string format, Stream stream)
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<org.iringtools.applicationConfig.DataFilters>(stream, format);
                    response = _applicationConfigurationProvider.UpdateDataFilter(resourceId, siteId, dataFilterTypeId, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Get exchange collection for user")]
        [WebGet(UriTemplate = "/exchanges/{userName}?siteId={siteId}&commodityId={commodityId}&format={format}")]
        public void GetExchangesForUser(string userName, int siteId, Guid commodityId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                org.iringtools.applicationConfig.Exchanges exchanges = _applicationConfigurationProvider.GetExchangesForUser(userName, siteId, commodityId);
                _applicationConfigurationProvider.FormatOutgoingMessage<org.iringtools.applicationConfig.Exchanges>(exchanges, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert Exchange to the data base.")]
        [WebInvoke(Method = "POST", UriTemplate = "/insertExchange/{userName}?groupIds={groupIds}&format={format}")]
        public void InsertExchange(string userName, string groupIds, string format, Stream stream)
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<org.iringtools.applicationConfig.Exchange>(stream, format);
                    response = _applicationConfigurationProvider.InsertExchange(userName, groupIds, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Insert Exchange to the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/updateExchange/{userName}?groupIds={groupIds}&format={format}")]
        public void UpdateExchange(string userName, string groupIds, string format, Stream stream)
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<org.iringtools.applicationConfig.Exchange>(stream, format);
                    response = _applicationConfigurationProvider.UpdateExchange(userName, groupIds, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("delete Exchange to the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/deleteExchange/{exchangeId}?format={format}")]
        public void DeleteExchange(string exchangeId, string format)
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
                    response = _applicationConfigurationProvider.DeleteExchange(exchangeId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Get Commodities collection for user")]
        [WebGet(UriTemplate = "/commodities/{userName}?siteId={siteId}&contextId={contextId}&format={format}")]
        public void GetCommoditiesForUser(string userName, int siteId, Guid contextId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                org.iringtools.applicationConfig.Commodities exchanges = _applicationConfigurationProvider.GetCommoditiesForUser(userName, siteId, contextId);
                _applicationConfigurationProvider.FormatOutgoingMessage<org.iringtools.applicationConfig.Commodities>(exchanges, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert Commodity to the data base.")]
        [WebInvoke(Method = "POST", UriTemplate = "/insertCommodity/{userName}?format={format}")]
        public void InsertCommodity(string userName, string format, Stream stream)
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<Commodity>(stream, format);
                    response = _applicationConfigurationProvider.InsertCommodity(userName, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("update Commodity to the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/updateCommodity/{userName}?format={format}")]
        public void UpdateCommodity(string userName, string format, Stream stream)
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
                    XElement xElement = _applicationConfigurationProvider.FormatIncomingMessage<Commodity>(stream, format);
                    response = _applicationConfigurationProvider.UpdateCommodity(userName, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("delete Commodity to the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/deleteCommodity/{commodityId}?format={format}")]
        public void DeleteCommodity(string commodityId, string format)
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
                    response = _applicationConfigurationProvider.DeleteCommodity(commodityId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _applicationConfigurationProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Get valueList collection for user")]
        [WebGet(UriTemplate = "/valueList/{userName}?siteId={siteId}&applicationId={applicationId}&format={format}")]
        public void GetValueListForUser(string userName, int siteId, Guid applicationId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                ValueListMaps valueListMaps = _applicationConfigurationProvider.GetValueListForUser(userName, siteId, applicationId);
                _applicationConfigurationProvider.FormatOutgoingMessage<ValueListMaps>(valueListMaps, format, true);
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