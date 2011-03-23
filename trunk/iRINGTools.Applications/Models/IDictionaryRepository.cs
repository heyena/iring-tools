using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using org.iringtools.library;
using org.iringtools.mapping;

namespace iRINGTools.Web.Models
{
  public interface IDictionaryRepository
  {
    ScopeProjects GetScopes();

    ScopeProject GetScope(string name);

    DataLayers GetDataLayers();

    ScopeApplication GetScopeApplication(string scope, string application);

    DataLayer GetDataLayer(string scope, string application);

    Mapping GetMapping(string scope, string application);

    DataDictionary GetDictionary(string scope, string application);

    string AddScope(ScopeProject scope);

    string DeleteScope(string p);

    string DeleteApplication(string scope, string app);


    string AddApplication(string scope , ScopeApplication app);
  }
}