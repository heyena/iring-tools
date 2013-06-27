using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class MappingEditor1 : System.Web.Mvc.ViewPage<System.Web.UI.Page>
{
  /// <summary>
  /// The following code is blogged about on the following link:
  /// http://www.global-webnet.net/blogengine/post/2009/01/12/Passing-Server-information-to-Silverlight-Client.aspx
  /// 
  /// Handles the Load event of the Page control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
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
