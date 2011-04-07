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

    string UpdateScope(string scope, string name, string description);

    string DeleteScope(string scope);

    string UpdateApplication(string scope, string application, string name, string description, string assembly);

    string DeleteApplication(string scope, string application);

    DataProviders GetDataProviders();

    DatabaseDictionary GetDatabaseDictionary(string scope, string application);

    DataObjects GetDataObjects(string scope, string application);
  }
}