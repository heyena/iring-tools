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
  public class DataService2
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DataService2));
    private AdapterProvider _adapterProvider = null;

    public DataService2()
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

    [Description("Gets object definitions of an application.")]
    [WebGet(UriTemplate = "/all/{app}/dictionary")]
    public DataDictionary GetDictionaryAll(string app)
    {
      return GetDictionary("all", app);
    }

    [Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/all/{app}/{graph}?format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}")]
    public void GetListAll(string app, string graph, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      GetList("all", app, graph, format, start, limit, sortOrder, sortBy, indexStyle);
    }

    [Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebInvoke(Method = "POST", UriTemplate = "/all/{app}/{graph}/filter?format={format}&start={start}&limit={limit}&indexStyle={indexStyle}")]
    public void GetListWithFilterAll(string app, string graph, DataFilter filter, string format, int start, int limit, string indexStyle)
    {
      GetListWithFilter("all", app, graph, filter, format, start, limit, indexStyle);
    }

    [Description("Gets an XML projection of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/all/{app}/{graph}/{id}?format={format}")]
    public void GetItemAll(string app, string graph, string id, string format)
    {
      GetItem("all", app, graph, id, format);
    }

    [WebGet(UriTemplate = "/all/{app}/{graph}/{id}.{format}")]
    public void GetItemContentAll(string app, string graph, string id, string format)
    {
      GetItem("all", app, graph, id, format);
    }

    [WebGet(UriTemplate = "/all/{app}/{graph}/{clazz}/{id}.{format}")]
    public void GetIndividualContentAll(string app, string graph, string clazz, string id, string format)
    {
      GetIndividual("all", app, graph, clazz, id, format);
    }

    [Description("Gets an XML projection of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/all/{app}/{graph}/{clazz}/{id}?format={format}")]
    public void GetIndividualAll(string app, string graph, string clazz, string id, string format)
    {
      GetIndividual("all", app, graph, clazz, id, format);
    }

    //[Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    //[WebInvoke(Method = "POST", UriTemplate = "/all/{app}/{graph}?format={format}")]
    //public void PostListXmlAll(string app, string graph, string format, XElement xElement)
    //{
    //  PostListXml("all", app, graph, xElement);
    //}

    [Description("Deletes a graph in the specified application.")]
    [WebInvoke(Method = "DELETE", UriTemplate = "/all/{app}/{graph}/{id}")]
    public Response DeleteItemAll(string app, string graph, string id)
    {
      return DeleteItem("all", app, graph, id);
    }

    [Description("Gets object definitions of an application.")]
    [WebGet(UriTemplate = "/{app}/{project}/dictionary")]
    public DataDictionary GetDictionary(string project, string app)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.GetDictionary(project, app);
    }

    [WebGet(UriTemplate = "/{app}/{project}/{graph}/{id}.{format}")]
    public void GetItemContent(string project, string app, string graph, string id, string format)
    {
      GetItem(project, app, graph, id, format);
    }

    [WebGet(UriTemplate = "/{app}/{project}/{graph}/{clazz}/{id}.{format}")]
    public void GetIndividualContent(string project, string app, string graph, string clazz, string id, string format)
    {
      GetIndividual(project, app, graph, clazz, id, format);
    }

    [Description("Gets an XML projection of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{app}/{project}/{graph}/{id}?format={format}")]
    public void GetItem(string project, string app, string graph, string id, string format)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        object content = _adapterProvider.GetDataProjection(project, app, graph, String.Empty, id, ref format, false);
        FormatOutgoingMessage(content, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(context, ex);
      }
    }

    [Description("Gets an XML projection of the specified scope, graph and id in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{app}/{project}/{graph}/{clazz}/{id}?format={format}")]
    public void GetIndividual(string project, string app, string graph, string clazz, string id, string format)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        object content = _adapterProvider.GetDataProjection(project, app, graph, clazz, id, ref format, false);
        FormatOutgoingMessage(content, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(context, ex);
      }
    }

    [Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{app}/{project}/{graph}/filter?format={format}&start={start}&limit={limit}&indexStyle={indexStyle}")]
    public void GetListWithFilter(string project, string app, string graph, DataFilter filter, string format, int start, int limit, string indexStyle)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        XDocument xDocument = _adapterProvider.GetDataProjection(project, app, graph, filter, ref format, start, limit, fullIndex);
        FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(context, ex);
      }
    }

    [Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{app}/{project}/{graph}?format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}")]
    public void GetList(string project, string app, string graph, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

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
        ExceptionHandler(context, ex);
      }
    }

    [Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{app}/{project}/{graph}/search?q={query}&format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}")]
    public void GetSearch(string project, string app, string graph, string query, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

      try
      {
        //NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        XDocument xDocument = _adapterProvider.GetDataProjection(project, app, graph, query, ref format, start, limit, sortOrder, sortBy, fullIndex);
        FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        ExceptionHandler(context, ex);
      }
    }

    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{app}/{project}/{graph}?format=xml", RequestFormat=WebMessageFormat.Xml)]
    public Response PostListXml(string project, string app, string graph, XElement xElement)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.Post(project, app, graph, "xml", new XDocument(xElement));
    }

    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{app}/{project}/{graph}?format=p7xml", RequestFormat = WebMessageFormat.Xml)]
    public Response PostListP7Xml(string project, string app, string graph, XElement xElement)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.Post(project, app, graph, "p7xml", new XDocument(xElement));
    }

    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{app}/{project}/{graph}?format=rdf", RequestFormat = WebMessageFormat.Xml)]
    public Response PostListRdf(string project, string app, string graph, XElement xElement)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _adapterProvider.Post(project, app, graph, "rdf", new XDocument(xElement));
    }
    
    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "PUT", UriTemplate = "/{app}/{project}/{graph}", RequestFormat=WebMessageFormat.Json)]
    public Response PutItems(string project, string app, string graph, DataItems dataItems)
    {
      HttpResponse context = HttpContext.Current.Response;
      context.ContentType = "application/json; charset=utf-8";

      XElement xElement = dataItems.ToXElement();

      return _adapterProvider.Post(project, app, graph, "json", new XDocument(xElement));
    }

    [WebInvoke(Method = "PUT", UriTemplate = "/{app}/{project}/{graph}?format=json", RequestFormat=WebMessageFormat.Json)]
    public Response PutJsonItems(string project, string app, string graph, DataItems dataItems)
    {
      HttpResponse context = HttpContext.Current.Response;
      context.ContentType = "application/json; charset=utf-8";

      XElement xElement = dataItems.ToXElement();

      return _adapterProvider.Post(project, app, graph, "json", new XDocument(xElement));
    }

    [Description("Updates the specified scope and graph with an JSON projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "PUT", UriTemplate = "/{app}/{project}/{graph}?format=content")]
    public Response PutStreamItems(string project, string app, string graph, Stream dataItemsStream)
    {
      DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DataItems));
      DataItems dataItems = (DataItems)serializer.ReadObject(dataItemsStream);

      HttpResponse context = HttpContext.Current.Response;
      context.ContentType = "application/json; charset=utf-8";

      XElement xElement = dataItems.ToXElement();
      return _adapterProvider.Post(project, app, graph, "json", new XDocument(xElement));
    }

    [Description("Updates the specified scope and graph with an JSON projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{app}/{project}/{graph}/{id}")]
    public Response PostContent(string project, string app, string graph, string id, Stream content)
    {
      HttpResponse context = HttpContext.Current.Response;
      context.ContentType = "application/json; charset=utf-8";

      return _adapterProvider.PostContent(project, app, graph, "json", id, content);
    }

    [Description("Deletes a graph in the specified application.")]
    [WebInvoke(Method = "DELETE", UriTemplate = "/{app}/{project}/{graph}/{id}")]
    public Response DeleteItem(string project, string app, string graph, string id)
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
        String json = serializer.Serialize(dataItems);
        HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
        HttpContext.Current.Response.Write(json);
      }
      else
      {
        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(xElement.ToString());
      }
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

    [Description("Gets WADL of the service.")]
    [WebGet(UriTemplate = "/wadl")]
    public XElement ApigeeGetWADL()
    {
      WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
      string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

      return Utility.ReadXml(baseDirectory + @"XML\iring-adapter-wadl.xml");
    }
  }
}