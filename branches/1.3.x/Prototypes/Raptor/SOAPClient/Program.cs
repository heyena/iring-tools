using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SOAPClient.DataService;

namespace SOAPClient
{
  class Program
  {
    static void Main(string[] args)
    {
      DataServiceClient client = new DataServiceClient();
      List<DataTransferObject> dtoList = client.GetDataList("applicationName", "Lines", String.Empty, "projectName");

      foreach (DataTransferObject dto in dtoList)
      {
        Lines lineObject = (Lines)dto;

        string tag = lineObject.tpl_PipingNetworkSystemName_identifier;

        Console.WriteLine("tpl:PipingNetworkSystemName.identifier: " + tag);

        Lines.TemplateSystemPipingNetworkSystemAssembly.ClassSystem systemObject =
          lineObject.tpl_SystemPipingNetworkSystemAssembly.hasClassOfWhole_rdl_System;

        Console.WriteLine("tpl:SystemPipingNetworkSystemAssembly.hasClassOfWhole: " + systemObject.Identifier);

        string system = systemObject.tpl_SystemName_identifier;

        Console.WriteLine("tpl:SystemName.identifier: " + system);

        Console.WriteLine();
      }
      Console.ReadKey();
    }
  }
}
