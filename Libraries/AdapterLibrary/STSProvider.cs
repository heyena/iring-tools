using System.Collections.Generic;
using System.Web;
using Ninject;
using System;
using System.Configuration;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;
using System.Text;

namespace org.iringtools.adapter.identity
{
    public class STSProvider : IIdentityLayer
    {
        /// <summary>
        /// Classes for dealing with STS for web services.
        /// 
        /// Requires a web operation context...wcf use only
        /// To enable add app settings:
        /// <add key="sso_enabled" value="true"/>
        /// <add key="restTokenAddress" value will be listed shortly/>
        /// </summary>
        /// 

        private AdapterSettings _settings = null;

        [Inject]
        public STSProvider(AdapterSettings settings)
        {
            _settings = settings;
        }

        public void Initialize()
        {
            bool sso_enabled = Convert.ToBoolean(ConfigurationManager.AppSettings["sso_enabled"]);
            HttpContext context = System.Web.HttpContext.Current;

            if (sso_enabled)
            {
                string header = context.Request.Headers["Authorization"];
                string restTokenAddress = ConfigurationManager.AppSettings["restTokenAddress"];

                DataContractJsonSerializer dictionarySerializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
                WebClient client = new WebClient();
                client.UseDefaultCredentials = true;

                string xmlToken = client.DownloadString(restTokenAddress + "/JSON/DecodeOAuthHeader?header=" + header);
                MemoryStream str = new MemoryStream(Encoding.Unicode.GetBytes(xmlToken));
                str.Position = 0;
                Dictionary<string, string> result = (Dictionary<string, string>)dictionarySerializer.ReadObject(str);
                _settings["UserName"] = result.Values.ToString();
            }
        }
    }
}
