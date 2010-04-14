using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QMXFGenerator
{
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3074")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://ns.ids-adi.org/registry/schema#")]
  [System.Xml.Serialization.XmlRootAttribute(ElementName = "result", Namespace = "http://ns.ids-adi.org/registry/schema#", IsNullable = false)]
  public partial class RegistryResult
  {

    private string registryidField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("registry-id")]
    public string registryid
    {
      get
      {
        return this.registryidField;
      }
      set
      {
        this.registryidField = value;
      }
    }
  }
}
