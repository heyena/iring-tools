using System;
using System.Collections.Specialized;
using org.iringtools.utility;
using System.Net;
using System.ServiceModel;
using org.iringtools.mapping;

namespace org.iringtools.library
{
  public class ServiceSettings : NameValueCollection
  {
    public ServiceSettings()
    {
      this.Add("BaseDirectoryPath", AppDomain.CurrentDomain.BaseDirectory);
      this.Add("XmlPath", @".\XML\");
      this.Add("DataPath", @".\App_Data\");
      this.Add("ProxyCredentialToken", String.Empty);
      this.Add("ProxyHost", String.Empty);
      this.Add("ProxyPort", String.Empty);
      this.Add("IgnoreSslErrors", "True");
      this.Add("PrimaryClassificationStyle", "Type");
      this.Add("SecondaryClassificationStyle", "Template");
      this.Add("ClassificationTemplateFile", @".\XML\ClassificationTemplate.xml");

      if (OperationContext.Current != null)
      {
        string baseAddress = OperationContext.Current.Host.BaseAddresses[0].ToString();

        if (!baseAddress.EndsWith("/"))
          baseAddress = baseAddress + "/";

        this.Add("BaseAddress", baseAddress);
      }
      else
      {
        this.Add("BaseAddress", @"http://www.example.com/");
      }
    }

    //Append Web.config settings
    public void AppendSettings(NameValueCollection settings)
    {
      foreach (string key in settings.AllKeys)
      {
        if (key.Equals("PrimaryClassificationStyle") || key.Equals("SecondaryClassificationStyle"))
        {
          if (key.Equals("PrimaryClassificationStyle") && settings[key].ToString() == ClassificationStyle.Template.ToString())
            throw new Exception("Primary Classification Style value can only be 'Type' or 'Both'!");

          if (settings[key].ToString().ToUpper() == ClassificationStyle.Type.ToString().ToUpper() ||
              settings[key].ToString().ToUpper() == ClassificationStyle.Template.ToString().ToUpper() ||
              settings[key].ToString().ToUpper() == ClassificationStyle.Both.ToString().ToUpper())
          {
            this[key] = Utility.TitleCase(settings[key].ToString());  // override the default
          }
        }
        else
        {
          //Override existing settings, and create new ones
          this.Set(key, settings[key]);
        }
      }
    }

    public NetworkCredential GetProxyCredential()
    {
      return GetWebProxyCredentials().GetNetworkCredential();
    }

    public WebProxyCredentials GetWebProxyCredentials()
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
