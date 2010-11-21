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
using org.iringtools.adapter;
using org.iringtools.exchange;
using org.iringtools.dxfr.manifest;

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

    [Description("Gets data transfer objects for a particular graph.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}?hashAlgorithm={hashAlgorithm}")]
    public DataTransferIndices GetDataTransferIndices(string scope, string app, string graph, string hashAlgorithm)
    {
      return _dxfrProvider.GetDataTransferIndices(scope, app, graph, hashAlgorithm);
    }

    [Description("Gets data transfer objects according to dti request.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/page")]
    public DataTransferObjects GetDataTransferObjects(string scope, string app, string graph, DataTransferIndices dataTransferIndices)
    {
      return _dxfrProvider.GetDataTransferObjects(scope, app, graph, dataTransferIndices);
    }

    [Description("Gets data transfer objects.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}")]
    public DataTransferObjects GetDataTransferObjectsFull(string scope, string app, string graph)
    {
      return _dxfrProvider.GetDataTransferObjects(scope, app, graph);
    }

    [Description("Posts data transfer objects to add/update/delete.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}")]
    public Response PostDataTransferObjects(string scope, string app, string graph, DataTransferObjects dataTransferObjects)
    {
      return _dxfrProvider.PostDataTransferObjects(scope, app, graph, dataTransferObjects);
    }

    [Description("Gets single data transfer object by id.")]
    [WebGet(UriTemplate = "/{scope}/{app}/{graph}/{id}")]
    public DataTransferObjects GetDataTransferObject(string scope, string app, string graph, string id)
    {
      return _dxfrProvider.GetDataTransferObject(scope, app, graph, id);
    }

    [Description("Deletes a data transfer object by id.")]
    [WebInvoke(Method = "DELETE", UriTemplate = "/{scope}/{app}/{graph}/{id}")]
    public Response DeletetDataTransferObject(string scope, string app, string graph, string id)
    {
      return _dxfrProvider.DeleteDataTransferObject(scope, app, graph, id);
    }

    [Description("Gets data transfer indices according to manifest request.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/xfr?hashAlgorithm={hashAlgorithm}")]
    public DataTransferIndices GetDataTransferIndicesWithManifest(string scope, string app, string graph, string hashAlgorithm, Manifest manifest)
    {
      return _dxfrProvider.GetDataTransferIndicesWithManifest(scope, app, graph, hashAlgorithm, manifest);
    }

    [Description("Gets data transfer objects according to manifest and dti request.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{app}/{graph}/xfr/page")]
    public DataTransferObjects GetDataTransferObjectsWithManifest(string scope, string app, string graph, DtoPageRequest dtoPageRequest)
    {
      return _dxfrProvider.GetDataTransferObjects(scope, app, graph, dtoPageRequest);
    }

    [Description("Gets manifest for an application.")]
    [WebGet(UriTemplate = "/{scope}/{app}/manifest")]
    public Manifest GetManifest(string scope, string app)
    {
      return _dxfrProvider.GetManifest(scope, app);
    }

    [Description("Gets dto provider version.")]
    [WebGet(UriTemplate = "/version")]
    public VersionInfo GetVersion()
    {
      return _dxfrProvider.GetVersion();
    }
  }
}