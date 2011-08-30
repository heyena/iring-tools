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
using System.Web.Script.Serialization;

namespace org.iringtools.adapter.identity
{
  public class SSOIdentityProvider : IIdentityLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SSOIdentityProvider));
    
    public IDictionary GetKeyRing()
    {
      IDictionary keyRing = new Dictionary<string, string>();

      if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers.Count > 0)
      {
        WebHeaderCollection headers = WebOperationContext.Current.IncomingRequest.Headers;

        _logger.Debug("OAuth headers: ");

        string userAttrs = headers.Get("X-myPSN-UserAttributes");
        if (!String.IsNullOrEmpty(userAttrs))
        {
          JavaScriptSerializer desSSO = new JavaScriptSerializer();
          keyRing = desSSO.Deserialize<Dictionary<string, string>>(userAttrs);

          if (keyRing.Contains("BechtelEmailAddress"))
          {
            keyRing.Add("UserName", keyRing["BechtelEmailAddress"]);
          }
          else
          {
            keyRing.Add("UserName", keyRing["EMailAddress"]);
          }
        }
        _logger.Debug("X-myPSN-UserAttributes [" + userAttrs + "]");

        string accessToken = headers.Get("X-myPSN-AccessToken");
        _logger.Debug("X-myPSN-AccessToken [" + accessToken + "]");
        if (!String.IsNullOrEmpty(accessToken))
          keyRing.Add("AccessToken", accessToken);

        string emailAddress = headers.Get("X-myPSN-EmailAddress");
        _logger.Debug("X-myPSN-EmailAddress [" + emailAddress + "]");
        if (!String.IsNullOrEmpty(emailAddress))
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
      }

      keyRing.Add("Provider", "SSOIdentityProvider");

      return keyRing;
    }
  }
}
