using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using iRINGTools.Data;

namespace iRINGTools.Tests
{
  public class TestAdapterRepository: IAdapterRepository
  {
    private int nextScopeId = 1;
    private List<Scope> _scopeList;

    private int nextApplicationId = 1;
    private List<Application> _applicationList;

    private int nextConfigurationId = 1;
    private List<Configuration> _configurationList;

    private int nextDictionaryId = 1;
    private List<Dictionary> _dictionaryList;

    private int nextMappingId = 1;
    private List<Mapping> _mappingList;

    private IDataLayerRepository _dataLayerRepository;

    private string _description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce ornare neque eget justo fermentum sed tincidunt diam viverra. Suspendisse mollis mi vel arcu egestas sit amet imperdiet justo cursus. In quis dui arcu, id luctus augue. Quisque nec libero nibh, id imperdiet nunc. Nulla vitae dui nec lectus volutpat lacinia. In porttitor convallis nisl, nec viverra erat feugiat vitae. Cras adipiscing nibh ac nibh dapibus vulputate. Fusce elementum, felis vel semper eleifend, purus tellus fringilla mauris, et tristique sapien massa eget ipsum. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Ut imperdiet urna elit.";

    public TestAdapterRepository()
    {
    }

    public TestAdapterRepository(IDataLayerRepository dataLayerRepository)
    {
      _dataLayerRepository = dataLayerRepository;
    }

    public IQueryable<Scope> GetScopes()
    {
      if (_scopeList == null)
      {
        _scopeList = new List<Scope>();

        for (int i = 1; i <= 5; i++)
        {
          _scopeList.Add(new Scope
          {
            Id = nextScopeId++,
            Name = "Scope" + i.ToString(),
            Description = _description,
            IsDefault = (i == 1)
          });
        }
      }

      return _scopeList.AsQueryable();
    }

    public IQueryable<Application> GetApplications()
    {
      if (_applicationList == null)
      {
        DataLayerItem defaultDataLayer = _dataLayerRepository.GetDataLayers().DefaultDataLayer();
        _applicationList = new List<Application>();

        int s = 0;
        for (int a = 1; a <= 25; a++)
        {
          int i = nextApplicationId++;

          if (((a - 1) % 5) == 0)
            s++;

          _applicationList.Add(new Application
          {
            Id = i,
            Name = "Application" + i.ToString(),
            Description = _description,
            Scope = GetScopes().WithScopeId(s).SingleOrDefault(),
            Configuration = GetConfigurations().WithConfigurationId(a).SingleOrDefault(),
            Dictionary = GetDictionaries().WithDictionaryId(a).SingleOrDefault(),
            Mapping = GetMappings().WithMappingId(a).SingleOrDefault(),
            DataLayerItem = GetDataLayers().WithDataLayerGuid(defaultDataLayer.Guid).SingleOrDefault()
          });
        }
      }

      return _applicationList.AsQueryable();
    }

    public IQueryable<Configuration> GetConfigurations()
    {
      if (_configurationList == null)
      {
        _configurationList = new List<Configuration>();

        for (int i = 1; i <= 25; i++)
        {
          _configurationList.Add(new Configuration
          {
            Id = nextConfigurationId++,
            ConnectionString = "ConnectionString" + i
          });
        }
      }

      return _configurationList.AsQueryable();
    }

    public IQueryable<Dictionary> GetDictionaries()
    {
      if (_dictionaryList == null)
      {
        _dictionaryList = new List<Dictionary>();

        for (int i = 1; i <= 25; i++)
        {
          _dictionaryList.Add(new TestDictionary
          {
            Id = nextDictionaryId++
          });
        }
      }

      return _dictionaryList.AsQueryable();
    }

    public IQueryable<Mapping> GetMappings()
    {
      if (_mappingList == null)
      {
        _mappingList = new List<Mapping>();

        for (int i = 1; i <= 25; i++)
        {
          _mappingList.Add(new Mapping
          {
            Id = nextMappingId++,
            GraphMaps = new LazyList<GraphMap> {
              new GraphMap {
                Name = "Lines",
                DictionaryObjectName = "LINES",
                ClassTemplateMaps = new LazyList<ClassTemplateMap> {
                  #region ClassTemplateMaps
                  new ClassTemplateMap { 
                    ClassMap = new ClassMap {
                      Id = "rdl:R19192462550",
                      Name = "PIPING NETWORK SYSTEM",
                      Identifiers = new LazyList<String> { "LINES.TAG" }
                    },
                    TemplateMaps = new LazyList<TemplateMap> {
                      #region TemplateMaps
                      new TemplateMap { 
                        Id = "tpl:R66921101783",
                        Name = "IdentificationByTag",
                        RoleMaps = new LazyList<RoleMap>() {
                          #region RoleMaps
                          new RoleMap {
                            Type = RoleType.Possessor,
                            Id = "tpl:R44537504070",
                            Name = "hasObject"
                          },
                          new RoleMap {
                            Type = RoleType.Reference,
                            Id = "tpl:R30790108016",
                            Name = "hasIdentificationType",
                            Value = "rdl:R40471041754"
                          },
                          new RoleMap {
                            Id = "tpl:R22674749688",
                            Name = "valIdentifier",
                            DataType = "string",
                            PropertyName = "LINES.TAG"
                          }
                          #endregion
                        } 
                      }, 
                      new TemplateMap { 
                        Id = "tpl:R41248767846",
                        Name = "PlantAreaHasPlantObject",
                        RoleMaps = new LazyList<RoleMap> {
                          #region RoleMaps
                          new RoleMap { 
                            Type = RoleType.Possessor,
                            Id = "tpl:R77585325576",
                            Name = "part"
                          },  
                          new RoleMap {
                            Type = RoleType.Reference,
                            Id = "tpl:R62157498012",
                            Name = "assemblyType",
                            Value = "rdl:R30942378492"
                          }, 
                          new RoleMap { 
                            Type = RoleType.Reference,
                            Id = "tpl:R62348020201",
                            Name = "whole",
                            Value = "rdl:R49658319833",
                            ClassMap = new ClassMap { 
                              Id = "rdl:R49658319833",
                              Name = "PLANT AREA",
                              Identifiers = new LazyList<string> { "LINES.AREA" }
                            }
                          }
                          #endregion
                        }
                      },
                      new TemplateMap { 
                        Id = "tpl:R11327281485",
                        Name = "PAndIDRepresentation",
                        RoleMaps = new LazyList<RoleMap>() {
                          #region RoleMaps
                          new RoleMap {
                            Type = RoleType.Possessor,
                            Id = "tpl:R22715013877",
                            Name = "object"
                          },
                          new RoleMap {
                            Type = RoleType.Reference,
                            Id = "tpl:R35290929175",
                            Name = "representationType",
                            Value = "rdl:R99486931975"
                          }, 
                          new RoleMap {
                            Type = RoleType.Reference,
                            Id = "tpl:R74328815710",
                            Name = "representation",
                            Value = "rdl:R99486931975",
                            ClassMap = new ClassMap {
                              Id = "rdl:R99486931975",
                              Name = "P AND I DIAGRAM",
                              Identifiers = new LazyList<string> { "LINES.PIDNUMBER" }
                            }
                          } 
                          #endregion
                        } 
                      },
                      new TemplateMap {
                        Id = "tpl:R77443358818",
                        Name = "NominalDiameter",
                        RoleMaps = new LazyList<RoleMap> { 
                          #region RoleMaps
                          new RoleMap { 
                            Type = RoleType.Possessor,
                            Id = "tpl:R16314127825",
                            Name = "hasPossessor"
                          },
                          new RoleMap { 
                            Id = "tpl:R93884193692",
                            Name = "hasScale",
                            DataType = "String",
                            PropertyName = "LINES.UOM_NOMDIAMETER"
                          },
                          new RoleMap { 
                            Type = RoleType.Reference,
                            Id = "tpl:R59896853239",
                            Name = "hasType", 
                            Value = "rdl:R17622148043"
                          },
                          new RoleMap { 
                            Id = "tpl:R90688063648",
                            Name = "valValue",
                            DataType = "double",
                            PropertyName = "LINES.NOMDIAMETER"
                          }
                          #endregion
                        }
                      }
                      #endregion
                    },
                  },
                  new ClassTemplateMap {
                    ClassMap = new ClassMap { 
                      Id = "rdl:R49658319833",
                      Name = "PLANT AREA",
                      Identifiers = new LazyList<string> { "LINES.AREA" }
                    },
                    TemplateMaps = new LazyList<TemplateMap> {
                      #region TemplateMaps
                      new TemplateMap { 
                        Id = "tpl:R66921101783",
                        Name = "IdentificationByTag",
                        RoleMaps = new LazyList<RoleMap> {
                          #region RoleMap
                          new RoleMap { 
                            Type = RoleType.Possessor,
                            Id = "tpl:R44537504070",
                            Name = "hasObject"
                          },
                          new RoleMap { 
                            Type = RoleType.Reference,
                            Id = "tpl:R30790108016",
                            Name = "hasIdentificationType",
                            Value = "rdl:R40471041754"
                          },
                          new RoleMap { 
                            Id = "tpl:R22674749688",
                            Name = "valIdentifier",
                            DataType = "string",
                            PropertyName = "LINES.AREA"
                          }
                          #endregion
                        }
                      }
                      #endregion
                    }
                  },
                  new ClassTemplateMap {
                    ClassMap = new ClassMap { 
                      Id = "rdl:R99486931975",
                      Name = "P AND I DIAGRAM",
                      Identifiers = new LazyList<string> { "LINES.PIDNUMBER" }
                    },
                    TemplateMaps = new LazyList<TemplateMap> {
                      #region TemplateMaps
                      new TemplateMap { 
                        Id = "tpl:R30193386273",
                        Name = "ClassifiedIdentification1",
                        RoleMaps = new LazyList<RoleMap> {
                          #region RoleMaps
                          new RoleMap { 
                            Type = RoleType.Possessor,
                            Id = "tpl:R44537504070",
                            Name = "hasObject"
                          },
                          new RoleMap { 
                            Type = RoleType.Reference,
                            Id = "tpl:R30790108016",
                            Name = "hasIdentificationType",
                            Value = "rdl:R16893283050"
                          },
                          new RoleMap { 
                            Id = "tpl:R22674749688",
                            Name = "valIdentifier",
                            DataType = "string",
                            PropertyName = "LINES.PIDNUMBER"
                          }
                          #endregion
                        } 
                      }
                      #endregion
                    } 
                  }
                  #endregion
                }
              }
            }
          });
        }
      }

      return _mappingList.AsQueryable();
    }

    public IQueryable<DataLayerItem> GetDataLayers()
    {
      return _dataLayerRepository.GetDataLayers();
    }

    public void SaveConfiguration(Configuration configuration)
    {
      var item = _configurationList.Where(x => x.Id == configuration.Id).SingleOrDefault();
      if (item != null)
      {
        item = configuration;
      }
      else
      {
        item.Id = nextConfigurationId++;
        _configurationList.Add(configuration);
      }
    }

    public bool DeleteConfiguration(int configurationId)
    {
      Configuration item = _configurationList.Where(x => x.Id == configurationId).SingleOrDefault();
      if (item != null)
      {
        _configurationList.Remove(item);
        return true;
      }

      return false;
    }

    public void SaveDictionary(Dictionary dictionary)
    {
      var item = _dictionaryList.Where(x => x.Id == dictionary.Id).SingleOrDefault();
      if (item != null)
      {
        item = dictionary;
      }
      else
      {
        item.Id = nextDictionaryId++;
        _dictionaryList.Add(dictionary);
      }
    }

    public bool DeleteDictionary(int dictionaryId)
    {
      Dictionary item = _dictionaryList.Where(x => x.Id == dictionaryId).SingleOrDefault();
      if (item != null)
      {
        _dictionaryList.Remove(item);
        return true;
      }

      return false;
    }

    public void SaveMapping(Mapping mapping)
    {
      var item = _mappingList.Where(x => x.Id == mapping.Id).SingleOrDefault();
      if (item != null)
      {
        item = mapping;
      }
      else
      {
        item.Id = nextMappingId++;
        _mappingList.Add(mapping);
      }
    }

    public bool DeleteMapping(int mappingId)
    {
      throw new NotImplementedException();
    }

    public void SaveApplication(Application application)
    {
      var item = _applicationList.Where(x => x.Id == application.Id).SingleOrDefault();
      if (item != null)
      {
        item = application;
      }
      else
      {
        item.Id = nextApplicationId++;
        _applicationList.Add(application);
      }

      SaveConfiguration(item.Configuration);
      SaveDictionary(item.Dictionary);
      SaveMapping(item.Mapping);
    }

    public bool DeleteApplication(int applicationId)
    {
      Application item = _applicationList.Where(x => x.Id == applicationId).SingleOrDefault();
      if (item != null)
      {
        _applicationList.Remove(item);
        return true;
      }

      return false;
    }

    public void SaveApplications(Scope scope)
    {
      foreach(var application in GetApplications())
      {
        SaveApplication(application);
      }
    }

    public void SaveScope(Scope scope)
    {
      var item = _scopeList.Where(x => x.Id == scope.Id).SingleOrDefault();
      if (item != null)
      {
        item = scope;
      }
      else
      {
        item.Id = nextScopeId++;
        _scopeList.Add(scope);
      }
    }

    public bool DeleteScope(int scopeId)
    {
      Scope item = _scopeList.Where(x => x.Id == scopeId).SingleOrDefault();
      if (item != null)
      {
        _scopeList.Remove(item);
        return true;
      }

      return false;
    }
  }
}
