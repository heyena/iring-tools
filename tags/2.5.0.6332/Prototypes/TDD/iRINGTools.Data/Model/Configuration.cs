using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public class Configuration
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string ConnectionString { get; set; }

    public Configuration()
    {
    }

    #region object overrides
    public override bool Equals(object obj)
    {
      if (obj is Scope)
      {
        Scope compareTo = (Scope)obj;
        return compareTo.Id == this.Id;
      }
      else
      {
        return base.Equals(obj);
      }
    }

    public override string ToString()
    {
      return this.ConnectionString;
    }
    public override int GetHashCode()
    {
      return this.Id.GetHashCode();
    }
    #endregion
  }
}
