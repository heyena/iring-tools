using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Xml.Linq;
using Ninject;
using log4net;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Storage;
using org.iringtools.utility;
using Microsoft.ServiceModel.Web;
using System.Xml.Serialization;

namespace org.iringtools.adapter.projection
{
  public class DtoProjectionEngine : IProjectionLayer
  {
    private static readonly string DATALAYER_NS = "org.iringtools.adapter.datalayer";
    
    private static readonly XNamespace RDF_NS = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace OWL_NS = "http://www.w3.org/2002/07/owl#";
    private static readonly XNamespace XSD_NS = "http://www.w3.org/2001/XMLSchema#";
    private static readonly XNamespace XSI_NS = "http://www.w3.org/2001/XMLSchema-instance#";
    private static readonly XNamespace TPL_NS = "http://tpl.rdlfacade.org/data#";
    private static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";

    private static readonly XName OWL_THING = OWL_NS + "Thing";
    private static readonly XName RDF_ABOUT = RDF_NS + "about";
    private static readonly XName RDF_DESCRIPTION = RDF_NS + "Description";
    private static readonly XName RDF_TYPE = RDF_NS + "type";
    private static readonly XName RDF_RESOURCE = RDF_NS + "resource";
    private static readonly XName RDF_DATATYPE = RDF_NS + "datatype";

    private static readonly string RDF_PREFIX = "rdf:";
    private static readonly string RDF_NIL = RDF_PREFIX + "nil";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(DtoProjectionEngine));

    private IDataLayer _dataLayer = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private DataDictionary _dataDictionary = null;
    private IList<IDataObject> _dataObjects = null;
    private Dictionary<string, List<string>> _classIdentifiers = null; // dictionary of class ids and list of identifiers
    private List<Dictionary<string, string>> _xPathValuePairs = null;  // dictionary of property xpath and value pairs
    private Dictionary<string, List<string>> _hierachicalDTOClasses = null;  // dictionary of class rdlUri and identifiers
    private XNamespace _graphNs = String.Empty;
    private string _dataObjectsAssemblyName = String.Empty;
    private string _dataObjectNs = String.Empty;

    [Inject]
    public DtoProjectionEngine(AdapterSettings adapterSettings, ApplicationSettings appSettings, IDataLayer dataLayer)
    {
      string scope = appSettings.ProjectName + "{0}" + appSettings.ApplicationName;

      _dataObjects = new List<IDataObject>();
      _classIdentifiers = new Dictionary<string, List<string>>();
      _xPathValuePairs = new List<Dictionary<string, string>>();
      _hierachicalDTOClasses = new Dictionary<string, List<string>>();

      _dataLayer = dataLayer;
      _mapping = Utility.Read<Mapping>(String.Format(adapterSettings.XmlPath + "Mapping." + scope + ".xml", "."));
      _graphNs = String.Format(adapterSettings.GraphBaseUri + scope + "#", "/");
      _dataObjectNs = String.Format(DATALAYER_NS + ".proj_" + scope, ".");
      _dataObjectsAssemblyName = adapterSettings.ExecutingAssemblyName;
    }

    public XElement GetXml(ref GraphMap graphMap, ref DataDictionary dataDictionary, ref IList<IDataObject> dataObjects)
    {
      try
      {
        _graphMap = graphMap;
        _dataDictionary = dataDictionary;
        _dataObjects = dataObjects;

        _xPathValuePairs.Clear();
        for (int i = 0; i < _dataObjects.Count; i++)
        {
          _xPathValuePairs.Add(new Dictionary<string, string>());
        }

        ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
        FillDTOList(classMap.classId, "rdl:" + classMap.name);

        return SerializationExtensions.ToXml<List<Dictionary<string, string>>>(_xPathValuePairs);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public IList<IDataObject> GetDataObjects(ref GraphMap graphMap, ref DataDictionary dataDictionary, ref XElement xml)
    {
      throw new NotImplementedException();
    }

    #region helper methods
    private string ExtractId(string qualifiedId)
    {
      if (String.IsNullOrEmpty(qualifiedId) || !qualifiedId.Contains(":"))
        return qualifiedId;

      return qualifiedId.Substring(qualifiedId.IndexOf(":") + 1);
    }

    private string TitleCase(string value)
    {
      string returnValue = String.Empty;
      string[] words = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

      foreach (string word in words)
      {
        returnValue += word.Substring(0, 1).ToUpper();

        if (word.Length > 1)
          returnValue += word.Substring(1).ToLower();
      }

      return returnValue;
    }

    private void FillDTOList(string classId, string xPath)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
      string classPath = xPath;

      foreach (TemplateMap templateMap in classTemplateListMap.Value)
      {
        xPath = classPath + "/tpl:" + templateMap.name;
        string templatePath = xPath;

        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          if (roleMap.type == RoleType.Property)
          {
            xPath += "/tpl:" + roleMap.name;

            string[] property = roleMap.propertyName.Split('.');
            string objectName = property[0].Trim();
            string propertyName = property[1].Trim();
            string value = String.Empty;

            for (int i = 0; i < _dataObjects.Count; i++)
            {
              value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

              if (!String.IsNullOrEmpty(roleMap.valueList))
              {
                value = _mapping.ResolveValueList(roleMap.valueList, value);
              }

              Dictionary<string, string> propertyValuePair = _xPathValuePairs[i];
              propertyValuePair[xPath] = value;
            }

            xPath = templatePath;
          }

          if (roleMap.classMap != null)
          {
            FillDTOList(roleMap.classMap.classId, xPath + "/rdl:" + roleMap.classMap.name);
          }
        }
      }
    }

    #endregion
  }
}
