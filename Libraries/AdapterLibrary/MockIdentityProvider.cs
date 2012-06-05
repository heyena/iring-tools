using System;
using System.Security.Principal;
using System.ServiceModel;
//using Ninject;
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
  public class MockIdentityProvider : IIdentityLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MockIdentityProvider));
    
    public IDictionary GetKeyRing()
    {
      IDictionary keyRing = new Dictionary<string, string>();

      keyRing.Add("Provider", "MockIdentityProvider");

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

      string accessToken = "621cb78f3581e4e121e2c9bb5009e655";
      string userId = "784cc0ae-8b2e-43fc-8de7-31863958b2d8";
      string bechtelUserName = "rpdecarl";
      string bechtelDomain = "IAMERS";
      string bechtelEmployeeNumber = "12345";
      string isBechtelEmployee = "1";
      string emailAddress = "rpdecarl@bechtel.com";
      //string userAttrs = 
      //  "{" + 
      //    "\"token_type\":\"urn:pingidentity.com:oauth2:validated_token\"," + 
      //    "\"expires_in\":4712," + 
      //    "\"client_id\":null," + 
      //    "\"access_token\": " + 
      //      "{" + 
      //        "\"GBU\":\"Corporate\"," + 
      //        "\"subject\":\"784cc0ae-8b2e-43fc-8de7-31863958b2d8\"," + 
      //        "\"BechtelEmployeeNumber\":\"12345\"," + 
      //        "\"Function\":\"Information Systems & Technology\"," + 
      //        "\"BechtelEmailAddress\":\"jdoe@bechtel.com\"," + 
      //        "\"WorkPhoneNumber\":\"865-555-1234\"," + 
      //        "\"LastName\":\"Doe\"," + 
      //        "\"Title\":\"Software Engineering Senior\"," + 
      //        "\"BechtelUserName\":\"JDOE\"," + 
      //        "\"FirstName\":\"John\"," + 
      //        "\"Project\":\"Not On Project\"," + 
      //        "\"IsBechtelEmployee\":\"1\"," + 
      //        "\"BechtelDomain\":\"IAMERS\"," + 
      //        "\"EMailAddress\":\"jdoe@bechtel.com\"" + 
      //      "}" + 
      //  "}";
      string userAttrs =
        "{" +
          "\"GBU\":\"Corporate\"," +
          "\"LoginName\":\"jdoe\"," +
          "\"subject\":\"784cc0ae-8b2e-43fc-8de7-31863958b2d8\"," +
          "\"BechtelEmployeeNumber\":\"12345\"," +
          "\"Function\":\"Information Systems & Technology\"," +
          "\"BechtelEmailAddress\":\"jdoe@bechtel.com\"," +
          "\"WorkPhoneNumber\":\"865-555-1234\"," +
          "\"LastName\":\"Doe\"," +
          "\"sessionid\":\"AaCpkquisElqOwxnjI3aw7wMG6N\"," +
          "\"Title\":\"Software Engineering Senior\"," +
          "\"authnCtx\":\"urn:oasis:names:tc:SAML:2.0:ac:classes:unspecified;\"," +
          "\"MobilePhoneNumber\":\"865-555-1234\"," +
          "\"partnerEntityID\":\"PSN2-SAML2-Entity\"," +
          "\"BechtelUserName\":\"JDOE\"," +
          "\"FirstName\":\"John\"," +
          "\"Project\":\"Not On Project\"," +
          "\"IsBechtelEmployee\":\"1\"," +
          "\"BechtelDomain\":\"IAMERS\"," +
          "\"authnInst\":\"2011-11-29 17:04:53-0500;\"," +
          "\"EMailAddress\":\"jdoe@bechtel.com\"" +
        "}";

      JavaScriptSerializer desSSO = new JavaScriptSerializer();
      desSSO.MaxJsonLength = int.MaxValue;
      keyRing = desSSO.Deserialize<Dictionary<string, string>>(userAttrs);

      keyRing.Add("X-myPSN-AccessToken", accessToken);
      keyRing.Add("AccessToken", accessToken);

      keyRing.Add("X-myPSN-UserID", userId);
      keyRing.Add("X-myPSN-EmailAddress", emailAddress);
      keyRing.Add("X-myPSN-IsBechtelEmployee", isBechtelEmployee);

      if (isBechtelEmployee.ToLower() == "true" || isBechtelEmployee.ToLower() == "1")
      {
        keyRing.Add("X-myPSN-BechtelUserName", bechtelUserName);
        keyRing.Add("UserName", bechtelUserName);

        keyRing.Add("X-myPSN-BechtelDomain", bechtelDomain);
        keyRing.Add("X-myPSN-BechtelEmployeeNumber", bechtelEmployeeNumber);       
      }
      else
      {
        keyRing.Add("UserName", emailAddress);
      }

      return keyRing;
    }
  }
}
