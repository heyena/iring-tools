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

namespace org.iringtools.adapter
{
  [ServiceContract(Namespace = "http://ns.iringtools.org/protocol")]
  public partial interface IService
  {
    #region Basic Stuff
    [OperationContract]
    [WebGet(UriTemplate = "/version")]
    string GetVersion();

    [OperationContract]
    [WebGet(UriTemplate = "/scopes")]
    List<ScopeProject> GetScopes();

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/scopes")]
    Response UpdateScopes(List<ScopeProject> scopes);

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/binding")]
    XElement GetBinding(string projectName, string applicationName);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/binding")]
    Response UpdateBinding(string projectName, string applicationName, XElement binding);

    #endregion

    #region metadata

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/manifest")]
    Manifest GetManifest(string projectName, string applicationName);

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/datadictionary")]
    DataDictionary GetDictionary(string projectName, string applicationName);

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/mapping")]
    Mapping GetMapping(string projectName, string applicationName);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/mapping")]
    Response UpdateMapping(string projectName, string applicationName, XElement mappingXml);

    #endregion

    #region Xml Get
    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}/{identifier}?format={format}")]
    XElement Get(string projectName, string applicationName, string graphName, string identifier, string format);

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{alias}/{classId}/{identifier}?graph={graphName}&format={format}")]
    XElement GetIndividual(string projectName, string applicationName, string alias, string classId, string identifier, string graphName, string format);

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}?format={format}")]
    XElement GetList(string projectName, string applicationName, string graphName, string format);
    #endregion

    //Xml Put/Post
    [OperationContract]
    [WebInvoke(Method = "PUT", UriTemplate = "/{projectName}/{applicationName}/{graphName}/?format={format}")]
    Response Put(string projectName, string applicationName, string graphName, string format, XElement xml);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}/?format={format}")]
    Response Post(string projectName, string applicationName, string graphName, string format, XElement xml);

    //Xml Delete
    //Receive identifiers and delete from DataLayer
    //[OperationContract]
    //[WebInvoke(Method = "DELETE", UriTemplate = "/{projectName}/{applicationName}/{graphName}/?format={format}")]
    //Response Delete(string projectName, string applicationName, string graphName, string format, XElement xml);

    #region Xml DataExchange
    //Xml Pull style DataExchange
    //Calls Get on another endpoint and posts to DataLayer
    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/pullDTO")]
    Response PullDTO(string projectName, string applicationName, Request request);

    //Xml Push style DataExchange
    //Calls Get from DataLayer and post to on another endpoint
    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/pushDTO")]
    Response PushDTO(string projectName, string applicationName, Request request);
    #endregion

    #region TripleStore
    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/delete")]
    Response DeleteAll(string projectName, string applicationName);

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}/delete")]
    Response DeleteGraph(string projectName, string applicationName, string graphName);

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/refresh")]
    Response RefreshAll(string projectName, string applicationName);

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}/refresh")]
    Response RefreshGraph(string projectName, string applicationName, string graphName);

    //SPARQL Query
    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/pull")]
    Response Pull(string projectName, string applicationName, Request request);
    #endregion
  }
}
