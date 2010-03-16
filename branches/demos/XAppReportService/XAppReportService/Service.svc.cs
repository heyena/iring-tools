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
    private List<string> _templateMapNames = null;
    private string _classPath = String.Empty;
    private string _dtoTemplatePath = String.Empty;
    private Dictionary<string, string> _dtoPropertiesOimPathNamePairs = null;
    private XAppReport _reportConfig = null;

    public Service()
    {
      // Load report configuration xml
      _reportConfig = Utility.Read<XAppReport>(_dataPath + "XAppReportConfig.xml", false);

      // Load encrypted tokens to a diciontary of adapterUri and tokens
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
        //TEST: return Utility.Read<Report>(_dataPath + "TestReport.xml", false);

        foreach (Report report in _reportConfig.reports)
        {
          if (report.name.ToLower() == name.ToLower())
          {
            foreach (Company company in report.companies)
            {
              FillDtoPropertiesInfo(company.dtoUrl, company.dtoProperties);
            }

            //TEST: Utility.Write<Report>(report, _dataPath + "TestReport.xml", false);
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

    private void FillDtoPropertiesInfo(string dtoUrl, List<DTOProperty> filteredDtoProperties)
    {
      // Extract graph name from dto url
      int index = dtoUrl.LastIndexOf("/");
      string appUri = dtoUrl.Substring(0, index);
      string graphName = dtoUrl.Substring(index + 1);

      // Extract adapter uri from dto url
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

      // Find dto properties OIM paths
      FindDtoPropertiesOimPaths(appUri, graphName, credentialToken);

      // Fill OIM paths and values to filteredDtoProperties
      if (_dtoPropertiesOimPathNamePairs != null && _dtoPropertiesOimPathNamePairs.Count > 0)
      {
        // Get dto properties values
        Dictionary<string, List<string>> dtoPropertiesOimPathValuesPairs = GetDtoPropertiesOimPathValuesPairs(dtoUrl, credentialToken);

        foreach (DTOProperty dtoProperty in filteredDtoProperties)
        {
          string dtoPropertyName = dtoProperty.name.ToUpper();

          if (_dtoPropertiesOimPathNamePairs.ContainsValue(dtoPropertyName))
          {
            string dtoPropertyOimPath = 
              (from oimPathNamePair in _dtoPropertiesOimPathNamePairs
               where oimPathNamePair.Value == dtoPropertyName
               select oimPathNamePair.Key).First();

            dtoProperty.name = dtoPropertyName; // upper case it
            dtoProperty.values = dtoPropertiesOimPathValuesPairs[dtoPropertyOimPath];
          }
        }        
      }
    }

    private void FindDtoPropertiesOimPaths(string appUri, string graphName, string credentialToken)
    {
      // Get mapping file
      //TEST: Mapping mapping = Utility.Read<Mapping>(_dataPath + "Mapping.12345_000.Inspec.xml", false);
      string mappingXml = sendHttpRequest(appUri + "/mapping", credentialToken);
      Mapping mapping = Utility.Deserialize<Mapping>(mappingXml, false);

      foreach (GraphMap graphMap in mapping.graphMaps)
      {
        if (graphMap.name.ToLower() == graphName.ToLower())
        {
          _dtoPropertiesOimPathNamePairs = new Dictionary<string, string>();
          _templateMapNames = new List<string>();

          // Compute dto property paths
          foreach (TemplateMap templateMap in graphMap.templateMaps)
          {
            _classPath = String.Empty;
            _dtoTemplatePath = String.Empty;

            ProcessTemplateMap(templateMap);
          }

          break;
        }
      }
    }

    private Dictionary<string, List<string>> GetDtoPropertiesOimPathValuesPairs(string dtoUrl, string credentialToken)
    {
      Dictionary<string, List<string>> dtoPropertiesOimPathValuesPairs = new Dictionary<string, List<string>>();

      //TEST: string dtoListXml = Utility.ReadString(_dataPath + "DTO.12345_000.Inspec.LineList.xml");
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
          string oimPath = property.Attribute("name").Value;

          if (_dtoPropertiesOimPathNamePairs.ContainsKey(oimPath))
          {
            string propertyValue = property.Attribute("value").Value;

            if (dtoPropertiesOimPathValuesPairs.ContainsKey(oimPath))
            {
              dtoPropertiesOimPathValuesPairs[oimPath].Add(propertyValue);
            }
            else
            {
              List<string> values = new List<string>() { propertyValue };
              dtoPropertiesOimPathValuesPairs.Add(oimPath, values);
            }
          }

          property = (XElement)property.NextNode;
        }

        dto = (XElement)dto.NextNode;
      }

      return dtoPropertiesOimPathValuesPairs;
    }

    private void ProcessTemplateMap(TemplateMap templateMap)
    {
      templateMap.name = Utility.NameSafe(templateMap.name) + _templateMapNames.Count;
      _templateMapNames.Add(templateMap.name);

      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        roleMap.name = Utility.NameSafe(roleMap.name);

        if (templateMap.type == TemplateType.Property)
        {
          processRoleMap(templateMap.name, roleMap);
        }
        else if (templateMap.type == TemplateType.Relationship)
        {
          if (roleMap.classMap == null)
          {
            processRoleMap(templateMap.name, roleMap);
          }
          else if (roleMap.classMap.templateMaps != null && roleMap.classMap.templateMaps.Count > 0)
          {
            roleMap.classMap.name = Utility.NameSafe(roleMap.classMap.name);

            if (_classPath == String.Empty)
            {
              _classPath = "Template" + templateMap.name;
              _dtoTemplatePath = "tpl:" + templateMap.name;
            }
            else
            {
              _classPath += ".Template" + templateMap.name;
              _dtoTemplatePath += ".tpl:" + templateMap.name;
            }

            processClassMap(roleMap);
          }
        }
      }
    }

    private void processRoleMap(string templateName, RoleMap roleMap)
    {
      if (!String.IsNullOrEmpty(roleMap.propertyName))
      {
        string dtoPropertyPath = "tpl:" + templateName + ".tpl:" + roleMap.name;
        if (_dtoTemplatePath != String.Empty)
        {
          dtoPropertyPath = _dtoTemplatePath + "." + dtoPropertyPath;
        }

        _dtoPropertiesOimPathNamePairs.Add(dtoPropertyPath, roleMap.propertyName.ToUpper());
      }
    }

    private void processClassMap(RoleMap roleMap)
    {
      _classPath += ".Class" + roleMap.classMap.name;
      _dtoTemplatePath += ".tpl:" + roleMap.name + ".rdl:" + roleMap.classMap.name;

      string lastClassMapPath = _classPath;
      string lastDtoTemplateMapPath = _dtoTemplatePath;

      foreach (TemplateMap templateMap in roleMap.classMap.templateMaps)
      {
        _classPath = lastClassMapPath;
        _dtoTemplatePath = lastDtoTemplateMapPath;

        ProcessTemplateMap(templateMap);
      }
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
