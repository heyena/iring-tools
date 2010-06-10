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
using log4net;
using System.ServiceModel.Channels;
using System;
using System.ServiceModel;
using System.Net;
using System.Xml.Linq;
using System.Reflection;

namespace org.iringtools.adapter
{
  public class AdapterService : IService
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterService));
    private AdapterProvider _adapterProvider = null;

    /// <summary>
    /// Adapter Service Constructor
    /// </summary>
    public AdapterService()
    {
      _adapterProvider = new AdapterProvider(ConfigurationManager.AppSettings);
    }

    /// <summary>
    /// Gets the VersionInfo from the AssembyInfo
    /// </summary>
    /// <returns>Returns Data Dictionary object.</returns>
    public string GetVersion()
    {
      return _adapterProvider.GetType().Assembly.GetName().Version.ToString();
    }

    public Manifest GetManifest(string projectName, string applicationName)
    {
      return _adapterProvider.GetManifest(projectName, applicationName);
    }

    /// <summary>
    /// Gets the mapping by reading Mapping.xml.
    /// </summary>
    /// <returns>Returns mapping object.</returns>
    public Mapping GetMapping(string projectName, string applicationName)
    {
      return _adapterProvider.GetMapping(projectName, applicationName);
    }

    /// <summary>
    /// Gets the list of projects by reading Project.xml.
    /// </summary>
    /// <returns>Returns a strongly typed list of ScopeProject objects.</returns>
    public List<ScopeProject> GetScopes()
    {
      return _adapterProvider.GetScopes();
    }

    /// <summary>
    /// Gets the Data Dictionary by reading DataDictionary.xml
    /// </summary>
    /// <returns>Returns Data Dictionary object.</returns>
    public DataDictionary GetDictionary(string projectName, string applicationName)
    {
      return _adapterProvider.GetDictionary(projectName, applicationName);
    }

    /// <summary>
    /// Updates mapping.
    /// </summary>
    /// <param name="mapping">The new mapping object with which the mapping file is to be updated.</param>
    /// <returns>Returns the response as success/failure.</returns>
    public Response UpdateMapping(string projectName, string applicationName, Mapping mapping)
    {
      return _adapterProvider.UpdateMapping(projectName, applicationName, mapping);
    }

    /// <summary>
    /// Refreshes the triple store for all the graphmaps.
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response RefreshAll(string projectName, string applicationName)
    {
      return _adapterProvider.RefreshAll(projectName, applicationName);
    }

    /// <summary>
    /// Refreshes the triple store for the graphmap passed.
    /// </summary>
    /// <param name="graphName">The name of graph for which triple store will be refreshed.</param>
    /// <returns>Returns the response as success/failure.</returns>
    public Response RefreshGraph(string projectName, string applicationName, string graphName)
    {
      return _adapterProvider.Refresh(projectName, applicationName, graphName);
    }

      /// <summary>
      /// Calls adapter provider to get a specific data object in specific format
      /// </summary>
      /// <param name="projectName"></param>
      /// <param name="applicationName"></param>
      /// <param name="graphName"></param>
      /// <param name="identifier"></param>
      /// <param name="format"></param>
      /// <returns></returns>
    public XElement Get(string projectName, string applicationName, string graphName, string identifier, string format)
    {
        return _adapterProvider.GetProjection(projectName, applicationName, graphName, identifier, format.ToLower());
    }

    /// <summary>
    /// Calls adapter provider to produce specific format for a graph
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    /// <param name="graphName"></param>
    /// <returns>xelement</returns>
    public XElement GetList(string projectName, string applicationName, string graphName, string format)
    {
      return _adapterProvider.GetProjection(projectName, applicationName, graphName, format.ToLower());
    }

    /// <summary>
    /// Pulls the data from a triple store into legacy database
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    /// <param name="graphName"></param>
    /// <returns></returns>
    public Response Pull(string projectName, string applicationName, string graphName)
    {
      return _adapterProvider.Pull(projectName, applicationName, graphName);
    }

    /// <summary>
    /// Pulls the DTO.
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    /// <returns></returns>
    public Response PullDTO(string projectName, string applicationName, Request request)
    {
        return _adapterProvider.PullDTO(projectName, applicationName, request);
    }

    /// <summary>
    /// Puts the DTO.
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    /// <param name="graphName"></param>
    /// <param name="dtoElement"></param>
    /// <returns></returns>
    public Response Put(string projectName, string applicationName, string graphName, XElement dtoElement)
    {
      //return _adapterProvider.Put(projectName, applicationName, graphName, dtoElement);
      return null;
    }

    /// <summary>
    /// Clears the triple store.
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response DeleteAll(string projectName, string applicationName)
    {
      return _adapterProvider.DeleteAll(projectName, applicationName);
    }

    /// <summary>
    /// Clears a triple store graph.
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response DeleteGraph(string projectName, string applicationName, string graphName)
    {
      return _adapterProvider.Delete(projectName, applicationName, graphName);
    }

    /// <summary>
    /// Pass the call to adapter service provider
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response UpdateDatabaseDictionary(string projectName, string applicationName, DatabaseDictionary dbDictionary)
    {
      return _adapterProvider.UpdateDatabaseDictionary(projectName, applicationName, dbDictionary);
    }
  }
}