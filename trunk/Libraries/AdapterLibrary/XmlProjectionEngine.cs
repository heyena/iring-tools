using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Xml;
using System.Xml.Linq;
using Ninject;
using log4net;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Storage;
using org.iringtools.utility;
using org.iringtools.mapping;
using org.iringtools.dxfr.manifest;
using System.Web;

namespace org.iringtools.adapter.projection
{
  public class XmlProjectionEngine : BaseProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(XmlProjectionEngine));
    private Dictionary<string, List<string>> _classIdentifiersCache = null;
    private XNamespace _appNamespace = null;

    [Inject]
    public XmlProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping)
    {
      _settings = settings;
      _dataLayer = dataLayer;
      _mapping = mapping;
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XElement xElement = null;

      try
      {
        _appNamespace = String.Format("{0}{1}/{2}",
           _settings["BaseAddress"],
           HttpUtility.UrlEncode(_settings["ProjectName"]),
           HttpUtility.UrlEncode(_settings["ApplicationName"])
         );

        _graphMap = _mapping.FindGraphMap(graphName);
        _dataObjects = dataObjects;

        //TODO: use entities for rdl & tpl instead of namespaces
        xElement = new XElement(_appNamespace + Utility.TitleCase(graphName),
          new XAttribute(XNamespace.Xmlns + "i", XSI_NS),
          new XAttribute(XNamespace.Xmlns + "rdl", RDL_NS),
          new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

        if (_graphMap != null && _graphMap.classTemplateMaps.Count > 0 &&
          _dataObjects != null && _dataObjects.Count > 0)
        {
          _classIdentifiersCache = new Dictionary<string, List<string>>();
          SetClassIdentifiers(DataDirection.Outbound);

          ClassTemplateMap classTemplateMap = _graphMap.classTemplateMaps.First();
          for (int i = 0; i < _dataObjects.Count; i++)
          {
            CreateHierarchicalXml(xElement, classTemplateMap, i);
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }

      return new XDocument(xElement);
    }

    public override IList<IDataObject> ToDataObjects(string graphName, ref XDocument xml)
    {
      throw new NotImplementedException();
    }

    #region helper methods
    private void CreateHierarchicalXml(XElement parentElement, ClassTemplateMap classTemplateMap, int dataObjectIndex)
    {
      ClassMap classMap = classTemplateMap.classMap;
      List<TemplateMap> templateMaps = classTemplateMap.templateMaps;
      string classIdentifier = _classIdentifiers[classMap.id][dataObjectIndex];

      XElement classElement = new XElement(_appNamespace + Utility.TitleCase(classMap.name));
      classElement.Add(new XAttribute("rdlUri", classMap.id));
      parentElement.Add(classElement);
      bool classExists = false;

      if (_classIdentifiersCache.ContainsKey(classMap.id))
      {
        List<string> classIdentifiers = _classIdentifiersCache[classMap.id];

        if (!classIdentifiers.Contains(classIdentifier))
        {
          classIdentifiers.Add(classIdentifier);
          classElement.Add(new XAttribute("id", classIdentifier));
        }
        else
        {
          classElement.Add(new XAttribute("reference", "#" + classIdentifier));
          classExists = true;
        }
      }
      else
      {
        _classIdentifiersCache[classMap.id] = new List<string> { classIdentifier };
        classElement.Add(new XAttribute("id", classIdentifier));
      }

      if (!classExists)
      {
        foreach (TemplateMap templateMap in templateMaps)
        {
          Dictionary<string, List<IDataObject>> relatedObjects = new Dictionary<string, List<IDataObject>>();
          List<RoleMap> propertyRoles = new List<RoleMap>();
          RoleMap classRole = null;

          XElement baseTemplateElement = new XElement(_appNamespace + templateMap.name);
          baseTemplateElement.Add(new XAttribute("rdlUri", templateMap.id));

          foreach (RoleMap roleMap in templateMap.roleMaps)
          {
            XElement roleElement = new XElement(_appNamespace + roleMap.name);

            switch (roleMap.type)
            {
              case RoleType.Possessor:
                baseTemplateElement.Add(new XAttribute("possessorRole", roleMap.id));
                break;

              case RoleType.Reference:
                if (roleMap.classMap != null)
                  classRole = roleMap;
                else
                {
                  roleElement.Add(new XAttribute("rdlUri", roleMap.id));
                  roleElement.Add(new XAttribute("reference", roleMap.value));
                  baseTemplateElement.Add(roleElement);
                }
                break;

              case RoleType.FixedValue:
                roleElement.Add(new XAttribute("rdlUri", roleMap.id));
                roleElement.Add(new XText(roleMap.value));
                baseTemplateElement.Add(roleElement);
                break;

              case RoleType.Property:
              case RoleType.DataProperty:
              case RoleType.ObjectProperty:
                propertyRoles.Add(roleMap);
                break;
            }
          }

          if (classRole != null)
          {
            XElement roleElement = new XElement(_appNamespace + classRole.name);
            roleElement.Add(new XAttribute("rdlUri", classRole.id));
            baseTemplateElement.Add(roleElement);
            classElement.Add(baseTemplateElement);

            string classId = classRole.classMap.id;
            ClassTemplateMap subClassTemplateMap = _graphMap.GetClassTemplateMap(classId);
            CreateHierarchicalXml(roleElement, subClassTemplateMap, dataObjectIndex);
          }
          else
          {
            List<List<XElement>> multiPropertyElements = new List<List<XElement>>();
            List<IDataObject> valueObjects = null;

            foreach (RoleMap propertyRole in propertyRoles)
            {
              List<XElement> propertyElements = new List<XElement>();
              multiPropertyElements.Add(propertyElements);

              string propertyMap = propertyRole.propertyName;
              int lastDotPos = propertyMap.LastIndexOf('.');
              string propertyName = propertyMap.Substring(lastDotPos + 1);
              string objectPath = propertyMap.Substring(0, lastDotPos);

              if (propertyMap.Split('.').Length > 2)  // related property
              {
                if (!relatedObjects.TryGetValue(objectPath, out valueObjects))
                {
                  valueObjects = GetRelatedObjects(propertyRole.propertyName, _dataObjects[dataObjectIndex]);
                  relatedObjects.Add(objectPath, valueObjects);
                }
              }
              else  // direct property
              {
                valueObjects = new List<IDataObject> { _dataObjects[dataObjectIndex] };
              }

              foreach (IDataObject valueObject in valueObjects)
              {
                string value = Convert.ToString(valueObject.GetPropertyValue(propertyName));

                XElement propertyElement = new XElement(_appNamespace + propertyRole.name);
                propertyElement.Add(new XAttribute("rdlUri", propertyRole.id));
                propertyElements.Add(propertyElement);

                if (String.IsNullOrEmpty(propertyRole.valueListName))
                {
                  if (String.IsNullOrEmpty(value))
                    propertyElement.Add(new XAttribute("reference", RDF_NIL));
                  else
                  {
                    if (propertyRole.dataType.Contains("dateTime"))
                      value = Utility.ToXsdDateTime(value);

                    propertyElement.Add(new XText(value));
                  }
                }
                else // resolve value list to uri
                {
                  value = _mapping.ResolveValueList(propertyRole.valueListName, value);

                  if (value != null)
                  {
                    value = value.Replace(RDL_NS.NamespaceName, "rdl:");
                    propertyElement.Add(new XAttribute("reference", value));
                  }
                }
              }
            }

            if (valueObjects != null)
            {
              for (int i = 0; i < valueObjects.Count; i++)
              {
                XElement templateElement = new XElement(baseTemplateElement);
                classElement.Add(templateElement);

                for (int j = 0; j < propertyRoles.Count; j++)
                {
                  templateElement.Add(multiPropertyElements[j][i]);
                }
              }
            }
          }
        }
      }
    }
    #endregion
  }
}
