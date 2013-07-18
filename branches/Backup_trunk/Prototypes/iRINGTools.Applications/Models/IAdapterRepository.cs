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
    
    DataLayers GetDataLayers();    

    Tree GetDirectoryTree();

    DataLayer GetDataLayer();

    Mapping GetMapping();

    DataDictionary GetDictionary();

    Entity GetClassLabel(string classId);

    string Folder(string newFolderName, string description, string path, string state);    

    string DeleteFolder(string path);

    string Endpoint(string scopeName, string endpointName, string description, string assembly, string path, string state);

    string DeleteEndpoint(string scopeName, string path);

    void getAppScopeName(string baseUri);

    string getScopeName();

    string getAppName();

    string getNodeIconCls(string type);
    
  
 }
}