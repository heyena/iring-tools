// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
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
using System.ServiceModel;
using System.ServiceModel.Web;
using org.iringtools.library;
using System.Xml.Linq;
using System.ComponentModel;
using org.iringtools.library.manifest;

namespace org.iringtools.adapter
{
  [ServiceContract(Namespace = "http://ns.iringtools.org/protocol")]
  public partial interface IService
  {
    #region Public Resources
    [OperationContract]
    [Description("Returns the version of the service.")]
    [WebGet(UriTemplate = "/version")]
    string GetVersion();

    #region iRING Interface
    [OperationContract]
    [Description("Returns the scopes available from the service (project and application combinations).")]
    [WebGet(UriTemplate = "/scopes", ResponseFormat=WebMessageFormat.Json)]
    List<ScopeProject> GetScopes();

    [OperationContract]
    [Description("Returns the iRING manifest for the specified scope.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/manifest")]
    Manifest GetManifest(string projectName, string applicationName);

    [OperationContract]
    [Description("Returns an XML projection of the specified scope, graph and identifier in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}/{identifier}?format={format}")]
    XElement Get(string projectName, string applicationName, string graphName, string identifier, string format);

    [OperationContract]
    [Description("Returns an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}?format={format}")]
    XElement GetList(string projectName, string applicationName, string graphName, string format);

    [OperationContract]
    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}?format={format}")]
    Response PostList(string projectName, string applicationName, string graphName, string format, XElement xml);
    #endregion
    #endregion

    #region Private Resources
    [OperationContract]
    [Description("(Private) Appends scopes to the available scopes and returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/scopes")]
    Response UpdateScopes(List<ScopeProject> scopes);

    [OperationContract]
    [Description("(Private) Returns the Ninject binding configuration for the specified scope (projectName and applicationName).")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/binding")]
    XElement GetBinding(string projectName, string applicationName);

    [OperationContract]
    [Description("(Private) Replaces the Ninject binding configuration for the specified scope and returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/binding")]
    Response UpdateBinding(string projectName, string applicationName, XElement binding);

    [OperationContract]
    [Description("(Private) Returns the dictionary of data objects for the specified scope.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/dictionary")]
    DataDictionary GetDictionary(string projectName, string applicationName);

    [OperationContract]
    [Description("(Private) Returns the iRING mapping for the specified scope.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/mapping")]
    Mapping GetMapping(string projectName, string applicationName);

    [OperationContract]
    [Description("(Private) Replaces the iRING mapping for the specified scope and retuns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/mapping")]
    Response UpdateMapping(string projectName, string applicationName, XElement mappingXml);
    #endregion

    #region Adapter-based Data Exchange
    [OperationContract]
    [Description("Pull Style Adapter-based data exchange. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/pull")]
    Response PullDTO(string projectName, string applicationName, Request request);

    [OperationContract]
    [Description("Push Style Adapter-based data exchange. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/push")]
    Response PushDTO(string projectName, string applicationName, PushRequest request);
    #endregion

    #region Difference-based Data Exchange
    [OperationContract]
    [Description("DXI Resource for Difference-based data exchange.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}/dxi")]
    XElement GetDxi(string projectName, string applicationName, string graphName, DXRequest request);

    [OperationContract]
    [Description("DXO Resource for Difference-based data exchange.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}/dxo")]
    // request must include graphName, manifest, and list of identifiers
    XElement GetDxo(string projectName, string applicationName, string graphName, DXRequest request);
    #endregion

    #region Facade-based Data Exchange (Part 9 Draft)
    [OperationContract]
    [Description("Clear all graphs in the specified scope from the Facade. Returns a response with status.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/delete")]
    Response DeleteAll(string projectName, string applicationName);

    [OperationContract]
    [Description("Clear the specified graph in the scope from the Facade. Returns a response with status.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}/delete")]
    Response DeleteGraph(string projectName, string applicationName, string graphName);

    [OperationContract]
    [Description("Re-publish all graphs in the specified scope to the Facade. Returns a response with status.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/refresh")]
    Response RefreshAll(string projectName, string applicationName);

    [OperationContract]
    [Description("Re-publish the specified graph in the scope to the Facade. Returns a response with status.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}/refresh")]
    Response RefreshGraph(string projectName, string applicationName, string graphName);

    [OperationContract]
    [Description("Pull Style Facade-based data exchange using SPARQL query. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/pull?method=sparql")]
    Response Pull(string projectName, string applicationName, Request request);
    #endregion
  }

  [ServiceContract(Namespace = "http://ns.iringtools.org/protocol")]
  public partial interface IExchangeService
  {
    #region Difference-based Data Exchange
    [OperationContract]
    [Description("DXI Resource for Difference-based data exchange.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}/dxi")]
    XElement GetDxi(string projectName, string applicationName, string graphName, DXRequest request);

    [OperationContract]
    [Description("DXO Resource for Difference-based data exchange.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}/dxo")]
    // request must include graphName, manifest, and list of identifiers
    XElement GetDxo(string projectName, string applicationName, string graphName, DXRequest request);
    #endregion
  }
}
