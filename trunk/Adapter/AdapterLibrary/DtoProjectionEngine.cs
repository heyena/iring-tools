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
    private DataTransferObjects _dataTransferObjects;

    [Inject]
    public DtoProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping, DataDictionary dataDictionary)
    {
      _dataObjects = new List<IDataObject>();
      _classIdentifiers = new Dictionary<string, List<string>>();
      _xPathValuePairs = new List<Dictionary<string, string>>();
      _hierachicalDTOClasses = new Dictionary<string, List<string>>();

      _dataLayer = dataLayer;
      _dataDictionary = dataDictionary;
      _mapping = mapping;

      _graphNs = String.Format("{0}{1}/{2}",
        settings["GraphBaseUri"],
        settings["ProjectName"],
        settings["ApplicationName"]
      );

      _dataObjectNs = String.Format("{0}.proj_{1}.{2}",
        DATALAYER_NS,
        settings["ProjectName"],
        settings["ApplicationName"]
      );

      _dataObjectsAssemblyName = settings["ExecutingAssemblyName"];
    }

    public string ObjectType
    {
      get
      {
        return _dataObjectNs + "." + _graphMap.dataObjectMap + ", " + _dataObjectsAssemblyName;
      }
    }

    public XElement GetXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);
        _dataObjects = dataObjects;

        PopulateClassIdentifiers();

        _dataTransferObjects = new DataTransferObjects();
        ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
        
        for (int dataObjectIndex = 0; dataObjectIndex < _dataObjects.Count; dataObjectIndex++)
        {
          DataTransferObject dataTransferObject = new DataTransferObject();
          _dataTransferObjects.Add(dataTransferObject);
          PopulateDataTransferObjects(ref dataTransferObject, classMap, dataObjectIndex);
        }

        return SerializationExtensions.ToXml<DataTransferObjects>(_dataTransferObjects);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public IList<IDataObject> GetDataObjects(string graphName, ref XElement xml)
    {
      _graphMap = _mapping.FindGraphMap(graphName);
      _dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(xml);

      ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
      List<string> identifiers = new List<string>();
      for (int i = 0; i < _dataTransferObjects.Count; i++)
      {
        DataTransferObject dataTransferObject = _dataTransferObjects[i];
        ClassObject classObject = dataTransferObject.GetClassObject(classMap.classId);

        if (classObject != null)
        {
          identifiers.Add(classObject.identifier);
        }
      }

      IList<IDataObject> dataObjects = _dataLayer.Create(ObjectType, identifiers);
      for (int dataTransferObjectIndex = 0; dataTransferObjectIndex < _dataTransferObjects.Count; dataTransferObjectIndex++)
      {
        IDataObject dataObject = dataObjects[dataTransferObjectIndex];
        PopulateDataObjects(ref dataObject, classMap.classId, dataTransferObjectIndex);
      }

      return dataObjects;
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

    private void PopulateClassIdentifiers()
    {
      _classIdentifiers.Clear();

      foreach (ClassMap classMap in _graphMap.classTemplateListMaps.Keys)
      {
        List<string> classIdentifiers = new List<string>();

        foreach (string identifier in classMap.identifiers)
        {
          // identifier is a fixed value
          if (identifier.StartsWith("#") && identifier.EndsWith("#"))
          {
            string value = identifier.Substring(1, identifier.Length - 2);

            for (int i = 0; i < _dataObjects.Count; i++)
            {
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
          else  // identifier comes from a property
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
        }

        _classIdentifiers[classMap.classId] = classIdentifiers;
      }
    }

    private void PopulateDataTransferObjects(ref DataTransferObject dataTransferObject, ClassMap classMap, int dataObjectIndex)
    {
      string classId = classMap.classId;
      string className = classMap.name;

      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
      List<TemplateMap> templateMaps = classTemplateListMap.Value;
      string classIdentifier = _classIdentifiers[classId][dataObjectIndex];

      ClassObject classObject = new ClassObject
      {
        classId = classId,
        name = className,
        identifier = classIdentifier,
      };
      dataTransferObject.classObjects.Add(classObject);

      foreach (TemplateMap templateMap in templateMaps)
      {
        TemplateObject templateObject = new TemplateObject
        {
          templateId = templateMap.templateId,
          name = templateMap.name,
        };
        classObject.templateObjects.Add(templateObject);

        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          RoleObject roleObject = new RoleObject();
          roleObject.roleId = roleMap.roleId;
          roleObject.name = roleMap.name;
          templateObject.roleObjects.Add(roleObject);

          if (roleMap.type == RoleType.Property)
          {
            string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectMap.Length + 1);
            roleObject.value = Convert.ToString(_dataObjects[dataObjectIndex].GetPropertyValue(propertyName));
          }
          else if (roleMap.type == RoleType.Reference)
          {
            roleObject.reference = roleMap.value;

            if (roleMap.classMap != null)
            {
              PopulateDataTransferObjects(ref dataTransferObject, roleMap.classMap, dataObjectIndex);
            }
          }
          else
          {
            roleObject.value = roleMap.value;
          }
        }
      }
    }

    private void PopulateDataObjects(ref IDataObject dataObject, string classId, int dataTransferObjectIndex)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
      List<TemplateMap> templateMaps = classTemplateListMap.Value;
      ClassObject classObject = _dataTransferObjects[dataTransferObjectIndex].GetClassObject(classId);

      foreach (TemplateMap templateMap in templateMaps)
      {
        TemplateObject templateObject = classObject.GetTemplateObject(templateMap);

        if (templateObject != null)
        {
          foreach (RoleMap roleMap in templateMap.roleMaps)
          {
            if (roleMap.type == RoleType.Property)
            {
              string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectMap.Length + 1);

              foreach (RoleObject roleObject in templateObject.roleObjects)
              {
                if (roleObject.roleId == roleMap.roleId)
                {
                  dataObject.SetPropertyValue(propertyName, roleObject.value);
                }
              }
            }

            if (roleMap.classMap != null)
            {
              PopulateDataObjects(ref dataObject, roleMap.classMap.classId, dataTransferObjectIndex);
            }
          }
        }
      }
    }
    #endregion
  }
}
