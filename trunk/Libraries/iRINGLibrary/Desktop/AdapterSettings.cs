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
      this.Add("InterfaceService",        @"http://localhost/InterfaceService/sparql/");
      this.Add("ReferenceDataServiceUri", @"http://showroom.iringsandbox.org/RefDataService/Service.svc");
      this.Add("DefaultProjectionFormat", "xml");
      this.Add("EndpointTimeout",         "30000");
      this.Add("DBServer",                @".\SQLEXPRESS");
      this.Add("DBName",                  "InterfaceDb");
      this.Add("DBUser",                  "dotNetRDF");
      this.Add("DBPassword",              "dotNetRDF");
      this.Add("TrimData",                "False");
      this.Add("ExecutingAssemblyName",   "App_Code");

      if (OperationContext.Current != null)
      {
        var baseAddress = OperationContext.Current.Host.BaseAddresses[0];
        this.Add("GraphBaseUri", baseAddress.ToString());
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
