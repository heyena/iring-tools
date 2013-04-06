using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public static class XElementExtentions
  {
    public static string GetValue(this XElement element, XName name)
    {
      var e = element.Element(name);

      if (e != null)
        return e.Value;
      else
        return null;
    }
  }
}
