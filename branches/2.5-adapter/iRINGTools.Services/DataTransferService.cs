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
using System;
using System.Web;
using org.iringtools.utility;
using System.Net;
using System.IO;

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
      catch (Exception e)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(e.ToString());
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
      catch (Exception e)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(e.ToString());
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
      catch (Exception e)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(e.ToString());
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
      catch (Exception e)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(e.ToString());
      }
    }

    [Description("Gets data transfer indices of requested manifest with filter.")]
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
      catch (Exception e)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(e.ToString());
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
      catch (Exception e)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(e.ToString());
      }
    }

    [Description("Gets data transfer objects of requested indices and manifest.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/dxo")]
    public void GetDataTransferObjectsWithManifest(string scope, string app, string graph, DxoRequest dxoRequest)
    {
      try
      {
        if (IsAsync())
        {
          string statusURL = _dtoProvider.AsyncGetDataTransferObjects(scope, app, graph, dxoRequest);
          WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
          WebOperationContext.Current.OutgoingResponse.Headers["location"] = statusURL;
        }
        else
        {
          DataTransferObjects dtos = _dtoProvider.GetDataTransferObjects(scope, app, graph, dxoRequest);

          HttpContext.Current.Response.ContentType = "application/xml";
          HttpContext.Current.Response.Write(Utility.SerializeDataContract<DataTransferObjects>(dtos));
        }
      }
      catch (Exception e)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(e.ToString());
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
      catch (Exception e)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(e.ToString());
      }
    }

    [Description("Post data transfer objects to perfom add/update/delete.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}")]
    public void PostDataTransferObjects(string scope, string app, string graph, DataTransferObjects dataTransferObjects)
    {
      try
      {
        Response response = _dtoProvider.PostDataTransferObjects(scope, app, graph, dataTransferObjects);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<Response>(response));
      }
      catch (Exception e)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(e.ToString());
      }
    }

    [Description("Posts data transfer objects as stream to perform add/update/delete.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}?format=stream")]
    public void PostStream(string scope, string app, string graph, Stream stream)
    {
      try
      {
        DataTransferObjects dtos = Utility.DeserializeFromStream<DataTransferObjects>(stream.ToMemoryStream(), true);
         
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
      catch (Exception e)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(e.ToString());
      }
    }

    [Description("Deletes a data transfer object by id.")]
    [WebInvoke(Method = "DELETE", UriTemplate = "/{scope}/{app}/{graph}/{id}")]
    public void DeletetDataTransferObject(string scope, string app, string graph, string id)
    {
      try
      {
        Response response = _dtoProvider.DeleteDataTransferObject(scope, app, graph, id);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(Utility.SerializeDataContract<Response>(response));
      }
      catch (Exception e)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
        HttpContext.Current.Response.ContentType = "text/plain";
        HttpContext.Current.Response.Write(e.ToString());
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
  }
}