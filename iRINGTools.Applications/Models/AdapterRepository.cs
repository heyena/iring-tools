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
    private WebHttpClient _dataClient = null;
    private string _refDataServiceURI = string.Empty;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));
    private static Dictionary<string, NodeIconCls> nodeIconClsMap;

    [Inject]
    public AdapterRepository()
    {
      _settings = ConfigurationManager.AppSettings;
      _client = new WebHttpClient(_settings["AdapterServiceUri"]);
      _javaCoreClient = new WebHttpClient(_settings["JavaCoreUri"]);
      _dataClient = new WebHttpClient(_settings["DataServiceURI"]);
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

    public Directories GetScopes()
    {
      _logger.Debug("In AdapterRepository GetScopes");
      Directories obj = null;
      string msg = null;

      try
      {
        msg = _javaCoreClient.GetMessage("directory/session");

        if (_javaCoreClient.getBaseUri().Contains("dirxml"))
          obj = _javaCoreClient.Get<Directories>("directory", true);
        else
          obj = _javaCoreClient.Get<Directories>("directory", true);

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
          case NodeIconCls.project: return "treeProject";
          case NodeIconCls.application: return "application";
          case NodeIconCls.resource: return "resource";
          case NodeIconCls.key: return "treeKey";
          case NodeIconCls.property: return "treeProperty";
          case NodeIconCls.relation: return "treeRelation";
          default: return "folder";
        }
      }
      catch (Exception ex)
      {
        return "folder";
      }
    }

    public Tree GetDirectoryTree()
    {
      _logger.Debug("In ScopesNode case block");
      Directories directory = GetScopes();
      Tree tree = null;
      string context = "";

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

          if (folder.context != null)
            context = folder.context;

          Object record = new
          {
            Name = folder.Name,
            context = context,
            Description = folder.Description,
            securityRole = folder.securityRole
          };
          folderNode.record = record;
          folderNode.property = new Dictionary<string, string>();
          folderNode.property.Add("Name", folder.Name);
          folderNode.property.Add("Description", folder.Description);
          folderNode.property.Add("Context", folder.context);
          folderNodes.Add(folderNode);
          traverseDirectory(folderNode, folder);
        }
      }
      return tree;
    }

    public XElement GetBinding(string context, string endpoint)
    {
      XElement obj = null;

      try
      {
        obj = _client.Get<XElement>(String.Format("/{0}/{1}/binding", context, endpoint), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public DataLayer GetDataLayer(string context, string endpoint)
    {
      XElement binding = GetBinding(context, endpoint);
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
      string context = "";
      string endpointName;
      string assembly = "";
      string dataLayerName = "";

      if (endpoints != null)
      {
        foreach (Endpoint endpoint in endpoints)
        {
          LeafNode endPointNode = new LeafNode();
          endpointName = endpoint.Name;
          endPointNode.text = endpoint.Name;
          endPointNode.iconCls = "application";
          endPointNode.type = "ApplicationNode";
          endPointNode.setLeaf(false);
          endPointNode.hidden = false;
          endPointNode.id = folderNode.id + "/" + endpoint.Name;
          endPointNode.identifier = endPointNode.id;
          endPointNode.nodeType = "async";
          folderNodeList.Add(endPointNode);

          if (endpoint.context != null)
            context = endpoint.context;

          DataLayer dataLayer = GetDataLayer(context, endpointName);

          if (dataLayer != null)
          {
            assembly = dataLayer.Assembly;
            dataLayerName = dataLayer.Name;
          }

          Object record = new
          {
            Name = endpointName,
            Description = endpoint.Description,
            DataLayer = dataLayerName,
            context = context,
            endpoint = endpointName,
            Assembly = assembly,
            securityRole = endpoint.securityRole
          };

          endPointNode.record = record;
          endPointNode.property = new Dictionary<string, string>();
          endPointNode.property.Add("Name", endpointName);
          endPointNode.property.Add("Description", endpoint.Description);
          endPointNode.property.Add("Context", context);
          endPointNode.property.Add("Data Layer", dataLayerName);
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

          if (subFolder.context != null)
            context = subFolder.context;

          Object record = new
          {
            Name = subFolder.Name,
            context = context,
            Description = subFolder.Description,
            securityRole = subFolder.securityRole
          };
          subFolderNode.record = record;
          subFolderNode.property = new Dictionary<string, string>();
          subFolderNode.property.Add("Name", subFolder.Name);
          subFolderNode.property.Add("Description", subFolder.Description);
          subFolderNode.property.Add("Context", subFolder.context);
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
        obj = _javaCoreClient.Post<Directories>("directory", scopes, true);
        _logger.Debug("Successfully called Adapter.");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public DataLayers GetDataLayers(string contextName, string endpoint)
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

    public Mapping GetMapping(string contextName, string endpoint)
    {
      Mapping obj = null;

      try
      {
        obj = _client.Get<Mapping>(String.Format("/{0}/{1}/mapping", contextName, endpoint), true);
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

    public string getRootSecurityRole()
    {
      string rootSecurityRole = "";

      try
      {
        rootSecurityRole = _javaCoreClient.GetMessage("directory/security");
        _logger.Debug("Successfully called Adapter.");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return rootSecurityRole;
    }

    public string getBaseUrl()
    {
      return _javaCoreClient.getBaseUri();
    }

    public string Folder(string newFolderName, string description, string path, string state, string context)
    {
      string obj = null;

      if (state == "new")
      {
        if (path != "")
          path = path + '.' + newFolderName;
        else
          path = newFolderName;
      }

      path = path.Replace('/', '.');
      try
      {
        obj = _javaCoreClient.PostMessage(string.Format("directory/folder/{0}/{1}/{2}/{3}", path, newFolderName, "folder", context), description, true);
        _logger.Debug("Successfully called Adapter.");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string Endpoint(string newEndpointName, string path, string description, string state, string context, string assembly)
    {
      string obj = null;
      string endpointName = null;

      if (state == "new")
      {
        path = path + '.' + newEndpointName;
        endpointName = newEndpointName;
      }
      else
      {
        endpointName = path.Substring(path.LastIndexOf('/') + 1);
      }

      path = path.Replace('/', '.');

      try
      {
        obj = _javaCoreClient.PostMessage(string.Format("/endpoint/{0}/{1}/{2}", path, newEndpointName, "endpoint"), description, true);
        _logger.Debug("Successfully called Adapter.");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message.ToString());
      }

      UpdateBinding(context, endpointName, assembly);
      return obj;
    }

    public string DeleteEntry(string path)
    {
      string obj = null;
      path = path.Replace('/', '.');
      try
      {
        obj = _javaCoreClient.Post<String>(String.Format("directory/{0}", path), "", true);
        _logger.Debug("Successfully called Adapter.");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message.ToString());
      }

      return obj;
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
          property = new Dictionary<string, string>
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
              property = properties,
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
              property = properties
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
}