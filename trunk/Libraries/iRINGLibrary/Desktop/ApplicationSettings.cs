using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StaticDust.Configuration;

namespace org.iringtools.library
{
  public class ApplicationSettings
  {
    public ApplicationSettings(string projectName, string applicationName)
    {
      ProjectName = projectName;
      ApplicationName = applicationName;

      BindingConfigurationPath =  "BindingConfiguration." + ProjectName + "." + ApplicationName + ".xml";
      
    }

    public string ApplicationName { get; set; }
    public string ProjectName { get; set; }
    public string BindingConfigurationPath { get; set; }
    public AppSettingsReader Configuration { get; set; }
  }
}
