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

//These are the CommonServices for AdapterDataServices and DataServices.
//Common functionality for both are added here.

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
using System.Collections.Generic;
using net.java.dev.wadl;

namespace org.iringtools.services
{
  public class CommonDataService
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(CommonDataService));
    private AdapterProvider _adapterProvider = null;

    public CommonDataService()
    {
      _adapterProvider = new AdapterProvider(ConfigurationManager.AppSettings);
    }

    public void GetVersion(string format)
    {
      format = MapContentType(format);

      VersionInfo version = _adapterProvider.GetVersion();

      _adapterProvider.FormatOutgoingMessage<VersionInfo>(version, format, true);
    }

    public void GetContexts(string app, string format)
    {
      format = MapContentType(format);

      Contexts contexts = _adapterProvider.GetContexts(app);

      _adapterProvider.FormatOutgoingMessage<Contexts>(contexts, format, true);
    }

    public void GetAllWADL(string app)
    {
      WADLApplication wadl = _adapterProvider.GetWADL("all", app);

      _adapterProvider.FormatOutgoingMessage<WADLApplication>(wadl, "xml", false);
    }

    public void GetAppWADL(string app)
    {
      WADLApplication wadl = _adapterProvider.GetWADL("app", app);

      _adapterProvider.FormatOutgoingMessage<WADLApplication>(wadl, "xml", false);
    }

    public void GetScopeWADL(string app, string project)
    {
      WADLApplication wadl = _adapterProvider.GetWADL(project, app);

      _adapterProvider.FormatOutgoingMessage<WADLApplication>(wadl, "xml", false);
    }

    public void GetDictionary(string project, string app, string format)
    {
      try
      {
        format = MapContentType(format);

        if (IsAsync())
        {
          string statusUrl = _adapterProvider.AsyncGetDictionary(project, app);
          WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
          WebOperationContext.Current.OutgoingResponse.Headers["location"] = statusUrl;
        }
        else
        {
          DataDictionary dictionary = _adapterProvider.GetDictionary(project, app);
          _adapterProvider.FormatOutgoingMessage<DataDictionary>(dictionary, format, true);
        }
      }
      catch (Exception ex)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(ex.Message);
      }
    }

    public void GetDictionaryGraph(string project, string app, string resource, string format)
    {
      format = MapContentType(format);

      DataDictionary dictionary = _adapterProvider.GetDictionary(project, app);

      DataObject dataObject = dictionary.dataObjects.Find(o => o.objectName.ToLower() == resource.ToLower());

      if (dataObject == null)
        throw new FileNotFoundException();
      //ExceptionHandler(new FileNotFoundException());

      _adapterProvider.FormatOutgoingMessage<DataObject>(dataObject, format, true);
    }

    public void GetList(string project, string app, string resource, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      try
      {
        NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        XDocument xDocument = _adapterProvider.GetDataProjection(project, app, resource, ref format, start, limit, sortOrder, sortBy, fullIndex, parameters);
        _adapterProvider.FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    //[Description("Gets the Header of the specified project, application and resource.")]
    //[WebInvoke(Method="HEAD", UriTemplate = "/{app}/{project}/{resource}")]
    //public void GetHeader(string project, string app, string resource)
    //{
    //    try
    //    {
    //        //...
    //    }
    //    catch (Exception ex)
    //    {
    //        ExceptionHandler(ex);
    //    }
    //}

    //[Description("Gets an XML or JSON projection of a single item in the specified project, application and resource in the format specified. Valid formats include json, xml, p7xml, and rdf.")]
    //[WebGet(UriTemplate = "/{app}/{project}/{resource}/{id}?format={format}")]

    public void GetItem(string project, string app, string resource, string id, string format)
    {
      try
      {
        object content = _adapterProvider.GetDataProjection(project, app, resource, String.Empty, id, ref format, false);
        _adapterProvider.FormatOutgoingMessage(content, format);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void GetPicklists(string project, string app, string format)
    {
      format = MapContentType(format);
      try
      {
        IList<PicklistObject> objs = _adapterProvider.GetPicklists(project, app, format);
        if (format.ToLower() == "xml") //there is Directory in Picklists, have to use DataContractSerializer
          _adapterProvider.FormatOutgoingMessage<IList<PicklistObject>>(objs, format, true);
        else
          _adapterProvider.FormatOutgoingMessage<IList<PicklistObject>>(objs, format, false);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void GetPicklist(string project, string app, string name, string format, int start, int limit)
    {
      format = MapContentType(format);
      try
      {
        Picklists obj = _adapterProvider.GetPicklist(project, app, name, format, start, limit);

        XElement xml = obj.ToXElement<Picklists>();

        if (format.ToLower() == "xml") //there is Directory in Picklists, have to use DataContractSerializer
          _adapterProvider.FormatOutgoingMessage(xml, format);
        else
          _adapterProvider.FormatOutgoingMessage(xml, format);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void GetWithFilter(string project, string app, string resource, string format, int start, int limit, string indexStyle, Stream stream)
    {
      try
      {
        format = MapContentType(format);

        bool fullIndex = false;

        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        if (IsAsync())
        {
          DataFilter filter = _adapterProvider.FormatIncomingMessage<DataFilter>(stream, format, true);
          string statusUrl = _adapterProvider.AsyncGetWithFilter(project, app, resource, format,
            start, limit, fullIndex, filter);
          WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
          WebOperationContext.Current.OutgoingResponse.Headers["location"] = statusUrl;
        }
        else
        {
          DataFilter filter = _adapterProvider.FormatIncomingMessage<DataFilter>(stream, format, true);
          XDocument xDocument = null;

          if (filter != null && filter.RollupExpressions != null && filter.RollupExpressions.Count > 0)
          {
            xDocument = _adapterProvider.GetDataProjectionWithRollups(project, app, resource, filter, ref format, start, limit, fullIndex);
          }
          else
          {
            xDocument = _adapterProvider.GetDataProjection(project, app, resource, filter, ref format, start, limit, fullIndex);
          }

          _adapterProvider.FormatOutgoingMessage(xDocument.Root, format);
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void GetSearch(string project, string app, string resource, string query, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      try
      {
        NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        XDocument xDocument = _adapterProvider.GetDataProjection(project, app, resource, ref format, query, start, limit, sortOrder, sortBy, fullIndex, parameters);
        _adapterProvider.FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    //NOTE: Due to uri conflict, this template serves both part 7 individual and non-part7 related items.
    public void GetIndividual(string project, string app, string resource, string id, string related, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      try
      {
        if (format != null)
        {
          format = format.ToLower();

          // if format is one of the part 7 formats
          if (format == "rdf" || format == "dto" || format == "p7xml")
          {
            // id is clazz, related is individual
            object content = _adapterProvider.GetDataProjection(project, app, resource, id, related, ref format, false);
            _adapterProvider.FormatOutgoingMessage(content, format);
          }
        }
        else
        {
          NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

          bool fullIndex = false;
          if (indexStyle != null && indexStyle.ToUpper() == "FULL")
            fullIndex = true;

          XDocument xDocument = _adapterProvider.GetDataProjection(project, app, resource, id, related, ref format, start, limit, sortOrder, sortBy, fullIndex, parameters);
          _adapterProvider.FormatOutgoingMessage(xDocument.Root, format);
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void GetRelatedItem(string project, string app, string resource, string id, string related, string relatedId, string format)
    {
      try
      {
        if (format != null)
        {
          format = format.ToLower();

          // if format is one of the part 7 formats
          if (format == "rdf" || format == "dto" || format == "p7xml")
          {
            throw new Exception("Not supported.");
          }
        }
        else
        {
          XDocument xDocument = _adapterProvider.GetDataProjection(project, app, resource, id, related, relatedId, ref format);
          _adapterProvider.FormatOutgoingMessage(xDocument.Root, format);
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void UpdateList(string project, string app, string resource, string format, Stream stream)
    {
      format = MapContentType(format);

      if (format == "raw")
      {
        throw new Exception("");
      }
      else
      {
        XElement xElement = _adapterProvider.FormatIncomingMessage(stream, format);

        Response response = _adapterProvider.Post(project, app, resource, format, new XDocument(xElement));

        _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
      }
    }

    public void UpdateRelatedList(string project, string app, string resource, string parentid, string relatedResource, string format, Stream stream)
    {
      format = MapContentType(format);

      if (format == "raw")
      {
        throw new Exception("");
      }
      else
      {
        XElement xElement = _adapterProvider.FormatIncomingMessage(stream, format);

        Response response = _adapterProvider.PostRelated(project, app, resource, parentid, relatedResource, format, new XDocument(xElement));

        _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
      }
    }

    public void UpdateItem(string project, string app, string resource, string id, string format, Stream stream)
    {
      _logger.Debug("I'm in!");

      format = MapContentType(format);

      Response response = new Response();

      if (format == "raw")
      {
        response = _adapterProvider.PostContent(project, app, resource, format, id, stream);
      }
      else if (format == "json")
      {
        DataItems dataItems = _adapterProvider.FormatIncomingMessage(stream);

        if (dataItems != null && dataItems.items.Count > 0)
        {
          dataItems.items[0].id = id;
          XElement xElement = dataItems.ToXElement();
          response = _adapterProvider.Post(project, app, resource, format, new XDocument(xElement));
        }
      }
      else
      {
        DataItems dataItems = _adapterProvider.FormatIncomingMessage(stream);

        dataItems.items[0].id = id;

        _logger.Debug("Deserialized!");

        response = _adapterProvider.Post(project, app, resource, format, dataItems);
      }

      PrepareResponse(ref response);

      _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
    }

    public void UpdateRelatedItem(string project, string app, string resource, string parentid, string relatedresource, string id, string format, Stream stream)
    {
      _logger.Debug("I'm in!");

      format = MapContentType(format);

      Response response = new Response();

      if (format == "raw")
      {
        response = _adapterProvider.PostContent(project, app, resource, format, id, stream);
      }
      else if (format == "json")
      {
        DataItems dataItems = _adapterProvider.FormatIncomingMessage(stream);

        if (dataItems != null && dataItems.items.Count > 0)
        {
          dataItems.items[0].id = id;
          XElement xElement = dataItems.ToXElement();
          response = _adapterProvider.PostRelated(project, app, resource, parentid, relatedresource, format, new XDocument(xElement));
        }
      }
      else
      {
        DataItems dataItems = _adapterProvider.FormatIncomingMessage(stream);

        dataItems.items[0].id = id;

        _logger.Debug("Deserialized!");

        response = _adapterProvider.Post(project, app, resource, format, dataItems);
      }

      PrepareResponse(ref response);

      _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
    }

    private void PrepareResponse(ref Response response)
    {
      switch (response.Level)
      {
        case StatusLevel.Error:
          response.StatusCode = HttpStatusCode.InternalServerError;
          break;
        default:
          response.StatusCode = HttpStatusCode.OK;
          break;
      }

      foreach (Status status in response.StatusList)
      {
        foreach (string msg in status.Messages)
        {
          response.StatusText += msg;
        }
      }
    }

    public void CreateItem(string project, string app, string resource, string format, Stream stream)
    {
      format = MapContentType(format);

      if (format == "raw")
      {
        throw new Exception("");
      }
      else
      {
        XElement xElement = _adapterProvider.FormatIncomingMessage(stream, format);

        Response response = _adapterProvider.Post(project, app, resource, format, new XDocument(xElement));

        PrepareResponse(ref response);

        _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
      }
    }

    public void CreateRelatedItem(string project, string app, string resource, string format, string parentid, string relatedresource, Stream stream)
    {
      format = MapContentType(format);

      if (format == "raw")
      {
        throw new Exception("");
      }
      else
      {
        XElement xElement = _adapterProvider.FormatIncomingMessage(stream, format);

        Response response = _adapterProvider.PostRelated(project, app, resource, parentid, relatedresource, format, new XDocument(xElement));

        PrepareResponse(ref response);

        _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
      }
    }

    public void DeleteItem(string project, string app, string resource, string id, string format)
    {
      try
      {

        format = MapContentType(format);

        Response response = _adapterProvider.DeleteIndividual(project, app, resource, id, format);

        PrepareResponse(ref response);

        _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);

      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void DeleteRelatedItem(string project, string app, string resource, string parentid, string relatedresource, string id, string format)
    {
      try
      {

        format = MapContentType(format);

        Response response = _adapterProvider.DeleteRelated(project, app, resource, parentid, relatedresource, id, format);

        PrepareResponse(ref response);

        _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);

      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void GetSummary(string project, string app)
    {
      try
      {
        IList<Object> objects = _adapterProvider.GetSummary(project, app);
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        String json = serializer.Serialize(objects);

        HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
        HttpContext.Current.Response.Write(json);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }


    #region Async request queue
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

    #region "All" Methods
    public void GetDictionaryAll(string app, string format)
    {
      GetDictionary("all", app, format);
    }

    public void GetListAll(string app, string resource, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      GetList("all", app, resource, format, start, limit, sortOrder, sortBy, indexStyle);
    }

    public void GetItemAll(string app, string resource, string id, string format)
    {
      GetItem("all", app, resource, id, format);
    }

    public void GetWithFilterAll(string app, string resource, string format, int start, int limit, string indexStyle, Stream stream)
    {
      GetWithFilter("all", app, resource, format, start, limit, indexStyle, stream);
    }

    public void GetIndividualAll(string app, string resource, string clazz, string id, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      GetIndividual("all", app, resource, clazz, id, format, start, limit, sortOrder, sortBy, indexStyle);
    }

    public void GetRelatedItemAll(string app, string resource, string id, string related, string relatedId, string format)
    {
      GetRelatedItem("all", app, resource, id, related, relatedId, format);
    }

    public void UpdateListAll(string app, string resource, string format, Stream stream)
    {
      UpdateList("all", app, resource, format, stream);
    }

    public void UpdateItemAll(string app, string resource, string id, string format, Stream stream)
    {
      UpdateItem("all", app, resource, id, format, stream);
    }

    public void CreateItemAll(string app, string resource, string format, Stream stream)
    {
      CreateItem("all", app, resource, format, stream);
    }

    public void DeleteItemAll(string app, string resource, string id, string format)
    {
      DeleteItem("all", app, resource, id, format);
    }
    #endregion

    #region Private Methods
    private string MapContentType(string format)
    {
      IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;

      string contentType = request.ContentType;

      // if it's a known format then return it
      if (format != null && (format.ToLower().Contains("xml") || format.ToLower().Contains("json") ||
        format.ToLower().Contains("dto") || format.ToLower().Contains("rdf")))
      {
        return format;
      }

      // default to json, but honor the contentType of the request if it has one.
      format = "json";

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
    #endregion
  }
}