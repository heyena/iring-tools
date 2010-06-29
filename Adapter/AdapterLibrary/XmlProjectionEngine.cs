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
  public class XmlProjectionEngine : IProjectionLayer
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

    private static readonly ILog _logger = LogManager.GetLogger(typeof(XmlProjectionEngine));

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
    public XmlProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping, DataDictionary dataDictionry)
    {
      _dataObjects = new List<IDataObject>();
      _classIdentifiers = new Dictionary<string, List<string>>();
      _xPathValuePairs = new List<Dictionary<string, string>>();
      _hierachicalDTOClasses = new Dictionary<string, List<string>>();

      _dataLayer = dataLayer;
      _dataDictionary = dataDictionry;
      _mapping = mapping;

      _graphNs = String.Format("{0}/{1}/{2}",
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
    
    public XElement GetXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);
        _dataObjects = dataObjects;

        PopulateClassIdentifiers();

        _hierachicalDTOClasses.Clear();

        XElement graphElement = new XElement(_graphNs + _graphMap.name,
          new XAttribute(XNamespace.Xmlns + "i", XSI_NS),
          new XAttribute(XNamespace.Xmlns + "rdl", RDL_NS),
          new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

        ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
        
        for (int i = 0; i < _dataObjects.Count; i++)
        {
          XElement classElement = new XElement(_graphNs + TitleCase(classMap.name));
          graphElement.Add(classElement);
          FillHierarchicalDTOList(classElement, classMap.classId, i);
        }

        return graphElement;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public IList<IDataObject> GetDataObjects(string graphName, ref XElement xml)
    {
      throw new NotImplementedException();
    }

    #region helper methods
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

    private void FillHierarchicalDTOList(XElement classElement, string classId, int dataObjectIndex)
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
            case RoleType.Possessor:
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

                  FillHierarchicalDTOList(element, roleMap.classMap.classId, dataObjectIndex);
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
              IDataObject dataObject = _dataObjects.ElementAt(dataObjectIndex);

              roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
              templateElement.Add(roleElement);

              string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));
              if (!String.IsNullOrEmpty(roleMap.valueList))
              {
                value = _mapping.ResolveValueList(roleMap.valueList, value);
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
