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
using org.iringtools.exchange;

namespace org.iringtools.services
{
  [ServiceContract(Namespace = "http://iringtools.org/services")]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class DataTransferService
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DataTransferService));
    private DataTranferProvider _dxfrProvider = null;

    public DataTransferService()
    {
      _dxfrProvider = new DataTranferProvider(ConfigurationManager.AppSettings);
    }

    [Description("Gets dto provider version.")]
    [WebGet(UriTemplate = "/version")]
    public VersionInfo GetVersion()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _dxfrProvider.GetVersion();
    }

    [Description("Gets manifest for an application.")]
    [WebGet(UriTemplate = "/{scope}/{app}/manifest")]
    public Manifest GetManifest(string scope, string app)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _dxfrProvider.GetManifest(scope, app);
    }

    [Description("Gets data transfer indices for a particular graph.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}?hashAlgorithm={hashAlgorithm}")]
    public DataTransferIndices GetDataTransferIndices(string scope, string app, string graph, string hashAlgorithm)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      if (hashAlgorithm == null)
        hashAlgorithm = "MD5";
      return _dxfrProvider.GetDataTransferIndices(scope, app, graph, hashAlgorithm);
    }

    [Description("Gets data transfer indices according to data filter.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/filter?hashAlgorithm={hashAlgorithm}")]
    public DataTransferIndices GetDataTransferIndicesWithFilter(string scope, string app, string graph, string hashAlgorithm, DataFilter filter)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _dxfrProvider.GetDataTransferIndicesWithFilter(scope, app, graph, hashAlgorithm, filter);
    }
    [Description("Gets data transfer indices according to the posted manifest.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/dxi?hashAlgorithm={hashAlgorithm}")]
    public DataTransferIndices GetDataTransferIndicesWithManifest(string scope, string app, string graph, Manifest manifest, string hashAlgorithm)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _dxfrProvider.GetDataTransferIndicesWithManifest(scope, app, graph, hashAlgorithm, manifest);
    }
    [Description("Gets data transfer indices according to manifest and filter request.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/dxi/filter?hashAlgorithm={hashAlgorithm}")]
    public DataTransferIndices GetDataTransferIndicesByRequest(string scope, string app, string graph, string hashAlgorithm, DxiRequest request)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _dxfrProvider.GetDataTransferIndicesByRequest(scope, app, graph, hashAlgorithm, request);
    }
    [Description("Gets data transfer objects according to the posted data transfer indices.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/page")]
    public DataTransferObjects GetDataTransferObjects(string scope, string app, string graph, DataTransferIndices dataTransferIndices)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _dxfrProvider.GetDataTransferObjects(scope, app, graph, dataTransferIndices);
    }
    [Description("Gets data transfer objects according to the posted manifest and data transfer indices.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/dxo")]
    public DataTransferObjects GetDataTransferObjectsWithManifest(string scope, string app, string graph, DxoRequest dxoRequest)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _dxfrProvider.GetDataTransferObjects(scope, app, graph, dxoRequest);
    }
    [Description("Gets single data transfer object by id.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}/{id}")]
    public DataTransferObjects GetDataTransferObject(string scope, string app, string graph, string id)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _dxfrProvider.GetDataTransferObject(scope, app, graph, id);
    }
    [Description("Posts data transfer objects to add/update/delete to data layer.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}")]
    public Response PostDataTransferObjects(string scope, string app, string graph, DataTransferObjects dataTransferObjects)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _dxfrProvider.PostDataTransferObjects(scope, app, graph, dataTransferObjects);
    }
    [Description("Deletes a data transfer object by id.")]
    [WebInvoke(Method = "DELETE", UriTemplate = "/{scope}/{app}/{graph}/{id}")]
    public Response DeletetDataTransferObject(string scope, string app, string graph, string id)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _dxfrProvider.DeleteDataTransferObject(scope, app, graph, id);
    }



  }
}