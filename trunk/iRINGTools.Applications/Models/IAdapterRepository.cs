using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using org.iringtools.library;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;

namespace iRINGTools.Web.Models
{
  public interface IAdapterRepository
  {
    Directories GetScopes();

    DataLayers GetDataLayers(string contextName, string endpoint);

    Tree GetDirectoryTree();

    DataLayer GetDataLayer(string contextName, string endpoint);

    Mapping GetMapping(string contextName, string endpoint);

    DataDictionary GetDictionary(string contextName, string endpoint);

    Entity GetClassLabel(string classId);

    string Folder(string newFolderName, string description, string path, string state, string context);

    string DeleteEntry(string path);

    string Endpoint(string newEndpointName, string path, string description, string states);

    string getNodeIconCls(string type);

    string getRootSecurityRole();
  }
}