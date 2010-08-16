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

    #region Difference-based Data Exchange
    
    #region GetDxi
    /// <summary>
    /// DXI Resource for Difference-based data exchange.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="graphName">Graph name</param>
    /// <param name="request">DXRequest object containing the manifest to be used.</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("DXI Resource for Difference-based data exchange.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}/dxi")]
    public XElement GetDxi(string projectName, string applicationName, string graphName, DXRequest request)
    {
      return _exchangeProvider.GetDxi(projectName, applicationName, graphName, request);
    }
    #endregion

    #region GetDxo
    /// <summary>
    /// DXO Resource for Difference-based data exchange.
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <param name="applicationName">Application name</param>
    /// <param name="graphName">Graph name</param>
    /// <param name="request">DXRequest object containing the manifest to be used and a list of identifiers to return.</param>
    /// <returns>Returns an arbitrary XML</returns>
    [Description("DXO Resource for Difference-based data exchange.")]
    [WebInvoke(Method = "POST", UriTemplate = "/{projectName}/{applicationName}/{graphName}/dxo")]
    public XElement GetDxo(string projectName, string applicationName, string graphName, DXRequest request)
    {
      return _exchangeProvider.GetDto(projectName, applicationName, graphName, request);
    }
    #endregion
    
    #endregion
  }
}