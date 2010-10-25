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

using System.ComponentModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Xml.Linq;
using log4net;
using org.iringtools.library;
using org.iringtools.exchange;

namespace org.iringtools.services
{
  [ServiceContract(Namespace = "http://ns.iringtools.org/protocol")]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class ExchangeService
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ExchangeService));
    private ExchangeProvider _exchangeProvider = null;

    /// <summary>
    /// Exchange Service Constructor
    /// </summary>
    public ExchangeService()
    {
      _exchangeProvider = new ExchangeProvider(ConfigurationManager.AppSettings);
    }

    #region GetVersion
    /// <summary>
    /// Gets the version of the service.
    /// </summary>
    /// <returns>Returns the version as a string.</returns>
    [Description("Gets the version of the service.")]
    [WebGet(UriTemplate = "/version")]
    public string GetVersion()
    {
      return _exchangeProvider.GetType().Assembly.GetName().Version.ToString();
    }
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
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}/pull")]
    public Response PullDTO(string projectName, string applicationName, string graphName, Request request)
    {
      return _exchangeProvider.PullDTO(projectName, applicationName, graphName, request);
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
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}/push")]
    public Response PushDTO(string projectName, string applicationName, string graphName, PushRequest request)
    {
      return _exchangeProvider.Push(projectName, applicationName, graphName, request);
    }
    #endregion
    #endregion
  }
}