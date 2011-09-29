using System;
using System.Security.Principal;
using System.ServiceModel;
using Ninject;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel.Web;
using System.Web;
using System.Text;
using log4net;
using System.Net;

namespace org.iringtools.adapter.identity
{
  public class SSOIdentityProvider : IIdentityLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SSOIdentityProvider));
    
    public IDictionary GetKeyRing()
    {
      IDictionary keyRing = new Dictionary<string, string>();

      keyRing.Add("Provider", "SSOIdentityProvider");
      if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers.Count > 0)
      {
        WebHeaderCollection headers = WebOperationContext.Current.IncomingRequest.Headers;

        /*
         * Available headers from Apigee:
         * 
         *  X-myPSN-AccessToken = [a valid token]
         *  X-myPSN-UserID = [a valid guid]
         *  X-myPSN-BechtelUserName = [a valid BUN]
         *  X-myPSN-BechtelDomain = [domain]
         *  X-myPSN-BechtelEmployeeNumber = [value]
         *  X-myPSN-IsBechtelEmployee = [bool]
         *  X-myPSN-EmailAddress = [value]**
         *  X-myPSN-UserAttributes = [Full JSON Payload]
         */

        _logger.Debug("OAuth headers: ");

        string accessToken = headers.Get("X-myPSN-AccessToken");
        _logger.Debug("X-myPSN-AccessToken [" + accessToken + "]");
        if (!String.IsNullOrEmpty(accessToken))
          keyRing.Add("AccessToken", accessToken);

        string emailAddress = headers.Get("X-myPSN-EmailAddress");
        _logger.Debug("X-myPSN-EmailAddress [" + emailAddress + "]");
        if (!String.IsNullOrEmpty(emailAddress) && !keyRing.Contains("UserName"))
          keyRing.Add("UserName", emailAddress);

        string userId = headers.Get("X-myPSN-UserID");
        _logger.Debug("X-myPSN-UserID [" + userId + "]");
        if (!String.IsNullOrEmpty(userId))
          keyRing.Add("UserId", userId);

        string isBechtelEmployee = headers.Get("X-myPSN-IsBechtelEmployee");
        _logger.Debug("X-myPSN-IsBechtelEmployee [" + isBechtelEmployee + "]");

        if (!String.IsNullOrEmpty(isBechtelEmployee) &&
          (isBechtelEmployee.ToLower() == "true" || isBechtelEmployee.ToLower() == "yes"))
        {
          string bechtelUserName = headers.Get("X-myPSN-BechtelUserName");
          _logger.Debug("X-myPSN-BechtelUserName [" + bechtelUserName + "]");
          if (!String.IsNullOrEmpty(bechtelUserName))
            keyRing.Add("BechtelUserName", bechtelUserName);

          string bechtelDomain = headers.Get("X-myPSN-BechtelDomain");
          _logger.Debug("X-myPSN-BechtelDomain [" + bechtelDomain + "]");
          if (!String.IsNullOrEmpty(bechtelDomain))
            keyRing.Add("BechtelDomain", bechtelDomain);

          string bechtelEmployeeNumber = headers.Get("X-myPSN-BechtelEmployeeNumber");
          _logger.Debug("X-myPSN-BechtelEmployeeNumber [" + bechtelEmployeeNumber + "]");
          if (!String.IsNullOrEmpty(bechtelEmployeeNumber))
            keyRing.Add("BechtelEmployeeNumber", bechtelEmployeeNumber);
        }

        string userAttrs = headers.Get("X-myPSN-UserAttributes");
        _logger.Debug("X-myPSN-UserAttributes [" + userAttrs + "]");
      }

      return keyRing;
    }
  }
}
