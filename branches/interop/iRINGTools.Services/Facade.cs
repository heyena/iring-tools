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
using org.iringtools.protocol.manifest;
using org.iringtools.adapter;
using org.iringtools.exchange;
using System.Xml;
using System.ServiceModel.Channels;
using System.IO;
using System.Text;
using System;
using org.iringtools.utility;
using org.iringtools.common.mapping;

namespace org.iringtools.services
{
  [ServiceContract(Namespace = "http://ns.iringtools.org/protocol")]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class Facade
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterService));
    private AdapterProvider _adapterProvider = null;
    private ExchangeProvider _exchangeProvider = null;

    /// <summary>
    /// Facade Constructor
    /// </summary>
    public Facade()
    {
      _adapterProvider = new AdapterProvider(ConfigurationManager.AppSettings);
      _exchangeProvider = new ExchangeProvider(ConfigurationManager.AppSettings);
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
    #endregion

    #region Facade-based Data Exchange (Part 9 Draft)
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
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}/pull?method=sparql")]
    public Response Pull(string projectName, string applicationName, string graphName, Request request)
    {
      return _exchangeProvider.Pull(projectName, applicationName, graphName, request);
    }
    #endregion
    #endregion

    #region Private Resources
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
    #endregion
  }
}