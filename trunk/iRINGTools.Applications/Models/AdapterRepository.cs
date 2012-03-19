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
    private string proxyHost = "";
    private string proxyPort = "";
    private WebProxy webProxy = null;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));
    private static Dictionary<string, NodeIconCls> nodeIconClsMap;
    private string combinationMsg = null;
    private string adapterServiceUri = "";
    private string hibernateServiceUri = "";
    private string referenceDataServiceUri = "";

    [Inject]
    public AdapterRepository()
    {
      NameValueCollection settings = ConfigurationManager.AppSettings;

      ServiceSettings _settings = new ServiceSettings();
      _settings.AppendSettings(settings);

      #region initialize webHttpClient for converting old mapping
      proxyHost = _settings["ProxyHost"];
      proxyPort = _settings["ProxyPort"];
      adapterServiceUri = _settings["AdapterServiceUri"];
      string javaCoreUri = _settings["JavaCoreUri"];
      hibernateServiceUri = _settings["NHibernateServiceUri"];
      referenceDataServiceUri = _settings["ReferenceDataServiceUri"];
      SetNodeIconClsMap();

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        webProxy = new WebProxy(proxyHost, Int32.Parse(proxyPort));
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

    public WebHttpClient getServiceClinet(string uri, string serviceName)
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
      Resources resources = null;

      string _key = user + "." + "directory";
      string _resource = user + "." + "resource";
      directory = GetScopes();
      HttpContext.Current.Session[user + "." + "directory"] = directory; 
      resources = GetResource(user);

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

    public XElement GetBinding(string context, string endpoint, string baseUrl)
    {
      XElement obj = null;

      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = _newServiceClient.Get<XElement>(String.Format("/{0}/{1}/binding", context, endpoint), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public DataLayer GetDataLayer(string context, string endpoint, string baseUrl)
    {
      XElement binding = GetBinding(context, endpoint, baseUrl);
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

    public DataLayers GetDataLayers(string baseUrl)
    {
      DataLayers obj = null;

      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = _newServiceClient.Get<DataLayers>("/datalayers");
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

    public DataDictionary GetDictionary(string contextName, string endpoint, string baseUrl)
    {
      DataDictionary obj = null;

      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = _newServiceClient.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", contextName, endpoint), true);
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
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = _newServiceClient.Get<Mapping>(String.Format("/{0}/{1}/mapping", contextName, endpoint), true);
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
        XElement binding = new XElement("module",
            new XAttribute("name", string.Format("{0}.{1}", scope, application)),
            new XElement("bind",
            new XAttribute("name", "DataLayer"),
            new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
            new XAttribute("to", dataLayer)
          )
        );

        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = _newServiceClient.Post<XElement>(String.Format("/{0}/{1}/binding", scope, application), binding, true);

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

    public string GetUserLdap()
    {
      return _javaServiceClient.GetMessage("/directory/userldap");
    }

    public BaseUrls GetEndpointBaseUrl(string user)
    {
      Resources resources = GetResource(user);
      BaseUrls baseUrls = new BaseUrls();

      foreach (Resource resource in resources)
      {
        BaseUrl baseUrl = new BaseUrl { Url = resource.BaseUrl };
        baseUrls.Add(baseUrl);
      }
      return baseUrls;
    }

    public string Folder(string newFolderName, string description, string path, string state, string context, string oldContext, string user)
    {
      string obj = null;

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

        Resources resources = (Resources)HttpContext.Current.Session[user + ".resources"]; 
        obj = _javaServiceClient.PostMessage(string.Format("/directory/folder/{0}/{1}/{2}/{3}", path, newFolderName, "folder", context), description, true);

        if (state != "new" && !context.Equals(oldContext))
        {
          Folder folder = PrepareFolder(user, path);

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

    public string Endpoint(string newEndpointName, string path, string description, string state, string context, string oldAssembly, string newAssembly, string baseUrl, string oldBaseUrl, string user)
    {
      string obj = "";
      Locator scope = null;
      EndpointApplication application = null;
      string endpointName = null;
      Resource resourceOld = null;
      Resource resourceNew = null;

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
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        CheckeCombination(baseUrl, oldBaseUrl, context, context, newEndpointName, endpointName, path, user);
        Resources resourcesOld = (Resources)HttpContext.Current.Session[user + ".resources"];
        obj = _javaServiceClient.PostMessage(string.Format("/directory/endpoint/{0}/{1}/{2}/{3}/{4}", path, newEndpointName, "endpoint", baseUri.Replace('/', '.'), newAssembly), description, true);
        Resources resourcesNew = GetResource(user); 

        //&& (!newAssembly.Equals(oldAssembly) || !newEndpointName.Equals(endpointName))
        if (!state.Equals("new"))
        {
          resourceOld = FindResource(CleanBaseUrl(baseUrl, '/'), resourcesOld); 
            
          if (resourceOld != null)
          {
            scope = resourceOld.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());
            application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == endpointName.ToLower());
          }
          else
          {
            application = new EndpointApplication()
            {
              Endpoint = endpointName,
              Description = description,
              Assembly = oldAssembly
            };
          }
          
          obj = _newServiceClient.Post<EndpointApplication>(String.Format("/scopes/{0}/apps/{1}", context, newEndpointName), application, true);
        }
        else if (state.Equals("new"))
        {
          resourceNew = FindResource(CleanBaseUrl(baseUrl, '.'), resourcesNew);           

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

          obj = _newServiceClient.Post<EndpointApplication>(String.Format("/scopes/{0}/apps", context), application, true);          
        }

        _logger.Debug("Successfully called Adapter and Java Directory Service.");
        ClearDirSession(user);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message.ToString());
        obj = "ERROR";
      }     

      //UpdateBinding(context, endpointName, assembly);
      return obj;
    }

    public string DeleteEntry(string path, string type, string context, string baseUrl, string user)
    {
      string obj = null;     

      string name = null;
      path = path.Replace('/', '.');
      Locator scope = null;
      EndpointApplication application = null;

      try
      {
        Resources resources = (Resources)HttpContext.Current.Session[user + ".resources"];
        name = path.Substring(path.LastIndexOf('.') + 1);                  

        if (type.Equals("endpoint"))
        {
          Resource resource = FindResource(CleanBaseUrl(baseUrl, '/'), resources);
          scope = resource.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());
          application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == name.ToLower());

          WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
          obj = _newServiceClient.Post<EndpointApplication>(String.Format("/scopes/{0}/delete", context), application, true);
        }
        else if (type.Equals("folder"))
        {
          Folder folder = PrepareFolder(user, path);

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
    
    public string GetCombinationMsg()
    {
      return combinationMsg;
    }

    #region Private methods for Directory 

    private WebHttpClient PrepareServiceClient(string baseUrl, string serviceName)
    {
      if (!baseUrl.ToLower().Equals(CleanBaseUrl(adapterServiceUri.ToLower(), '/')))
        return getServiceClinet(baseUrl, serviceName);
      else
        return _adapterServiceClient;
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

    private Folder PrepareFolder(string user, string path)
    {
      string _key = user + "." + "directory";
      if (HttpContext.Current.Session[_key] != null)
      {
        Directories directory = (Directories)HttpContext.Current.Session[_key];
        return FindFolder(directory, path);        
      }
      return null;
    }

    private string UpdateFolders(Folder folder, string context, Resources resources, String oldContext)
    {
      string obj = null;
      Endpoints endpoints = folder.Endpoints;
      Resource resource = null;

      if (endpoints != null)
      {
        foreach (Endpoint endpoint in folder.Endpoints)
        {
          resource = FindResource(endpoint.BaseUrl, resources);
          Locator scope = resource.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == oldContext.ToLower());

          WebHttpClient _newServiceClient = PrepareServiceClient(endpoint.BaseUrl, "adapter");
          obj = _newServiceClient.Post<Locator>(string.Format("/scopes/{0}", context), scope, true);
        }
      }

      Folders subFolders = folder.Folders;

      if (subFolders == null)
        return null;
      else
      {
        foreach (Folder subFolder in subFolders)
        {
          obj = UpdateFolders(subFolder, context, resources, oldContext);
        }
      }

      return obj;
    }

    private string DeleteFolders(Folder folder, string context, Resources resources)
    {
      string obj = null;
      Endpoints endpoints = folder.Endpoints;    
      Resource resource = null;
      EndpointApplication application = null;

      Locator scope = null;

      if (endpoints != null)
      {
        foreach (Endpoint endpoint in endpoints)
        {
          resource = FindResource(endpoint.BaseUrl, resources);
          scope = resource.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());
          application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == endpoint.Name.ToLower());

          WebHttpClient _newServiceClient = PrepareServiceClient(endpoint.BaseUrl, "adapter");
          obj = _newServiceClient.Post<EndpointApplication>(String.Format("/scopes/{0}/delete", context), application, true);
        }
      }

      Folders subFolders = folder.Folders;

      if (subFolders == null)
        return null;
      else
      {
        foreach (Folder subFolder in subFolders)
        {
          obj = DeleteFolders(subFolder, context, resources);
        }
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
    
    private void ClearDirSession(string user)
    {
      if (HttpContext.Current.Session[user + "." + "directory"] != null)
        HttpContext.Current.Session[user + "." + "directory"] = null;

      if (HttpContext.Current.Session[user + "." + "resource"] != null)
        HttpContext.Current.Session[user + "." + "resource"] = null;
    }

    private Resource FindResource(string baseUrl, Resources resources)
    {
      foreach (Resource rc in resources)
      {
        if (rc.BaseUrl.ToLower().Equals(baseUrl.ToLower()))
        {
          return rc;
        }
      }
      return null;
    }

    private string CleanBaseUrl(string url, char con)
    {
      System.Uri uri = new System.Uri(url);
      return uri.Scheme + ":" + con + con + uri.Host + ":" + uri.Port;
    }

    private void CheckeCombination(string baseUrl, string oldBaseUrl, string context, string oldContext, string endpointName, string oldEndpointName, string path, string user)
    {
      string _resource = user + ".resources";
      string lpath = "";
      Locator scope = null;

      if (HttpContext.Current.Session[_resource] != null)
      {
        Resources resources = (Resources)HttpContext.Current.Session[_resource];
        Resource resource = FindResource(CleanBaseUrl(oldBaseUrl, '/'), resources);

        if (resource != null)
          scope = resource.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());
        
        if (scope != null)
        {
          EndpointApplication application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == endpointName.ToLower());
          
          if (application != null && !application.Path.Replace("/", ".").Equals(path))
          {
            lpath = application.Path;
            combinationMsg = "The combination of (" + baseUrl.Replace(".", "/") + ", " + context + ", " + endpointName + ") at " + path.Replace(".", "/") + " is allready existed at " + lpath + ".";
            _logger.Error("Duplicated combination of baseUrl, context, and endpoint name");
            throw new Exception("Duplicated combination of baseUrl, context, and endpoint name");
          }
        }        
      }      
    }

    private void CheckCombination(Folder folder, string path, string context, string oldContext, string user)
    {
      Endpoints endpoints = folder.Endpoints;
      string endpointPath = "";

      if (endpoints != null)
      {
        foreach (Endpoint endpoint in endpoints)
        {
          endpointPath = path + "." + endpoint.Name;
          CheckeCombination(endpoint.BaseUrl, endpoint.BaseUrl, context, oldContext, endpoint.Name, endpoint.Name, endpointPath, user);
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
          CheckCombination(subFolder, path, context, oldContext, user);
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
        CheckCombination(folder, path, context, oldContext, user);
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
            baseUrl = endpoint.BaseUrl + "/adapter";
          
          DataLayer dataLayer = GetDataLayer(context, endpointName, baseUrl);

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

    public DatabaseDictionary GetDBDictionary(string scope, string application, string baseUrl)
    {
      WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "hibernate");
      DatabaseDictionary dbDictionary = _newServiceClient.Get<DatabaseDictionary>(String.Format("/{0}/{1}/dictionary", scope, application));

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
    #endregion
  }
}