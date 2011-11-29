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

    [Inject]
    public MappingRepository()
    {
      _settings = ConfigurationManager.AppSettings;
      _client = new WebHttpClient(_settings["AdapterServiceUri"]);
    }

    public Mapping GetMapping(string context, string endpoint)
    {
      Mapping obj = null;

      try
      {
        obj = _client.Get<Mapping>(String.Format("/{0}/{1}/mapping", context, endpoint), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public void UpdateMapping(Mapping mapping, string context, string endpoint)
    {
      XElement mappingXml = XElement.Parse(Utility.SerializeDataContract<Mapping>(mapping));
      try
      {
        _client.Post<XElement>(String.Format("/{0}/{1}/mapping", context, endpoint), mappingXml, true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }
    }
  }
}