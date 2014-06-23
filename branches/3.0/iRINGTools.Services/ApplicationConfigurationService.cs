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
        [WebGet(UriTemplate = "/contexts?format={format}")]
        public void GetContexts(string format)  // Completed.
        {
            try
            {
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";

                org.iringtools.applicationConfig.Contexts contexts = _applicationConfigurationProvider.GetAllContexts();
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
        [WebInvoke(Method = "DELETE", UriTemplate = "/contexts/{internalName}?format={format}")]
        public void DeleteContexts(string internalName, string format) // Completed.
        {
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
                    response = _applicationConfigurationProvider.DeleteContext(internalName);
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
        [WebGet(UriTemplate = "/apps?format={format}")]
        public void GetApplications(string format)
        {
            try
            {
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";

                Applications applications = _applicationConfigurationProvider.GetAllApplications();
                _applicationConfigurationProvider.FormatOutgoingMessage<Applications>(applications, format, true);
            }

            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetScope, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Gets all graphs available from the data base.")]
        [WebGet(UriTemplate = "/graphs?format={format}")]
        public void GetGraphs(string format)
        {
            try
            {
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";

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


        [Description("Get application collection for user")]
        [WebGet(UriTemplate = "/apps/{user}?format={format}")]
        public void GetApplicationForUser(string user, string format)
        {
            try
            {
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";

                Applications applications = _applicationConfigurationProvider.GetApplications(user);
                _applicationConfigurationProvider.FormatOutgoingMessage<Applications>(applications, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert application for user")]
        [WebInvoke(Method = "POST", UriTemplate = "/app/{user}?format={format}")]
        public void AddApplicationForUser(string user, string format, Stream stream)
        {
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
                    response = _applicationConfigurationProvider.InsertApplication(user, format, new XDocument(xElement));
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
                    response = _applicationConfigurationProvider.UpdateApplication(user, format, new XDocument(xElement));
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
        public void DeleteApplicationForUser(string user)
        {
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