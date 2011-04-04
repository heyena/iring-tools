using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using org.iringtools.library;
using org.iringtools.mapping;

namespace iRINGTools.Web.Models
{
  public interface IAdapterRepository
  {
    ScopeProjects GetScopes();

    ScopeProject GetScope(string name);

    DataLayers GetDataLayers();

    ScopeApplication GetScopeApplication(string scope, string application);

    DataLayer GetDataLayer(string scope, string application);

    Mapping GetMapping(string scope, string application);

    DataDictionary GetDictionary(string scope, string application);

    string UpdateScope(string scopeName, string name, string description);

    string DeleteScope(string scopeName);

    string UpdateApplication(string scopeName, string applicationName, string name, string description, string assembly);

    string DeleteApplication(string scopeName, string applicationName);
    
  }
}