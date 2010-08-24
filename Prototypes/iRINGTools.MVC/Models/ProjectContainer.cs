using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using org.iringtools.library;

namespace org.iringtools.client.Models
{
  public class ProjectContainer
  {
    public Collection<ScopeProject> Projects { get; set; }
    public int Count { get; set; }
  }
}