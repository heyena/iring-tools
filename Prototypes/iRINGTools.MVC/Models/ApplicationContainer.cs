using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using org.iringtools.library;

namespace org.iringtools.client.Models
{
  public class ApplicationContainer
  {
    public List<ScopeApplication> Applications { get; set; }
    public int Total { get; set; }
  }
}