using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;
using System.Collections.Specialized;

namespace org.ids_adi.iring.referenceData
{
    public class ReferenceDataSettings : ServiceSettings
    {
        public ReferenceDataSettings(NameValueCollection AppSettings)
        {
            this.BaseDirectoryPath = AppSettings["BaseDirectory"];
            this.SPARQLPath = AppSettings["SPARQLPath"];
            this.XMLPath = AppSettings["XMLPath"];
            this.PageSize = AppSettings["PageSize"];
            this.ClassRegistryBase = AppSettings["ClassRegistryBase"];
            this.TemplateRegistryBase = AppSettings["TemplateRegistryBase"];
            this.ExampleRegistryBase = AppSettings["ExampleRegistryBase"];
            this.UseExampleRegistryBase = Convert.ToBoolean(AppSettings["UseExampleRegistryBase"]);
            this.RegistryCredentialToken = AppSettings["RegistryCredentialToken"];
            this.ProxyCredentialToken = AppSettings["ProxyCredentialToken"];
            this.ProxyHost = AppSettings["ProxyHost"];
            this.ProxyPort = AppSettings["ProxyPort"];
        }

        //public string BaseDirectoryPath { get; set; }
        public string ClassRegistryBase { get; set; }
        public string TemplateRegistryBase { get; set; }
        public string ExampleRegistryBase { get; set; }
        public bool UseExampleRegistryBase { get; set; }
        public string SPARQLPath { get; set; }
        public string XMLPath { get; set; }
        public string RegistryCredentialToken { get; set; }
        public string ProxyCredentialToken { get; set; }
        //public string ProxyHost { get; set; }
        //public string ProxyPort { get; set; }
        public string PageSize { get; set; }
        public string IgnoreSslErrors { get; set; }
    }
}
