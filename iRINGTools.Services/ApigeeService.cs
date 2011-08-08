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
  [ServiceContract(Namespace = "http://www.iringtools.org/service")]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class ApigeeService
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ApigeeService));
    private AdapterProvider _adapterProvider = null;

    public ApigeeService()
    {
      _adapterProvider = new AdapterProvider(ConfigurationManager.AppSettings);
    }

    #region DataService URIs
    [Description("Gets version of the service.")]
    [WebGet(UriTemplate = "/version")]
    public VersionInfo GetVersion()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.GetVersion();
    }

    [Description("Gets object definitions of an application.")]
    [WebGet(UriTemplate = "/{project}/{app}/dictionary")]
    public DataDictionary GetDictionary(string project, string app)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.GetDictionary(project, app);
    }

    //[Description("Gets an XML projection of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.")]
    //[WebGet(UriTemplate = "/{project}/{app}/{graph}/{id}?format={format}")]
    public void Get(string project, string app, string graph, string id, string format)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        XDocument xDocument = _adapterProvider.GetDataProjection(project, app, graph, String.Empty, id, format, false);
        FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(context, ex);
      }
    }

    [Description("Gets an XML projection of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{project}/{app}/{graph}/{clazz}/{id}?format={format}")]
    public void GetIndividual(string project, string app, string graph, string clazz, string id, string format)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        XDocument xDocument = _adapterProvider.GetDataProjection(project, app, graph, clazz, id, format, false);
        FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(context, ex);
      }
    }

    [Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{project}/{app}/{graph}/filter?format={format}&start={start}&limit={limit}&indexStyle={indexStyle}")]
    public void GetListWithFilter(string project, string app, string graph, DataFilter filter, string format, int start, int limit, string indexStyle)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        XDocument xDocument = _adapterProvider.GetDataProjection(project, app, graph, filter, format, start, limit, fullIndex);
        FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(context, ex);
      }
    }

    //[Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    //[WebGet(UriTemplate = "/{project}/{app}/{graph}?format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}")]
    public void GetList(string project, string app, string graph, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        XDocument xDocument = _adapterProvider.GetDataProjection(project, app, graph, format, start, limit, sortOrder, sortBy, fullIndex, parameters);
        FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(context, ex);
      }
    }

    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{project}/{app}/{graph}?format={format}")]
    public Response PostList(string project, string app, string graph, string format, XElement xElement)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.Post(project, app, graph, format, new XDocument(xElement));
    }

    [Description("Deletes a graph in the specified application.")]
    [WebInvoke(Method = "DELETE", UriTemplate = "/{project}/{app}/{graph}/{id}")]
    public Response Delete(string project, string app, string graph, string id)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.DeleteIndividual(project, app, graph, id);
    }

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
    #endregion DataService URIs

    #region ApigeeService URIs
    [Description("Gets WADL of the service.")]
    [WebGet(UriTemplate = "/wadl")]
    public XElement ApigeeGetWADL()
    {
      WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
      string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

      return Utility.ReadXml(baseDirectory + @"XML\iring-adapter-wadl.xml");
    }

    [Description("Gets object definitions of an application.")]
    [WebGet(UriTemplate = "/{app}/dictionary?project={project}")]
    public DataDictionary ApigeeGetDictionary(string project, string app)
    {
      return GetDictionary(project, app);
    }

    [Description("Gets an XML projection of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{app}/{graph}/{id}?project={project}&format={format}")]
    public void ApigeeGet(string project, string app, string graph, string id, string format)
    {
      Get(project, app, graph, id, format);
    }

    [Description("Gets an XML projection of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{app}/{graph}/{clazz}/{id}?project={project}&format={format}")]
    public void ApigeeGetIndividual(string project, string app, string graph, string clazz, string id, string format)
    {
      GetIndividual(project, app, graph, clazz, id, format);
    }

    [Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{app}/{graph}/filter?project={project}&format={format}&start={start}&limit={limit}&indexStyle={indexStyle}")]
    public void ApigeeGetListWithFilter(string project, string app, string graph, DataFilter filter, string format, int start, int limit, string indexStyle)
    {
      GetListWithFilter(project, app, graph, filter, format, start, limit, indexStyle);
    }

    [Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{app}/{graph}?project={project}&format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}")]
    public void ApigeeGetList(string project, string app, string graph, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      GetList(project, app, graph, format, start, limit, sortOrder, sortBy, indexStyle);
    }

    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{app}/{graph}?project={project}&format={format}")]
    public Response ApigeePostList(string project, string app, string graph, string format, XElement xElement)
    {
      return ApigeePostList(project, app, graph, format, xElement);
    }

    [Description("Deletes a graph in the specified application.")]
    [WebInvoke(Method = "DELETE", UriTemplate = "/{app}/{graph}/{id}?project={project}")]
    public Response ApigeeDelete(string project, string app, string graph, string id)
    {
      return Delete(project, app, graph, id);
    }
    #endregion ApigeeService URIs
  }
}