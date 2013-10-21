﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using System.Web;
using System.Xml.Linq;
using net.java.dev.wadl;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;

namespace org.iringtools.services
{
  public class CommonDataService
  {
      private AbstractProvider _abstractPrivder = null;

    public CommonDataService()
    {
        _abstractPrivder = new AbstractProvider(ConfigurationManager.AppSettings);
    }

    public void GetVersion(string format)
    {
      format = MapContentType(null, null, format);

      VersionInfo version = _abstractPrivder.GetVersion();

      _abstractPrivder.FormatOutgoingMessage<VersionInfo>(version, format, true);
    }

    public void GetContexts(string app, string format)
    {
      format = MapContentType(null, null, format);

      Contexts contexts = _abstractPrivder.GetContexts(app);

      _abstractPrivder.FormatOutgoingMessage<Contexts>(contexts, format, true);
    }

    public void GetAllWADL(string app)
    {
        WADLApplication wadl = _abstractPrivder.GetWADL("all", app);

        _abstractPrivder.FormatOutgoingMessage<WADLApplication>(wadl, "xml", false);
    }

    public void GetAppWADL(string app)
    {
        WADLApplication wadl = _abstractPrivder.GetWADL("app", app);

        _abstractPrivder.FormatOutgoingMessage<WADLApplication>(wadl, "xml", false);
    }

    public void GetScopeWADL(string app, string project)
    {
        WADLApplication wadl = _abstractPrivder.GetWADL(project, app);

      _abstractPrivder.FormatOutgoingMessage<WADLApplication>(wadl, "xml", false);
    }

    public void GetDictionary(string project, string app, string format)
    {
      try
      {
        format = MapContentType(project, app, format);

        if (IsAsync())
        {
            string statusUrl = _abstractPrivder.AsyncGetDictionary(project, app);
          WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
          WebOperationContext.Current.OutgoingResponse.Headers["location"] = statusUrl;
        }
        else
        {
            DataDictionary dictionary = _abstractPrivder.GetDictionary(project, app);
            _abstractPrivder.FormatOutgoingMessage<DataDictionary>(dictionary, format, true);
        }
      }
      catch (Exception ex)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(ex.Message);
      }
    }

    public void GetObjectType(string project, string app, string objectType, string format)
    {
      format = MapContentType(project, app, format);

      DataDictionary dictionary = _abstractPrivder.GetDictionary(project, app);

      DataObject dataObject = dictionary.dataObjects.Find(o => o.objectName.ToLower() == objectType.ToLower());

      if (dataObject == null)
        throw new FileNotFoundException();

      _abstractPrivder.FormatOutgoingMessage<DataObject>(dataObject, format, true);
    }

    public void GetList(string project, string app, string resource, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      try
      {
        // get list of contents is not allowed in this service
        if (string.IsNullOrEmpty(format) || !(format.ToLower() == "dto" || format.ToLower() == "rdf" ||
          format.ToLower().Contains("xml") || format.ToLower().Contains("json") || format.ToLower().Contains("jsonld")))
        {
          format = "json";
        }

        NameValueCollection parameters = new NameValueCollection() ;//FKM =  WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
        parameters.Add("format", format);

        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        XDocument xDocument = _abstractPrivder.GetList(project, app, resource, ref format, start, limit, sortOrder, sortBy, fullIndex, parameters);
        _abstractPrivder.FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void GetItem(string project, string app, string resource, string id, string format)
    {
      try
      {
        //format = MapContentType(project, app, format);
          object content = _abstractPrivder.GetItem(project, app, resource, String.Empty, id, ref format, false);
          _abstractPrivder.FormatOutgoingMessage(content, format);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void GetPicklists(string project, string app, string format)
    {
      try
      {
        format = MapContentType(project, app, format);

        IList<PicklistObject> objs = _abstractPrivder.GetPicklists(project, app, format);

        if (format.ToLower() == "xml") //there is Directory in Picklists, have to use DataContractSerializer
            _abstractPrivder.FormatOutgoingMessage<IList<PicklistObject>>(objs, format, true);
        else
            _abstractPrivder.FormatOutgoingMessage<IList<PicklistObject>>(objs, format, false);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void GetPicklist(string project, string app, string name, string format, int start, int limit)
    {
        format = MapContentType(project, app, format);
        try
        {
            Picklists obj = _abstractPrivder.GetPicklist(project, app, name, format, start, limit);

            XElement xml = obj.ToXElement<Picklists>();

            if (format.ToLower() == "xml") //there is Directory in Picklists, have to use DataContractSerializer
                _abstractPrivder.FormatOutgoingMessage(xml, format);
            else
                _abstractPrivder.FormatOutgoingMessage(xml, format);
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
        format = MapContentType(project, app, format);

        bool fullIndex = false;

        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        if (IsAsync())
        {
            DataFilter filter = _abstractPrivder.FormatIncomingMessage<DataFilter>(stream, format, true);
            string statusUrl = _abstractPrivder.AsyncGetWithFilter(project, app, resource, format,
            start, limit, fullIndex, filter);
          WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
          WebOperationContext.Current.OutgoingResponse.Headers["location"] = statusUrl;
        }
        else
        {
            DataFilter filter = _abstractPrivder.FormatIncomingMessage<DataFilter>(stream, format, true);
          XDocument xDocument = null;

          xDocument = _abstractPrivder.GetWithFilter(project, app, resource, filter, ref format, start, limit, fullIndex);

          _abstractPrivder.FormatOutgoingMessage(xDocument.Root, format);
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void Search(string project, string app, string resource, string query, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      try
      {
        NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

        bool fullIndex = false;
        if (indexStyle != null && indexStyle.ToUpper() == "FULL")
          fullIndex = true;

        XDocument xDocument = _abstractPrivder.Search(project, app, resource, ref format, query, start, limit, sortOrder, sortBy, fullIndex, parameters);
        _abstractPrivder.FormatOutgoingMessage(xDocument.Root, format);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    //NOTE: Due to uri conflict, this template serves both part 7 individual and non-part7 related items.
    public void GetRelatedList(string project, string app, string resource, string id, string related, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      try
      {
        format = MapContentType(project, app, format);

        // if format is one of the part 7 formats
        if (format == "rdf" || format == "dto")
        {
          // id is clazz, related is individual
            object content = _abstractPrivder.GetItem(project, app, resource, id, related, ref format, false);
            _abstractPrivder.FormatOutgoingMessage(content, format);
        }
        else
        {
          NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

          bool fullIndex = false;
          if (indexStyle != null && indexStyle.ToUpper() == "FULL")
            fullIndex = true;

          XDocument xDocument = _abstractPrivder.GetRelatedList(project, app, resource, id, related, ref format, start, limit, sortOrder, sortBy, fullIndex, parameters);
          _abstractPrivder.FormatOutgoingMessage(xDocument.Root, format);
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
        format = MapContentType(project, app, format);

        // if format is one of the part 7 formats
        if (format == "rdf" || format == "dto")
        {
          throw new Exception("Not supported.");
        }
        else
        {
            XDocument xDocument = _abstractPrivder.GetRelatedItem(project, app, resource, id, related, relatedId, ref format);
            _abstractPrivder.FormatOutgoingMessage(xDocument.Root, format);
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void UpdateList(string project, string app, string resource, string format, Stream stream)
    {
      Response response = new Response();

      try
      {
        format = MapContentType(project, app, format);

        if (format == "raw")
        {
          throw new Exception("");
        }
        else
        {
            XElement xElement = _abstractPrivder.FormatIncomingMessage(stream, format);

            response = _abstractPrivder.Update(project, app, resource, format, new XDocument(xElement));
        }
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
    }

    public void UpdateRelatedList(string project, string app, string resource, string parentid, string relatedResource, string format, Stream stream)
    {
      Response response = new Response();

      try
      {
        format = MapContentType(project, app, format);

        if (format == "raw")
        {
          throw new Exception("");
        }
        else
        {
            XElement xElement = _abstractPrivder.FormatIncomingMessage(stream, format);

            response = _abstractPrivder.UpdateRelated(project, app, resource, parentid, relatedResource, format, new XDocument(xElement));

        }
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
    }

    public void UpdateItem(string project, string app, string resource, string id, string format, Stream stream)
    {
      Response response = new Response();

      try
      {
        format = MapContentType(project, app, format);

        if (format == "raw")
        {
            response = _abstractPrivder.PostContent(project, app, resource, format, id, stream);
        }
        else if (format == "json")
        {
            DataItems dataItems = _abstractPrivder.FormatIncomingMessage(stream);

          if (dataItems != null && dataItems.items != null && dataItems.items.Count > 0)
          {
            dataItems.items[0].id = id;
            XElement xElement = dataItems.ToXElement();
            response = _abstractPrivder.Update(project, app, resource, format, new XDocument(xElement));
          }
          else
          {
            throw new Exception("No items to update or invalid payload.");
          }
        }
        else
        {
            DataItems dataItems = _abstractPrivder.FormatIncomingMessage(stream);
          dataItems.items[0].id = id;

          response = _abstractPrivder.Update(project, app, resource, format, dataItems);
        }
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      PrepareResponse(ref response);
      _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
    }

    public void UpdateRelatedItem(string project, string app, string resource, string parentid, string relatedresource, string id, string format, Stream stream)
    {
      Response response = new Response();

      try
      {
        format = MapContentType(project, app, format);

        if (format == "raw")
        {
            response = _abstractPrivder.PostContent(project, app, resource, format, id, stream);
        }
        else if (format == "json")
        {
            DataItems dataItems = _abstractPrivder.FormatIncomingMessage(stream);

          if (dataItems != null && dataItems.items != null && dataItems.items.Count > 0)
          {
            dataItems.items[0].id = id;
            XElement xElement = dataItems.ToXElement();
            response = _abstractPrivder.UpdateRelated(project, app, resource, parentid, relatedresource, format, new XDocument(xElement));
          }
          else
          {
            throw new Exception("No items to update or invalid payload.");
          }
        }
        else
        {
            DataItems dataItems = _abstractPrivder.FormatIncomingMessage(stream);
          dataItems.items[0].id = id;

          response = _abstractPrivder.Update(project, app, resource, format, dataItems);
        }
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      PrepareResponse(ref response);
      _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
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

    public void CreateItem(string project, string app, string resource, string format, Stream stream)
    {
      Response response = new Response();

      try
      {
        format = MapContentType(project, app, format);

        if (format == "raw")
        {
          throw new Exception("");
        }
        else
        {
            XElement xElement = _abstractPrivder.FormatIncomingMessage(stream, format);

            response = _abstractPrivder.Create(project, app, resource, format, new XDocument(xElement));
        }
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      PrepareResponse(ref response);
      _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
    }

    public void CreateRelatedItem(string project, string app, string resource, string format, string parentid, string relatedResource, Stream stream)
    {
      Response response = new Response();

      try
      {
        format = MapContentType(project, app, format);

        if (format == "raw")
        {
          throw new Exception("");
        }
        else
        {
            XElement xElement = _abstractPrivder.FormatIncomingMessage(stream, format);

            response = _abstractPrivder.UpdateRelated(project, app, resource, parentid, relatedResource, format, new XDocument(xElement));
        }
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      PrepareResponse(ref response);
      _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
    }

    public void DeleteItem(string project, string app, string resource, string id, string format)
    {
      Response response = new Response();
      
      try
      {
        format = MapContentType(project, app, format);

        response = _abstractPrivder.DeleteItem(project, app, resource, id, format);

      }
      catch (Exception ex)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(ex.Message);
      }

      PrepareResponse(ref response);
      _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
    }

    public void DeleteRelatedItem(string project, string app, string resource, string parentid, string relatedresource, string id, string format)
    {
      Response response = new Response();

      try
      {
        format = MapContentType(project, app, format);

        response = _abstractPrivder.DeleteRelated(project, app, resource, parentid, relatedresource, id, format);
      }
      catch (Exception ex)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(ex.Message);
      }

      PrepareResponse(ref response);
      _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
    }

    #region Async request queue
    public void GetRequestStatus(string id)
    {
      RequestStatus status = null;

      try
      {
        OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
        status = _abstractPrivder.GetRequestStatus(id);

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

      _abstractPrivder.FormatOutgoingMessage<RequestStatus>(status, "xml", true);
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

    public void GetRelatedListAll(string app, string resource, string clazz, string id, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
    {
      GetRelatedList("all", app, resource, clazz, id, format, start, limit, sortOrder, sortBy, indexStyle);
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
    private string MapContentType(string project, string app, string format)
    {
      //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
      //string contentType = request.ContentType;

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