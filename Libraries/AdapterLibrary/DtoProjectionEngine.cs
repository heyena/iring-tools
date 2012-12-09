using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using log4net;
using Microsoft.ServiceModel.Web;
using Ninject;
using org.iringtools.library;
using org.iringtools.mapping;
using org.iringtools.utility;
using System.Text;

namespace org.iringtools.adapter.projection
{
  public class DtoProjectionEngine : BasePart7ProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DtoProjectionEngine));
    private DataTransferObjects _dataTransferObjects;

    [Inject]
    public DtoProjectionEngine(AdapterSettings settings, IDataLayer2 dataLayer, Mapping mapping)
      : base(settings, dataLayer, mapping) { }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XDocument dtoDoc = null;

      try
      {
        GraphMap graphMap = _mapping.FindGraphMap(graphName);
        dtoDoc = ToXml(graphMap, ref dataObjects);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in ToXml: " + ex);
        throw ex;
      }

      return dtoDoc;
    }

    public XDocument ToXml(GraphMap graphMap, ref IList<IDataObject> dataObjects)
    {
      XDocument dtoDoc = null;

      try
      {
        _graphMap = graphMap;

        if (_graphMap != null && _graphMap.ClassTemplateMaps.Count > 0 &&
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

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects, string className, string classIdentifier)
    {
      XDocument dtoDoc = null;

      try
      {
        GraphMap graphMap = _mapping.FindGraphMap(graphName);
        dtoDoc = ToXml(graphMap, ref dataObjects, className, classIdentifier);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in ToXml: " + ex);
        throw ex;
      }

      return dtoDoc;
    }

    public XDocument ToXml(GraphMap graphMap, ref IList<IDataObject> dataObjects, string className, string classIdentifier)
    {
      XDocument dtoDoc = null;

      try
      {
        _graphMap = graphMap;

        if (_graphMap != null && _graphMap.ClassTemplateMaps.Count > 0 && dataObjects != null)
        {
          _dataObjects = dataObjects;
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
          throw ex;
        }
      }

      return dataObjects;
    }

    public IList<IDataObject> ToDataObjects(GraphMap graphMap, ref DataTransferObjects dataTransferObjects)
    {
      _graphMap = graphMap;
      _dataTransferObjects = dataTransferObjects;
      _dataObjects = new List<IDataObject>();

      if (_graphMap != null && _graphMap.ClassTemplateMaps.Count > 0 && _dataTransferObjects != null)
      {
        ClassTemplateMap rootClassTemplateMap = _graphMap.ClassTemplateMaps.First();
        int objectCount = _dataTransferObjects.DataTransferObjectList.Count;

        if (rootClassTemplateMap != null && rootClassTemplateMap.ClassMap != null)
        {
          IDataObject dataObject = null;

          _dataObjects = new List<IDataObject>();
          _dataRecords = new Dictionary<string, string>[objectCount];
          _relatedRecordsMaps = new Dictionary<string, List<Dictionary<string, string>>>[objectCount];
          _relatedObjectPaths = new List<string>();

          for (int dataObjectIndex = 0; dataObjectIndex < objectCount; dataObjectIndex++)
          {
            _dataRecords[dataObjectIndex] = new Dictionary<string, string>();
            _relatedRecordsMaps[dataObjectIndex] = new Dictionary<string, List<Dictionary<string, string>>>();

            ProcessInboundClass(dataObjectIndex, rootClassTemplateMap);

            try
            {
              dataObject = CreateDataObject(_graphMap.DataObjectName, dataObjectIndex);
              _dataObjects.Add(dataObject);
            }
            catch (Exception e)
            {
              StringBuilder builder = new StringBuilder();
              Dictionary<string, string> dataRecord = _dataRecords[dataObjectIndex];
              string identifier = _dataTransferObjects.DataTransferObjectList[dataObjectIndex].identifier;

              builder.AppendLine("Error creating data object for [" + identifier + "]. " + e);
              builder.AppendLine("Data Record: ");

              foreach (var pair in dataRecord)
              {
                builder.AppendLine("\t" + pair.Key + ": " + pair.Value);
              }

              _logger.Error(builder.ToString());
            }
          }

          // fill related data objects and append them to top level data objects
          if (dataObject != null && _relatedObjectPaths != null && _relatedObjectPaths.Count > 0)
          {
            ProcessRelatedItems();
            CreateRelatedObjects();
          }
        }
      }

      return _dataObjects;
    }

    public DataTransferIndices GetDataTransferIndices(GraphMap graphMap, IList<IDataObject> dataObjects, string sortIndex)
    {
      _graphMap = graphMap;
      _dataObjects = dataObjects;

      DataTransferIndices dtis = new DataTransferIndices();

      if (_graphMap != null && _graphMap.ClassTemplateMaps != null && _graphMap.ClassTemplateMaps.Count > 0 &&
        dataObjects != null && dataObjects.Count > 0)
      {
        ClassTemplateMap classTemplateMap = _graphMap.ClassTemplateMaps.First();
        string keyDelimiter = classTemplateMap.ClassMap.IdentifierDelimiter;
        List<KeyProperty> keyProperties = GetKeyProperties(_graphMap.DataObjectName);
        List<string> keyPropertyNames = new List<string>();

        foreach (KeyProperty keyProperty in keyProperties)
        {
          keyPropertyNames.Add(_graphMap.DataObjectName + '.' + keyProperty.keyPropertyName);
        }

        string sortType = null;

        for (int dataObjectIndex = 0; dataObjectIndex < _dataObjects.Count; dataObjectIndex++)
        {
          if (_dataObjects[dataObjectIndex] != null)
          {
            DataTransferIndex dti = new DataTransferIndex();
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            StringBuilder internalIdentifier = new StringBuilder();
            StringBuilder propertyValues = new StringBuilder();

            BuildDataTransferIndex(dti, dataObjectIndex, classTemplateMap, keyDelimiter, keyPropertyNames, keyValues,
              propertyValues, sortIndex, ref sortType);

            foreach (string identifierValue in keyValues.Values)
            {
              if (internalIdentifier.Length > 0)
              {
                internalIdentifier.Append(keyDelimiter);
              }

              internalIdentifier.Append(identifierValue);
            }

            dti.InternalIdentifier = internalIdentifier.ToString();
            dti.HashValue = Utility.MD5Hash(propertyValues.ToString());
            dtis.DataTransferIndexList.Add(dti);
          }
        }

        if (sortType != null)
        {
          dtis.SortType = sortType;
        }
      }

      return dtis;
    }

    #region data transfer indices helper methods
    private void BuildDataTransferIndex(DataTransferIndex dti, int dataObjectIndex, ClassTemplateMap classTemplateMap, string keyDelimiter,
      List<string> keyPropertyNames, Dictionary<string, string> keyValues, StringBuilder propertyValues, string sortIndex, ref string sortType)
    {
      if (classTemplateMap != null && classTemplateMap.ClassMap != null)
      {
        IDataObject dataObject = _dataObjects[dataObjectIndex];
        bool hasRelatedProperty;
        List<string> classIdentifiers = GetClassIdentifiers(classTemplateMap.ClassMap, dataObjectIndex, out hasRelatedProperty);

        for (int classIdentifierIndex = 0; classIdentifierIndex < classIdentifiers.Count; classIdentifierIndex++)
        {
          string classIdentifier = classIdentifiers[classIdentifierIndex];

          if (!String.IsNullOrEmpty(classIdentifier))
          {
            List<RoleMap> classRoles = new List<RoleMap>();

            // dti identifier is root class identifier
            if (String.IsNullOrEmpty(dti.Identifier))
            {
              dti.Identifier = classIdentifier;
            }

            // if key property(properties) is(are) mapped in classMap identifier(s), append it(them) to keyValues
            List<string> identifierParts = classTemplateMap.ClassMap.Identifiers;
            string[] identifierValueParts = classIdentifier.Split(new string[] { keyDelimiter }, StringSplitOptions.None);

            for (int identifierIndex = 0; identifierIndex < identifierParts.Count; identifierIndex++)
            {
              string identifierPart = identifierParts[identifierIndex];

              if (!IsFixedIdentifier(identifierPart) && identifierValueParts.Length >= identifierIndex &&
                !keyValues.ContainsKey(identifierPart) && keyPropertyNames.Contains(identifierPart))
              {
                string identifierValue = identifierValueParts[identifierIndex];
                keyValues.Add(identifierPart, identifierValue);
              }
            }

            foreach (TemplateMap templateMap in classTemplateMap.TemplateMaps)
            {
              StringBuilder tempPropertyValues = new StringBuilder();
              bool isTemplateValid = true;

              foreach (RoleMap roleMap in templateMap.RoleMaps)
              {
                if (roleMap.Type == RoleType.Property ||
                    roleMap.Type == RoleType.DataProperty ||
                    roleMap.Type == RoleType.ObjectProperty ||
                    roleMap.Type == RoleType.FixedValue)
                {
                  if (String.IsNullOrEmpty(roleMap.PropertyName))
                  {
                    throw new Exception("No data property mapped to role [" + classTemplateMap.ClassMap.Name + "." + templateMap.Name + "." + roleMap.Name + "]");
                  }

                  string[] propertyParts = roleMap.PropertyName.Split('.');
                  string propertyName = propertyParts[propertyParts.Length - 1];

                  int lastDotPos = roleMap.PropertyName.LastIndexOf('.');
                  string objectPath = roleMap.PropertyName.Substring(0, lastDotPos);

                  if (propertyParts.Length == 2)  // direct property
                  {
                    string propertyValue = Convert.ToString(dataObject.GetPropertyValue(propertyName));
                    propertyValue = ParsePropertyValue(roleMap, propertyValue);

                    if (!String.IsNullOrEmpty(roleMap.ValueListName) && String.IsNullOrEmpty(propertyValue))
                    {
                      isTemplateValid = false;
                      break;
                    }
                    
                    if (propertyName == sortIndex)
                    {
                      dti.SortIndex = propertyValue;

                      if (sortType == null)
                      {
                        sortType = Utility.XsdTypeToCSharpType(roleMap.DataType);
                      }
                    }

                    if (keyPropertyNames.Contains(roleMap.PropertyName))
                    {
                      if (!keyValues.ContainsKey(roleMap.PropertyName))
                      {
                        keyValues.Add(roleMap.PropertyName, propertyValue);
                      }
                    }

                    tempPropertyValues.Append(propertyValue);                    
                  }
                  else  // related property
                  {
                    string key = objectPath + "." + dataObjectIndex;
                    List<IDataObject> relatedObjects = null;

                    if (!_relatedObjectsCache.TryGetValue(key, out relatedObjects))
                    {
                      relatedObjects = GetRelatedObjects(roleMap.PropertyName, dataObject);
                      _relatedObjectsCache.Add(key, relatedObjects);
                    }

                    if (hasRelatedProperty)  // reference class identifier has related property
                    {
                      IDataObject relatedObject = relatedObjects[classIdentifierIndex];
                      string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
                      propertyValue = ParsePropertyValue(roleMap, propertyValue);

                      if (!String.IsNullOrEmpty(roleMap.ValueListName) && String.IsNullOrEmpty(propertyValue))
                      {
                        isTemplateValid = false;
                        break;
                      }
                      
                      tempPropertyValues.Append(propertyValue);
                    }
                    else  // related property is property map
                    {
                      foreach (IDataObject relatedObject in relatedObjects)
                      {
                        string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
                        propertyValue = ParsePropertyValue(roleMap, propertyValue);

                        if (!String.IsNullOrEmpty(roleMap.ValueListName) && String.IsNullOrEmpty(propertyValue))
                        {
                          isTemplateValid = false;
                          break;
                        }
                        
                        tempPropertyValues.Append(propertyValue);
                      }

                      if (!isTemplateValid) break;
                    }
                  }
                }
                else if (roleMap.Type == RoleType.Reference && roleMap.ClassMap != null)
                {
                  classRoles.Add(roleMap);
                }
              }

              if (isTemplateValid)
              {
                propertyValues.Append(tempPropertyValues.ToString());
              }
            }

            foreach (RoleMap classRole in classRoles)
            {
              ClassTemplateMap relatedClassTemplateMap = _graphMap.GetClassTemplateMap(classRole.ClassMap.Id);

              if (relatedClassTemplateMap != null && relatedClassTemplateMap.ClassMap != null)
              {
                BuildDataTransferIndex(dti, dataObjectIndex, relatedClassTemplateMap, keyDelimiter, keyPropertyNames, keyValues,
                  propertyValues, sortIndex, ref sortType);
              }
            }
          }
        }
      }
    }
    #endregion

    #region outbound helper methods
    private DataTransferObjects BuildDataTransferObjects()
    {
      DataTransferObjects dataTransferObjects = new DataTransferObjects();
      dataTransferObjects.ScopeName = _settings["Scope"];
      dataTransferObjects.AppName = _settings["ApplicationName"];

      ClassTemplateMap classTemplateMap = _graphMap.ClassTemplateMaps.First();

      if (classTemplateMap != null && classTemplateMap.ClassMap != null)
      {
        for (int dataObjectIndex = 0; dataObjectIndex < _dataObjects.Count; dataObjectIndex++)
        {
          DataTransferObject dto = new DataTransferObject();
          dataTransferObjects.DataTransferObjectList.Add(dto);

          bool hasRelatedProperty;
          List<string> classIdentifiers = GetClassIdentifiers(classTemplateMap.ClassMap, dataObjectIndex, out hasRelatedProperty);

          ProcessOutboundClass(dto, dataObjectIndex, String.Empty, String.Empty, true, classIdentifiers, hasRelatedProperty,
            classTemplateMap.ClassMap, classTemplateMap.TemplateMaps);
        }
      }

      return dataTransferObjects;
    }

    // build DTO that's rooted at className with classIdentifier
    private DataTransferObject BuildDataTransferObject(string startClassName, string startClassIdentifier)
    {
      DataTransferObject dto = new DataTransferObject();

      ClassTemplateMap classTemplateMap = _graphMap.GetClassTemplateMapByName(startClassName);

      if (classTemplateMap != null && classTemplateMap.ClassMap != null)
      {
        bool hasRelatedProperty;
        List<string> classIdentifiers = GetClassIdentifiers(classTemplateMap.ClassMap, 0, out hasRelatedProperty);

        ProcessOutboundClass(dto, 0, startClassName, startClassIdentifier, true, classIdentifiers, hasRelatedProperty,
          classTemplateMap.ClassMap, classTemplateMap.TemplateMaps);
      }

      return dto;
    }

    private void ProcessOutboundClass(DataTransferObject dto, int dataObjectIndex, string startClassName, string startClassIdentifier, bool isRootClass,
      List<string> classIdentifiers, bool hasRelatedProperty, ClassMap classMap, List<TemplateMap> templateMaps)
    {
      string className = Utility.TitleCase(classMap.Name);
      string classId = classMap.Id.Substring(classMap.Id.IndexOf(":") + 1);

      for (int classIdentifierIndex = 0; classIdentifierIndex < classIdentifiers.Count; classIdentifierIndex++)
      {
        string classIdentifier = classIdentifiers[classIdentifierIndex];

        if (String.IsNullOrEmpty(startClassIdentifier) || className != startClassName || classIdentifier == startClassIdentifier)
        {
          ClassObject classObject = new ClassObject()
          {
            classId = classMap.Id,
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
      Dictionary<RoleMap, RoleObject> roleObjectMaps = new Dictionary<RoleMap, RoleObject>();

      TemplateObject baseTemplateObject = new TemplateObject
      {
        templateId = templateMap.Id,
        name = templateMap.Name,
      };

      foreach (RoleMap roleMap in templateMap.RoleMaps)
      {
        RoleObject roleObject = new RoleObject
        {
          type = roleMap.Type,
          roleId = roleMap.Id,
          name = roleMap.Name
        };

        switch (roleMap.Type)
        {
          case RoleType.Possessor:
            baseTemplateObject.roleObjects.Add(roleObject);
            break;

          case RoleType.FixedValue:
            roleObject.dataType = roleMap.DataType;
            roleObject.value = roleMap.Value;
            baseTemplateObject.roleObjects.Add(roleObject);
            break;

          case RoleType.Reference:
            if (roleMap.ClassMap != null)
            {
              roleObject.relatedClassId = roleMap.ClassMap.Id;
              roleObject.relatedClassName = roleMap.ClassMap.Name;
              classRoles.Add(roleMap);
              roleObjectMaps.Add(roleMap, roleObject);
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
        bool isTemplateValid = true;  // template is not valid when value list uri is empty
        List<List<RoleObject>> multiRoleObjects = new List<List<RoleObject>>();

        // create property elements
        foreach (RoleMap propertyRole in propertyRoles)
        {
          List<RoleObject> roleObjects = new List<RoleObject>();
          multiRoleObjects.Add(roleObjects);

          string[] propertyParts = propertyRole.PropertyName.Split('.');
          string propertyName = propertyParts[propertyParts.Length - 1];

          int lastDotPos = propertyRole.PropertyName.LastIndexOf('.');
          string objectPath = propertyRole.PropertyName.Substring(0, lastDotPos);

          if (propertyParts.Length == 2)  // direct property
          {
            string propertyValue = Convert.ToString(dataObject.GetPropertyValue(propertyName));
            propertyValue = ParsePropertyValue(propertyRole, propertyValue);

            RoleObject roleObject = new RoleObject()
            {
              type = propertyRole.Type,
              roleId = propertyRole.Id,
              name = propertyRole.Name,
              dataType = propertyRole.DataType,
              value = propertyValue
            };

            if (!String.IsNullOrEmpty(propertyRole.ValueListName))
            {
              if (String.IsNullOrEmpty(propertyValue))
              {
                isTemplateValid = false;
                break;
              }
              else 
              {
                roleObject.hasValueMap = true;
              }
            }

            roleObjects.Add(roleObject);
          }
          else  // related property
          {
            string key = objectPath + "." + dataObjectIndex;
            List<IDataObject> relatedObjects = null;

            if (!_relatedObjectsCache.TryGetValue(key, out relatedObjects))
            {
              relatedObjects = GetRelatedObjects(propertyRole.PropertyName, dataObject);
              _relatedObjectsCache.Add(key, relatedObjects);
            }

            RoleObject roleObject = new RoleObject()
            {
              type = propertyRole.Type,
              roleId = propertyRole.Id,
              name = propertyRole.Name,
              dataType = propertyRole.DataType
            };

            // reference class identifier has related property, process related object at classIdentifierIndex only
            if (classIdentifierHasRelatedProperty)
            {
              IDataObject relatedObject = relatedObjects[classIdentifierIndex];
              string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
              propertyValue = ParsePropertyValue(propertyRole, propertyValue);
              roleObject.value = propertyValue;
            }
            else  // related property is property map
            {
              roleObject.values = new RoleValues();

              foreach (IDataObject relatedObject in relatedObjects)
              {
                string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
                propertyValue = ParsePropertyValue(propertyRole, propertyValue);

                if (!String.IsNullOrEmpty(propertyRole.ValueListName))
                {
                  if (String.IsNullOrEmpty(propertyValue))
                  {
                    isTemplateValid = false;
                    break;
                  }
                  else
                  {
                    roleObject.hasValueMap = true;
                    roleObject.values.Add(propertyValue);
                  }
                }
              }

              if (!isTemplateValid) break;
            }

            roleObjects.Add(roleObject);
          }
        }

        if (isTemplateValid)
        {
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
      }
      else if (classRoles.Count > 0)  // relationship template
      {
        bool templateValid = false;  // template is valid when exist at least one class referernce identifier that is not null

        foreach (RoleMap classRole in classRoles)
        {
          bool refClassHasRelatedProperty;
          List<string> refClassIdentifiers = GetClassIdentifiers(classRole.ClassMap, dataObjectIndex, out refClassHasRelatedProperty);

          if (refClassIdentifiers.Count > 0 && !String.IsNullOrEmpty(refClassIdentifiers.First()))
          {
            templateValid = true;
            ClassTemplateMap relatedClassTemplateMap = _graphMap.GetClassTemplateMap(classRole.ClassMap.Id);

            if (relatedClassTemplateMap != null && relatedClassTemplateMap.ClassMap != null)
            {
              // update class reference role object values
              RoleObject roleObjectMap = roleObjectMaps[classRole];

              if (roleObjectMap.values == null)
                roleObjectMap.values = new RoleValues();

              foreach (string identifier in refClassIdentifiers)
              {
                roleObjectMap.values.Add(identifier);
              }

              ProcessOutboundClass(dto, dataObjectIndex, startClassName, startClassIdentifier, false, refClassIdentifiers,
                refClassHasRelatedProperty, relatedClassTemplateMap.ClassMap, relatedClassTemplateMap.TemplateMaps);
            }
            else  // reference class not found (leaf node role map), treat it as "data property" role with value
            {
              foreach (RoleObject roleObject in baseTemplateObject.roleObjects)
              {
                if (roleObject.relatedClassId == classRole.ClassMap.Id)
                {
                  roleObject.value = refClassIdentifiers.First();
                  break;
                }
              }
            }
          }
        }

        if (templateValid)
        {
          classObject.templateObjects.Add(baseTemplateObject);
        }
      }
    }

    private string ParsePropertyValue(RoleMap propertyRole, string propertyValue)
    {
      string value = propertyValue;

      if (String.IsNullOrEmpty(propertyRole.ValueListName))
      {
        if (propertyRole.DataType.ToLower().Contains("datetime"))
          value = Utility.ToXsdDateTime(propertyValue);
      }
      else  // resolve value list to uri
      {
        value = _mapping.ResolveValueList(propertyRole.ValueListName, propertyValue);

        if (value == MappingExtensions.RDF_NIL)
        {
          value = String.Empty;
        }
      }

      return value;
    }

    private string GetReferenceRoleValue(RoleMap referenceRole)
    {
      string value = referenceRole.Value;

      if (!String.IsNullOrEmpty(referenceRole.ValueListName))
      {
        value = _mapping.ResolveValueList(referenceRole.ValueListName, value);
      }

      return value;
    }
    #endregion

    #region inbound helper methods
    private void ProcessInboundClass(int dataObjectIndex, ClassTemplateMap classTemplateMap)
    {
      ClassMap classMap = classTemplateMap.ClassMap;
      List<TemplateMap> templateMaps = classTemplateMap.TemplateMaps;
      List<ClassObject> classObjects = GetClassObjects(dataObjectIndex, classMap.Id);

      for (int classObjectIndex = 0; classObjectIndex < classObjects.Count; classObjectIndex++)
      {
        ClassObject classObject = classObjects[classObjectIndex];
        string identifierValue = classObject.identifier;
        ProcessInboundClassIdentifiers(dataObjectIndex, classMap, classObjectIndex, identifierValue);

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

        if (templateObject != null)
        {
          foreach (RoleMap roleMap in templateMap.RoleMaps)
          {
            switch (roleMap.Type)
            {
              case RoleType.Reference:
                if (roleMap.ClassMap != null)
                {
                  ClassTemplateMap classTemplateMap = _graphMap.GetClassTemplateMap(roleMap.ClassMap.Id);

                  if (classTemplateMap != null && classTemplateMap.ClassMap != null)
                  {
                    ProcessInboundClass(dataObjectIndex, classTemplateMap);
                  }
                  else  // reference class not found, process it as property role
                  {
                    foreach (RoleObject roleObject in templateObject.roleObjects)
                    {
                      if (roleObject.roleId == roleMap.Id && roleObject.relatedClassId != null && roleObject.relatedClassId == roleMap.ClassMap.Id)
                      {
                        string identifier = roleObject.value;
                        ProcessInboundClassIdentifiers(dataObjectIndex, roleMap.ClassMap, classObjectIndex, identifier);
                        break;
                      }
                    }
                  }
                }
                break;

              case RoleType.Property:
              case RoleType.DataProperty:
              case RoleType.ObjectProperty:
              case RoleType.FixedValue:
                ProcessInboundPropertyRole(dataObjectIndex, classObjectIndex, roleMap, templateObject);
                break;
            }
          }
        }
        else
        {
          // handle value list that has no uri map
          foreach (RoleMap roleMap in templateMap.RoleMaps)
          {
            if (!String.IsNullOrEmpty(roleMap.ValueListName))
            {
              ValueListMap valueListMap = _mapping.ValueListMaps.Find(x => x.Name.ToLower() == roleMap.ValueListName.ToLower());

              if (valueListMap != null && valueListMap.ValueMaps != null)
              {
                ValueMap valueMap = valueListMap.ValueMaps.Find(x => String.IsNullOrEmpty(x.Uri));

                if (valueMap != null)
                {
                  string[] propertyPath = roleMap.PropertyName.Split('.');
                  string propertyName = propertyPath[propertyPath.Length - 1];

                  if (propertyPath.Length > 2)  // related property
                  {
                    List<string> values = new List<string>();         
                    values.Add(valueMap.InternalValue);

                    SetRelatedRecords(dataObjectIndex, classObjectIndex, roleMap.PropertyName, values);
                  }
                  else
                  {
                    _dataRecords[dataObjectIndex][propertyName] = valueMap.InternalValue;
                  }
                }
              }
            }
          }
        }
      }
    }

    private void ProcessInboundPropertyRole(int dataObjectIndex, int classObjectIndex, RoleMap roleMap, TemplateObject templateObject)
    {
      string[] propertyPath = roleMap.PropertyName.Split('.');
      string propertyName = propertyPath[propertyPath.Length - 1];
      List<string> values = new List<string>();

      foreach (RoleObject roleObject in templateObject.roleObjects)
      {
        if (roleObject.roleId == roleMap.Id)
        {
          if (propertyPath.Length > 2)  // related property
          {
            foreach (string value in roleObject.values)
            {
              values.Add(value);
            }
          }
          else  // direct property
          {
            string value = roleObject.value;

            if (!String.IsNullOrEmpty(roleMap.ValueListName))
            {
              value = _mapping.ResolveValueMap(roleMap.ValueListName, value);
            }

            _dataRecords[dataObjectIndex][propertyName] = value;
          }

          break;
        }
      }

      if (propertyPath.Length > 2 && values.Count > 0)
      {
        SetRelatedRecords(dataObjectIndex, classObjectIndex, roleMap.PropertyName, values);
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