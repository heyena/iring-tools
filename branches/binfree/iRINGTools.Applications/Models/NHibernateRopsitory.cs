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
  public class NHibernateRopsitory  : INHibernateRe3pository
  {
    private WebHttpClient _adapterServiceClient = null;
    private WebHttpClient _hibernateServiceClient = null;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateRopsitory));
    private static Dictionary<string, NodeIconCls> nodeIconClsMap;
    private string proxyHost = "";
    private string proxyPort = "";
    private WebProxy webProxy = null;    
    private string adapterServiceUri = "";
    private string hibernateServiceUri = "";

    [Inject]
    public NHibernateRopsitory()
    {
      //_settings = ConfigurationManager.AppSettings;
      //_adapterServiceClient = new WebHttpClient(_settings["AdapterServiceUri"]);
      SetNodeIconClsMap();
      NameValueCollection settings = ConfigurationManager.AppSettings;
      ServiceSettings _settings = new ServiceSettings();
      _settings.AppendSettings(settings);

      #region initialize webHttpClient for converting old mapping
      proxyHost = _settings["ProxyHost"];
      proxyPort = _settings["ProxyPort"];      
      adapterServiceUri = _settings["AdapterServiceUri"];
      hibernateServiceUri = _settings["NHibernateServiceUri"];

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        webProxy = new WebProxy(proxyHost, Int32.Parse(proxyPort));
        webProxy.Credentials = _settings.GetProxyCredential();
        _adapterServiceClient = new WebHttpClient(adapterServiceUri, null, webProxy);
        _hibernateServiceClient = new WebHttpClient(hibernateServiceUri, null, webProxy);
      }
      else
      {
        _adapterServiceClient = new WebHttpClient(adapterServiceUri);
        _hibernateServiceClient = new WebHttpClient(hibernateServiceUri);
      }
      #endregion
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

    public Response RegenAll(string user)
    {
      Response totalObj = new Response();
      string _key = user + "." + "directory";
      Directories directory = null;
      if (HttpContext.Current.Session[_key] != null)
        directory = (Directories)HttpContext.Current.Session[_key];

      foreach (Folder folder in directory)
      {
        GenerateFolders(folder, totalObj);
      }
      return totalObj;
    }

    public DataDictionary GetDictionary(string contextName, string endpoint, string baseUrl)
    {
      DataDictionary obj = null;

      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "hibernate");
        obj = _newServiceClient.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", contextName, endpoint), true);
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
    public DataProviders GetDBProviders(string baseUrl)
    {
      WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "hibernate");
      return _newServiceClient.Get<DataProviders>("/providers");
    }

    public string SaveDBDictionary(string scope, string application, string tree, string baseUrl)
    {
      WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "hibernate");
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
        postResult = _newServiceClient.Post<DatabaseDictionary>("/" + scope + "/" + application + "/dictionary", dbDictionary, true);
      }
      catch (Exception ex)
      {
        _logger.Error("Error posting DatabaseDictionary." + ex);
      }
      return postResult;
    }

    public DatabaseDictionary GetDBDictionary(string context, string application, string baseUrl)
    {
      WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "hibernate");
      DatabaseDictionary dbDictionary = _newServiceClient.Get<DatabaseDictionary>(String.Format("/{0}/{1}/dictionary", context, application));

      string connStr = dbDictionary.ConnectionString;
      if (!String.IsNullOrEmpty(connStr))
      {
        dbDictionary.ConnectionString = Utility.EncodeTo64(connStr);
      }

      return dbDictionary;
    }

    public List<string> GetTableNames(string scope, string application, string dbProvider, string dbServer,
      string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string portNumber, string serName, string baseUrl)
    {
      WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "hibernate");
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

      return _newServiceClient.Post<Request, List<string>>(uri, request, true);
    }

    // use appropriate icons especially node with children
    public List<JsonTreeNode> GetDBObjects(string scope, string application, string dbProvider, string dbServer,
      string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string tableNames, string portNumber, string serName, string baseUrl)
    {
      List<JsonTreeNode> dbObjectNodes = new List<JsonTreeNode>();

      WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "hibernate");
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

      List<DataObject> dataObjects = _newServiceClient.Post<Request, List<DataObject>>(uri, request, true);

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

    private WebHttpClient PrepareServiceClient(string baseUrl, string serviceName)
    {
      if (baseUrl == "" || baseUrl == null)
        return _hibernateServiceClient;
      
      string baseUri = CleanBaseUrl(baseUrl.ToLower(), '/');
      string adapterBaseUri = CleanBaseUrl(hibernateServiceUri.ToLower(), '/');

      if (!baseUri.Equals(adapterBaseUri))
        return GetServiceClient(baseUrl, serviceName);
      else
        return _hibernateServiceClient;
    }

    private WebHttpClient GetServiceClient(string uri, string serviceName)
    {
      WebHttpClient _newServiceClient = null;
      string serviceUri = uri + "/" + serviceName;

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        _newServiceClient = new WebHttpClient(serviceUri, null, webProxy);
      }
      else
      {
        _newServiceClient = new WebHttpClient(serviceUri);
      }
      return _newServiceClient;
    }

    private string CleanBaseUrl(string url, char con)
    {
      System.Uri uri = new System.Uri(url);
      return uri.Scheme + ":" + con + con + uri.Host + ":" + uri.Port;
    }

    private Response GenerateFolders(Folder folder, Response totalObj)
    {
      Response obj = null;
      Endpoints endpoints = folder.Endpoints;

      if (endpoints != null)
      {
        foreach (Endpoint endpoint in endpoints)
        {
          WebHttpClient _newServiceClient = PrepareServiceClient(endpoint.BaseUrl, "adapter");
          obj = _newServiceClient.Get<Response>(String.Format("/{0}/{1}/generate", endpoint.Context, endpoint.Name));
          totalObj.Append(obj);
        }
      }

      Folders subFolders = folder.Folders;

      if (subFolders == null)
        return totalObj;
      else
      {
        foreach (Folder subFolder in subFolders)
        {
          obj = GenerateFolders(subFolder, totalObj);
        }
      }

      return totalObj;
    }

    #endregion
  }

  public interface INHibernateRe3pository
  {
    DataDictionary GetDictionary(string contextName, string endpoint, string baseUrl);
  }

}

