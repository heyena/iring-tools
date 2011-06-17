using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using iRINGTools.Data;

namespace iRINGTools.Services
{
  [ServiceContract]
  public interface IAdapterService
  {
    [OperationContract]
    IList<Scope> GetScopes();

    Scope GetScope(int scopeId);
    Scope GetScope(string scopeName);
    void DeleteScope(int scopeId);

    IList<Application> GetApplications();
    Application GetApplication(int applicationId);
    Application GetApplication(string scopeName, string applicationName);
    void DeleteApplication(int applicationId);
    
    Configuration GetConfiguration(int applicationId);
    void SaveConfiguration(Configuration add);
    void DeleteConfiguration(int configurationId);

    Dictionary GetDictionary(int applicationId);
    void SaveDictionary(Dictionary add);
    void DeleteDictionary(int dictionaryId);

    Mapping GetMapping(int applicationId);
    void SaveMapping(Mapping add);
    void DeleteMapping(int mappingId);

    IList<DataLayerItem> GetDataLayers();

    Scope GetDefaultScope();
    Application GetDefaultApplication();
  }
}
