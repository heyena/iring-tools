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
    List<DataTransferObject> _dataTransferObjects;

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

        PopulateClassIdentifiers();

        ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;

        _dataTransferObjects = new List<DataTransferObject>();
        for (int dataObjectIndex = 0; dataObjectIndex < _dataObjects.Count; dataObjectIndex++ )
        {
            DataTransferObject dataTransferObject = new DataTransferObject();
            _dataTransferObjects.Add(dataTransferObject);
            dataTransferObject.classObjects = new List<ClassObject>();
            FillDataTransferObjectList(dataTransferObject, classMap.classId, dataObjectIndex);            
        }
          XElement xElement = SerializationExtensions.ToXml<List<DataTransferObject>>(_dataTransferObjects);
          return xElement;       
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

   
    public IList<IDataObject> GetDataObjects(ref GraphMap graphMap, ref DataDictionary dataDictionary, ref XElement xml)
    {
        _graphMap = graphMap;
        _dataDictionary = dataDictionary;

        _dataTransferObjects = SerializationExtensions.ToObject<List<DataTransferObject>>(xml);
        ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
        List<string> identifiers = new List<string>();
        foreach (DataTransferObject dataTransferObject in _dataTransferObjects)
        {
            List<ClassObject> classObjects = dataTransferObject.GetClassObjects(classMap.classId);
            identifiers.Add(classObjects[0].identifier);                        
        }
        string objectType = _dataObjectNs + "." + _graphMap.dataObjectMap + ", " + _dataObjectsAssemblyName;
        IList<IDataObject> dataObjectList = _dataLayer.Create(objectType, identifiers);

        for (int dataTransferObjectIndex = 0; dataTransferObjectIndex < _dataTransferObjects.Count; dataTransferObjectIndex++)
        {
            IDataObject dataObject = dataObjectList[dataTransferObjectIndex];
            FillDataObjectList(dataObject, classMap.classId, dataTransferObjectIndex);
        }
        return dataObjectList;
    }

    #region helper methods

    private void FillDataTransferObjectList(DataTransferObject dataTransferObject, string classId, int dataObjectIndex)
    {
        KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
        List<TemplateMap> templateMaps = classTemplateListMap.Value;

        string classIdentifier = _classIdentifiers[classId][dataObjectIndex];

        ClassObject classObject = new ClassObject();
        classObject.classId = classId;
        classObject.templateObjects = new List<TemplateObject>();
        classObject.identifier = classIdentifier;
        
        foreach (TemplateMap templateMap in templateMaps)
        {
            TemplateObject templateObject = new TemplateObject();
            templateObject.templateId = templateMap.templateId;
            templateObject.name = templateMap.name;
            templateObject.roleObjects = new List<RoleObject>();
            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
                RoleObject roleObject = new RoleObject();
                roleObject.roleId = roleMap.roleId;
                roleObject.name = roleMap.name;
                if (roleMap.type == RoleType.Property)
                {
                    roleObject.value = _dataObjects[dataObjectIndex].GetPropertyValue(roleMap.propertyName.Substring(_graphMap.dataObjectMap.Length + 1)).ToString();
                }
                else if (roleMap.type == RoleType.Reference)
                {

                    if (roleMap.classMap != null)
                    {
                        bool classExists = false;
                        if (dataTransferObject.GetClassObjects(roleMap.classMap.classId).Count > 0)
                        {
                            roleObject.reference = roleMap.value;
                            classExists = true;
                        }
                        if (!classExists)
                        {
                            roleObject.reference = roleMap.value;
                            FillDataTransferObjectList(dataTransferObject, roleMap.classMap.classId, dataObjectIndex);
                        }
                    }
                    else
                    {
                        roleObject.reference = roleMap.value;
                    }
                }
                else
                {
                    roleObject.value = roleMap.value;
                }
                templateObject.roleObjects.Add(roleObject);
            }
            classObject.templateObjects.Add(templateObject);
            
        }
        dataTransferObject.classObjects.Add(classObject);
    }

    private void FillDataObjectList(IDataObject dataObject, string classId, int dataTransferObjectIndex)
    {
        KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
        List<TemplateMap> templateMaps = classTemplateListMap.Value;
        List<ClassObject> classObjects = _dataTransferObjects[dataTransferObjectIndex].GetClassObjects(classId);
      
        foreach (TemplateMap templateMap in templateMaps)
        {            
            List<TemplateObject> templateObjects = classObjects[0].GetTemplateObjects(templateMap.templateId);
           
            int roleObjectIndex = 0;
            foreach (RoleMap roleMap in templateMap.roleMaps)
            {                
                List<RoleObject> roleObjects = templateObjects[0].roleObjects;
               
                if (roleMap.type == RoleType.Property)
                {
                    string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectMap.Length + 1);
                    dataObject.SetPropertyValue(propertyName, roleObjects[roleObjectIndex].value);                    
                }
                else if (roleMap.type == RoleType.Reference)
                {
                    if (roleMap.classMap != null)
                    {
                       FillDataObjectList(dataObject, roleMap.classMap.classId, dataTransferObjectIndex);                       
                    }                    
                }
                roleObjectIndex++;               
            }
        }       
    }

    private void PopulateClassIdentifiers()
    {
        _classIdentifiers.Clear();

        foreach (ClassMap classMap in _graphMap.classTemplateListMaps.Keys)
        {
            List<string> classIdentifiers = new List<string>();

            foreach (string identifier in classMap.identifiers)
            {
                string[] property = identifier.Split('.');
                string objectName = property[0].Trim();
                string propertyName = property[1].Trim();

                if (_dataObjects != null)
                {
                    for (int i = 0; i < _dataObjects.Count; i++)
                    {
                        string value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                        if (classIdentifiers.Count == i)
                        {
                            classIdentifiers.Add(value);
                        }
                        else
                        {
                            classIdentifiers[i] += classMap.identifierDelimeter + value;
                        }
                    }
                }
            }

            _classIdentifiers[classMap.classId] = classIdentifiers;
        }
    }

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
