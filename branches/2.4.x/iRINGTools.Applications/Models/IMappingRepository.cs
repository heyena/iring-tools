using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.mapping;

namespace iRINGTools.Web.Models
{
  public interface IMappingRepository
  {
    Mapping GetMapping(string scopeName, string applicationName);
    void UpdateMapping(string scopeName, string applicationName, Mapping mapping);
  }
}