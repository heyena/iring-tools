//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated.
//     Runtime Version:2.0.50727.3074
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Iesi.Collections.Generic;

namespace org.iringtools.adapter.proj_12345_000.MM
{
  public class LINE
  {
    public virtual String Id { get; set; }
    public virtual String TAGNO
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual Double DIAMETER { get; set; }
    public virtual String LENGTH_UOM { get; set; }
    public virtual String B_SYS_LOC { get; set; }
  }
}
