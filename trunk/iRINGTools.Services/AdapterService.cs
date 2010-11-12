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
  public class AdapterService
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

    #region GetDataLayers
    [Description("Get a list of Data Layers available from the service.")]
    [WebGet(UriTemplate = "/datalayers")]
    public List<string> GetDatalayers()
    {
      return _adapterProvider.GetDataLayers();
    }
    #endregion
    #endregion
  }
}