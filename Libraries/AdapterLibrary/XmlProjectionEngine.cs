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
using System.Web;

namespace org.iringtools.adapter.projection
{
  public class XmlProjectionEngine : BasePart7ProjectionEngine
  {    
    private static readonly ILog _logger = LogManager.GetLogger(typeof(XmlProjectionEngine));
    private Dictionary<string, List<string>> _classIdentifiersCache = null;
    private DataDictionary _dictionary = null;
    private XNamespace _appNamespace = null;
    
    [Inject]
    public XmlProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping, DataDictionary dictionary)
    {
      _settings = settings;
      _dataLayer = dataLayer;
      _dictionary = dictionary;
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

        xElement = new XElement(_appNamespace + Utility.TitleCase(graphName),
                   new XAttribute(XNamespace.Xmlns + "i", XSI_NS),
                   new XAttribute(XNamespace.Xmlns + "rdl", RDL_NS),
                   new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

        if (_graphMap != null && _graphMap.classTemplateListMaps.Count > 0 &&
          _dataObjects != null && _dataObjects.Count == 1)
        {
          _classIdentifiersCache = new Dictionary<string, List<string>>();
          SetClassIdentifiers(DataDirection.Outbound);

          var pair = _graphMap.classTemplateListMaps.First();          
          for (int i = 0; i < _dataObjects.Count; i++)
          {
            CreateHierarchicalXml(xElement, pair, i);
          }
        }
        if (_dataObjects != null && _dataObjects.Count > 1)
        {
          xElement = new XElement(_appNamespace + Utility.TitleCase(graphName));

          var pair = _graphMap.classTemplateListMaps.First(); 

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            XElement rowElement = new XElement(_appNamespace + Utility.TitleCase(pair.Key.name));
            CreateIndexXml(rowElement, pair, i);
            xElement.Add(rowElement);
          }

          XAttribute total = new XAttribute("total", this.Count);
          xElement.Add(total);
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
    private void CreateIndexXml(XElement parentElement, KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap, int dataObjectIndex)
    {
      string uri = _appNamespace.ToString() + "/" + _graphMap.name + "/";

      foreach (string keyPropertyName in classTemplateListMap.Key.identifiers)
      {
        RoleMap roleMap = null;
        foreach(TemplateMap templateMap in classTemplateListMap.Value)
        {
          roleMap = templateMap.roleMaps.Find(rm => rm.propertyName == keyPropertyName);
          if (roleMap != null) break;
        }

        if (roleMap != null)
        {
          string[] propertyNameParts = roleMap.propertyName.Split('.');
          string propertyName = String.Empty;

          if (propertyNameParts.Count() > 1)
          {
            propertyName = propertyNameParts[1];
          }
          else
          {
            propertyName = propertyNameParts[0];
          }

          var value = _dataObjects[dataObjectIndex].GetPropertyValue(propertyName);
          if (value != null)
            uri += classTemplateListMap.Key.identifierDelimiter + value;
        }        
      }

      parentElement.Value = uri;
    }

    private void CreateHierarchicalXml(XElement parentElement, KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap, int dataObjectIndex)
    {
      ClassMap classMap = classTemplateListMap.Key;
      List<TemplateMap> templateMaps = classTemplateListMap.Value;
      string classIdentifier = _classIdentifiers[classMap.classId][dataObjectIndex];

      XElement classElement = new XElement(_appNamespace + Utility.TitleCase(classMap.name));
      classElement.Add(new XAttribute("rdlUri", classMap.classId));
      parentElement.Add(classElement);
      bool classExists = false;

      if (_classIdentifiersCache.ContainsKey(classMap.classId))
      {
        List<string> classIdentifiers = _classIdentifiersCache[classMap.classId];

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
        _classIdentifiersCache[classMap.classId] = new List<string> { classIdentifier };
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
          baseTemplateElement.Add(new XAttribute("rdlUri", templateMap.templateId));

          foreach (RoleMap roleMap in templateMap.roleMaps)
          {
            XElement roleElement = new XElement(_appNamespace + roleMap.name);

            switch (roleMap.type)
            {
              case RoleType.Possessor:
                baseTemplateElement.Add(new XAttribute("possessorRole", roleMap.roleId));
                break;

              case RoleType.Reference:
                if (roleMap.classMap != null)
                  classRole = roleMap;
                else
                {
                  roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
                  roleElement.Add(new XAttribute("reference", roleMap.value));
                  baseTemplateElement.Add(roleElement);
                }
                break;

              case RoleType.FixedValue:
                roleElement.Add(new XAttribute("rdlUri", roleMap.roleId));
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
            roleElement.Add(new XAttribute("rdlUri", classRole.roleId));
            baseTemplateElement.Add(roleElement);
            classElement.Add(baseTemplateElement);

            string classId = classRole.classMap.classId;
            KeyValuePair<ClassMap, List<TemplateMap>> subClassTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
            CreateHierarchicalXml(roleElement, subClassTemplateListMap, dataObjectIndex);
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
                propertyElement.Add(new XAttribute("rdlUri", propertyRole.roleId));
                propertyElements.Add(propertyElement);

                if (String.IsNullOrEmpty(propertyRole.valueList))
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
                  value = _mapping.ResolveValueList(propertyRole.valueList, value);

                  if (value != null)
                  {
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

    public DataObject FindGraphDataObject(string dataObjectName)
    {
      foreach (DataObject dataObject in _dictionary.dataObjects)
      {
        if (dataObject.objectName.ToLower() == dataObjectName.ToLower())
        {
          return dataObject;
        }
      }

      throw new Exception("DataObject [" + dataObjectName + "] does not exist.");
    }
    #endregion
  }
}
