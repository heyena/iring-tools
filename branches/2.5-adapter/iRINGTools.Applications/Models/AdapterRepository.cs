using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Threading;


namespace iRINGTools.Web.Models
{
  public class AdapterRepository : IAdapterRepository
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));

    protected ServiceSettings _settings;
    protected string _proxyHost;
    protected string _proxyPort;
    protected string _adapterServiceUri = null;
    protected string _dataServiceUri = null;
    protected string _hibernateServiceUri = null;
    protected string _referenceDataServiceUri = null;

    public IDictionary<string, string> AuthHeaders { get; set; }

    public AdapterRepository()
    {
      NameValueCollection settings = ConfigurationManager.AppSettings;

      _settings = new ServiceSettings();
      _settings.AppendSettings(settings);

      _proxyHost = _settings["ProxyHost"];
      _proxyPort = _settings["ProxyPort"];

      _adapterServiceUri = _settings["AdapterServiceUri"];
      if (_adapterServiceUri.EndsWith("/"))
        _adapterServiceUri = _adapterServiceUri.Remove(_adapterServiceUri.Length - 1);

      _dataServiceUri = _settings["DataServiceUri"];
      if (_dataServiceUri.EndsWith("/"))
        _dataServiceUri = _dataServiceUri.Remove(_dataServiceUri.Length - 1);

      _hibernateServiceUri = _settings["HibernateServiceUri"];
      if (_hibernateServiceUri.EndsWith("/"))
        _hibernateServiceUri = _hibernateServiceUri.Remove(_hibernateServiceUri.Length - 1);

      _referenceDataServiceUri = _settings["RefDataServiceUri"];
      if (_referenceDataServiceUri.EndsWith("/"))
        _referenceDataServiceUri = _referenceDataServiceUri.Remove(_referenceDataServiceUri.Length - 1);
    }

    public HttpSessionStateBase Session { get; set; }

    protected WebHttpClient CreateWebClient(string baseUri)
    {
      WebHttpClient client = null;

      if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
      {
        WebProxy webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
        client = new WebHttpClient(baseUri, null, webProxy);
      }
      else
      {
        client = new WebHttpClient(baseUri);
      }

      if (AuthHeaders != null && AuthHeaders.Count > 0)
      {
        _logger.Debug("Injecting authorization [" + AuthHeaders.Count + "] headers.");
        client.Headers = AuthHeaders;
      }
      else
      {
        _logger.Debug("No authorization headers.");
      }

      return client;
    }

    protected T WaitForRequestCompletion<T>(string baseUri, string statusUrl)
    {
      T obj;

      try
      {
        long timeoutCount = 0;

        long asyncTimeout = 1800;  // seconds
        if (_settings["AsyncTimeout"] != null)
        {
          long.TryParse(_settings["AsyncTimeout"], out asyncTimeout);
        }
        asyncTimeout *= 1000;  // convert to milliseconds

        int asyncPollingInterval = 2;  // seconds
        if (_settings["AsyncPollingInterval"] != null)
        {
          int.TryParse(_settings["AsyncPollingInterval"], out asyncPollingInterval);
        }
        asyncPollingInterval *= 1000;  // convert to milliseconds

        WebHttpClient client = CreateWebClient(baseUri);
        RequestStatus requestStatus = null;

        while (timeoutCount < asyncTimeout)
        {
          requestStatus = client.Get<RequestStatus>(statusUrl);

          if (requestStatus.State != State.InProgress)
            break;

          Thread.Sleep(asyncPollingInterval);
          timeoutCount += asyncPollingInterval;
        }

        if (requestStatus.State != State.Completed)
        {
          throw new Exception(requestStatus.Message);
        }

        if (typeof(T) == typeof(string))
        {
          obj = (T)Convert.ChangeType(requestStatus.ResponseText, typeof(T));
        }
        else
        {
          obj = Utility.Deserialize<T>(requestStatus.ResponseText, true);
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message);
        throw ex;
      }

      return obj;
    }

    public ScopeProjects GetScopes()
    {
      _logger.Debug("In AdapterRepository GetScopes");

      ScopeProjects obj = null;

      try
      {
        WebHttpClient client = CreateWebClient(_adapterServiceUri);
        obj = client.Get<ScopeProjects>("/scopes");

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
        WebHttpClient client = CreateWebClient(_adapterServiceUri);
        obj = client.Get<DataLayers>("/datalayers");
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
        WebHttpClient client = CreateWebClient(_referenceDataServiceUri);
        entity = client.Get<Entity>(String.Format("/classes/{0}/label", classId), true);
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
      DataDictionary dictionary = null;

      try
      {
        WebHttpClient client = CreateWebClient(_dataServiceUri);        
        string isAsync = _settings["Async"];

        if (isAsync != null && isAsync.ToLower() == "true")
        {
          client.Async = true;
          string statusUrl = client.Get<string>(String.Format("/{0}/{1}/dictionary?format=xml", applicationName, scopeName));

          if (string.IsNullOrEmpty(statusUrl))
          {
            throw new Exception("Asynchronous status URL not found.");
          }

          dictionary = WaitForRequestCompletion<DataDictionary>(_dataServiceUri, statusUrl);
        }
        else
        {
          dictionary = client.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary?format=xml", applicationName, scopeName), true);
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        throw ex;
      }

      return dictionary;
    }

    public Mapping GetMapping(string scopeName, string applicationName)
    {
      Mapping obj = null;

      try
      {
        WebHttpClient client = CreateWebClient(_adapterServiceUri);        
        obj = client.Get<Mapping>(String.Format("/{0}/{1}/mapping", scopeName, applicationName), true);
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
        WebHttpClient client = CreateWebClient(_adapterServiceUri);        
        obj = client.Get<XElement>(String.Format("/{0}/{1}/binding", scope, application), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        throw ex;
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

        WebHttpClient client = CreateWebClient(_adapterServiceUri);        
        obj = client.Post<XElement>(String.Format("/{0}/{1}/binding", scope, application), binding, true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public DataLayer GetDataLayer(string scopeName, string applicationName)
    {
      DataLayer dataLayer = null;
      XElement binding = GetBinding(scopeName, applicationName);

      if (binding != null)
      {
        _logger.Debug("Received data layer binding: " + binding.ToString());

        dataLayer = new DataLayer();
        dataLayer.Assembly = binding.Element("bind").Attribute("to").Value;
        dataLayer.Name = binding.Element("bind").Attribute("to").Value.Split(',')[1].Trim();
      }
      else
      {
        throw new Exception("Error getting data layer binding configuration for [" + scopeName + "." + applicationName + "]");
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

        WebHttpClient client = CreateWebClient(_adapterServiceUri);
        obj = client.Post<ScopeProject>("/scopes", scope, true);
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
        WebHttpClient client = CreateWebClient(_adapterServiceUri);
        obj = client.Post<ScopeProject>(uri, scope, true);
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
        WebHttpClient client = CreateWebClient(_adapterServiceUri);
        obj = client.Get<string>(uri, true);
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
        WebHttpClient client = CreateWebClient(_adapterServiceUri);
        obj = client.Post<ScopeApplication>(uri, application, true);
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
        WebHttpClient client = CreateWebClient(_adapterServiceUri);
        obj = client.Post<ScopeApplication>(uri, application, true);
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
        WebHttpClient client = CreateWebClient(_adapterServiceUri);
        obj = client.Get<string>(uri, true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public Response Refresh(string scope, string application)
    {
      Response response = null;

      try
      {
        WebHttpClient client = CreateWebClient(_adapterServiceUri);
        string isAsync = _settings["Async"];

        if (isAsync != null && isAsync.ToLower() == "true")
        {
          client.Async = true;
          string statusUrl = client.Get<string>(String.Format("/{0}/{1}/refresh?format=xml", scope, application));

          if (string.IsNullOrEmpty(statusUrl))
          {
            throw new Exception("Asynchronous status URL not found.");
          }

          response = WaitForRequestCompletion<Response>(_adapterServiceUri, statusUrl);
        }
        else
        {
          response = client.Get<Response>(String.Format("/{0}/{1}/refresh?format=xml", scope, application), true);
        }

        string dictKey = string.Format("Dictionary.{0}.{1}", scope, application);
        Session.Remove(dictKey);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message);

        response = new Response()
        {
          Level = StatusLevel.Error,
          Messages = new Messages { ex.Message }
        };
      }

      return response;
    }

    public Response Refresh(string scope, string application, string dataObjectName)
    {
      WebHttpClient client = CreateWebClient(_adapterServiceUri);
      Response response = client.Get<Response>(String.Format("/{0}/{1}/{2}/refresh", scope, application, dataObjectName));
      return response;
    }
    
    public Response UpdateDataLayer(MemoryStream dataLayerStream)
    {
      try
      {
        WebHttpClient client = CreateWebClient(_adapterServiceUri);
        Response response = Utility.Deserialize<Response>(client.PostStream("/datalayers", dataLayerStream), true);
        return response;
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
      WebHttpClient client = CreateWebClient(_hibernateServiceUri);
      return client.Get<DataProviders>("/providers");
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
        WebHttpClient client = CreateWebClient(_hibernateServiceUri);
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
      WebHttpClient client = CreateWebClient(_hibernateServiceUri);
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

      WebHttpClient client = CreateWebClient(_hibernateServiceUri);
      return client.Post<Request, List<string>>(uri, request, true);
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

      WebHttpClient client = CreateWebClient(_hibernateServiceUri);
      List<DataObject> dataObjects = client.Post<Request, List<DataObject>>(uri, request, true);

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
                {"keyDelimiter", dataObject.keyDelimeter},
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
              leaf = true,
              property = properties,
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
              properties = properties,
              property = properties,
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
      WebHttpClient client = CreateWebClient(_adapterServiceUri);
      Response response = client.Get<Response>("/generate");
      return response;
    }
    #endregion
  }
}