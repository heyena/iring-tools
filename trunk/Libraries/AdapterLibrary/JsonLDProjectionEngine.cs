using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.adapter.projection;
using Ninject;
using org.iringtools.library;
using org.iringtools.mapping;
using System.Xml.Linq;
using org.iringtools.utility;
using System.IO;
using log4net;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.Xml;
using VDS.RDF.Query.Paths;
using org.iringtools.tip;

namespace org.iringtools.adapter
{
    public class JsonLDProjectionEngine : BasePart7ProjectionEngine// BaseDataProjectionEngine
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JsonProjectionEngine));
        private string[] arrSpecialcharlist;
        private string[] arrSpecialcharValue;


        protected static readonly string QUALIFIED_RDF_NIL = RDF_NS.NamespaceName + "nil";
        private Dictionary<string, List<string>> _individualsCache;
        private string _graphBaseUri;
        private XElement _rdfXml;
        private NameValueCollection tableMapping;

        //[Inject]
        //public JsonLDProjectionEngine(AdapterSettings settings, DataDictionary dictionary, Mapping mapping)
        //    : base(settings, dictionary, mapping)
        //{
        //    _dictionary = dictionary;
        //    _individualsCache = new Dictionary<string, List<string>>();
        //    tableMapping = new NameValueCollection();
        //    JD = new JsonLDBase();

        //    if (_settings["SpCharList"] != null && _settings["SpCharValue"] != null)
        //    {
        //        arrSpecialcharlist = _settings["SpCharList"].ToString().Split(',');
        //        arrSpecialcharValue = _settings["SpCharValue"].ToString().Split(',');
        //    }
        //}

        [Inject]
        public JsonLDProjectionEngine(AdapterSettings settings, DataDictionary dictionary, TipMapping tipMapping)
            : base(settings, dictionary, tipMapping)
        {
            _dictionary = dictionary;
            _individualsCache = new Dictionary<string, List<string>>();
            tableMapping = new NameValueCollection();

            if (_settings["SpCharList"] != null && _settings["SpCharValue"] != null)
            {
                arrSpecialcharlist = _settings["SpCharList"].ToString().Split(',');
                arrSpecialcharValue = _settings["SpCharValue"].ToString().Split(',');
            }
        }

        //public XDocument ToXml(string graphName, ref List<IDataObject> dataObjects, GraphMap gp)
        //{
        //    return ToXml(graphName, ref dataObjects);
        //}


        //public override XDocument ToXml(string graphName, ref List<IDataObject> dataObjects)
        //{
        //    XDocument rdfDoc = null;


        //    _rdfXml = new XElement(RDF_NS + "JSON-LD",
        //      new XAttribute(XNamespace.Xmlns + "json-ld", RDF_NS),
        //      new XAttribute(XNamespace.Xmlns + "owl", OWL_NS),
        //      new XAttribute(XNamespace.Xmlns + "xsd", XSD_NS),
        //      new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));


        //    //JD.Serialize();

        //    //_rdfXml = JsonConvert.SerializeObject(JD).ToXElement();

        //    //rdfDoc = new XDocument(_rdfXml);

        //    //return rdfDoc;

        //    try
        //    {
        //        _graphMap = _mapping.FindGraphMap(graphName);

        //        if (_graphMap != null && _graphMap.classTemplateMaps.Count > 0 &&
        //          dataObjects != null && dataObjects.Count > 0)
        //        {
        //            string baseUri = _settings["GraphBaseUri"];
        //            string project = _settings["ProjectName"];
        //            string app = _settings["ApplicationName"];
        //            string appBaseUri = Utility.FormEndpointBaseURI(_uriMaps, baseUri, project, app);

        //            _graphBaseUri = appBaseUri + _graphMap.name + "/";
        //            _dataObjects = dataObjects;
        //            rdfDoc = new XDocument(BuildRdfXml());
        //        }
        //        else
        //        {
        //            rdfDoc = new XDocument(_rdfXml);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error("Error in ToXml: " + ex);
        //        throw ex;
        //    }

        //    return rdfDoc;
        //}




        public override XDocument ToXml(string graphName, ref List<IDataObject> dataObjects)
        {
            try
            {

                string baseUri = _settings["GraphBaseUri"];

                string app = _settings["ApplicationName"].ToLower();
                string proj = _settings["ProjectName"].ToLower();

                string appBaseUri = Utility.FormEndpointBaseURI(_uriMaps, baseUri, proj, app);

                // GraphMap graphMap = _mapping.FindGraphMap(graphName);

                TipMap tipMap = _tipMapping.FindTipMap(graphName);


                //_graphBaseUri = appBaseUri + graphMap.name + "/";
                _graphBaseUri = appBaseUri + tipMap.name + "/";
                _dataObjects = dataObjects;
                //rdfDoc = new XDocument(BuildRdfXml());


                //createFields(tipMap);
                createFields();


                string resource = graphName.ToLower();

                DataItems dataItems = new DataItems()
                {
                    total = this.Count,
                    start = this.Start,
                    items = new List<DataItem>()
                };

                if (dataObjects.Count > 0)
                {
                    DataObject dataObject = FindGraphDataObject(graphName);
                    dataItems.version = dataObject.version;

                    if (dataObject == null)
                    {
                        return new XDocument();
                    }

                    bool showNullValue = _settings["ShowJsonNullValues"] != null &&
                      _settings["ShowJsonNullValues"].ToString() == "True";

                    for (int i = 0; i < dataObjects.Count; i++)
                    {
                        IDataObject dataObj = dataObjects[i];

                        if (dataObj != null)
                        {
                            if (i == 0)
                            {
                                dataItems.type = graphName;
                            }

                            DataItem dataItem = new DataItem()
                            {
                                properties = new Dictionary<string, object>(),
                            };

                            if (dataObj is GenericDataObject)
                            {
                                dataItem.hasContent = ((GenericDataObject)dataObj).HasContent;
                            }

                            bool isContentObject = false;
                            if (dataObj is IContentObject)
                            {
                                dataItem.hasContent = true;
                                isContentObject = true;
                            }

                            if (isContentObject)
                            {
                                MemoryStream stream = ((IContentObject)dataObj).Content.ToMemoryStream();
                                byte[] data = stream.ToArray();
                                string base64Content = Convert.ToBase64String(data);
                                dataItem.content = base64Content;
                            }

                            foreach (KeyProperty keyProperty in dataObject.keyProperties)
                            {
                                DataProperty dataProperty = dataObject.dataProperties.Find(x => keyProperty.keyPropertyName.ToLower() == x.propertyName.ToLower());

                                if (dataProperty != null)
                                {
                                    object value = dataObj.GetPropertyValue(keyProperty.keyPropertyName);

                                    if (value != null)
                                    {
                                        if (dataProperty.dataType == DataType.Char ||
                                            dataProperty.dataType == DataType.DateTime ||
                                            dataProperty.dataType == DataType.Date ||
                                            dataProperty.dataType == DataType.String ||
                                            dataProperty.dataType == DataType.TimeStamp)
                                        {
                                            string valueStr = Convert.ToString(value);
                                            valueStr = Utility.ConvertSpecialCharOutbound(valueStr, arrSpecialcharlist, arrSpecialcharValue);  //Handling special Characters here.

                                            if (dataProperty.dataType == DataType.DateTime ||
                                                dataProperty.dataType == DataType.Date)
                                                valueStr = Utility.ToXsdDateTime(valueStr);

                                            value = valueStr;
                                        }
                                    }
                                    else
                                    {
                                        value = string.Empty;
                                    }

                                    if (!string.IsNullOrEmpty(dataItem.id))
                                    {
                                        dataItem.id += dataObject.keyDelimeter;
                                    }

                                    dataItem.id += value;
                                }
                            }

                            foreach (DataProperty dataProperty in dataObject.dataProperties)
                            {
                                if (!dataProperty.isHidden)
                                {
                                    object value = dataObj.GetPropertyValue(dataProperty.propertyName);

                                    if (value != null)
                                    {
                                        if (dataProperty.dataType == DataType.Char ||
                                              dataProperty.dataType == DataType.DateTime ||
                                              dataProperty.dataType == DataType.Date ||
                                              dataProperty.dataType == DataType.String ||
                                              dataProperty.dataType == DataType.TimeStamp)
                                        {
                                            string valueStr = Convert.ToString(value);

                                            if (dataProperty.dataType == DataType.DateTime ||
                                                dataProperty.dataType == DataType.Date)
                                                valueStr = Utility.ToXsdDateTime(valueStr);

                                            value = valueStr;
                                        }

                                        string dataParameterName = graphName.ToUpper() + "." + dataProperty.propertyName;

                                        if (tableMapping[dataParameterName] != null)
                                        {
                                            dataItem.properties.Add(tableMapping.Get(dataParameterName).ToString(), value);
                                        }
                                    }
                                }
                                else if (showNullValue)
                                {
                                    dataItem.properties.Add(dataProperty.propertyName, null);
                                }
                            }

                            if (_settings["DisplayLinks"].ToLower() == "true")
                            {
                                string itemHref = String.Format("{0}/{1}", BaseURI, dataItem.id);

                                dataItem.links = new List<Link> {
                                                              new Link {
                                                                href = itemHref,
                                                                rel = "self"
                                                              }
                                                            };

                                foreach (DataRelationship dataRelationship in dataObject.dataRelationships)
                                {
                                    long relObjCount = 0;
                                    bool validateLinks = (_settings["ValidateLinks"].ToLower() == "true");

                                    if (validateLinks)
                                    {
                                        //TODO:
                                        //relObjCount = _dataLayer.GetRelatedCount(dataObj, dataRelationship.relatedObjectName);
                                    }

                                    // only add link for related object that has data
                                    if (!validateLinks || relObjCount > 0)
                                    {
                                        string relObj = dataRelationship.relatedObjectName.ToLower();
                                        string relName = dataRelationship.relationshipName.ToLower();

                                        Link relLink = new Link()
                                        {
                                            href = String.Format("{0}/{1}", itemHref, relName),
                                            rel = relObj
                                        };

                                        dataItem.links.Add(relLink);
                                    }
                                }
                            }

                            dataItems.items.Add(dataItem);
                        }
                    }
                }

                dataItems.limit = dataItems.items.Count;

                if (dataItems.limit == 0) //Blank data item must have atleast version and type
                {
                    DataObject dataObject = FindGraphDataObject(graphName);
                    dataItems.version = dataObject.version;
                    dataItems.type = graphName;
                }

                string xml = Utility.SerializeDataContract<DataItems>(dataItems);
                XElement xElement = XElement.Parse(xml);
                return new XDocument(xElement);
            }
            catch (Exception e)
            {
                _logger.Error("Error creating JSON content: " + e);
                throw e;
            }
        }

        public override XDocument ToXml(string graphName, ref List<IDataObject> dataObjects, string className, string classIdentifier)
        {
            return ToXml(graphName, ref dataObjects);

            //XDocument dtoDoc = null;

            //try
            //{
            //    GraphMap graphMap = _mapping.FindGraphMap(graphName);
            //    //dtoDoc = ToXml(graphMap, ref dataObjects, className, classIdentifier);
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error("Error in ToXml: " + ex);
            //    throw ex;
            //}

            //return dtoDoc;
        }

        public override List<IDataObject> ToDataObjects(string graphName, ref XDocument xml)
        {
            try
            {
                List<IDataObject> dataObjects = new List<IDataObject>();
                DataObject objectType = FindGraphDataObject(graphName);

                if (objectType != null)
                {
                    DataItems dataItems = Utility.DeserializeDataContract<DataItems>(xml.ToString());

                    foreach (DataItem dataItem in dataItems.items)
                    {
                        if (dataItem.id != null)
                        {
                            dataItem.id = Utility.ConvertSpecialCharInbound(dataItem.id, arrSpecialcharlist, arrSpecialcharValue);  //Handling special Characters here.
                        }
                        else // if id doesn't exist, make it from key properties.
                        {
                            if (objectType.keyProperties.Count == 1)
                            {
                                string keyProp = objectType.keyProperties[0].keyPropertyName;
                                //object id = dataItem.properties[keyProp];

                                //if (id == null || id.ToString() == string.Empty)
                                //{
                                //  throw new Exception("Value of key property: " + keyProp + " cannot be null.");
                                //}

                                //dataItem.id = id.ToString();
                                if (dataItem.properties.ContainsKey(keyProp))
                                {
                                    object id = dataItem.properties[keyProp];

                                    if (id != null)
                                    {
                                        dataItem.id = id.ToString();
                                    }
                                }
                            }
                            else
                            {
                                StringBuilder builder = new StringBuilder();

                                foreach (KeyProperty keyProp in objectType.keyProperties)
                                {
                                    string propName = objectType.keyProperties[0].keyPropertyName;
                                    object propValue = dataItem.properties[propName];

                                    // it is acceptable to have some key property values to be null but not all
                                    if (propValue == null)
                                        propValue = string.Empty;

                                    builder.Append(objectType.keyDelimeter + propValue);
                                }

                                builder.Remove(0, objectType.keyDelimeter.Length);

                                if (builder.Length == 0)
                                {
                                    throw new Exception("Invalid identifier.");
                                }

                                dataItem.id = builder.ToString();
                            }
                        }

                        SerializableDataObject dataObject = new SerializableDataObject();
                        dataObject.Type = objectType.objectName;
                        dataObject.Id = dataItem.id;

                        if (objectType.hasContent)
                        {
                            string base64Content = dataItem.content;

                            if (!String.IsNullOrEmpty(base64Content))
                            {
                                dataObject.Content = base64Content.ToMemoryStream();
                                dataObject.Content.Position = 0;
                                dataObject.HasContent = true;
                                dataObject.ContentType = dataItem.contentType;
                            }
                        }

                        //
                        // set key properties from id
                        //
                        if (objectType.keyProperties.Count == 1)
                        {
                            dataObject.SetPropertyValue(objectType.keyProperties[0].keyPropertyName, dataItem.id);
                        }
                        else if (objectType.keyProperties.Count > 1)
                        {
                            string[] idParts = dataItem.id.Split(new string[] { objectType.keyDelimeter }, StringSplitOptions.None);

                            for (int i = 0; i < objectType.keyProperties.Count; i++)
                            {
                                string keyProp = objectType.keyProperties[i].keyPropertyName;
                                string keyValue = idParts[i];

                                dataObject.SetPropertyValue(keyProp, keyValue);
                            }
                        }

                        //
                        // set data properties
                        //
                        foreach (var pair in dataItem.properties)
                        {
                            dataObject.SetPropertyValue(pair.Key, pair.Value);
                        }

                        dataObjects.Add(dataObject);
                    }
                }

                return dataObjects;
            }
            catch (Exception e)
            {
                string message = "Error marshalling data items to data objects." + e;
                _logger.Error(message);
                throw new Exception(message);
            }
        }

        #region helper methods
        private DataObject FindGraphDataObject(string dataObjectName)
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

        private bool IsNumeric(DataType dataType)
        {
            return (dataType == DataType.Decimal ||
                    dataType == DataType.Single ||
                    dataType == DataType.Double ||
                    dataType == DataType.Int16 ||
                    dataType == DataType.Int32 ||
                    dataType == DataType.Int64);
        }
        #endregion


        //////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////


        private XElement BuildRdfXml()
        {
            ClassTemplateMap classTemplateMap = _graphMap.classTemplateMaps.First();

            

            if (classTemplateMap != null)
            {
                ClassMap classMap = classTemplateMap.classMap;
                List<TemplateMap> templateMaps = classTemplateMap.templateMaps;

                if (classMap != null)
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < 1; dataObjectIndex++)
                    // for (int dataObjectIndex = 0; dataObjectIndex < _dataObjects.Count; dataObjectIndex++)
                    {
                        bool hasRelatedProperty;
                        List<string> classIdentifiers = GetClassIdentifiers(classMap, dataObjectIndex, out hasRelatedProperty);

                        ProcessOutboundClass(dataObjectIndex, String.Empty, String.Empty, true, classIdentifiers, hasRelatedProperty,
                          classTemplateMap.classMap, classTemplateMap.templateMaps);
                    }
                }
            }

            return _rdfXml;
        }

        private void ProcessOutboundClass(int dataObjectIndex, string startClassName, string startClassIdentifier, bool isRootClass,
            List<string> classIdentifiers, bool hasRelatedProperty, ClassMap classMap, List<TemplateMap> templateMaps)
        {
            string className = Utility.TitleCase(classMap.name);
            string baseUri = _graphBaseUri + className + "/";
            string classId = classMap.id.Substring(classMap.id.IndexOf(":") + 1);

            for (int classIdentifierIndex = 0; classIdentifierIndex < classIdentifiers.Count; classIdentifierIndex++)
            {
                string classIdentifier = classIdentifiers[classIdentifierIndex];

                if (String.IsNullOrEmpty(startClassIdentifier) || className != startClassName || classIdentifier == startClassIdentifier)
                {
                    XElement individualElement = CreateIndividualElement(baseUri, classId, classIdentifier);

                    if (individualElement != null)
                    {
                        _rdfXml.Add(individualElement);


                        //JLD.name.id = baseUri + classIdentifier;
                        //JLD.AddListType(classId);

                        // add primary classification template
                        if (isRootClass && _primaryClassificationStyle == ClassificationStyle.Both)
                        {
                            TemplateMap classificationTemplate = _classificationConfig.TemplateMap;
                            CreateTemplateElement(dataObjectIndex, startClassName, startClassIdentifier, baseUri, classIdentifier,
                              classIdentifierIndex, classificationTemplate, hasRelatedProperty);
                        }

                        ProcessOutboundTemplates(startClassName, startClassIdentifier, dataObjectIndex, individualElement, templateMaps,
                          baseUri, classIdentifier, classIdentifierIndex, hasRelatedProperty);
                    }
                }
            }
        }

        private void CreateTemplateElement(int dataObjectIndex, string startClassName, string startClassIdentifier, string baseUri,
          string classIdentifier, int classIdentifierIndex, TemplateMap templateMap, bool classIdentifierHasRelatedProperty)
        {
            string classInstance = baseUri + classIdentifier;
            IDataObject dataObject = _dataObjects[dataObjectIndex];
            string templateId = templateMap.id.Replace(TPL_PREFIX, TPL_NS.NamespaceName);
            List<RoleMap> propertyRoles = new List<RoleMap>();
            XElement baseTemplateElement = new XElement(OWL_THING);
            StringBuilder baseValues = new StringBuilder(templateMap.id);
            List<RoleMap> classRoles = new List<RoleMap>();

            baseTemplateElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, templateId)));

            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
                string roleId = roleMap.id.Substring(roleMap.id.IndexOf(":") + 1);
                XElement roleElement = new XElement(TPL_NS + roleId);

                switch (roleMap.type)
                {
                    case RoleType.Possessor:
                        roleElement.Add(new XAttribute(RDF_RESOURCE, classInstance));
                        baseTemplateElement.Add(roleElement);
                        baseValues.Append(classIdentifier);
                        break;

                    case RoleType.FixedValue:
                        string dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
                        roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
                        roleElement.Add(new XText(roleMap.value));
                        baseTemplateElement.Add(roleElement);
                        baseValues.Append(roleMap.value);
                        break;

                    case RoleType.Reference:
                        if (roleMap.classMap != null)
                        {
                            classRoles.Add(roleMap);
                        }
                        else
                        {
                            string value = GetReferenceRoleValue(roleMap);
                            roleElement.Add(new XAttribute(RDF_RESOURCE, value));
                            baseTemplateElement.Add(roleElement);
                            baseValues.Append(roleMap.value);
                        }
                        break;

                    case RoleType.Property:
                    case RoleType.DataProperty:
                    case RoleType.ObjectProperty:
                        if (String.IsNullOrEmpty(roleMap.propertyName))
                        {
                            throw new Exception("No data property mapped to role [" + startClassName + "." + templateMap.name + "." + roleMap.name + "]");
                        }
                        propertyRoles.Add(roleMap);
                        break;
                }
            }

            if (propertyRoles.Count > 0)  // property template
            {
                bool isTemplateValid = true;  // template is not valid when value list uri is empty
                List<List<XElement>> multiPropertyElements = new List<List<XElement>>();

                // create property elements
                foreach (RoleMap propertyRole in propertyRoles)
                {
                    List<XElement> propertyElements = new List<XElement>();
                    multiPropertyElements.Add(propertyElements);

                    string[] propertyParts = propertyRole.propertyName.Split('.');
                    string propertyName = propertyParts[propertyParts.Length - 1];

                    int lastDotPos = propertyRole.propertyName.LastIndexOf('.');
                    string objectPath = propertyRole.propertyName.Substring(0, lastDotPos);

                    if (propertyParts.Length == 2)  // direct property
                    {
                        string propertyValue = Convert.ToString(dataObject.GetPropertyValue(propertyName));
                        XElement propertyElement = CreatePropertyElement(propertyRole, propertyValue);

                        if (propertyElement == null)
                        {
                            isTemplateValid = false;
                            break;
                        }

                        propertyElements.Add(propertyElement);
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
                            XElement propertyElement = CreatePropertyElement(propertyRole, propertyValue);

                            if (propertyElement == null)
                            {
                                isTemplateValid = false;
                                break;
                            }

                            propertyElements.Add(propertyElement);
                        }
                        else  // related property is property map
                        {
                            foreach (IDataObject relatedObject in relatedObjects)
                            {
                                string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
                                XElement propertyElement = CreatePropertyElement(propertyRole, propertyValue);

                                if (propertyElement == null)
                                {
                                    isTemplateValid = false;
                                    break;
                                }

                                propertyElements.Add(propertyElement);
                            }

                            if (!isTemplateValid) break;
                        }
                    }
                }

                if (isTemplateValid)
                {
                    // add property elements to template element(s)
                    if (multiPropertyElements.Count > 0 && multiPropertyElements[0].Count > 0)
                    {
                        // enforce dotNetRDF to store/retrieve templates in order as expressed in RDF
                        string hashPrefixFormat = Regex.Replace(multiPropertyElements[0].Count.ToString(), "\\d", "0") + "0";

                        for (int i = 0; i < multiPropertyElements[0].Count; i++)
                        {
                            XElement templateElement = new XElement(baseTemplateElement);
                            _rdfXml.Add(templateElement);

                            StringBuilder templateValue = new StringBuilder(baseValues.ToString());
                            for (int j = 0; j < multiPropertyElements.Count; j++)
                            {
                                XElement propertyElement = multiPropertyElements[j][i];
                                templateElement.Add(propertyElement);

                                if (!String.IsNullOrEmpty(propertyElement.Value))
                                    templateValue.Append(propertyElement.Value);
                                else
                                    templateValue.Append(propertyElement.Attribute(RDF_RESOURCE).Value);
                            }

                            string hashCode = Utility.MD5Hash(templateValue.ToString());
                            hashCode = i.ToString(hashPrefixFormat) + hashCode.Substring(hashPrefixFormat.Length);
                            templateElement.Add(new XAttribute(RDF_ABOUT, hashCode));
                        }
                    }
                }
            }
            else if (classRoles.Count > 0)  // relationship template with known class role
            {
                bool isTemplateValid = false;  // template is valid when there is at least one class referernce identifier that is not null
                Dictionary<RoleMap, List<string>> relatedClassRoles = new Dictionary<RoleMap, List<string>>();

                foreach (RoleMap classRole in classRoles)
                {
                    bool refClassHasRelatedProperty;
                    List<string> refClassIdentifiers = GetClassIdentifiers(classRole.classMap, dataObjectIndex, out refClassHasRelatedProperty);

                    if (refClassHasRelatedProperty)
                    {
                        relatedClassRoles[classRole] = refClassIdentifiers;
                    }
                    else
                    {
                        string refClassIdentifier = refClassIdentifiers.First();

                        if (!String.IsNullOrEmpty(refClassIdentifier))
                        {
                            isTemplateValid = true;
                            baseValues.Append(refClassIdentifier);

                            string roleId = classRole.id.Substring(classRole.id.IndexOf(":") + 1);
                            XElement roleElement = new XElement(TPL_NS + roleId);
                            roleElement.Add(new XAttribute(RDF_RESOURCE, _graphBaseUri +
                              Utility.TitleCase(classRole.classMap.name) + "/" + refClassIdentifier));
                            baseTemplateElement.Add(roleElement);
                        }

                        ClassTemplateMap relatedClassTemplateMap = _graphMap.GetClassTemplateMap(classRole.classMap.id, classRole.classMap.index);

                        if (relatedClassTemplateMap != null && relatedClassTemplateMap.classMap != null)
                        {
                            ProcessOutboundClass(dataObjectIndex, startClassName, startClassIdentifier, false, refClassIdentifiers,
                              refClassHasRelatedProperty, relatedClassTemplateMap.classMap, relatedClassTemplateMap.templateMaps);
                        }
                    }
                }

                if (relatedClassRoles.Count > 0)
                {
                    string refClassBaseValues = baseValues.ToString();

                    // enforce dotNetRDF to store/retrieve templates in order as expressed in RDF
                    string hashPrefixFormat = Regex.Replace(relatedClassRoles.Count.ToString(), "\\d", "0") + "0";

                    foreach (var pair in relatedClassRoles)
                    {
                        RoleMap classRole = pair.Key;
                        List<string> refClassIdentifiers = pair.Value;

                        string roleId = classRole.id.Substring(classRole.id.IndexOf(":") + 1);
                        string baseRelatedClassUri = _graphBaseUri + Utility.TitleCase(classRole.classMap.name) + "/";

                        for (int i = 0; i < refClassIdentifiers.Count; i++)
                        {
                            string refClassIdentifier = refClassIdentifiers[i];

                            if (!String.IsNullOrEmpty(refClassIdentifier))
                            {
                                XElement refBaseTemplateElement = new XElement(baseTemplateElement);

                                string hashCode = Utility.MD5Hash(refClassBaseValues + refClassIdentifier);
                                hashCode = i.ToString(hashPrefixFormat) + hashCode.Substring(hashPrefixFormat.Length);
                                refBaseTemplateElement.Add(new XAttribute(RDF_ABOUT, hashCode));

                                XElement roleElement = new XElement(TPL_NS + roleId);
                                roleElement.Add(new XAttribute(RDF_RESOURCE, baseRelatedClassUri + refClassIdentifier));
                                refBaseTemplateElement.Add(roleElement);
                                _rdfXml.Add(refBaseTemplateElement);
                            }
                        }

                        ClassTemplateMap relatedClassTemplateMap = _graphMap.GetClassTemplateMap(classRole.classMap.id, classRole.classMap.index);

                        if (relatedClassTemplateMap != null && relatedClassTemplateMap.classMap != null)
                        {
                            ProcessOutboundClass(dataObjectIndex, startClassName, startClassIdentifier, false, refClassIdentifiers,
                              true, relatedClassTemplateMap.classMap, relatedClassTemplateMap.templateMaps);
                        }
                    }
                }
                else if (isTemplateValid)
                {
                    string hashCode = Utility.MD5Hash(baseValues.ToString());
                    baseTemplateElement.Add(new XAttribute(RDF_ABOUT, hashCode));
                    _rdfXml.Add(baseTemplateElement);
                }
            }
            else  // relationship template with no class role (e.g. primary classification template)
            {
                string hashCode = Utility.MD5Hash(baseValues.ToString());
                baseTemplateElement.Add(new XAttribute(RDF_ABOUT, hashCode));
                _rdfXml.Add(baseTemplateElement);
            }
        }


        private void ProcessOutboundTemplates(string startClassName, string startClassIdentifier, int dataObjectIndex, XElement individualElement,
          List<TemplateMap> templateMaps, string baseUri, string classIdentifier, int classIdentifierIndex, bool hasRelatedProperty)
        {
            if (templateMaps != null && templateMaps.Count > 0)
            {
                foreach (TemplateMap templateMap in templateMaps)
                {
                    if ((_secondaryClassificationStyle == ClassificationStyle.Type ||
                          _secondaryClassificationStyle == ClassificationStyle.Both) &&
                        _classificationConfig.TemplateIds.Contains(templateMap.id))
                    {
                        foreach (RoleMap roleMap in templateMap.roleMaps)
                        {
                            if (roleMap.type == RoleType.Reference)
                            {
                                string value = GetReferenceRoleValue(roleMap);
                                individualElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, value)));
                            }
                        }

                        continue;
                    }

                    CreateTemplateElement(dataObjectIndex, startClassName, startClassIdentifier, baseUri, classIdentifier,
                      classIdentifierIndex, templateMap, hasRelatedProperty);
                }
            }
        }

        private XElement CreatePropertyElement(RoleMap propertyRole, string propertyValue)
        {
            XElement propertyElement = new XElement(TPL_NS + propertyRole.id.Replace(TPL_PREFIX, String.Empty));

            if (String.IsNullOrEmpty(propertyRole.valueListName))
            {
                if (String.IsNullOrEmpty(propertyValue))
                {
                    propertyElement.Add(new XAttribute(RDF_RESOURCE, QUALIFIED_RDF_NIL));
                }
                else
                {
                    propertyElement.Add(new XAttribute(RDF_DATATYPE,
                      propertyRole.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName)));

                    if (propertyRole.dataType.Contains("dateTime"))
                        propertyValue = Utility.ToXsdDateTime(propertyValue);

                    propertyElement.Add(new XText(propertyValue));
                }
            }
            else  // resolve value list to uri
            {
                propertyValue = _mapping.ResolveValueList(propertyRole.valueListName, propertyValue);

                if (String.IsNullOrEmpty(propertyValue))
                {
                    return null;
                }

                propertyValue = propertyValue.Replace(RDL_PREFIX, RDL_NS.NamespaceName);
                propertyElement.Add(new XAttribute(RDF_RESOURCE, propertyValue));
            }

            return propertyElement;
        }

        private string GetReferenceRoleValue(RoleMap referenceRole)
        {
            string value = referenceRole.dataType;

            if (string.IsNullOrEmpty(value))
            {
                value = referenceRole.value;  // for backward compatibility
            }

            if (string.IsNullOrEmpty(value) || (!value.StartsWith(RDL_PREFIX) && !value.StartsWith(RDL_NS.NamespaceName)))
                throw new Exception("Role map [" + referenceRole.name + "] has invalid class reference.");

            return value.Replace(RDL_PREFIX, RDL_NS.NamespaceName);
        }

        private XElement CreateIndividualElement(string baseUri, string classId, string classIdentifier)
        {
            XElement individualElement = null;

            if (!String.IsNullOrEmpty(classIdentifier))
            {
                string individual = baseUri + classIdentifier;
                bool individualCreated = true;

                if (!_individualsCache.ContainsKey(classId))
                {
                    _individualsCache[classId] = new List<string> { individual };
                    individualCreated = false;
                }
                else if (!_individualsCache[classId].Contains(individual))
                {
                    _individualsCache[classId].Add(individual);
                    individualCreated = false;
                }

                if (!individualCreated)
                {
                    individualElement = new XElement(OWL_THING, new XAttribute(RDF_ABOUT, individual));
                    individualElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, RDL_NS.NamespaceName + classId)));
                }
            }

            return individualElement;
        }

        private void createFields()
        {
            tableMapping.Clear();

            foreach (TipMap tipMap in _tipMapping.tipMaps)
            {
                foreach (ParameterMap parameterMap in tipMap.parameterMaps)
                {
                    tableMapping.Add(parameterMap.dataPropertyName, parameterMap.name);

                }
            }
        }



        //private void createFields(GraphMap graph)
        //{
        //    tableMapping.Clear();

        //    foreach (ClassTemplateMap classTemplate in graph.classTemplateMaps)
        //    {
        //        if (classTemplate != null && classTemplate.templateMaps != null)
        //        {
        //            String className = Utility.TitleCase(graph.name);
        //            String classTemplateclassMapName = Utility.TitleCase(classTemplate.classMap.name);
        //            foreach (TemplateMap template in classTemplate.templateMaps)
        //            {
        //                foreach (RoleMap role in template.roleMaps)
        //                {
        //                    RoleType roleType = role.type;
        //                    // Cardinality cardinality = role.getCardinality();

        //                    if (roleType == null || roleType == RoleType.Property || roleType == RoleType.DataProperty ||
        //                                        roleType == RoleType.ObjectProperty || roleType == RoleType.FixedValue)
        //                    //|| (cardinality != null && cardinality == Cardinality.SELF))
        //                    {

        //                        String fieldName;
        //                        string colName;

        //                        fieldName = classTemplateclassMapName + '.' + template.name + "." + role.name;
        //                        colName = role.propertyName.Remove(0, className.Length + 1);
        //                        tableMapping.Add(colName, fieldName);
        //                        //tableMapping.Add(colName, role.parameterName);
        //                    }
        //                    //else if (role.getClazz() != null && (cardinality == null || cardinality == Cardinality.ONE_TO_ONE))
        //                    //{
        //                    //    String classId = role.getClazz().getId();
        //                    //    String clsPath = role.getClazz().getPath();
        //                    //    int clsIndex = role.getClazz().getIndex();


        //                    //    ClassTemplates relatedClassTemplates = getClassTemplates(graph, classId, clsPath);
        //                    //    createFields(fields, graph, clsPath, clsIndex);
        //                    //}
        //                }
        //            }
        //        }
        //    }
        //}

    }


}






















