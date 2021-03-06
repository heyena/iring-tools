﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using org.iringtools.library;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;
using System.IO;

namespace iRINGTools.Web.Models
{
  public interface IAdapterRepository
  {
    ScopeProjects GetScopes();

    ScopeProject GetScope(string name);

    DataLayers GetDataLayers();

    DataLayer GetDataLayer(string scope, string application);

    Mapping GetMapping(string scope, string application);

    DataDictionary GetDictionary(string scope, string application);

    Entity GetClassLabel(string classId);

    string AddScope(string name, string description, string cacheDBConnStr, string permissions);

    string UpdateScope(string name, string newName, string newDescription, string cacheDBConnStr, string permissions);

    string DeleteScope(string name);

    string AddApplication(string scopeName, ScopeApplication application);

    string UpdateApplication(string scopeName, string applicationName, ScopeApplication updatedApplication);

    string DeleteApplication(string scope, string application);

    Response Refresh(string scope, string application);

    Response Refresh(string scope, string application, string dataObjectName);

    Response RefreshCache(string scope, string application, int timeout);

    Response RefreshCache(string scope, string application, string dataObjectName);

    Response ImportCache(string scope, string application, string cacheUri, int timeout);

    Response DeleteCache(string scope, string application);
    
    Response UpdateDataLayer(MemoryStream dataLayerStream);
  }
}