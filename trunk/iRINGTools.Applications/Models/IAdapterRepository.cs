using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using org.iringtools.library;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;
using System.Collections;


namespace iRINGTools.Web.Models
{
  public interface IAdapterRepository
  {
    Directories GetScopes();

    DataLayers GetDataLayers();

    Tree GetDirectoryTree(string user);

    DataLayer GetDataLayer(string contextName, string endpoint);

    Mapping GetMapping(string contextName, string endpoint);

    DataDictionary GetDictionary(string contextName, string endpoint);

    Entity GetClassLabel(string classId);

    string Folder(string newFolderName, string description, string path, string state, string context, string oldContext, string user);

    string DeleteEntry(string path, string type, string context, string baseUrl, string user);

    string Endpoint(string newEndpointName, string path, string description, string states, string context, string oldAssembly, string newAssembly, string baseUrl, string oldBaseUrl, string user);

    string GetNodeIconCls(string type);

    string GetRootSecurityRole();

    string GetUserLdap();

    BaseUrls GetEndpointBaseUrl();

    string GetCombinationMsg();
  }
}