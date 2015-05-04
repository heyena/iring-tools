using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Xml.Linq;
using log4net;
using org.iringtools.library;
using org.iringtools.dxfr.manifest;
using org.iringtools.adapter;
using System.Xml;
using System.ServiceModel.Channels;
using System.IO;
using System.Text;
using System;
using org.iringtools.utility;
using org.iringtools.mapping;
using System.Web;
using System.Net;
using System.Runtime.Serialization;


namespace org.iringtools.services
{
    [ServiceContract(Namespace = "http://ns.iringtools.org/protocol")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class AdapterService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterService));
        private AdapterProvider _adapterProvider = null;
        private CustomError _CustomError = null;
        private string errMsg = string.Empty;

        public AdapterService()
        {
            _adapterProvider = new AdapterProvider(ConfigurationManager.AppSettings);
        }

        [Description("Gets version of the service.")]
        [WebGet(UriTemplate = "/version")]
        public VersionInfo GetVersion()
        {
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            context.ContentType = "application/xml";
            return _adapterProvider.GetVersion();
        }

        [Description("Gets the scopes (project and application combinations) available from the service.")]
        [WebGet(UriTemplate = "/scopes")]
        public ScopeProjects GetScopes()
        {
            try
            {
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";
                return _adapterProvider.GetScopes();
            }

            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetScope, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
                return null;
            }
        }

        [Description("Gets a specific scope information.")]
        [WebGet(UriTemplate = "/scopes/{scopeName}")]
        public ScopeProject GetScope(string scopeName)
        {
            try
            {
                
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";
                return _adapterProvider.GetScope(scopeName);
            }
            catch (Exception ex)
            {

                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetScopeName, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
                return null;
            }

        }

        [Description("Gets cache information for an application.")]
        [WebGet(UriTemplate = "/{scope}/{app}/cacheinfo")]
        public CacheInfo GetCacheInfo(string scope, string app)
        {
            try
            {
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";
                return _adapterProvider.GetCacheInfo(scope, app);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetCacheInfo, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
                return null;
            }

        }

        [Description("Gets a specific application information.")]
        [WebGet(UriTemplate = "/scopes/{scopeName}/apps/{appName}")]
        public ScopeApplication GetApplication(string scopeName, string appName)
        {
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            context.ContentType = "application/xml";

            return _adapterProvider.GetApplication(scopeName, appName);
        }

        [Description("Gets datalayer resource data.")]
        [WebInvoke(Method = "GET", UriTemplate = "/{scope}/{app}/resourcedata")]
        public DocumentBytes GetResourceData(string scope, string app)
        {
            return _adapterProvider.GetResourceData(scope, app);
        }

        [Description("Gets datalayer resource data.")]
        [WebInvoke(Method = "GET", UriTemplate = "/{scope}/{app}/resourcebytes")]
        public Stream GetResourceDataBytes(string scope, string app)
        {
            byte[] data = _adapterProvider.GetResourceDataBytes(scope, app);
            if (data != null)
            {
                MemoryStream stream = new MemoryStream(data);
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/x-msaccess"; //or whatever your mime type is
                stream.Position = 0;
                return stream;
            }
            else
            {
                return new MemoryStream();
            }
        }

        [Description("Generic Upload Service.")]
        [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/upload", RequestFormat = WebMessageFormat.Json)]
        public Response Upload(String scope, String app, Stream filecontent)
        {
            try
            {
                return _adapterProvider.UploadFile(filecontent, scope + "." + app + "." + Convert.ToString(HttpContext.Current.Request.Headers.GetValues("FileName")[0]));
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex);
            }
        }

        [Description("Generic Download Service.")]
        [WebInvoke(Method = "GET", UriTemplate = "/{scope}/{app}/File/{fileName}/{extension}")]
        public DocumentBytes DownloadFile(string scope, string app, string fileName, string extension)
        {
            return _adapterProvider.DownLoadFile(scope, app, fileName, extension);
        }

        [Description("Download List.")]
        [WebInvoke(Method = "GET", UriTemplate = "/{scope}/{app}/Downloadlist")]
        public List<Files> getDownloadedList(string scope, string app)
        {
            return _adapterProvider.GetDownloadedList(scope, app);
        }

        [Description("Creates a new scope.")]
        [WebInvoke(Method = "POST", UriTemplate = "/scopes")]
        public Response AddScope(ScopeProject scope)
        {
            try
            {
              
                return _adapterProvider.AddScope(scope);
            }
            catch (Exception ex)
            {
                // return PrepareErrorResponse(ex);
                return PrepareErrorResponse(ex, ErrorMessages.errAddScope);
            }
        }

        [Description("Updates an existing scope.")]
        [WebInvoke(Method = "POST", UriTemplate = "/scopes/{scope}")]
        public Response UpdateScope(string scope, ScopeProject updatedScope)
        {
            try
            {
               
                return _adapterProvider.UpdateScope(scope, updatedScope);
            }
            catch (Exception ex)
            {
                //return PrepareErrorResponse(ex);
                return PrepareErrorResponse(ex, ErrorMessages.errUpdateScope);
            }
        }

        [Description("Deletes a scope.")]
        [WebInvoke(Method = "GET", UriTemplate = "/scopes/{scope}/delete")]
        public Response DeleteScope(string scope)
        {
            try
            {
                return _adapterProvider.DeleteScope(scope);
            }
            catch (Exception ex)
            {
               // return PrepareErrorResponse(ex);
                return PrepareErrorResponse(ex, ErrorMessages.errDeleteScope);
            }
        }

        [Description("Creates a new application in a specific scope.")]
        [WebInvoke(Method = "POST", UriTemplate = "/scopes/{scope}/apps")]
        public Response AddApplication(string scope, ScopeApplication application)
        {
            try
            {
               
                return _adapterProvider.AddApplication(scope, application);
            }
            catch (Exception ex)
            {
                //return PrepareErrorResponse(ex);
                return PrepareErrorResponse(ex, ErrorMessages.errAddNewApplication);
            }
        }

        [Description("Updates an existing application in a specific scope.")]
        [WebInvoke(Method = "POST", UriTemplate = "/scopes/{scope}/apps/{app}")]
        public Response UpdateApplication(string scope, string app, ScopeApplication updatedApplication)
        {
            try
            {
               
                return _adapterProvider.UpdateApplication(scope, app, updatedApplication);
            }
            catch (Exception ex)
            {
               // return PrepareErrorResponse(ex);
                return PrepareErrorResponse(ex, ErrorMessages.errUpdateApplication);
            }
        }

        [Description("Deletes an application in a specific scope.")]
        [WebInvoke(Method = "GET", UriTemplate = "/scopes/{scope}/apps/{app}/delete")]
        public Response DeleteApplication(string scope, string app)
        {
            try
            {
              
                return _adapterProvider.DeleteApplication(scope, app);
            }
            catch (Exception ex)
            {
               // return PrepareErrorResponse(ex);
                return PrepareErrorResponse(ex, ErrorMessages.errDeleteApp);
            }
        }

        [Description("Gets app settings of an application.")]
        [WebGet(UriTemplate = "/{scope}/{app}/config")]
        public XElement GetConfig(string scope, string app)
        {
            return _adapterProvider.GetConfig(scope, app);
        }

        [Description("Gets the Ninject binding configuration for the specified scope.")]
        [WebGet(UriTemplate = "/{scope}/{app}/binding")]
        public XElement GetBinding(string scope, string app)
        {
            return _adapterProvider.GetBinding(scope, app);
        }

        [Description("Replaces the Ninject binding configuration for the specified scope and returns a response with status.")]
        [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/binding")]
        public Response UpdateBinding(string scope, string app, XElement binding)
        {
            try
            {
                return _adapterProvider.UpdateBinding(scope, app, binding);
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex);
            }
        }

        [Description("Generate artifacts for all applications in all projects.")]
        [WebInvoke(Method = "GET", UriTemplate = "/generate")]
        public Response GenerateAll()
        {
            try
            {
                return _adapterProvider.Generate();
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex);
            }
        }

        [Description("Generate artifacts for all applications in a specific project.")]
        [WebInvoke(Method = "GET", UriTemplate = "/{scope}/generate")]
        public Response GenerateScope(string scope)
        {
            try
            {
                return _adapterProvider.Generate(scope);
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex);
            }
        }

        [Description("Generate artifacts for a specific application in a project.")]
        [WebInvoke(Method = "GET", UriTemplate = "/{scope}/{app}/generate")]
        public Response Generate(string scope, string app)
        {
            try
            {
                return _adapterProvider.Generate(scope, app);
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex);
            }
        }

        [Description("Gets the dictionary of data objects for the specified scope.")]
        [WebGet(UriTemplate = "/{scope}/{app}/dictionary")]
        public DataDictionary GetDictionary(string scope, string app)
        {
            try
            {
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";

                DataDictionary dictionary = _adapterProvider.GetDictionary(scope, app);
                return dictionary;
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetDictionary, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
                return null;
            }

        }

        [Description("Gets the iRING mapping for the specified scope.")]
        [WebGet(UriTemplate = "/{scope}/{app}/mapping")]
        public Mapping GetMapping(string scope, string app)
        {
            try
            {
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";

                return _adapterProvider.GetMapping(scope, app);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetMapping, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
                return null;
            }

        }

        [Description("Replaces the iRING mapping for the specified scope and retuns a response with status.")]
        [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/mapping")]
        public Response UpdateMapping(string scope, string app, XElement mappingXml)
        {
            try
            {
                return _adapterProvider.UpdateMapping(scope, app, mappingXml);
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex);
            }
        }

        [Description("Get a list of Data Layers available from the service.")]
        [WebGet(UriTemplate = "/datalayers")]
        public void GetDatalayers()
        {
            try
            {
                DataLayers dataLayers = _adapterProvider.GetDataLayers();
                string xml = Utility.Serialize<DataLayers>(dataLayers, true);

                HttpContext.Current.Response.ContentType = "application/xml";
                HttpContext.Current.Response.Write(xml);
            }
            catch (Exception e)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetDataLayer, e, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        #region TODO - DO NOT DELETE
        //[Description("Adds/updates a dataLayer to/in adapter.")]
        //[WebInvoke(Method = "POST", UriTemplate = "/datalayers")]
        //public void PostDataLayer(Stream dataLayerStream)
        //{
        //  try
        //  {
        //    DataContractSerializer serializer = new DataContractSerializer(typeof(DataLayer));
        //    DataLayer dataLayer = (DataLayer)serializer.ReadObject(dataLayerStream);

        //    Response response = _adapterProvider.PostDataLayer(dataLayer);
        //    string xml = Utility.Serialize<Response>(response, true);

        //    HttpContext.Current.Response.ContentType = "application/xml";
        //    HttpContext.Current.Response.Write(xml);
        //  }
        //  catch (Exception e)
        //  {
        //    OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
        //    context.StatusCode = HttpStatusCode.InternalServerError;

        //    HttpContext.Current.Response.ContentType = "text/html";
        //    HttpContext.Current.Response.Write(e);
        //  }
        //}

        //[Description("Deletes a data layer from adapter.")]
        //[WebInvoke(Method = "DELETE", UriTemplate = "/datalayers/{name}")]
        //public void DeleteDatalayer(string name)
        //{
        //  try
        //  {
        //    Response response = _adapterProvider.DeleteDataLayer(name);
        //    string xml = Utility.Serialize<Response>(response, true);

        //    HttpContext.Current.Response.ContentType = "application/xml";
        //    HttpContext.Current.Response.Write(xml);
        //  }
        //  catch (Exception e)
        //  {
        //    OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
        //    context.StatusCode = HttpStatusCode.InternalServerError;

        //    HttpContext.Current.Response.ContentType = "text/html";
        //    HttpContext.Current.Response.Write(e);
        //  }
        //}
        #endregion

        [Description("Resets all data objects state in data layer.")]
        [WebGet(UriTemplate = "/{scope}/{app}/refresh")]
        public void RefreshAll(string scope, string app)
        {
            try
            {
                if (IsAsync())
                {
                    string statusUrl = _adapterProvider.AsyncRefreshDictionary(scope, app);
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
                    WebOperationContext.Current.OutgoingResponse.Headers["location"] = statusUrl;
                }
                else
                {
                    Response response = _adapterProvider.RefreshAll(scope, app);
                    _adapterProvider.FormatOutgoingMessage<Response>(response, "xml", true);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errRefreshAll, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Resets a data object state in data layer.")]
        [WebGet(UriTemplate = "/{scope}/{app}/{objectType}/refresh")]
        public Response RefreshDataObject(string scope, string app, string objectType)
        {
            try
            {
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";

                return _adapterProvider.RefreshDataObject(scope, app, objectType);
            }
            catch (Exception ex)
            {
                return PrepareErrorResponse(ex);
            }
        }

        [Description("Gets status of a asynchronous request.")]
        [WebGet(UriTemplate = "/requests/{id}")]
        public void GetRequestStatus(string id)
        {
            RequestStatus status = null;

            try
            {
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                status = _adapterProvider.GetRequestStatus(id);

                if (status.State == State.NotFound)
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                status = new RequestStatus()
                {
                    State = State.Error,
                    Message = ex.Message
                };
            }

            _adapterProvider.FormatOutgoingMessage<RequestStatus>(status, "xml", true);
        }

        [Description("Switches data mode. Creates cache if it does not exist.")]
        [WebGet(UriTemplate = "/{scope}/{app}/data/{mode}")]
        public void SwitchDataMode(string scope, string app, string mode)
        {
            Response response = _adapterProvider.SwitchDataMode(scope, app, mode);
            FormatOutgoingMessage<Response>(response, true);
        }

        //[Description("Creates/refreshes application cache.")]
        //[WebGet(UriTemplate = "/{scope}/{app}/cache/refresh?updatedictionary={updateDictionary}")]
        //public void RefreshCache(string scope, string app, bool updateDictionary)
        //{
        //    Response response = _adapterProvider.RefreshCache(scope, app, updateDictionary);
        //    FormatOutgoingMessage<Response>(response, true);
        //}

        [Description("Creates/refreshes application cache.")]
        [WebGet(UriTemplate = "/RefreshCacheForDataObject?dataObjectId={dataObjectId}&updatedictionary={updateDictionary}")]
        public void RefreshCache(Guid dataObjectId, bool updateDictionary)
        {
            Response response = _adapterProvider.RefreshCache(dataObjectId, updateDictionary);
            FormatOutgoingMessage<Response>(response, true);
        }

        [Description("Creates/refreshes application cache for specific object type.")]
        [WebGet(UriTemplate = "/{scope}/{app}/{objectType}/cache/refresh?updatedictionary={updateDictionary}")]
        public void RefreshObjectTypeCache(string scope, string app, string objectType, bool updateDictionary)
        {
            Response response = _adapterProvider.RefreshCache(scope, app, objectType, updateDictionary);
            FormatOutgoingMessage<Response>(response, true);
        }

        [Description("Imports application cache. Cache files are baseUri followed by <object type>.dat is required.")]
        [WebGet(UriTemplate = "/{scope}/{app}/cache/import?baseuri={baseUri}&updatedictionary={updateDictionary}")]
        public void ImportCache(string scope, string app, string baseUri, bool updateDictionary)
        {
            Response response = _adapterProvider.ImportCache(scope, app, baseUri, updateDictionary);
            FormatOutgoingMessage<Response>(response, true);
        }

        [Description("Imports application cache for specific object type. Cache file URL is required.")]
        [WebGet(UriTemplate = "/{scope}/{app}/{objectType}/cache/import?url={url}&updatedictionary={updateDictionary}")]
        public void ImportObjectTypeCache(string scope, string app, string objectType, string url, bool updateDictionary)
        {
            Response response = _adapterProvider.ImportCache(scope, app, objectType, url, updateDictionary);
            FormatOutgoingMessage<Response>(response, true);
        }

        [Description("Deletes application cache.")]
        [WebGet(UriTemplate = "/{scope}/{app}/cache/delete")]
        public void DeleteCache(string scope, string app)
        {
            Response response = _adapterProvider.DeleteCache(scope, app);
            FormatOutgoingMessage<Response>(response, true);
        }

        [Description("Deletes application cache for specific object type.")]
        [WebGet(UriTemplate = "/{scope}/{app}/{objectType}/cache/delete")]
        public void DeleteObjectTypeCache(string scope, string app, string objectType)
        {
            Response response = _adapterProvider.DeleteCache(scope, app, objectType);
            FormatOutgoingMessage<Response>(response, true);
        }

        [Description("Get all groups for the specified user from LDAP.")]
        [WebGet(UriTemplate = "/{username}/groups")]
        public PermissionGroups GetLDAPUserGroups(string username)
        {
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            context.ContentType = "application/xml";

            return _adapterProvider.GetUserGroups(username);
        }

        [Description("Get all the security groups from LDAP.")]
        [WebGet(UriTemplate = "/groups")]
        public PermissionGroups GetAllLDAPSecurityGroups()
        {
            try
            {
               
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";
                return _adapterProvider.GetAllSecurityGroups();
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errSecurityGroups, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
                return null;
            }

        }

        [Description("Gets the authorized scopes (project and application combinations) available from the service.")]
        [WebGet(UriTemplate = "/{username}/authorizedscopes")]
        public ScopeProjects GetAuthorizedScopes(string username)
        {
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            context.ContentType = "application/xml";

            return _adapterProvider.GetAuthorizedScope(username);
        }

        [Description("Gets the settings for UI available from this service.")]
        [WebGet(UriTemplate = "/settings")]
        public NameValueList GetUISettings()
        {
            try
            {
               
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";

                return _adapterProvider.GetGlobalUISettings();
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
                return null;

            }
        }

        private void FormatOutgoingMessage<T>(T graph, bool useDataContractSerializer)
        {
            string reqContentType = WebOperationContext.Current.IncomingRequest.ContentType;

            if (reqContentType != null && reqContentType.ToLower() == "application/json")
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

        private Response PrepareErrorResponse(Exception ex)
        {
            Response response = new Response
            {
                Level = StatusLevel.Error,
                Messages = new Messages
          {
            ex.Message
          }
            };


            return response;
        }

        private bool IsAsync()
        {
            bool async = false;
            string asyncHeader = WebOperationContext.Current.IncomingRequest.Headers["async"];

            if (asyncHeader != null && asyncHeader.ToLower() == "true")
            {
                async = true;
            }

            return async;
        }


        //Overloading method added by Atul

        private Response PrepareErrorResponse(Exception ex, string errMsg)
        {
            CustomErrorLog objCustomErrorLog = new CustomErrorLog();
            _CustomError = objCustomErrorLog.customErrorLogger(errMsg, ex, _logger);
            Response response = new Response
            {
                Level = StatusLevel.Error,
                Messages = new Messages
          {
            //ex.Message
            "[ Message Id " + _CustomError.msgId + "] - " + errMsg  
          },
                StatusText = "[ " + _CustomError.msgId + "] " + _CustomError.stackTraceDescription,
                StatusCode = HttpStatusCode.InternalServerError,
                StatusList = null
            };
            return response;
        }

    }
}