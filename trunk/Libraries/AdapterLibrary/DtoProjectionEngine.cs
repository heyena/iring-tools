using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using org.iringtools.common.mapping;
using System.Xml.Linq;
using Ninject;
using log4net;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Storage;
using org.iringtools.utility;
using Microsoft.ServiceModel.Web;
using System.Xml.Serialization;
using org.iringtools.library.manifest;

namespace org.iringtools.adapter.projection
{
  public class DtoProjectionEngine : BaseProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DtoProjectionEngine));

    private DataTransferObjects _dataTransferObjects;

    [Inject]
    public DtoProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping)
    {
      _dataObjects = new List<IDataObject>();
      _classIdentifiers = new Dictionary<string, List<string>>();

      _dataLayer = dataLayer;
      _mapping = mapping;
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XDocument xDocument = null;

      List<DataTransferObject> dataTransferObjectList = _dataTransferObjects.DataTransferObjectList;

      try
      {
        GraphMap graphMap = _mapping.FindGraphMap(graphName);
        DataTransferObjects dataTransferObjects = ToDataTransferObjects(graphMap, ref dataObjects);
        dataTransferObjectList = dataTransferObjects.DataTransferObjectList;
        XElement xElement = SerializationExtensions.ToXml<DataTransferObjects>(dataTransferObjects);
        xDocument = new XDocument(xElement);
      }
      catch (Exception ex)
      {
        _logger.Error("Error projecting data objects to xml." + ex);
      }

      return xDocument;
    }

    public DataTransferObjects ToDataTransferObjects(GraphMap graphMap, ref IList<IDataObject> dataObjects)
    {
      _dataTransferObjects = new DataTransferObjects();

      List<DataTransferObject> dataTransferObjectList = _dataTransferObjects.DataTransferObjectList;

      try
      {
        _graphMap = graphMap;
        _dataObjects = dataObjects;

        if (_graphMap != null && _graphMap.ClassTemplateMaps.Count > 0 &&
          _dataObjects != null && _dataObjects.Count > 0)
        {
          SetClassIdentifiers(DataDirection.InboundDto);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            DataTransferObject dto = new DataTransferObject();
            dataTransferObjectList.Add(dto);

            foreach (ClassTemplateMap classTemplateMap in _graphMap.ClassTemplateMaps)
            {
              ClassMap classMap = classTemplateMap.ClassMap;
              List<TemplateMap> templateMaps = classTemplateMap.TemplateMaps;

              ClassObject classObject = new ClassObject
              {
                classId = classMap.ClassId,
                name = classMap.Name,
                identifier = _classIdentifiers[classMap.ClassId][i],
              };

              dto.classObjects.Add(classObject);

              foreach (TemplateMap templateMap in templateMaps)
              {
                TemplateObject templateObject = new TemplateObject
                {
                  templateId = templateMap.TemplateId,
                  name = templateMap.Name,
                };

                classObject.templateObjects.Add(templateObject);

                foreach (RoleMap roleMap in templateMap.RoleMaps)
                {
                  RoleObject roleObject = new RoleObject
                  {
                    type = roleMap.Type,
                    roleId = roleMap.RoleId,
                    name = roleMap.Name,
                    dataType = roleMap.DataType
                  };

                  templateObject.roleObjects.Add(roleObject);
                  string value = roleMap.Value;

                  if (
                    roleMap.Type == RoleType.Property ||
                    roleMap.Type == RoleType.DataProperty ||
                    roleMap.Type == RoleType.ObjectProperty
                    )
                  {
                    string propertyName = roleMap.PropertyName.Substring(_graphMap.DataObjectName.Length + 1);
                    value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                    if (!String.IsNullOrEmpty(roleMap.ValueListName))
                    {
                      value = _mapping.ResolveValueList(roleMap.ValueListName, value);

                      if (value != null)
                        value = value.Replace(RDL_NS.NamespaceName, "rdl:");
                    }
                    else if (roleMap.DataType.Contains("dateTime"))
                    {
                      value = Utility.ToXsdDateTime(value);
                    }
                  }
                  else if (roleMap.ClassMap != null)
                  {
                    value = "#" + _classIdentifiers[roleMap.ClassMap.ClassId][i];
                  }

                  roleObject.value = value;
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

      _dataTransferObjects.DataTransferObjectList = dataTransferObjectList;

      return _dataTransferObjects;
    }

    public override IList<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument)
    {
      IList<IDataObject> dataObjects = null;

      try
      {
        GraphMap graphMap = _mapping.FindGraphMap(graphName);
        _dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(xDocument.Root);

        dataObjects = ToDataObjects(graphMap, ref _dataTransferObjects);
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
        List<DataTransferObject> dataTransferObjectList = _dataTransferObjects.DataTransferObjectList;

        if (_graphMap != null && _graphMap.ClassTemplateMaps.Count > 0 &&
          dataTransferObjectList != null && dataTransferObjectList.Count > 0)
        {
          ClassMap classMap = _graphMap.ClassTemplateMaps.First().ClassMap;
          List<string> identifiers = new List<string>();
          for (int i = 0; i < dataTransferObjectList.Count; i++)
          {
            DataTransferObject dataTransferObject = dataTransferObjectList[i];
            ClassObject classObject = dataTransferObject.GetClassObject(classMap.ClassId);

            if (classObject != null)
            {
              identifiers.Add(classObject.identifier);
            }
          }

          dataObjects = _dataLayer.Create(_graphMap.DataObjectName, identifiers);
          for (int dataTransferObjectIndex = 0; dataTransferObjectIndex < dataTransferObjectList.Count; dataTransferObjectIndex++)
          {
            IDataObject dataObject = dataObjects[dataTransferObjectIndex];
            CreateDataObjects(ref dataObject, classMap.ClassId, dataTransferObjectIndex);
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
      List<DataTransferObject> dataTransferObjectList = _dataTransferObjects.DataTransferObjectList;
      ClassTemplateMap classTemplateListMap = _graphMap.GetClassTemplateMap(classId);
      List<TemplateMap> templateMaps = classTemplateListMap.TemplateMaps;
      ClassObject classObject = dataTransferObjectList[dataTransferObjectIndex].GetClassObject(classId);

      foreach (TemplateMap templateMap in templateMaps)
      {
        TemplateObject templateObject = classObject.GetTemplateObject(templateMap);

        if (templateObject != null)
        {
          foreach (RoleMap roleMap in templateMap.RoleMaps)
          {
            if (
              roleMap.Type == RoleType.Property ||
              roleMap.Type == RoleType.DataProperty ||
              roleMap.Type == RoleType.ObjectProperty
              )
            {
              string propertyName = roleMap.PropertyName.Substring(_graphMap.DataObjectName.Length + 1);

              foreach (RoleObject roleObject in templateObject.roleObjects)
              {
                if (roleObject.roleId == roleMap.RoleId)
                {
                  string value = roleObject.value;

                  if (!String.IsNullOrEmpty(roleMap.ValueListName))
                  {
                    value = _mapping.ResolveValueMap(roleMap.ValueListName, value);
                  }

                  dataObject.SetPropertyValue(propertyName, value);
                }
              }
            }

            if (roleMap.ClassMap != null)
            {
              CreateDataObjects(ref dataObject, roleMap.ClassMap.ClassId, dataTransferObjectIndex);
            }
          }
        }
      }
    }
  }
}
