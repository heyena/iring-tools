using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public static class DictionaryFilters
  {
    public static DictionaryObject DictionaryObjectByName(this Dictionary dictionary, string dataObjectName)
    {
      DictionaryObject dataObject = null;

      if (dictionary.DictionaryObjects != null)
      {
        dataObject = dictionary.DictionaryObjects
          .Where(o => o.ObjectName.ToUpper() == dataObjectName.ToUpper())
          .SingleOrDefault();
      }

      return dataObject;
    }
  }
}
