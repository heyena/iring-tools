using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;
using System.Collections.Specialized;

namespace iringtools.sdk.sp3ddatalayer
{
  public class NHibernateSettings : ServiceSettings
  {
    public NHibernateSettings()
      : base()
    {
      this.Add("ExecutingAssemblyName", "App_Code");
      this.Add("BinaryPath", @".\Bin\");
      this.Add("CodePath", @".\App_Code\");
    }
  }
}
