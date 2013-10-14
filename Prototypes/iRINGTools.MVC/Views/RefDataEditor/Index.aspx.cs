using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


public partial class ReferenceDataEditor : System.Web.Mvc.ViewPage<System.Web.UI.Page>
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Load all Web.Config AppSettings and ConnectionStrings
            string configString = Server.MapPath("~/Web.Config").GetConfigString();

            // Load application specific variables
            IDictionary<string, string> configItems = new Dictionary<string, string>();
            configItems.Add("WebServerURL", string.Format("http://{0}", Request.Url.Authority));
            configItems.Add("WebServerPath", Server.MapPath("~"));
            configItems.Add("WebSiteName", Request.Url.AbsolutePath
                .Replace("/default.aspx", "").Replace("/Default.aspx", "").Substring(0));

            // Set InitParameters for Silverlight use
            Xaml1.InitParameters = string.Format("InitParameters={0}~{1}",
                configString, configItems.GetConfigString());
        }
    }

