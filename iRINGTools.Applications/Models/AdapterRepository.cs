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
using System.Collections;
using System.Net;


namespace iRINGTools.Web.Models
{
  public class AdapterRepository : IAdapterRepository
  {
    private NameValueCollection _settings = null;
    private WebHttpClient _adapterServiceClient = null;
    private WebHttpClient _hibernateServiceClient = null;
    private WebHttpClient _referenceDataServiceClient = null;
    private WebHttpClient _javaServiceClient = null;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));
    private static Dictionary<string, NodeIconCls> nodeIconClsMap;
    private string combinationMsg = null;

    [Inject]
    public AdapterRepository()
    {
      NameValueCollection settings = ConfigurationManager.AppSettings;

      ServiceSettings _settings = new ServiceSettings();
      _settings.AppendSettings(settings);

      #region initialize webHttpClient for converting old mapping
      string proxyHost = _settings["ProxyHost"];
      string proxyPort = _settings["ProxyPort"];
      string adapterServiceUri = _settings["AdapterServiceUri"];
      string javaCoreUri = _settings["JavaCoreUri"];
      string hibernateServiceUri = _settings["NHibernateServiceUri"];
      string referenceDataServiceUri = _settings["ReferenceDataServiceUri"];

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        WebProxy webProxy = new WebProxy(proxyHost, Int32.Parse(proxyPort));

        webProxy.Credentials = _settings.GetProxyCredential();

        _javaServiceClient = new WebHttpClient(javaCoreUri, null, webProxy);
        _adapterServiceClient = new WebHttpClient(adapterServiceUri, null, webProxy);
        _hibernateServiceClient = new WebHttpClient(hibernateServiceUri, null, webProxy);
        _referenceDataServiceClient = new WebHttpClient(referenceDataServiceUri, null, webProxy);
      }
      else
      {
        _javaServiceClient = new WebHttpClient(javaCoreUri);
        _adapterServiceClient = new WebHttpClient(adapterServiceUri);
        _hibernateServiceClient = new WebHttpClient(hibernateServiceUri);
        _referenceDataServiceClient = new WebHttpClient(referenceDataServiceUri);
      }
      #endregion
    }

    public Directories GetScopes()
    {
      _logger.Debug("In AdapterRepository GetScopes");
      Directories obj = null;
      string msg = null;

      try
      {
        msg = _javaServiceClient.GetMessage("/directory/session");        
        obj = _javaServiceClient.Get<Directories>("/directory", true);
        _logger.Debug("Successfully called Adapter.");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
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
      catch (Exception)
      {
        return "folder";
      }
    }

    public Tree GetDirectoryTree(string user)
    {
      _logger.Debug("In ScopesNode case block");
      Directories directory = null;

      string _key = user + "." + "directory";
      directory = GetScopes();
      HttpContext.Current.Session[_key] = directory;      

      Tree tree = null;
      string context = "";
      string treePath = "";

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
          folderNode.iconCls = GetNodeIconCls(folder.Type);
          folderNode.type = "folder";
          treePath = folder.Name;

          if (folder.Context != null)
            context = folder.Context;

          Object record = new
          {
            Name = folder.Name,
            context = context,
            Description = folder.Description,
            securityRole = folder.SecurityRole
          };

          folderNode.record = record;
          folderNode.property = new Dictionary<string, string>();
          folderNode.property.Add("Name", folder.Name);
          folderNode.property.Add("Description", folder.Description);
          folderNode.property.Add("Context", folder.Context);
          folderNodes.Add(folderNode);
          TraverseDirectory(folderNode, folder, treePath);
        }
      }
     
      return tree;
    }

    public XElement GetBinding(string context, string endpoint)
    {
      XElement obj = null;

      try
      {
        obj = _adapterServiceClient.Get<XElement>(String.Format("/{0}/{1}/binding", context, endpoint), true);
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

    public string PostScopes(Directories scopes)
    {
      string obj = null;

      try
      {
        obj = _javaServiceClient.Post<Directories>("/directory", scopes, true);
        _logger.Debug("Successfully called Adapter.");
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
        obj = _adapterServiceClient.Get<DataLayers>("/datalayers");
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
        obj = _adapterServiceClient.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", contextName, endpoint), true);
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
        obj = _adapterServiceClient.Get<Mapping>(String.Format("/{0}/{1}/mapping", contextName, endpoint), true);
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

        obj = _adapterServiceClient.Post<XElement>(String.Format("/{0}/{1}/binding", scope, application), binding, true);

      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string GetRootSecurityRole()
    {
      string rootSecurityRole = "";

      try
      {
        rootSecurityRole = _javaServiceClient.GetMessage("/directory/security");
        _logger.Debug("Successfully called Adapter.");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return rootSecurityRole;
    }

    public string GetDirectoryBaseUrl()
    {
      return _javaServiceClient.GetBaseUri();
    }

    public BaseUrls GetEndpointBaseUrl()
    {
      BaseUrls baseUrls = new BaseUrls();
      BaseUrl baseUrl = new BaseUrl { Url = _adapterServiceClient.GetBaseUri() };      
      baseUrls.Add(baseUrl);      
      return baseUrls;
    }

    public string Folder(string newFolderName, string description, string path, string state, string context, string oldContext, string user)
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
        if (state != "new")        
          CheckCombination(path, context, oldContext, user);       

        obj = _javaServiceClient.PostMessage(string.Format("/directory/folder/{0}/{1}/{2}/{3}", path, newFolderName, "folder", context), description, true);
        _logger.Debug("Successfully called Adapter.");

        ClearDirSession(user);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        obj = "ERROR";
      }      

      return obj;
    }

    public string Endpoint(string newEndpointName, string path, string description, string state, string context, string assembly, string baseUrl, string oldBaseUrl, string user)
    {
      string obj = null;
      string endpointName = null;

      if (state == "new")
      {
        path = path + '/' + newEndpointName;
        endpointName = newEndpointName;
      }
      else
      {
        endpointName = path.Substring(path.LastIndexOf('/') + 1);
      }

      path = path.Replace('/', '.');
      baseUrl = baseUrl.Replace('/', '.');      

      try
      {
        CheckeCombination(baseUrl, oldBaseUrl, context, context, newEndpointName, endpointName, path);
        obj = _javaServiceClient.PostMessage(string.Format("/directory/endpoint/{0}/{1}/{2}/{3}", path, newEndpointName, "endpoint", baseUrl), description, true);
        _logger.Debug("Successfully called Adapter.");
        ClearDirSession(user);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message.ToString());
        obj = "ERROR";
      }     

      UpdateBinding(context, endpointName, assembly);
      return obj;
    }

    public string DeleteEntry(string path, string user)
    {
      string obj = null;
      path = path.Replace('/', '.');
      try
      {
        obj = _javaServiceClient.Post<String>(String.Format("/directory/{0}", path), "", true);
        _logger.Debug("Successfully called Adapter.");
        ClearDirSession(user);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message.ToString());
      }

      return obj;
    }

    public string GetCombinationMsg()
    {
      return combinationMsg;
    }

    #region Private methods for Directory 
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
    
    private void ClearDirSession(string user)
    {
      if (HttpContext.Current.Session[user + "." + "tree"] != null)
        HttpContext.Current.Session[user + "." + "tree"] = null;

      if (HttpContext.Current.Session[user + "." + "directory"] != null)
        HttpContext.Current.Session[user + "." + "directory"] = null;
    }

    private void CheckeCombination(string baseUrl, string oldBaseUrl, string context, string oldContext, string endpointName, string oldEndpointName, string path)
    {
      string newSessionKey = baseUrl + "." + context + "." + endpointName;
      string oldSessionKey = oldBaseUrl + "." + oldContext + "." + oldEndpointName;
      string getPath = "";

      if (!newSessionKey.Equals(oldSessionKey))
      {
        if (HttpContext.Current.Session[oldSessionKey] != null)
        {
          HttpContext.Current.Session[oldSessionKey] = null;
        }
      }
        
      if (HttpContext.Current.Session[newSessionKey] != null)
      {
        getPath = HttpContext.Current.Session[newSessionKey].ToString();

        if (getPath == "")
          HttpContext.Current.Session[newSessionKey] = path;
        else if (!getPath.Equals(path))
        {
          combinationMsg = "The combination of (" + baseUrl.Replace(".", "/") + ", " + context + ", " + endpointName + ") at " + path.Replace(".", "/") + " is allready existed at " + getPath.Replace(".", "/") + ".";
          _logger.Error("Duplicated combination of baseUrl, context, and endpoint name");
          throw new Exception("Duplicated combination of baseUrl, context, and endpoint name");
        }
      }
      else
      {
        HttpContext.Current.Session[newSessionKey] = path;
      } 
    }

    private void CheckCombination(Folder folder, string path, string context, string oldContext)
    {
      Endpoints endpoints = folder.Endpoints;
      string endpointPath = "";

      if (endpoints != null)
      {
        foreach (Endpoint endpoint in endpoints)
        {
          endpointPath = path + "." + endpoint.Name;
          CheckeCombination(endpoint.BaseUrl, endpoint.BaseUrl, context, oldContext, endpoint.Name, endpoint.Name, endpointPath);
        }
      }

      Folders subFolders = folder.Folders;

      if (subFolders == null)
        return;
      else
      {
        foreach (Folder subFolder in subFolders)
        {
          path = path + "." + subFolder.Name;
          CheckCombination(subFolder, path, context, oldContext);
        }
      }
    }

    private void CheckCombination(string path, string context, string oldContext, string user)
    {
      string _key = user + "." + "directory";
      if (HttpContext.Current.Session[_key] != null)
      {
        Directories directory = (Directories)HttpContext.Current.Session[_key];
        Folder folder = FindFolder(directory, path);
        CheckCombination(folder, path, context, oldContext);
      }
    }

    private void GetLastName(string path, out string newpath, out string name)
    {
      int dotPos = path.LastIndexOf('.');

      if (dotPos < 0)
      {
        newpath = "";
        name = path;
      }
      else
      {
        newpath = path.Substring(0, dotPos);
        name = path.Substring(dotPos + 1, path.Length - dotPos - 1);
      }
    }

    private Folder FindFolder(List<Folder> scopes, string path)
    {
      string folderName, newpath;
      GetLastName(path, out newpath, out folderName);

      if (newpath == "")
      {
        foreach (Folder folder in scopes)
        {
          if (folder.Name == folderName)
            return folder;
        }
      }
      else
      {
        Folders folders = GetFolders(scopes, newpath);
        return folders.FirstOrDefault<Folder>(o => o.Name == folderName);
      }
      return null;
    }

    private Folders GetFolders(List<Folder> scopes, string path)
    {
      if (path == "")
        return (Folders)scopes;

      string[] level = path.Split('.');

      foreach (Folder folder in scopes)
      {
        if (folder.Name.Equals(level[0]))
        {
          if (level.Length == 1)
            return folder.Folders;
          else
            return TraverseGetFolders(folder, level, 0);
        }
      }
      return null;
    }

    private Folders TraverseGetFolders(Folder folder, string[] level, int depth)
    {
      if (folder.Folders == null)
      {
        folder.Folders = new Folders();
        return folder.Folders;
      }
      else
      {
        if (level.Length > depth + 1)
        {
          foreach (Folder subFolder in folder.Folders)
          {
            if (subFolder.Name == level[depth + 1])
              return TraverseGetFolders(subFolder, level, depth + 1);
          }
        }
        else
        {
          return folder.Folders;
        }
      }
      return null;
    }

    private void TraverseDirectory(TreeNode folderNode, Folder folder, string treePath)
    {
      List<JsonTreeNode> folderNodeList = folderNode.getChildren();
      Endpoints endpoints = folder.Endpoints;
      string context = "";
      string endpointName;
      string folderName;
      string baseUrl = "";
      string assembly = "";
      string dataLayerName = "";
      string sessionKey = "";
      string folderPath = treePath;

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
          treePath = folderPath + "." + endpoint.Name;

          if (endpoint.Context != null)
            context = endpoint.Context;

          if (endpoint.BaseUrl != null)
            baseUrl = endpoint.BaseUrl;

          sessionKey = baseUrl + "." + context + "." + endpointName;
          HttpContext.Current.Session[sessionKey] = treePath;

          baseUrl = baseUrl.Replace('.', '/');

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
            BaseUrl = baseUrl,
            endpoint = endpointName,
            Assembly = assembly,
            securityRole = endpoint.SecurityRole
          };

          endPointNode.record = record;
          endPointNode.property = new Dictionary<string, string>();
          endPointNode.property.Add("Name", endpointName);
          endPointNode.property.Add("Description", endpoint.Description);
          endPointNode.property.Add("Context", context);
          endPointNode.property.Add("Data Layer", dataLayerName);
        }
      }

      if (folder.Folders == null)
        return;
      else
      {
        foreach (Folder subFolder in folder.Folders)
        {
          folderName = subFolder.Name;
          TreeNode subFolderNode = new TreeNode();
          subFolderNode.text = folderName;
          subFolderNode.iconCls = GetNodeIconCls(subFolder.Type);
          subFolderNode.type = "folder";
          subFolderNode.hidden = false;
          subFolderNode.leaf = false;
          subFolderNode.id = folderNode.id + "/" + subFolder.Name;
          subFolderNode.identifier = subFolderNode.id;

          if (subFolder.Context != null)
            context = subFolder.Context;

          Object record = new
          {
            Name = folderName,
            context = context,
            Description = subFolder.Description,
            securityRole = subFolder.SecurityRole
          };
          subFolderNode.record = record;
          subFolderNode.property = new Dictionary<string, string>();
          subFolderNode.property.Add("Name", folderName);
          subFolderNode.property.Add("Description", subFolder.Description);
          subFolderNode.property.Add("Context", subFolder.Context);
          folderNodeList.Add(subFolderNode);
          treePath = folderPath + "." + folderName;
          TraverseDirectory(subFolderNode, subFolder, treePath);
        }
      }
    }
    #endregion

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
      return _adapterServiceClient.Get<Response>("/generate");
    }

    #endregion
  }
}