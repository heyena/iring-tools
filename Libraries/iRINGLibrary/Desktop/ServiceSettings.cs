using System;
using System.Collections.Specialized;
using System.Linq;
using org.iringtools.utility;
using StaticDust.Configuration;

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

    public void AppendSettings(NameValueCollection settings)
    {
      foreach (string s in settings.AllKeys)
      {
        if (this.AllKeys.Contains(s))
        {
          this[s] = settings[s];
        }
        else
        {
          this.Add(s, settings[s]);
        }
      }
    }

    public void AppendSettings(AppSettingsReader settings)
    {
      foreach (string s in settings.Keys)
      {
        if (this.AllKeys.Contains(s))
        {
          this[s] = settings[s].ToString();
        }
        else
        {
          this.Add(s, settings[s].ToString());
        }
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
