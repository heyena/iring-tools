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
using System;
using System.Web;
using org.iringtools.utility;
using System.Net;
using System.IO;
using org.iringtools.mapping;
using System.Collections;

namespace org.iringtools.services
{
  [ServiceContract(Namespace = "http://iringtools.org/services")]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class DataTransferService
  {
    private DtoProvider _dtoProvider = null;

    public DataTransferService()
    {
      _dtoProvider = new DtoProvider(ConfigurationManager.AppSettings);
    }

    [Description("Gets service version.")]
    [WebGet(UriTemplate = "/version")]
    public void GetVersion()
    {
      try
      {
        VersionInfo versionInfo = _dtoProvider.GetVersion();

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<VersionInfo>(versionInfo));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets list of scopes at this endpoint.")]
    [WebGet(UriTemplate = "/scopes")]
    public void GetScopeList()
    {
      try
      {
        NameValueList scopeList = _dtoProvider.GetScopeList();

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<NameValueList>(scopeList));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets list of scopes at this endpoint.")]
    [WebGet(UriTemplate = "/scopes/{scope}/apps")]
    public void GetAppList(string scope)
    {
      try
      {
        NameValueList appList = _dtoProvider.GetAppList(scope);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<NameValueList>(appList));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }
    
    [Description("Gets list of scopes at this endpoint.")]
    [WebGet(UriTemplate = "/scopes/{scope}/apps/{app}/graphs")]
    public void GetGraphList(string scope, string app)
    {
      try
      {
        NameValueList graphList = _dtoProvider.GetGraphList(scope, app);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<NameValueList>(graphList));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets data mode cache/live for an application.")]
    [WebInvoke(Method = "GET", UriTemplate = "/{scope}/{app}/datamode")]
    public void GetDataMode(string scope, string app)
    {
      try
      {
        string dataMode = _dtoProvider.GetDataMode(scope, app);

        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(dataMode);
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets cache information for an application.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}/cacheinfo")]
    public void GetCacheInfo(string scope, string app, string graph)
    {
      try
      {
        CacheInfo cacheInfo = _dtoProvider.GetCacheInfo(scope, app, graph);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<CacheInfo>(cacheInfo));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets manifest of an application.")]
    [WebGet(UriTemplate = "/{scope}/{app}/manifest")]
    public void GetManifest(string scope, string app)
    {
      try
      {
        Manifest manifest = _dtoProvider.GetManifest(scope, app);
        
        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<Manifest>(manifest));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets manifest for a specific graph of an application.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}/manifest")]
    public void GetManifestByGraph(string scope, string app, string graph)
    {
      try
      {
        Manifest manifest = _dtoProvider.GetManifest(scope, app, graph);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<Manifest>(manifest));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Get manifest for user")]
    [WebGet(UriTemplate = "/manifest/{userName}?siteId={siteId}&graphId={graphId}&format={format}")]
    public void GetManifestForUser(string userName, int siteId, Guid graphId, string format)
    {
        try
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

             Manifest manifest = _dtoProvider.GetManifestForUser(userName, siteId, graphId);
            _dtoProvider.FormatOutgoingMessage<Manifest>(manifest, format, true);
        }
        catch (Exception ex)
        {
            ExceptionHander(ex);
        }
    }

    [Description("Gets data transfer indices of requested manifest.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/dxi?hashAlgorithm={hashAlgorithm}")]
    public void GetDataTransferIndicesWithManifest(string scope, string app, string graph, string hashAlgorithm, Manifest manifest)
    {
      try
      {
        DataTransferIndices dtis = _dtoProvider.GetDataTransferIndicesWithManifest(scope, app, graph, hashAlgorithm, manifest);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<DataTransferIndices>(dtis));        
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets data transfer indices of requested manifest and filter.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/dxi/filter?hashAlgorithm={hashAlgorithm}")]
    public void GetDataTransferIndicesWithFilter(string scope, string app, string graph, string hashAlgorithm, DxiRequest request)
    {
      try
      {
        if (IsAsync())
        {
          string statusURL = _dtoProvider.AsyncGetDataTransferIndicesWithFilter(scope, app, graph, hashAlgorithm, request);
          WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
          WebOperationContext.Current.OutgoingResponse.Headers["location"] = statusURL;
        }
        else
        {
          DataTransferIndices dtis = _dtoProvider.GetDataTransferIndicesWithFilter(scope, app, graph, hashAlgorithm, request);

          HttpContext.Current.Response.ContentType = "application/xml";
          HttpContext.Current.Response.Write(Utility.SerializeDataContract<DataTransferIndices>(dtis));
        }
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets internal identifiers for a given manifest and filter.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/filter")]
    public void GetInternalIdentifiers(string scope, string app, string graph, DxiRequest request)
    {
      try
      {
        if (IsAsync())
        {
          string statusURL = _dtoProvider.AsyncGetInternalIdentifiers(scope, app, graph, request);
          WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
          WebOperationContext.Current.OutgoingResponse.Headers["location"] = statusURL;
        }
        else
        {
          Identifiers identifiers = _dtoProvider.GetInternalIdentifiers(scope, app, graph, request);

          HttpContext.Current.Response.ContentType = "application/xml";
          HttpContext.Current.Response.Write(Utility.SerializeDataContract<Identifiers>(identifiers));
        }
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets data transfer objects for a given manifest and filter.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/dto?start={start}&limit={limit}")]
    public void GetPageDataTransferObjects(string scope, string app, string graph, DxiRequest request, int start, int limit)
    {
      try
      {
        DataTransferObjects dtos = _dtoProvider.GetPageDataTransferObjects(scope, app, graph, request, start, limit);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<DataTransferObjects>(dtos));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets a page data transfer indices.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}/dti?start={start}&limit={limit}")]
    public void GetPagedDataTransferIndices(string scope, string app, string graph, int start, int limit)
    {
      try
      {
        DataTransferIndices dtis = _dtoProvider.GetPagedDataTransferIndices(scope, app, graph, null, start, limit);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<DataTransferIndices>(dtis));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets a page data transfer indices by graphid.")]
    [WebGet(UriTemplate = "/{userName}/{graphId}/{siteId}/dtiByGraphId?start={start}&limit={limit}")]
    public void GetPagedDataTransferIndicesByGraphID(string userName, string graphId, string siteId, int start, int limit)
    {
        try
        {
            DataTransferIndices dtis = _dtoProvider.GetPagedDataTransferIndicesByGraphID(userName, graphId, siteId, null, start, limit);

            HttpContext.Current.Response.ContentType = "application/xml";
            HttpContext.Current.Response.Write(Utility.SerializeDataContract<DataTransferIndices>(dtis));
        }
        catch (Exception ex)
        {
            ExceptionHander(ex);
        }
    }


    [Description("Gets a page data transfer indices with filter.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/dti?start={start}&limit={limit}")]
    public void GetPagedDataTransferIndicesWithFilter(string scope, string app, string graph, DataFilter filter, int start, int limit)
    {
      try
      {
        DataTransferIndices dtis = _dtoProvider.GetPagedDataTransferIndices(scope, app, graph, filter, start, limit);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<DataTransferIndices>(dtis));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets data transfer objects of requested indices.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/page")]
    public void GetDataTransferObjects(string scope, string app, string graph, DataTransferIndices dataTransferIndices)
    {
      try
      {
        DataTransferObjects dtos = _dtoProvider.GetDataTransferObjects(scope, app, graph, dataTransferIndices);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<DataTransferObjects>(dtos));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets data transfer objects of requested indices and manifest.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/dxo?includeContent={includeContent}")]
    public void GetDataTransferObjectsWithManifest(string scope, string app, string graph, DxoRequest dxoRequest, bool includeContent)
    {
      try
      {
        if (IsAsync())
        {
          string statusURL = _dtoProvider.AsyncGetDataTransferObjects(scope, app, graph, dxoRequest, includeContent);
          WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
          WebOperationContext.Current.OutgoingResponse.Headers["location"] = statusURL;
        }
        else
        {
          DataTransferObjects dtos = _dtoProvider.GetDataTransferObjects(scope, app, graph, dxoRequest, includeContent);

          HttpContext.Current.Response.ContentType = "application/xml";
          HttpContext.Current.Response.Write(Utility.SerializeDataContract<DataTransferObjects>(dtos));
        }
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets single data transfer object by id.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}/{id}")]
    public void GetDataTransferObject(string scope, string app, string graph, string id)
    {
      try
      {
        DataTransferObjects dtos = _dtoProvider.GetDataTransferObject(scope, app, graph, id);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<DataTransferObjects>(dtos));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Posts data transfer objects to perfom add/update/delete.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}")]
    public void PostDataTransferObjects(string scope, string app, string graph, DataTransferObjects dataTransferObjects)
    {
      try
      {
        Response response = _dtoProvider.PostDataTransferObjects(scope, app, graph, dataTransferObjects);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<Response>(response));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Posts data transfer objects as stream to perform add/update/delete.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}?format=stream")]
    public void PostDataTransferObjectsStream(string scope, string app, string graph, Stream stream)
    {
      try
      {
        MemoryStream ms = stream.ToMemoryStream();
        DataTransferObjects dtos = Utility.DeserializeFromStream<DataTransferObjects>(ms);
         
        if (IsAsync())
        {
          string statusURL = _dtoProvider.AsyncPostDataTransferObjects(scope, app, graph, dtos);
          WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
          WebOperationContext.Current.OutgoingResponse.Headers["location"] = statusURL;
        }
        else
        {
          Response response = _dtoProvider.PostDataTransferObjects(scope, app, graph, dtos);
          string responseXml = Utility.SerializeDataContract<Response>(response);

          HttpContext.Current.Response.ContentType = "application/xml";
          HttpContext.Current.Response.Write(responseXml);
        }
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets content objects by ids and formats in JSON format.")]
    [WebInvoke(Method = "GET", UriTemplate = "/{scope}/{app}/{graph}/content?filter={filter}")]
    public void GetContents(string scope, string app, string graph, string filter)
    {
      try
      {
        ContentObjects contentObjects = _dtoProvider.GetContents(scope, app, graph, filter);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<ContentObjects>(contentObjects));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Gets single content object.")]
    [WebInvoke(Method = "GET", UriTemplate = "/{scope}/{app}/{graph}/{id}/content?format={format}")]
    public void GetContent(string scope, string app, string graph, string id, string format)
    {
      try
      {
        IContentObject iContentObject = _dtoProvider.GetContent(scope, app, graph, id, format);
        HttpContext.Current.Response.ContentType = iContentObject.ContentType;
        HttpContext.Current.Response.BinaryWrite(iContentObject.Content.ToMemoryStream().GetBuffer());
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Creates/rebuilds graph data cache.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}/refresh")]
    public void RefreshCache(string scope, string app, string graph)
    {
      try
      {
        Response response = _dtoProvider.RefreshCache(scope, app, graph);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<Response>(response));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    }

    [Description("Imports graph data cache. Cache files are baseUri followed by <object type>.dat is required.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}/import?baseuri={baseUri}")]
    public void ImportCache(string scope, string app, string graph, string baseUri)
    {
      try
      {
        Response response = _dtoProvider.ImportCache(scope, app, graph, baseUri);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<Response>(response));
      }
      catch (Exception ex)
      {
        ExceptionHander(ex);
      }
    } 
        
    [Description("Gets status of a asynchronous request.")]
    [WebGet(UriTemplate = "/requests/{id}")]
    public void GetRequestStatus(string id)
    {
      RequestStatus status = null;

      try
      {
        OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
        status = _dtoProvider.GetRequestStatus(id);

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

      string xml = Utility.SerializeDataContract<RequestStatus>(status);
      HttpContext.Current.Response.ContentType = "application/xml";
      HttpContext.Current.Response.Write(xml);
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

    private void ExceptionHander(Exception ex)
    {
      string statusText = string.Empty;

      if (ex is WebFaultException && ex.Data != null)
      {
        foreach (DictionaryEntry entry in ex.Data)
        {
          statusText += ex.Data[entry.Key].ToString();
        }
      }

      if (string.IsNullOrEmpty(statusText))
      {
        statusText = ex.ToString();
      }

      WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
      HttpContext.Current.Response.ContentType = "text/plain";
      HttpContext.Current.Response.Write(statusText);
    }
  }
}