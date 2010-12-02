﻿using System;
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
      this.Add("InterfaceService", @"http://localhost:54321/facade/query");
      this.Add("ReferenceDataServiceUri", @"http://localhost:54321/refdata");
      this.Add("DefaultProjectionFormat", "xml");
      this.Add("EndpointTimeout", "30000");
      this.Add("dotNetRDFServer", @".\SQLEXPRESS");
      this.Add("dotNetRDFCatalog", "InterfaceDb");
      this.Add("dotNetRDFUser", "dotNetRDF");
      this.Add("dotNetRDFPassword", "dotNetRDF");
      this.Add("TrimData", "False");
      this.Add("ExecutingAssemblyName", "App_Code");
    }

    //Append Scope specific {projectName}.{appName}.config settings.
    public void AppendSettings(AppSettingsReader settings)
    {
      foreach (string key in settings.Keys)
      {
        if (key.Equals("BaseAddress"))
        {
          string baseAddress = settings[key].ToString();
          
          if (!baseAddress.EndsWith("/"))
            baseAddress = baseAddress + "/";
          
          settings[key] = baseAddress;
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
