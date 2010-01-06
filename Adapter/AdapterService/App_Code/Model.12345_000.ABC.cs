using System;
using System.Collections.Generic;

namespace org.iringtools.adapter.proj_12345_000.ABC
{
  public class Line
  {
    public virtual String Id { get; set; }
    public virtual Double diameter { get; set; }
    public virtual String uomDiameter { get; set; }
    public virtual String system { get; set; }
  }
  
  public class InLinePipingComponent
  {
    public virtual String Id { get; set; }
    public virtual String componentType { get; set; }
    public virtual Double diameter { get; set; }
    public virtual String uomDiameter { get; set; }
    public virtual String rating { get; set; }
    public virtual String system { get; set; }
    public virtual String unit { get; set; }
    public virtual String projectNumber { get; set; }
    public virtual String pid { get; set; }
    public virtual String lineTag { get; set; }
    public virtual Int32 quantity { get; set; }
  }
  
  public class KOPot
  {
    public virtual String Id { get; set; }
    public virtual String description { get; set; }
  }
  
  public class VacuumTower
  {
    public virtual String Id { get; set; }
    public virtual String description { get; set; }
  }
}
