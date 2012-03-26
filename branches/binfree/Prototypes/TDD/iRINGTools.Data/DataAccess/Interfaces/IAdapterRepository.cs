using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public interface IAdapterRepository
  {
    IQueryable<Scope> GetScopes();
    IQueryable<Application> GetApplications();

    void SaveApplication(Application application);
    bool DeleteApplication(int applicationId);

    void SaveApplications(Scope scope);

    void SaveScope(Scope scope);
    bool DeleteScope(int scopeId);

    IQueryable<Configuration> GetConfigurations();
    void SaveConfiguration(Configuration configuration);
    bool DeleteConfiguration(int configurationId);

    IQueryable<Dictionary> GetDictionaries();
    void SaveDictionary(Dictionary dictionary);
    bool DeleteDictionary(int dictionaryId);

    IQueryable<Mapping> GetMappings();
    void SaveMapping(Mapping mapping);
    bool DeleteMapping(int mappingId);

    IQueryable<DataLayerItem> GetDataLayers();
  }
}
