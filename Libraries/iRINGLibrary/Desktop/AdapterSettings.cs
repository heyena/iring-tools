using System;
using System.Linq;
using System.ServiceModel;
using org.iringtools.library;
using StaticDust.Configuration;

namespace org.iringtools.adapter
{
  public class AdapterSettings : ServiceSettings
  {
    public AdapterSettings() : base()
    {
      this.Add("InterfaceService", @"http://localhost/Services/InterfaceService/query");
      this.Add("ReferenceDataServiceUri", @"http://localhost/Services/RefDataService");
      this.Add("DefaultProjectionFormat", "xml");
      this.Add("EndpointTimeout", "30000");
      this.Add("dotNetRDFServer", @".\SQLEXPRESS");
      this.Add("dotNetRDFCatalog", "InterfaceDb");
      this.Add("dotNetRDFUser", "dotNetRDF");
      this.Add("dotNetRDFPassword", "dotNetRDF");
      this.Add("TrimData", "False");
      this.Add("ExecutingAssemblyName", "App_Code");

      if (OperationContext.Current != null)
      {
        string baseAddress = OperationContext.Current.Host.BaseAddresses[0].ToString();

        if (!baseAddress.EndsWith("/"))
        {
          baseAddress = baseAddress + "/";
        }

        this.Add("GraphBaseUri", baseAddress);
      }
      else
      {
        this.Add("GraphBaseUri", @"http://yourcompany.com/");
      }
    }

    //Append Scope specific {projectName}.{appName}.config settings.
    public void AppendSettings(AppSettingsReader settings)
    {
      foreach (string s in settings.Keys)
      {
        //Protect existing settings, but add new ones.
        if (!this.AllKeys.Contains(s, StringComparer.CurrentCultureIgnoreCase))
        {
          this.Add(s, settings[s].ToString());
        }
      }
    }
  }  
}
