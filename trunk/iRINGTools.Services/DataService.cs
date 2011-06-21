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

using System.ComponentModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Xml.Linq;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter;
using System.Collections.Specialized;
using System;
using System.IO;
using System.Net;

namespace org.iringtools.services
{
  [ServiceContract(Namespace = "http://ns.iringtools.org/protocol")]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class DataService
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterService));
    private AdapterProvider _adapterProvider = null;

    /// <summary>
    /// Adapter Service Constructor
    /// </summary>
    public DataService()
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

    #region GetDictionary
    /// <summary>
    /// Gets the dictionary of data objects for the specified scope.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <returns>Returns a DataDictionary object.</returns>
    [Description("Gets the dictionary of data objects for the specified scope.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/dictionary")]
    public DataDictionary GetDictionary(string projectName, string applicationName)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.GetDictionary(projectName, applicationName);
    }
    #endregion
    #region Get
    /// <summary>
    /// Gets an XML projection of the specified scope, graph and identifier in the format (xml, dto, rdf ...) specified.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="graphName">Graph name</param>
    /// <param name="identifier">Identifier</param>
    /// <param name="format">Format to be returned (xml, dto, rdf ...)</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("Gets an XML projection of the specified scope, graph and identifier in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}/{identifier}?format={format}")]
    public XElement Get(string projectName, string applicationName, string graphName, string identifier, string format)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        XDocument xDocument = _adapterProvider.GetDataProjection(projectName, applicationName, graphName, String.Empty, identifier, format, false);

        context.ContentType = "application/xml";

        return xDocument.Root;
      }
      catch (Exception ex)
      {
        HandleException(context, ex);
        return null;
      }
    }

    /// <summary>
    /// Gets an XML projection of the specified scope, graph and identifier in the format (xml, dto, rdf ...) specified.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="graphName">Graph name</param>
    /// <param name="graphName">Class name</param>
    /// <param name="identifier">Identifier</param>
    /// <param name="format">Format to be returned (xml, dto, rdf ...)</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("Gets an XML projection of the specified scope, graph and identifier in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}/{className}/{identifier}?format={format}")]
    public XElement GetIndividual(string projectName, string applicationName, string graphName, string className, string identifier, string format)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        XDocument xDocument = _adapterProvider.GetDataProjection(projectName, applicationName, graphName, className, identifier, format, false);

        context.ContentType = "application/xml";

        return xDocument.Root;
      }
      catch (Exception ex)
      {
        HandleException(context, ex);
        return null;
      }
    }
    #endregion

    #region GetListWithFilter
    /// <summary>
    /// Gets an XML projection of the specified scope and graph.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="graphName">Graph name</param>
    /// <param name="format">Format to be returned (xml, dto, rdf ...)</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}/filter?format={format}&start={start}&limit={limit}&indexStyle={indexStyle}")]
    public XElement GetListWithFilter(string projectName, string applicationName, string graphName, DataFilter filter, string format, int start, int limit, string indexStyle)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        XDocument xDocument = _adapterProvider.GetDataProjection(projectName, applicationName, graphName, filter, format, start, limit, fullIndex);

        context.ContentType = "application/xml";

        return xDocument.Root;
      }
      catch (Exception ex)
      {
        HandleException(context, ex);
        return null;
      }
    }
    #endregion
    #region GetList
    /// <summary>
    /// Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="graphName">Graph name</param>
    /// <param name="format">Format to be returned (xml, dto, rdf ...)</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}?format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}")]
    public XElement GetList(string projectName, string applicationName, string graphName, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;
        XDocument xDocument = _adapterProvider.GetDataProjection(projectName, applicationName, graphName, format, start, limit, sortOrder, sortBy, fullIndex, parameters);

        context.ContentType = "application/xml";

        return xDocument.Root;
      }
      catch (Exception ex)
      {
        HandleException(context, ex);
        return null;
      }
    }
    #endregion

    #region PostList
    /// <summary>
    /// Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="graphName">Graph name</param>
    /// <param name="format">Format to be returned (xml, dto, rdf ...)</param>
    /// <param name="xml">Arbitrary XML</param>
    /// <returns>Returns a Response object</returns>
    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}?format={format}")]
    public Response PostList(string projectName, string applicationName, string graphName, string format, XElement xElement)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.Post(projectName, applicationName, graphName, format, new XDocument(xElement));
    }
    #endregion

    #region Delete
    /// <summary>
    /// Deletes the specified individual based on scope, graph and identifier.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="graphName">Graph name</param>
    /// <param name="identifier">Identifier</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("Deletes the specified individual based on scope, graph and identifier.")]
    [WebInvoke(Method = "DELETE", UriTemplate = "/{projectName}/{applicationName}/{graphName}/{identifier}")]
    public Response Delete(string projectName, string applicationName, string graphName, string identifier)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.DeleteIndividual(projectName, applicationName, graphName, identifier);
    }
    #endregion
    #endregion

    private void HandleException(OutgoingWebResponseContext context, Exception ex)
    {
      if (ex is FileNotFoundException)
      {
        context.ContentType = "text/xml";
        context.StatusCode = HttpStatusCode.NotFound;
      }
      else if (ex is UnauthorizedAccessException)
      {
        context.ContentType = "text/xml";
        context.StatusCode = HttpStatusCode.Unauthorized;
      }
      else
      {
        context.ContentType = "text/xml";
        context.StatusCode = HttpStatusCode.InternalServerError;
      }
    }
  }
}