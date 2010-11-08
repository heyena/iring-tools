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
using org.iringtools.common.mapping;
using org.iringtools.protocol.manifest;
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
           _settings["GraphBaseUri"],
           HttpUtility.UrlEncode(_settings["ProjectName"]),
           HttpUtility.UrlEncode(_settings["ApplicationName"])
         );

        _graphMap = _mapping.FindGraphMap(graphName);
        _dataObjects = dataObjects;

        if (_graphMap != null && _graphMap.ClassTemplateMaps.Count > 0 &&
          _dataObjects != null && _dataObjects.Count > 0)
        {
          _classIdentifiersCache = new Dictionary<string, List<string>>();
          SetClassIdentifiers(DataDirection.Outbound);

          //TODO: use entities for rdl & tpl instead of namespaces
          xElement = new XElement(_appNamespace + Utility.TitleCase(graphName),
            new XAttribute(XNamespace.Xmlns + "i", XSI_NS),
            new XAttribute(XNamespace.Xmlns + "rdl", RDL_NS),
            new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

          ClassTemplateMap classTemplateMap = _graphMap.ClassTemplateMaps.First();          
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
    private void CreateHierarchicalXml(XElement parentElement,  ClassTemplateMap classTemplateMap, int dataObjectIndex)
    {
      ClassMap classMap = classTemplateMap.ClassMap;
      List<TemplateMap> templateMaps = classTemplateMap.TemplateMaps;
      string classIdentifier = _classIdentifiers[classMap.ClassId][dataObjectIndex];

      XElement classElement = new XElement(_appNamespace + Utility.TitleCase(classMap.Name));
      classElement.Add(new XAttribute("rdlUri", classMap.ClassId));
      parentElement.Add(classElement);
      bool classExists = false;

      if (_classIdentifiersCache.ContainsKey(classMap.ClassId))
      {
        List<string> classIdentifiers = _classIdentifiersCache[classMap.ClassId];

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
        _classIdentifiersCache[classMap.ClassId] = new List<string> { classIdentifier };
        classElement.Add(new XAttribute("id", classIdentifier));
      }

      if (!classExists)
      {
        foreach (TemplateMap templateMap in templateMaps)
        {
          Dictionary<string, List<IDataObject>> relatedObjects = new Dictionary<string, List<IDataObject>>();
          List<RoleMap> propertyRoles = new List<RoleMap>();
          RoleMap classRole = null;

          XElement baseTemplateElement = new XElement(_appNamespace + templateMap.Name);
          baseTemplateElement.Add(new XAttribute("rdlUri", templateMap.TemplateId));

          foreach (RoleMap roleMap in templateMap.RoleMaps)
          {
            XElement roleElement = new XElement(_appNamespace + roleMap.Name);

            switch (roleMap.Type)
            {
              case RoleType.Possessor:
                baseTemplateElement.Add(new XAttribute("possessorRole", roleMap.RoleId));
                break;

              case RoleType.Reference:
                if (roleMap.ClassMap != null)
                  classRole = roleMap;
                else
                {
                  roleElement.Add(new XAttribute("rdlUri", roleMap.RoleId));
                  roleElement.Add(new XAttribute("reference", roleMap.Value));
                  baseTemplateElement.Add(roleElement);
                }
                break;

              case RoleType.FixedValue:
                roleElement.Add(new XAttribute("rdlUri", roleMap.RoleId));
                roleElement.Add(new XText(roleMap.Value));
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
            XElement roleElement = new XElement(_appNamespace + classRole.Name);
            roleElement.Add(new XAttribute("rdlUri", classRole.RoleId));
            baseTemplateElement.Add(roleElement);
            classElement.Add(baseTemplateElement);

            string classId = classRole.ClassMap.ClassId;
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

              string propertyMap = propertyRole.PropertyName;
              int lastDotPos = propertyMap.LastIndexOf('.');
              string propertyName = propertyMap.Substring(lastDotPos + 1);
              string objectPath = propertyMap.Substring(0, lastDotPos);

              if (propertyMap.Split('.').Length > 2)  // related property
              {
                if (!relatedObjects.TryGetValue(objectPath, out valueObjects))
                {
                  valueObjects = GetRelatedObjects(propertyRole.PropertyName, _dataObjects[dataObjectIndex]);
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

                XElement propertyElement = new XElement(_appNamespace + propertyRole.Name);
                propertyElement.Add(new XAttribute("rdlUri", propertyRole.RoleId));
                propertyElements.Add(propertyElement);

                if (String.IsNullOrEmpty(propertyRole.ValueListName))
                {
                  if (String.IsNullOrEmpty(value))
                    propertyElement.Add(new XAttribute("reference", RDF_NIL));
                  else
                  {
                    if (propertyRole.DataType.Contains("dateTime"))
                      value = Utility.ToXsdDateTime(value);

                    propertyElement.Add(new XText(value));
                  }
                }
                else // resolve value list to uri
                {
                  value = _mapping.ResolveValueList(propertyRole.ValueListName, value);

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
