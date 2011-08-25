using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Linq;

using iRINGTools.Data;

namespace iRINGTools.Services
{
  [ServiceBehavior]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class AdapterService : IAdapterService
  {
    private IAdapterRepository _adapterRepository;
    
    public AdapterService()
    {
      
    }

    public AdapterService(IAdapterRepository adapterRepository)
    {
      _adapterRepository = adapterRepository;
    }

    /// <summary>
    /// Gets all scopes in the system
    /// </summary>
    /// <returns></returns>
    [WebGet(UriTemplate="/scopes")]
    public IList<Scope> GetScopes()
    {
      IList<Scope> scopes = _adapterRepository.GetScopes().ToList();

      foreach(var s in scopes)
      {
        s.Applications = _adapterRepository.GetApplications()
          .Where(a => s.Equals(a.Scope)).ToList();
      };

      return scopes;
    }

    /// <summary>
    /// Gets a single scope by Id
    /// </summary>
    public Scope GetScope(int scopeId)
    {
      Scope result = _adapterRepository.GetScopes()
        .WithScopeId(scopeId)
        .SingleOrDefault();

      //pre processing if required
      if (result != null)
      {
        result.Applications = _adapterRepository.GetApplications()
            .Where(a => result.Equals(a.Scope)).ToList();
      }

      return result;
    }

    /// <summary>
    /// Gets a single scope by Name
    /// </summary>
    public Scope GetScope(string scopeName)
    {
      Scope result = _adapterRepository.GetScopes()
        .WithScopeName(scopeName)
        .SingleOrDefault();

      //pre processing if required
      if (result != null)
      {
        result.Applications = _adapterRepository.GetApplications()
            .Where(a => result.Equals(a.Scope)).ToList();
      }

      return result;
    }

    /// <summary>
    /// Returns all applications
    /// </summary>
    public IList<Application> GetApplications()
    {
      return _adapterRepository.GetApplications()
        .ToList();
    }

    /// <summary>
    /// Returns a application for a given application id
    /// </summary>
    public Application GetApplication(int applicationId)
    {
      return _adapterRepository.GetApplications()
        .WithApplicationId(applicationId)
        .SingleOrDefault();
    }

    /// <summary>
    /// Returns a application for a given application id
    /// </summary>
    public Application GetApplication(string scopeName, string applicationName)
    {
      Scope scope = GetScope(scopeName);

      return _adapterRepository.GetApplications()
        .Where(a => a.Scope.Name == scopeName && a.Name == applicationName) 
        .SingleOrDefault();
    }

    /// <summary>
    /// Returns a configuration for a given application id
    /// </summary>
    public Configuration GetConfiguration(int applicationId)
    {
      return GetApplication(applicationId).Configuration;
    }

    /// <summary>
    /// Saves an configuration
    /// </summary>
    public void SaveConfiguration(Configuration add)
    {
      _adapterRepository.SaveConfiguration(add);
    }

    /// <summary>
    /// Returns a dictionary for a given application id
    /// </summary>
    public Dictionary GetDictionary(int applicationId)
    {
      return GetApplication(applicationId).Dictionary;
    }

    /// <summary>
    /// Saves an dictionary
    /// </summary>
    public void SaveDictionary(Dictionary add)
    {
      _adapterRepository.SaveDictionary(add);
    }

    /// <summary>
    /// Returns a mapping for a given application id
    /// </summary>
    public Mapping GetMapping(int applicationId)
    {
      return GetApplication(applicationId).Mapping;
    }

    /// <summary>
    /// Saves an mapping
    /// </summary>
    public void SaveMapping(Mapping add)
    {
      _adapterRepository.SaveMapping(add);
    }

    /// <summary>
    /// Returns all datalayers
    /// </summary>
    public IList<DataLayerItem> GetDataLayers()
    {
      return _adapterRepository.GetDataLayers()
        .ToList();
    }

    public void DeleteScope(int scopeId)
    {
      _adapterRepository.DeleteScope(scopeId);
    }

    public void DeleteApplication(int applicationId)
    {
      _adapterRepository.DeleteApplication(applicationId);
    }

    public void DeleteConfiguration(int configurationId)
    {
      _adapterRepository.DeleteConfiguration(configurationId);
    }

    public void DeleteDictionary(int dictionaryId)
    {
      _adapterRepository.DeleteDictionary(dictionaryId);
    }

    public void DeleteMapping(int mappingId)
    {
      _adapterRepository.DeleteMapping(mappingId);
    }

    public Scope GetDefaultScope()
    {
      var result = _adapterRepository.GetScopes().DefaultScope();
      return result;
    }

    public Application GetDefaultApplication()
    {
      var result = _adapterRepository.GetApplications().DefaultApplication();
      return result;
    }
  }
}
