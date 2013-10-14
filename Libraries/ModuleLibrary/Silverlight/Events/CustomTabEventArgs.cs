using System;
using org.iringtools.informationmodel.usercontrols;
using org.iringtools.informationmodel.types;

namespace org.iringtools.informationmodel.events
{
  public class CustomTabEventArgs : EventArgs
  {
    public CustomTabItem ActiveTab { get; set; }
    public CustomTabProcess Process { get; set; }
 
  }
}
