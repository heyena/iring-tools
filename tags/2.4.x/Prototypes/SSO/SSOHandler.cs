using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using opentoken;
using opentoken.util;
using System.Configuration;
using System.Web.Configuration;

namespace SSO
{
    public static class SSOHandler
    {
        const string LOGON_COOKIE_NAME = "Auth";

        public static string EnsureAuthenticationAndReturnUserName(ref IDictionary allClaims)
        {
            string authenticatedUserName = "";

            if (HttpContext.Current.Request.Cookies[LOGON_COOKIE_NAME] != null)
            {
                allClaims = new Dictionary<String, String>();

                HttpCookie authCookie = HttpContext.Current.Request.Cookies[LOGON_COOKIE_NAME];

                NameValueCollection col = new NameValueCollection(authCookie.Values);
                String[] keyNames = col.AllKeys;
                foreach (string key in keyNames)
                {
                    allClaims.Add(key, col.GetValues(key)[0]);
                }

                authenticatedUserName = authCookie["subject"].ToString();
            }
            else
            {
                    string rootPath = HttpContext.Current.Server.MapPath("~");
                    Agent openTokenAgent = new Agent(rootPath + "sp-agent-config.config");

                    IDictionary userInfo = openTokenAgent.ReadToken(HttpContext.Current.Request);

                    if (null != userInfo)
                    {
                        HttpCookie authCookie = new HttpCookie(LOGON_COOKIE_NAME);

                        foreach (DictionaryEntry entry in userInfo)
                        {
                            switch (entry.Key.ToString())
                            {
                                case "not-before":
                                    break;
                                case "authnContext":
                                    break;
                                case "not-on-or-after":
                                    break;
                                case "renew-until":
                                    break;
                                default:
                                    authCookie.Values[entry.Key.ToString()] = entry.Value.ToString();
                                    break;
                            }
                        }

                        authenticatedUserName = userInfo["subject"].ToString();
                        HttpContext.Current.Response.Cookies.Add(authCookie);
                        allClaims = userInfo;
                    }
                    else
                    {
                        string port = HttpContext.Current.Request.ServerVariables["SERVER_PORT"];
                        if (port == null || port == "80" || port == "443")
                            port = "";
                        else
                            port = ":" + port;

                        string returnPath = HttpContext.Current.Request.Url.Scheme + ":" + "//" +
                            HttpContext.Current.Request.Url.Host + port + 
                            HttpContext.Current.Request.Url.PathAndQuery;

                        //returnPath = HttpContext.Current.Server.UrlEncode(returnPath);

                        string FederationServerAddress = ConfigurationManager.AppSettings["FederationServerAddress"].ToString();
                        string PartnerIdpId = ConfigurationManager.AppSettings["PartnerIdpId"].ToString();
                        string SPFederationEndPoint = ConfigurationManager.AppSettings["SPFederationEndPoint"].ToString();

                        string url = FederationServerAddress + SPFederationEndPoint + "?PartnerIdpId=" + PartnerIdpId +
                            "&TargetResource=" + returnPath;

                        HttpContext.Current.Response.Redirect(url);
                    }
            }
            return authenticatedUserName;
        }
    }
}

