using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Net;
using System.IO;
using org.iringtools.utility;
using org.iringtools.library;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.ServiceModel.Web;
using System.Web.Script.Serialization;

namespace XAppReportService
{
  public class Service : IService
  {
    private string _dataPath = System.AppDomain.CurrentDomain.BaseDirectory + "App_Data\\";
    private Dictionary<string, string> _encryptedTokens = null;
    private List<string> _templateMapNames = new List<string>();
    private string _classPath = String.Empty;
    private string _dtoTemplatePath = String.Empty;
    private XAppReport _reportConfig = null;

    public Service()
    {
      // Load report configuration xml
      _reportConfig = Utility.Read<XAppReport>(_dataPath + "XAppReportConfig.xml", false);

      // Load encrypted tokens to a dictiontary of adapterUri and tokens
      string configXml = Utility.ReadString(_dataPath + "ConfiguredEndpoints.xml");
      configXml = Utility.Transform(configXml, _dataPath + "RemoveNamespace.xsl", null);
      _encryptedTokens = new Dictionary<string, string>();
      
      XDocument document = XDocument.Parse(configXml);
      IEnumerable<XElement> iRINGEndpoints =
        document.Element("ConfiguredEndpoints").Element("adapterEndpoints").Elements("iRINGEndpoint");

      foreach (var iRINGEndpoint in iRINGEndpoints)
      {
        string serviceUri = iRINGEndpoint.Element("serviceUri").Value;

        XElement encryptedTokenElement = iRINGEndpoint.Element("credentials").Element("encryptedToken");
        string encryptedToken = (encryptedTokenElement != null) ? encryptedTokenElement.Value : String.Empty;

        _encryptedTokens.Add(serviceUri.ToLower(), encryptedToken);
      }
    }

    public List<string> GetReportNames()
    {
      try {
        List<string> reportNames = new List<string>();

        foreach (Report report in _reportConfig.reports)
        {
          reportNames.Add(report.name);
        }

        return reportNames;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public Report GetReport(string name)
    {
      try
      {
        foreach (Report report in _reportConfig.reports)
        {
          if (report.name.ToLower() == name.ToLower())
          {
            foreach (Company company in report.companies)
            {
              FillDtoPropertiesValues(company.dtoUrl, company.dtoProperties);
            }
            return report;
          }
        }

        return null;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private void FillDtoPropertiesValues(string dtoUrl, List<DTOProperty> configDtoProperties)
    {
      // Extract adapter uri from dto url
      int index = dtoUrl.LastIndexOf("/");
      string appUri = dtoUrl.Substring(0, index);
      index = appUri.LastIndexOf("/");
      string projUri = appUri.Substring(0, index);
      index = projUri.LastIndexOf("/");
      string adapterUri = projUri.Substring(0, index);

      // Get credential token for the adapter uri
      string credentialToken = String.Empty;
      if (_encryptedTokens.ContainsKey(adapterUri.ToLower()))
      {
        credentialToken = _encryptedTokens[adapterUri.ToLower()];
      }
            
      // Get dto properties values
      Dictionary<string, List<string>> xPathValuesPairs = GetXPathValuesPairs(dtoUrl, credentialToken);

      foreach (DTOProperty dtoProperty in configDtoProperties)
      {
        if (!String.IsNullOrEmpty(dtoProperty.xPath))
        {
          string xpath = dtoProperty.xPath.Replace(".tpl", @"\d+\.tpl");
          foreach (var pair in xPathValuesPairs)
          {
            if (Regex.IsMatch(pair.Key, @xpath, RegexOptions.IgnoreCase))
            {
              dtoProperty.values = pair.Value;
            }
          }
        }
      } 
    }

    private Dictionary<string, List<string>> GetXPathValuesPairs(string dtoUrl, string credentialToken)
    {
      Dictionary<string, List<string>> xPathValuesPairs = new Dictionary<string, List<string>>();
      string dtoListXml = sendHttpRequest(dtoUrl, credentialToken);
      dtoListXml = Utility.Transform(dtoListXml, _dataPath + "RemoveNamespace.xsl", null);
      XDocument document = XDocument.Parse(dtoListXml);
      XElement payload = document.Element("Envelope").Element("Payload");

      XElement dto = (XElement)payload.FirstNode;
      while (dto != null)
      {
        XElement properties = dto.Element("Properties");
        XElement property = (XElement)properties.FirstNode;

        while (property != null)
        {
          string xPath = property.Attribute("name").Value;
          string propertyValue = property.Attribute("value").Value;

          if (xPathValuesPairs.ContainsKey(xPath))
          {
            xPathValuesPairs[xPath].Add(propertyValue);
          }
          else
          {
            List<string> values = new List<string>() { propertyValue };
            xPathValuesPairs.Add(xPath, values);
          }

          property = (XElement)property.NextNode;
        }

        dto = (XElement)dto.NextNode;
      }

      return xPathValuesPairs;
    }

    private string sendHttpRequest(string url, string credentialToken)
    {
      try
      {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

        if (credentialToken != String.Empty)
        {
          WebCredentials credentials = new WebCredentials(credentialToken);
          if (credentials.isEncrypted) credentials.Decrypt();
          request.Credentials = credentials.GetNetworkCredential();
        }

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        Stream stream = response.GetResponseStream();
        StreamReader reader = new StreamReader(stream);

        return reader.ReadToEnd();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }
  }
}
