﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using log4net;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;
using System.Net;

namespace iRINGTools.Web.Models
{
    public class MappingRepository : IMappingRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MappingRepository));
        private WebHttpClient _adapterServiceClient = null;
        private string _refDataServiceURI = string.Empty;
        private WebHttpClient _appConfigServiceClient = null;
        public MappingRepository()
        {
          NameValueCollection settings = ConfigurationManager.AppSettings;
          ServiceSettings _settings = new ServiceSettings();
          _settings.AppendSettings(settings);

          #region initialize webHttpClient for converting old mapping
          string proxyHost = _settings["ProxyHost"];
          string proxyPort = _settings["ProxyPort"];
          string adapterServiceUri = _settings["AdapterServiceUri"];
          string applicationConfigServiceUri = _settings["ApplicationConfigServiceUri"];

          if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
          {
            WebProxy webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
            _adapterServiceClient = new WebHttpClient(adapterServiceUri, null, webProxy);
            _appConfigServiceClient = new WebHttpClient(applicationConfigServiceUri, null, webProxy);


          }
          else
          {
            _adapterServiceClient = new WebHttpClient(adapterServiceUri);
            _appConfigServiceClient = new WebHttpClient(applicationConfigServiceUri);


          }
          #endregion
        }

        public Mapping GetMapping(string scopeName, string applicationName)
        {
            Mapping obj = null;

            try
            {
                obj = _adapterServiceClient.Get<Mapping>(String.Format("/{0}/{1}/mapping", scopeName, applicationName), true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }

            return obj;
        }

        public void UpdateMapping(string scopeName, string applicationName, Mapping mapping)
        {
            XElement mappingXml = XElement.Parse(Utility.SerializeDataContract<Mapping>(mapping));
            try
            {
                _adapterServiceClient.Post<XElement>(String.Format("/{0}/{1}/mapping", scopeName, applicationName), mappingXml, true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
        }

        public void UpdateMapping(string scopeName, string applicationName, org.iringtools.applicationConfig.Graph graph, string userName)
        {
           // XElement mappingXml = XElement.Parse(Utility.SerializeDataContract<Mapping>(mapping));
            try
            {
               // _adapterServiceClient.Post<XElement>(String.Format("/{0}/{1}/mapping", scopeName, applicationName), mappingXml, true);
                _appConfigServiceClient.Post<org.iringtools.applicationConfig.Graph>(String.Format("/insertGraph/{0}?format=xml", userName), graph,true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
        }

    }
}