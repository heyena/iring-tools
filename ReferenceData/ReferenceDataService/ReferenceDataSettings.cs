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
    public ReferenceDataSettings()
      : base()
    {
      this.Add("SparqlPath",              @".\SPARQL\");
      this.Add("PageSize",                "100");
      this.Add("ClassRegistryBase",       @"http://rdl.rdlfacade.org/data#");
      this.Add("TemplateRegistryBase",    @"http://tpl.rdlfacade.org/data#");
      this.Add("ExampleRegistryBase",     @"http://example.org/data#");
      this.Add("UseExampleRegistryBase",  "False");
      this.Add("RegistryCredentialToken", String.Empty);
    }
  }
}
