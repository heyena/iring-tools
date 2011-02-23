using System;
using System.Linq;
using System.ServiceModel;
using org.iringtools.library;
using StaticDust.Configuration;
using org.iringtools.utility;
using org.iringtools.mapping;

namespace org.iringtools.adapter
{
  public class AdapterSettings : ServiceSettings
  {
    public AdapterSettings() : base()
    {
      this.Add("InterfaceService", @"http://localhost/services/facade/query");
      this.Add("ReferenceDataServiceUri", @"http://localhost/services/refdata");
      this.Add("DefaultProjectionFormat", "xml");
      this.Add("EndpointTimeout", "30000");
      this.Add("dotNetRDFServer", @".\SQLEXPRESS");
      this.Add("dotNetRDFCatalog", "InterfaceDb");
      this.Add("dotNetRDFUser", "dotNetRDF");
      this.Add("dotNetRDFPassword", "dotNetRDF");
      this.Add("TrimData", "False");
      this.Add("DumpSettings", "False");
      this.Add("PrimaryClassificationStyle", "Type");
      this.Add("SecondaryClassificationStyle", "Template");
      this.Add("ClassificationTemplateFile", ".\\XML\\ClassificationTemplate.xml");
      this.Add("ExecutingAssemblyName", "App_Code");

      if (OperationContext.Current != null)
      {
        string baseAddress = OperationContext.Current.Host.BaseAddresses[0].ToString();

        if (!baseAddress.EndsWith("/"))
            baseAddress = baseAddress + "/";

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
      foreach (string key in settings.Keys)
      {
        if (key.Equals("GraphBaseUri"))
        {
          string baseAddress = settings[key].ToString();
          
          if (!baseAddress.EndsWith("/"))
            baseAddress = baseAddress + "/";
          
          settings[key] = baseAddress;
        }

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

        //Protect existing settings, but add new ones.
        if (!this.AllKeys.Contains(key, StringComparer.CurrentCultureIgnoreCase))
        {
          this.Add(key, settings[key].ToString());
        }
      }
    }
  }  
}
