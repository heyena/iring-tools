using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.ServiceModel.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using log4net;
using Microsoft.ServiceModel.Web;
using Newtonsoft.Json;
using Ninject;
using org.iringtools.adapter.projection;
using org.iringtools.dxfr.manifest;
using org.iringtools.library;
using org.iringtools.mapping;
using org.iringtools.utility;
using System.Web;

namespace org.iringtools.adapter
{
    public class DtoProvider : BaseProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DtoProvider));

        protected GraphMap _graphMap;
        protected string _fixedIdentifierBoundary = "#";
        private string _connSecurityDb;
        private int _siteID;
        DictionaryProvider dictionaryProvider;

        [Inject]
        public DtoProvider(NameValueCollection settings)
            : base(settings)
        {
            try
            {
                _connSecurityDb = settings["SecurityConnection"];
                _siteID = Convert.ToInt32(settings["SiteId"]);
                dictionaryProvider = new adapter.DictionaryProvider(settings);
            }
            catch (Exception e)
            {
                _logger.Error("Error initializing adapter provider: " + e.Message);
            }
        }

        public VersionInfo GetVersion()
        {
            System.Version version = this.GetType().Assembly.GetName().Version;

            return new org.iringtools.library.VersionInfo()
            {
                Major = version.Major,
                Minor = version.Minor,
                Build = version.Build,
                Revision = version.Revision
            };
        }

        public Key GetKey(GraphMap graphMap, ClassMap classMap, string identifier)
        {
            foreach (var classTemplateMap in graphMap.classTemplateMaps)
            {
                foreach (var templateMap in classTemplateMap.templateMaps)
                {
                    foreach (var roleMap in templateMap.roleMaps)
                    {
                        if ((roleMap.type == RoleType.Property ||
                             roleMap.type == RoleType.DataProperty ||
                             roleMap.type == RoleType.ObjectProperty) &&
                             identifier.ToLower() == roleMap.propertyName.ToLower())
                        {
                            Key key = new Key()
                            {
                                classId = classTemplateMap.classMap.id,
                                templateId = templateMap.id,
                                roleId = roleMap.id
                            };

                            return key;
                        }
                    }
                }
            }

            string error = string.Format(
              "Property [{0}], which is class identifier of class [{1}], is not mapped to any role in graph [{2}].",
              identifier, classMap.name, graphMap.name);

            throw new Exception(error);
        }

        public NameValueList GetScopeList()
        {
            NameValueList list = new NameValueList();

            try
            {
                if (_scopes != null)
                {
                    foreach (ScopeProject scope in _scopes)
                    {
                        list.Add(new ListItem()
                        {
                            Name = scope.DisplayName,
                            Value = scope.Name
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting scope list: " + ex);
                throw ex;
            }

            return list;
        }

        public NameValueList GetAppList(string scopeName)
        {
            NameValueList list = new NameValueList();

            try
            {
                if (_scopes != null)
                {
                    foreach (ScopeProject scope in _scopes)
                    {
                        if (scope.Name.ToLower() == scopeName.ToLower())
                        {
                            foreach (ScopeApplication app in scope.Applications)
                            {
                                list.Add(new ListItem()
                                {
                                    Name = app.DisplayName,
                                    Value = app.Name
                                });
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting application list: " + ex);
                throw ex;
            }

            return list;
        }

        public NameValueList GetGraphList(string scope, string app)
        {
            NameValueList list = new NameValueList();

            try
            {
                InitializeScope(scope, app, true);

                if (_mapping != null)
                {
                    foreach (GraphMap graph in _mapping.graphMaps)
                    {
                        list.Add(new ListItem()
                        {
                            Name = graph.name,
                            Value = graph.name
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting graph list: " + ex);
                throw ex;
            }

            return list;
        }

        public string GetDataMode(string scope, string app)
        {
            try
            {
                InitializeScope(scope, app);

                ScopeApplication application = GetApplication(scope, app);

                return application.DataMode.ToString();
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting application data mode: " + ex);
                throw ex;
            }
        }

        //Created on 21-Apr-15, took reference from GetDataMode(.,.)  
        public string GetDataModeByApplicationId(string applicationId)
        {
            string dataMode = string.Empty;
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@ApplicationId", Value = applicationId });

                dataMode = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spgDataModeByApplicationId", nvl);
                
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Graph by GraphID: " + ex);
            }
            return dataMode;
        }

        public CacheInfo GetCacheInfo(string scope, string app, string graph)
        {
            CacheInfo cacheInfo = new CacheInfo()
            {
                CacheEntries = new CacheEntries()
            };

            try
            {
                InitializeScope(scope, app);

                GraphMap graphMap = _mapping.graphMaps.Find(x => x.name.ToLower() == graph.ToLower());

                if (graphMap == null)
                {
                    throw new Exception("Graph [" + graph + "] not found.");
                }

                string objectName = graphMap.dataObjectName;
                ScopeApplication application = GetApplication(scope, app);

                if (application.CacheInfo != null && application.CacheInfo.CacheEntries != null)
                {
                    foreach (CacheEntry cacheEntry in application.CacheInfo.CacheEntries)
                    {
                        if (cacheEntry.ObjectName.ToLower() == objectName.ToLower())
                        {
                            cacheInfo.ImportURI = application.CacheInfo.ImportURI;
                            cacheInfo.Timeout = application.CacheInfo.Timeout;
                            cacheInfo.CacheEntries.Add(cacheEntry);

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting application cache information: " + ex);
                throw ex;
            }

            return cacheInfo;
        }
        public Manifest GetManifest(string scope, string app, string graph)
        {
            Manifest manifest = new Manifest()
            {
                graphs = new Graphs(),
                version = "2.3.1",
                valueListMaps = new ValueListMaps()
            };

            try
            {
                InitializeScope(scope, app);
                InitializeDataLayer();

                DataDictionary dataDictionary = _dataLayerGateway.GetDictionary();

                foreach (GraphMap graphMap in _mapping.graphMaps)
                {
                    Graph manifestGraph = null;

                    if (string.IsNullOrEmpty(graph) || graph.ToLower() == graphMap.name.ToLower())
                    {
                        manifestGraph = new Graph
                        {
                            classTemplatesList = new ClassTemplatesList(),
                            name = graphMap.name
                        };

                        manifest.graphs.Add(manifestGraph);
                    }
                    else
                    {
                        continue;
                    }

                    if (manifestGraph == null)
                    {
                        throw new Exception("Graph [" + graph + "] does not exist in mapping.");
                    }

                    string dataObjectName = graphMap.dataObjectName;
                    DataObject dataObject = null;

                    foreach (DataObject dataObj in dataDictionary.dataObjects)
                    {
                        if (dataObj.objectName.ToLower() == dataObjectName.ToLower())
                        {
                            dataObject = dataObj;
                            break;
                        }
                    }

                    if (dataObject == null)
                    {
                        throw new Exception("Data Object [" + dataObjectName + "] does not exist in data dictionary.");
                    }

                    foreach (var classTemplateMap in graphMap.classTemplateMaps)
                    {
                        ClassTemplates manifestClassTemplates = new ClassTemplates();
                        manifestGraph.classTemplatesList.Add(manifestClassTemplates);

                        ClassMap classMap = classTemplateMap.classMap;
                        List<TemplateMap> templateMaps = classTemplateMap.templateMaps;

                        Keys keys = new Keys();

                        if (templateMaps.Count > 0)
                        {
                            foreach (string identifier in classMap.identifiers)
                            {
                                Key key = GetKey(graphMap, classMap, identifier);
                                keys.Add(key);
                            }
                        }

                        Class manifestClass = new Class
                        {
                            id = classMap.id,
                            name = classMap.name,
                            keys = keys,
                            index = classMap.index,
                            path = classMap.path
                        };
                        manifestClassTemplates.@class = manifestClass;


                        foreach (TemplateMap templateMap in templateMaps)
                        {
                            Template manifestTemplate = new Template
                            {
                                roles = new Roles(),
                                id = templateMap.id,
                                name = templateMap.name,
                                transferOption = TransferOption.Desired,
                            };
                            manifestClassTemplates.templates.Add(manifestTemplate);

                            //loop thorough all roleMap and assign following nodes to roles.
                            foreach (RoleMap roleMap in templateMap.roleMaps)
                            {
                                string strDataType = "";
                                int intPrecision = 0;
                                int intScale = 0;

                                //find column name from roleMap.Use the column name to find DBdatatype, precision and scale from dataProperties.
                                if (roleMap.propertyName != null && dataObject != null)
                                {
                                    string strColumnName = roleMap.propertyName;
                                    strColumnName = strColumnName.Substring(strColumnName.LastIndexOf('.') + 1);


                                    DataProperty dataProperty = dataObject.dataProperties.Where(x => x.propertyName == strColumnName).FirstOrDefault();
                                    ExtensionProperty extensionProperty = null;
                                    if (dataProperty != null)
                                    {
                                        strDataType = dataProperty.dataType.ToString();
                                        intPrecision = Convert.ToInt16(dataProperty.precision);
                                        intScale = Convert.ToInt16(dataProperty.scale);
                                    }
                                    else
                                    {
                                        if (dataObject.extensionProperties != null)
                                        {
                                            extensionProperty = dataObject.extensionProperties.Where(x => x.propertyName == strColumnName).FirstOrDefault();
                                            if (extensionProperty != null)
                                            {
                                                strDataType = extensionProperty.dataType.ToString();
                                                intPrecision = Convert.ToInt16(extensionProperty.precision);
                                                intScale = Convert.ToInt16(extensionProperty.scale);
                                            }
                                        }
                                        else
                                        {
                                            _logger.Info("No Extension property in the dataObject ");
                                        }

                                    }
                                }

                                //assign valus to the following nodes.
                                Role manifestRole = new Role
                                {
                                    type = roleMap.type,
                                    id = roleMap.id,
                                    name = roleMap.name,
                                    dataType = roleMap.dataType,
                                    value = roleMap.value,
                                    dbDataType = strDataType,
                                    precision = intPrecision,
                                    scale = intScale,
                                };

                                manifestTemplate.roles.Add(manifestRole);

                                if (roleMap.type == RoleType.Property ||
                                    roleMap.type == RoleType.DataProperty ||
                                    roleMap.type == RoleType.ObjectProperty)
                                {
                                    if (!String.IsNullOrEmpty(roleMap.propertyName))
                                    {
                                        string[] propertyParts = roleMap.propertyName.Split('.');
                                        string objectName = propertyParts[propertyParts.Length - 2].Trim();
                                        string propertyName = propertyParts[propertyParts.Length - 1].Trim();
                                        DataObject dataObj = dataObject;

                                        if (propertyParts.Length < 2)
                                        {
                                            throw new Exception("Property [" + roleMap.propertyName + "] is invalid.");
                                        }
                                        else if (propertyParts.Length > 2) // related property
                                        {
                                            // find related object
                                            for (int i = 1; i < propertyParts.Length - 1; i++)
                                            {
                                                DataRelationship rel = dataObj.dataRelationships.Find(x => x.relationshipName.ToLower() == propertyParts[i].ToLower());
                                                if (rel == null)
                                                {
                                                    throw new Exception("Relationship [" + rel.relationshipName + "] does not exist.");
                                                }

                                                dataObj = dataDictionary.dataObjects.Find(x => x.objectName.ToLower() == rel.relatedObjectName.ToLower());
                                                if (dataObj == null)
                                                {
                                                    throw new Exception("Related object [" + rel.relatedObjectName + "] is not found.");
                                                }
                                            }
                                        }

                                        DataProperty dataProp = dataObj.dataProperties.Find(x => x.propertyName.ToLower() == propertyName.ToLower());
                                        ExtensionProperty extensionProp = null;
                                        if (dataProp == null)
                                        {
                                            extensionProp = dataObj.extensionProperties.Find(x => x.propertyName.ToLower() == propertyName.ToLower());
                                            if (extensionProp == null)
                                            {
                                                throw new Exception("Property [" + roleMap.propertyName + "] does not exist in data dictionary.");
                                            }
                                            else
                                            {
                                                manifestRole.dataLength = extensionProp.dataLength;
                                                if (manifestRole.dataType == "xsd:dateTime" || manifestRole.dataType == "xsd:date")
                                                {
                                                    if (extensionProp.dataType == DataType.Date)
                                                        manifestRole.dataType = "xsd:date";
                                                    else if (extensionProp.dataType == DataType.DateTime)
                                                        manifestRole.dataType = "xsd:dateTime";
                                                }
                                            }
                                        }
                                        else
                                        {

                                            manifestRole.dataLength = dataProp.dataLength;
                                            if (manifestRole.dataType == "xsd:dateTime" || manifestRole.dataType == "xsd:date")
                                            {
                                                if (dataProp.dataType == DataType.Date)
                                                    manifestRole.dataType = "xsd:date";
                                                else if (dataProp.dataType == DataType.DateTime)
                                                    manifestRole.dataType = "xsd:dateTime";
                                            }

                                            if (dataObj.isKeyProperty(propertyName))
                                            {
                                                manifestTemplate.transferOption = TransferOption.Required;
                                            }
                                        }
                                    }
                                }

                                if (roleMap.classMap != null)
                                {
                                    Cardinality cardinality = graphMap.GetCardinality(roleMap, _dictionary, _fixedIdentifierBoundary);
                                    manifestRole.cardinality = cardinality;

                                    manifestRole.@class = new Class
                                    {
                                        id = roleMap.classMap.id,
                                        name = roleMap.classMap.name,
                                        index = roleMap.classMap.index,
                                        path = roleMap.classMap.path
                                    };
                                }
                            }
                        }
                    }
                }

                manifest.valueListMaps = Utility.CloneDataContractObject<ValueListMaps>(_mapping.valueListMaps);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting manifest: " + ex);
                throw ex;
            }

            return manifest;
        }

        public Manifest GetManifest(string scope, string app)
        {
            return GetManifest(scope, app, null);
        }


        public Manifest GetManifestForUser(string userName, string graphId)
        {
            Manifest manifest = new Manifest()
            {
                graphs = new Graphs(),
                version = "2.3.1",
                valueListMaps = new ValueListMaps()
            };

            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@GraphId", Value = Convert.ToString(graphId) });
                System.Data.DataTable dt = DBManager.Instance.ExecuteStoredProcedure(_connSecurityDb, "spgNames", nvl);

                string graph = string.Empty; string scope = string.Empty;
                string appName = string.Empty; Guid applicationId;

                System.Data.DataRow row = dt.Rows[0];

                graph = Convert.ToString(row["GraphName"]);
                scope = Convert.ToString(row["ScopeName"]);
                appName = Convert.ToString(row["AppName"]);
                applicationId = Guid.Parse(Convert.ToString(row["AppId"]));

                nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@ApplicationId", Value = applicationId });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgValuelistforManifest", nvl);
                ValueListMaps valueListMaps = utility.Utility.Deserialize<ValueListMaps>(xmlString, true);

                nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@GraphId", Value = Convert.ToString(graphId) });

                byte[] xmlbyte = DBManager.Instance.ExecuteBytesQuery(_connSecurityDb, "spgGraphBinary", nvl);
                string bytesToXml = System.Text.Encoding.Default.GetString(xmlbyte);
                //mapping.Mapping _mapping = utility.Utility.Deserialize<mapping.Mapping>(bytesToXml, true); ;
                bytesToXml = bytesToXml.Replace("<graphMap>", "<graphMap xmlns=\"http://www.iringtools.org/mapping\">");
                GraphMap graphMap = utility.Utility.Deserialize<GraphMap>(bytesToXml, true); ;

                DatabaseDictionary dataDictionary = dictionaryProvider.GetDictionary(applicationId);

                //foreach (GraphMap graphMap in _mapping.graphMaps)
                //{
                Graph manifestGraph = null;

                //if (string.IsNullOrEmpty(graph) || graph.ToLower() == graphMap.name.ToLower())
                //{
                    manifestGraph = new Graph
                    {
                        classTemplatesList = new ClassTemplatesList(),
                        name = graphMap.name,
                        dataObjectName = graphMap.dataObjectName
                    };

                    manifest.graphs.Add(manifestGraph);
                //}
                //else
                //{
                //    continue;
                //}

                //if (manifestGraph == null)
                //{
                //    throw new Exception("Graph [" + graph + "] does not exist in mapping.");
                //}

                string dataObjectName = graphMap.dataObjectName;
                DataObject dataObject = null;

                foreach (DataObject dataObj in dataDictionary.dataObjects)
                {
                    if (dataObj.objectName.ToLower() == dataObjectName.ToLower())
                    {
                        dataObject = dataObj;
                        manifestGraph.dataObjectId = dataObj.dataObjectId.ToString();
                        break;
                    }
                }

                if (dataObject == null)
                {
                    throw new Exception("Data Object [" + dataObjectName + "] does not exist in data dictionary.");
                }

                     foreach (var classTemplateMap in graphMap.classTemplateMaps)
                    {
                        ClassTemplates manifestClassTemplates = new ClassTemplates();
                        manifestGraph.classTemplatesList.Add(manifestClassTemplates);

                        ClassMap classMap = classTemplateMap.classMap;
                        List<TemplateMap> templateMaps = classTemplateMap.templateMaps;

                        Keys keys = new Keys();

                        if (templateMaps.Count > 0)
                        {
                            foreach (string identifier in classMap.identifiers)
                            {
                                Key key = GetKey(graphMap, classMap, identifier);
                                keys.Add(key);
                            }
                        }

                        Class manifestClass = new Class
                        {
                            id = classMap.id,
                            name = classMap.name,
                            keys = keys,
                            index = classMap.index,
                            path = classMap.path
                        };
                        manifestClassTemplates.@class = manifestClass;


                        foreach (TemplateMap templateMap in templateMaps)
                        {
                            Template manifestTemplate = new Template
                            {
                                roles = new Roles(),
                                id = templateMap.id,
                                name = templateMap.name,
                                transferOption = TransferOption.Desired,
                            };
                            manifestClassTemplates.templates.Add(manifestTemplate);

                            //loop thorough all roleMap and assign following nodes to roles.
                            foreach (RoleMap roleMap in templateMap.roleMaps)
                            {
                                string strDataType = "";
                                int intPrecision = 0;
                                int intScale = 0;

                                //find column name from roleMap.Use the column name to find DBdatatype, precision and scale from dataProperties.
                                if (roleMap.propertyName != null && dataObject != null)
                                {
                                    string strColumnName = roleMap.propertyName;
                                    strColumnName = strColumnName.Substring(strColumnName.LastIndexOf('.') + 1);


                                    DataProperty dataProperty = dataObject.dataProperties.Where(x => x.propertyName == strColumnName).FirstOrDefault();
                                    ExtensionProperty extensionProperty = null;
                                    if (dataProperty != null)
                                    {
                                        strDataType = dataProperty.dataType.ToString();
                                        intPrecision = Convert.ToInt16(dataProperty.precision);
                                        intScale = Convert.ToInt16(dataProperty.scale);
                                    }
                                    else
                                    {
                                        if (dataObject.extensionProperties != null)
                                        {
                                            extensionProperty = dataObject.extensionProperties.Where(x => x.propertyName == strColumnName).FirstOrDefault();
                                            if (extensionProperty != null)
                                            {
                                                strDataType = extensionProperty.dataType.ToString();
                                                intPrecision = Convert.ToInt16(extensionProperty.precision);
                                                intScale = Convert.ToInt16(extensionProperty.scale);
                                            }
                                        }
                                        else
                                        {
                                            _logger.Info("No Extension property in the dataObject ");
                                        }
                                       
                                    }
                                }

                                //assign valus to the following nodes.
                                Role manifestRole = new Role
                                {
                                    type = roleMap.type,
                                    id = roleMap.id,
                                    name = roleMap.name,
                                    dataType = roleMap.dataType,
                                    value = roleMap.value,
                                    dbDataType = strDataType,
                                    precision = intPrecision,
                                    scale = intScale,
                                };

                                manifestTemplate.roles.Add(manifestRole);

                                if (roleMap.type == RoleType.Property ||
                                    roleMap.type == RoleType.DataProperty ||
                                    roleMap.type == RoleType.ObjectProperty)
                                {
                                    if (!String.IsNullOrEmpty(roleMap.propertyName))
                                    {
                                        string[] propertyParts = roleMap.propertyName.Split('.');
                                        string objectName = propertyParts[propertyParts.Length - 2].Trim();
                                        string propertyName = propertyParts[propertyParts.Length - 1].Trim();
                                        DataObject dataObj = dataObject;

                                        if (propertyParts.Length < 2)
                                        {
                                            throw new Exception("Property [" + roleMap.propertyName + "] is invalid.");
                                        }
                                        else if (propertyParts.Length > 2) // related property
                                        {
                                            // find related object
                                            for (int i = 1; i < propertyParts.Length - 1; i++)
                                            {
                                                DataRelationship rel = dataObj.dataRelationships.Find(x => x.relationshipName.ToLower() == propertyParts[i].ToLower());
                                                if (rel == null)
                                                {
                                                    throw new Exception("Relationship [" + rel.relationshipName + "] does not exist.");
                                                }

                                                dataObj = dataDictionary.dataObjects.Find(x => x.objectName.ToLower() == rel.relatedObjectName.ToLower());
                                                if (dataObj == null)
                                                {
                                                    throw new Exception("Related object [" + rel.relatedObjectName + "] is not found.");
                                                }
                                            }
                                        }

                                        DataProperty dataProp = dataObj.dataProperties.Find(x => x.propertyName.ToLower() == propertyName.ToLower());
                                        ExtensionProperty extensionProp = null;
                                        if (dataProp == null)
                                        {
                                            extensionProp = dataObj.extensionProperties.Find(x => x.propertyName.ToLower() == propertyName.ToLower());
                                            if (extensionProp == null)
                                            {
                                                throw new Exception("Property [" + roleMap.propertyName + "] does not exist in data dictionary.");
                                            }
                                            else
                                            {
                                                manifestRole.dataLength = extensionProp.dataLength;
                                                if (manifestRole.dataType == "xsd:dateTime" || manifestRole.dataType == "xsd:date")
                                                {
                                                    if (extensionProp.dataType == DataType.Date)
                                                        manifestRole.dataType = "xsd:date";
                                                    else if (extensionProp.dataType == DataType.DateTime)
                                                        manifestRole.dataType = "xsd:dateTime";
                                                }
                                            }
                                        }
                                        else
                                        {

                                            manifestRole.dataLength = dataProp.dataLength;
                                            if (manifestRole.dataType == "xsd:dateTime" || manifestRole.dataType == "xsd:date")
                                            {
                                                if (dataProp.dataType == DataType.Date)
                                                    manifestRole.dataType = "xsd:date";
                                                else if (dataProp.dataType == DataType.DateTime)
                                                    manifestRole.dataType = "xsd:dateTime";
                                            }

                                            if (dataObj.isKeyProperty(propertyName))
                                            {
                                                manifestTemplate.transferOption = TransferOption.Required;
                                            }
                                        }
                                    }
                                }

                                if (roleMap.classMap != null)
                                {
                                    Cardinality cardinality = graphMap.GetCardinality(roleMap, _dictionary, _fixedIdentifierBoundary);
                                    manifestRole.cardinality = cardinality;

                                    manifestRole.@class = new Class
                                    {
                                        id = roleMap.classMap.id,
                                        name = roleMap.classMap.name,
                                        index = roleMap.classMap.index,
                                        path = roleMap.classMap.path
                                    };
                                }
                            }
                        }
                    }
                //}

                manifest.valueListMaps = valueListMaps;
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  valueListMaps: " + ex);
            }
            return manifest;
        }


        public DataTransferIndices GetDataTransferIndicesWithManifest(string scope, string app, string graph, string hashAlgorithm, Manifest manifest)
        {
            DataTransferIndices dataTransferIndices = null;

            try
            {
                InitializeScope(scope, app);
                InitializeDataLayer();

                DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                dtoProjectionEngine.dataLayerGateway = _dataLayerGateway;

                _graphMap = _mapping.graphMaps.Find(x => x.name.ToLower() == graph.ToLower());
                DataFilter filter = GetPresetFilters(dtoProjectionEngine);

                BuildCrossGraphMap(manifest, graph);
                DataObject dataObject = _dictionary.dataObjects.Find(o => o.objectName == _graphMap.dataObjectName);

                if (_settings["MultiGetDTIs"] == null || bool.Parse(_settings["MultiGetDTIs"]))
                {
                    _logger.Debug("Running multi-threaded mode.");
                    dataTransferIndices = MultiGetDataTransferIndices(filter);
                }
                else
                {
                    _logger.Debug("Running single-threaded mode.");
                    List<IDataObject> tmpDataObjects = PageDataObjects(dataObject, filter);
                    List<IDataObject> dataObjects = ProcessRollups(dataObject, tmpDataObjects, filter);

                    _logger.Debug("Transforming into DTI");
                    dataTransferIndices = dtoProjectionEngine.GetDataTransferIndices(_graphMap, dataObjects, String.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting data transfer indices: " + ex);
                throw ex;
            }

            return dataTransferIndices;
        }

        private decimal ToDecimal(string fraction)
        {
            string[] split = fraction.Split(new char[] { '/' });
            decimal a, b;

            if (decimal.TryParse(split[0], out a) && decimal.TryParse(split[1], out b))
            {
                if (split.Length == 2)
                {
                    return (decimal)a / b;
                }
            }

            return 0;
        }

        /*
         * Sample filter with rollups:
         * 
        <?xml version="1.0" encoding="utf-8"?>
        <dataFilter xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://www.iringtools.org/data/filter">
          <rollupExpressions>
            <rollupExpression>
              <groupBy>ID</groupBy>
              <rollups>
                <rollup>
                  <propertyName>NOMDIAMETER</propertyName>
                  <type>Max</type>
                </rollup>
                <rollup>
                  <propertyName>AREA</propertyName>
                  <type>First</type>
                </rollup>
              </rollups>
            </rollupExpression>
            <rollupExpression>
              <groupBy>AREA</groupBy>
              <rollups>
                <rollup>
                  <propertyName>NOMDIAMETER</propertyName>
                  <type>Sum</type>
                </rollup>
              </rollups>
            </rollupExpression>
          </rollupExpressions>
        </dataFilter>
         */
        private List<IDataObject> ProcessRollups(DataObject objDef, List<IDataObject> dataObjects, DataFilter filter)
        {
            if (filter != null && filter.RollupExpressions != null && filter.RollupExpressions.Count > 0)
            {
                foreach (RollupExpression rollupExpr in filter.RollupExpressions)
                {
                    DataProperty groupBy = objDef.dataProperties.Find(x => x.propertyName.ToLower() == rollupExpr.GroupBy.ToLower());

                    // group data object indices by groupBy property values 
                    bool[] processedIndices = new bool[dataObjects.Count];
                    List<List<int>> groups = new List<List<int>>();

                    for (int i = 0; i < dataObjects.Count; i++)
                    {
                        if (processedIndices[i])
                            continue;

                        string value = Convert.ToString(dataObjects[i].GetPropertyValue(groupBy.propertyName));
                        processedIndices[i] = true;
                        List<int> newGroup = new List<int>() { i };
                        groups.Add(newGroup);

                        for (int j = i + 1; j < dataObjects.Count; j++)
                        {
                            string v = Convert.ToString(dataObjects[j].GetPropertyValue(groupBy.propertyName));

                            if (v == value)
                            {
                                processedIndices[j] = true;
                                newGroup.Add(j);
                            }
                        }
                    }

                    // apply rollups to each group
                    List<IDataObject> rollupObjects = new List<IDataObject>();

                    foreach (Rollup rollup in rollupExpr.Rollups)
                    {
                        DataProperty rollupProp = objDef.dataProperties.Find(x => x.propertyName.ToLower() == rollup.PropertyName.ToLower());

                        for (int k = 0; k < groups.Count; k++)
                        {
                            List<int> group = groups[k];
                            IDataObject rollupObject = dataObjects[group[0]];

                            switch (rollup.Type)
                            {
                                case RollupType.Null:
                                    {
                                        rollupObject.SetPropertyValue(rollupProp.propertyName, null);
                                        break;
                                    }
                                case RollupType.Max:
                                    {
                                        object max = dataObjects[group[0]].GetPropertyValue(rollupProp.propertyName);
                                        int maxIndex = 0;

                                        if (IsNumeric(rollupProp))
                                        {
                                            if (max == null)
                                            {
                                                max = Decimal.Negate(Decimal.MaxValue);
                                            }
                                            else
                                            {
                                                if (max.ToString().Contains("/"))
                                                {
                                                    max = ToDecimal(max.ToString());
                                                }
                                                else
                                                {
                                                    max = Convert.ToDecimal(max);
                                                }
                                            }

                                            for (int l = 1; l < group.Count; l++)
                                            {
                                                object value = dataObjects[group[l]].GetPropertyValue(rollupProp.propertyName);

                                                if (value != null)
                                                {
                                                    if (value.ToString().Contains("/"))
                                                    {
                                                        value = ToDecimal(value.ToString());
                                                    }
                                                    else
                                                    {
                                                        value = Convert.ToDecimal(value);
                                                    }

                                                    if ((Decimal)value > (Decimal)max)
                                                    {
                                                        max = (Decimal)value;
                                                        maxIndex = l;
                                                    }
                                                }
                                            }

                                            rollupObject.SetPropertyValue(rollupProp.propertyName, max);
                                        }
                                        else if (rollupProp.dataType == DataType.DateTime)
                                        {
                                            for (int l = 1; l < group.Count; l++)
                                            {
                                                DateTime value = (DateTime)dataObjects[group[l]].GetPropertyValue(rollupProp.propertyName);

                                                if (value != null && DateTime.Compare(value, (DateTime)max) > 0)
                                                {
                                                    max = value;
                                                    maxIndex = l;
                                                }
                                            }
                                        }
                                        else if (rollupProp.dataType == DataType.Boolean)
                                        {
                                            max = true;
                                        }
                                        else
                                        {
                                            for (int l = 1; l < group.Count; l++)
                                            {
                                                string value = (string)dataObjects[group[l]].GetPropertyValue(rollupProp.propertyName);

                                                if (value != null && string.Compare(value, (string)max) > 0)
                                                {
                                                    max = value;
                                                    maxIndex = l;
                                                }
                                            }
                                        }

                                        rollupObject = dataObjects[group[maxIndex]];
                                        break;
                                    }
                                case RollupType.Min:
                                    {
                                        object min = dataObjects[group[0]].GetPropertyValue(rollupProp.propertyName);
                                        int minIndex = 0;

                                        if (IsNumeric(rollupProp))
                                        {
                                            if (min == null)
                                            {
                                                min = Decimal.MaxValue;
                                            }
                                            else
                                            {
                                                if (min.ToString().Contains("/"))
                                                {
                                                    min = ToDecimal(min.ToString());
                                                }
                                                else
                                                {
                                                    min = Convert.ToDecimal(min);
                                                }
                                            }

                                            for (int l = 1; l < group.Count; l++)
                                            {
                                                object value = dataObjects[group[l]].GetPropertyValue(rollupProp.propertyName);

                                                if (value != null)
                                                {
                                                    if (value.ToString().Contains("/"))
                                                    {
                                                        value = ToDecimal(value.ToString());
                                                    }
                                                    else
                                                    {
                                                        value = Convert.ToDecimal(value);
                                                    }

                                                    if ((Decimal)value < (Decimal)min)
                                                    {
                                                        min = (Decimal)value;
                                                    }
                                                }
                                            }
                                        }
                                        else if (rollupProp.dataType == DataType.DateTime)
                                        {
                                            for (int l = 1; l < group.Count; l++)
                                            {
                                                DateTime value = (DateTime)dataObjects[group[l]].GetPropertyValue(rollupProp.propertyName);

                                                if (value != null && DateTime.Compare(value, (DateTime)min) < 0)
                                                {
                                                    min = value;
                                                    minIndex = l;
                                                }
                                            }
                                        }
                                        else if (rollupProp.dataType == DataType.Boolean)
                                        {
                                            min = false;
                                        }
                                        else
                                        {
                                            for (int l = 1; l < group.Count; l++)
                                            {
                                                string value = (string)dataObjects[group[l]].GetPropertyValue(rollupProp.propertyName);

                                                if (value != null && string.Compare(value, (string)min) < 0)
                                                {
                                                    min = value;
                                                    minIndex = l;
                                                }
                                            }
                                        }

                                        rollupObject = dataObjects[group[minIndex]];
                                        break;
                                    }
                                case RollupType.Sum:
                                    {
                                        if (IsNumeric(rollupProp))
                                        {
                                            decimal sum = 0;

                                            for (int l = 0; l < group.Count; l++)
                                            {
                                                object value = dataObjects[group[l]].GetPropertyValue(rollupProp.propertyName);

                                                if (value != null)
                                                {
                                                    if (value.ToString().Contains("/"))
                                                    {
                                                        value = ToDecimal(value.ToString());
                                                    }

                                                    sum += Convert.ToDecimal(value);
                                                }
                                            }

                                            rollupObject.SetPropertyValue(rollupProp.propertyName, sum);
                                        }
                                        else
                                        {
                                            rollupObject.SetPropertyValue(rollupProp.propertyName, null);
                                        }

                                        break;
                                    }
                                case RollupType.Average:
                                    {
                                        if (IsNumeric(rollupProp))
                                        {
                                            decimal sum = 0;

                                            for (int l = 0; l < group.Count; l++)
                                            {
                                                object value = dataObjects[group[l]].GetPropertyValue(rollupProp.propertyName);

                                                if (value != null)
                                                {
                                                    if (value.ToString().Contains("/"))
                                                    {
                                                        value = ToDecimal(value.ToString());
                                                    }

                                                    sum += Convert.ToDecimal(value);
                                                }
                                            }

                                            rollupObject.SetPropertyValue(rollupProp.propertyName, sum / group.Count);
                                        }
                                        else
                                        {
                                            rollupObject.SetPropertyValue(rollupProp.propertyName, null);
                                        }

                                        break;
                                    }
                            }

                            rollupObjects.Add(rollupObject);
                        }
                    }

                    dataObjects = rollupObjects;
                }
            }

            return dataObjects;
        }

        private bool IsNumeric(DataProperty dataProperty)
        {
            return (dataProperty.dataType == DataType.Byte ||
                dataProperty.dataType == DataType.Decimal ||
                dataProperty.dataType == DataType.Double ||
                dataProperty.dataType == DataType.Int16 ||
                dataProperty.dataType == DataType.Int32 ||
                dataProperty.dataType == DataType.Int64 ||
                dataProperty.dataType == DataType.Single);
        }

        public string AsyncGetDataTransferIndicesWithFilter(string scope, string app, string graph, string hashAlgorithm, DxiRequest dxiRequest)
        {
            try
            {
                var id = NewQueueRequest();
                Task task = Task.Factory.StartNew(() => DoGetDataTransferIndicesWithFilter(scope, app, graph, hashAlgorithm, dxiRequest, id));
                return "/requests/" + id;
            }
            catch (Exception e)
            {
                _logger.Error("Error getting data transfer indices: " + e.Message);
                throw e;
            }
        }

        private void DoGetDataTransferIndicesWithFilter(string scope, string app, string graph, string hashAlgorithm, DxiRequest dxiRequest, string id)
        {
            try
            {
                DataTransferIndices dataTransferIndices = GetDataTransferIndicesWithFilter(scope, app, graph, hashAlgorithm, dxiRequest);

                _requests[id].ResponseText = Utility.Serialize<DataTransferIndices>(dataTransferIndices, true);
                _requests[id].State = State.Completed;
            }
            catch (Exception ex)
            {
                if (ex is WebFaultException)
                {
                    _requests[id].Message = Convert.ToString(((WebFaultException)ex).Data["StatusText"]);
                }
                else
                {
                    _requests[id].Message = ex.Message;
                }

                _requests[id].State = State.Error;
            }
        }

        public string AsyncGetInternalIdentifiers(string scope, string app, string graph, DxiRequest dxiRequest)
        {
            try
            {
                var id = NewQueueRequest();
                Task task = Task.Factory.StartNew(() => DoGetInternalIdentifiers(scope, app, graph, dxiRequest, id));
                return "/requests/" + id;
            }
            catch (Exception e)
            {
                _logger.Error("Error getting data transfer indices: " + e.Message);
                throw e;
            }
        }

        private void DoGetInternalIdentifiers(string scope, string app, string graph, DxiRequest dxiRequest, string id)
        {
            try
            {
                Identifiers identifiers = GetInternalIdentifiers(scope, app, graph, dxiRequest);

                _requests[id].ResponseText = Utility.Serialize<Identifiers>(identifiers, true);
                _requests[id].State = State.Completed;
            }
            catch (Exception ex)
            {
                if (ex is WebFaultException)
                {
                    _requests[id].Message = Convert.ToString(((WebFaultException)ex).Data["StatusText"]);
                }
                else
                {
                    _requests[id].Message = ex.Message;
                }

                _requests[id].State = State.Error;
            }
        }

        public Identifiers GetInternalIdentifiers(string scope, string app, string graph, DxiRequest dxiRequest)
        {
            Identifiers identifiers = new Identifiers();

            try
            {
                InitializeScope(scope, app);
                InitializeDataLayer();

                DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                dtoProjectionEngine.dataLayerGateway = _dataLayerGateway;

                _graphMap = _mapping.graphMaps.Find(x => x.name.ToLower() == graph.ToLower());
                DataFilter presetFilter = GetPresetFilters(dtoProjectionEngine);

                BuildCrossGraphMap(dxiRequest.Manifest, graph);

                DataFilter filter = dxiRequest.DataFilter;
                DataObject dataObject = _dictionary.dataObjects.Find(o => o.objectName == _graphMap.dataObjectName);

                dtoProjectionEngine.ProjectDataFilter(dataObject, ref filter, _graphMap);
                filter.AppendFilter(presetFilter);

                List<string> identifierList = _dataLayerGateway.GetIdentifiers(dataObject, filter);
                if (identifierList != null)
                {
                    identifiers.AddRange(identifierList.ToList<string>());
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting internal identifiers: " + ex);
                throw ex;
            }

            return identifiers;
        }

        public DataTransferIndices GetDataTransferIndicesWithFilter(string scope, string app, string graph, string hashAlgorithm, DxiRequest dxiRequest)
        {
            DataTransferIndices dataTransferIndices = null;

            try
            {
                InitializeScope(scope, app);
                InitializeDataLayer();

                DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                dtoProjectionEngine.dataLayerGateway = _dataLayerGateway;

                _graphMap = _mapping.graphMaps.Find(x => x.name.ToLower() == graph.ToLower());
                DataFilter presetFilter = GetPresetFilters(dtoProjectionEngine);

                BuildCrossGraphMap(dxiRequest.Manifest, graph);

                DataFilter filter = dxiRequest.DataFilter;
                DataObject dataObject = _dictionary.dataObjects.Find(o => o.objectName == _graphMap.dataObjectName);

                dtoProjectionEngine.ProjectDataFilter(dataObject, ref filter, _graphMap);
                filter.AppendFilter(presetFilter);

                // get sort index
                string sortIndex = String.Empty;
                string sortOrder = String.Empty;

                if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
                {
                    sortIndex = filter.OrderExpressions.First().PropertyName;
                    sortOrder = filter.OrderExpressions.First().SortOrder.ToString();
                }

                if (_settings["MultiGetDTIs"] == null || bool.Parse(_settings["MultiGetDTIs"]))
                {
                    _logger.Debug("Running muti-threaded DTIs.");
                    dataTransferIndices = MultiGetDataTransferIndices(filter);
                }
                else
                {
                    _logger.Debug("Running single-threaded DTIs.");
                    List<IDataObject> tmpDataObjects = PageDataObjects(dataObject, filter);
                    List<IDataObject> dataObjects = ProcessRollups(dataObject, tmpDataObjects, filter);

                    dataTransferIndices = dtoProjectionEngine.GetDataTransferIndices(_graphMap, dataObjects, sortIndex);
                }

                if (sortOrder != String.Empty)
                    dataTransferIndices.SortOrder = sortOrder;
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting data transfer indices: " + ex);
                throw ex;
            }

            return dataTransferIndices;
        }

        public DataTransferIndices GetPagedDataTransferIndices(string scope, string app, string graph, DataFilter filter, int start, int limit)
        {
            DataTransferIndices dataTransferIndices = new DataTransferIndices();

            try
            {
                InitializeScope(scope, app);
                InitializeDataLayer();

                _graphMap = _mapping.FindGraphMap(graph);
                if (_graphMap == null)
                {
                    throw new Exception("Graph [" + graph + "] not found.");
                }

                DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                dtoProjectionEngine.dataLayerGateway = _dataLayerGateway;

                DataObject dataObject = _dictionary.dataObjects.Find(o => o.objectName == _graphMap.dataObjectName);
                AddNewDataProperties(_graphMap, dataObject);

                if (filter != null)
                {
                    dtoProjectionEngine.ProjectDataFilter(dataObject, ref filter, _graphMap);
                    filter.AppendFilter(GetPresetFilters(dtoProjectionEngine));
                }
                else
                {
                    filter = GetPresetFilters(dtoProjectionEngine);
                }

                List<IDataObject> dataObjects = _dataLayerGateway.Get(dataObject, filter, start, limit);

                if (dataObjects != null && dataObjects.Count > 0)
                {
                    dataTransferIndices = dtoProjectionEngine.GetDataTransferIndices(_graphMap, dataObjects, string.Empty);
                    dataTransferIndices.TotalCount = _dataLayerGateway.GetCount(dataObject, filter);
                }

                return dataTransferIndices;
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting data transfer indices: " + ex);
                throw ex;
            }
        }

        public DataTransferIndices GetPagedDataTransferIndicesByGraphID(string userName, string graphId, DataFilter filter, int start, int limit)
        {
            DataTransferIndices dataTransferIndices = new DataTransferIndices();

            try
            {

                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@GraphId", Value = Convert.ToString(graphId) });
                System.Data.DataTable dt = DBManager.Instance.ExecuteStoredProcedure(_connSecurityDb, "spgNames", nvl);

                string graph = string.Empty; string scope = string.Empty;
                string appName = string.Empty; Guid applicationId;

                System.Data.DataRow row = dt.Rows[0];

                graph = Convert.ToString(row["GraphName"]);
                scope = Convert.ToString(row["ScopeName"]);
                appName = Convert.ToString(row["AppName"]);
                applicationId = Guid.Parse(Convert.ToString(row["AppId"]));

                nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@ApplicationId", Value = applicationId });

                //string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgValuelistforManifest", nvl);
                //ValueListMaps valueListMaps = utility.Utility.Deserialize<ValueListMaps>(xmlString, true);

                nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                nvl.Add(new ListItem() { Name = "@GraphId", Value = Convert.ToString(graphId) });

                byte[] xmlbyte = DBManager.Instance.ExecuteBytesQuery(_connSecurityDb, "spgGraphBinary", nvl);
                string bytesToXml = System.Text.Encoding.Default.GetString(xmlbyte);
                //mapping.Mapping _mapping = utility.Utility.Deserialize<mapping.Mapping>(bytesToXml, true); ;
                bytesToXml = bytesToXml.Replace("<graphMap>", "<graphMap xmlns=\"http://www.iringtools.org/mapping\">");
                 _graphMap = utility.Utility.Deserialize<GraphMap>(bytesToXml, true); ;

                 

                DataDictionary dataDictionary = dictionaryProvider.GetDictionary(applicationId);

                org.iringtools.applicationConfig.Application objApplication = GetApplicationByApplicationID(userName, applicationId);
                _settings["ProjectName"] = scope;
                _settings["ApplicationName"] = appName;

                InitializeDataLayer(objApplication, ref dataDictionary);

                if (_graphMap == null)
                {
                    throw new Exception("Graph [" + _graphMap + "] not found.");
                }


                DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                dtoProjectionEngine.dataLayerGateway = _dataLayerGateway;

                DataObject dataObject = _dictionary.dataObjects.Find(o => o.objectName == _graphMap.dataObjectName);

                if (filter != null)
                {
                    dtoProjectionEngine.ProjectDataFilter(dataObject, ref filter, _graphMap);
                    filter.AppendFilter(GetPresetFilters(dtoProjectionEngine));
                }
                else
                {
                    filter = GetPresetFilters(dtoProjectionEngine);
                }

                List<IDataObject> dataObjects = _dataLayerGateway.Get(dataObject,objApplication.ApplicationDataMode, filter, start, limit);

                if (dataObjects != null && dataObjects.Count > 0)
                {
                    dataTransferIndices = dtoProjectionEngine.GetDataTransferIndices(_graphMap, dataObjects, string.Empty);
                    dataTransferIndices.TotalCount = _dataLayerGateway.GetCount(dataObject, objApplication.ApplicationDataMode, filter);
                }

                return dataTransferIndices;
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting data transfer indices: " + ex);
                throw ex;
            }
        }

        public org.iringtools.applicationConfig.Application GetApplicationByApplicationID(string userName, Guid applicationID)
        {
            org.iringtools.applicationConfig.Application application = new org.iringtools.applicationConfig.Application();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@ApplicationId", Value = Convert.ToString(applicationID) });
                nvl.Add(new ListItem() { Name = "@UserName", Value = userName });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgApplicationByApplicationID", nvl);
                application = utility.Utility.Deserialize<org.iringtools.applicationConfig.Application>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Application By ApplicationID: " + ex);
            }
            return application;
        }

        // get single data transfer object (but wrap it in a list!)
        public DataTransferObjects GetDataTransferObject(string scope, string app, string graph, string id)
        {
            DataTransferObjects dataTransferObjects = new DataTransferObjects();

            try
            {
                InitializeScope(scope, app);
                InitializeDataLayer();

                _graphMap = _mapping.FindGraphMap(graph);
                DataObject dataObject = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == _graphMap.dataObjectName.ToLower());
                List<string> identifiers = new List<string> { id };
                List<IDataObject> dataObjects = _dataLayerGateway.Get(dataObject, identifiers);

                DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                dtoProjectionEngine.dataLayerGateway = _dataLayerGateway;

                XDocument dtoDoc = dtoProjectionEngine.ToXml(_graphMap.name, ref dataObjects);

                if (dtoDoc != null && dtoDoc.Root != null)
                {
                    dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(dtoDoc.Root);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting data transfer objects: " + ex);
                throw ex;
            }

            return dataTransferObjects;
        }

        public DataTransferObjects GetDataTransferObjects(string scope, string app, string graph, DataTransferIndices dataTransferIndices)
        {
            DataTransferObjects dataTransferObjects = new DataTransferObjects();

            if (dataTransferIndices != null && dataTransferIndices.DataTransferIndexList.Count > 0)
            {
                try
                {
                    InitializeScope(scope, app);
                    InitializeDataLayer();

                    _graphMap = _mapping.FindGraphMap(graph);
                    DataObject dataObject = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == _graphMap.dataObjectName.ToLower());
                    AddNewDataProperties(_graphMap, dataObject);
                    List<DataTransferIndex> dataTrasferIndexList = dataTransferIndices.DataTransferIndexList;
                    List<string> identifiers = new List<string>();

                    foreach (DataTransferIndex dti in dataTrasferIndexList)
                    {
                        identifiers.Add(dti.InternalIdentifier);
                    }

                    if (identifiers.Count > 0)
                    {
                        if (_settings["MultiGetDTOs"] == null || bool.Parse(_settings["MultiGetDTOs"]))
                        {
                            dataTransferObjects = MultiGetDataTransferObjects(dataObject, identifiers);
                        }
                        else
                        {
                            List<IDataObject> dataObjects = _dataLayerGateway.Get(dataObject, identifiers);
                            DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                            dtoProjectionEngine.dataLayerGateway = _dataLayerGateway;
                            dataTransferObjects = dtoProjectionEngine.BuildDataTransferObjects(_graphMap, ref dataObjects);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error getting data transfer objects: " + ex);
                    throw ex;
                }
            }

            return dataTransferObjects;
        }

        public DataTransferObjects GetDataTransferObjectsByGraphId(string userName, string graphId, DataTransferIndices dataTransferIndices)
        {
            DataTransferObjects dataTransferObjects = new DataTransferObjects();

            if (dataTransferIndices != null && dataTransferIndices.DataTransferIndexList.Count > 0)
            {
                try
                {
                    //InitializeScope(scope, app);
                    //InitializeDataLayer();

                    //_graphMap = _mapping.FindGraphMap(graph);

                    NameValueList nvl = new NameValueList();
                    nvl.Add(new ListItem() { Name = "@GraphId", Value = Convert.ToString(graphId) });
                    System.Data.DataTable dt = DBManager.Instance.ExecuteStoredProcedure(_connSecurityDb, "spgNames", nvl);

                    string graph = string.Empty; string scope = string.Empty;
                    string appName = string.Empty; Guid applicationId;

                    System.Data.DataRow row = dt.Rows[0];

                    graph = Convert.ToString(row["GraphName"]);
                    scope = Convert.ToString(row["ScopeName"]);
                    appName = Convert.ToString(row["AppName"]);
                    applicationId = Guid.Parse(Convert.ToString(row["AppId"]));

                    //nvl = new NameValueList();
                    //nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                    //nvl.Add(new ListItem() { Name = "@ApplicationId", Value = applicationId });

                    nvl = new NameValueList();
                    nvl.Add(new ListItem() { Name = "@UserName", Value = userName });
                    nvl.Add(new ListItem() { Name = "@GraphId", Value = Convert.ToString(graphId) });

                    byte[] xmlbyte = DBManager.Instance.ExecuteBytesQuery(_connSecurityDb, "spgGraphBinary", nvl);
                    string bytesToXml = System.Text.Encoding.Default.GetString(xmlbyte);
                    //mapping.Mapping _mapping = utility.Utility.Deserialize<mapping.Mapping>(bytesToXml, true); ;
                    bytesToXml = bytesToXml.Replace("<graphMap>", "<graphMap xmlns=\"http://www.iringtools.org/mapping\">");
                    _graphMap = utility.Utility.Deserialize<GraphMap>(bytesToXml, true); ;

                    

                    DataDictionary dataDictionary = dictionaryProvider.GetDictionary(applicationId);

                    _mapping = new Mapping();
                    _mapping.graphMaps.Add(_graphMap);

                    _kernel.Bind<mapping.Mapping>().ToConstant(_mapping).InThreadScope(); 

                    org.iringtools.applicationConfig.Application objApplication = GetApplicationByApplicationID(userName, applicationId);
                    _settings["ProjectName"] = scope;
                    _settings["ApplicationName"] = appName;

                    InitializeDataLayer(objApplication, ref dataDictionary);

                    

                    if (_graphMap == null)
                    {
                        throw new Exception("Graph [" + _graphMap + "] not found.");
                    }

                    DataObject dataObject = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == _graphMap.dataObjectName.ToLower());
                    AddNewDataProperties(_graphMap, dataObject);
                    List<DataTransferIndex> dataTrasferIndexList = dataTransferIndices.DataTransferIndexList;
                    List<string> identifiers = new List<string>();

                    foreach (DataTransferIndex dti in dataTrasferIndexList)
                    {
                        identifiers.Add(dti.InternalIdentifier);
                    }

                    if (identifiers.Count > 0)
                    {
                        if (_settings["MultiGetDTOs"] == null || bool.Parse(_settings["MultiGetDTOs"]))
                        {
                            dataTransferObjects = MultiGetDataTransferObjects(dataObject, identifiers);
                        }
                        else
                        {
                            List<IDataObject> dataObjects = _dataLayerGateway.Get(dataObject, identifiers);
                            DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                            dtoProjectionEngine.dataLayerGateway = _dataLayerGateway;
                            dataTransferObjects = dtoProjectionEngine.BuildDataTransferObjects(_graphMap, ref dataObjects);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error getting data transfer objects: " + ex);
                    throw ex;
                }
            }

            return dataTransferObjects;
        }

        public string AsyncGetDataTransferObjects(string scope, string app, string graph, DxoRequest dxoRequest, bool includeContent)
        {
            try
            {
                var id = NewQueueRequest();
                Task task = Task.Factory.StartNew(() => DoGetDataTransferObjects(scope, app, graph, dxoRequest, id, includeContent));
                return "/requests/" + id;
            }
            catch (Exception e)
            {
                _logger.Error("Error getting data transfer objects: " + e.Message);
                throw e;
            }
        }

        private void DoGetDataTransferObjects(string scope, string app, string graph, DxoRequest dxoRequest, string id, bool includeContent)
        {
            try
            {
                DataTransferObjects dtos = GetDataTransferObjects(scope, app, graph, dxoRequest, includeContent);

                _requests[id].ResponseText = Utility.Serialize<DataTransferObjects>(dtos, true);
                _requests[id].State = State.Completed;
            }
            catch (Exception ex)
            {
                if (ex is WebFaultException)
                {
                    _requests[id].Message = Convert.ToString(((WebFaultException)ex).Data["StatusText"]);
                }
                else
                {
                    _requests[id].Message = ex.Message;
                }

                _requests[id].State = State.Error;
            }
        }

        // get list (page) of data transfer objects per dto page request
        public DataTransferObjects GetDataTransferObjects(string scope, string app, string graph, DxoRequest dxoRequest, bool includeContent)
        {
            DataTransferObjects dtos = new DataTransferObjects();

            if (dxoRequest != null && dxoRequest.DataTransferIndices != null &&
                dxoRequest.DataTransferIndices.DataTransferIndexList.Count > 0)
            {
                try
                {
                    InitializeScope(scope, app);
                    InitializeDataLayer();

                    BuildCrossGraphMap(dxoRequest.Manifest, graph);
                    DataObject objectType = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == _graphMap.dataObjectName.ToLower());

                    List<DataTransferIndex> dataTrasferIndexList = dxoRequest.DataTransferIndices.DataTransferIndexList;

                    Dictionary<string, string> idFormats = new Dictionary<string, string>();
                    foreach (DataTransferIndex dti in dataTrasferIndexList)
                    {
                        idFormats[dti.InternalIdentifier] = string.Empty;
                    }

                    List<IDataObject> dataObjects = null;

                    if (includeContent)
                    {
                        List<IContentObject> contentObjects = _dataLayerGateway.GetContents(objectType, idFormats);
                        dataObjects = new List<IDataObject>();

                        foreach (IContentObject contentObject in contentObjects)
                        {
                            dataObjects.Add((IDataObject)contentObject);
                        }
                    }
                    else
                    {
                        dataObjects = _dataLayerGateway.Get(objectType, idFormats.Keys.ToList());
                    }

                    DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                    dtoProjectionEngine.dataLayerGateway = _dataLayerGateway;
                    dtos = dtoProjectionEngine.BuildDataTransferObjects(_graphMap, ref dataObjects);
                }
                catch (Exception ex)
                {
                    _logger.Error("Error getting data transfer objects: " + ex);
                    throw ex;
                }
            }

            return dtos;
        }

        public DataTransferObjects GetPageDataTransferObjects(string scope, string app, string graph, DxiRequest dxiRequest, int start, int limit)
        {
            try
            {
                InitializeScope(scope, app);
                InitializeDataLayer();

                DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                dtoProjectionEngine.dataLayerGateway = _dataLayerGateway;

                _graphMap = _mapping.graphMaps.Find(x => x.name.ToLower() == graph.ToLower());
                DataFilter presetFilter = GetPresetFilters(dtoProjectionEngine);

                BuildCrossGraphMap(dxiRequest.Manifest, graph);

                DataFilter filter = dxiRequest.DataFilter;
                DataObject dataObject = _dictionary.dataObjects.Find(o => o.objectName == _graphMap.dataObjectName);

                dtoProjectionEngine.ProjectDataFilter(dataObject, ref filter, _graphMap);
                filter.AppendFilter(presetFilter);

                long totalCount = 0;
                List<IDataObject> tmpDataObjects = GetDataObjects(dataObject, filter, start, limit, out totalCount);
                List<IDataObject> dataObjects = ProcessRollups(dataObject, tmpDataObjects, filter);

                DataTransferObjects dtos = dtoProjectionEngine.BuildDataTransferObjects(_graphMap, ref dataObjects);

                if (dtos != null)
                {
                    dtos.TotalCount = totalCount;
                }

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting data transfer objects: " + ex);
                throw ex;
            }
        }

        public string AsyncPostDataTransferObjects(string scope, string app, string graph, DataTransferObjects dtos)
        {
            try
            {
                var id = NewQueueRequest();
                Task task = Task.Factory.StartNew(() => DoPostDataTransferObjects(scope, app, graph, dtos, id));
                return "/requests/" + id;
            }
            catch (Exception e)
            {
                _logger.Error("Error posting data transfer objects: " + e.Message);
                throw e;
            }
        }

        private void DoPostDataTransferObjects(string scope, string app, string graph, DataTransferObjects dtos, string id)
        {
            try
            {
                Response response = PostDataTransferObjects(scope, app, graph, dtos);

                _requests[id].ResponseText = Utility.Serialize<Response>(response, true);
                _requests[id].State = State.Completed;
            }
            catch (Exception ex)
            {
                if (ex is WebFaultException)
                {
                    _requests[id].Message = Convert.ToString(((WebFaultException)ex).Data["StatusText"]);
                }
                else
                {
                    _requests[id].Message = ex.Message;
                }

                _requests[id].State = State.Error;
            }
        }


        public Response PostDataTransferObjects(string scope, string app, string graph, DataTransferObjects dataTransferObjects)
        {
            Response response = new Response();
            response.DateTimeStamp = DateTime.Now;

            try
            {
                _settings["SenderProjectName"] = dataTransferObjects.SenderScopeName;
                _settings["SenderApplicationName"] = dataTransferObjects.SenderAppName;

                InitializeScope(scope, app);

                if (_settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
                {
                    string message = "Can not perform post on read-only data layer of [" + scope + "." + app + "].";
                    _logger.Error(message);

                    response = new Response();
                    response.DateTimeStamp = DateTime.Now;
                    response.Level = StatusLevel.Error;
                    response.Messages = new Messages() { message };

                    return response;
                }

                response.Level = StatusLevel.Success;

                InitializeDataLayer();

                _graphMap = _mapping.FindGraphMap(graph);

                // collecting mapped properties
                List<string> mappedProps = new List<string>();
                foreach (ClassTemplateMap ctm in _graphMap.classTemplateMaps)
                {
                    foreach (TemplateMap tm in ctm.templateMaps)
                    {
                        foreach (RoleMap rm in tm.roleMaps)
                        {
                            if (rm.type == RoleType.DataProperty ||
                                rm.type == RoleType.ObjectProperty ||
                                rm.type == RoleType.Property)
                            {
                                int index = rm.propertyName.LastIndexOf('.');
                                mappedProps.Add(rm.propertyName.Substring(index + 1));
                            }
                        }
                    }
                }

                DataObject objectType = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == _graphMap.dataObjectName.ToLower());
                List<DataTransferObject> dataTransferObjectList = dataTransferObjects.DataTransferObjectList;

                // extract deleted identifiers from data transfer objects
                List<IDataObject> deletedDataObjects = new List<IDataObject>();
                List<IDataObject> setNullsDataObjects = new List<IDataObject>();

                for (int i = 0; i < dataTransferObjectList.Count; i++)
                {
                    if (dataTransferObjectList[i].transferType == TransferType.Delete)
                    {
                        deletedDataObjects.Add(new SerializableDataObject()
                        {
                            Id = dataTransferObjectList[i].identifier,
                            Type = objectType.objectName,
                            State = ObjectState.Delete
                        });

                        dataTransferObjectList.RemoveAt(i--);
                    }
                    else if (dataTransferObjectList[i].transferType == TransferType.SetNulls)
                    {
                        string identifier = dataTransferObjectList[i].identifier;

                        SerializableDataObject sdo = new SerializableDataObject()
                        {
                            Id = identifier,
                            Type = objectType.objectName,
                            State = ObjectState.Update
                        };

                        // set nulls for all mapped properties in manifest
                        foreach (string prop in mappedProps)
                        {
                            sdo.Dictionary[prop] = null;
                        }

                        // set key properties
                        if (objectType.keyProperties.Count == 1)
                        {
                            sdo.Dictionary[objectType.keyProperties[0].keyPropertyName] = identifier;
                        }
                        else if (objectType.keyProperties.Count > 1)
                        {
                            string[] parts = identifier.Split(
                                new string[] { objectType.keyDelimeter }, StringSplitOptions.None);

                            for (int j = 0; j < parts.Length; j++)
                            {
                                sdo.Dictionary[objectType.keyProperties[j].keyPropertyName] = parts[j];
                            }
                        }

                        setNullsDataObjects.Add(sdo);
                        dataTransferObjectList.RemoveAt(i--);
                    }
                }

                if (deletedDataObjects.Count > 0)
                {
                    response.Append(_dataLayerGateway.Update(objectType, deletedDataObjects));
                }

                if (setNullsDataObjects.Count > 0)
                {
                    response.Append(_dataLayerGateway.Update(objectType, setNullsDataObjects));
                }

                if (dataTransferObjectList.Count > 0)
                {
                    if (_settings["MultiPostDTOs"] != null && bool.Parse(_settings["MultiPostDTOs"]))
                    {
                        response.Append(MultiPostDataTransferObjects(_dataLayerGateway, objectType, dataTransferObjects));
                    }
                    else
                    {
                        _logger.Debug("Single threaded post DTOs.");
                        DtoProjectionEngine dtoProjectionEngine = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                        dtoProjectionEngine.dataLayerGateway = _dataLayerGateway;

                        List<IDataObject> dataObjects = dtoProjectionEngine.ToDataObjects(_graphMap, ref dataTransferObjects);

                        Response postResponse = _dataLayerGateway.Update(objectType, dataObjects);
                        response.Append(postResponse);
                    }
                }

                if (response.Level != StatusLevel.Success)
                {
                    string dtoFilename = String.Format(_settings["BaseDirectoryPath"] + "/Logs/DTO_{0}.{1}.{2}.xml", scope, app, graph);
                    Utility.Write<DataTransferObjects>(dataTransferObjects, dtoFilename, true);
                }
            }
            catch (Exception ex)
            {
                string message = "Error posting data transfer objects: " + ex;

                Status status = new Status
                {
                    Level = StatusLevel.Error,
                    Messages = new Messages { message },
                };

                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);

                _logger.Error(message);
            }

            return response;
        }

        public ContentObjects GetContents(string scope, string app, string graph, string filter)
        {
            try
            {
                ContentObjects contentObjects = new ContentObjects();

                Dictionary<string, string> idFormats = (Dictionary<string, string>)
                  JsonConvert.DeserializeObject<Dictionary<string, string>>(filter);

                InitializeScope(scope, app);
                InitializeDataLayer();

                GraphMap graphMap = _mapping.FindGraphMap(graph);
                if (graph == null)
                {
                    throw new Exception("Graph [" + graph + "] not found.");
                }

                DataObject dataObject = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == graphMap.dataObjectName.ToLower());
                if (dataObject == null)
                {
                    throw new Exception("Data object [" + graphMap.dataObjectName + "] not found.");
                }

                List<IContentObject> iContentObjects = _dataLayerGateway.GetContents(dataObject, idFormats);

                #region marshall iContentObjects to contentObjects
                foreach (IContentObject iContentObject in iContentObjects)
                {
                    ContentObject contentObject = new ContentObject()
                    {
                        Identifier = iContentObject.Identifier,
                        MimeType = iContentObject.ContentType,
                        Content = iContentObject.Content.ToMemoryStream().ToArray(),
                        HashType = iContentObject.HashType,
                        HashValue = iContentObject.HashValue,
                        URL = iContentObject.URL
                    };

                    foreach (DataProperty prop in dataObject.dataProperties)
                    {
                        object value = iContentObject.GetPropertyValue(prop.propertyName);
                        if (value != null)
                        {
                            string valueStr = Convert.ToString(value);

                            if (prop.dataType == DataType.DateTime)
                                valueStr = Utility.ToXsdDateTime(valueStr);

                            Attribute attr = new Attribute()
                            {
                                Name = prop.propertyName,
                                Value = valueStr
                            };

                            contentObject.Attributes.Add(attr);
                        }
                    }

                    contentObjects.Add(contentObject);
                }
                #endregion

                return contentObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting content objects: " + ex.ToString());
                throw ex;
            }
        }

        public IContentObject GetContent(string scope, string app, string graph, string id, string format)
        {
            try
            {
                InitializeScope(scope, app);
                InitializeDataLayer();

                GraphMap graphMap = _mapping.FindGraphMap(graph);
                if (graph == null)
                {
                    throw new Exception("Graph [" + graph + "] not found.");
                }

                DataObject dataObject = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == _graphMap.dataObjectName.ToLower());

                Dictionary<string, string> idFormats = new Dictionary<string, string>() { { id, format } };

                List<IContentObject> iContentObjects = _dataLayerGateway.GetContents(dataObject, idFormats);

                if (iContentObjects == null || iContentObjects.Count == 0)
                    throw new Exception("Content object [" + id + "] not found.");

                return iContentObjects[0];
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting content object: " + ex.ToString());
                throw ex;
            }
        }

        public Response RefreshCache(string scope, string app, string graph)
        {
            Response response = new Response();

            try
            {
                InitializeScope(scope, app, true);

                if (_mapping == null)
                {
                    response.Level = StatusLevel.Error;
                    response.Messages.Add("Mapping for [" + scope + "." + app + ".] not found.");
                    return response;
                }

                string objectType = null;

                foreach (GraphMap g in _mapping.graphMaps)
                {
                    if (g.name.ToLower() == graph.ToLower())
                    {
                        objectType = g.dataObjectName;
                        break;
                    }
                }

                if (objectType == null)
                {
                    response.Level = StatusLevel.Error;
                    response.Messages.Add("Graph [" + scope + "." + app + "." + graph + ".] not found.");
                    return response;
                }

                response = RefreshCache(scope, app, objectType, false);
            }
            catch (Exception e)
            {
                _logger.Error("Error refreshing cache: ", e);
                response.Level = StatusLevel.Error;
                response.Messages.Add(e.Message);
            }

            return response;
        }

        public Response ImportCache(string scope, string app, string graph, string baseUri)
        {
            Response response = new Response();

            try
            {
                InitializeScope(scope, app, true);

                if (_mapping == null)
                {
                    response.Level = StatusLevel.Error;
                    response.Messages.Add("Mapping for [" + scope + "." + app + ".] not found.");
                    return response;
                }

                string objectType = null;

                foreach (GraphMap g in _mapping.graphMaps)
                {
                    if (g.name.ToLower() == graph.ToLower())
                    {
                        objectType = g.dataObjectName;
                        break;
                    }
                }

                if (objectType == null)
                {
                    response.Level = StatusLevel.Error;
                    response.Messages.Add("Graph [" + scope + "." + app + "." + graph + ".] not found.");
                    return response;
                }

                response = ImportCache(scope, app, objectType, baseUri, false);
            }
            catch (Exception e)
            {
                _logger.Error("Error refreshing cache: ", e);
                response.Level = StatusLevel.Error;
                response.Messages.Add(e.Message);
            }

            return response;
        }

        // build cross _graphmap from manifest graph and mapping graph
        private void BuildCrossGraphMap(Manifest manifest, string graph)
        {
            if (manifest == null || manifest.graphs == null || manifest.graphs.Count == 0)
                throw new Exception("Manifest of graph [" + graph + "] is empty.");

            Graph manifestGraph = manifest.FindGraph(graph);

            if (manifestGraph.classTemplatesList == null || manifestGraph.classTemplatesList.Count == 0)
                throw new Exception("Manifest of graph [" + graph + "] does not contain any class-template-maps.");

            GraphMap mappingGraph = _mapping.FindGraphMap(graph);
            ClassTemplates manifestClassTemplatesMap = manifestGraph.classTemplatesList.First();
            Class manifestClass = manifestClassTemplatesMap.@class;

            _graphMap = new GraphMap()
            {
                name = mappingGraph.name,
                dataObjectName = mappingGraph.dataObjectName,
                dataFilter = mappingGraph.dataFilter
            };

            if (manifestClassTemplatesMap != null)
            {
                foreach (var mappingClassTemplatesMap in mappingGraph.classTemplateMaps)
                {
                    ClassMap mappingClass = mappingClassTemplatesMap.classMap;

                    //if (mappingClass.id == manifestClass.id && (String.IsNullOrWhiteSpace(mappingClass.path) 
                    //  ? String.IsNullOrWhiteSpace(manifestClass.path) 
                    //  : mappingClass.path == manifestClass.path))

                    if (mappingClass.id == manifestClass.id &&
                       (String.IsNullOrWhiteSpace(mappingClass.path) ||
                        (!String.IsNullOrWhiteSpace(manifestClass.path) &&
                          mappingClass.path == manifestClass.path)))
                    {
                        RecurBuildCrossGraphMap(ref manifestGraph, manifestClass, mappingGraph, mappingClass);
                        break;
                    }
                }
            }
        }

        private DataFilter GetPresetFilters(DtoProjectionEngine dtoProjection)
        {
            DataFilter dataFilter = new DataFilter();

            if (_dictionary == null)
            {
                _dictionary = _dataLayerGateway.GetDictionary();
            }

            DataObject dataObject = _dictionary.GetDataObject(_graphMap.dataObjectName);
            DataFilter graphFilter = _graphMap.dataFilter;

            dtoProjection.ProjectDataFilter(dataObject, ref graphFilter, _graphMap);
            dataFilter.AppendFilter(dataObject.dataFilter);
            dataFilter.AppendFilter(graphFilter);

            return dataFilter;
        }

        private void RecurBuildCrossGraphMap(ref Graph manifestGraph, Class manifestClass,
            GraphMap mappingGraph, ClassMap mappingClass)
        {
            List<Template> manifestTemplates = null;

            // get manifest templates from the manifest class
            foreach (ClassTemplates manifestClassTemplates in manifestGraph.classTemplatesList)
            {
                //if (manifestClassTemplates.@class.id == manifestClass.id 
                //    && (String.IsNullOrWhiteSpace(manifestClassTemplates.@class.path) 
                //  ? String.IsNullOrWhiteSpace(manifestClass.path) 
                //  : manifestClassTemplates.@class.path == manifestClass.path))

                if (manifestClassTemplates.@class.id == manifestClass.id &&
                    (String.IsNullOrWhiteSpace(manifestClassTemplates.@class.path) ||
                      (!String.IsNullOrWhiteSpace(manifestClass.path) &&
                       manifestClassTemplates.@class.path == manifestClass.path)))
                {
                    manifestTemplates = manifestClassTemplates.templates;
                    break;
                }
            }

            if (manifestTemplates != null)
            {
                // find mapping templates for the mapping class
                foreach (var pair in mappingGraph.classTemplateMaps)
                {
                    ClassMap localMappingClass = pair.classMap;
                    List<TemplateMap> mappingTemplates = pair.templateMaps;

                    //if (localMappingClass.id == manifestClass.id && 
                    //  (String.IsNullOrWhiteSpace(localMappingClass.path) 
                    //    ? String.IsNullOrWhiteSpace(manifestClass.path) 
                    //    : localMappingClass.path == manifestClass.path))

                    if (localMappingClass.id == manifestClass.id &&
                      (String.IsNullOrWhiteSpace(localMappingClass.path) ||
                        (!String.IsNullOrWhiteSpace(manifestClass.path) &&
                          localMappingClass.path == manifestClass.path)))
                    {
                        ClassMap crossedClass = localMappingClass.CrossClassMap(mappingGraph, manifestClass);
                        TemplateMaps crossedTemplates = new TemplateMaps();

                        _graphMap.classTemplateMaps.Add(new ClassTemplateMap { classMap = crossedClass, templateMaps = crossedTemplates });

                        foreach (Template manifestTemplate in manifestTemplates)
                        {
                            TemplateMap crossedTemplate = null;

                            foreach (TemplateMap mappingTemplate in mappingTemplates)
                            {
                                if (mappingTemplate.id == manifestTemplate.id)
                                {
                                    List<string> unmatchedRoleIds = new List<string>();
                                    int rolesMatchedCount = 0;

                                    for (int i = 0; i < mappingTemplate.roleMaps.Count; i++)
                                    {
                                        RoleMap roleMap = mappingTemplate.roleMaps[i];
                                        bool found = false;

                                        foreach (Role manifestRole in manifestTemplate.roles)
                                        {
                                            if (manifestRole.id == roleMap.id)
                                            {
                                                found = true;

                                                if (roleMap.type != RoleType.Reference || roleMap.value == manifestRole.value)
                                                {
                                                    rolesMatchedCount++;
                                                }

                                                if (manifestRole.type == RoleType.Property ||
                                                    manifestRole.type == RoleType.DataProperty ||
                                                    manifestRole.type == RoleType.ObjectProperty ||
                                                    manifestRole.type == RoleType.FixedValue)
                                                {
                                                    roleMap.dataLength = manifestRole.dataLength;
                                                    roleMap.dataType = manifestRole.dataType;
                                                    roleMap.precision = manifestRole.precision;
                                                    roleMap.scale = manifestRole.scale;
                                                    roleMap.dbDataType = manifestRole.dbDataType;
                                                }

                                                break;
                                            }
                                        }

                                        if (!found)
                                        {
                                            unmatchedRoleIds.Add(roleMap.id);
                                        }
                                    }

                                    if (rolesMatchedCount == manifestTemplate.roles.Count)
                                    {
                                        crossedTemplate = CloneTemplateMap(mappingTemplate);

                                        if (unmatchedRoleIds.Count > 0)
                                        {
                                            // remove unmatched roles                      
                                            for (int i = 0; i < crossedTemplate.roleMaps.Count; i++)
                                            {
                                                if (unmatchedRoleIds.Contains(crossedTemplate.roleMaps[i].id))
                                                {
                                                    crossedTemplate.roleMaps.RemoveAt(i--);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (crossedTemplate != null)
                            {
                                crossedTemplates.Add(crossedTemplate);

                                // set cardinality for crossed role map that references to class map
                                foreach (Role manifestRole in manifestTemplate.roles)
                                {
                                    if (manifestRole.@class != null)
                                    {
                                        foreach (RoleMap mappingRole in crossedTemplate.roleMaps)
                                        {
                                            //if (mappingRole.classMap != null && mappingRole.classMap.id == manifestRole.@class.id 
                                            //    && (String.IsNullOrWhiteSpace(mappingRole.classMap.path) 
                                            //  ? String.IsNullOrWhiteSpace(manifestRole.@class.path) 
                                            //  : mappingRole.classMap.path == manifestRole.@class.path))

                                            if (mappingRole.classMap != null && mappingRole.classMap.id == manifestRole.@class.id &&
                                                (String.IsNullOrWhiteSpace(mappingRole.classMap.path) ||
                                                   (!String.IsNullOrWhiteSpace(manifestRole.@class.path) &&
                                                       mappingRole.classMap.path == manifestRole.@class.path)))
                                            {
                                                Cardinality cardinality = mappingGraph.GetCardinality(mappingRole, _dictionary, _fixedIdentifierBoundary);

                                                // get crossed role map and set its cardinality
                                                foreach (RoleMap crossedRoleMap in crossedTemplate.roleMaps)
                                                {
                                                    if (crossedRoleMap.id == mappingRole.id)
                                                    {
                                                        manifestRole.cardinality = cardinality;
                                                        break;
                                                    }
                                                }

                                                Class childManifestClass = manifestRole.@class;
                                                foreach (ClassTemplates anyClassTemplates in manifestGraph.classTemplatesList)
                                                {
                                                    //if (manifestRole.@class.id == anyClassTemplates.@class.id && 
                                                    //      (String.IsNullOrWhiteSpace(manifestRole.@class.path) 
                                                    //  ? String.IsNullOrWhiteSpace(anyClassTemplates.@class.path) 
                                                    //  : manifestRole.@class.path == anyClassTemplates.@class.path))

                                                    if (manifestRole.@class.id == anyClassTemplates.@class.id &&
                                                      (String.IsNullOrWhiteSpace(manifestRole.@class.path) ||
                                                        (!String.IsNullOrWhiteSpace(anyClassTemplates.@class.path) &&
                                                          manifestRole.@class.path == anyClassTemplates.@class.path)))
                                                    {
                                                        childManifestClass = anyClassTemplates.@class;
                                                    }
                                                }

                                                RecurBuildCrossGraphMap(ref manifestGraph, childManifestClass, mappingGraph, mappingRole.classMap);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }

        private TemplateMap CloneTemplateMap(TemplateMap templateMap)
        {
            TemplateMap clonedTemplateMap = new TemplateMap()
            {
                id = templateMap.id,
                name = templateMap.name,
                type = templateMap.type
            };

            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
                clonedTemplateMap.roleMaps.Add(roleMap);
            }

            return clonedTemplateMap;
        }

        private List<IDataObject> PageDataObjects(DataObject objectType, DataFilter filter)
        {
            List<IDataObject> dataObjects = new List<IDataObject>();

            int pageSize = (String.IsNullOrEmpty(_settings["DefaultPageSize"]))
              ? 250 : int.Parse(_settings["DefaultPageSize"]);

            long count = _dataLayerGateway.GetCount(objectType, filter);

            for (int offset = 0; offset < count; offset = offset + pageSize)
            {
                _logger.Debug(string.Format("Getting paged data {0}-{1}.", offset, offset + pageSize));

                dataObjects.AddRange(_dataLayerGateway.Get(objectType, filter, offset, pageSize));

                _logger.Debug(string.Format("Paged data {0}-{1} completed.", offset, offset + pageSize));
            }

            return dataObjects;
        }

        private List<IDataObject> GetDataObjects(DataObject objectType, DataFilter filter,
          int start, int limit, out long totalCount)
        {
            List<IDataObject> dataObjects = _dataLayerGateway.Get(objectType, filter, start, limit);

            totalCount = _dataLayerGateway.GetCount(objectType, filter);

            return dataObjects;
        }

        private DataTransferIndices MultiGetDataTransferIndices(DataFilter filter)
        {
            DataTransferIndices dataTransferIndices = new DataTransferIndices();
            DataObject dataObject = _dictionary.dataObjects.Find(x => x.objectName.ToLower() == _graphMap.dataObjectName.ToLower());

            _logger.Debug("Data object: " + dataObject.objectName);

            long total = _dataLayerGateway.GetCount(dataObject, filter);
            int maxThreads = int.Parse(_settings["MaxThreads"]);

            _logger.Debug("Total count [" + total + "].");

            if (total > 0)
            {
                long numOfThreads = Math.Min(total, maxThreads);
                int itemsPerThread = (int)Math.Ceiling((double)total / numOfThreads);

                _logger.Debug("Items per thread [" + itemsPerThread + "].");

                List<ManualResetEvent> doneEvents = new List<ManualResetEvent>();
                List<DtiTask> dtiTasks = new List<DtiTask>();
                int threadCount;

                for (threadCount = 0; threadCount < numOfThreads; threadCount++)
                {
                    int offset = threadCount * itemsPerThread;
                    if (offset >= total)
                        break;

                    int pageSize = (offset + itemsPerThread > total) ? (int)(total - offset) : itemsPerThread;
                    DtoProjectionEngine projectionLayer = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                    projectionLayer.dataLayerGateway = _dataLayerGateway;

                    ManualResetEvent doneEvent = new ManualResetEvent(false);

                    DtiTask dtiTask = new DtiTask(doneEvent, projectionLayer, _dataLayerGateway, _dictionary,
                      _graphMap, filter, pageSize, offset);

                    doneEvents.Add(doneEvent);
                    dtiTasks.Add(dtiTask);

                    ThreadPool.QueueUserWorkItem(dtiTask.ThreadPoolCallback, threadCount);
                }

                _logger.Debug("Number of threads [" + threadCount + "].");
                _logger.Debug("DTI tasks started!");

                // wait for all tasks to complete
                WaitHandle.WaitAll(doneEvents.ToArray());

                _logger.Debug("DTI tasks completed!");

                // collect DTIs from the tasks
                for (int i = 0; i < threadCount; i++)
                {
                    DataTransferIndices dtis = dtiTasks[i].DataTransferIndices;

                    if (dtis != null)
                    {
                        dataTransferIndices.DataTransferIndexList.AddRange(dtis.DataTransferIndexList);
                    }
                }

                _logger.Debug("DTIs assembled count: " + dataTransferIndices.DataTransferIndexList.Count);
            }

            return dataTransferIndices;
        }

        private DataTransferObjects MultiGetDataTransferObjects(DataObject dataObject, List<string> identifiers)
        {
            DataTransferObjects dataTransferObjects = new DataTransferObjects();

            int total = identifiers.Count;
            int maxThreads = int.Parse(_settings["MaxThreads"]);

            int numOfThreads = Math.Min(total, maxThreads);
            int itemsPerThread = (int)Math.Ceiling((double)total / numOfThreads);

            List<ManualResetEvent> doneEvents = new List<ManualResetEvent>();
            List<OutboundDtoTask> dtoTasks = new List<OutboundDtoTask>();
            int threadCount;

            for (threadCount = 0; threadCount < numOfThreads; threadCount++)
            {
                int offset = threadCount * itemsPerThread;
                if (offset >= total)
                    break;

                int pageSize = (offset + itemsPerThread > total) ? (int)(total - offset) : itemsPerThread;
                List<string> pageIdentifiers = identifiers.GetRange(offset, pageSize);
                DtoProjectionEngine projectionLayer = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                projectionLayer.dataLayerGateway = _dataLayerGateway;
                ManualResetEvent doneEvent = new ManualResetEvent(false);
                OutboundDtoTask dtoTask = new OutboundDtoTask(doneEvent, projectionLayer, _dataLayerGateway, _graphMap, dataObject, pageIdentifiers);
                ThreadPool.QueueUserWorkItem(dtoTask.ThreadPoolCallback, threadCount);

                doneEvents.Add(doneEvent);
                dtoTasks.Add(dtoTask);
            }

            _logger.Debug("Number of threads [" + threadCount + "].");
            _logger.Debug("Items per thread [" + itemsPerThread + "].");

            // wait for all tasks to complete
            WaitHandle.WaitAll(doneEvents.ToArray());

            // collect DTIs from the tasks
            for (int i = 0; i < threadCount; i++)
            {
                DataTransferObjects dtos = dtoTasks[i].DataTransferObjects;

                if (dtos != null)
                {
                    dataTransferObjects.DataTransferObjectList.AddRange(dtos.DataTransferObjectList);
                }
            }

            return dataTransferObjects;
        }

        private Response MultiPostDataTransferObjects(DataLayerGateway dataLayerGateway,
          DataObject objectType, DataTransferObjects dataTransferObjects)
        {
            Response response = new Response();

            {
                int total = dataTransferObjects.DataTransferObjectList.Count;
                int maxThreads = int.Parse(_settings["MaxThreads"]);

                int numOfThreads = Math.Min(total, maxThreads);
                int itemsPerThread = (int)Math.Ceiling((double)total / numOfThreads);

                List<ManualResetEvent> doneEvents = new List<ManualResetEvent>();
                List<DataTransferObjectsTask> dtoTasks = new List<DataTransferObjectsTask>();
                int threadCount;

                for (threadCount = 0; threadCount < numOfThreads; threadCount++)
                {
                    int offset = threadCount * itemsPerThread;
                    if (offset >= total)
                        break;

                    int pageSize = (offset + itemsPerThread > total) ? (int)(total - offset) : itemsPerThread;
                    DataTransferObjects dtos = new DataTransferObjects();
                    dtos.DataTransferObjectList = dataTransferObjects.DataTransferObjectList.GetRange(offset, pageSize);

                    DtoProjectionEngine projectionLayer = (DtoProjectionEngine)_kernel.Get<IProjectionLayer>("dto");
                    IDataLayer dataLayer = _kernel.Get<IDataLayer>();

                    projectionLayer.dataLayerGateway = _dataLayerGateway;

                    ManualResetEvent doneEvent = new ManualResetEvent(false);
                    DataTransferObjectsTask dtoTask = new DataTransferObjectsTask(doneEvent, projectionLayer, dataLayerGateway, _graphMap, objectType, dtos);
                    ThreadPool.QueueUserWorkItem(dtoTask.ThreadPoolCallback, threadCount);

                    doneEvents.Add(doneEvent);
                    dtoTasks.Add(dtoTask);
                }

                _logger.Debug("Number of threads [" + threadCount + "].");
                _logger.Debug("Items per thread [" + itemsPerThread + "].");

                // wait for all tasks to complete
                WaitHandle.WaitAll(doneEvents.ToArray());

                // collect responses from the tasks
                for (int i = 0; i < threadCount; i++)
                {
                    response.Append(dtoTasks[i].Response);
                }
            }

            return response;
        }

        public void FormatOutgoingMessage<T>(T graph, string format, bool useDataContractSerializer)
        {
            if (format.ToUpper() == "JSON")
            {
                string json = Utility.SerializeJson<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
                HttpContext.Current.Response.Write(json);
            }
            else
            {
                string xml = Utility.Serialize<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/xml";
                HttpContext.Current.Response.Write(xml);
            }
        }

        /// <summary>
        /// Adds new data properties(dbDataType, precision  and scale) to graphMap from dataobject.
        /// </summary>
        /// <param name="_graphMap">GraphMap</param>
        /// <param name="dataObject">DataObject</param>
        private void AddNewDataProperties(GraphMap _graphMap, DataObject dataObject)
        {
            // adding dbDataType, precision and scale to graphMap with dataProperties.
            foreach (ClassTemplateMap classTemplateMap in _graphMap.classTemplateMaps)
            {
                //find column name from roleMap.Use the column name to find DBdatatype, precision and scale from dataProperties.
                if (classTemplateMap.templateMaps != null && dataObject != null)
                {
                    foreach (TemplateMap templateMap in classTemplateMap.templateMaps)
                    {
                        foreach (RoleMap roleMap in templateMap.roleMaps)
                        {
                            if (roleMap.propertyName != null)
                            {
                                //string strTableName= roleMap.
                                string strColumnName = roleMap.propertyName;
                                strColumnName = strColumnName.Substring(strColumnName.LastIndexOf('.') + 1);

                                DataProperty dataProperty = dataObject.dataProperties.Where(x => x.columnName == strColumnName).FirstOrDefault();
                                if (dataProperty != null)
                                {
                                    roleMap.dbDataType = dataProperty.dataType.ToString();
                                    roleMap.precision = Convert.ToInt16(dataProperty.precision);
                                    roleMap.scale = Convert.ToInt16(dataProperty.scale);
                                }
                            }
                        }
                    }
                }
            }
        }


    }
}