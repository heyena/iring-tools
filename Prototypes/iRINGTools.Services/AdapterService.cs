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
using org.iringtools.library.manifest;
using org.iringtools.adapter;

namespace org.iringtools.services
{
  [ServiceContract(Namespace = "http://ns.iringtools.org/protocol")]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class AdapterService
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

    #region Public Resources
    #region GetVersion
    /// <summary>
    /// Gets the version of the service.
    /// </summary>
    /// <returns>Returns the version as a string.</returns>
    [Description("Gets the version of the service.")]
    [WebGet(UriTemplate = "/version")]
    public string GetVersion()
    {
      return _adapterProvider.GetType().Assembly.GetName().Version.ToString();
    }
    #endregion

    #region iRING Interface
    // The iRING Interface is defined by the resources available on the endpoint.
    // This information can be found in the SDK guide as a WADL file.
    #region GetScopes
    /// <summary>
    /// Gets the scopes (project and application combinations) available from the service.
    /// </summary>
    /// <returns>Returns a list of ScopeProject objects.</returns>
    [Description("Gets the scopes (project and application combinations) available from the service.")]
    [WebGet(UriTemplate = "/scopes")]
    public List<ScopeProject> GetScopes()
    {
      return _adapterProvider.GetScopes();
    }
    #endregion

    #region GetManifest
    /// <summary>
    /// Gets the iRING manifest for the specified scope (project and application combination).
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <returns>Returns a Manifest object.</returns>
    [Description("Gets the iRING manifest for the specified scope.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/manifest")]
    public Manifest GetManifest(string projectName, string applicationName)
    {
      return _adapterProvider.GetManifest(projectName, applicationName);
    }
    #endregion

    #region Get
    /// <summary>
    /// Gets an XML projection of the specified scope, graph and identifier in the format (xml, dto, rdf ...) specified.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="graphName">Graph name</param>
    /// <param name="identifier">Identifier</param>
    /// <param name="format">Format to be returned (xml, dto, rdf ...)</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("Gets an XML projection of the specified scope, graph and identifier in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}/{identifier}?format={format}")]
    public XElement Get(string projectName, string applicationName, string graphName, string identifier, string format)
    {
      return _adapterProvider.GetProjection(projectName, applicationName, graphName, identifier, format);
    }
    #endregion

    #region GetList
    /// <summary>
    /// Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="graphName">Graph name</param>
    /// <param name="format">Format to be returned (xml, dto, rdf ...)</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("Gets an XML projection of the specified scope and graph in the format (xml, dto, rdf ...) specified.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}?format={format}")]
    public XElement GetList(string projectName, string applicationName, string graphName, string format)
    {
      return _adapterProvider.GetProjection(projectName, applicationName, graphName, format);
    }
    #endregion

    #region PostList
    /// <summary>
    /// Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="graphName">Graph name</param>
    /// <param name="format">Format to be returned (xml, dto, rdf ...)</param>
    /// <param name="xml">Arbitrary XML</param>
    /// <returns>Returns a Response object</returns>
    [Description("Updates the specified scope and graph with an XML projection in the format (xml, dto, rdf ...) specified. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}?format={format}")]
    public Response PostList(string projectName, string applicationName, string graphName, string format, XElement xml)
    {
      return _adapterProvider.Post(projectName, applicationName, graphName, format, xml);
    }
    #endregion
    #endregion
    #endregion

    #region Private Resources
    #region UpdateScopes
    /// <summary>
    /// Replaces the available scopes.
    /// </summary>
    /// <param name="scopes">The scopes object.</param>
    /// <returns>Returns a Response object.</returns>
    [Description("Replaces the available scopes with the posted scopes.")]
    [WebInvoke(Method = "POST", UriTemplate = "/scopes")]
    public Response UpdateScopes(List<ScopeProject> scopes)
    {
      return _adapterProvider.UpdateScopes(scopes);
    }
    #endregion

    #region GetBinding
    /// <summary>
    /// Gets the Ninject binding configuration for the specified scope.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <returns>Returns an arbitrary XML.</returns>
    [Description("Gets the Ninject binding configuration for the specified scope.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/binding")]
    public XElement GetBinding(string projectName, string applicationName)
    {
      return _adapterProvider.GetBinding(projectName, applicationName);
    }
    #endregion

    #region UpdateBinding
    /// <summary>
    /// Replaces the Ninject binding configuration for the specified scope.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="binding">An arbitrary XML</param>
    /// <returns>Returns a Response object.</returns>
    [Description("Replaces the Ninject binding configuration for the specified scope and returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/binding")]
    public Response UpdateBinding(string projectName, string applicationName, XElement binding)
    {
      return _adapterProvider.UpdateBinding(projectName, applicationName, binding);
    }
    #endregion

    #region GetDictionary
    /// <summary>
    /// Gets the dictionary of data objects for the specified scope.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <returns>Returns a DataDictionary object.</returns>
    [Description("Gets the dictionary of data objects for the specified scope.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/dictionary")]
    public DataDictionary GetDictionary(string projectName, string applicationName)
    {
      return _adapterProvider.GetDictionary(projectName, applicationName);
    }
    #endregion

    #region GetMapping
    /// <summary>
    /// Gets the iRING mapping for the specified scope.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <returns>Returns a Mapping object.</returns>
    [Description("Gets the iRING mapping for the specified scope.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/mapping")]
    public Mapping GetMapping(string projectName, string applicationName)
    {
      return _adapterProvider.GetMapping(projectName, applicationName);
    }
    #endregion

    #region UpdateMapping
    /// <summary>
    /// Replaces the iRING mapping for the specified scope.
    /// </summary>
    /// <param name="mapping">An arbitrary XML</param>
    /// <returns>Returns a Response object.</returns>
    [Description("Replaces the iRING mapping for the specified scope and retuns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/mapping")]
    public Response UpdateMapping(string projectName, string applicationName, XElement mappingXml)
    {
      return _adapterProvider.UpdateMapping(projectName, applicationName, mappingXml);
    }
    #endregion
    #endregion

    #region Adapter-based Data Exchange
    #region PullDTO
    /// <summary>
    /// Pull Style Adapter-based data exchange.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="request">
    /// Request object containing the following: targetUri, targetCredentials, graphName, targetGraphName, filter, projectName, applicationName.
    /// </param>
    /// <returns>Returns a Response object.</returns>
    [Description("Pull Style Adapter-based data exchange. Returns a response with status. Request should include: " +
      "targetUri, targetCredentials, graphName, targetGraphName, filter, projectName, applicationName.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/pull")]
    public Response PullDTO(string projectName, string applicationName, Request request)
    {
      return _adapterProvider.PullDTO(projectName, applicationName, request);
    }
    #endregion

    #region PushDTO
    /// <summary>
    /// Push Style Adapter-based data exchange.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="request">
    /// PushRequest object containing the following: targetUri, targetCredentials, graphName, targetGraphName, filter, projectName, applicationName.
    /// </param>
    /// <returns>Returns a Response object.</returns>
    [Description("Push Style Adapter-based data exchange. Returns a response with status. PushRequest should include: " +
      "targetUri, targetCredentials, graphName, targetGraphName, filter, projectName, applicationName.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/push")]
    public Response PushDTO(string projectName, string applicationName, PushRequest request)
    {
      return _adapterProvider.Push(projectName, applicationName, request);
    }
    #endregion
    #endregion

    #region Facade-based Data Exchange (Part 9 Draft)
    #region DeleteAll
    /// <summary>
    /// Clears all graphs in the specified scope from the Facade.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <returns>Returns a Response object.</returns>
    [Description("Clears all graphs in the specified scope from the Facade. Returns a response with status.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/delete")]
    public Response DeleteAll(string projectName, string applicationName)
    {
      return _adapterProvider.DeleteAll(projectName, applicationName);
    }
    #endregion

    #region DeleteGraph
    /// <summary>
    /// Clears the specified graph in the scope from the Facade.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// /// <param name="graphName">Graph name</param>
    /// <returns>Returns a Response object.</returns>
    [Description("Clear the specified graph in the scope from the Facade. Returns a response with status.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}/delete")]
    public Response DeleteGraph(string projectName, string applicationName, string graphName)
    {
      return _adapterProvider.Delete(projectName, applicationName, graphName);
    }
    #endregion

    #region RefreshAll
    /// <summary>
    /// Re-publish all graphs in the specified scope to the Facade.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <returns>Returns a Response object.</returns>
    [Description("Re-publish all graphs in the specified scope to the Facade. Returns a response with status.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/refresh")]
    public Response RefreshAll(string projectName, string applicationName)
    {
      return _adapterProvider.RefreshAll(projectName, applicationName);
    }
    #endregion

    #region RefreshGraph
    /// <summary>
    /// Re-publish the specified graph in the scope to the Facade.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// /// <param name="graphName">Graph name</param>
    /// <returns>Returns a Response object.</returns>
    [Description("Re-publish the specified graph in the scope to the Facade. Returns a response with status.")]
    [WebGet(UriTemplate = "/{projectName}/{applicationName}/{graphName}/refresh")]
    public Response RefreshGraph(string projectName, string applicationName, string graphName)
    {
      return _adapterProvider.Refresh(projectName, applicationName, graphName);
    }
    #endregion

    #region Pull
    /// <summary>
    /// Pulls the data from a triple store into legacy database
    /// </summary>
    /// <param name="projectName">project name</param>
    /// <param name="applicationName">application name</param>
    /// <param name="graphName">graph name</param>
    /// <param name="request">request containing credentials and uri to pull rdf from</param>
    /// <returns></returns>
    [Description("Pull Style Facade-based data exchange using SPARQL query. Returns a response with status.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/pull?method=sparql")]
    public Response Pull(string projectName, string applicationName, Request request)
    {
      return _adapterProvider.Pull(projectName, applicationName, request);
    }
    #endregion
    #endregion
  }
}