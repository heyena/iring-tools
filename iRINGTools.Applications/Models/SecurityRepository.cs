using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using iRINGTools.Web.Helpers;
using log4net;
using Microsoft.ServiceModel.Web;
using org.iringtools.library;
using org.iringtools.mapping;
using org.iringtools.utility;
using org.iringtools.UserSecurity;
namespace iRINGTools.Web.Models
{

    public class SecurityRepository : IsecurityRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SecurityRepository));
        private CustomError _CustomError = null;
        protected ServiceSettings _settings;
        protected string _proxyHost;
        protected string _proxyPort;
        protected string _adapterServiceUri = null;
        protected string _dataServiceUri = null;
        protected string _hibernateServiceUri = null;
        protected string _referenceDataServiceUri = null;
        protected string _servicesBasePath = string.Empty;

        public SecurityRepository()
        {
            NameValueCollection settings = ConfigurationManager.AppSettings;

            _settings = new ServiceSettings();
            _settings.AppendSettings(settings);

            _proxyHost = _settings["ProxyHost"];
            _proxyPort = _settings["ProxyPort"];

            _adapterServiceUri = _settings["SecurityUri"];
            if (_adapterServiceUri.EndsWith("/"))
                _adapterServiceUri = _adapterServiceUri.Remove(_adapterServiceUri.Length - 1);


        }
        protected WebHttpClient CreateWebClient(string baseUri)
        {
            WebHttpClient client = null;
            client = new WebHttpClient(baseUri);
            return client;
        }
        public Users GetAllUsers(string format)
        {
            Users items = null;
            _logger.Debug("In AdapterRepository GetScopes");
            try
            {
                WebHttpClient client = CreateWebClient(_adapterServiceUri);
                items = client.Get<Users>("/users?format=?" + format);  ///users?format={format}

                _logger.Debug("Successfully called Security Service.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;

            }
            return items;

        }

       

    }



}
