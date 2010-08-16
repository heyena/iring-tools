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
using System.ServiceModel.Activation;
using org.iringtools.exchange;

namespace org.iringtools.adapter
{
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class AdapterService : IService
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterService));
    private AdapterProvider _adapterProvider = null;
    private ExchangeProvider _exchangeProvider = null;

    /// <summary>
    /// Adapter Service Constructor
    /// </summary>
    public AdapterService()
    {
      _adapterProvider = new AdapterProvider(ConfigurationManager.AppSettings);
      _exchangeProvider = new ExchangeProvider(ConfigurationManager.AppSettings);
    }

    /// <summary>
    /// Gets the VersionInfo from the AssembyInfo
    /// </summary>
    /// <returns>Returns Data Dictionary object.</returns>
    public string GetVersion()
    {
      return _adapterProvider.GetType().Assembly.GetName().Version.ToString();
    }

    /// <summary>
      /// Gets the manifest.
      /// </summary>
      /// <param name="projectName"></param>
      /// <param name="applicationName"></param>
      /// <returns></returns>
    public org.iringtools.library.manifest.Manifest GetManifest(string projectName, string applicationName)
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
    /// Gets the list of projects by reading Project.xml.
    /// </summary>
    /// <returns>Returns a strongly typed list of ScopeProject objects.</returns>
    public XElement GetBinding(string projectName, string applicationName)
    {
      return _adapterProvider.GetBinding(projectName, applicationName);
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
    /// Updates scopes.
    /// </summary>
    /// <param name="scopes">The new scopes object with which the mapping file is to be updated.</param>
    /// <returns>Returns the response as success/failure.</returns>
    public Response UpdateScopes(List<ScopeProject> scopes)
    {
      return _adapterProvider.UpdateScopes(scopes);
    }

    /// <summary>
    /// Pass the call to adapter service provider
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response UpdateBinding(string projectName, string applicationName, XElement binding)
    {
      return _adapterProvider.UpdateBinding(projectName, applicationName, binding);
    }

    /// <summary>
    /// Updates mapping.
    /// </summary>
    /// <param name="mapping">The new mapping object with which the mapping file is to be updated.</param>
    /// <returns>Returns the response as success/failure.</returns>
    public Response UpdateMapping(string projectName, string applicationName, XElement mappingXml)
    {
      return _adapterProvider.UpdateMapping(projectName, applicationName, mappingXml);
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
        return _adapterProvider.GetProjection(projectName, applicationName, graphName, identifier, format);
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
      return _adapterProvider.GetProjection(projectName, applicationName, graphName, format);
    }

    /// <summary>
    /// Pulls the data from a triple store into legacy database
    /// </summary>
    /// <param name="projectName">project name</param>
    /// <param name="applicationName">application name</param>
    /// <param name="graphName">graph name</param>
    /// <param name="request">request containing credentials and uri to pull rdf from</param>
    /// <returns></returns>
    public Response Pull(string projectName, string applicationName, Request request)
    {
      return _adapterProvider.Pull(projectName, applicationName, request);
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
    /// Post data exchange objects to adapter service
    /// </summary>
    /// <param name="projectName">project name</param>
    /// <param name="applicationName">application name</param>
    /// <param name="graphName">graph name</param>
    /// <param name="format">xml/dto/dxo</param>
    /// <param name="xml">dxo xml</param>
    /// <returns>response object</returns>
    public Response PostList(string projectName, string applicationName, string graphName, string format, XElement xml)
    {
      return _adapterProvider.Post(projectName, applicationName, graphName, format, xml);
    }

    /// <summary>
      /// Push the DTO
      /// </summary>
      /// <param name="projectName"></param>
      /// <param name="applicationName"></param>
      /// <param name="request"></param>
      /// <returns></returns>
    public Response PushDTO(string projectName, string applicationName, PushRequest request)
    {
       return _adapterProvider.Push(projectName, applicationName, request);
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
    /// Gets a dictionary of identifiers and hash values.
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    /// <param name="graphName"></param>
    /// <returns></returns>
    public XElement GetDxi(string projectName, string applicationName, string graphName, DXRequest request)
    {
      return _exchangeProvider.GetDxi(projectName, applicationName, graphName, request);
    }

    public XElement GetDxo(string projectName, string applicationName, string graphName, DXRequest request)
    {
      return _exchangeProvider.GetDto(projectName, applicationName, graphName, request);
    }
  }

  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class ExchangeService : IExchangeService
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ExchangeService));
    private ExchangeProvider _exchangeProvider = null;

    /// <summary>
    /// Adapter Service Constructor
    /// </summary>
    public ExchangeService()
    {
      _exchangeProvider = new ExchangeProvider(ConfigurationManager.AppSettings);
    }

    /// <summary>
    /// Gets a dictionary of identifiers and hash values.
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    /// <param name="graphName"></param>
    /// <returns></returns>
    public XElement GetDxi(string projectName, string applicationName, string graphName, DXRequest request)
    {
      return _exchangeProvider.GetDxi(projectName, applicationName, graphName, request);
    }

    public XElement GetDxo(string projectName, string applicationName, string graphName, DXRequest request)
    {
      return _exchangeProvider.GetDto(projectName, applicationName, graphName, request);
    }
  }
}