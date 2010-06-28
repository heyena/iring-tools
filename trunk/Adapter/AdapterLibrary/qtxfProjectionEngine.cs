using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using org.iringtools.library;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Ninject;
using org.iringtools.utility;
using VDS.RDF;

namespace org.iringtools.adapter.projection
{
  public class QtxfProjectionEngine : IProjectionLayer
  {
    private static readonly string DATALAYER_NS = "org.iringtools.adapter.datalayer";

    private static readonly XNamespace XSI_NS = "http://www.w3.org/2001/XMLSchema-instance#";
    private static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(QtxfProjectionEngine));

    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private DataDictionary _dataDictionary = null;
    private IList<IDataObject> _dataObjects = null; // dictionary of object names and list of data objects
    private Dictionary<string, List<string>> _classIdentifiers = null; // dictionary of class ids and list of identifiers
    private XNamespace _graphNs = String.Empty;
    private string _dataObjectNs = String.Empty;

    private static readonly string RDF_PREFIX = "rdf:";

    private static readonly string RDF_TYPE_ID = "tpl:R63638239485";
    private static readonly string CLASSIFICATION_INSTANCE_ID = "tpl:R55055340393";
    private static readonly string CLASS_INSTANCE_ID = "tpl:R99011248051";
    private static readonly string RDF_NIL = RDF_PREFIX + "nil";

    [Inject]
    public QtxfProjectionEngine(AdapterSettings adapterSettings, ApplicationSettings appSettings, IDataLayer dataLayer)
    {
      string scope = appSettings.ProjectName + "{0}" + appSettings.ApplicationName;

      _dataObjects = new List<IDataObject>();
      _classIdentifiers = new Dictionary<string, List<string>>();
      _mapping = Utility.Read<Mapping>(String.Format(adapterSettings.XmlPath + "Mapping." + scope + ".xml", "."));
      _graphNs = String.Format(adapterSettings.GraphBaseUri + "/" + scope + "#", "/");
      _dataObjectNs = String.Format(DATALAYER_NS + ".proj_" + scope, ".");
    }

    public XElement GetXml(ref GraphMap graphMap, ref DataDictionary dataDictionary, ref IList<IDataObject> dataObjects)
    {
      try
      {
        _graphMap = graphMap;
        _dataDictionary = dataDictionary;
        _dataObjects = dataObjects;

        return GetQtxf();
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

    public XElement GetQtxf()
    {
      try
      {
        PopulateClassIdentifiers();

        XElement graphElement = new XElement(_graphNs + _graphMap.name, new XAttribute(XNamespace.Xmlns + "i", XSI_NS));

        for (int i = 0; i < _dataObjects.Count; i++)
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
          case RoleType.Possessor:
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

                  IDataObject dataObject = _dataObjects.ElementAt(objectIndex);

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

              IDataObject dataObject = _dataObjects.ElementAt(objectIndex);
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
                string valueListUri = _mapping.ResolveValueList(roleMap.valueList, value);
                roleElement.Add(new XAttribute("reference", Regex.Replace(valueListUri, ".*#", "rdl:")));
              }

              break;
            }
        }
      }

      return templateElement;
    }
  }
}
