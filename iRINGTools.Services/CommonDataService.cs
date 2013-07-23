using System;
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
    private AdapterProvider _adapterProvider = null;

    public CommonDataService()
    {
      _adapterProvider = new AdapterProvider(ConfigurationManager.AppSettings);
    }

    public void GetVersion(string format)
    {
      format = MapContentType(null, null, format);

      VersionInfo version = _adapterProvider.GetVersion();

      _adapterProvider.FormatOutgoingMessage<VersionInfo>(version, format, true);
    }

    public void GetContexts(string app, string format)
    {
      format = MapContentType(null, null, format);

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
        format = MapContentType(project, app, format);

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

    public void GetObjectType(string project, string app, string objectType, string format)
    {
      format = MapContentType(project, app, format);

      DataDictionary dictionary = _adapterProvider.GetDictionary(project, app);

      DataObject dataObject = dictionary.dataObjects.Find(o => o.objectName.ToLower() == objectType.ToLower());

      if (dataObject == null)
        throw new FileNotFoundException();

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

        XDocument xDocument = _adapterProvider.GetList(project, app, resource, ref format, start, limit, sortOrder, sortBy, fullIndex, parameters);
        _adapterProvider.FormatOutgoingMessage(xDocument.Root, format);
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
        format = MapContentType(project, app, format);
        object content = _adapterProvider.GetItem(project, app, resource, String.Empty, id, ref format, false);
        _adapterProvider.FormatOutgoingMessage(content, format);
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
        format = MapContentType(project, app, format);
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
        format = MapContentType(project, app, format);

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

          xDocument = _adapterProvider.GetWithFilter(project, app, resource, filter, ref format, start, limit, fullIndex);

          _adapterProvider.FormatOutgoingMessage(xDocument.Root, format);
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

        XDocument xDocument = _adapterProvider.Search(project, app, resource, ref format, query, start, limit, sortOrder, sortBy, fullIndex, parameters);
        _adapterProvider.FormatOutgoingMessage(xDocument.Root, format);
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
          object content = _adapterProvider.GetItem(project, app, resource, id, related, ref format, false);
          _adapterProvider.FormatOutgoingMessage(content, format);
        }
        else
        {
          NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

          bool fullIndex = false;
          if (indexStyle != null && indexStyle.ToUpper() == "FULL")
            fullIndex = true;

          XDocument xDocument = _adapterProvider.GetRelatedList(project, app, resource, id, related, ref format, start, limit, sortOrder, sortBy, fullIndex, parameters);
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
        format = MapContentType(project, app, format);

        // if format is one of the part 7 formats
        if (format == "rdf" || format == "dto")
        {
          throw new Exception("Not supported.");
        }
        else
        {
          XDocument xDocument = _adapterProvider.GetRelatedItem(project, app, resource, id, related, relatedId, ref format);
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
          XElement xElement = _adapterProvider.FormatIncomingMessage(stream, format);

          response = _adapterProvider.Update(project, app, resource, format, new XDocument(xElement));
        }
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
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
          XElement xElement = _adapterProvider.FormatIncomingMessage(stream, format);

          response = _adapterProvider.UpdateRelated(project, app, resource, parentid, relatedResource, format, new XDocument(xElement));

        }
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }
        
      _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
    }

    public void UpdateItem(string project, string app, string resource, string id, string format, Stream stream)
    {
      Response response = new Response();

      try
      {
        format = MapContentType(project, app, format);

        if (format == "raw")
        {
          response = _adapterProvider.PostContent(project, app, resource, format, id, stream);
        }
        else if (format == "json")
        {
          DataItems dataItems = _adapterProvider.FormatIncomingMessage(stream);

          if (dataItems != null && dataItems.items != null && dataItems.items.Count > 0)
          {
            dataItems.items[0].id = id;
            XElement xElement = dataItems.ToXElement();
            response = _adapterProvider.Update(project, app, resource, format, new XDocument(xElement));
          }
          else
          {
            throw new Exception("No items to update or invalid payload.");
          }
        }
        else
        {
          DataItems dataItems = _adapterProvider.FormatIncomingMessage(stream);
          dataItems.items[0].id = id;

          response = _adapterProvider.Update(project, app, resource, format, dataItems);
        }
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      PrepareResponse(ref response);
      _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
    }

    public void UpdateRelatedItem(string project, string app, string resource, string parentid, string relatedresource, string id, string format, Stream stream)
    {
      Response response = new Response();

      try
      {
        format = MapContentType(project, app, format);

        if (format == "raw")
        {
          response = _adapterProvider.PostContent(project, app, resource, format, id, stream);
        }
        else if (format == "json")
        {
          DataItems dataItems = _adapterProvider.FormatIncomingMessage(stream);

          if (dataItems != null && dataItems.items != null && dataItems.items.Count > 0)
          {
            dataItems.items[0].id = id;
            XElement xElement = dataItems.ToXElement();
            response = _adapterProvider.UpdateRelated(project, app, resource, parentid, relatedresource, format, new XDocument(xElement));
          }
          else
          {
            throw new Exception("No items to update or invalid payload.");
          }
        }
        else
        {
          DataItems dataItems = _adapterProvider.FormatIncomingMessage(stream);
          dataItems.items[0].id = id;

          response = _adapterProvider.Update(project, app, resource, format, dataItems);
        }
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      PrepareResponse(ref response);
      _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
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
          XElement xElement = _adapterProvider.FormatIncomingMessage(stream, format);

          response = _adapterProvider.Create(project, app, resource, format, new XDocument(xElement));
        }
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      PrepareResponse(ref response);
      _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
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
          XElement xElement = _adapterProvider.FormatIncomingMessage(stream, format);

          response = _adapterProvider.UpdateRelated(project, app, resource, parentid, relatedResource, format, new XDocument(xElement));
        }
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      PrepareResponse(ref response);
      _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
    }

    public void DeleteItem(string project, string app, string resource, string id, string format)
    {
      Response response = new Response();
      
      try
      {
        format = MapContentType(project, app, format);

        response = _adapterProvider.DeleteItem(project, app, resource, id, format);

      }
      catch (Exception ex)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(ex.Message);
      }

      PrepareResponse(ref response);
      _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
    }

    public void DeleteRelatedItem(string project, string app, string resource, string parentid, string relatedresource, string id, string format)
    {
      Response response = new Response();

      try
      {
        format = MapContentType(project, app, format);

        response = _adapterProvider.DeleteRelated(project, app, resource, parentid, relatedresource, id, format);
      }
      catch (Exception ex)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(ex.Message);
      }

      PrepareResponse(ref response);      
      _adapterProvider.FormatOutgoingMessage<Response>(response, format, false);
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
      IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
      string contentType = request.ContentType;

      // if it's a known format then return it
      if (format != null && (format.ToLower() == "raw" || format.ToLower() == "dto" || format.ToLower() == "rdf" ||
        format.ToLower().Contains("xml") || format.ToLower().Contains("json")))
      {
        return format;
      }

      // otherwise determine the appropriate format
      if (!string.IsNullOrEmpty(project) && !string.IsNullOrEmpty(app))
      {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string appConfigPath = string.Format(@"{0}\{1}\{2}.{3}.config", basePath, "App_Data", project, app);

        if (File.Exists(appConfigPath))
        {
          StaticDust.Configuration.AppSettingsReader appConfig =
            new StaticDust.Configuration.AppSettingsReader(appConfigPath);

          string defaultFormat = Convert.ToString(appConfig["DefaultFormat"]);

          if (!string.IsNullOrEmpty(defaultFormat))
          {
            format = defaultFormat.ToLower();
          }
        }
      }

      if (string.IsNullOrEmpty(format))
      {
        format = "json";
      }

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