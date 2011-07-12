﻿using System;
using System.Security.Principal;
using System.ServiceModel;
using Ninject;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel.Web;
using System.Web;
using System.Text;

namespace org.iringtools.adapter.identity
{
  public class SSOIdentityProvider : IIdentityLayer
  {
    private const string OAUTH_HEADER_NAME = "Auth";

    public IDictionary GetKeyRing()
    {
      IDictionary keyRing = new Dictionary<string, string>();

      keyRing.Add("Provider", "SSOIdentityProvider");

      if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers.Count > 0)
      {
        string oAuthHeader = WebOperationContext.Current.IncomingRequest.Headers.Get(OAUTH_HEADER_NAME);
        string[] attrs = oAuthHeader.Split('&');

        foreach (string attr in attrs)
        {
          string[] pair = attr.Trim().Split('=');
          keyRing.Add(pair[0], HttpUtility.UrlDecode(pair[1], Encoding.UTF8));

          if (pair[0].ToLower() == "bechtelusername")
          {
            keyRing.Add("UserName", pair[1].ToLower());
          }
        }
      }

      if (!keyRing.Contains("UserName") && keyRing.Contains("EMailAddress"))
      {
        keyRing.Add("UserName", keyRing["EMailAddress"]);
      }

      return keyRing;
    }
  }
}
