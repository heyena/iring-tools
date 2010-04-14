using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace org.ids_adi.iring.referenceData
{
  public class ConfigSettings
  {
    public string BaseDirectoryPath { get; set; }

    public string ClassRegistryBase { get; set; }

    public string TemplateRegistryBase { get; set; }

    public string ExampleRegistryBase { get; set; }

    public bool UseExampleRegistryBase { get; set; }

    public string SPARQLPath { get; set; }

    public string XMLPath { get; set; }

    public string RegistryCredentialToken { get; set; }

    public string ProxyCredentialToken { get; set; }

    public string ProxyHost { get; set; }

    public string ProxyPort { get; set; }

    public string PageSize { get; set; }

    public string IgnoreSslErrors { get; set; }
  }
}
