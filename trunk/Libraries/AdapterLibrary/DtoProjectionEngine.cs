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
using Microsoft.ServiceModel.Web;
using System.Xml.Serialization;

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

    public override XElement ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XElement xml = null;

      try
      {
        _dataTransferObjects = ToDataTransferObjects(graphName, ref dataObjects);
        xml = SerializationExtensions.ToXml<DataTransferObjects>(_dataTransferObjects);
      }
      catch (Exception ex)
      {
        throw ex;
      }

      return xml;
    }

    public DataTransferObjects ToDataTransferObjects(string graphName, ref IList<IDataObject> dataObjects)
    {
      _dataTransferObjects = new DataTransferObjects();

      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);
        _dataObjects = dataObjects;

        if (_graphMap != null && _graphMap.classTemplateListMaps.Count > 0 &&
          _dataObjects != null && _dataObjects.Count > 0)
        {
          SetClassIdentifiers(DataDirection.InboundDto);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            DataTransferObject dto = new DataTransferObject();
            _dataTransferObjects.Add(dto);

            foreach (var pair in _graphMap.classTemplateListMaps)
            {
              ClassMap classMap = pair.Key;
              List<TemplateMap> templateMaps = pair.Value;

              ClassObject classObject = new ClassObject
              {
                classId = classMap.classId,
                name = classMap.name,
                identifier = _classIdentifiers[classMap.classId][i],
              };

              dto.classObjects.Add(classObject);

              foreach (TemplateMap templateMap in templateMaps)
              {
                TemplateObject templateObject = new TemplateObject
                {
                  templateId = templateMap.templateId,
                  name = templateMap.name,
                };

                classObject.templateObjects.Add(templateObject);

                foreach (RoleMap roleMap in templateMap.roleMaps)
                {
                  RoleObject roleObject = new RoleObject
                  {
                    type = roleMap.type,
                    roleId = roleMap.roleId,
                    name = roleMap.name,
                  };

                  templateObject.roleObjects.Add(roleObject);
                  string value = roleMap.value;

                  if (roleMap.type == RoleType.Property)
                  {
                    string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectMap.Length + 1);
                    value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                    if (!String.IsNullOrEmpty(roleMap.valueList))
                    {
                      value = _mapping.ResolveValueList(roleMap.valueList, value);

                      if (value != null)
                        value = value.Replace(RDL_NS.NamespaceName, "rdl:");
                    }
                  }
                  else if (roleMap.classMap != null)
                  {
                    value = "#" + _classIdentifiers[roleMap.classMap.classId][i];
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
        throw ex;
      }

      return _dataTransferObjects;
    }
    
    public override IList<IDataObject> ToDataObjects(string graphName, ref XElement xml)
    {
      IList<IDataObject> dataObjects = null;

      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);
        _dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(xml);

        dataObjects = ToDataObjects(graphName, ref _dataTransferObjects);
      }
      catch (Exception ex)
      {
        _logger.Error("Error projecting xml to data objects" + ex);
      }

      return dataObjects;
    }

    public IList<IDataObject> ToDataObjects(string graphName, ref DataTransferObjects dataTransferObjects)
    {
      IList<IDataObject> dataObjects = null;

      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);
        _dataTransferObjects = dataTransferObjects;

        if (_graphMap != null && _graphMap.classTemplateListMaps.Count > 0 &&
          _dataTransferObjects != null && _dataTransferObjects.Count > 0)
        {
          ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
          List<string> identifiers = new List<string>();
          for (int i = 0; i < _dataTransferObjects.Count; i++)
          {
            DataTransferObject dataTransferObject = _dataTransferObjects[i];
            ClassObject classObject = dataTransferObject.GetClassObject(classMap.classId);

            if (classObject != null)
            {
              identifiers.Add(classObject.identifier);
            }
          }

          dataObjects = _dataLayer.Create(_graphMap.dataObjectMap, identifiers);
          for (int dataTransferObjectIndex = 0; dataTransferObjectIndex < _dataTransferObjects.Count; dataTransferObjectIndex++)
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
      ClassObject classObject = _dataTransferObjects[dataTransferObjectIndex].GetClassObject(classId);

      foreach (TemplateMap templateMap in templateMaps)
      {
        TemplateObject templateObject = classObject.GetTemplateObject(templateMap);

        if (templateObject != null)
        {
          foreach (RoleMap roleMap in templateMap.roleMaps)
          {
            if (roleMap.type == RoleType.Property)
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
