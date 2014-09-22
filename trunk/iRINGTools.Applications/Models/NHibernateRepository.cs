using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Mvc;
using org.iringtools.library;
using org.iringtools.utility;
using System.Collections.Specialized;
using iRINGTools.Web.Helpers;
using log4net;
using Ninject;
using System.Configuration;

namespace org.iringtools.web.Models
{
    public class NHibernateRepository : INHibernateRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateRepository));
        protected ServiceSettings _settings;
        protected string _proxyHost;
        protected string _proxyPort;
        protected string _adapterServiceUri = null;
        protected string _dataServiceUri = null;
        protected string _hibernateServiceUri = null;
        protected string _referenceDataServiceUri = null;

        [Inject]
        public NHibernateRepository()
        {
            NameValueCollection settings = ConfigurationManager.AppSettings;

            _settings = new ServiceSettings();
            _settings.AppendSettings(settings);

            _proxyHost = _settings["ProxyHost"];
            _proxyPort = _settings["ProxyPort"];

            _adapterServiceUri = _settings["AdapterServiceUri"];
            _dataServiceUri = _settings["DataServiceUri"];
            _hibernateServiceUri = _settings["NHibernateServiceUri"];
            _referenceDataServiceUri = _settings["ReferenceDataServiceUri"];

        }

        protected WebHttpClient CreateWebClient(string baseUri)
        {
            WebHttpClient client = null;

            if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
            {
                var webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
                client = new WebHttpClient(baseUri, null, webProxy);
            }
            else
            {
                client = new WebHttpClient(baseUri);
            }

            return client;
        }

        //public Response RegenAll(string user)
        //{
        //  var totalObj = new Response();
        //  var key = user + "." + "directory";
        //  ScopeProject directory = null;
        //  if (HttpContext.Current.Session[key] != null)
        //    directory = (Directories)HttpContext.Current.Session[key];

        //  foreach (Folder folder in directory)
        //  {
        //    GenerateFolders(folder, totalObj);
        //  }
        //  return totalObj;
        //}

        public DataDictionary GetDictionary(string contextName, string endpoint, string baseUrl)
        {
            DataDictionary obj = null;

            try
            {
                var _newServiceClient = CreateWebClient(_hibernateServiceUri);
                obj = _newServiceClient.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", contextName, endpoint), true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }

            return obj;
        }

        //private static void SetNodeIconClsMap()
        //{
        //  nodeIconClsMap = new Dictionary<string, NodeIconCls>()
        //  {
        //    {"folder", NodeIconCls.folder},
        //    {"project", NodeIconCls.project},
        //    {"scope", NodeIconCls.scope},
        //    {"proj", NodeIconCls.project},
        //    {"application", NodeIconCls.application},
        //    {"app", NodeIconCls.application},
        //    {"resource", NodeIconCls.resource},
        //    {"resrc", NodeIconCls.resource}, 	
        //    {"key", NodeIconCls.key}, 
        //    {"property", NodeIconCls.property}, 
        //    {"relation", NodeIconCls.relation} 
        //  };
        //}

        #region NHibernate Configuration Wizard support methods
        public DataProviders GetDbProviders()
        {
            var newServiceClient = CreateWebClient(_hibernateServiceUri);
            return newServiceClient.Get<DataProviders>("/providers");
        }

        public string SaveDbDictionary(string scope, string application, string tree)
        {
            var newServiceClient = CreateWebClient(_hibernateServiceUri);
            var dbDictionary = Utility.FromJson<DatabaseDictionary>(tree);

            var connStr = dbDictionary.ConnectionString;
            if (!String.IsNullOrEmpty(connStr))
            {
                string urlEncodedConnStr = Utility.DecodeFrom64(connStr);
                dbDictionary.ConnectionString = HttpUtility.UrlDecode(urlEncodedConnStr);
            }

            string postResult = null;
            try
            {
                postResult = newServiceClient.Post<DatabaseDictionary>("/" + scope + "/" + application + "/dictionary", dbDictionary, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error posting DatabaseDictionary." + ex);
            }
            return postResult;
        }

        public List<string> GetTableNames(string scope, string application, string dbProvider, string dbServer,
          string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string portNumber, string serName)
        {
            var newServiceClient = CreateWebClient(_hibernateServiceUri);
            var uri = String.Format("/{0}/{1}/tables", scope, application);

            var request = new Request
        {
          {"dbProvider", dbProvider},
          {"dbServer", dbServer},
          {"portNumber", portNumber},
          {"dbInstance", dbInstance},
          {"dbName", dbName},
          {"dbSchema", dbSchema},
          {"dbUserName", dbUserName},
          {"dbPassword", dbPassword},
          {"serName", serName}
        };

            return newServiceClient.Post<Request, List<string>>(uri, request, true);
        }

        public DatabaseDictionary GetDbDictionary(string scope, string application)
        {
            var client = CreateWebClient(_hibernateServiceUri);
            var dbDictionary = client.Get<DatabaseDictionary>(String.Format("/{0}/{1}/dictionary", scope, application));

            var connStr = dbDictionary.ConnectionString;
            if (!String.IsNullOrEmpty(connStr))
            {
                dbDictionary.ConnectionString = Utility.EncodeTo64(connStr);
            }

            return dbDictionary;
        }
        // use appropriate icons especially node with children
        public List<JsonTreeNode> GetDbObjects(string scope, string application, string dbProvider, string dbServer,
         string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string tableNames, string portNumber, string serName)
        {
            var dbObjectNodes = new List<JsonTreeNode>();
            var hasDBDictionary = false;
            var hasDataObjectinDBDictionary = false;
            DatabaseDictionary dbDictionary = null;
            var uri = String.Format("/{0}/{1}/objects", scope, application);

            var request = new Request
        {
          {"dbProvider", dbProvider},
          {"dbServer", dbServer},
          {"portNumber", portNumber},
          {"dbInstance", dbInstance},
          {"dbName", dbName},
          {"dbSchema", dbSchema},
          {"dbUserName", dbUserName},
          {"dbPassword", dbPassword},
          {"tableNames", tableNames},
          {"serName", serName}
        };

            var client = CreateWebClient(_hibernateServiceUri);
            var dataObjects = client.Post<Request, List<DataObject>>(uri, request, true);

            try
            {
                dbDictionary = GetDbDictionary(scope, application);

                if (dbDictionary != null)
                    if (dbDictionary.dataObjects.Count > 0)
                        hasDBDictionary = true;
            }
            catch (Exception)
            {
                hasDBDictionary = false;
            }

            foreach (var dataObject in dataObjects)
            {
                hasDataObjectinDBDictionary = false;

                if (hasDBDictionary)
                    if (dbDictionary.dataObjects.FirstOrDefault<DataObject>(o => o.tableName.ToLower() == dataObject.tableName.ToLower()) != null)
                        hasDataObjectinDBDictionary = true;

                var keyPropertiesNode = new JsonTreeNode()
                {
                    text = "Keys",
                    type = "keys",
                    expanded = true,
                    iconCls = "folder",
                    leaf = false,

                    children = new List<JsonTreeNode>()
                };

                var dataPropertiesNode = new JsonTreeNode()
                {
                    text = "Properties",
                    type = "properties",
                    expanded = true,
                    iconCls = "folder",
                    leaf = false,
                    children = new List<JsonTreeNode>()
                };

                var relationshipsNode = new JsonTreeNode()
                {
                    text = "Relationships",
                    type = "relationships",
                    expanded = true,
                    iconCls = "folder",
                    leaf = false,
                    children = new List<JsonTreeNode>()
                };

                // create data object node
                var dataObjectNode = new JsonTreeNode()
                {
                    text = dataObject.tableName,
                    type = "dataObject",
                    iconCls = "treeObject",
                    leaf = false,
                    children = new List<JsonTreeNode>()
              {
                keyPropertiesNode, dataPropertiesNode, relationshipsNode
              },
                    properties = new Dictionary<string, string>
              {
                {"objectNamespace", "org.iringtools.adapter.datalayer.proj_" + scope + "." + application},
                {"objectName", dataObject.objectName},
                {"keyDelimiter", dataObject.keyDelimeter}
              }
                };

                // add key/data property nodes
                foreach (var dataProperty in dataObject.dataProperties)
                {
                    var properties = new Dictionary<string, string>()
              {
                {"columnName", dataProperty.columnName},
                {"propertyName", dataProperty.propertyName},
                {"dataType", dataProperty.dataType.ToString()},
                {"dataLength", dataProperty.dataLength.ToString()},
                {"nullable", dataProperty.isNullable.ToString()},
                {"showOnIndex", dataProperty.showOnIndex.ToString()},
                {"numberOfDecimals", dataProperty.numberOfDecimals.ToString()},
                {"isHidden", dataProperty.isHidden.ToString()},
                {"precision", dataProperty.precision.ToString()},
                {"scale", dataProperty.scale.ToString()}
              };

                    if (dataObject.isKeyProperty(dataProperty.propertyName) && !hasDataObjectinDBDictionary)
                    {
                        properties.Add("keyType", dataProperty.keyType.ToString());

                        var keyPropertyNode = new JsonTreeNode()
                        {
                            text = dataProperty.columnName,
                            type = "keyProperty",
                            properties = properties,
                            iconCls = "treeKey",
                            leaf = true
                        };

                        keyPropertiesNode.children.Add(keyPropertyNode);
                    }
                    else
                    {
                        var dataPropertyNode = new JsonTreeNode()
                        {
                            text = dataProperty.columnName,
                            type = "dataProperty",
                            iconCls = "treeProperty",
                            leaf = true,
                            hidden = true,
                            properties = properties
                        };

                        dataPropertiesNode.children.Add(dataPropertyNode);
                    }
                }

                dbObjectNodes.Add(dataObjectNode);
            }

            return dbObjectNodes;
        }

        //rootNode(dataObjectNode) is from database tables
        //dbDict(dataObject) is from database dictionary
        //private Tree DbOjbectsAndDbDictionary(Tree tree, string contextName, string endpoint, string baseUrl, DatabaseDictionary dbDict)
        //{
        //  string[] relationTypeStr = { "OneToOne", "OneToMany" };
        //  JsonTreeNode hiddenNode = null;
        //  DataRelationship relation = null;
        //  bool hasProperty = false;
        //  DataProperty tempDataProperty = null;
        //  JsonTreeNode tempPropertyNode = null;

        //  if (tree == null)
        //    tree = new Tree();
        //  var dbObjectNodes = tree.getNodes();


        //  // sync data object tree with data dictionary
        //  foreach (var dataObjectNode in dbObjectNodes.Cast<TreeNode>())
        //  {
        //    dataObjectNode.properties["tableName"] = dataObjectNode.text;

        //    foreach (var dataObject in dbDict.dataObjects.Where(dataObject => dataObjectNode.properties["tableName"].ToUpper() == dataObject.tableName.ToUpper()))
        //    {
        //      // sync data object
        //      dataObjectNode.properties["objectNamespace"] = dataObject.objectNamespace;
        //      dataObjectNode.properties["objectName"] = dataObject.objectName;
        //      dataObjectNode.properties["keyDelimiter"] = dataObject.keyDelimeter;
        //      dataObjectNode.properties["description"] = dataObject.description;
        //      dataObjectNode.text = dataObject.objectName;

        //      if (dataObject.objectName.ToLower() == dataObjectNode.text.ToLower())
        //      {
        //        var shownProperty = new List<string>();
        //        var keysNode = (TreeNode)dataObjectNode.children[0];
        //        var propertiesNode = (JsonPropertyNode)dataObjectNode.children[1];
        //        var hiddenRootNode = propertiesNode.hiddenNodes["hiddenNode"];
        //        var relationshipsNode = (TreeNode)dataObjectNode.children[2];

        //        // sync data properties
        //        for (int jj = 0; jj < dataObject.dataProperties.Count; jj++)            
        //        {
        //          tempDataProperty = dataObject.dataProperties[jj];
        //          hasProperty = false;

        //          for (int j = 0; j < propertiesNode.children.Count; j++)
        //          {
        //            tempPropertyNode = propertiesNode.children[j];

        //            if (tempPropertyNode.text.ToLower() == tempDataProperty.columnName.ToLower())
        //            {
        //              hasProperty = true;

        //              if (!tempDataProperty.isHidden)
        //              {
        //                if (!HasShown(shownProperty, tempPropertyNode.text.ToLower()))
        //                {
        //                  shownProperty.Add(tempPropertyNode.text.ToLower());
        //                  tempPropertyNode.hidden = false;
        //                }

        //                tempPropertyNode.text = tempDataProperty.propertyName;
        //                tempPropertyNode.properties["keyType"] = tempDataProperty.keyType.ToString();
        //                tempPropertyNode.properties["propertyName"] = tempDataProperty.propertyName;
        //                tempPropertyNode.properties["isHidden"] = tempDataProperty.isHidden.ToString();
        //              }
        //              else
        //              {
        //                JsonTreeNode newHiddenNode = new JsonTreeNode();
        //                newHiddenNode.text = tempDataProperty.propertyName;
        //                newHiddenNode.type = "DATAPROPERTY";
        //                newHiddenNode.id = propertiesNode.id + "/" + newHiddenNode.text;
        //                newHiddenNode.identifier = newHiddenNode.id;
        //                newHiddenNode.iconCls = "treeProperty";
        //                newHiddenNode.leaf = true;
        //                newHiddenNode.hidden = true;
        //                newHiddenNode.properties = new Dictionary<string, string>()
        //                  {
        //                    {"columnName", tempPropertyNode.properties["columnName"]},
        //                    {"propertyName", tempDataProperty.propertyName},
        //                    {"dataType", tempPropertyNode.properties["dataType"]},
        //                    {"keyType", tempDataProperty.keyType.ToString()},
        //                    {"dataLength", tempPropertyNode.properties["dataLength"]},
        //                    {"nullable", tempPropertyNode.properties["nullable"]},
        //                    {"showOnIndex", tempPropertyNode.properties["showOnIndex"]},
        //                    {"numberOfDecimals", tempPropertyNode.properties["numberOfDecimals"]},
        //                    {"isHidden", tempDataProperty.isHidden.ToString()}
        //                  };
        //                newHiddenNode.record = new
        //                  {
        //                    Name = tempPropertyNode.text
        //                  };
        //                hiddenRootNode.children.Add(newHiddenNode);
        //                propertiesNode.children.RemoveAt(j);
        //                j--;
        //              }
        //              break;
        //            }
        //          }

        //          if (!hasProperty)
        //            for (int j = 0; j < hiddenRootNode.children.Count; j++)
        //            {
        //              if (hiddenRootNode.children[j].text.ToLower() == tempDataProperty.columnName.ToLower())
        //              {
        //                hasProperty = true;
        //                if (!HasShown(shownProperty, hiddenRootNode.children[j].text.ToLower()))
        //                {
        //                  shownProperty.Add(hiddenRootNode.children[j].text.ToLower());
        //                  hiddenNode = hiddenRootNode.children[j];
        //                  JsonTreeNode dataPropertyNode = new JsonTreeNode();
        //                  dataPropertyNode.text = tempDataProperty.propertyName;
        //                  dataPropertyNode.type = "DATAPROPERTY";
        //                  dataPropertyNode.id = propertiesNode.id + "/" + dataPropertyNode.text;
        //                  dataPropertyNode.identifier = dataPropertyNode.id;
        //                  dataPropertyNode.iconCls = "treeProperty";
        //                  dataPropertyNode.leaf = true;
        //                  dataPropertyNode.hidden = false;
        //                  dataPropertyNode.properties = new Dictionary<string, string>()
        //                    {
        //                      {"columnName", hiddenNode.properties["columnName"]},
        //                      {"propertyName", tempDataProperty.propertyName},
        //                      {"dataType", hiddenNode.properties["dataType"]},
        //                      {"keyType", tempDataProperty.keyType.ToString()},
        //                      {"dataLength", hiddenNode.properties["dataLength"]},
        //                      {"nullable", hiddenNode.properties["nullable"]},
        //                      {"showOnIndex", hiddenNode.properties["showOnIndex"]},
        //                      {"numberOfDecimals", hiddenNode.properties["numberOfDecimals"]},
        //                      {"isHidden", tempDataProperty.isHidden.ToString()}
        //                    };
        //                  dataPropertyNode.record = new
        //                    {
        //                      Name = dataPropertyNode.text
        //                    };

        //                  propertiesNode.children.Add(dataPropertyNode);
        //                  hiddenRootNode.children.RemoveAt(j);
        //                  j--;
        //                  break;
        //                }
        //              }
        //            }

        //          if (!hasProperty)
        //          {
        //            Dictionary<string, string> properties = new Dictionary<string, string>()
        //              {
        //                {"columnName", tempDataProperty.columnName},
        //                {"propertyName", tempDataProperty.propertyName},
        //                {"dataType", tempDataProperty.dataType.ToString()},
        //                {"keyType", ""},
        //                {"dataLength", tempDataProperty.dataLength.ToString()},
        //                {"nullable", tempDataProperty.isNullable.ToString()},
        //                {"showOnIndex", tempDataProperty.showOnIndex.ToString()},
        //                {"numberOfDecimals", tempDataProperty.numberOfDecimals.ToString()},
        //                {"isHidden", tempDataProperty.isHidden.ToString()}
        //              };

        //            JsonTreeNode dataPropertyNode = new JsonTreeNode();
        //            dataPropertyNode.text = tempDataProperty.columnName;
        //            dataPropertyNode.type = "DATAPROPERTY";
        //            dataPropertyNode.id = dataPropertyNode.id + "/" + dataPropertyNode.text;
        //            dataPropertyNode.identifier = dataPropertyNode.id;
        //            dataPropertyNode.iconCls = "treeProperty";
        //            dataPropertyNode.leaf = true;
        //            dataPropertyNode.properties = properties;
        //            dataPropertyNode.record = new
        //              {
        //                Name = dataPropertyNode.text
        //              };

        //            if (tempDataProperty.isHidden)
        //            {
        //              dataPropertyNode.hidden = true;
        //              hiddenRootNode.children.Add(dataPropertyNode);
        //            }
        //            else
        //            {
        //              propertiesNode.children.Add(dataPropertyNode);
        //            }
        //          }
        //        }

        //        // sync key properties
        //        bool foundIkk = false;
        //        for (int ij = 0; ij < dataObject.keyProperties.Count; ij++)
        //        {
        //          for (int k = 0; k < keysNode.children.Count; k++)
        //          {
        //            for (int ikk = 0; ikk < dataObject.dataProperties.Count; ikk++)
        //            {
        //              if (dataObject.keyProperties[ij].keyPropertyName.ToLower() == dataObject.dataProperties[ikk].propertyName.ToLower())
        //              {
        //                if (keysNode.children[k].text.ToLower() == dataObject.dataProperties[ikk].columnName.ToLower())
        //                {
        //                  keysNode.children[k].text = dataObject.keyProperties[ij].keyPropertyName;
        //                  keysNode.children[k].properties["propertyName"] = dataObject.keyProperties[ij].keyPropertyName;
        //                  keysNode.children[k].properties["isHidden"] = "false";
        //                  keysNode.children[k].properties["keyType"] = "assigned";
        //                  ij++;
        //                  foundIkk = true;
        //                  break;
        //                }
        //              }
        //            }
        //            if (foundIkk)
        //              break;
        //          }
        //          if (ij < dataObject.keyProperties.Count)
        //          {
        //            string nodeText;
        //            for (int ijj = 0; ijj < propertiesNode.children.Count; ijj++)
        //            {
        //              nodeText = dataObject.keyProperties[ij].keyPropertyName;
        //              if (propertiesNode.children[ijj].text.ToLower() == nodeText.ToLower())
        //              {
        //                propertiesNode.children[ijj].properties["propertyName"] = nodeText;
        //                JsonTreeNode newKeyNode = new JsonTreeNode();
        //                newKeyNode.text = nodeText;
        //                newKeyNode.type = "keyProperty";
        //                newKeyNode.id = keysNode.id + "/" + newKeyNode.text;
        //                newKeyNode.identifier = newKeyNode.id;
        //                newKeyNode.leaf = true;
        //                newKeyNode.iconCls = "treeKey";
        //                newKeyNode.hidden = false;
        //                newKeyNode.properties = propertiesNode.children[ijj].properties;
        //                newKeyNode.record = new
        //                  {
        //                    Name = newKeyNode.text
        //                  };
        //                propertiesNode.children.RemoveAt(ijj);
        //                ijj--;

        //                if (newKeyNode != null)
        //                  keysNode.children.Add(newKeyNode);

        //                break;
        //              }
        //            }

        //            for (int ijj = 0; ijj < hiddenRootNode.children.Count; ijj++)
        //            {
        //              nodeText = dataObject.keyProperties[ij].keyPropertyName;
        //              if (hiddenRootNode.children[ijj].text.ToLower() == nodeText.ToLower())
        //              {
        //                hiddenRootNode.children[ijj].properties["propertyName"] = nodeText;
        //                JsonTreeNode newKeyNode = new JsonTreeNode();
        //                newKeyNode.text = nodeText;
        //                newKeyNode.type = "keyProperty";
        //                newKeyNode.id = keysNode.id + "/" + newKeyNode.text;
        //                newKeyNode.identifier = newKeyNode.id;
        //                newKeyNode.leaf = true;
        //                newKeyNode.iconCls = "treeKey";
        //                newKeyNode.hidden = false;
        //                newKeyNode.properties = hiddenRootNode.children[ijj].properties;
        //                newKeyNode.record = new
        //                  {
        //                    Name = newKeyNode.text
        //                  };
        //                hiddenRootNode.children.RemoveAt(ijj);
        //                ijj--;

        //                if (newKeyNode != null)
        //                  keysNode.children.Add(newKeyNode);

        //                break;
        //              }
        //            }
        //          }
        //        }

        //        // sync relationships 
        //        for (int kj = 0; kj < dataObject.dataRelationships.Count; kj++)
        //        {
        //          Dictionary<string, Object> relatedObjMap = new Dictionary<string, Object>();
        //          relation = dataObject.dataRelationships[kj];
        //          JsonRelationNode relationNode = new JsonRelationNode();
        //          relationNode.text = relation.relationshipName;
        //          relationNode.type = "relationship";
        //          relationNode.id = relationshipsNode.id + "/" + relationNode.text;
        //          relationNode.identifier = relationNode.id;
        //          relationNode.leaf = true;
        //          relationNode.iconCls = "treeRelation";              
        //          relationNode.relatedObjMap = relatedObjMap;
        //          relationNode.objectName = dataObjectNode.text;
        //          relationNode.relatedObjectName = relation.relatedObjectName;
        //          relationNode.relatedTableName = GetRelatedTableName(relation.relatedObjectName, dbDict);
        //          relationNode.relationshipType = relation.relationshipType.ToString();
        //          relationNode.relationshipTypeIndex = ((int)relation.relationshipType).ToString();
        //          relationNode.record = new
        //            {
        //              Name = relationNode.text
        //            };
        //          List<Dictionary<string, string>> mapArray = new List<Dictionary<string, string>>();
        //          for (int kjj = 0; kjj < relation.propertyMaps.Count; kjj++)
        //          {
        //            Dictionary<string, string> mapItem = new Dictionary<string, string>()
        //              {
        //                {"dataPropertyName", relation.propertyMaps[kjj].dataPropertyName},
        //                {"dataColumnName", GetColumnName(dataObject, relation.propertyMaps[kjj].dataPropertyName)},
        //                {"relatedPropertyName", relation.propertyMaps[kjj].relatedPropertyName},
        //                {"relatedColumnName", GetColumnName(GetRelatedDataObject(relationNode.relatedTableName, dbDict), relation.propertyMaps[kjj].relatedPropertyName)}
        //              };
        //            mapArray.Add(mapItem);
        //          }

        //          relationNode.propertyMap = mapArray;
        //          relationshipsNode.expanded = true;
        //          relationshipsNode.children.Add(relationNode);
        //        }
        //      }
        //    }
        //  }

        //  return tree;
        //}

        private static DataObject GetRelatedDataObject(String relatedTableName, DatabaseDictionary dbDict)
        {
            return dbDict == null ? null : dbDict.dataObjects.FirstOrDefault(d => d.tableName.Equals(relatedTableName, StringComparison.InvariantCultureIgnoreCase));
        }

        private string GetRelatedTableName(string relatedObjectName, DatabaseDictionary dbDict)
        {
            return dbDict == null ? "" : dbDict.dataObjects.FirstOrDefault(r => r.objectName.Equals(relatedObjectName, StringComparison.InvariantCultureIgnoreCase)).tableName;
        }

        private string GetColumnName(DataObject dataObject, string propertyName)
        {
            if (dataObject == null)
                return "";
            return dataObject.dataProperties.FirstOrDefault(p => p.columnName.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)).columnName;
        }

        private static Boolean HasShown(IEnumerable<string> shownArray, string text)
        {
            return shownArray.Any(t => t == text);
        }

        //  private WebHttpClient PrepareServiceClient(string baseUrl, string serviceName)
        //{
        //  if (baseUrl == "" || baseUrl == null)
        //    return _hibernateServiceClient;

        //  string baseUri = CleanBaseUrl(baseUrl.ToLower(), '/');
        //  string adapterBaseUri = CleanBaseUrl(hibernateServiceUri.ToLower(), '/');

        //  if (!baseUri.Equals(adapterBaseUri))
        //    return GetServiceClient(baseUrl, serviceName);
        //  else
        //    return _hibernateServiceClient;
        //}

        //private WebHttpClient GetServiceClient(string uri, string serviceName)
        //{
        //  WebHttpClient _newServiceClient = null;
        //  string serviceUri = uri + "/" + serviceName;

        //  if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
        //  {
        //    _newServiceClient = new WebHttpClient(serviceUri, null, webProxy);
        //  }
        //  else
        //  {
        //    _newServiceClient = new WebHttpClient(serviceUri);
        //  }
        //  return _newServiceClient;
        //}

        private string CleanBaseUrl(string url, char con)
        {
            var uri = new System.Uri(url);
            return uri.Scheme + ":" + con + con + uri.Host + ":" + uri.Port;
        }

        //  private Response GenerateFolders(Folder folder, Response totalObj)
        //  {
        //    Response obj = null;
        //    Endpoints endpoints = folder.Endpoints;

        //    if (endpoints != null)
        //    {
        //      foreach (Endpoint endpoint in endpoints)
        //      {
        //        WebHttpClient _newServiceClient = PrepareServiceClient(endpoint.BaseUrl, "adapter");
        //        obj = _newServiceClient.Get<Response>(String.Format("/{0}/{1}/generate", endpoint.Context, endpoint.Name));
        //        totalObj.Append(obj);
        //      }
        //    }

        //    Folders subFolders = folder.Folders;

        //    if (subFolders == null)
        //      return totalObj;
        //    else
        //    {
        //      foreach (Folder subFolder in subFolders)
        //      {
        //        obj = GenerateFolders(subFolder, totalObj);
        //      }
        //    }

        //    return totalObj;
        //  }

        #endregion
    }

    public interface INHibernateRepository
    {
        DataDictionary GetDictionary(string contextName, string endpoint, string baseUrl);
    }

}

