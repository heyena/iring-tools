﻿using System;
using System.Linq;
using System.ServiceModel;
using org.iringtools.library;
using StaticDust.Configuration;
using org.iringtools.utility;
using System.Collections;
using log4net;

namespace org.iringtools.adapter
{
  public class AdapterSettings : ServiceSettings
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterSettings));

    public AdapterSettings()
      : base()
    {
      this.Add("InterfaceService", @"http://localhost/services/facade/query");
      this.Add("ReferenceDataServiceUri", @"http://localhost/services/refdata");
      this.Add("EndpointTimeout", "30000");
      this.Add("dotNetRDFServer", @".\SQLEXPRESS");
      this.Add("dotNetRDFCatalog", "FacadeDb");
      this.Add("dotNetRDFUser", "dotNetRDF");
      this.Add("dotNetRDFPassword", "dotNetRDF");
      this.Add("TrimData", "False");
      this.Add("DumpSettings", "False");
      this.Add("ExecutingAssemblyName", "App_Code");
      this.Add("DefaultStyleSheet", @".\App_Data\default.css");
      this.Add("ValidateLinks", "False");
      this.Add("DisplayLinks", "False");
      this.Add("ShowJsonNullValues", "False");
      this.Add("MaxThreads", "32");
      this.Add("SpCharList", System.Configuration.ConfigurationManager.AppSettings["SpCharList"]);
      this.Add("SpCharValue", System.Configuration.ConfigurationManager.AppSettings["SpCharValue"]);

      if (OperationContext.Current != null)
      {
        string baseAddress = OperationContext.Current.Host.BaseAddresses[0].ToString();

        if (!baseAddress.EndsWith("/"))
            baseAddress = baseAddress + "/";

        this.Add("GraphBaseUri", baseAddress);
      }
      else
      {
        this.Add("GraphBaseUri", @"http://localhost:54321/data");
        //this.Add("GraphBaseUri", @"http://yourcompany.com/");
      }
    }

    //Append Scope specific {projectName}.{appName}.config settings.
    public void AppendSettings(AppSettingsReader settings)
    {
      try
      {
        if (settings != null && settings.Keys != null)
        {
          foreach (string key in settings.Keys)
          {
            if (key.Equals("GraphBaseUri"))
            {
              string baseAddress = settings[key].ToString();

              if (!baseAddress.EndsWith("/"))
                baseAddress = baseAddress + "/";

              this[key] = baseAddress;

              continue;
            }

            // list of overrideable settings
            if (key.Equals("DefaultProjectionFormat") ||
                key.Equals("ValidateLinks") ||
                key.Equals("DisplayLinks") ||
                key.Equals("ShowJsonNullValues") ||
                key.Equals("DefaultPageSize") ||
                key.Equals("MultiGetDTIs") ||
                key.Equals("MultiGetDTOs") ||
                key.Equals("MultiPostDTOs") ||
                key.Equals("MaxThreads") ||
                key.Equals("CachePageSize"))
                
            {
              string format = settings[key].ToString();
              this[key] = format;

              continue;
            }

            //Protect existing and identity settings, but add new ones.
            string lowerCaseKey = key.ToLower();

            if (!(this.AllKeys.Contains(key, StringComparer.CurrentCultureIgnoreCase) ||
                lowerCaseKey == "username" || lowerCaseKey == "domain" ||
                lowerCaseKey == "emailaddress"))
            {
              this.Add(key, settings[key].ToString());
            }
            else if (this[key] == String.Empty)
            {
              this[key] = settings[key].ToString();
            }
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error appending settings: " + e);
      }
    }

    //Append KeyRing from IdentityProvider.
    public void AppendKeyRing(IDictionary keyRing)
    {
      if (keyRing != null)
      {
        foreach (string key in keyRing.Keys)
        {
          object valueObj = keyRing[key];

          string value = String.Empty;
          if (valueObj != null)
            value = valueObj.ToString();

          //Protect existing settings, but add new ones.
          if (!this.AllKeys.Contains(key, StringComparer.CurrentCultureIgnoreCase))
          {
            this.Add(key, value);
          }
        }
      }
    }
  }  
}
