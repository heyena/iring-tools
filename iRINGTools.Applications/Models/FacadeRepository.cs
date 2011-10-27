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

namespace org.iringtools.web.Models
{
    public class FacadeRepository : IFacadeRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FacadeRepository));
        private WebHttpClient _client = null;
        private NameValueCollection _settings = null;
        private string _facadeServiceURI = string.Empty;
        private string relativeUri = string.Empty;

        [Inject]
        public FacadeRepository()
        {
            _settings = ConfigurationManager.AppSettings;
            _client = new WebHttpClient(_settings["FacadeServiceUri"]);
        }

        public Response RefreshGraph(string scope, string app, string graph)
        {
            Response resp = null;
            try
            {
                relativeUri = string.Format("/{0}/{1}/{2}/refresh", scope, app, graph);
                resp = _client.Get<Response>(relativeUri);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
            return resp;
        }
    }
}