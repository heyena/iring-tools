using System;
using System.Collections.Specialized;
using org.iringtools.utility;

namespace org.iringtools.library
{
  public class ServiceSettings : NameValueCollection
  {
    public ServiceSettings()
    {
      this.Add("BaseDirectoryPath",     AppDomain.CurrentDomain.BaseDirectory);
      this.Add("ExecutingAssemblyName", "App_Code");
      this.Add("XmlPath",               @".\XML\");
      this.Add("ProxyCredentialToken",  String.Empty);
      this.Add("ProxyHost",             String.Empty);
      this.Add("ProxyPort",             String.Empty);
      this.Add("IgnoreSslErrors",       "True");
    }

    //Append Web.config settings
    public void AppendSettings(NameValueCollection settings)
    {
      foreach (string s in settings.AllKeys)
      {
        //Override existing settings, and create new ones
        this.Set(s, settings[s]);
      }
    }

    public WebProxyCredentials GetProxyCredentials()
    {
      WebProxyCredentials proxyCredentials = null;

      if (this["ProxyCredentialToken"] == String.Empty)
      {
        proxyCredentials = new WebProxyCredentials();
      }
      else
      {
        int portNumber = 8080;
        Int32.TryParse(this["ProxyPort"], out portNumber);

        proxyCredentials = new WebProxyCredentials(
          this["ProxyCredentialToken"],
          this["ProxyHost"], 
          portNumber);

        proxyCredentials.Decrypt();
      }

      return proxyCredentials;
    }
  }
  
}
