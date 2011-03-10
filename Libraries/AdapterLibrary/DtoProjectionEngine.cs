using System;
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
using org.iringtools.mapping;

namespace org.iringtools.adapter.projection
{
  public class DtoProjectionEngine : BasePart7ProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DtoProjectionEngine));

    private DataTransferObjects _dataTransferObjects;
    private string _scopeName;
    private string _appName;

    [Inject]
    public DtoProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping)
    {
      _dataObjects = new List<IDataObject>();
      _classIdentifiers = new Dictionary<string, List<string>>();

      _scopeName = settings["ProjectName"];
      _appName = settings["ApplicationName"];
      _dataLayer = dataLayer;
      _mapping = mapping;
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

    public override XDocument ToXml(string graphName, string className, string classIdentifier, ref IDataObject dataObject)
    {
      //TODO: need to update to use className
      IList<IDataObject> dataObjects = new List<IDataObject> { dataObject };
      return ToXml(graphName, ref dataObjects);
    }

    public DataTransferObjects ToDataTransferObjects(GraphMap graphMap, ref IList<IDataObject> dataObjects)
    {
      _dataTransferObjects = new DataTransferObjects()
	  {
        ScopeName = _scopeName,
        AppName = _appName,
      };

      List<DataTransferObject> dataTransferObjectList = _dataTransferObjects.DataTransferObjectList;

      try
      {
        _graphMap = graphMap;
        _dataObjects = dataObjects;

        if (_graphMap != null && _graphMap.classTemplateMaps.Count > 0 &&
          _dataObjects != null && _dataObjects.Count > 0)
        {
          SetClassIdentifiers(DataDirection.InboundDto);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            DataTransferObject dto = new DataTransferObject();
            dataTransferObjectList.Add(dto);

            foreach (ClassTemplateMap classTemplateMap in _graphMap.classTemplateMaps)
            {
              ClassMap classMap = classTemplateMap.classMap;
              List<TemplateMap> templateMaps = classTemplateMap.templateMaps;
              string classIdentifier = _classIdentifiers[classMap.id][i];

              if (!String.IsNullOrEmpty(classIdentifier))
              {
                ClassObject classObject = new ClassObject
                {
                  classId = classMap.id,
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
                    templateId = templateMap.id,
                    name = templateMap.name,
                  };

                  foreach (RoleMap roleMap in templateMap.roleMaps)
                  {
                    RoleObject roleObject = new RoleObject
                    {
                      type = roleMap.type,
                      roleId = roleMap.id,
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
                      string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectName.Length + 1);
                      value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                      if (!String.IsNullOrEmpty(roleMap.valueListName))
                      {
                        roleObject.hasValueMap = true;
                        value = _mapping.ResolveValueList(roleMap.valueListName, value);

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
                      roleObject.relatedClassId = roleMap.classMap.id;
                      roleObject.relatedClassName = roleMap.classMap.name;

                      if (!String.IsNullOrEmpty(_classIdentifiers[roleMap.classMap.id][i]))
                      {
                        value = "#" + _classIdentifiers[roleMap.classMap.id][i];
                      }
                      else
                      {
                        isTemplateValid = false;
                      }
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

        if (_graphMap != null && _graphMap.classTemplateMaps.Count > 0 &&
          dataTransferObjectList != null && dataTransferObjectList.Count > 0)
        {
          ClassMap classMap = _graphMap.classTemplateMaps[0].classMap;
          List<string> identifiers = new List<string>();
          for (int i = 0; i < dataTransferObjectList.Count; i++)
          {
            DataTransferObject dataTransferObject = dataTransferObjectList[i];
            ClassObject classObject = dataTransferObject.GetClassObject(classMap.id);

            if (dataTransferObject.identifier != classObject.identifier)
            {
              identifiers.Add(dataTransferObject.identifier);
            }
            else if (classObject != null)
            {
              identifiers.Add(classObject.identifier);
            }
          }

          dataObjects = _dataLayer.Create(_graphMap.dataObjectName, identifiers);
          for (int dataTransferObjectIndex = 0; dataTransferObjectIndex < dataTransferObjectList.Count; dataTransferObjectIndex++)
          {
            IDataObject dataObject = dataObjects[dataTransferObjectIndex];
            CreateDataObjects(ref dataObject, classMap.id, dataTransferObjectIndex);
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
      List<TemplateMap> templateMaps = classTemplateListMap.templateMaps;
      ClassObject classObject = dataTransferObjectList[dataTransferObjectIndex].GetClassObject(classId);

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
              string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectName.Length + 1);

              foreach (RoleObject roleObject in templateObject.roleObjects)
              {
                if (roleObject.roleId == roleMap.id)
                {
                  string value = roleObject.value;

                  if (!String.IsNullOrEmpty(roleMap.valueListName))
                  {
                    value = _mapping.ResolveValueMap(roleMap.valueListName, value);
                  }

                  dataObject.SetPropertyValue(propertyName, value);
                }
              }
            }

            if (roleMap.classMap != null)
            {
              CreateDataObjects(ref dataObject, roleMap.classMap.id, dataTransferObjectIndex);
            }
          }
        }
      }
    }
  }
}
