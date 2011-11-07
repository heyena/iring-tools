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
using System.ServiceModel.Channels;
using System.Runtime.Serialization.Json;
using System.Text;

namespace org.iringtools.services
{
  [ServiceContract(Namespace = "http://www.iringtools.org/service")]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class DataService
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DataService));
    private AdapterProvider _adapterProvider = null;

    public DataService()
    {
      _adapterProvider = new AdapterProvider(ConfigurationManager.AppSettings);
    }

    [Description("Gets version of the service.")]
    [WebGet(UriTemplate = "/version?format={format}")]
    public VersionInfo GetVersion(string format)
    {
      MapResponseType(format);

      return _adapterProvider.GetVersion();
    }

    [Description("Gets object definitions of an application.")]
    [WebGet(UriTemplate = "/{app}/{project}/dictionary?format={format}")]
    public DataDictionary GetDictionary(string project, string app, string format)
    {
      MapResponseType(format);

      return _adapterProvider.GetDictionary(project, app);
    }

    [Description("Gets an XML or JSON projection of the specified project, application and graph in the format specified. Valid formats include json, xml, p7xml, and rdf.")]
    [WebGet(UriTemplate = "/{app}/{project}/{graph}?format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}")]
    public void GetList(string project, string app, string graph, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      try
      {
        NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        XDocument xDocument = _adapterProvider.GetDataProjection(project, app, graph, ref format, start, limit, sortOrder, sortBy, fullIndex, parameters);
        FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(ex);
      }
    }

    [Description("Gets an XML or JSON projection of a single item in the specified project, application and graph in the format specified. Valid formats include json, xml, p7xml, and rdf.")]
    [WebGet(UriTemplate = "/{app}/{project}/{graph}/{id}?format={format}")]
    public void GetItem(string project, string app, string graph, string id, string format)
    {
      try
      {
        object content = _adapterProvider.GetDataProjection(project, app, graph, String.Empty, id, ref format, false);
        FormatOutgoingMessage(content, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(ex);
      }
    }

    [Description("Gets an XML or JSON projection of a filtered set in the specified project, application and graph in the format specified. Valid formats include json, xml, p7xml, and rdf.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{app}/{project}/{graph}/filter?format={format}&start={start}&limit={limit}&indexStyle={indexStyle}")]
    public void GetWithFilter(string project, string app, string graph, string format, int start, int limit, string indexStyle, Stream stream)
    {
      try
      {
        format = MapContentType(format);

        DataFilter filter = FormatIncomingMessage<DataFilter>(stream, format, true);

        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        XDocument xDocument = _adapterProvider.GetDataProjection(project, app, graph, filter, ref format, start, limit, fullIndex);
        FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(ex);
      }
    }

    [Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{app}/{project}/{graph}/search?q={query}&format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}")]
    public void GetSearch(string project, string app, string graph, string query, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      try
      {
        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        XDocument xDocument = _adapterProvider.GetDataProjection(project, app, graph, query, ref format, start, limit, sortOrder, sortBy, fullIndex);
        FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(ex);
      }
    }

    [Description("Gets an XML projection of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{app}/{project}/{graph}/{clazz}/{id}?format={format}")]
    public void GetIndividual(string project, string app, string graph, string clazz, string id, string format)
    {
      try
      {
        object content = _adapterProvider.GetDataProjection(project, app, graph, clazz, id, ref format, false);
        FormatOutgoingMessage(content, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(ex);
      }
    }

    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "PUT", UriTemplate = "/{app}/{project}/{graph}?format={format}")]
    public Response UpdateList(string project, string app, string graph, string format, Stream stream)
    {
      format = MapContentType(format);

      if (format == "raw")
      {
        throw new Exception("");
      }
      else
      {
        XElement xElement = FormatIncomingMessage(stream, format);

        return _adapterProvider.Post(project, app, graph, format, new XDocument(xElement));
      }
    }

    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "PUT", UriTemplate = "/{app}/{project}/{graph}/{id}?format={format}")]
    public Response UpdateItem(string project, string app, string graph, string id, string format, Stream stream)
    {
      format = MapContentType(format);

      if (format == "raw")
      {
        return _adapterProvider.PostContent(project, app, graph, format, id, stream);
      }
      else
      {
        XElement xElement = FormatIncomingMessage(stream, format);

        return _adapterProvider.Post(project, app, graph, format, new XDocument(xElement));
      }
    }

    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{app}/{project}/{graph}?format={format}")]
    public Response CreateItem(string project, string app, string graph, string format, Stream stream)
    {
      format = MapContentType(format);

      if (format == "raw")
      {
        throw new Exception("");
      }
      else
      {
        XElement xElement = FormatIncomingMessage(stream, format);

        return _adapterProvider.Post(project, app, graph, format, new XDocument(xElement));
      }
    }

    [Description("Deletes a graph in the specified application.")]
    [WebInvoke(Method = "DELETE", UriTemplate = "/{app}/{project}/{graph}/{id}?format={format}")]
    public Response DeleteItem(string project, string app, string graph, string id, string format)
    {
      MapContentType(format);

      return _adapterProvider.DeleteIndividual(project, app, graph, id);
    }

    #region "All" Methods
    [Description("Gets object definitions of an application.")]
    [WebGet(UriTemplate = "/all/{app}/dictionary?format={format}")]
    public DataDictionary GetDictionaryAll(string app, string format)
    {
      return GetDictionary("all", app, format);
    }

    [Description("Gets an XML or JSON projection of the specified application and graph in the format specified.")]
    [WebGet(UriTemplate = "/all/{app}/{graph}?format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}")]
    public void GetListAll(string app, string graph, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      GetList("all", app, graph, format, start, limit, sortOrder, sortBy, indexStyle);
    }

    [Description("Gets an XML or JSON projection of a single item in the specified application and graph in the format specified.")]
    [WebGet(UriTemplate = "/all/{app}/{graph}/{id}?format={format}")]
    public void GetItemAll(string app, string graph, string id, string format)
    {
      GetItem("all", app, graph, id, format);
    }

    [Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebInvoke(Method = "POST", UriTemplate = "/all/{app}/{graph}/filter?format={format}&start={start}&limit={limit}&indexStyle={indexStyle}")]
    public void GetWithFilterAll(string app, string graph, string format, int start, int limit, string indexStyle, Stream stream)
    {
      GetWithFilter("all", app, graph, format, start, limit, indexStyle, stream);
    }

    [Description("Gets an XML projection of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/all/{app}/{graph}/{clazz}/{id}?format={format}")]
    public void GetIndividualAll(string app, string graph, string clazz, string id, string format)
    {
      GetIndividual("all", app, graph, clazz, id, format);
    }

    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "PUT", UriTemplate = "/all/{app}/{graph}?format={format}")]
    public Response UpdateListAll(string app, string graph, string format, Stream stream)
    {
      return UpdateList("all", app, graph, format, stream);
    }

    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "PUT", UriTemplate = "/all/{app}/{graph}/{id}?format={format}")]
    public Response UpdateItemAll(string app, string graph, string id, string format, Stream stream)
    {
      return UpdateItem("all", app, graph, id, format, stream);
    }

    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/all/{app}/{graph}?format={format}")]
    public Response CreateItemAll(string app, string graph, string format, Stream stream)
    {
      return CreateItem("all", app, graph, format, stream);
    }

    [Description("Deletes a graph in the specified application.")]
    [WebInvoke(Method = "DELETE", UriTemplate = "/all/{app}/{graph}/{id}?format={format}")]
    public Response DeleteItemAll(string app, string graph, string id, string format)
    {
      return DeleteItem("all", app, graph, id, format);
    }
    #endregion

    #region Private Methods
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
        MemoryStream ms = Utility.SerializeToStreamJSON<DataItems>(dataItems, false);
        byte[] json = ms.ToArray();
        ms.Close();

        HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
        HttpContext.Current.Response.Write(Encoding.UTF8.GetString(json, 0, json.Length));
      }
      else
      {
        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(xElement.ToString());
      }
    }

    private void MapResponseType(string format)
    {
      OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
      if (format != null && format.ToLower() == "xml")
      {
        response.ContentType = "application/xml";
      }
      else
      {
        response.ContentType = "application/json; charset=utf-8";
      }
    }

    private string MapContentType(string format)
    {
      // if it's a known xml format then return it
      if (format != null && (format.ToLower().Contains("xml") || format.ToLower().Contains("json") ||
        format.ToLower().Contains("dto") || format.ToLower().Contains("rdf") || format.ToLower().Contains("p7xml")))
      {
        return format;
      }

      // make decision on what format to apply based on request content type
      string postedFormat = "raw";

      IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
      OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;

      string contentType = request.ContentType;
      response.ContentType = contentType;

      if (contentType != null)
      {
        if (contentType.ToLower().Contains("application/xml"))
        {
          postedFormat = "xml";
        }
        else if (contentType.ToLower().Contains("application/json"))
        {
          postedFormat = "json";
        }
      }

      if (format != postedFormat)
      {
        throw new Exception("Requested format [" + format + "] is invalid.");
      }

      return postedFormat;
    }

    private XElement FormatIncomingMessage(Stream stream, string format)
    {
      XElement xElement = null;

      if (format != null && (format.ToLower().Contains("xml") || format.ToLower().Contains("rdf") || 
        format.ToLower().Contains("dto") || format.ToLower().Contains("p7xml")))
      {
        xElement = XElement.Load(stream);
      }
      else
      {
        DataItems dataItems = Utility.DeserializeFromStreamJson<DataItems>(stream, false);
        xElement = Utility.SerializeToXElement<DataItems>(dataItems);
      }

      return xElement;
    }

    private T FormatIncomingMessage<T>(Stream stream, string format, bool useDataContractSerializer)
    {
      T graph = default(T);

      if (format != null && format.ToLower().Contains("xml"))
      {
        graph = Utility.DeserializeFromStream<T>(stream, useDataContractSerializer);
      }
      else
      {
        graph = Utility.DeserializeFromStreamJson<T>(stream, useDataContractSerializer);
      }

      return graph;
    }

    private void FormatOutgoingMessage(object content, string format)
    {
      if (typeof(IContentObject).IsInstanceOfType(content))
      {
        IContentObject contentObject = (IContentObject)content;
        HttpContext.Current.Response.ContentType = contentObject.contentType;
        HttpContext.Current.Response.BinaryWrite(contentObject.content.ToMemoryStream().GetBuffer());
      }
      else if (content.GetType() == typeof(XDocument))
      {
        XDocument xDoc = (XDocument)content;
        FormatOutgoingMessage(xDoc.Root, format);
      }
      else
      {
        throw new Exception("Invalid response type from DataLayer.");
      }
    }

    private void ExceptionHandler(Exception ex)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

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