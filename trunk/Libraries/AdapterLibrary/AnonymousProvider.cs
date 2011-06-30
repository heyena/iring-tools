using System.Security.Principal;
using Ninject;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;

namespace org.iringtools.adapter.identity
{
  public class AnonymousProvider : IIdentityLayer
  {
    public IDictionary GetKeyRing()
    {
      IDictionary keyRing = new Dictionary<string, string>();      
      if (ServiceSecurityContext.Current == null)
      {
        keyRing.Add("Provider", "AnonymousProvider");
        keyRing.Add("Name", "anonymous");
      }

      return keyRing;
    }
  }
}
