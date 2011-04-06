﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Xml.Linq;
using log4net;
using Microsoft.ServiceModel.Web;
using Ninject;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Storage;
using org.iringtools.utility;

namespace org.iringtools.adapter.projection
{
  public class DtoProjectionEngine : BasePart7ProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DtoProjectionEngine));

    private DataTransferObjects _dataTransferObjects;

    [Inject]
    public DtoProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping)
      : base(settings, dataLayer, mapping)
    {
      _classIdentifiers = new Dictionary<string, List<string>>();
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XDocument xDocument = null;

      try
      {
        GraphMap graphMap = _mapping.FindGraphMap(graphName);
        DataTransferObjects dataTransferObjects = ToDataTransferObjects(graphMap, ref dataObjects);
        string xml = Utility.SerializeDataContract<DataTransferObjects>(dataTransferObjects);
        XElement xElement = XElement.Parse(xml);
        xDocument = new XDocument(xElement);
      }
      catch (Exception ex)
      {
        _logger.Error("Error projecting data objects to xml." + ex);
      }

      return xDocument;
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects, string className, string classIdentifier)
    {
      //TODO: need to update to use className
      return ToXml(graphName, ref dataObjects);
    }

    public DataTransferObjects ToDataTransferObjects(GraphMap graphMap, ref IList<IDataObject> dataObjects)
    {
      _dataTransferObjects = new DataTransferObjects()
	    {
        ScopeName = _settings["ProjectName"],
        AppName = _settings["ApplicationName"],
      };

      List<DataTransferObject> dataTransferObjectList = _dataTransferObjects.DataTransferObjectList;

      try
      {
        _graphMap = graphMap;
        _dataObjects = dataObjects;

        if (_graphMap != null && _graphMap.classTemplateListMaps.Count > 0 &&
          _dataObjects != null && _dataObjects.Count > 0)
        {
          SetClassIdentifiers(DataDirection.InboundDto);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            DataTransferObject dto = new DataTransferObject();
            _dataTransferObjects.DataTransferObjectList.Add(dto);

            foreach (var pair in _graphMap.classTemplateListMaps)
            {
              ClassMap classMap = pair.Key;
              List<TemplateMap> templateMaps = pair.Value;
              string classIdentifier = _classIdentifiers[classMap.classId][i];

              if (!String.IsNullOrEmpty(classIdentifier))
              {
                ClassObject classObject = new ClassObject
                {
                  classId = classMap.classId,
                  name = classMap.name,
                  identifier = classIdentifier,
                };

                if (dto.classObjects.Count == 0)
                  dto.identifier = classIdentifier;

                dto.classObjects.Add(classObject);

                foreach (TemplateMap templateMap in templateMaps)
                {
                  bool isTemplateValid = true;

                  TemplateObject templateObject = new TemplateObject
                  {
                    templateId = templateMap.templateId,
                    name = templateMap.name,
                  };

                  foreach (RoleMap roleMap in templateMap.roleMaps)
                  {
                    RoleObject roleObject = new RoleObject
                    {
                      type = roleMap.type,
                      roleId = roleMap.roleId,
                      name = roleMap.name,
                      dataType = roleMap.dataType,
                      hasValueMap = false
                    };

                    templateObject.roleObjects.Add(roleObject);
                    string value = roleMap.value;

                    if (
                      roleMap.type == RoleType.Property ||
                      roleMap.type == RoleType.DataProperty ||
                      roleMap.type == RoleType.ObjectProperty
                      )
                    {
                      string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectMap.Length + 1);
                      value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                      if (!String.IsNullOrEmpty(roleMap.valueList))
                      {
                        roleObject.hasValueMap = true;
                        value = _mapping.ResolveValueList(roleMap.valueList, value);

                        if (value == RDF_NIL)
                          value = String.Empty;
                      }
                      else if (roleMap.dataType.Contains("dateTime"))
                      {
                        value = Utility.ToXsdDateTime(value);
                      }
                    }
                    else if (roleMap.classMap != null)
                    {
                      roleObject.relatedClassId = roleMap.classMap.classId;
                      roleObject.relatedClassName = roleMap.classMap.name;

                      if (!String.IsNullOrEmpty(_classIdentifiers[roleMap.classMap.classId][i]))
                        value = "#" + _classIdentifiers[roleMap.classMap.classId][i];
                      else
                        isTemplateValid = false;
                    }

                    roleObject.value = value;
                  }

                  if (isTemplateValid)
                  {
                    classObject.templateObjects.Add(templateObject);
                  }
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error projecting data objects to data transfer objects." + ex);
      }

      return _dataTransferObjects;
    }

    public override IList<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument)
    {
      IList<IDataObject> dataObjects = null;

      try
      {
        GraphMap graphMap = _mapping.FindGraphMap(graphName);
        string xml = xDocument.Root.ToString();
        DataTransferObjects dataTransferObjects = Utility.DeserializeDataContract<DataTransferObjects>(xml);

        dataObjects = ToDataObjects(graphMap, ref dataTransferObjects);
      }
      catch (Exception ex)
      {
        _logger.Error("Error projecting xml to data objects" + ex);
      }

      return dataObjects;
    }

    public IList<IDataObject> ToDataObjects(GraphMap graphMap, ref DataTransferObjects dataTransferObjects)
    {
      IList<IDataObject> dataObjects = null;

      try
      {
        _graphMap = graphMap;
        _dataTransferObjects = dataTransferObjects;

        if (_graphMap != null && _graphMap.classTemplateListMaps.Count > 0 &&
          _dataTransferObjects != null && _dataTransferObjects.DataTransferObjectList.Count > 0)
        {
          ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
          List<string> identifiers = new List<string>();

          for (int i = 0; i < _dataTransferObjects.DataTransferObjectList.Count; i++)
          {
            DataTransferObject dataTransferObject = _dataTransferObjects.DataTransferObjectList[i];
            ClassObject classObject = dataTransferObject.GetClassObject(classMap.classId);

            // These should be the same, but if they are different then
            // the sender has modified the DTO and intends for this to be
            // used instead.
            if (dataTransferObject.identifier != classObject.identifier)
            {
              identifiers.Add(dataTransferObject.identifier);
            }
            else if (classObject != null)
            {
              identifiers.Add(classObject.identifier);
            }
          }

          dataObjects = _dataLayer.Create(_graphMap.dataObjectMap, identifiers);

          for (int dataTransferObjectIndex = 0; dataTransferObjectIndex < _dataTransferObjects.DataTransferObjectList.Count; dataTransferObjectIndex++)
          {
            IDataObject dataObject = dataObjects[dataTransferObjectIndex];
            CreateDataObjects(ref dataObject, classMap.classId, dataTransferObjectIndex);
          }

          return dataObjects;
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error projecting data transfer objects to data objects" + ex);
      }

      return dataObjects;
    }

    private void CreateDataObjects(ref IDataObject dataObject, string classId, int dataTransferObjectIndex)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
      List<TemplateMap> templateMaps = classTemplateListMap.Value;
      ClassObject classObject = _dataTransferObjects.DataTransferObjectList[dataTransferObjectIndex].GetClassObject(classId);

      foreach (TemplateMap templateMap in templateMaps)
      {
        TemplateObject templateObject = classObject.GetTemplateObject(templateMap);

        if (templateObject != null)
        {
          foreach (RoleMap roleMap in templateMap.roleMaps)
          {
            if (
              roleMap.type == RoleType.Property ||
              roleMap.type == RoleType.DataProperty ||
              roleMap.type == RoleType.ObjectProperty
              )
            {
              string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectMap.Length + 1);

              foreach (RoleObject roleObject in templateObject.roleObjects)
              {
                if (roleObject.roleId == roleMap.roleId)
                {
                  string value = roleObject.value;

                  if (!String.IsNullOrEmpty(roleMap.valueList))
                  {
                    value = _mapping.ResolveValueMap(roleMap.valueList, value);
                  }

                  dataObject.SetPropertyValue(propertyName, value);
                }
              }
            }

            if (roleMap.classMap != null)
            {
              CreateDataObjects(ref dataObject, roleMap.classMap.classId, dataTransferObjectIndex);
            }
          }
        }
      }
    }
  }
}
