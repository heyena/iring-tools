using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;

namespace org.iringtools.client.Models
{
  public class EntitiesContainer
  {
    public List<Entity> Entities { get; set; }
    public int Total { get; set; }
  }
}