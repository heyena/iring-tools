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

namespace SSOSupplierDemo
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            IDictionary  allKeys = null;

            string authenticatedUserName = SSO.SSOHandler.EnsureAuthenticationAndReturnUserName(ref allKeys);
            lblInfo.Text = "Welcome " + authenticatedUserName;

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


        }
    }
}
