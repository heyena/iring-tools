using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using Ninject;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;


namespace iRINGTools.Web.Models
{
    public class AdapterRepository : IAdapterRepository
    {
        private NameValueCollection _settings = null;
        private WebHttpClient _client = null;
        private string _refDataServiceURI = string.Empty;

        [Inject]
        public AdapterRepository()
        {
            _settings = ConfigurationManager.AppSettings;
            _client = new WebHttpClient(_settings["AdapterServiceUri"]);
        }

        public ScopeProjects GetScopes()
        {
            ScopeProjects obj = null;

            try
            {
                obj = _client.Get<ScopeProjects>("/scopes");
            }
            catch (Exception ex)
            {
            }

            return obj;

        }

        public string PostScopes(ScopeProjects scopes)
        {
            string obj = null;

            try
            {
                obj = _client.Post<ScopeProjects>("/scopes", scopes, true);
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public DataLayers GetDataLayers()
        {
            DataLayers obj = null;

            try
            {
                obj = _client.Get<DataLayers>("/datalayers");
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public ScopeProject GetScope(string scopeName)
        {
            ScopeProjects scopes = GetScopes();

            return scopes.FirstOrDefault<ScopeProject>(o => o.Name == scopeName);
        }

        public ScopeApplication GetScopeApplication(string scopeName, string applicationName)
        {
            ScopeProject scope = GetScope(scopeName);

            ScopeApplication obj = null;

            try
            {
                obj = scope.Applications.FirstOrDefault<ScopeApplication>(o => o.Name == applicationName);
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public DataDictionary GetDictionary(string scopeName, string applicationName)
        {
            DataDictionary obj = null;

            try
            {
                obj = _client.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", scopeName, applicationName), true);
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public Mapping GetMapping(string scopeName, string applicationName)
        {
            Mapping obj = null;

            try
            {
                obj = _client.Get<Mapping>(String.Format("/{0}/{1}/mapping", scopeName, applicationName), true);
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public XElement GetBinding(string scope, string application)
        {
            XElement obj = null;

            try
            {
                obj = _client.Get<XElement>(String.Format("/{0}/{1}/binding", scope, application), true);
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public string UpdateBinding(string scope, string application, string dataLayer)
        {
            string obj = null;

            try
            {
                XElement binding = new XElement("module",
            new XAttribute("name", string.Format("{0}.{1}", scope, application)),
              new XElement("bind",
                new XAttribute("name", "DataLayer"),
                new XAttribute("service", "org.iringtools.library.IDataLayer2, iRINGLibrary"),
                new XAttribute("to", dataLayer)
              )
            );

                obj = _client.Post<XElement>(String.Format("/{0}/{1}/binding", scope, application), binding, true);

            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public DataLayer GetDataLayer(string scopeName, string applicationName)
        {
            XElement binding = GetBinding(scopeName, applicationName);

            DataLayer dataLayer = null;

            if (binding != null)
            {
                dataLayer = new DataLayer();
                dataLayer.Assembly = binding.Element("bind").Attribute("to").Value;
                dataLayer.Name = binding.Element("bind").Attribute("to").Value.Split(',')[1].Trim();
            }

            return dataLayer;
        }

        public string AddScope(ScopeProject scope)
        {
            ScopeProjects scopes = GetScopes();
            scopes.Add(scope);

            return PostScopes(scopes);
        }

        public string UpdateScope(string scopeName, string name, string description)
        {
            ScopeProjects scopes = GetScopes();
            ScopeProject scope = scopes.FirstOrDefault<ScopeProject>(o => o.Name == scopeName);

            if (scope == null)
            {
                scope = new ScopeProject()
                {
                    Name = name,
                    Description = description,
                    Applications = new ScopeApplications()
                };
                scopes.Add(scope);
            }
            else
            {
                scope.Name = name;
                scope.Description = description;
            }
            return PostScopes(scopes);
        }

        public string DeleteScope(string scopeName)
        {
            ScopeProjects scopes = GetScopes();
            ScopeProject scope = scopes.FirstOrDefault<ScopeProject>(o => o.Name == scopeName);

            scopes.Remove(scope);

            return PostScopes(scopes);
        }

        public string UpdateApplication(string scopeName, string applicationName, string name, string description, string assembly)
        {
            ScopeProjects scopes = GetScopes();
            ScopeProject scope = scopes.FirstOrDefault<ScopeProject>(o => o.Name == scopeName);
            ScopeApplication application = scope.Applications.FirstOrDefault<ScopeApplication>(o => o.Name == applicationName);

            if (scope != null)
            {
                if (application == null)
                {
                    application = new ScopeApplication()
                    {
                        Name = name,
                        Description = description
                    };

                    scope.Applications.Add(application);
                }
                else
                {
                    application.Name = name;
                    application.Description = description;
                }
            }

            //First Step: Save Scopes
            string result= PostScopes(scopes);

            //Second Step: UpdateBinding
            UpdateBinding(scopeName, applicationName, assembly);
            return result;
        }

        public string DeleteApplication(string scopeName, string applicationName)
        {
            ScopeProjects scopes = GetScopes();
            ScopeProject scope = scopes.FirstOrDefault<ScopeProject>(o => o.Name == scopeName);
            ScopeApplication application = scope.Applications.FirstOrDefault<ScopeApplication>(o => o.Name == applicationName);

            scope.Applications.Remove(application);

            return PostScopes(scopes);
        }

        #region NHibernate Configuration Wizard support methods
        public DataProviders GetDBProviders()
        {
          WebHttpClient client = new WebHttpClient(_settings["NHibernateServiceURI"]);
          return client.Get<DataProviders>("/providers");
        }

        public DatabaseDictionary GetDBDictionary(string scope, string application)
        {
          WebHttpClient client = new WebHttpClient(_settings["NHibernateServiceURI"]);
          return client.Get<DatabaseDictionary>(String.Format("/{0}/{1}/dictionary", scope, application));
        }

        public List<string> GetTableNames(string scope, string application, string dbProvider, string dbServer,
          string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword)
        {
          WebHttpClient client = new WebHttpClient(_settings["NHibernateServiceURI"]);
          return client.Get<List<string>>(String.Format("/{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8}/tables", 
            scope, application, dbProvider, dbServer, dbInstance, dbName, dbSchema, dbUserName, dbPassword));
        }

        //TODO: use appropriate icons especially node with children
        public List<JsonTreeNode> GetDBObjects(string scope, string application, string dbProvider, string dbServer,
          string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string tableNames)
        {
          List<JsonTreeNode> dbObjectNodes = new List<JsonTreeNode>();

          WebHttpClient client = new WebHttpClient(_settings["NHibernateServiceURI"]);
          List<DataObject> dataObjects = client.Get<List<DataObject>>(String.Format("/{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8}/{9}/objects",
            scope, application, dbProvider, dbServer, dbInstance, dbName, dbSchema, dbUserName, dbPassword, tableNames));

          foreach (DataObject dataObject in dataObjects)
          {
            JsonTreeNode keyPropertiesNode = new JsonTreeNode()
            {
              text = "Keys",
              type = "keys",
              leaf = false,
              children = new List<JsonTreeNode>()
            };

            JsonTreeNode dataPropertiesNode = new JsonTreeNode()
            {
              text = "Properties",
              type = "properties",
              leaf = false,
              children = new List<JsonTreeNode>()
            };

            JsonTreeNode relationshipsNode = new JsonTreeNode()
            {
              text = "Relationships",
              type = "relationships",
              leaf = false,
              children = new List<JsonTreeNode>()
            };

            // create data object node
            JsonTreeNode dataObjectNode = new JsonTreeNode()
            {
              text = dataObject.tableName,
              type = "dataObject",
              leaf = false,
              children = new List<JsonTreeNode>()
              {
                keyPropertiesNode, dataPropertiesNode, relationshipsNode
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
                  properties = properties,
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
                  leaf = true,
                  properties = properties
                };

                dataPropertiesNode.children.Add(dataPropertyNode);
              }
            }

            // add relationship nodes
            foreach (DataRelationship relationship in dataObject.dataRelationships)
            {
              JsonTreeNode relationshipNode = new JsonTreeNode()
              {
                text = relationship.relationshipName,
                type = "relationship",
                leaf = true
              };

              dataPropertiesNode.children.Add(relationshipNode);
            }

            dbObjectNodes.Add(dataObjectNode);
          }

          return dbObjectNodes;
        }
        #endregion
    }
}