using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Text;
using System.IO;

namespace iRINGTools.Data
{
  public class LinqToXMLAdapterRepository: IAdapterRepository
  {
    protected static readonly XNamespace XML_NS = XNamespace.Get("http://www.iringtools.org/library");
    protected static readonly XNamespace W3_NS = XNamespace.Get("http://wwww.w3.org/2001/XMLSchema-instance");

    protected static readonly string XMLNS_PREFIX = "xmlns";
    protected static readonly string W3_PREFIX = "i";

    protected static readonly string SCOPES_ELEMENT = "scopes";
    protected static readonly string SCOPE_ELEMENT = "scope";

    protected static readonly string SCOPE_NAME = "name";
    protected static readonly string SCOPE_DESCRIPTION = "description";

    protected static readonly string APPLICATIONS_ELEMENT = "applications";
    protected static readonly string APPLICATION_ELEMENT = "application";
    
    protected static readonly string APPLICATION_NAME = "name";
    protected static readonly string APPLICATION_DESCRIPTION = "description";
    protected static readonly string APPLICATION_DATALAYER = "dataLayerName";

    private LazyList<Scope> _scopeList;
    private LazyList<Application> _applicationList;
    private LazyList<Configuration> _configurationList;
    private LazyList<Dictionary> _dictionaryList;
    private LazyList<Mapping> _mappingList;

    private IDataLayerRepository _dataLayerRepository;

    private string _path;
    private string _fileName;
    private XDocument _document;
    
    private int nextScopeId = 1;
    private int nextApplicationId = 1;
    private int nextConfigurationId = 1;
    private int nextDictionaryId = 1;
    private int nextMappingId = 1;

    public LinqToXMLAdapterRepository(string fileName, IDataLayerRepository dataLayerRepository)
    {
      _dataLayerRepository = dataLayerRepository;

      _fileName = fileName;

      if (!File.Exists(_fileName))
        throw new InvalidOperationException(string.Format("\"{0}\" can not access file or does not exist!", _fileName));

      _path = Path.GetDirectoryName(fileName);
    }

    #region Scopes

    public IQueryable<Scope> GetScopes()
    {
      if (_scopeList == null)
      {
        _scopeList = new LazyList<Scope>();
        _document = XDocument.Load(_fileName);

        foreach (var scopeItem in _document.Descendants(XML_NS + SCOPE_ELEMENT))
        {
          var scope = new Scope
          {
            Id = nextScopeId++,
            Name = scopeItem.GetValue(XML_NS + SCOPE_NAME) ?? "",
            Description = scopeItem.GetValue(XML_NS + SCOPE_DESCRIPTION) ?? ""
          };

          _scopeList.Add(scope);
        }

      }
      
      return _scopeList.AsQueryable();
    }

    public void SaveScope(Scope scope)
    {
      //find the Scope
      Scope s = _scopeList.Where(x => x.Name == scope.Name).SingleOrDefault();
      if (s != null)
      {
        s = scope;
      }
      else
      {
        scope.Id = nextScopeId++;
        _scopeList.Add(scope);
      }
    }

    public void SaveApplications(Scope scope)
    {
      foreach (Application a in scope.Applications)
      {
        SaveApplication(a);
      }
    }

    public bool DeleteScope(int scopeId)
    {
      //find the Scope
      Scope s = _scopeList.Where(x => x.Id== scopeId).SingleOrDefault();
      if (s != null)
      {
        foreach (Application a in s.Applications)
        {
          DeleteApplication(a.Id);
        }

        return _scopeList.Remove(s);
      }

      return false;
    }
    #endregion

    #region Applications

    public IQueryable<Application> GetApplications()
    {
      if (_applicationList == null) {
        _applicationList = new LazyList<Application>();
        
        foreach (var applicationItem in _document.Descendants(XML_NS + APPLICATION_ELEMENT))
        {
          string scopeName = applicationItem.Parent.GetValue(XML_NS + SCOPE_NAME);
          Scope scope = GetScopes().WithScopeName(scopeName).SingleOrDefault();

          string name = applicationItem.GetValue(XML_NS + APPLICATION_NAME) ?? "";
          string context = string.Format("{0}.{1}", scope.Name, name);

          var application = new Application
          {
            Id = nextApplicationId++,
            Name = name,
            Description = applicationItem.GetValue(XML_NS + APPLICATION_DESCRIPTION) ?? "",
            Scope = scope,

            Configuration = GetConfigurations()
              .WithConfigurationName(context)
              .SingleOrDefault(),

            Dictionary = GetDictionaries()
              .WithDictionaryName(context)
              .SingleOrDefault(),

            Mapping = GetMappings()
              .WithMappingName(context)
              .SingleOrDefault(),

            DataLayerItem = GetDataLayers()
              .WithDataLayerName(applicationItem.GetValue(XML_NS + APPLICATION_DATALAYER) ?? "")
              .SingleOrDefault()
          };

          _applicationList.Add(application);
        }
      }

      return _applicationList.AsQueryable();
    }

    public void SaveApplication(Application application)
    {
      //find the Application
      Application a = _applicationList.Where(x => x.Name == application.Name).SingleOrDefault();
      if (a != null)
      {
        a = application;
      }
      else
      {
        application.Id = nextApplicationId++;
        _applicationList.Add(application);
      }
    }

    public bool DeleteApplication(int applicationId)
    {
      //find the Application
      Application a = _applicationList.Where(x => x.Id == applicationId).SingleOrDefault();
      if (a != null)
      {
        return _applicationList.Remove(a);
      }

      return false;
    }

    #endregion

    #region Configurations

    public IQueryable<Configuration> GetConfigurations()
    {
      if (_configurationList == null)
      {
        _configurationList = new LazyList<Configuration>();

        string filePrefix = "Configuration.";
        var files = Directory.GetFiles(_path, filePrefix);

        foreach (var file in files)
        {
          string fileName = Path.GetFileNameWithoutExtension(file);
          string name = fileName.Replace(filePrefix, "");

          var configuration = new Configuration
          {
            Id = nextConfigurationId++, 
            Name = name,
            ConnectionString = ""
          };

          _configurationList.Add(configuration);
        }
      }

      return _configurationList.AsQueryable();
    }

    public void SaveConfiguration(Configuration configuration)
    {
      throw new NotImplementedException();
    }

    public bool DeleteConfiguration(int configurationId)
    {
      //find the Configuration
      Configuration o = _configurationList.Where(x => x.Id == configurationId).SingleOrDefault();
      if (o != null)
      {
        return _configurationList.Remove(o);
      }

      return false;
    }

    #endregion

    #region Dictionaries

    public IQueryable<Dictionary> GetDictionaries()
    {
      if (_dictionaryList != null)
      {
        _dictionaryList = new LazyList<Dictionary>();
        
        string filePrefix = "DataDictionary.";
        var files = Directory.GetFiles(_path, filePrefix);

        foreach (var file in files)
        {
          string fileName = Path.GetFileNameWithoutExtension(file);
          string name = fileName.Replace(filePrefix, "");

          var dictionary = new Dictionary
          {
            Id = nextDictionaryId++,
            Name = name
          };

          _dictionaryList.Add(dictionary);
        }
      }

      return _dictionaryList.AsQueryable();
    }

    public void SaveDictionary(Dictionary dictionary)
    {
      throw new NotImplementedException();
    }

    public bool DeleteDictionary(int dictionaryId)
    {
      //find the Dictionary
      Dictionary o = _dictionaryList.Where(x => x.Id == dictionaryId).SingleOrDefault();
      if (o != null)
      {
        return _dictionaryList.Remove(o);
      }

      return false;
    }

    #endregion

    #region Mappings

    public IQueryable<Mapping> GetMappings()
    {
      if (_mappingList != null)
      {
        _mappingList = new LazyList<Mapping>();

        string filePrefix = "Mapping.";
        var files = Directory.GetFiles(_path, filePrefix);

        foreach (var file in files)
        {
          string fileName = Path.GetFileNameWithoutExtension(file);
          string name = fileName.Replace(filePrefix, "");

          var mapping = new Mapping
          {
            Id = nextMappingId++,
            Name = name
          };

          _mappingList.Add(mapping);
        }
      }

      return _mappingList.AsQueryable();
    }

    public void SaveMapping(Mapping mapping)
    {
      throw new NotImplementedException();
    }

    public bool DeleteMapping(int mappingId)
    {
      //find the Mapping
      Mapping o = _mappingList.Where(x => x.Id == mappingId).SingleOrDefault();
      if (o != null)
      {
        return _mappingList.Remove(o);
      }

      return false;
    }

    #endregion

    #region DataLayers

    public IQueryable<DataLayerItem> GetDataLayers()
    {
      return _dataLayerRepository.GetDataLayers();
    }

    #endregion

    public void SaveChanges(string fileName)
    {
      var scopes = new XElement(XML_NS + SCOPES_ELEMENT
        , new XAttribute(XMLNS_PREFIX, XML_NS)
        , new XAttribute(XNamespace.Xmlns + W3_PREFIX, W3_NS)
      );

      foreach (Scope s in _scopeList)
      {
        var applications = new XElement(XML_NS + APPLICATIONS_ELEMENT);

        var query = _applicationList.Where(a => a.Scope.Id == s.Id);
        foreach (var a in query)
        {
          var application = new XElement(XML_NS + APPLICATION_ELEMENT);
          application.Add(new XElement(XML_NS + APPLICATION_NAME) { Value = a.Name });
          application.Add(new XElement(XML_NS + APPLICATION_DESCRIPTION) { Value = a.Description });
          application.Add(new XElement(XML_NS + APPLICATION_DATALAYER) { Value = a.DataLayerItem != null ? a.DataLayerItem.Name : "" });
          
          applications.Add(application);
        }

        var scope = new XElement(XML_NS + SCOPE_ELEMENT);
        scope.Add(new XElement(XML_NS + SCOPE_NAME) { Value = s.Name });
        scope.Add(new XElement(XML_NS + SCOPE_DESCRIPTION) { Value = s.Description });

        scope.Add(applications);
        scopes.Add(scope);
      };

      _document = new XDocument();
      _document.Add(scopes);

      _document.Save(fileName);
    }
  }
}
