using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using org.iringtools.library;
using org.iringtools.common.mapping;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Ninject;
using org.iringtools.utility;
using VDS.RDF;
using org.iringtools.protocol.manifest;

namespace org.iringtools.adapter.projection
{
  public class QtxfProjectionEngine : IProjectionLayer
  {
    private static readonly XNamespace XSI_NS = "http://www.w3.org/2001/XMLSchema-instance#";
    private static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(QtxfProjectionEngine));

    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private IList<IDataObject> _dataObjects = null; // dictionary of object names and list of data objects
    private Dictionary<string, List<string>> _classIdentifiers = null; // dictionary of class ids and list of identifiers
    private XNamespace _graphNs = String.Empty;

    private static readonly string RDF_PREFIX = "rdf:";

    private static readonly string RDF_TYPE_ID = "tpl:R63638239485";
    private static readonly string CLASSIFICATION_INSTANCE_ID = "tpl:R55055340393";
    private static readonly string CLASS_INSTANCE_ID = "tpl:R99011248051";
    private static readonly string RDF_NIL = RDF_PREFIX + "nil";

    [Inject]
    public QtxfProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping)
    {
      _mapping = mapping;

      _graphNs = String.Format("{0}{1}/{2}",
        settings["GraphBaseUri"],
        settings["ProjectName"],
        settings["ApplicationName"]
      );

      _dataObjects = new List<IDataObject>();
      _classIdentifiers = new Dictionary<string, List<string>>();
    }

    public XElement ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);
        _dataObjects = dataObjects;

        return GetQtxf();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public IList<IDataObject> ToDataObjects(string graphName, ref XElement xml)
    {
      throw new NotImplementedException();
    }

    #region helper methods
    private void PopulateClassIdentifiers()
    {
      _classIdentifiers.Clear();

      foreach (ClassTemplateMap classTemplateMap in _graphMap.ClassTemplateMaps)
      {
        ClassMap classMap = classTemplateMap.ClassMap;

        List<string> classIdentifiers = new List<string>();

        foreach (string identifier in classMap.Identifiers)
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
                classIdentifiers[i] += classMap.IdentifierDelimeter + value;
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
                  classIdentifiers[i] += classMap.IdentifierDelimeter + value;
                }
              }
            }
          }
        }

        _classIdentifiers[classMap.ClassId] = classIdentifiers;
      }
    }

    private XElement GetQtxf()
    {
      try
      {
        PopulateClassIdentifiers();

        XElement graphElement = new XElement(_graphNs + _graphMap.Name, new XAttribute(XNamespace.Xmlns + "i", XSI_NS));

        for (int i = 0; i < _dataObjects.Count; i++)
        {
          foreach (ClassTemplateMap classTemplateMap in _graphMap.ClassTemplateMaps)
          {
            ClassMap classMap = classTemplateMap.ClassMap;
            List<TemplateMap> templateMaps = classTemplateMap.TemplateMaps;
            string classInstance = _classIdentifiers[classMap.ClassId][i];

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
      hasClassElement.Add(new XAttribute("reference", classMap.ClassId));
      typeOfThingElement.Add(hasClassElement);

      XElement hasIndividualElement = new XElement(_graphNs + "hasIndividual");
      hasIndividualElement.Add(new XAttribute("rdlUri", CLASS_INSTANCE_ID));
      hasIndividualElement.Add(new XAttribute("reference", classInstance));
      typeOfThingElement.Add(hasIndividualElement);

      return typeOfThingElement;
    }

    private XElement CreateQtxfTemplateElement(TemplateMap templateMap, string classInstance, int objectIndex)
    {
      XElement templateElement = new XElement(_graphNs + templateMap.Name);
      templateElement.Add(new XAttribute("rdlUri", templateMap.TemplateId));

      foreach (RoleMap roleMap in templateMap.RoleMaps)
      {
        XElement roleElement = new XElement(_graphNs + roleMap.Name);
        roleElement.Add(new XAttribute("rdlUri", roleMap.RoleId));
        templateElement.Add(roleElement);

        switch (roleMap.Type)
        {
          case RoleType.Possessor:
            roleElement.Add(new XAttribute("reference", classInstance));
            break;

          case RoleType.Reference:
            {
              if (roleMap.ClassMap != null)
              {
                string identifierValue = String.Empty;

                foreach (string identifier in roleMap.ClassMap.Identifiers)
                {
                  if (identifier.StartsWith("#") && identifier.EndsWith("#"))
                  {
                    identifierValue += identifier.Substring(1, identifier.Length - 2);
                  }
                  else
                  {
                    string[] property = identifier.Split('.');
                    string objectName = property[0].Trim();
                    string propertyName = property[1].Trim();

                    IDataObject dataObject = _dataObjects.ElementAt(objectIndex);

                    if (dataObject != null)
                    {
                      string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

                      if (identifierValue != String.Empty)
                        identifierValue += roleMap.ClassMap.IdentifierDelimeter;

                      identifierValue += value;
                    }
                  }
                }

                roleElement.Add(new XAttribute("reference", identifierValue));
              }
              else
              {
                roleElement.Add(new XAttribute("reference", roleMap.Value));
              }
              break;
            }

          case RoleType.FixedValue:
            roleElement.Add(new XAttribute("reference", roleMap.Value));
            break;

          case RoleType.Property:
            {
              string[] property = roleMap.PropertyName.Split('.');
              string objectName = property[0].Trim();
              string propertyName = property[1].Trim();

              IDataObject dataObject = _dataObjects.ElementAt(objectIndex);
              string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

              if (String.IsNullOrEmpty(roleMap.ValueListName))
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
                string valueListUri = _mapping.ResolveValueList(roleMap.ValueListName, value);
                roleElement.Add(new XAttribute("reference", Regex.Replace(valueListUri, ".*#", "rdl:")));
              }

              break;
            }
        }
      }

      return templateElement;
    }
    #endregion
  }
}
