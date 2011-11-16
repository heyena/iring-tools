﻿// Copyright (c) 2010, iringtools.org //////////////////////////////////////////
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
    [Description("Gets the version of the service.")]
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
    #region UpdateScopes
    /// <summary>
    /// Replaces the available scopes.
    /// </summary>
    /// <param name="scopes">The scopes object.</param>
    /// <returns>Returns a Response object.</returns>
    [Description("Replaces the available scopes with the posted scopes.")]
    [WebInvoke(Method = "POST", UriTemplate = "/scopes")]
    public Response UpdateScopes(ScopeProjects scopes)
    {
      try
      {
        return _adapterProvider.UpdateScopes(scopes);
      }
      catch (Exception ex)
      {
        return PrepareErrorResponse(ex);
      }
    }
    #endregion

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

      return _adapterProvider.GetDictionary(scope, app);
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
    public DataLayers GetDatalayers()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.GetDataLayers();
    }
    #endregion

    #region RefreshDataObjects
    [Description("Resets all data objects state in data layer.")]
    [WebGet(UriTemplate = "/{scope}/{app}/refresh")]
    public Response RefreshDataObjects(string scope, string app)
    {
      try
      {
        OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
        context.ContentType = "application/xml";

        return _adapterProvider.RefreshDataObjects(scope, app);
      }
      catch (Exception ex)
      {
        return PrepareErrorResponse(ex);
      }
    }
    #endregion

    #region RefreshDataObject
    [Description("Resets a data object state in data layer.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{dataObject}/refresh")]
    public Response RefreshDataObject(string scope, string app, string dataObject)
    {
      try
      {
        OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
        context.ContentType = "application/xml";

        return _adapterProvider.RefreshDataObject(scope, app, dataObject);
      }
      catch (Exception ex)
      {
        return PrepareErrorResponse(ex);
      }
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