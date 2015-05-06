using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.mapping;
using org.iringtools.applicationConfig;
namespace iRINGTools.Web.Models
{
  public interface IMappingRepository
  {
    Mapping GetMapping(string scopeName, string applicationName);
    void UpdateMapping(string scopeName, string applicationName, Mapping mapping);
    void UpdateMapping(string scopeName, string applicationName, Graph graph, string userName, bool isAdded, string graphId = null);
    org.iringtools.applicationConfig.Graph GetGraphByGrapgId(string userName, Guid graphId);
    void DeleteGraphByGrapgId(string userName, Guid graphId);
    void updateValueListMap(Guid applicationId,Guid valueListMapid,string valueListname);
    void InsertValueListMap(string valueListname,Guid applicationId);
  }
}