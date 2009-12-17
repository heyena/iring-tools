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

  public class Service : IService
  {
    private AdapterProvider _adapterServiceProvider = null;

    /// <summary>
    /// Adapter Service Constructor
    /// </summary>
    public Service()
    {
      _adapterServiceProvider = new AdapterProvider(ConfigurationManager.AppSettings);
    }

    /// <summary>
    /// Gets the mapping by reading Mapping.xml.
    /// </summary>
    /// <returns>Returns mapping object.</returns>
    public Mapping GetMapping(string projectName, string applicationName)
    {
      return _adapterServiceProvider.GetMapping(projectName, applicationName);
    }

    /// <summary>
    /// Gets the Data Dictionary by reading DataDictionary.xml
    /// </summary>
    /// <returns>Returns Data Dictionary object.</returns>
    public DataDictionary GetDictionary(string projectName, string applicationName)
    {
      return _adapterServiceProvider.GetDictionary(projectName, applicationName);
    }

    /// <summary>
    /// Updates mapping.
    /// </summary>
    /// <param name="mapping">The new mapping object with which the mapping file is to be updated.</param>
    /// <returns>Returns the response as success/failure.</returns>
    public Response UpdateMapping(string projectName, string applicationName, Mapping mapping)
    {
      return _adapterServiceProvider.UpdateMapping(projectName, applicationName, mapping);
    }

    /// <summary>
    /// Refreshes Data Dictionary by generating a new DataDictionary.xml from csdl.
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response RefreshDictionary(string projectName, string applicationName)
    {
      return _adapterServiceProvider.RefreshDictionary(projectName, applicationName);
    }

    /// <summary>
    /// Refreshes the triple store for all the graphmaps.
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response RefreshAll(string projectName, string applicationName)
    {
      return _adapterServiceProvider.RefreshAll(projectName, applicationName);
    }

    /// <summary>
    /// Refreshes the triple store for the graphmap passed.
    /// </summary>
    /// <param name="graphName">The name of graph for which triple store will be refreshed.</param>
    /// <returns>Returns the response as success/failure.</returns>
    public Response RefreshGraph(string projectName, string applicationName, string graphName)
    {
      return _adapterServiceProvider.RefreshGraph(projectName, applicationName, graphName);
    }

    /// <summary>
    /// Pulls the data from a triple store into the database.
    /// </summary>
    /// <param name="request">The request parameter containing targetUri, targetCredentials, graphName, filter will be passed.</param>
    /// <returns>Returns the response as success/failure.</returns>
    public Response Pull(string projectName, string applicationName, Request request)
    {
      return _adapterServiceProvider.Pull(projectName, applicationName, request);
    }

    /// <summary>
    /// Pulls the DTO.
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    /// <returns></returns>
    public Response PullDTO(string projectName, string applicationName, Request request)
    {
      return _adapterServiceProvider.PullDTO(projectName, applicationName, request);
    }

    /// <summary>
    /// Generated the IAdapterService (partial) IDataService and ModelDTO using T4.
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response Generate(string projectName, string applicationName)
    {
      return _adapterServiceProvider.Generate(projectName, applicationName);
    }

    /// <summary>
    /// Clears the triple store.
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response ClearStore(string projectName, string applicationName)
    {
      return _adapterServiceProvider.ClearStore(projectName, applicationName);
    }

    /// <summary>
    /// Pass the call to adapter service provider
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response UpdateDatabaseDictionary(DatabaseDictionary dbDictionary, string projectName, string applicationName)
    {
      return _adapterServiceProvider.UpdateDatabaseDictionary(dbDictionary, projectName, applicationName);
    }

    /// <summary>
    /// Gets the data for a graphname and identifier in a QXF format.
    /// </summary>
    /// <param name="graphName">The name of graph for which data is to be fetched.</param>
    /// <param name="identifier">The unique identifier used as filter to return single row's data.</param>
    /// <returns>Returns the data in QXF format.</returns>
    public Envelope Get(string projectName, string applicationName, string graphName, string identifier)
    {
      return _adapterServiceProvider.Get(projectName, applicationName, graphName, identifier);
    }

    /// <summary>
    /// Gets all the data for the graphname.
    /// </summary>
    /// <param name="graphName">The name of graph for which data is to be fetched.</param>
    /// <returns>Returns the data in QXF format.</returns>
    public Envelope GetList(string projectName, string applicationName, string graphName)
    {
      return _adapterServiceProvider.GetList(projectName, applicationName, graphName);
    }
  }
}