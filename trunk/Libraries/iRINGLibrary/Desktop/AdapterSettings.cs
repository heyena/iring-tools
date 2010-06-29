using System.ServiceModel;
using org.iringtools.library;
using System.Collections.Specialized;

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
      this.Add("DBName",                  "dotNetRDF");
      this.Add("DBUser",                  "dotNetRDF");
      this.Add("DBPassword",              "dotNetRDF");
      this.Add("TrimData",                "False");
      this.Add("BinaryPath",              @".\Bin\");
      this.Add("CodePath",                @".\App_Code\");

      if (OperationContext.Current != null)
      {
        var baseAddress = OperationContext.Current.Host.BaseAddresses[0];
        this.Add("GraphBaseUri", baseAddress.ToString());
      }
      else
      {
        this.Add("GraphBaseUri", @"http://yourcompany.com");
      }
    }    
  }  
}
