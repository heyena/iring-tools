﻿using System;
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
        [WebInvoke(Method = "POST", UriTemplate = "/contexts?format={format}")]
        public void InsertContexts(string format, Stream stream) // Completed.
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
                    response = _applicationConfigurationProvider.InsertContext(new XDocument(xElement));
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
        [WebInvoke(Method = "PUT", UriTemplate = "/contexts?format={format}")]
        public void UpdateContexts(string format, Stream stream) // Completed.
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
                    response = _applicationConfigurationProvider.UpdateContext(new XDocument(xElement));
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
        [WebInvoke(Method = "DELETE", UriTemplate = "/contexts/{internalName}?siteId={siteId}&format={format}")]
        public void DeleteContexts(string internalName, int siteId, string format) // Completed.
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
                    response = _applicationConfigurationProvider.DeleteContext(internalName, siteId);
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