using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace ClassLibrary
{
  public class GlobalSettings : NameValueCollection
  {
    public GlobalSettings()
    {
      this.Add("AppContext", "iRINGTools");
    }
  }
}
