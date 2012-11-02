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
using System.Net;
using System.IO;


namespace iRINGTools.Web.Models
{
  public class AdapterRepository : IAdapterRepository
  {
    //private NameValueCollection _settings = null;
    private WebHttpClient _adapterServiceClient = null;
    private WebHttpClient _hibernateServiceClient = null;
    private WebHttpClient _referenceDataServiceClient = null;
    //private string _referenceDataServiceURI = string.Empty;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));

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
      string hibernateServiceUri = _settings["NHibernateServiceUri"];
      string referenceDataServiceUri = _settings["ReferenceDataServiceUri"];

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        //WebProxy webProxy = new WebProxy(proxyHost, Int32.Parse(proxyPort));
        WebProxy webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;

        _adapterServiceClient = new WebHttpClient(adapterServiceUri, null, webProxy);
        _hibernateServiceClient = new WebHttpClient(hibernateServiceUri, null, webProxy);
        _referenceDataServiceClient = new WebHttpClient(referenceDataServiceUri, null, webProxy);
      }
      else
      {
        _adapterServiceClient = new WebHttpClient(adapterServiceUri);
        _hibernateServiceClient = new WebHttpClient(hibernateServiceUri);
        _referenceDataServiceClient = new WebHttpClient(referenceDataServiceUri);
      }
      #endregion
    }

    public ScopeProjects GetScopes()
    {
      _logger.Debug("In AdapterRepository GetScopes");

      ScopeProjects obj = null;

      try
      {
        obj = _adapterServiceClient.Get<ScopeProjects>("/scopes");

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
        entity = _referenceDataServiceClient.Get<Entity>(String.Format("/classes/{0}/label", classId), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }
      return entity;
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
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public DataDictionary GetDictionary(string scopeName, string applicationName)
    {
      DataDictionary obj = null;

      try
      {
        obj = _adapterServiceClient.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", scopeName, applicationName), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public Mapping GetMapping(string scopeName, string applicationName)
    {
      Mapping obj = null;

      try
      {
        obj = _adapterServiceClient.Get<Mapping>(String.Format("/{0}/{1}/mapping", scopeName, applicationName), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public XElement GetBinding(string scope, string application)
    {
      XElement obj = null;

      try
      {
        obj = _adapterServiceClient.Get<XElement>(String.Format("/{0}/{1}/binding", scope, application), true);
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

    public string AddScope(string name, string description)
    {
      string obj = null;

      try
      {
        ScopeProject scope = new ScopeProject()
        {
          Name = name,
          Description = description
        };

        obj = _adapterServiceClient.Post<ScopeProject>("/scopes", scope, true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string UpdateScope(string name, string newName, string newDescription)
    {
      string obj = null;

      try
      {
        ScopeProject scope = new ScopeProject()
        {
          Name = newName,
          Description = newDescription
        };

        string uri = string.Format("/scopes/{0}", name);
        obj = _adapterServiceClient.Post<ScopeProject>(uri, scope, true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string DeleteScope(string name)
    {
      string obj = null;

      try
      {
        string uri = String.Format("/scopes/{0}/delete", name);
        obj = _adapterServiceClient.Get<string>(uri, true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string AddApplication(string scopeName, ScopeApplication application)
    {
      string obj = null;

      try
      {
        string uri = String.Format("/scopes/{0}/apps", scopeName);
        obj = _adapterServiceClient.Post<ScopeApplication>(uri, application, true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string UpdateApplication(string scopeName, string applicationName, ScopeApplication application)
    {
      string obj = null;

      try
      {
        string uri = String.Format("/scopes/{0}/apps/{1}", scopeName, applicationName);
        obj = _adapterServiceClient.Post<ScopeApplication>(uri, application, true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string DeleteApplication(string scopeName, string applicationName)
    {
      string obj = null;

      try
      {
        string uri = String.Format("/scopes/{0}/apps/{1}/delete", scopeName, applicationName);
        obj = _adapterServiceClient.Get<string>(uri, true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public Response Refresh(string scope, string application)
    {      
      return _adapterServiceClient.Get<Response>(String.Format("/{0}/{1}/refresh", scope, application));
    }

    public Response Refresh(string scope, string application, string dataObjectName)
    {
      return _adapterServiceClient.Get<Response>(String.Format("/{0}/{1}/{2}/refresh", scope, application, dataObjectName));
    }
    
    public Response UpdateDataLayer(MemoryStream dataLayerStream)
    {
      try
      {
        return Utility.Deserialize<Response>(_adapterServiceClient.PostStream("/datalayers", dataLayerStream), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message);

        Response response = new Response()
        {
          Level = StatusLevel.Error,
          Messages = new Messages { ex.Message }
        };

        return response;
      }
    }

    #region NHibernate Configuration Wizard support methods
    public DataProviders GetDBProviders()
    {
      return _hibernateServiceClient.Get<DataProviders>("/providers");
    }

    public string SaveDBDictionary(string scope, string application, string tree)
    {
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
        postResult = _hibernateServiceClient.Post<DatabaseDictionary>("/" + scope + "/" + application + "/dictionary", dbDictionary, true);
      }
      catch (Exception ex)
      {
        _logger.Error("Error posting DatabaseDictionary." + ex);
      }
      return postResult;
    }

    public DatabaseDictionary GetDBDictionary(string scope, string application)
    {
      DatabaseDictionary dbDictionary = _hibernateServiceClient.Get<DatabaseDictionary>(String.Format("/{0}/{1}/dictionary", scope, application));

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

      return _hibernateServiceClient.Post<Request, List<string>>(uri, request, true);
    }

    // use appropriate icons especially node with children
    public List<JsonTreeNode> GetDBObjects(string scope, string application, string dbProvider, string dbServer,
      string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string tableNames, string portNumber, string serName)
    {
      List<JsonTreeNode> dbObjectNodes = new List<JsonTreeNode>();
      bool hasDBDictionary = false;
      bool hasDataObjectinDBDictionary = false;
      DatabaseDictionary dbDictionary = null;
      string uri = String.Format("/{0}/{1}/objects", scope, application);

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

      List<DataObject> dataObjects = _hibernateServiceClient.Post<Request, List<DataObject>>(uri, request, true);

      try
      {
        dbDictionary = GetDBDictionary(scope, application);

        if (dbDictionary != null)
          if (dbDictionary.dataObjects.Count > 0)
            hasDBDictionary = true;
      }
      catch (Exception)
      {
        hasDBDictionary = false;
      }

      foreach (DataObject dataObject in dataObjects)
      {
        hasDataObjectinDBDictionary = false;

        if (hasDBDictionary)
          if (dbDictionary.dataObjects.FirstOrDefault<DataObject>(o => o.tableName.ToLower() == dataObject.tableName.ToLower()) != null)
            hasDataObjectinDBDictionary = true;

        JsonTreeNode keyPropertiesNode = new JsonTreeNode()
        {
          text = "Keys",
          type = "keys",
          expanded = true,
          iconCls = "folder",
          leaf = false,
          children = new List<JsonTreeNode>()
        };

        JsonTreeNode dataPropertiesNode = new JsonTreeNode()
        {
          text = "Properties",
          type = "properties",
          expanded = true,
          iconCls = "folder",
          leaf = false,
          children = new List<JsonTreeNode>()
        };

        JsonTreeNode relationshipsNode = new JsonTreeNode()
        {
          text = "Relationships",
          type = "relationships",
          expanded = true,
          iconCls = "folder",
          leaf = false,
          children = new List<JsonTreeNode>()
        };

        // create data object node
        JsonTreeNode dataObjectNode = new JsonTreeNode()
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
                {"isHidden", dataProperty.isHidden.ToString()},
              };

          if (dataObject.isKeyProperty(dataProperty.propertyName) && !hasDataObjectinDBDictionary)
          {
            properties.Add("keyType", dataProperty.keyType.ToString());

            JsonTreeNode keyPropertyNode = new JsonTreeNode()
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
            JsonTreeNode dataPropertyNode = new JsonTreeNode()
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

    public Response RegenAll()
    {
      return _adapterServiceClient.Get<Response>("/generate");
    }
    #endregion
  }
}