using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using Ninject;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;
using System.Text;


namespace iRINGTools.Web.Models
{
    public class AdapterRepository : IAdapterRepository
    {
        private NameValueCollection _settings = null;
        private WebHttpClient _client = null;
        private WebHttpClient _javaCoreClient = null;
        private string _refDataServiceURI = string.Empty;
				private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));
        private static Dictionary<string, NodeIconCls> nodeIconClsMap;   
        private string scopeName;
        private string appName;     
        
        [Inject]
        public AdapterRepository()
        {
          _settings = ConfigurationManager.AppSettings;
          _client = new WebHttpClient(_settings["AdapterServiceUri"]);
          _javaCoreClient = new WebHttpClient(_settings["JavaCoreUri"]);
          setNodeIconClsMap();
        }

        private static void setNodeIconClsMap()
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

        public string getScopeName()
        {
          return scopeName;
        }

        public string getAppName()
        {
          return appName;
        }

        public Directories GetScopes()
        {
          _logger.Debug("In AdapterRepository GetScopes");
          Directories obj = null;

          try
          {
            obj = _javaCoreClient.Get<Directories>("/directory");
            _logger.Debug("Successfully called Adapter.");
          }
          catch (Exception ex)  
          {
						_logger.Error(ex.ToString());
          }

          return obj;
        }

        public string getNodeIconCls(string type)
        {
          try
          {
            switch (nodeIconClsMap[type.ToLower()])
            {
              case NodeIconCls.folder: return "folder";
              case NodeIconCls.project: return "project";
              case NodeIconCls.application: return "application";
              case NodeIconCls.resource: return "resource";
              case NodeIconCls.key: return "key";
              case NodeIconCls.property: return "property";
              case NodeIconCls.relation: return "relation";
              default: return "folder";
            }
          }
          catch (Exception ex)
          {
            return "folder";
          }
        }

        public void getAppScopeName(string baseUri)
          {
            int index;
            for (int i = 0; i < 3; i++)
            {
              index = baseUri.LastIndexOf('/');

              if (i == 0)
                appName = baseUri.Substring(index + 1);
              else if (i == 1)
                scopeName = baseUri.Substring(index + 1);           

              baseUri = baseUri.Substring(0, index);
            }
          }

        public Tree GetDirectoryTree()
        {
          _logger.Debug("In ScopesNode case block");
          Directories directory = GetScopes();
          Tree tree = null;          

          if (directory != null)
          {
            tree = new Tree();
            List<JsonTreeNode> folderNodes = tree.getNodes();

            foreach (Folder folder in directory)
            {
              TreeNode folderNode = new TreeNode();
              folderNode.text = folder.Name;
              folderNode.id = folder.Name;
              folderNode.identifier = folderNode.id;
              folderNode.hidden = false;
              folderNode.leaf = false;
              folderNode.iconCls = getNodeIconCls(folder.type);
              folderNode.type = "folder";              
              Object record = new
              {
                Name = folder.Name                
              };
              folderNode.record = record;
              folderNodes.Add(folderNode);
              traverseDirectory(folderNode, folder);
            }            
          }
          return tree;
        }

        public XElement GetBinding()
        {
            XElement obj = null;

            try
            {
                obj = _client.Get<XElement>(String.Format("/{0}/{1}/binding", scopeName, appName), true);
            }
						catch (Exception ex)
						{
							_logger.Error(ex.ToString());
						}

            return obj;
        }

        public DataLayer GetDataLayer()
        {
            XElement binding = GetBinding();

            DataLayer dataLayer = null;

            if (binding != null)
            {
                dataLayer = new DataLayer();
                dataLayer.Assembly = binding.Element("bind").Attribute("to").Value;
                dataLayer.Name = binding.Element("bind").Attribute("to").Value.Split(',')[1].Trim();
            }

            return dataLayer;
        }

        private void traverseDirectory(TreeNode folderNode, Folder folder)
        {
          List<JsonTreeNode> folderNodeList = folderNode.getChildren();
  	      Endpoints endpoints = folder.endpoints;  	     
    
  	      if (endpoints != null)
          {  		
  		      foreach (Endpoint endpoint in endpoints) 
    	      {
              getAppScopeName(endpoint.baseUri);
              DataLayer dataLayer = GetDataLayer();
  			      LeafNode endPointNode = new LeafNode();
              endPointNode.text = endpoint.Name;
              endPointNode.iconCls = "application";
              endPointNode.type = "ApplicationNode";
              endPointNode.setLeaf(true);
              endPointNode.hidden = false;
              endPointNode.id = folderNode.id + "/" + endpoint.Name;
              endPointNode.identifier = endPointNode.id;
              endPointNode.nodeType = "async";        
              folderNodeList.Add(endPointNode);
        
              Object record = new
              {
                Name = endpoint.Name,
                Description = endpoint.Description,
                DataLayer = dataLayer.Name,
                Assembly = dataLayer.Assembly
              };

              endPointNode.record = record;
              Dictionary<string, string> properties = new Dictionary<string, string>();
              properties.Add("Name", endpoint.Name);
              properties.Add("Description", endpoint.Description);
              properties.Add("BaseURI", endpoint.baseUri);
              endPointNode.properties = properties;
    	      }
          }
    
          if (folder.folders == null)
    	      return;
          else
          {    	
    	      foreach (Folder subFolder in folder.folders)
    	      {
    		      TreeNode subFolderNode = new TreeNode();
    		      subFolderNode.text = subFolder.Name;
    		      subFolderNode.iconCls = getNodeIconCls(subFolder.type);
    		      subFolderNode.type = "folder";
              subFolderNode.hidden = false;
              subFolderNode.leaf = false;
              subFolderNode.id = folderNode.id + "/" + subFolder.Name;
              subFolderNode.identifier = subFolderNode.id;    		      
              Object record = new
              {
                Name = subFolder.Name
              };
              subFolderNode.record = record;
              folderNodeList.Add(subFolderNode);
    		      traverseDirectory(subFolderNode, subFolder);    		
    	      }
          }   
        }        

        public string PostScopes(Directories scopes)
        {
            string obj = null;

            try
            {
              obj = _client.Post<Directories>("/scopes", scopes, true);
            }
						catch (Exception ex)
						{
							_logger.Error(ex.ToString());
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
							_logger.Error(ex.ToString());
						}

            return obj;
        }

        public Entity GetClassLabel(string classId)
        {
            Entity entity = new Entity();
            try
            {
                WebHttpClient _tempClient = new WebHttpClient(_settings["ReferenceDataServiceUri"]);
                entity = _tempClient.Get<Entity>(String.Format("/classes/{0}/label", classId), true);
            }
						catch (Exception ex)
						{
							_logger.Error(ex.ToString());
						}
            return entity;
        }        

        public DataDictionary GetDictionary()
        {
            DataDictionary obj = null;

            try
            {
                obj = _client.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", scopeName, appName), true);
            }
						catch (Exception ex)
						{
							_logger.Error(ex.ToString());
						}

            return obj;
        }

        public Mapping GetMapping()
        {
            Mapping obj = null;

            try
            {
              obj = _client.Get<Mapping>(String.Format("/{0}/{1}/mapping", scopeName, appName), true);
            }
						catch (Exception ex)
						{
							_logger.Error(ex.ToString());
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
                new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
                new XAttribute("to", dataLayer)
              )
            );

                obj = _client.Post<XElement>(String.Format("/{0}/{1}/binding", scope, application), binding, true);

            }
						catch (Exception ex)
						{
							_logger.Error(ex.ToString());
						}

            return obj;
        }

        private Folders traverseGetFolders(Folder folder, string[] level, int depth)
        {
          if (folder.folders == null)
          {
            folder.folders = new Folders();
            return folder.folders;
          }
          else
          {
            if (level.Length > depth + 1)
            {
              foreach (Folder subFolder in folder.folders)
              {
                if (subFolder.Name == level[depth + 1])
                  return traverseGetFolders(subFolder, level, depth + 1);
              }
            }
            else
            {
              return folder.folders;
            }
          }
          return null;
        }

        private Folders getFolders(List<Folder> scopes, string path)
        {
          string[] level = path.Split('/');

          foreach (Folder folder in scopes)
          {
            if (folder.Name == level[0])
              return traverseGetFolders(folder, level, 0);            
          }
          return null;
        }        

        private void getLastName(string path, out string newpath, out string name)
        {
          int dotPos = path.LastIndexOf('/');
          newpath = path.Substring(0, dotPos - 1);
          name = path.Substring(dotPos + 1, path.Length - dotPos - 1);
        }

        private Folder findFolder(List<Folder> scopes, string path)
        {
          string folderName, newpath;
          getLastName(path, out newpath, out folderName);
          Folders folders = getFolders(scopes, newpath);
          return folders.FirstOrDefault<Folder>(o => o.Name == folderName);
        }

        public string Folder(string newFolderName, string description, string path, string state)
        {
          Directories scopes = GetScopes();
          if (state == "add")
          {            
            Folders folders = getFolders(scopes, path);
            Folder folder = new Folder();
            folder.Name = newFolderName;
            folder.Description = description;
            folders.Add(folder);            
          }
          else
          {
            Folder folder = findFolder(scopes, path);
            folder.Name = newFolderName;
            folder.Description = description;            
          }
          return PostScopes(scopes);
        }       

        public string DeleteFolder(string path)
        {
          Directories scopes = GetScopes();
          Folder scope = findFolder(scopes, path);
          scopes.Remove(scope);
          return PostScopes(scopes);
        }        

        private Endpoint GetEndpoint(List<Folder> scopes, string path, string endpointName)
        {          
          Folder folder = findFolder(scopes, path);
          return folder.endpoints.FirstOrDefault<Endpoint>(o => o.Name == endpointName);          
        }        
      
        public string DeleteEndpoint(string scopeName, string path)
        {
          string endpointName, newpath;
          getLastName(path, out newpath, out endpointName);
          Directories scopes = GetScopes();
          Folder folder = findFolder(scopes, newpath);
          Endpoint endpoint = folder.endpoints.FirstOrDefault<Endpoint>(o => o.Name == endpointName);
          folder.endpoints.Remove(endpoint);
          return PostScopes(scopes);
        }

        public string Endpoint(string scopeName, string newEndpointName, string description, string assembly, string path, string state)
        {
          Directories scopes = GetScopes();
          string name;
          if (state == "update")
          {
            string oldEndpointName, newpath;
            getLastName(path, out newpath, out oldEndpointName);            
            Endpoint endpoint = GetEndpoint(scopes, newpath, oldEndpointName);
            endpoint.Name = newEndpointName;
            endpoint.Description = description;
            name = oldEndpointName;            
          }
          else
          {
            Folders folders = getFolders(scopes, path);
            Folder folder = folders.FirstOrDefault<Folder>(o => o.Name == scopeName);
            Endpoint endpoint = new Endpoint()
            {
              Name = newEndpointName,
              Description = description
            };

            if (folder.endpoints == null)
              folder.endpoints = new Endpoints();

            folder.endpoints.Add(endpoint);
            name = newEndpointName;            
          }

          //First Step: Save Scopes
          string result = PostScopes(scopes);

          //Second Step: UpdateBinding
          UpdateBinding(scopeName, name, assembly);

          return result;
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

        public DatabaseDictionary GetDBDictionary(string scope, string application)
        {
          WebHttpClient client = new WebHttpClient(_settings["NHibernateServiceURI"]);
          DatabaseDictionary dbDictionary = client.Get<DatabaseDictionary>(String.Format("/{0}/{1}/dictionary", scope, application));

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
          request.Add("dbProvider",dbProvider);
          request.Add("dbServer",dbServer);
          request.Add("portNumber",portNumber);
          request.Add("dbInstance",dbInstance);
          request.Add("dbName",dbName);
          request.Add("dbSchema",dbSchema);
          request.Add("dbUserName",dbUserName);
          request.Add("dbPassword",dbPassword);
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
							iconCls = "object",
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
									iconCls = "key",
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
									iconCls = "property",
                  leaf = true,
                  hidden = true,
                  properties = properties
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
        #endregion
    }
}