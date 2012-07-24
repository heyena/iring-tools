using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace SSOSupplierDemo
{
    public partial class Page2 : System.Web.UI.Page
    {
        public string userNameForJQuery;

        protected void Page_Load(object sender, EventArgs e)
        {
            IDictionary  allKeys = null;

            string authenticatedUserName = SSO.SSOHandler.EnsureAuthenticationAndReturnUserName(ref allKeys);
            lblInfo.Text = "Welcome " + authenticatedUserName;

            foreach (DictionaryEntry entry in allKeys)
            {
                lbInfo.Items.Add(entry.Key.ToString() + ": " + entry.Value.ToString());
            }

            userNameForJQuery = authenticatedUserName;
        }

        protected void btnCallService_Click(object sender, EventArgs e)
        {
            WebClient web = new WebClient();
            string token = web.DownloadString(ConfigurationManager.AppSettings["tokenServiceAddressSS"] +
                "/GetToken?username=" + userNameForJQuery);

            token = token.Trim('"');

            web.Headers.Add("From", token);
            txtServiceResponse.Text = web.DownloadString(ConfigurationManager.AppSettings["webServiceAddressSS"] +
                "/Hello?World=Bob");        
        }

    }
}
