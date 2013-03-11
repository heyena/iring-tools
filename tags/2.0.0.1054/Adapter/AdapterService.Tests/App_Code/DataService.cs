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

using System.Configuration;
using System.IO;
using org.iringtools.library;
using System.Collections.Generic;

namespace org.iringtools.adapter
{

  public class DataService : IDataService
  {
    //private AdapterProvider _adapterServiceProvider = null;

    ///// <summary>
    ///// Adapter Service Constructor
    ///// </summary>
    //public DataService()
    //{
    //  _adapterServiceProvider = new AdapterProvider(ConfigurationManager.AppSettings);
    //}

    ///// <summary>
    ///// Gets the list of projects by reading Project.xml.
    ///// </summary>
    ///// <returns>Returns a strongly typed list of ScopeProject objects.</returns>
    //public List<ScopeProject> GetScopes()
    //{
    //  return _adapterServiceProvider.GetScopes();
    //}

    //public DTOResponse GetData(DTORequest request)
    //{
    //  DTOResponse response = new DTOResponse();

    //  response.dto = _adapterServiceProvider.GetDTO(request.projectName, request.applicationName, request.graphName, request.identifier);

    //  return response;
    //}

    //public DTOListResponse GetDataList(DTORequest request)
    //{
    //  DTOListResponse response = new DTOListResponse();

    //  response.dtoList = _adapterServiceProvider.GetDTOList(request.projectName, request.applicationName, request.graphName);

    //  return response;
    //}
  }
}