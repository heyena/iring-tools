using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;
using org.iringtools.utility;
using System.Collections.Specialized;
using Ninject;
using System.Configuration;
using log4net;
using iRINGTools.Web.Helpers;
using System.Net;

namespace org.iringtools.web.Models
{
    public class FacadeRepository : IFacadeRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FacadeRepository));
        //private NameValueCollection _settings = null;
        private WebHttpClient _facadeServiceClient = null;
        
        private string relativeUri = string.Empty;

        [Inject]
        public FacadeRepository()
        {
          NameValueCollection settings = ConfigurationManager.AppSettings;
          ServiceSettings _settings = new ServiceSettings();
          _settings.AppendSettings(settings);

          #region initialize webHttpClient for converting old mapping
          string proxyHost = _settings["ProxyHost"];
          string proxyPort = _settings["ProxyPort"];
          string facadeServiceUri = _settings["FacadeServiceUri"];
           
          if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
          {
            WebProxy webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
            _facadeServiceClient = new WebHttpClient(facadeServiceUri, null, webProxy);              
          }
          else
          {
            _facadeServiceClient = new WebHttpClient(facadeServiceUri);
            
          }
          #endregion
        }

        public Response RefreshGraph(string scope, string app, string graph)
        {
            Response resp = null;
            try
            {
                relativeUri = string.Format("/{0}/{1}/{2}/refresh", scope, app, graph);
                resp = _facadeServiceClient.Get<Response>(relativeUri);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
            return resp;
        }
    }
}