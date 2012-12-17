// Copyright (c) 2010, iringtools.org //////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

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

    /// <summary>
    /// Adapter Service Constructor
    /// </summary>
    public AdapterService()
    {
      _adapterProvider = new AdapterProvider(ConfigurationManager.AppSettings);
    }

    #region Public Resources
    #region GetVersion
    /// <summary>
    /// Gets the version of the service.
    /// </summary>
    /// <returns>Returns the version as a string.</returns>
    [Description("Gets version of the service.")]
    [WebGet(UriTemplate = "/version")]
    public VersionInfo GetVersion()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _adapterProvider.GetVersion();
    }
    #endregion

    #region GetScopes
    /// <summary>
    /// Gets the scopes (project and application combinations) available from the service.
    /// </summary>
    /// <returns>Returns a list of ScopeProject objects.</returns>
    [Description("Gets the scopes (project and application combinations) available from the service.")]
    [WebGet(UriTemplate = "/scopes")]
    public ScopeProjects GetScopes()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.GetScopes();
    }
    #endregion

    #region GetResourceData
    ///// <summary>
    ///// Gets the resource data for a datalayer.
    ///// </summary>
    ///// <returns>Returns a list of service settings.</returns>
    //[Description("Gets datalayer resource data.")]
    //[WebInvoke(Method = "GET", UriTemplate = "/{scope}/{app}/resourcedata")]
    //public DocumentBytes GetResourceData(string scope, string app)
    //{
    //  return _adapterProvider.GetResourceData(scope, app);
    //}
    #endregion

    #region Config methods
    [Description("Configure the selected data layer in the service.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/configure")]
    public Response Configure(String scope, String app)
    {
      try
      {
        return _adapterProvider.Configure(scope, app, HttpContext.Current.Request);
      }
      catch (Exception ex)
      {
        return PrepareErrorResponse(ex);
      }
    }

    [Description("Get configuration for a selected data layer in the service.")]
    [WebInvoke(Method = "GET", UriTemplate = "/{scope}/{app}/configuration")]
    public XElement GetConfiguration(String scope, String app)
    {
      return _adapterProvider.GetConfiguration(scope, app);
    }
    #endregion Config methods
    #endregion

    #region Private Resources
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
        return PrepareErrorResponse(ex);
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
        return PrepareErrorResponse(ex);
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
        return PrepareErrorResponse(ex);
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
        return PrepareErrorResponse(ex);
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
        return PrepareErrorResponse(ex);
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
        return PrepareErrorResponse(ex);
      }
    }

    #region GetBinding
    /// <summary>
    /// Gets the Ninject binding configuration for the specified scope.
    /// </summary>
    /// <param name="scope">Project name</param>
    /// <param name="app">Application name</param>
    /// <returns>Returns an arbitrary XML.</returns>
    [Description("Gets the Ninject binding configuration for the specified scope.")]
    [WebGet(UriTemplate = "/{scope}/{app}/binding")]
    public XElement GetBinding(string scope, string app)
    {
      return _adapterProvider.GetBinding(scope, app);
    }
    #endregion

    #region UpdateBinding
    /// <summary>
    /// Replaces the Ninject binding configuration for the specified scope.
    /// </summary>
    /// <param name="scope">Project name</param>
    /// <param name="app">Application name</param>
    /// <param name="binding">An arbitrary XML</param>
    /// <returns>Returns a Response object.</returns>
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
    #endregion

    #region Generate methods
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
    #endregion Generate methods

    #region GetDictionary
    /// <summary>
    /// Gets the dictionary of data objects for the specified scope.
    /// </summary>
    /// <param name="scope">Project name</param>
    /// <param name="app">Application name</param>
    /// <returns>Returns a DataDictionary object.</returns>
    [Description("Gets the dictionary of data objects for the specified scope.")]
    [WebGet(UriTemplate = "/{scope}/{app}/dictionary")]
    public DataDictionary GetDictionary(string scope, string app)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      DataDictionary dictionary = _adapterProvider.GetDictionary(scope, app);
      return dictionary;
    }
    #endregion

    #region GetMapping
    /// <summary>
    /// Gets the iRING mapping for the specified scope.
    /// </summary>
    /// <param name="scope">Project name</param>
    /// <param name="app">Application name</param>
    /// <returns>Returns a Mapping object.</returns>
    [Description("Gets the iRING mapping for the specified scope.")]
    [WebGet(UriTemplate = "/{scope}/{app}/mapping")]
    public Mapping GetMapping(string scope, string app)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.GetMapping(scope, app);
    }
    #endregion

    #region UpdateMapping
    /// <summary>
    /// Replaces the iRING mapping for the specified scope.
    /// </summary>
    /// <param name="mapping">An arbitrary XML</param>
    /// <returns>Returns a Response object.</returns>
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
    #endregion

    #region GetDataLayers
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
        OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
        context.StatusCode = HttpStatusCode.InternalServerError;

        HttpContext.Current.Response.ContentType = "text/html";
        HttpContext.Current.Response.Write(e);
      }
    }
    #endregion

    //TODO: pending on testing, do not delete
    #region data layers managment
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

    #region RefreshDataObjects
    [Description("Resets all data objects state in data layer.")]
    [WebGet(UriTemplate = "/{scope}/{app}/refresh")]
    public void RefreshDataObjects(string scope, string app)
    {
      try
      {
        bool isAsync = false;
        string asyncHeader = WebOperationContext.Current.IncomingRequest.Headers["async"];

        if (asyncHeader != null && asyncHeader.ToLower() == "true")
        {
          isAsync = true;
        }

        if (isAsync)
        {
          string statusUrl = _adapterProvider.AsyncRefreshDictionary(scope, app);
          WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
          WebOperationContext.Current.OutgoingResponse.Headers["location"] = statusUrl;
        }
        else
        {
          Response response = _adapterProvider.RefreshDataObjects(scope, app);
          _adapterProvider.FormatOutgoingMessage<Response>(response, "xml", true);
        }
      }
      catch (Exception ex)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(ex.Message);
      }
    }
    #endregion

    #region RefreshDataObject
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
    #endregion

    #region RefreshDataObject with filter
    [Description("Resets a data object state in data layer.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{objectType}/refresh")]
    public Response RefreshDataObjectWithFilter(string scope, string app, string objectType, DataFilter dataFilter)
    {
      try
      {
        OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
        context.ContentType = "application/xml";

        return _adapterProvider.RefreshDataObject(scope, app, objectType, dataFilter);
      }
      catch (Exception ex)
      {
        return PrepareErrorResponse(ex);
      }
    }

    //[WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/binding")]
    #endregion

    #region Async request queue
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
    #endregion

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
    #endregion
  }
}