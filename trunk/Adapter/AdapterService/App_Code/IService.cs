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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using org.ids_adi.qxf;
using org.iringtools.adapter;
using org.iringtools.library;

namespace org.iringtools.adapter
{
  [ServiceContract(Namespace = "http://ns.iringtools.org/protocol")]
  public partial interface IService
  {
    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/datadictionary")]
    DataDictionary GetDictionary(string projectName, string applicationName);

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/datadictionary/refresh")]
    Response RefreshDictionary(string projectName, string applicationName);

    [OperationContract]
    [WebGet(UriTemplate = "/scopes")]
    List<ScopeProject> GetScopes();

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/manifest")]
    Manifest GetManifest(string projectName, string applicationName);

    [XmlSerializerFormat]
    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/mapping")]
    Mapping GetMapping(string projectName, string applicationName);

    [XmlSerializerFormat]
    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/mapping")]
    Response UpdateMapping(string projectName, string applicationName, Mapping mapping);

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/generate")]
    Response Generate(string projectName, string applicationName);

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/clear")]
    Response ClearStore(string projectName, string applicationName);

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}/refresh")]
    Response RefreshGraph(string projectName, string applicationName, string graphName);

    [OperationContract]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/refresh")]
    Response RefreshAll(string projectName, string applicationName);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/pull")]
    Response Pull(string projectName, string applicationName, Request request);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/pullDTO")]
    Response PullDTO(string projectName, string applicationName, Request request);
    
    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/dbdictionary")]
    Response UpdateDatabaseDictionary(DatabaseDictionary databaseDictionary, string projectName, string applicationName);

    //[OperationContract]
    //[ServiceKnownType("GetKnownTypes", typeof(KnownTypeProvider))]
    //[WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}/{identifier}")]
    //DataTransferObject Get(string projectName, string applicationName, string graphName, string identifier);

    //[OperationContract]
    //[ServiceKnownType("GetKnownTypes", typeof(KnownTypeProvider))]
    //[WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}")]
    //List<DataTransferObject> GetList(string projectName, string applicationName, string graphName);
  } 
}
