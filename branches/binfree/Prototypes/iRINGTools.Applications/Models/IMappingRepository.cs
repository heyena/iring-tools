using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.mapping;

namespace iRINGTools.Web.Models
{
  public interface IMappingRepository
  {
    Mapping GetMapping();
    void UpdateMapping(Mapping mapping);
    void getAppScopeName(string baseUri);
    string getScope();
    string getApplication();
    void setScope(string value);
    void setApplication(string value);
  }
}