using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
    public class NHibernateRopsitory  : INHibernateRe3pository
    {
        private NameValueCollection _settings = null;
        private WebHttpClient _client = null;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateRopsitory));
        private static Dictionary<string, NodeIconCls> nodeIconClsMap;

        [Inject]
        public NHibernateRopsitory()
        {
            _settings = ConfigurationManager.AppSettings;
            _client = new WebHttpClient(_settings["AdapterServiceUri"]);
            SetNodeIconClsMap();
        }

        public string GetNodeIconCls(string type)
        {
            try
            {
                switch (nodeIconClsMap[type.ToLower()])
                {
                    case NodeIconCls.folder: return "folder";
                    case NodeIconCls.project: return "treeProject";
                    case NodeIconCls.application: return "application";
                    case NodeIconCls.resource: return "resource";
                    case NodeIconCls.key: return "treeKey";
                    case NodeIconCls.property: return "treeProperty";
                    case NodeIconCls.relation: return "treeRelation";
                    default: return "folder";
                }
            }
            catch 
            {
                return "folder";
            }
        }

        public DataDictionary GetDictionary(string contextName, string endpoint)
        {
            DataDictionary obj = null;

            try
            {
                obj = _client.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", contextName, endpoint), true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }

            return obj;
        }

        private static void SetNodeIconClsMap()
        {
            nodeIconClsMap = new Dictionary<string, NodeIconCls>()
          {
  	        {"folder", NodeIconCls.folder},
  	        {"project", NodeIconCls.project},
  	        {"scope", NodeIconCls.scope},
  	        {"proj", NodeIconCls.project},
  	        {"application", NodeIconCls.application},
  	        {"app", NodeIconCls.application},
  	        {"resource", NodeIconCls.resource},
  	        {"resrc", NodeIconCls.resource}, 	
            {"key", NodeIconCls.key}, 
            {"property", NodeIconCls.property}, 
            {"relation", NodeIconCls.relation} 
          };
        }

        #region NHibernate Configuration Wizard support methods
        public DataProviders GetDBProviders()
        {
            WebHttpClient client = new WebHttpClient(_settings["NHibernateServiceURI"]);
            return client.Get<DataProviders>("/providers");
        }

        public string SaveDBDictionary(string scope, string application, string tree)
        {
            WebHttpClient client = new WebHttpClient(_settings["NHibernateServiceURI"]);
            DatabaseDictionary dbDictionary = Utility.FromJson<DatabaseDictionary>(tree);

            string connStr = dbDictionary.ConnectionString;
            if (!String.IsNullOrEmpty(connStr))
            {
                string urlEncodedConnStr = Utility.DecodeFrom64(connStr);
                dbDictionary.ConnectionString = HttpUtility.UrlDecode(urlEncodedConnStr);
            }

            string postResult = null;
            try
            {
                postResult = client.Post<DatabaseDictionary>("/" + scope + "/" + application + "/dictionary", dbDictionary, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error posting DatabaseDictionary." + ex);
            }
            return postResult;
        }

        public DatabaseDictionary GetDBDictionary(string context, string application)
        {
            WebHttpClient client = new WebHttpClient(_settings["NHibernateServiceURI"]);
            DatabaseDictionary dbDictionary = client.Get<DatabaseDictionary>(String.Format("/{0}/{1}/dictionary", context, application));

            string connStr = dbDictionary.ConnectionString;
            if (!String.IsNullOrEmpty(connStr))
            {
                dbDictionary.ConnectionString = Utility.EncodeTo64(connStr);
            }

            return dbDictionary;
        }

        public List<string> GetTableNames(string scope, string application, string dbProvider, string dbServer,
          string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string portNumber, string serName)
        {
            WebHttpClient client = new WebHttpClient(_settings["NHibernateServiceURI"]);
            var uri = String.Format("/{0}/{1}/tables", scope, application);

            Request request = new Request();
            request.Add("dbProvider", dbProvider);
            request.Add("dbServer", dbServer);
            request.Add("portNumber", portNumber);
            request.Add("dbInstance", dbInstance);
            request.Add("dbName", dbName);
            request.Add("dbSchema", dbSchema);
            request.Add("dbUserName", dbUserName);
            request.Add("dbPassword", dbPassword);
            request.Add("serName", serName);

            return client.Post<Request, List<string>>(uri, request, true);
        }

        // use appropriate icons especially node with children
        public List<JsonTreeNode> GetDBObjects(string scope, string application, string dbProvider, string dbServer,
          string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string tableNames, string portNumber, string serName)
        {
            List<JsonTreeNode> dbObjectNodes = new List<JsonTreeNode>();

            WebHttpClient client = new WebHttpClient(_settings["NHibernateServiceURI"]);
            var uri = String.Format("/{0}/{1}/objects", scope, application);

            Request request = new Request();
            request.Add("dbProvider", dbProvider);
            request.Add("dbServer", dbServer);
            request.Add("portNumber", portNumber);
            request.Add("dbInstance", dbInstance);
            request.Add("dbName", dbName);
            request.Add("dbSchema", dbSchema);
            request.Add("dbUserName", dbUserName);
            request.Add("dbPassword", dbPassword);
            request.Add("tableNames", tableNames);
            request.Add("serName", serName);

            List<DataObject> dataObjects = client.Post<Request, List<DataObject>>(uri, request, true);

            foreach (DataObject dataObject in dataObjects)
            {
                TreeNode keyPropertiesNode = new TreeNode()
                {
                    text = "Keys",
                    type = "keys",
                    expanded = true,
                    iconCls = "folder",
                    leaf = false,
                    children = new List<JsonTreeNode>()
                };

                TreeNode dataPropertiesNode = new TreeNode()
                {
                    text = "Properties",
                    type = "properties",
                    expanded = true,
                    iconCls = "folder",
                    leaf = false,
                    children = new List<JsonTreeNode>()
                };

                TreeNode relationshipsNode = new TreeNode()
                {
                    text = "Relationships",
                    type = "relationships",
                    expanded = true,
                    iconCls = "folder",
                    leaf = false,
                    children = new List<JsonTreeNode>()
                };

                // create data object node
                TreeNode dataObjectNode = new TreeNode()
                {
                    text = dataObject.tableName,
                    type = "dataObject",
                    iconCls = "treeObject",
                    leaf = false,
                    children = new List<JsonTreeNode>()
              {
                keyPropertiesNode, dataPropertiesNode, relationshipsNode
              },
                    nhproperty = new Dictionary<string, string>
              {
                {"objectNamespace", "org.iringtools.adapter.datalayer.proj_" + scope + "." + application},
                {"objectName", dataObject.objectName},
                {"keyDelimiter", dataObject.keyDelimeter}
              }
                };

                // add key/data property nodes
                foreach (DataProperty dataProperty in dataObject.dataProperties)
                {
                    Dictionary<string, string> properties = new Dictionary<string, string>()
              {
                {"columnName", dataProperty.columnName},
                {"propertyName", dataProperty.propertyName},
                {"dataType", dataProperty.dataType.ToString()},
                {"dataLength", dataProperty.dataLength.ToString()},
                {"nullable", dataProperty.isNullable.ToString()},
                {"showOnIndex", dataProperty.showOnIndex.ToString()},
                {"numberOfDecimals", dataProperty.numberOfDecimals.ToString()},
              };

                    if (dataObject.isKeyProperty(dataProperty.propertyName))
                    {
                        properties.Add("keyType", dataProperty.keyType.ToString());

                        JsonTreeNode keyPropertyNode = new JsonTreeNode()
                        {
                            text = dataProperty.columnName,
                            type = "keyProperty",
                            nhproperty = properties,
                            iconCls = "treeKey",
                            leaf = true
                        };

                        keyPropertiesNode.children.Add(keyPropertyNode);
                    }
                    else
                    {
                        JsonTreeNode dataPropertyNode = new JsonTreeNode()
                        {
                            text = dataProperty.columnName,
                            type = "dataProperty",
                            iconCls = "treeProperty",
                            leaf = true,
                            hidden = true,
                            nhproperty = properties
                        };

                        dataPropertiesNode.children.Add(dataPropertyNode);
                    }
                }

                // add relationship nodes
                if (dataObject.dataRelationships.Count == 0)
                {
                    JsonTreeNode relationshipNode = new JsonTreeNode()
                    {
                        text = "",
                        type = "relationship",
                        leaf = true,
                        hidden = true
                    };
                    relationshipsNode.children.Add(relationshipNode);
                }

                foreach (DataRelationship relationship in dataObject.dataRelationships)
                {
                    JsonTreeNode relationshipNode = new JsonTreeNode()
                    {
                        text = relationship.relationshipName,
                        type = "relationship",
                        iconCls = "relation",
                        leaf = true
                    };

                    relationshipsNode.children.Add(relationshipNode);
                }

                dbObjectNodes.Add(dataObjectNode);
            }

            return dbObjectNodes;
        }

        public Response RegenAll()
        {
            WebHttpClient client = new WebHttpClient(_settings["NHibernateServiceURI"]);
            return client.Get<Response>("/regen");
        }
        #endregion
    }

    public interface INHibernateRe3pository
    {
        DataDictionary GetDictionary(string contextName, string endpoint);
    }

}

