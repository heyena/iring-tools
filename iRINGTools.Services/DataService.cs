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
using System.Web.Script.Serialization;
using System.Web;

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
    [Description("Gets version of the service.")]
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
    /// <param name="scope">scope name</param>
    /// <param name="app">Application name</param>
    /// <returns>Returns a DataDictionary object.</returns>
    [Description("Gets object definitions of an application.")]
    [WebGet(UriTemplate = "/{scope}/{app}/dictionary")]
    public DataDictionary GetDictionary(string scope, string app)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.GetDictionary(scope, app);
    }
    #endregion
    #region Get
    /// <summary>
    /// Gets an XML scopeion of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.
    /// </summary>
    /// <param name="scope">scope name</param>
    /// <param name="app">Application name</param>
    /// <param name="graph">Graph name</param>
    /// <param name="id">id</param>
    /// <param name="format">Format to be returned (xml, dto, rdf ...)</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("Gets an XML scopeion of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}/{id}?format={format}")]
    public void Get(string scope, string app, string graph, string id, string format)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        XDocument xDocument = _adapterProvider.GetDataProjection(scope, app, graph, String.Empty, id, format, false);
        FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(context, ex);
      }
    }

    /// <summary>
    /// Gets an XML scopeion of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.
    /// </summary>
    /// <param name="scope">scope name</param>
    /// <param name="app">Application name</param>
    /// <param name="graph">Graph name</param>
    /// <param name="graph">Class name</param>
    /// <param name="id">id</param>
    /// <param name="format">Format to be returned (xml, dto, rdf ...)</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("Gets an XML scopeion of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}/{clazz}/{id}?format={format}")]
    public void GetIndividual(string scope, string app, string graph, string clazz, string id, string format)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        XDocument xDocument = _adapterProvider.GetDataProjection(scope, app, graph, clazz, id, format, false);
        FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(context, ex);
      }
    }
    #endregion

    #region GetListWithFilter
    /// <summary>
    /// Gets an XML scopeion of the specified scope and graph.
    /// </summary>
    /// <param name="scope">scope name</param>
    /// <param name="app">Application name</param>
    /// <param name="graph">Graph name</param>
    /// <param name="format">Format to be returned (xml, dto, rdf ...)</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("Gets an XML scopeion of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/filter?format={format}&start={start}&limit={limit}&indexStyle={indexStyle}")]
    public void GetListWithFilter(string scope, string app, string graph, DataFilter filter, string format, int start, int limit, string indexStyle)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        XDocument xDocument = _adapterProvider.GetDataProjection(scope, app, graph, filter, format, start, limit, fullIndex);
        FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(context, ex);
      }
    }
    #endregion
    #region GetList
    /// <summary>
    /// Gets an XML scopeion of the specified scope and graph in the format (xml, dto, rdf ...) specified.
    /// </summary>
    /// <param name="scope">scope name</param>
    /// <param name="app">Application name</param>
    /// <param name="graph">Graph name</param>
    /// <param name="format">Format to be returned (xml, dto, rdf ...)</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("Gets an XML scopeion of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}?format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}")]
    public void GetList(string scope, string app, string graph, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;
        
        XDocument xDocument = _adapterProvider.GetDataProjection(scope, app, graph, format, start, limit, sortOrder, sortBy, fullIndex, parameters);
        FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(context, ex);
      }
    }
    #endregion

    #region PostList
    /// <summary>
    /// Updates the specified scope and graph with an XML scopeion in the format (xml, dto, rdf ...) specified.
    /// </summary>
    /// <param name="scope">scope name</param>
    /// <param name="app">Application name</param>
    /// <param name="graph">Graph name</param>
    /// <param name="format">Format to be returned (xml, dto, rdf ...)</param>
    /// <param name="xml">Arbitrary XML</param>
    /// <returns>Returns a Response object</returns>
    [Description("Updates the specified scope and graph with an XML scopeion in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}?format={format}")]
    public Response PostList(string scope, string app, string graph, string format, XElement xElement)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.Post(scope, app, graph, format, new XDocument(xElement));
    }
    #endregion

    #region Delete
    /// <summary>
    /// Deletes the specified individual based on scope, graph and id.
    /// </summary>
    /// <param name="scope">scope name</param>
    /// <param name="app">Application name</param>
    /// <param name="graph">Graph name</param>
    /// <param name="id">id</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("Deletes a graph in the specified application.")]
    [WebInvoke(Method = "DELETE", UriTemplate = "/{scope}/{app}/{graph}/{id}")]
    public Response Delete(string scope, string app, string graph, string id)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.DeleteIndividual(scope, app, graph, id);
    }
    #endregion
    #endregion
    
    #region Private methods
    private void FormatOutgoingMessage(XElement xElement, string format)
    {
      if (format == null)
      {
        format = String.Empty;
      }

      if (format.ToUpper() == "HTML")
      {
        HttpContext.Current.Response.ContentType = "text/html";
        HttpContext.Current.Response.Write(xElement.ToString());
      }
      else if (format.ToUpper() == "JSON")
      {
        DataItems dataItems = Utility.DeserializeDataContract<DataItems>(xElement.ToString());
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        string json = serializer.Serialize(dataItems);

        HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
        HttpContext.Current.Response.Write(json);
      }
      else
      {
        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(xElement.ToString());
      }
    }
    
    private void ExceptionHandler(OutgoingWebResponseContext context, Exception ex)
    {
      if (ex is FileNotFoundException)
      {
        context.StatusCode = HttpStatusCode.NotFound;
      }
      else if (ex is UnauthorizedAccessException)
      {
        context.StatusCode = HttpStatusCode.Unauthorized;
      }
      else
      {
        context.StatusCode = HttpStatusCode.InternalServerError;
      }

      HttpContext.Current.Response.ContentType = "text/html";
      HttpContext.Current.Response.Write(ex.ToString());
    }
    #endregion
  }
}