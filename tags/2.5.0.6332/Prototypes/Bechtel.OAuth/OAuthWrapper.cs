using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.ServiceModel.Web;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;
using System.Web;

namespace Bechtel.WebServices
{
    /// <summary>
    /// Classes for dealing with OAuth for web services.
    /// 
    /// Requires a web operation context...wcf use only
    /// To enable add app settings:
    /// <add key="sso_enabled" value="true"/>
    /// <add key="restTokenAddress" value="https://theTokenService"/>
    /// </summary>
    public class OAuth
    {
        private HttpContext mContext;
        private Dictionary<string, string> attributes;

        public OAuth(System.Web.HttpContext context)
        {
            mContext = context;
            attributes = UserDetails();
        }

        /// <summary>
        /// Determines whether the request has a valid OAuth token 
        /// </summary>
        /// <returns></returns>
        public bool RequestIsValid()
        {
            bool sso_enabled = Convert.ToBoolean(ConfigurationManager.AppSettings["sso_enabled"]);

            if (sso_enabled && attributes.Count == 0)
            {
                return false;
            }

            return true;

        }

        private void checkRequest()
        {
            bool sso_enabled = Convert.ToBoolean(ConfigurationManager.AppSettings["sso_enabled"]);

            if (sso_enabled)
            {
                string header = mContext.Request.Headers["Authorization"];
            string restTokenAddress = ConfigurationManager.AppSettings["restTokenAddress"];

            DataContractJsonSerializer dictionarySerializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
            WebClient client = new WebClient();
            client.UseDefaultCredentials = true;

            string xmlToken = client.DownloadString(restTokenAddress + "/JSON/DecodeOAuthHeader?header=" + header);
            MemoryStream str = new MemoryStream(Encoding.Unicode.GetBytes(xmlToken));
            str.Position = 0;
            Dictionary<string, string> result = (Dictionary<string, string>)dictionarySerializer.ReadObject(str);
            }
        }

        /// <summary>
        /// Gets a dictionary of user details associated with the OAuth token.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> UserDetails()
        {
            return attributes;

        }
    }
}
