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
using System.IO;

namespace org.iringtools.adapter.projection
{
    public class DtoProjectionEngine : BasePart7ProjectionEngine
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DtoProjectionEngine));
        private DataTransferObjects _dataTransferObjects;

        [Inject]
        public DtoProjectionEngine(AdapterSettings settings, DataDictionary dictionary, Mapping mapping)
            : base(settings, dictionary, mapping) { }

        public override XDocument ToXml(string graphName, ref List<IDataObject> dataObjects)
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

        public XDocument ToXml(GraphMap graphMap, ref List<IDataObject> dataObjects)
        {
            XDocument dtoDoc = null;

            try
            {
                _graphMap = graphMap;

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

        public DataTransferObjects BuildDataTransferObjects(GraphMap graphMap, ref List<IDataObject> dataObjects)
        {
            _graphMap = graphMap;
            _dataObjects = dataObjects;

            return BuildDataTransferObjects();
        }

        public override XDocument ToXml(string graphName, ref List<IDataObject> dataObjects, string className, string classIdentifier)
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

        public XDocument ToXml(GraphMap graphMap, ref List<IDataObject> dataObjects, string className, string classIdentifier)
        {
            XDocument dtoDoc = null;

            try
            {
                _graphMap = graphMap;

                if (_graphMap != null && _graphMap.classTemplateMaps.Count > 0 && dataObjects != null)
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

        public override List<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument)
        {
            List<IDataObject> dataObjects = null;

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

        public List<IDataObject> ToDataObjects(GraphMap graphMap, ref DataTransferObjects dataTransferObjects)
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
                    IDataObject dataObject = null;

                    _dataObjects = new List<IDataObject>();
                    _dataRecords = new Dictionary<string, string>[objectCount];
                    _relatedRecordsMaps = new Dictionary<string, List<Dictionary<string, string>>>[objectCount];
                    _relatedObjectPaths = new List<string>();

                    for (int dataObjectIndex = 0; dataObjectIndex < objectCount; dataObjectIndex++)
                    {
                        DataTransferObject dto = _dataTransferObjects.DataTransferObjectList[dataObjectIndex];

                        _dataRecords[dataObjectIndex] = new Dictionary<string, string>();
                        _relatedRecordsMaps[dataObjectIndex] = new Dictionary<string, List<Dictionary<string, string>>>();

                        if (_graphMap.classTemplateMaps.Count > dto.classObjects.Count)
                        {
                            foreach (ClassTemplateMap ctm in _graphMap.classTemplateMaps)
                            {
                                ClassObject co = dto.classObjects.Find(x => x.classId == ctm.classMap.id);

                                if (co == null)
                                {
                                    ProcessNullClassObject(dataObjectIndex, ctm.templateMaps);
                                }
                            }
                        }

                        ProcessInboundClass(dataObjectIndex, rootClassTemplateMap);

                        try
                        {
                            dataObject = CreateDataObject(_graphMap.dataObjectName, dataObjectIndex);

                            if (dto.transferType == TransferType.Add)
                            {
                                ((SerializableDataObject)dataObject).State = ObjectState.Create;
                            }
                            else if (dto.transferType == TransferType.Change)
                            {
                                ((SerializableDataObject)dataObject).State = ObjectState.Update;
                            }

                            if (dto.content != null && dto.content.Length > 0)
                            {
                                ((SerializableDataObject)dataObject).HasContent = true;
                                ((SerializableDataObject)dataObject).Content = new MemoryStream(dto.content);
                            }

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

        public DataTransferIndices GetDataTransferIndices(GraphMap graphMap, List<IDataObject> dataObjects, string sortIndex)
        {
            DataTransferIndices dtis = new DataTransferIndices();

            try
            {
                _graphMap = graphMap;
                _dataObjects = dataObjects;

                if (_graphMap != null && _graphMap.classTemplateMaps != null && _graphMap.classTemplateMaps.Count > 0 &&
                  dataObjects != null && dataObjects.Count > 0)
                {
                    ClassTemplateMap classTemplateMap = _graphMap.classTemplateMaps.First();
                    DataObject dataObject = _dictionary.dataObjects.First(c => c.objectName.ToUpper() == _graphMap.dataObjectName.ToUpper());
                    string keyDelimiter = dataObject.keyDelimeter ?? string.Empty;

                    List<string> keyPropertyNames = new List<string>();
                    foreach (KeyProperty keyProperty in dataObject.keyProperties)
                    {
                        keyPropertyNames.Add(_graphMap.dataObjectName + '.' + keyProperty.keyPropertyName);
                    }

                    string sortType = null;

                    for (int dataObjectIndex = 0; dataObjectIndex < _dataObjects.Count; dataObjectIndex++)
                    {
                        if (_dataObjects[dataObjectIndex] != null)
                        {
                            DataTransferIndex dti = new DataTransferIndex();
                            StringBuilder internalIdentifier = new StringBuilder();
                            Dictionary<string, string> propertyValues = new Dictionary<string, string>();

                            BuildDataTransferIndex(dti, dataObjectIndex, classTemplateMap, keyPropertyNames,
                              propertyValues, sortIndex, ref sortType);

                            //
                            // set dti identifier
                            //
                            if (_graphMap.classTemplateMaps.Count > 0)
                            {
                                ClassTemplateMap ctm = _graphMap.classTemplateMaps[0];
                                string delimiter = ctm.classMap.identifierDelimiter ?? string.Empty;

                                if (ctm.classMap.identifierKeyMaps != null)
                                {
                                    foreach (KeyMap keyMap in ctm.classMap.identifierKeyMaps)
                                    {
                                        string key = keyMap.classId + keyMap.templateId + keyMap.roleId;

                                        if (propertyValues.ContainsKey(key))
                                        {
                                            dti.Identifier += delimiter + propertyValues[key];
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (string identifier in ctm.classMap.identifiers)
                                    {
                                        string property = identifier.Substring(identifier.IndexOf(".") + 1);
                                        string value = Convert.ToString(_dataObjects[dataObjectIndex].GetPropertyValue(property));
                                        dti.Identifier += delimiter + value;
                                    }
                                }

                                if (dti.Identifier != null)
                                {
                                    dti.Identifier = dti.Identifier.Remove(0, delimiter.Length);
                                }
                            }

                            //
                            // set dti internal identifier
                            //
                            foreach (KeyProperty keyProp in dataObject.keyProperties)
                            {
                                internalIdentifier.Append(keyDelimiter);
                                internalIdentifier.Append(_dataObjects[dataObjectIndex].GetPropertyValue(keyProp.keyPropertyName));
                            }

                            internalIdentifier.Remove(0, keyDelimiter.Length);
                            dti.InternalIdentifier = internalIdentifier.ToString();

                            //
                            // compute hash value
                            //
                            StringBuilder values = new StringBuilder();
                            foreach (var pair in propertyValues)
                            {
                                values.Append(pair.Value);
                            }
                            dti.HashValue = Utility.MD5Hash(values.ToString());

                            //
                            // determine HasContent flag
                            //
                            if (typeof(GenericDataObject).IsAssignableFrom(_dataObjects[dataObjectIndex].GetType()))
                            {
                                dti.HasContent = ((GenericDataObject)(_dataObjects[dataObjectIndex])).HasContent;
                            }
                            else if (typeof(IContentObject).IsAssignableFrom(_dataObjects[dataObjectIndex].GetType()))
                            {
                                dti.HasContent = ((GenericContentObject)(_dataObjects[dataObjectIndex])).HasContent;
                            }

                            if (string.IsNullOrEmpty(dti.Identifier))
                            {
                                _logger.Warn("DTI has no identifier: [" + values + "]");
                            }
                            else
                            {
                                dtis.DataTransferIndexList.Add(dti);
                            }
                        }
                    }

                    if (sortType != null)
                    {
                        dtis.SortType = sortType;
                    }
                }

                if (dtis != null)
                {
                    _logger.Debug(string.Format("DTI count [{0}.{1}]: {2}",
                      _settings["ProjectName"], _settings["ApplicationName"], dtis.DataTransferIndexList.Count));
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error gettting data indices: " + e.Message + e.StackTrace);
                throw e;
            }

            return dtis;
        }

        #region data transfer indices helper methods
        private void BuildDataTransferIndex(DataTransferIndex dti, int dataObjectIndex, ClassTemplateMap classTemplateMap,
          List<string> keyPropertyNames, Dictionary<string, string> propertyValues, string sortIndex, ref string sortType)
        {
            if (classTemplateMap != null && classTemplateMap.classMap != null)
            {
                IDataObject dataObject = _dataObjects[dataObjectIndex];
                bool hasRelatedProperty;
                List<string> classIdentifiers = GetClassIdentifiers(classTemplateMap.classMap, dataObjectIndex, out hasRelatedProperty);
                string identifierDelimiter = classTemplateMap.classMap.identifierDelimiter;

                for (int classIdentifierIndex = 0; classIdentifierIndex < classIdentifiers.Count; classIdentifierIndex++)
                {
                    string classIdentifier = classIdentifiers[classIdentifierIndex];

                    if (!String.IsNullOrEmpty(classIdentifier))
                    {
                        List<RoleMap> classRoles = new List<RoleMap>();

                        foreach (TemplateMap templateMap in classTemplateMap.templateMaps)
                        {
                            StringBuilder tempPropertyValues = new StringBuilder();
                            bool isTemplateValid = true;

                            foreach (RoleMap roleMap in templateMap.roleMaps)
                            {
                                if (roleMap.type == RoleType.Property ||
                                    roleMap.type == RoleType.DataProperty ||
                                    roleMap.type == RoleType.ObjectProperty ||
                                    roleMap.type == RoleType.FixedValue)
                                {
                                    if (String.IsNullOrEmpty(roleMap.propertyName) && roleMap.type != RoleType.FixedValue)
                                    {
                                        throw new Exception("No data property mapped to role [" + classTemplateMap.classMap.name + "." + templateMap.name + "." + roleMap.name + "]");
                                    }

                                    if (roleMap.type == RoleType.FixedValue) // if it is mapped with literal
                                    {
                                        string propertyValue = roleMap.value;
                                        propertyValue = ParsePropertyValue(roleMap, propertyValue);

                                        if (!String.IsNullOrEmpty(roleMap.valueListName) && String.IsNullOrEmpty(propertyValue))
                                        {
                                            isTemplateValid = false;
                                            break;
                                        }

                                        tempPropertyValues.Append(propertyValue);
                                    }
                                    else // if it is mapped with property
                                    {
                                        string[] propertyParts = roleMap.propertyName.Split('.');
                                        string propertyName = propertyParts[propertyParts.Length - 1];

                                        int lastDotPos = roleMap.propertyName.LastIndexOf('.');
                                        string objectPath = roleMap.propertyName.Substring(0, lastDotPos);

                                        if (propertyParts.Length == 2)  // direct property
                                        {
                                            string propertyValue = Convert.ToString(dataObject.GetPropertyValue(propertyName));
                                            string keyValue = propertyValue;
                                            propertyValue = ParsePropertyValue(roleMap, propertyValue);

                                            if (!String.IsNullOrEmpty(roleMap.valueListName) && String.IsNullOrEmpty(propertyValue))
                                            {
                                                isTemplateValid = false;
                                                break;
                                            }

                                            if (propertyName == sortIndex)
                                            {
                                                dti.SortIndex = propertyValue;

                                                if (sortType == null)
                                                {
                                                    sortType = Utility.XsdTypeToCSharpType(roleMap.dataType);
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
                                                relatedObjects = GetRelatedObjects(roleMap.propertyName, dataObject);
                                                _relatedObjectsCache.Add(key, relatedObjects);
                                            }

                                            if (hasRelatedProperty)  // reference class identifier has related property
                                            {
                                                IDataObject relatedObject = relatedObjects[classIdentifierIndex];
                                                string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
                                                string keyValue = propertyValue;
                                                propertyValue = ParsePropertyValue(roleMap, propertyValue);

                                                if (!String.IsNullOrEmpty(roleMap.valueListName) && String.IsNullOrEmpty(propertyValue))
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
                                                    string keyValue = propertyValue;
                                                    propertyValue = ParsePropertyValue(roleMap, propertyValue);

                                                    if (!String.IsNullOrEmpty(roleMap.valueListName) && String.IsNullOrEmpty(propertyValue))
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

                                    if (isTemplateValid)
                                    {
                                        string key = classTemplateMap.classMap.id + templateMap.id + roleMap.id;
                                        propertyValues[key] = tempPropertyValues.ToString();
                                    }
                                }
                                else if (roleMap.type == RoleType.Reference && roleMap.classMap != null)
                                {
                                    classRoles.Add(roleMap);
                                }
                            }
                        }

                        foreach (RoleMap classRole in classRoles)
                        {
                            ClassTemplateMap relatedClassTemplateMap = _graphMap.GetClassTemplateMap(classRole.classMap.id, classRole.classMap.index);

                            if (relatedClassTemplateMap != null && relatedClassTemplateMap.classMap != null)
                            {
                                BuildDataTransferIndex(dti, dataObjectIndex, relatedClassTemplateMap, keyPropertyNames,
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

            ClassTemplateMap classTemplateMap = _graphMap.classTemplateMaps.First();
            string objectName = _graphMap.dataObjectName;
            DataObject objDef = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectName.ToLower());
            string keyDelimiter = objDef.keyDelimeter ?? string.Empty;

            if (objDef == null)
            {
                throw new Exception("Data object [" + objectName + "] not found.");
            }

            if (classTemplateMap != null && classTemplateMap.classMap != null)
            {
                for (int dataObjectIndex = 0; dataObjectIndex < _dataObjects.Count; dataObjectIndex++)
                {
                    DataTransferObject dto = new DataTransferObject();
                    StringBuilder internalIdentifier = new StringBuilder();

                    foreach (KeyProperty keyProp in objDef.keyProperties)
                    {
                        internalIdentifier.Append(keyDelimiter);
                        internalIdentifier.Append(_dataObjects[dataObjectIndex].GetPropertyValue(keyProp.keyPropertyName));
                    }

                    internalIdentifier.Remove(0, keyDelimiter.Length);
                    dto.internalIdentifier = internalIdentifier.ToString();

                    if (typeof(IContentObject).IsAssignableFrom(_dataObjects[dataObjectIndex].GetType()))
                    {
                        GenericContentObject contentObject = (GenericContentObject)_dataObjects[dataObjectIndex];
                        dto.hasContent = (contentObject.Content != null);
                        if (dto.hasContent)
                        {
                            dto.content = contentObject.Content.ToMemoryStream().ToArray();
                        }
                    }
                    else if (typeof(GenericDataObject).IsAssignableFrom(_dataObjects[dataObjectIndex].GetType()))
                    {
                        dto.hasContent = ((GenericDataObject)_dataObjects[dataObjectIndex]).HasContent;
                    }

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

                if (String.IsNullOrEmpty(startClassIdentifier) || className != startClassName || classIdentifier == startClassIdentifier)
                {
                    ClassObject classObject = new ClassObject()
                    {
                        classId = classMap.id,
                        name = className,
                        identifier = classIdentifier,
                        index = classMap.index,
                        path = classMap.path
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
            List<RoleMap> fixedValueRoles = new List<RoleMap>();

            Dictionary<RoleMap, RoleObject> roleObjectMaps = new Dictionary<RoleMap, RoleObject>();

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
                        fixedValueRoles.Add(roleMap);
                        break;

                    case RoleType.Reference:
                        if (roleMap.classMap != null)
                        {
                            roleObject.relatedClassId = roleMap.classMap.id;
                            roleObject.relatedClassName = roleMap.classMap.name;
                            roleObject.classPath = roleMap.classMap.path;
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

                    string[] propertyParts = propertyRole.propertyName.Split('.');
                    string propertyName = propertyParts[propertyParts.Length - 1];

                    int lastDotPos = propertyRole.propertyName.LastIndexOf('.');
                    string objectPath = propertyRole.propertyName.Substring(0, lastDotPos);

                    if (propertyParts.Length == 2)  // direct property
                    {
                        string propertyValue = Convert.ToString(dataObject.GetPropertyValue(propertyName));
                        string keyValue = propertyValue;
                        propertyValue = ParsePropertyValue(propertyRole, propertyValue);

                        RoleObject roleObject = new RoleObject()
                        {
                            type = propertyRole.type,
                            roleId = propertyRole.id,
                            name = propertyRole.name,
                            dataType = propertyRole.dataType,
                            value = propertyValue
                        };

                        if (!string.IsNullOrEmpty(propertyRole.valueListName))
                        {
                            if (string.IsNullOrEmpty(propertyValue))
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
                            relatedObjects = GetRelatedObjects(propertyRole.propertyName, dataObject);
                            _relatedObjectsCache.Add(key, relatedObjects);
                        }

                        RoleObject roleObject = new RoleObject()
                        {
                            type = propertyRole.type,
                            roleId = propertyRole.id,
                            name = propertyRole.name,
                            dataType = propertyRole.dataType
                        };

                        // reference class identifier has related property, process related object at classIdentifierIndex only
                        if (classIdentifierHasRelatedProperty)
                        {
                            IDataObject relatedObject = relatedObjects[classIdentifierIndex];
                            string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
                            string keyValue = propertyValue;
                            propertyValue = ParsePropertyValue(propertyRole, propertyValue);
                            roleObject.value = propertyValue;
                        }
                        else  // related property is property map
                        {
                            roleObject.values = new RoleValues();

                            foreach (IDataObject relatedObject in relatedObjects)
                            {
                                string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
                                string keyValue = propertyValue;
                                propertyValue = ParsePropertyValue(propertyRole, propertyValue);

                                if (!string.IsNullOrEmpty(propertyRole.valueListName))
                                {
                                    if (string.IsNullOrEmpty(propertyValue))
                                    {
                                        isTemplateValid = false;
                                        break;
                                    }
                                    else
                                    {
                                        roleObject.hasValueMap = true;
                                    }
                                }

                                roleObject.values.Add(propertyValue);
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
                    List<string> refClassIdentifiers = GetClassIdentifiers(classRole.classMap, dataObjectIndex, out refClassHasRelatedProperty);

                    if (refClassIdentifiers.Count > 0 && !String.IsNullOrEmpty(refClassIdentifiers.First()))
                    {
                        templateValid = true;
                        ClassTemplateMap relatedClassTemplateMap = _graphMap.GetClassTemplateMap(classRole.classMap.id, classRole.classMap.index);

                        if (relatedClassTemplateMap != null && relatedClassTemplateMap.classMap != null)
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

                if (templateValid)
                {
                    classObject.templateObjects.Add(baseTemplateObject);
                }
            }
            else if (fixedValueRoles.Count > 0) // if Template has role with only fixed value
            {
                TemplateObject templateObject = Utility.CloneDataContractObject<TemplateObject>(baseTemplateObject);
                classObject.templateObjects.Add(templateObject);
            }
        }

        private string ParsePropertyValue(RoleMap propertyRole, string propertyValue)
        {
            string value = propertyValue.Trim();

            if (String.IsNullOrEmpty(propertyRole.valueListName))
            {
                if (propertyRole.dataType.ToLower().Contains("datetime"))
                {
                    value = Utility.ToXsdDateTime(value);
                }
                else if (propertyRole.dataType.ToLower().Contains("date"))
                {
                    value = Utility.ToXsdDate(value);
                }
                else if (propertyRole.dataType == "xsd:string" && propertyRole.dataLength > 0 && 
                    value.Length > propertyRole.dataLength && propertyRole.dbDataType != "Decimal" && 
                    !propertyRole.dbDataType.Contains("Int"))
                {
                    value = value.Substring(0, propertyRole.dataLength);

                    //value might contain trailing whitespaces when taking substring, trim it again
                    value = value.TrimEnd();
                }

                //if db data type is decimal parse out smallest integer length and smallest fractional length
                else if (propertyRole.dbDataType == "Decimal" && value.Length > 0)
                {
                    int nSmallestIntegerLength = propertyRole.precision - propertyRole.scale;
                    string[] strLength = value.Split('.');
                    string strSmallestIntegerPart = "";
                    string strSmallestFractionalPart = "";

                    if (strLength.Length >= 1)
                    {
                        //trim integer part if it contains more digit than defined in cross manifest.
                        if (strLength[0].Length > nSmallestIntegerLength)
                        {
                            strSmallestIntegerPart = strLength[0].Substring((strLength[0].Length - nSmallestIntegerLength), nSmallestIntegerLength);
                        }
                        else
                        {
                            strSmallestIntegerPart = strLength[0].Trim();
                        }
                    }
                    if (strLength.Length >= 2)
                    {
                        //trim fractional part if it contains more digit than defined in cross manifest.
                        if (strLength[1].Length > propertyRole.scale)
                        {
                            strSmallestFractionalPart = strLength[1].Substring(0, propertyRole.scale);
                        }
                        else
                        {
                            strSmallestFractionalPart = strLength[1].Trim();
                        }
                    }
                    value = strSmallestIntegerPart + "." + strSmallestFractionalPart;
                }
                else if (propertyRole.dbDataType != null && propertyRole.dbDataType.Contains("Int") && value.Length > 0)
                {
                    string[] strLength = value.Split('.');
                    value = strLength[0];
                }
            }
            else  // resolve value list to uri
            {
                value = _mapping.ResolveValueList(propertyRole.valueListName, propertyValue);

                if (value == MappingExtensions.RDF_NIL)
                {
                    value = String.Empty;
                }
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
        private void ProcessNullClassObject(int dataObjectIndex, List<TemplateMap> templateMaps)
        {
            if (templateMaps != null)
            {
                foreach (TemplateMap templateMap in templateMaps)
                {
                    foreach (RoleMap roleMap in templateMap.roleMaps)
                    {
                        if (roleMap.type == RoleType.DataProperty ||
                            roleMap.type == RoleType.ObjectProperty ||
                            roleMap.type == RoleType.Property)
                        {
                            string[] propertyPath = roleMap.propertyName.Split('.');
                            string propertyName = propertyPath[propertyPath.Length - 1];

                            if (propertyPath.Length > 2)  // related property
                            {
                                //SetRelatedRecords(dataObjectIndex, classObjectIndex, roleMap.propertyName, null);
                            }
                            else
                            {
                                _dataRecords[dataObjectIndex][propertyName] = null;
                            }
                        }
                        else if (roleMap.classMap != null)
                        {
                            ClassTemplateMap classTemplateMap = _graphMap.GetClassTemplateMap(roleMap.classMap.id, roleMap.classMap.index);
                            ClassMap classMap = classTemplateMap.classMap;

                            ProcessNullClassObject(dataObjectIndex, classTemplateMap.templateMaps);
                        }
                    }
                }
            }
        }

        private void ProcessInboundClass(int dataObjectIndex, ClassTemplateMap classTemplateMap)
        {
            ClassMap classMap = classTemplateMap.classMap;
            List<TemplateMap> templateMaps = classTemplateMap.templateMaps;
            List<ClassObject> classObjects = GetClassObjects(dataObjectIndex, classMap.id, classMap.path);

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
                    foreach (RoleMap roleMap in templateMap.roleMaps)
                    {
                        switch (roleMap.type)
                        {
                            case RoleType.Reference:
                                if (roleMap.classMap != null)
                                {
                                    ClassTemplateMap classTemplateMap = _graphMap.GetClassTemplateMap(roleMap.classMap.id, roleMap.classMap.index);

                                    if (classTemplateMap != null && classTemplateMap.classMap != null)
                                    {
                                        ProcessInboundClass(dataObjectIndex, classTemplateMap);
                                    }
                                    else  // reference class not found, process it as property role
                                    {
                                        foreach (RoleObject roleObject in templateObject.roleObjects)
                                        {
                                            if (roleObject.roleId == roleMap.id && roleObject.relatedClassId != null && roleObject.relatedClassId == roleMap.classMap.id)
                                            {
                                                string identifier = roleObject.value;
                                                ProcessInboundClassIdentifiers(dataObjectIndex, roleMap.classMap, classObjectIndex, identifier);
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
                //NOTE: the code block below is commented out to support template exclusion feature
                //else
                //{
                //    foreach (RoleMap roleMap in templateMap.roleMaps)
                //    {
                //        if (roleMap.type == RoleType.DataProperty || 
                //            roleMap.type == RoleType.ObjectProperty ||
                //            roleMap.type == RoleType.Property)
                //        {
                //            string[] propertyPath = roleMap.propertyName.Split('.');
                //            string propertyName = propertyPath[propertyPath.Length - 1];

                //            if (propertyPath.Length > 2)  // related property
                //            {
                //                SetRelatedRecords(dataObjectIndex, classObjectIndex, roleMap.propertyName, null);
                //            }
                //            else
                //            {
                //                _dataRecords[dataObjectIndex][propertyName] = null;
                //            }
                //        }
                //    }
                //}
            }
        }

        private void ProcessInboundPropertyRole(int dataObjectIndex, int classObjectIndex, RoleMap roleMap, TemplateObject templateObject)
        {
            if (roleMap.type == RoleType.FixedValue)
            {
                return;
            }

            string[] propertyPath = roleMap.propertyName.Split('.');
            string propertyName = propertyPath[propertyPath.Length - 1];
            List<string> values = new List<string>();

            foreach (RoleObject roleObject in templateObject.roleObjects)
            {
                if (roleObject.roleId == roleMap.id)
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

                        if (!string.IsNullOrEmpty(roleMap.valueListName))
                        {
                            value = _mapping.ResolveValueMap(roleMap.valueListName, value);
                        }

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

        private List<ClassObject> GetClassObjects(int dataObjectIndex, string classId, string path)
        {
            List<ClassObject> classObjects = new List<ClassObject>();
            DataTransferObject dto = _dataTransferObjects.DataTransferObjectList[dataObjectIndex];

            foreach (ClassObject classObject in dto.classObjects)
            {
                if (classObject.classId == classId &&
                  (String.IsNullOrWhiteSpace(classObject.path) ||
                    (!String.IsNullOrWhiteSpace(path) && classObject.path == path)))
                {
                    classObjects.Add(classObject);
                }
            }

            return classObjects;
        }
        #endregion
    }
}