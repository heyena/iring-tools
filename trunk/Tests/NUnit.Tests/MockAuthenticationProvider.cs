using System.Security.Principal;
using Ninject;
using System.Collections;
using System.Collections.Generic;

namespace org.iringtools.adapter.identity
{
  public class MockAuthenticationProvider : IIdentityLayer
  {
    public IDictionary GetKeyRing()
    {
      IDictionary keyRing = new Dictionary<string, string>();      
      keyRing.Add("Name", "testUser");
      return keyRing;
    }
  }
}
