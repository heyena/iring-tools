﻿// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
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
  public partial interface IAdapterService
  {
    [OperationContract]
    [WebGet(UriTemplate = "/dictionary")]
    DataDictionary GetDictionary();

    [OperationContract]
    [WebGet(UriTemplate = "/dictionary/refresh")]
    Response RefreshDictionary();

    [XmlSerializerFormat]
    [OperationContract]
    [WebGet(UriTemplate = "/mapping")]
    Mapping GetMapping();

    [XmlSerializerFormat]
    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/mapping")]
    Response UpdateMapping(Mapping mapping);

    [OperationContract]
    [WebGet(UriTemplate = "/generate")]
    Response Generate();

    [OperationContract]
    [WebGet(UriTemplate = "/clear")]
    Response ClearStore();

    [OperationContract]
    [WebGet(UriTemplate = "/{graphName}/refresh")]
    Response RefreshGraph(string graphName);

    [OperationContract]
    [WebGet(UriTemplate = "/refresh")]
    Response RefreshAll();

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "/pull")]
    Response Pull(Request request);
  } 
}
