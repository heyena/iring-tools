using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using System.Web;
using org.iringtools.mapping;
using Microsoft.ServiceModel.Web;

namespace org.iringtools.adapter.projection
{
  public class DtoProjectionEngine2 : BasePart7ProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DtoProjectionEngine2));
    private DataTransferObjects _dataTransferObjects;

    [Inject]
    public DtoProjectionEngine2(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping)
      : base(settings, dataLayer, mapping) { }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XDocument dtoDoc = null;

      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);

        if (_graphMap != null && _graphMap.classTemplateMaps.Count > 0 &&
          dataObjects != null && dataObjects.Count > 0)
        {
          _dataObjects = dataObjects;
          DataTransferObjects dataTransferObjects = BuildDataTransferObjects();
          string xml = Utility.SerializeDataContract<DataTransferObjects>(dataTransferObjects);
          XElement xElement = XElement.Parse(xml);
          dtoDoc = new XDocument(xElement);
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in ToXml: " + ex);
        throw ex;
      }

      return dtoDoc;
    }

    public override XDocument ToXml(string graphName, string className, string classIdentifier, ref IDataObject dataObject)
    {
      XDocument dtoDoc = null;

      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);

        if (_graphMap != null && _graphMap.classTemplateMaps.Count > 0 && dataObject != null)
        {
          _dataObjects = new List<IDataObject> { dataObject };
          DataTransferObject dto = BuildDataTransferObject(className, classIdentifier);
          string xml = Utility.SerializeDataContract<DataTransferObject>(dto);
          XElement xElement = XElement.Parse(xml);
          dtoDoc = new XDocument(xElement);
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in ToXml: " + ex);
        throw ex;
      }

      return dtoDoc;
    }

    public override IList<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument)
    {
      IList<IDataObject> dataObjects = null;

      if (xDocument != null)
      {
        try
        {
          GraphMap graphMap = _mapping.FindGraphMap(graphName);
          DataTransferObjects dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(xDocument.Root);
          dataObjects = ToDataObjects(graphMap, ref dataTransferObjects);
        }
        catch (Exception ex)
        {
          _logger.Error("Error projecting xml to data objects" + ex);
        }
      }

      return dataObjects;
    }

    public IList<IDataObject> ToDataObjects(GraphMap graphMap, ref DataTransferObjects dataTransferObjects)
    {
      _graphMap = graphMap;
      _dataTransferObjects = dataTransferObjects;
      _dataObjects = new List<IDataObject>();
      
      if (_graphMap != null && _graphMap.classTemplateMaps.Count > 0 && _dataTransferObjects != null)
      {
        ClassTemplateMap rootClassTemplateMap = _graphMap.classTemplateMaps.First();
        int objectCount = _dataTransferObjects.DataTransferObjectList.Count;
          
        if (rootClassTemplateMap != null && rootClassTemplateMap.classMap != null)
        {          
          _dataObjects = new List<IDataObject>();
          _dataRecords = new Dictionary<string, string>[objectCount];
          _relatedRecordsMaps = new Dictionary<string, List<Dictionary<string, string>>>[objectCount];
          _relatedObjectPaths = new List<string>();

          for (int dataObjectIndex = 0; dataObjectIndex < objectCount; dataObjectIndex++)
          {
            _dataRecords[dataObjectIndex] = new Dictionary<string, string>();
            _relatedRecordsMaps[dataObjectIndex] = new Dictionary<string, List<Dictionary<string, string>>>();

            ProcessInboundClass(dataObjectIndex, rootClassTemplateMap);

            //TBD: handle primary classification or composite graphs?

            IDataObject dataObject = CreateDataObject(_graphMap.dataObjectName, dataObjectIndex);
            _dataObjects.Add(dataObject);
          }

          // fill related data objects and append them to top level data objects
          if (_relatedObjectPaths != null && _relatedObjectPaths.Count > 0)
          {
            ProcessRelatedItems();
            CreateRelatedObjects();
          }
        }
      }

      return _dataObjects;
    }

    #region outbound helper methods
    private DataTransferObjects BuildDataTransferObjects()
    {
      DataTransferObjects dataTransferObjects = new DataTransferObjects();
      dataTransferObjects.ScopeName = _settings["Scope"];
      dataTransferObjects.AppName = _settings["ApplicationName"];

      ClassTemplateMap classTemplateMap = _graphMap.classTemplateMaps.First();

      if (classTemplateMap != null && classTemplateMap.classMap != null)
      {
        for (int dataObjectIndex = 0; dataObjectIndex < _dataObjects.Count; dataObjectIndex++)
        {
          DataTransferObject dto = new DataTransferObject();
          dataTransferObjects.DataTransferObjectList.Add(dto);

          bool hasRelatedProperty;
          List<string> classIdentifiers = GetClassIdentifiers(classTemplateMap.classMap, dataObjectIndex, out hasRelatedProperty);

          ProcessOutboundClass(dto, dataObjectIndex, String.Empty, String.Empty, true, classIdentifiers, hasRelatedProperty,
            classTemplateMap.classMap, classTemplateMap.templateMaps);
        }
      }

      return dataTransferObjects;
    }

    // build DTO that's rooted at className with classIdentifier
    private DataTransferObject BuildDataTransferObject(string startClassName, string startClassIdentifier)
    {
      DataTransferObject dto = new DataTransferObject();

      ClassTemplateMap classTemplateMap = _graphMap.GetClassTemplateMapByName(startClassName);

      if (classTemplateMap != null && classTemplateMap.classMap != null)
      {
        bool hasRelatedProperty;
        List<string> classIdentifiers = GetClassIdentifiers(classTemplateMap.classMap, 0, out hasRelatedProperty);

        ProcessOutboundClass(dto, 0, startClassName, startClassIdentifier, true, classIdentifiers, hasRelatedProperty,
          classTemplateMap.classMap, classTemplateMap.templateMaps);
      }

      return dto;
    }

    private void ProcessOutboundClass(DataTransferObject dto, int dataObjectIndex, string startClassName, string startClassIdentifier, bool isRootClass,
      List<string> classIdentifiers, bool hasRelatedProperty, ClassMap classMap, List<TemplateMap> templateMaps)
    {
      string className = Utility.TitleCase(classMap.name);
      string classId = classMap.id.Substring(classMap.id.IndexOf(":") + 1);

      for (int classIdentifierIndex = 0; classIdentifierIndex < classIdentifiers.Count; classIdentifierIndex++)
      {
        string classIdentifier = classIdentifiers[classIdentifierIndex];

        if (!String.IsNullOrEmpty(classIdentifier))
        {
          ClassObject classObject = new ClassObject()
          {
            classId = classMap.id,
            name = className,
            identifier = classIdentifier
          };

          if (dto.classObjects.Count == 0)
          {
            dto.identifier = classIdentifier;
          }

          //TBD: handle primary classification or composite graphs?

          dto.classObjects.Add(classObject);

          ProcessOutboundTemplates(dto, className, classIdentifier, dataObjectIndex, classObject, templateMaps,
              classIdentifier, classIdentifierIndex, hasRelatedProperty);
        }
      }
    }

    private void ProcessOutboundTemplates(DataTransferObject dto, string startClassName, string startClassIdentifier, int dataObjectIndex,
      ClassObject classObject, List<TemplateMap> templateMaps, string classIdentifier, int classIdentifierIndex,
      bool hasRelatedProperty)
    {
      if (templateMaps != null && templateMaps.Count > 0)
      {
        foreach (TemplateMap templateMap in templateMaps)
        {
          //TBD: handle secondary classification or composite graphs?

          CreateTemplateObjects(dto, dataObjectIndex, startClassName, startClassIdentifier, classIdentifier,
            classIdentifierIndex, classObject, templateMap, hasRelatedProperty);
        }
      }
    }

    private void CreateTemplateObjects(DataTransferObject dto, int dataObjectIndex, string startClassName, string startClassIdentifier,
      string classIdentifier, int classIdentifierIndex, ClassObject classObject, TemplateMap templateMap, bool classIdentifierHasRelatedProperty)
    {
      IDataObject dataObject = _dataObjects[dataObjectIndex];
      List<RoleMap> propertyRoles = new List<RoleMap>();
      List<RoleMap> classRoles = new List<RoleMap>();

      TemplateObject baseTemplateObject = new TemplateObject
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
          name = roleMap.name
        };

        switch (roleMap.type)
        {
          case RoleType.Possessor:
            baseTemplateObject.roleObjects.Add(roleObject);
            break;

          case RoleType.FixedValue:
            roleObject.dataType = roleMap.dataType;
            roleObject.value = roleMap.value;
            baseTemplateObject.roleObjects.Add(roleObject);
            break;

          case RoleType.Reference:
            if (roleMap.classMap != null)
            {
              roleObject.relatedClassId = roleMap.classMap.id;
              roleObject.relatedClassName = roleMap.classMap.name;
              classRoles.Add(roleMap);
            }
            else
            {
              string value = GetReferenceRoleValue(roleMap);
              roleObject.value = value;
            }
            baseTemplateObject.roleObjects.Add(roleObject);
            break;

          case RoleType.Property:
          case RoleType.DataProperty:
          case RoleType.ObjectProperty:
            propertyRoles.Add(roleMap);
            break;
        }
      }

      if (propertyRoles.Count > 0)  // property template
      {
        List<List<RoleObject>> multiRoleObjects = new List<List<RoleObject>>();

        // create property elements
        foreach (RoleMap propertyRole in propertyRoles)
        {
          List<RoleObject> roleObjects = new List<RoleObject>();
          multiRoleObjects.Add(roleObjects);

          string[] propertyParts = propertyRole.propertyName.Split('.');
          string propertyName = propertyParts[propertyParts.Length - 1];

          int lastDotPos = propertyRole.propertyName.LastIndexOf('.');
          string objectPath = propertyRole.propertyName.Substring(0, lastDotPos);

          if (propertyParts.Length == 2)  // direct property
          {
            string propertyValue = Convert.ToString(dataObject.GetPropertyValue(propertyName));
            propertyValue = ParsePropertyValue(propertyRole, propertyValue);

            RoleObject roleObject = new RoleObject()
            {
              type = propertyRole.type,
              roleId = propertyRole.id,
              name = propertyRole.name,
              value = propertyValue
            };

            roleObjects.Add(roleObject);
          }
          else  // related property
          {
            string key = objectPath + "." + dataObjectIndex;
            List<IDataObject> relatedObjects = null;

            if (!_relatedObjectsCache.TryGetValue(key, out relatedObjects))
            {
              relatedObjects = GetRelatedObjects(propertyRole.propertyName, dataObject);
              _relatedObjectsCache.Add(key, relatedObjects);
            }

            if (classIdentifierHasRelatedProperty)  // reference class identifier has related property
            {
              IDataObject relatedObject = relatedObjects[classIdentifierIndex];
              string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
              propertyValue = ParsePropertyValue(propertyRole, propertyValue);

              RoleObject roleObject = new RoleObject()
              {
                type = propertyRole.type,
                roleId = propertyRole.id,
                name = propertyRole.name,
                value = propertyValue
              };

              roleObjects.Add(roleObject);
            }
            else  // related property is property map
            {
              foreach (IDataObject relatedObject in relatedObjects)
              {
                string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
                propertyValue = ParsePropertyValue(propertyRole, propertyValue);

                RoleObject roleObject = new RoleObject()
                {
                  type = propertyRole.type,
                  roleId = propertyRole.id,
                  name = propertyRole.name,
                  value = propertyValue
                };

                roleObjects.Add(roleObject);
              }
            }
          }
        }

        // create template objects
        if (multiRoleObjects.Count > 0 && multiRoleObjects[0].Count > 0)
        {
          for (int i = 0; i < multiRoleObjects[0].Count; i++)
          {
            TemplateObject templateObject = Utility.CloneDataContractObject<TemplateObject>(baseTemplateObject);
            classObject.templateObjects.Add(templateObject);

            for (int j = 0; j < multiRoleObjects.Count; j++)
            {
              RoleObject roleObject = multiRoleObjects[j][i];
              templateObject.roleObjects.Add(roleObject);
            }
          }
        }
      }
      else if (classRoles.Count > 0)  // relationship template
      {
        classObject.templateObjects.Add(baseTemplateObject);

        foreach (RoleMap classRole in classRoles)
        {
          bool refClassHasRelatedProperty;
          List<string> refClassIdentifiers = GetClassIdentifiers(classRole.classMap, dataObjectIndex, out refClassHasRelatedProperty);

          if (refClassIdentifiers.Count > 0 && !String.IsNullOrEmpty(refClassIdentifiers.First()))
          {
            ClassTemplateMap relatedClassTemplateMap = _graphMap.GetClassTemplateMap(classRole.classMap.id);

            if (relatedClassTemplateMap != null && relatedClassTemplateMap.classMap != null)
            {
              ProcessOutboundClass(dto, dataObjectIndex, startClassName, startClassIdentifier, false, refClassIdentifiers,
                refClassHasRelatedProperty, relatedClassTemplateMap.classMap, relatedClassTemplateMap.templateMaps);
            }
            else  // reference class not found (leaf node role map), treat it as "data property" role with value
            {
              foreach (RoleObject roleObject in baseTemplateObject.roleObjects)
              {
                if (roleObject.relatedClassId == classRole.classMap.id)
                {
                  roleObject.value = refClassIdentifiers.First();
                  break;
                }
              }
            }
          }
        }
      }
    }

    private string ParsePropertyValue(RoleMap propertyRole, string propertyValue)
    {
      string value = propertyValue;

      if (String.IsNullOrEmpty(propertyRole.valueListName))
      {
        if (propertyRole.dataType.ToLower().Contains("datetime"))
          value = Utility.ToXsdDateTime(propertyValue);
      }
      else  // resolve value list to uri
      {
        value = _mapping.ResolveValueList(propertyRole.valueListName, propertyValue);
      }

      return value;
    }

    private string GetReferenceRoleValue(RoleMap referenceRole)
    {
      string value = referenceRole.value;

      if (!String.IsNullOrEmpty(referenceRole.valueListName))
      {
        value = _mapping.ResolveValueList(referenceRole.valueListName, value);
      }

      return value;
    }
    #endregion

    #region inbound helper methods
    private void ProcessInboundClass(int dataObjectIndex, ClassTemplateMap classTemplateMap)
    {
      ClassMap classMap = classTemplateMap.classMap;
      List<TemplateMap> templateMaps = classTemplateMap.templateMaps;
      List<ClassObject> classObjects = GetClassObjects(dataObjectIndex, classMap.id);

      for (int classObjectIndex = 0; classObjectIndex < classObjects.Count; classObjectIndex++)
      {
        ClassObject classObject = classObjects[classObjectIndex];
        string identifier = classObject.identifier;

        string[] identifierParts = !String.IsNullOrEmpty(classMap.identifierDelimiter)
          ? identifier.Split(new string[] { classMap.identifierDelimiter }, StringSplitOptions.None)
          : new string[] { identifier };

        for (int identifierPartIndex = 0; identifierPartIndex < identifierParts.Length; identifierPartIndex++)
        {
          string identifierPart = identifierParts[identifierPartIndex];

          // remove fixed values from identifier
          foreach (string classIdentifier in classMap.identifiers)
          {
            if (classIdentifier.StartsWith("#") && classIdentifier.EndsWith("#"))
            {
              identifierPart = identifierPart.Replace(classIdentifier.Substring(1, classIdentifier.Length - 2), "");
            }
          }

          // set identifier value to mapped property
          foreach (string classIdentifier in classMap.identifiers)
          {
            if (classIdentifier.Split('.').Length > 2)  // related property
            {
              SetRelatedRecords(dataObjectIndex, classObjectIndex, classIdentifier, new List<string> { identifierPart });
            }
            else  // direct property
            {
              _dataRecords[dataObjectIndex][classIdentifier.Substring(classIdentifier.LastIndexOf('.') + 1)] = identifierPart;
            }
          }
        }

        if (templateMaps != null && templateMaps.Count > 0)
        {
          ProcessInboundTemplates(dataObjectIndex, classObject, classObjectIndex, templateMaps);
        }
      }
    }

    private void ProcessInboundTemplates(int dataObjectIndex, ClassObject classObject, int classObjectIndex, List<TemplateMap> templateMaps)
    {
      foreach (TemplateMap templateMap in templateMaps)
      {
        TemplateObject templateObject = classObject.GetTemplateObject(templateMap);
        
        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          switch (roleMap.type)
          {
            case RoleType.Reference:
              if (roleMap.classMap != null)
              {
                ClassTemplateMap classTemplateMap = _graphMap.GetClassTemplateMap(roleMap.classMap.id);

                if (classTemplateMap != null && classTemplateMap.classMap != null)
                {
                  ProcessInboundClass(dataObjectIndex, classTemplateMap);
                }
                else  // reference class not found, treat this reference role as property role
                {
                  roleMap.propertyName = roleMap.classMap.identifiers.First();
                  ProcessInboundPropertyRole(dataObjectIndex, classObjectIndex, roleMap, templateObject);
                }
              }
              break;

            case RoleType.Property:
            case RoleType.DataProperty:
            case RoleType.ObjectProperty:
              ProcessInboundPropertyRole(dataObjectIndex, classObjectIndex, roleMap, templateObject);
              break;
          }
        }
      }
    }

    private void ProcessInboundPropertyRole(int dataObjectIndex, int classObjectIndex, RoleMap roleMap, TemplateObject templateObject)
    {
      string[] propertyPath = roleMap.propertyName.Split('.');
      string propertyName = propertyPath[propertyPath.Length - 1];
      List<string> values = new List<string>();

      foreach (RoleObject roleObject in templateObject.roleObjects)
      {
        if (roleObject.roleId == roleMap.id)
        {
          string value = roleObject.value;

          if (!String.IsNullOrEmpty(roleMap.valueListName))
          {
            value = _mapping.ResolveValueMap(roleMap.valueListName, value);
          }

          if (propertyPath.Length > 2)  // related property
          {
            values.Add(value);
          }
          else  // direct property
          {
            _dataRecords[dataObjectIndex][propertyName] = value;
          }

          break;
        }
      }

      if (propertyPath.Length > 2 && values.Count > 0)
      {
        SetRelatedRecords(dataObjectIndex, classObjectIndex, roleMap.propertyName, values);
      }
    }

    private List<ClassObject> GetClassObjects(int dataObjectIndex, string classId)
    {
      List<ClassObject> classObjects = new List<ClassObject>();
      DataTransferObject dto = _dataTransferObjects.DataTransferObjectList[dataObjectIndex];

      foreach (ClassObject classObject in dto.classObjects)
      {
        if (classObject.classId == classId)
        {
          classObjects.Add(classObject);
        }
      }

      return classObjects;
    }
    #endregion
  }
}