using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iRINGTools.Services;

namespace iRINGTools.Base
{
  public class ServiceSettings
  {
    public string BaseDirectoryPath { get; set; }

    public string EncryptedProxyToken { get; set; }

    public string ProxyHost { get; set; }

    public string ProxyPort { get; set; }
  }
}

namespace iRINGTools.Adapter
{
  public class AdapterServiceSettings : ServiceSettings
  {
    public string XmlPath { get; set; }

    public string ModelDTOPath { get; set; }

    public string IdsPath { get; set; }

    public string InterfaceServer { get; set; }

    public bool TrimData { get; set; }

    public bool UseSemweb { get; set; }

    public string TripleStoreConnectionString { get; set; }

    public string EncryptedToken { get; set; }

    public string TransformPath { get; set; }

    public string DataLayerConfigPath { get; set; }
  }
}
