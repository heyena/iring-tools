using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdapterPrototype
{
  public class Line
  {
    public virtual String Id { get; set; }
    public virtual String tag
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual Single? diameter { get; set; }
    public virtual String system { get; set; }
  }

  public class Valve
  {
    public virtual String Id { get; set; }
    public virtual String tag
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual Single? diameter { get; set; }
    public virtual String system { get; set; }
  }
}
