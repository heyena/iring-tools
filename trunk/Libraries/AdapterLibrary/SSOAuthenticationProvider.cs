using System;
using System.Security.Principal;
using System.ServiceModel;
using Ninject;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel.Web;

namespace org.iringtools.adapter.identity
{
  public class SSOAuthenticationProvider : IIdentityLayer
  {
    private const string OAUTH_HEADER_NAME = "authenticated-user";

    public IDictionary GetKeyRing()
    {
      IDictionary keyRing = new Dictionary<string, string>();

      if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers.Count > 0)
      {
        string oAuthHeader = WebOperationContext.Current.IncomingRequest.Headers.Get(OAUTH_HEADER_NAME);
        string[] attrs = oAuthHeader.Split(',');

        foreach (string attr in attrs)
        {
          string[] pair = attr.Trim().Split('=');
          keyRing.Add(pair[0], pair[1]);

          if (pair[0].ToLower() == "bechtelusername")
          {
            keyRing.Add("Name", pair[1].ToLower());
          }
        }
      }

      return keyRing;
    }
  }
}
