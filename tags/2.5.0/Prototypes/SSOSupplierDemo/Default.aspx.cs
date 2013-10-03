using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Net;
using System.Text;

namespace SSOSupplierDemo
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            IDictionary  allKeys = null;

            if (Session["authenticatedUserName"] == null)
            {
                string authenticatedUserName = SSO.SSOHandler.EnsureAuthenticationAndReturnUserName(ref allKeys);
                lblInfo.Text = "Welcome " + authenticatedUserName;

                string token = HttpContext.Current.Request.QueryString["opentoken"];
                lbInfo.Items.Add("OpenToken: " + token);

                foreach (DictionaryEntry entry in allKeys)
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
                            lbInfo.Items.Add(entry.Key.ToString() + ": " + entry.Value.ToString());
                            break;
                    }
                }

                Session["OAuthHeaderKey"] = GetOAuthHeader();
                Session["authenticatedUserName"] = authenticatedUserName;
            }

            Response.Redirect("LoggedIn.aspx");
        }

        protected string GetOAuthHeader()
        {
            string token = HttpContext.Current.Request.QueryString["opentoken"];
            // Create the web request  
            WebClient client = new WebClient();
            //client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);
            client.UseDefaultCredentials = true;

            if (ConfigurationManager.AppSettings["ProxyAddress"].ToString() != "")
            {
                client.Proxy = new WebProxy(ConfigurationManager.AppSettings["ProxyAddress"].ToString(), true);
                client.Proxy.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["ProxyCredentialUserName"].ToString(),
                    ConfigurationManager.AppSettings["ProxyCredentialPassword"].ToString(),
                    ConfigurationManager.AppSettings["ProxyCredentialDomain"].ToString());
            }

            string decodedToken = client.DownloadString(ConfigurationManager.AppSettings["tokenServiceAddress"] + "/XML/Decode?opentoken=" + token);
            //sts_decoded_token.Text = decodedToken;
            XDocument loaded = XDocument.Parse(decodedToken);

            var q = from c in loaded.Descendants()
                    where c.Name.LocalName == "KeyValueOfstringstring"
                    select c;

            string OAuthKey = "";
            string OAuthSecret = "";
            
            foreach (var item in q)
            {
                var xmlns = "{http://schemas.microsoft.com/2003/10/Serialization/Arrays}";

                string key = item.Element(xmlns + "Key").Value;
                string value = item.Element(xmlns + "Value").Value;

                if (key == "serviceKey") OAuthKey = value;
                if (key == "secret") OAuthSecret = value;
            }

            return OAuthHeader(OAuthKey, OAuthSecret);
        }

        private string OAuthHeader(string key, string secret)
        {
            OAuthBase oAuth = new OAuthBase();

            string nonce = oAuth.GenerateNonce();
            string timestamp = oAuth.GenerateTimeStamp();
            Uri url = new Uri("http://www.becweb.com");
            string normedURL = "";
            string requestParams = "";

            string sig = oAuth.GenerateSignature(url, key, secret, "", "", "GET", timestamp, nonce, out normedURL, out requestParams);

            sig = HttpUtility.UrlEncode(sig);

            StringBuilder sb = new StringBuilder("OAuth realm=\"\"");
            sb.AppendFormat(",oauth_consumer_key=\"{0}\"", key);
            sb.AppendFormat(",oauth_nonce=\"{0}\"", nonce);
            sb.AppendFormat(",oauth_timestamp=\"{0}\"", timestamp);
            sb.AppendFormat(",oauth_signature_method=\"{0}\"", "HMAC-SHA1");
            sb.AppendFormat(",oauth_version=\"{0}\"", "1.0");
            sb.AppendFormat(",oauth_signature=\"{0}\"", sig);

            string header = sb.ToString();

            return header;
        }
    }
}
