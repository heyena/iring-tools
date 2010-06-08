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

namespace org.iringtools.adapter.projection
{
  public class DTOProjectionEngine : IProjectionLayer
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
    private static readonly XName RDF_TYPE = RDF_NS + "type";
    private static readonly XName RDF_RESOURCE = RDF_NS + "resource";
    private static readonly XName RDF_DATATYPE = RDF_NS + "datatype";

    private static readonly string XSD_PREFIX = "xsd:";
    private static readonly string RDF_PREFIX = "rdf:";
    private static readonly string RDL_PREFIX = "rdl:";
    private static readonly string TPL_PREFIX = "tpl:";

    private static readonly string RDF_TYPE_ID = "tpl:R63638239485";
    private static readonly string CLASSIFICATION_INSTANCE_ID = "tpl:R55055340393";
    private static readonly string CLASS_INSTANCE_ID = "tpl:R99011248051";
    private static readonly string RDF_NIL = RDF_PREFIX + "nil";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(DTOProjectionEngine));

    private IDataLayer _dataLayer = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private Dictionary<string, IList<IDataObject>> _dataObjectSet = null; // dictionary of object names and list of data objects
    private Dictionary<string, List<string>> _classIdentifiers = null; // dictionary of class ids and list of identifiers
    private List<Dictionary<string, string>> _xPathValuePairs = null;  // dictionary of property xpath and value pairs
    private Dictionary<string, List<string>> _hierachicalDTOClasses = null;  // dictionary of class rdlUri and identifiers
    private XNamespace _graphNs = String.Empty;
    private string _dataObjectsAssemblyName = String.Empty;
    private string _dataObjectNs = String.Empty;

    [Inject]
    public DTOProjectionEngine(AdapterSettings adapterSettings, ApplicationSettings appSettings, IDataLayer dataLayer)
    {
      string scope = appSettings.ProjectName + "{0}" + appSettings.ApplicationName;

      _dataObjectSet = new Dictionary<string, IList<IDataObject>>();
      _classIdentifiers = new Dictionary<string, List<string>>();
      _xPathValuePairs = new List<Dictionary<string, string>>();
      _hierachicalDTOClasses = new Dictionary<string, List<string>>();

      _dataLayer = dataLayer;
      _mapping = Utility.Read<Mapping>(String.Format(adapterSettings.XmlPath + "Mapping." + scope + ".xml", "."));
      _graphNs = String.Format(adapterSettings.GraphBaseUri + scope + "#", "/");
      _dataObjectNs = String.Format(DATALAYER_NS + ".proj_" + scope, ".");
      _dataObjectsAssemblyName = adapterSettings.ExecutingAssemblyName;
    }

    public XElement GetRdf(string graphName)
    {
      try
      {
        FindGraphMap(graphName);
        LoadDataObjectSet();
        return GetRdf();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public XElement GetQtxf(string graphName)
    {
      try
      {
        FindGraphMap(graphName);
        LoadDataObjectSet();

        XElement graphElement = new XElement(_graphNs + graphName, new XAttribute(XNamespace.Xmlns + "i", XSI_NS));
        int maxDataObjectsCount = MaxDataObjectsCount();

        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          foreach (var pair in _graphMap.classTemplateListMaps)
          {
            ClassMap classMap = pair.Key;
            List<TemplateMap> templateMaps = pair.Value;
            string classInstance = _classIdentifiers[classMap.classId][i];

            XElement typeOfThingElement = CreateQtxfClassElement(classMap, classInstance);
            graphElement.Add(typeOfThingElement);

            foreach (TemplateMap templateMap in templateMaps)
            {
              XElement templateElement = CreateQtxfTemplateElement(templateMap, classInstance, i);
              graphElement.Add(templateElement);
            }
          }
        }

        return graphElement;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public List<Dictionary<string, string>> GetDTOList(string graphName)
    {
      try
      {
        FindGraphMap(graphName);
        LoadDataObjectSet();

        int maxDataObjectsCount = MaxDataObjectsCount();
        _xPathValuePairs.Clear();

        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          _xPathValuePairs.Add(new Dictionary<string, string>());
        }

        ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
        FillDTOList(classMap.classId, "rdl:" + classMap.name);

        return _xPathValuePairs;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public XElement GetHierachicalDTOList(string graphName)
    {
      try
      {
        FindGraphMap(graphName);
        LoadDataObjectSet();

        _hierachicalDTOClasses.Clear();

        XElement graphElement = new XElement(_graphNs + graphName,
          new XAttribute(XNamespace.Xmlns + "i", XSI_NS),
          new XAttribute(XNamespace.Xmlns + "rdl", RDL_NS),
          new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

        ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
        int maxDataObjectsCount = MaxDataObjectsCount();

        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          XElement classElement = new XElement(_graphNs + TitleCase(classMap.name));
          graphElement.Add(classElement);
          FillHierachicalDTOList(classElement, classMap.classId, i);
        }

        return graphElement;
      }
      catch (Exception ex)
      {
        throw ex;
      }
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

    private void FindGraphMap(string graphName)
    {
      foreach (GraphMap graphMap in _mapping.graphMaps)
      {
        if (graphMap.name.ToLower() == graphName.ToLower())
        {
          _graphMap = graphMap;

          if (_graphMap.classTemplateListMaps.Count == 0)
            throw new Exception("Graph [" + graphName + "] is empty.");

          return;
        }
      }

      throw new Exception("Graph [" + graphName + "] does not exist.");
    }

    private void LoadDataObjectSet()
    {
      _dataObjectSet.Clear();

      foreach (DataObjectMap dataObjectMap in _graphMap.dataObjectMaps)
      {
        _dataObjectSet.Add(dataObjectMap.name, _dataLayer.Get(dataObjectMap.name, null));
      }

      PopulateClassIdentifiers();
    }

    // get max # of data records from all data objects
    private int MaxDataObjectsCount()
    {
      int maxCount = 0;

      foreach (var pair in _dataObjectSet)
      {
        if (pair.Value.Count > maxCount)
        {
          maxCount = pair.Value.Count;
        }
      }

      return maxCount;
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

          IList<IDataObject> dataObjects = _dataObjectSet[objectName];
          if (dataObjects != null)
          {
            for (int i = 0; i < dataObjects.Count; i++)
            {
              string value = Convert.ToString(dataObjects[i].GetPropertyValue(propertyName));

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

    private string ResolveValueList(string valueList, string value)
    {
      if (_mapping != null && _mapping.valueMaps.Count > 0)
      {
        foreach (ValueMap valueMap in _mapping.valueMaps)
        {
          if (valueMap.valueList == valueList && valueMap.internalValue == value)
          {
            return valueMap.uri;
          }
        }
      }

      return RDF_NIL;
    }

    private XElement GetRdf()
    {
      XElement graphElement = new XElement(RDF_NS + "RDF",
        new XAttribute(XNamespace.Xmlns + "rdf", RDF_NS),
        new XAttribute(XNamespace.Xmlns + "owl", OWL_NS),
        new XAttribute(XNamespace.Xmlns + "xsd", XSD_NS),
        new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

      foreach (var pair in _graphMap.classTemplateListMaps)
      {
        ClassMap classMap = pair.Key;
        int maxDataObjectsCount = MaxDataObjectsCount();

        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          string classId = classMap.classId.Substring(classMap.classId.IndexOf(":") + 1);
          string classInstance = _graphNs.NamespaceName + _classIdentifiers[classMap.classId][i];

          graphElement.Add(CreateRdfClassElement(classId, classInstance));

          foreach (TemplateMap templateMap in pair.Value)
          {
            graphElement.Add(CreateRdfTemplateElement(templateMap, classInstance, i));
          }
        }
      }

      return graphElement;
    }

    private XElement CreateRdfClassElement(string classId, string classInstance)
    {
      return new XElement(OWL_THING, new XAttribute(RDF_ABOUT, classInstance),
        new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, RDL_NS.NamespaceName + classId)));
    }

    private XElement CreateRdfTemplateElement(TemplateMap templateMap, string classInstance, int dataObjectIndex)
    {
      string templateId = templateMap.templateId.Replace(TPL_PREFIX, TPL_NS.NamespaceName);

      XElement templateElement = new XElement(OWL_THING);
      templateElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, templateId)));

      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
        string dataType = String.Empty;
        XElement roleElement = new XElement(TPL_NS + roleId);

        switch (roleMap.type)
        {
          case RoleType.ClassRole:
            {
              roleElement.Add(new XAttribute(RDF_RESOURCE, classInstance));
              break;
            }
          case RoleType.Reference:
            {
              if (roleMap.classMap != null)
              {
                string identifierValue = String.Empty;

                foreach (string identifier in roleMap.classMap.identifiers)
                {
                  string[] property = identifier.Split('.');
                  string objectName = property[0].Trim();
                  string propertyName = property[1].Trim();

                  IDataObject dataObject = _dataObjectSet[objectName].ElementAt(dataObjectIndex);

                  if (dataObject != null)
                  {
                    string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

                    if (identifierValue != String.Empty)
                      identifierValue += roleMap.classMap.identifierDelimeter;

                    identifierValue += value;
                  }
                }

                roleElement.Add(new XAttribute(RDF_RESOURCE, _graphNs.NamespaceName + identifierValue));
              }
              else
              {
                roleElement.Add(new XAttribute(RDF_RESOURCE, roleMap.value.Replace(RDL_PREFIX, RDL_NS.NamespaceName)));
              }
              break;
            }
          case RoleType.FixedValue:
            {
              dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
              roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
              roleElement.Add(new XText(roleMap.value));
              break;
            }
          case RoleType.Property:
            {
              string[] property = roleMap.propertyName.Split('.');
              string objectName = property[0].Trim();
              string propertyName = property[1].Trim();

              IDataObject dataObject = _dataObjectSet[objectName].ElementAt(dataObjectIndex);
              string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

              if (String.IsNullOrEmpty(roleMap.valueList))
              {
                if (String.IsNullOrEmpty(value))
                {
                  roleElement.Add(new XAttribute(RDF_RESOURCE, RDF_NIL));
                }
                else
                {
                  dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
                  roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
                  roleElement.Add(new XText(value));
                }
              }
              else // resolve value list to uri
              {
                string valueListUri = ResolveValueList(roleMap.valueList, value);
                roleElement.Add(new XAttribute(RDF_RESOURCE, valueListUri));
              }

              break;
            }
        }

        templateElement.Add(roleElement);
      }

      return templateElement;
    }

    private XElement CreateQtxfClassElement(ClassMap classMap, string classInstance)
    {
      XElement typeOfThingElement = new XElement(_graphNs + "TypeOfThing");
      typeOfThingElement.Add(new XAttribute("rdlUri", RDF_TYPE_ID));

      XElement hasClassElement = new XElement(_graphNs + "hasClass");
      hasClassElement.Add(new XAttribute("rdlUri", CLASSIFICATION_INSTANCE_ID));
      hasClassElement.Add(new XAttribute("reference", classMap.classId));
      typeOfThingElement.Add(hasClassElement);

      XElement hasIndividualElement = new XElement(_graphNs + "hasIndividual");
      hasIndividualElement.Add(new XAttribute("rdlUri", CLASS_INSTANCE_ID));
      hasIndividualElement.Add(new XAttribute("reference", classInstance));
      typeOfThingElement.Add(hasIndividualElement);

      return typeOfThingElement;
    }

    private XElement CreateQtxfTemplateElement(TemplateMap templateMap, string classInstance, int objectIndex)
    {
      XElement templateElement = new XElement(_graphNs + templateMap.name);
      templateElement.Add(new XAttribute("rdlUri", templateMap.templateId));

      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        XElement roleElement = new XElement(_graphNs + roleMap.name);
        roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
        templateElement.Add(roleElement);

        switch (roleMap.type)
        {
          case RoleType.ClassRole:
            roleElement.Add(new XAttribute("reference", classInstance));
            break;

          case RoleType.Reference:
            {
              if (roleMap.classMap != null)
              {
                string identifierValue = String.Empty;

                foreach (string identifier in roleMap.classMap.identifiers)
                {
                  string[] property = identifier.Split('.');
                  string objectName = property[0].Trim();
                  string propertyName = property[1].Trim();

                  IDataObject dataObject = _dataObjectSet[objectName].ElementAt(objectIndex);

                  if (dataObject != null)
                  {
                    string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

                    if (identifierValue != String.Empty)
                      identifierValue += roleMap.classMap.identifierDelimeter;

                    identifierValue += value;
                  }
                }

                roleElement.Add(new XAttribute("reference", identifierValue));
              }
              else
              {
                roleElement.Add(new XAttribute("reference", roleMap.value));
              }
              break;
            }

          case RoleType.FixedValue:
            roleElement.Add(new XAttribute("reference", roleMap.value));
            break;

          case RoleType.Property:
            {
              string[] property = roleMap.propertyName.Split('.');
              string objectName = property[0].Trim();
              string propertyName = property[1].Trim();

              IDataObject dataObject = _dataObjectSet[objectName].ElementAt(objectIndex);
              string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

              if (String.IsNullOrEmpty(roleMap.valueList))
              {
                if (String.IsNullOrEmpty(value))
                {
                  roleElement.Add(new XAttribute("reference", RDF_NIL));
                }
                else
                {
                  roleElement.Add(new XText(value));
                }
              }
              else // resolve value list to uri
              {
                string valueListUri = ResolveValueList(roleMap.valueList, value);
                roleElement.Add(new XAttribute("reference", Regex.Replace(valueListUri, ".*#", "rdl:")));
              }

              break;
            }
        }
      }

      return templateElement;
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

            IList<IDataObject> dataObjects = _dataObjectSet[objectName];
            for (int i = 0; i < dataObjects.Count; i++)
            {
              value = Convert.ToString(dataObjects[i].GetPropertyValue(propertyName));

              if (!String.IsNullOrEmpty(roleMap.valueList))
              {
                value = ResolveValueList(roleMap.valueList, value);
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

    private void FillHierarchicalDTOList(XElement classElement, string classId, int dataObjectIndex)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
      ClassMap classMap = classTemplateListMap.Key;
      List<TemplateMap> templateMaps = classTemplateListMap.Value;

      classElement.Add(new XAttribute("rdlUri", classMap.classId));
      classElement.Add(new XAttribute("id", _classIdentifiers[classMap.classId][dataObjectIndex]));

      foreach (TemplateMap templateMap in templateMaps)
      {
        XElement templateElement = new XElement(_graphNs + templateMap.name);
        templateElement.Add(new XAttribute("rdlUri", templateMap.templateId));
        classElement.Add(templateElement);

        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          XElement roleElement = new XElement(_graphNs + roleMap.name);

          switch (roleMap.type)
          {
            case RoleType.ClassRole:
              templateElement.Add(new XAttribute("classRole", roleMap.roleId));
              break;

            case RoleType.Reference:
              roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
              templateElement.Add(roleElement);

              if (roleMap.classMap != null)
              {
                XElement element = new XElement(_graphNs + TitleCase(roleMap.classMap.name));
                roleElement.Add(element);
                FillHierarchicalDTOList(element, roleMap.classMap.classId, dataObjectIndex);
              }
              else
              {
                roleElement.Add(new XAttribute("reference", roleMap.value));
              }

              break;

            case RoleType.FixedValue:
              roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
              roleElement.Add(new XText(roleMap.value));
              templateElement.Add(roleElement);
              break;

            case RoleType.Property:
              string[] property = roleMap.propertyName.Split('.');
              string objectName = property[0].Trim();
              string propertyName = property[1].Trim();
              IDataObject dataObject = _dataObjectSet[objectName][dataObjectIndex];
              roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));

              string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));
              if (!String.IsNullOrEmpty(roleMap.valueList))
              {
                value = ResolveValueList(roleMap.valueList, value);
                value = value.Replace(RDL_NS.NamespaceName, "rdl:");
                roleElement.Add(new XAttribute("reference", value));
              }
              else
              {
                roleElement.Add(new XText(value));
              }

              templateElement.Add(roleElement);
              break;
          }
        }
      }
    }

    private void FillHierachicalDTOList(XElement classElement, string classId, int dataObjectIndex)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
      ClassMap classMap = classTemplateListMap.Key;
      List<TemplateMap> templateMaps = classTemplateListMap.Value;
      string classIdentifier = _classIdentifiers[classMap.classId][dataObjectIndex];

      classElement.Add(new XAttribute("rdlUri", classMap.classId));
      classElement.Add(new XAttribute("id", classIdentifier));

      if (_hierachicalDTOClasses.ContainsKey(classId))
      {
        List<string> classIdentifiers = _hierachicalDTOClasses[classId];
        classIdentifiers.Add(classIdentifier);
      }
      else
      {
        _hierachicalDTOClasses[classId] = new List<string> { classIdentifier };
      }

      foreach (TemplateMap templateMap in templateMaps)
      {
        XElement templateElement = new XElement(_graphNs + templateMap.name);
        templateElement.Add(new XAttribute("rdlUri", templateMap.templateId));
        classElement.Add(templateElement);

        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          XElement roleElement = new XElement(_graphNs + roleMap.name);

          switch (roleMap.type)
          {
            case RoleType.ClassRole:
              templateElement.Add(new XAttribute("classRole", roleMap.roleId));
              break;

            case RoleType.Reference:
              roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
              templateElement.Add(roleElement);

              if (roleMap.classMap != null)
              {
                bool classExists = false;

                // check if the class instance has been created
                if (_hierachicalDTOClasses.ContainsKey(roleMap.classMap.classId))
                {
                  List<string> identifiers = _hierachicalDTOClasses[roleMap.classMap.classId];
                  string identifier = _classIdentifiers[roleMap.classMap.classId][dataObjectIndex];

                  if (identifiers.Contains(identifier))
                  {
                    roleElement.Add(new XAttribute("reference", identifier));
                    classExists = true;
                  }
                }

                if (!classExists)
                {
                  XElement element = new XElement(_graphNs + TitleCase(roleMap.classMap.name));
                  roleElement.Add(element);

                  FillHierachicalDTOList(element, roleMap.classMap.classId, dataObjectIndex);
                }
              }
              else
              {
                roleElement.Add(new XAttribute("reference", roleMap.value));
              }

              break;

            case RoleType.FixedValue:
              roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
              roleElement.Add(new XText(roleMap.value));
              templateElement.Add(roleElement);
              break;

            case RoleType.Property:
              string[] property = roleMap.propertyName.Split('.');
              string objectName = property[0].Trim();
              string propertyName = property[1].Trim();
              IDataObject dataObject = _dataObjectSet[objectName][dataObjectIndex];

              roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
              templateElement.Add(roleElement);

              string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));
              if (!String.IsNullOrEmpty(roleMap.valueList))
              {
                value = ResolveValueList(roleMap.valueList, value);
                value = value.Replace(RDL_NS.NamespaceName, "rdl:");
                roleElement.Add(new XAttribute("reference", value));
              }
              else
              {
                roleElement.Add(new XText(value));
              }

              break;
          }
        }
      }
    }
    #endregion
  }
}
