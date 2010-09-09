using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;

namespace Hatch.DataLayers.iPasXL
{
  public class iPasXLSettings : ServiceSettings
  {
    public iPasXLSettings()
      : base()
    {
      this.Add("ExecutingAssemblyName", "App_Code");
      this.Add("BinaryPath", @".\Bin\");
      this.Add("CodePath", @".\App_Code\");
    }
  }
}
