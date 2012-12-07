using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using Ninject;
using iRINGTools.Web.Models;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;
using System.Net;
using System.IO;

namespace org.iringtools.web.Models
{
  public class AdapterRepository : IAdapterRepository
  {
    private ServiceSettings _settings = null;
    private WebHttpClient _adapterServiceClient = null;
    private WebHttpClient _hibernateServiceClient = null;
    private WebHttpClient _referenceDataServiceClient = null;
    private WebHttpClient _javaServiceClient = null;
    private string _proxyHost = "";
    private string _proxyPort = "";
    private WebProxy _webProxy = null;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));
    private static Dictionary<string, NodeIconCls> _nodeIconClsMap;
    private string _combinationMsg = null;
    private string _adapterServiceUri = "";
    private string _hibernateServiceUri = "";
    private string _referenceDataServiceUri = "";

    [Inject]
    public AdapterRepository()
    {
      var settings = ConfigurationManager.AppSettings;
      _settings = new ServiceSettings();
      _settings.AppendSettings(settings);
      #region initialize webHttpClient for converting old mapping
      _proxyHost = _settings["ProxyHost"];
      _proxyPort = _settings["ProxyPort"];
      _adapterServiceUri = _settings["AdapterServiceUri"];
      var javaCoreUri = _settings["JavaCoreUri"];
      _hibernateServiceUri = _settings["NHibernateServiceUri"];
      _referenceDataServiceUri = _settings["ReferenceDataServiceUri"];
      SetNodeIconClsMap();

      if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
      {
        _webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
        _javaServiceClient = new WebHttpClient(javaCoreUri, null, _webProxy);
        _adapterServiceClient = new WebHttpClient(_adapterServiceUri, null, _webProxy);
        _hibernateServiceClient = new WebHttpClient(_hibernateServiceUri, null, _webProxy);
        _referenceDataServiceClient = new WebHttpClient(_referenceDataServiceUri, null, _webProxy);
      }
      else
      {
        _javaServiceClient = new WebHttpClient(javaCoreUri);
        _adapterServiceClient = new WebHttpClient(_adapterServiceUri);
        _hibernateServiceClient = new WebHttpClient(_hibernateServiceUri);
        _referenceDataServiceClient = new WebHttpClient(_referenceDataServiceUri);
      }
      #endregion
    }

    public WebHttpClient GetServiceClient(string uri, string serviceName)
    {
      GetSetting();
      WebHttpClient newServiceClient = null;
      var serviceUri = uri + "/" + serviceName;

      if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
      {
        newServiceClient = new WebHttpClient(serviceUri, null, _webProxy);
      }
      else
      {
        newServiceClient = new WebHttpClient(serviceUri);       
      }
      return newServiceClient;
    }

    public Resources GetResource(String user)
    {
      Resources resources = null;      

      try
      {
        resources = _javaServiceClient.Get<Resources>("/directory/resources", true);
        HttpContext.Current.Session[user + "." + "resources"] = resources; 
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return resources;
    }

    public Directories GetScopes()
    {
      _logger.Debug("In AdapterRepository GetScopes");
      Directories obj = null;     

      try
      {
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
        switch (_nodeIconClsMap[type.ToLower()])
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

        var key = user + "." + "directory";
      var resource = user + "." + "resource";
      directory = GetScopes();
      HttpContext.Current.Session[key] = directory; 
      GetResource(user);

      Tree tree = null;
      var context = "";
      var treePath = "";

      if (directory != null)
      {
        tree = new Tree();
        var folderNodes = tree.getNodes();

        foreach (var folder in directory)
        {
          var folderNode = new TreeNode {text = folder.Name, id = folder.Name};
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
          folderNode.property = new Dictionary<string, string>
              {
                  {"Name", folder.Name},
                  {"Description", folder.Description},
                  {"Context", folder.Context},
                  {"User", folder.User}
              };
            folderNodes.Add(folderNode);
          TraverseDirectory(folderNode, folder, treePath);
        }
      }
     
      return tree;
    }

    public XElement GetBinding(string context, string endpoint, string baseUrl)
    {
      XElement obj = null;

      try
      {
        var newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = newServiceClient.Get<XElement>(String.Format("/{0}/{1}/binding", context, endpoint), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string TestBaseUrl(string baseUrl)
    {
      string obj = null;

      try
      {
        var newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = newServiceClient.Get<string>("/test");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return "ERROR";
      }

      return obj;
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

    
    public DataLayers GetDataLayers(string baseUrl)
    {
      DataLayers obj = null;

      try
      {
        var newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = newServiceClient.Get<DataLayers>("/datalayers");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public Entity GetClassLabel(string classId)
    {
      var entity = new Entity();
      try
      {
        var tempClient = new WebHttpClient(_settings["ReferenceDataServiceUri"]);
        entity = tempClient.Get<Entity>(String.Format("/classes/{0}/label", classId), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }
      return entity;
    }

    public DataDictionary GetDictionary(string contextName, string endpoint, string baseUrl)
    {
      DataDictionary obj = null;

      try
      {
        var newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = newServiceClient.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", contextName, endpoint), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public Mapping GetMapping(string contextName, string endpoint, string baseUrl)
    {
      Mapping obj = null;

      try
      {
        var newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = newServiceClient.Get<Mapping>(String.Format("/{0}/{1}/mapping", contextName, endpoint), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string UpdateBinding(string scope, string application, string dataLayer, string baseUrl)
    {
      string obj = null;

      try
      {
        var binding = new XElement("module",
            new XAttribute("name", string.Format("{0}.{1}", scope, application)),
            new XElement("bind",
            new XAttribute("name", "DataLayer"),
            new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
            new XAttribute("to", dataLayer)
          )
        );

        var newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = newServiceClient.Post<XElement>(String.Format("/{0}/{1}/binding", scope, application), binding, true);

      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string GetRootSecurityRole()
    {
      var rootSecurityRole = "";

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

    public string GetUserLdap()
    {
      return _javaServiceClient.GetMessage("/directory/userldap");
    }

    public Urls GetEndpointBaseUrl(string user)
    {
      var ifExit = false;
      Urls baseUrls = null;

      if (HttpContext.Current.Session[user + ".baseUrlList"] != null)
        baseUrls = (Urls)HttpContext.Current.Session[user + ".baseUrlList"];
      else
      {
        try
        {
          baseUrls = _javaServiceClient.Get<Urls>("/directory/baseUrls", true);
          HttpContext.Current.Session[user + ".baseUrlList"] = baseUrls;
          _logger.Debug("Successfully called Adapter.");
        }
        catch (Exception ex)
        {
          _logger.Error(ex.ToString());
        }
      }        

      var baseUri = _adapterServiceClient.GetBaseUri();


      foreach (var baseUrl in baseUrls.Where(baseUrl => baseUrl.Urlocator.ToLower().Equals(baseUri.ToLower())))
      {
          ifExit = true;
      }

      if (!ifExit)
      {
          var newBaseUrl = new Url { Urlocator = baseUri };
          if (baseUrls != null) baseUrls.Add(newBaseUrl);
      }

        return baseUrls;
    }

    public ContextNames GetFolderContexts(string user)
    {
      ContextNames contextNames = null;
      if (HttpContext.Current.Session[user + ".contextList"] != null)
        contextNames = (ContextNames)HttpContext.Current.Session[user + ".contextList"];
      else
      {
        try
        {
          contextNames = _javaServiceClient.Get<ContextNames>("/directory/contextNames", true);
          HttpContext.Current.Session[user + ".contextList"] = contextNames;
          _logger.Debug("Successfully called Adapter.");
        }
        catch (Exception ex)
        {
          _logger.Error(ex.ToString());
        }
      }

      return contextNames;
    }

    public string Folder(string newFolderName, string description, string path, string state, string context, string oldContext, string user)
    {
      string obj = null;

      if (context == "")
        context = "0";

      if (state.Equals("new"))
      {
        if (path != "")
          path = path + '.' + newFolderName;
        else
          path = newFolderName;        
      }
     
      path = path.Replace('/', '.');       

      try
      {
        if (!state.Equals("new"))        
          CheckCombination(path, context, oldContext, user);

        var resources = (Resources)HttpContext.Current.Session[user + ".resources"];
        obj = _javaServiceClient.PostMessage(string.Format("/directory/folder/{0}/{1}/{2}/{3}", path, newFolderName, "folder", context), description, true);

        if (state != "new" && !context.Equals(oldContext))
        {
          var folder = PrepareFolder(user, path);

          if (folder != null)
            obj = UpdateFolders(folder, context, resources, oldContext);          
        }

        _logger.Debug("Successfully called Adapter and Java Directory Service.");    
        ClearDirSession(user);        
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        obj = "ERROR";
      }      

      return obj;
    }

    public string Endpoint(string newEndpointName, string path, string description, string state, string context, string oldAssembly, string newAssembly, string baseUrl, string oldBaseUrl, string key)
    {
      var obj = "";
        EndpointApplication application = null;
      string endpointName = null;
        var createApp = false;

      string baseUri = CleanBaseUrl(baseUrl, '.');
      if (state.Equals("new"))
      {
        path = path + '/' + newEndpointName;
        endpointName = newEndpointName;
        oldAssembly = newAssembly;
        oldBaseUrl = baseUrl;
      }
      else
      {
        endpointName = path.Substring(path.LastIndexOf('/') + 1);
      }

      path = path.Replace('/', '.');      

      try
      {
        var newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        CheckeCombination(baseUrl, oldBaseUrl, context, context, newEndpointName, endpointName, path, key);
        var resourcesOld = (Resources)HttpContext.Current.Session[key + ".resources"];
        obj = _javaServiceClient.PostMessage(string.Format("/directory/endpoint/{0}/{1}/{2}/{3}/{4}", path, newEndpointName, "endpoint", baseUri.Replace('/', '.'), newAssembly), description, true);
        var resourcesNew = GetResource(key);


          Locator scope = null;
          if (!state.Equals("new"))
        {
          if (newAssembly.ToLower() == oldAssembly.ToLower() && newEndpointName.ToLower() == endpointName.ToLower())
            return "";
          
          var resourceOld = FindResource(CleanBaseUrl(baseUrl, '/'), resourcesOld); 
            
          if (resourceOld != null)
          {
            scope = resourceOld.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());
            application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == endpointName.ToLower());
            if (application == null)
              createApp = true;
          }
          else
            createApp = true;

          if (createApp)
          {
            application = new EndpointApplication()
            {
              Endpoint = endpointName,
              Description = description,
              Assembly = oldAssembly
            };
          }
          
          obj = newServiceClient.Post<EndpointApplication>(String.Format("/scopes/{0}/apps/{1}", context, newEndpointName), application, true);
        }
        else if (state.Equals("new"))
        {
          var resourceNew = FindResource(CleanBaseUrl(baseUrl, '.'), resourcesNew);           

          if (resourceNew != null)
          {
            scope = resourceNew.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());
            application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == newEndpointName.ToLower());
          }
          else
          {
            application = new EndpointApplication()
            {
              Endpoint = newEndpointName,
              Description = description,
              Assembly = newAssembly
            };
          }

          obj = newServiceClient.Post<EndpointApplication>(String.Format("/scopes/{0}/apps", context), application, true);          
        }

        _logger.Debug("Successfully called Adapter and Java Directory Service.");
        ClearDirSession(key);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message.ToString());
        obj = "ERROR";
      }     

      return obj;
    }

    public string DeleteEntry(string path, string type, string context, string baseUrl, string user)
    {
      string obj = null;

        path = path.Replace('/', '.');

        try
      {
        var resources = (Resources)HttpContext.Current.Session[user + ".resources"];
        var name = path.Substring(path.LastIndexOf('.') + 1);                  

        if (type.Equals("endpoint"))
        {
          var resource = FindResource(CleanBaseUrl(baseUrl, '/'), resources);
          var scope = resource.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());
          var application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == name.ToLower());

          var newServiceClient = PrepareServiceClient(baseUrl, "adapter");
          obj = newServiceClient.Post<EndpointApplication>(String.Format("/scopes/{0}/delete", context), application, true);
        }
        else if (type.Equals("folder"))
        {
          var folder = PrepareFolder(user, path);

          if (folder != null)
            DeleteFolders(folder, context, resources);          
        }

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

    public Response RegenAll(string user)
    {
      var totalObj = new Response();
      var key = user + "." + "directory";
      Directories directory = null;
      if (HttpContext.Current.Session[key] != null)      
        directory = (Directories)HttpContext.Current.Session[key];

      foreach (var folder in directory)
      {
        GenerateFolders(folder, totalObj);
      }
      return totalObj;      
    }
    
    public string GetCombinationMsg()
    {
      return _combinationMsg;
    }

    public Response SaveDataLayer(MemoryStream dataLayerStream)
    {
      try
      {
        return Utility.Deserialize<Response>(_adapterServiceClient.PostStream("/datalayers", dataLayerStream), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message);

        var response = new Response()
        {
          Level = StatusLevel.Error,
          Messages = new Messages { ex.Message }
        };

        return response;
      }
    }
    
    #region Private methods for Directory 

    static MemoryStream CreateDataLayerStream(string name, string mainDLL, string path)
    {
      var dataLayer = new DataLayer()
      {
        Name = name,
        MainDLL = mainDLL,
        Package = Utility.Zip(path),
      };

      var dataLayerStream = new MemoryStream();
      var serializer = new DataContractSerializer(typeof(DataLayer));
      
      serializer.WriteObject(dataLayerStream, dataLayer);
      dataLayerStream.Position = 0;

      return dataLayerStream;
    }

    private WebHttpClient PrepareServiceClient(string baseUrl, string serviceName)
    {
      if (string.IsNullOrEmpty(baseUrl))
        return _adapterServiceClient;

      var baseUri = CleanBaseUrl(baseUrl.ToLower(), '/');
      var adapterBaseUri = CleanBaseUrl(_adapterServiceUri.ToLower(), '/');

      return !baseUri.Equals(adapterBaseUri) ? GetServiceClient(baseUrl, serviceName) : _adapterServiceClient;
    }

    private Response GenerateFolders(Folder folder, Response totalObj)
    {
      Response obj = null;
      var endpoints = folder.Endpoints;      

      if (endpoints != null)
      {
        foreach (Endpoint endpoint in endpoints)
        {
          var newServiceClient = PrepareServiceClient(endpoint.BaseUrl, "adapter");
          obj = newServiceClient.Get<Response>(String.Format("/{0}/{1}/generate", endpoint.Context, endpoint.Name));
          totalObj.Append(obj);          
        }
      }

      var subFolders = folder.Folders;

      if (subFolders == null)
        return totalObj;
      else
      {
        foreach (var subFolder in subFolders)
        {
          obj = GenerateFolders(subFolder, totalObj);
        }
      }

      return totalObj;
    }

    private Folder PrepareFolder(string user, string path)
    {
      var key = user + "." + "directory";
      if (HttpContext.Current.Session[key] != null)
      {
        var directory = (Directories)HttpContext.Current.Session[key];
        return FindFolder(directory, path);        
      }
      return null;
    }

    private void GetSetting()
    {
      if (_settings == null)
        _settings = new ServiceSettings();
      
      _proxyHost = _settings["ProxyHost"];
      _proxyPort = _settings["ProxyPort"];
    }

    private string UpdateFolders(Folder folder, string context, Resources resources, String oldContext)
    {
      string obj = null;
      var endpoints = folder.Endpoints;

        if (endpoints != null)
      {
        foreach (var endpoint in folder.Endpoints)
        {
          var resource = FindResource(endpoint.BaseUrl, resources);
          var scope = resource.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == oldContext.ToLower());

          var newServiceClient = PrepareServiceClient(endpoint.BaseUrl, "adapter");
          obj = newServiceClient.Post<Locator>(string.Format("/scopes/{0}", context), scope, true);
        }
      }

      var subFolders = folder.Folders;

      if (subFolders == null)
        return null;
      else
      {
        foreach (var subFolder in subFolders)
        {
          obj = UpdateFolders(subFolder, context, resources, oldContext);
        }
      }

      return obj;
    }

    private string DeleteFolders(Folder folder, string context, Resources resources)
    {
      string obj = null;
      var endpoints = folder.Endpoints;
        EndpointApplication application = null;

      Locator scope = null;

      if (endpoints != null)
      {
        foreach (Endpoint endpoint in endpoints)
        {
          var resource = FindResource(endpoint.BaseUrl, resources);
          scope = resource.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());
          application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == endpoint.Name.ToLower());

          var newServiceClient = PrepareServiceClient(endpoint.BaseUrl, "adapter");
          obj = newServiceClient.Post<EndpointApplication>(String.Format("/scopes/{0}/delete", context), application, true);
        }
      }

      var subFolders = folder.Folders;

      if (subFolders == null)
        return null;
      else
      {
        foreach (var subFolder in subFolders)
        {
          obj = DeleteFolders(subFolder, context, resources);
        }
      }

      return obj;
    }

    private static void SetNodeIconClsMap()
    {
      _nodeIconClsMap = new Dictionary<string, NodeIconCls>()
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
      if (HttpContext.Current.Session[user + "." + "directory"] != null)
        HttpContext.Current.Session[user + "." + "directory"] = null;

      if (HttpContext.Current.Session[user + "." + "resource"] != null)
        HttpContext.Current.Session[user + "." + "resource"] = null;
    }

    private static Resource FindResource(string baseUrl, IEnumerable<Resource> resources)
    {
        return resources.FirstOrDefault(rc => rc.BaseUrl.ToLower().Equals(baseUrl.ToLower()));
    }

      private static string CleanBaseUrl(string url, char con)
    {
      try
      {
        var uri = new System.Uri(url);
        return uri.Scheme + ":" + con + con + uri.Host + ":" + uri.Port;
      }
      catch(Exception){}
      return null;
    }

    private void CheckeCombination(string baseUrl, string oldBaseUrl, string context, string oldContext, string endpointName, string oldEndpointName, string path, string user)
    {
      var _resource = user + ".resources";
        var lpath = "";
      Locator scope = null;

        if (HttpContext.Current.Session[_resource] == null) return;
        var resources = (Resources)HttpContext.Current.Session[_resource];
        var resource = FindResource(CleanBaseUrl(oldBaseUrl, '/'), resources);

        if (resource != null)
            scope = resource.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());

        if (scope == null) return;
        var application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == endpointName.ToLower());

        if (application == null || application.Path.Replace("/", ".").Equals(path)) return;
        lpath = application.Path;
        _combinationMsg = "The combination of (" + baseUrl.Replace(".", "/") + ", " + context + ", " + endpointName + ") at " + path.Replace(".", "/") + " is allready existed at " + lpath + ".";
        _logger.Error("Duplicated combination of baseUrl, context, and endpoint name");
        throw new Exception("Duplicated combination of baseUrl, context, and endpoint name");
    }

    private void CheckCombination(Folder folder, string path, string context, string oldContext, string user)
    {
      var endpoints = folder.Endpoints;
      var endpointPath = "";
      var folderPath = "";

      if (endpoints != null)
      {
        foreach (var endpoint in endpoints)
        {
          endpointPath = path + "." + endpoint.Name;
          CheckeCombination(endpoint.BaseUrl, endpoint.BaseUrl, context, oldContext, endpoint.Name, endpoint.Name, endpointPath, user);
        }
      }

      var subFolders = folder.Folders;

      if (subFolders == null)
        return;
      else
      {
        foreach (var subFolder in subFolders)
        {
          folderPath = path + "." + subFolder.Name;
          CheckCombination(subFolder, folderPath, context, oldContext, user);
        }
      }
    }

    private void CheckCombination(string path, string context, string oldContext, string user)
    {
      var _key = user + "." + "directory";
        if (HttpContext.Current.Session[_key] == null) return;
        var directory = (Directories)HttpContext.Current.Session[_key];
        var folder = FindFolder(directory, path);
        CheckCombination(folder, path, context, oldContext, user);
    }

    private static void GetLastName(string path, out string newpath, out string name)
    {
      var dotPos = path.LastIndexOf('.');

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
          return scopes.FirstOrDefault(folder => folder.Name == folderName);
      }
      else
      {
        var folders = GetFolders(scopes, newpath);
        return folders.FirstOrDefault<Folder>(o => o.Name == folderName);
      }
      return null;
    }

    private IEnumerable<Folder> GetFolders(List<Folder> scopes, string path)
    {
      if (path == "")
        return (Folders)scopes;

      var level = path.Split('.');

      return scopes.Where(folder => folder.Name.Equals(level[0])).Select(folder => level.Length == 1 ? folder.Folders : TraverseGetFolders(folder, level, 0)).FirstOrDefault();
    }

    private static Folders TraverseGetFolders(Folder folder, string[] level, int depth)
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
            return (from subFolder in folder.Folders where subFolder.Name == level[depth + 1] select TraverseGetFolders(subFolder, level, depth + 1)).FirstOrDefault();
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
      var folderNodeList = folderNode.getChildren();
      var endpoints = folder.Endpoints;
      var context = "";
        var baseUrl = "";
      var assembly = "";
      var dataLayerName = "";
      var folderPath = treePath;

      if (endpoints != null)
      {
        foreach (var endpoint in endpoints)
        {
          var endPointNode = new LeafNode();
          var endpointName = endpoint.Name;
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
            baseUrl = endpoint.BaseUrl + "/adapter";

          #region Get Assambly information
          var bindings = GetBinding(context, endpointName, baseUrl);
          DataLayer dataLayer = null;
          if (bindings != null)
          {
              dataLayer = new DataLayer();
              dataLayer.Assembly = bindings.Element("bind").Attribute("to").Value;
              dataLayer.Name = bindings.Element("bind").Attribute("to").Value.Split(',')[1].Trim();
          }

          if (dataLayer != null)
          {
              assembly = dataLayer.Assembly;
              dataLayerName = dataLayer.Name;
          } 
          #endregion        
          
          Object record = new
          {
            Name = endpointName,
            Description = endpoint.Description,
            DataLayer = dataLayerName,
            context = context,
            BaseUrl = baseUrl,
            endpoint = endpointName,
            Assembly = assembly,
            securityRole = endpoint.SecurityRole,
            dbInfo = new Object(),
            dbDict = new Object()
          };

          endPointNode.record = record;
          endPointNode.property = new Dictionary<string, string>();
          endPointNode.property.Add("Name", endpointName);
          endPointNode.property.Add("Description", endpoint.Description);
          endPointNode.property.Add("Context", context);
          endPointNode.property.Add("Data Layer", dataLayerName);
          endPointNode.property.Add("User", endpoint.User);
        }
      }

      if (folder.Folders == null)
        return;
      else
      {
        foreach (var subFolder in folder.Folders)
        {
          var folderName = subFolder.Name;
          var subFolderNode = new TreeNode();
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
          subFolderNode.property = new Dictionary<string, string>
              {
                  {"Name", folderName},
                  {"Description", subFolder.Description},
                  {"Context", subFolder.Context},
                  {"User", subFolder.User}
              };
            folderNodeList.Add(subFolderNode);
          treePath = folderPath + "." + folderName;
          TraverseDirectory(subFolderNode, subFolder, treePath);
        }
      }
    }
    #endregion   
  }
}