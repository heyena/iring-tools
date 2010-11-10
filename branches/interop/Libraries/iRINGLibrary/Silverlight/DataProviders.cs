using System.Collections.Generic;
using System.Runtime.Serialization;
using org.iringtools.utility;

namespace org.iringtools.library
{
  [CollectionDataContract(Namespace = "http://iringtools.org/common", Name = "providers", ItemName = "provider")]
  public class DataProviders : List<Provider>
  {
    public DataProviders()
    {
#if !SILVERLIGHT
      foreach (Provider provider in System.Enum.GetValues(typeof(Provider)))
      {
        this.Add(provider);
      }
#else
      
      this.AddRange(Utility.GetEnumValues<Provider>());
#endif
    }

  }
}
