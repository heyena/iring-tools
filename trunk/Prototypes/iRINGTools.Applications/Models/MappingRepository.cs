using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using Ninject;
using log4net;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;

namespace iRINGTools.Web.Models
{
    public class MappingRepository : IMappingRepository
    {
			  private static readonly ILog _logger = LogManager.GetLogger(typeof(MappingRepository));
        private NameValueCollection _settings = null;
        private WebHttpClient _client = null;
        private string _refDataServiceURI = string.Empty;
        private string application, scope;

        [Inject]
        public MappingRepository()
        {
            _settings = ConfigurationManager.AppSettings;
            _client = new WebHttpClient(_settings["AdapterServiceUri"]);
        }

        public Mapping GetMapping()
        {
            Mapping obj = null;

            try
            {
                obj = _client.Get<Mapping>(String.Format("/{0}/{1}/mapping", scope, application), true);
            }
						catch (Exception ex)
						{
							_logger.Error(ex.ToString());
						}

            return obj;
        }

        public void UpdateMapping(Mapping mapping)
        {
          XElement mappingXml = XElement.Parse(Utility.SerializeDataContract<Mapping>(mapping));
            try
            {
              _client.Post<XElement>(String.Format("/{0}/{1}/mapping", scope, application), mappingXml, true);
            }
						catch (Exception ex)
						{
							_logger.Error(ex.ToString());
						}  
        }

        public void getAppScopeName(string baseUri)
        {
          int index;
          for (int i = 0; i < 3; i++)
          {
            index = baseUri.LastIndexOf('/');

            if (i == 0)
              application = baseUri.Substring(index + 1);
            else if (i == 1)
              scope = baseUri.Substring(index + 1);

            baseUri = baseUri.Substring(0, index);
          }
        }

        public string getScope()
        {
          return scope;
        }

        public string getApplication()
        {
          return application;
        }

        public void setScope(string value)
        {
          this.scope = value;
        }

        public void setApplication(string value)
        {
          this.application = value;
        }
    }
}