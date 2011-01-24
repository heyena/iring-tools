using System.Security.Principal;
using System.ServiceModel;
using Ninject;
using System.Collections;
using System.Collections.Generic;

namespace org.iringtools.adapter.identity
{
  public class WindowsAuthenticationProvider : IIdentityLayer
  {
    public IDictionary GetKeyRing()
    {
      IDictionary keyRing = new Dictionary<string, string>();
      if (ServiceSecurityContext.Current != null)
      {
        IIdentity identity = ServiceSecurityContext.Current.PrimaryIdentity;

        keyRing.Add("Provider", "WindowsAuthenticationProvider");
        keyRing.Add("AuthenticationType", identity.AuthenticationType);
        keyRing.Add("IsAuthenticated", identity.IsAuthenticated.ToString());
        keyRing.Add("Name", identity.Name);
      }

      return keyRing;
    }
  }
}
