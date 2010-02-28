using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using DemoControlPanel.Web;

namespace Endpoints
{
  class Program
  {
    static void Main(string[] args)
    {
      iRINGEndpoints adapterEndPoints = new iRINGEndpoints
      {
        new iRINGEndpoint{id="1", name = "iRINGSandbox AdapterService", serviceUri = "http://www.iringsandbox.org/AdapterService/Service.svc"},
        new iRINGEndpoint{id="2", name = "Bechtel AdapterService", serviceUri = "https://facade.becpsn.com/AdapterService/Service.svc", 
          credentials = new WebCredentials("8stUFBRIGcYR0Nl8v25rm5+qecZZD6i6itQUBD8zM0bODWZk1TysqXLBvMkc4GVUSCGqS4I9G99Wcynf7Qdqssj5NgAqU7m16EguxwWAScfq8olyMFfjd271RNc35Nv9")},
        new iRINGEndpoint{id="3", name = "CCC AdapterService", serviceUri = "http://c3d.ccc.gr/AdapterService/Service.svc"},
        new iRINGEndpoint{id="4", name = "Hatch AdapterService", serviceUri = "http://iring.hatch.com.au/AdapterService/Service.svc", 
          credentials = new WebCredentials("zlNbMav8dBXUREoOR6ctMZ8aRcOiKmUtgU9mmyhyNqxJ0EAePsEoRq5C71fEde6GtBAGg/JcGoE3bucNdqLZDg==") },
        new iRINGEndpoint{id="5", name = "Dow AdapterService", serviceUri = "http://usmdlsdoww915/AdapterService/Service.svc"},
        new iRINGEndpoint{id="12", name = "Bechtel iLab AdapterService", serviceUri = "http://labs98142/AdapterService/Service.svc"},
      };

      iRINGEndpoints interfaceEndPoints = new iRINGEndpoints
      {
        new iRINGEndpoint{id="1", name = "iRINGSandbox InterfaceService", serviceUri = "http://www.iringsandbox.org/InterfaceService"},
        new iRINGEndpoint{id="2", name = "Bechtel InterfaceService", serviceUri = "https://facade.becpsn.com/InterfaceService", 
          credentials = new WebCredentials("8stUFBRIGcYR0Nl8v25rm5+qecZZD6i6itQUBD8zM0bODWZk1TysqXLBvMkc4GVUSCGqS4I9G99Wcynf7Qdqssj5NgAqU7m16EguxwWAScfq8olyMFfjd271RNc35Nv9")},
        new iRINGEndpoint{id="3", name = "CCC InterfaceService", serviceUri = "http://c3d.ccc.gr/InterfaceService"},
        new iRINGEndpoint{id="4", name = "Hatch InterfaceService", serviceUri = "http://iring.hatch.com.au/InterfaceService", 
          credentials = new WebCredentials("zlNbMav8dBXUREoOR6ctMZ8aRcOiKmUtgU9mmyhyNqxJ0EAePsEoRq5C71fEde6GtBAGg/JcGoE3bucNdqLZDg==") },
        new iRINGEndpoint{id="5", name = "Dow InterfaceService", serviceUri = "http://usmdlsdoww915/InterfaceService"},
        new iRINGEndpoint{id="6", name = "Bentley InterfaceService", serviceUri = "http://www.iringsandbox.org/InterfaceService/bentley2/sparql"},
        new iRINGEndpoint{id="12", name = "Bechtel iLab InterfaceService", serviceUri = "http://labs98142/InterfaceService"},
      };

      ConfiguredEndpoints config = new ConfiguredEndpoints();
      Scenarios scenarios = new Scenarios
      { 
        new Scenario{scenarioName = "Lines Bechtel (Inspec) -> Hatch (PlantSpace P&ID)", sender="Bechtel", receiver="Hatch", 
          senderAdapterServiceId = "2", receiverAdapterServiceId = "4", interfaceServiceId = "4",
          senderProjectName="12345_000", senderApplicationName="Inspec", senderGraphName="LineList",
          receiverProjectName="12345_000", receiverApplicationName="PSPID", receiverGraphName="Lines",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Lines Bechtel (OpenPlant PowerPID) -> Hatch (PlantSpace P&ID)", sender="Bechtel", receiver="Hatch", 
          senderAdapterServiceId = "2", receiverAdapterServiceId = "4", interfaceServiceId = "4",
          senderProjectName="12345_000", senderApplicationName="InspecPID", senderGraphName="LineList",
          receiverProjectName="12345_000", receiverApplicationName="PSPID", receiverGraphName="Lines",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Lines Hatch (PlantSpace P&ID) -> Bechtel (Inspec)", sender="Hatch", receiver="Bechtel", 
          senderAdapterServiceId = "4", receiverAdapterServiceId = "2", interfaceServiceId = "2",
          senderProjectName="12345_000", senderApplicationName="PSPID", senderGraphName="Lines",
          receiverProjectName="12345_000", receiverApplicationName="Inspec", receiverGraphName="LineList",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Lines Hatch (PlantSpace P&ID) -> Bechtel (OpenPlant PowerPID)", sender="Hatch", receiver="Bechtel", 
          senderAdapterServiceId = "4", receiverAdapterServiceId = "2", interfaceServiceId = "2",
          senderProjectName="12345_000", senderApplicationName="PSPID", senderGraphName="Lines",
          receiverProjectName="12345_000", receiverApplicationName="InspecPID", receiverGraphName="LineList",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Lines CCC -> Hatch", sender="CCC", receiver="Hatch", 
          senderAdapterServiceId = "3", receiverAdapterServiceId = "4", interfaceServiceId = "4",
          senderProjectName="CCC", senderApplicationName="C3D", senderGraphName="Lines",
          receiverProjectName="12345_000", receiverApplicationName="PSPID", receiverGraphName="Lines",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Lines Hatch -> CCC", sender="Hatch", receiver="CCC", 
          senderAdapterServiceId = "4", receiverAdapterServiceId = "3", interfaceServiceId = "3",
          senderProjectName="12345_000", senderApplicationName="PSPID", senderGraphName="Lines",
          receiverProjectName="CCC", receiverApplicationName="C3D", receiverGraphName="Lines",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Bechtel Lines Test", sender="Bechtel", receiver="Bechtel", 
          senderAdapterServiceId = "2", receiverAdapterServiceId = "2", interfaceServiceId = "2",
          senderProjectName="12345_000", senderApplicationName="ABC", senderGraphName="Lines",
          receiverProjectName="12345_000", receiverApplicationName="ABC", receiverGraphName="Lines",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "CCC Lines Test", sender="CCC", receiver="CCC", 
          senderAdapterServiceId = "3", receiverAdapterServiceId = "3", interfaceServiceId = "3",
          senderProjectName="CCC", senderApplicationName="C3D", senderGraphName="Lines",
          receiverProjectName="CCC", receiverApplicationName="C3D", receiverGraphName="Lines",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Hatch Lines Test", sender="Hatch", receiver="Hatch", 
          senderAdapterServiceId = "4", receiverAdapterServiceId = "4", interfaceServiceId = "4",
          senderProjectName="12345_000", senderApplicationName="PSPID", senderGraphName="Lines",
          receiverProjectName="12345_000", receiverApplicationName="PSPID", receiverGraphName="Lines",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Hatch Equipment Test", sender="Hatch", receiver="Hatch", 
          senderAdapterServiceId = "4", receiverAdapterServiceId = "4", interfaceServiceId = "4",
          senderProjectName="12345_000", senderApplicationName="PSPID", senderGraphName="Equipment",
          receiverProjectName="12345_000", receiverApplicationName="PSPID", receiverGraphName="Equipment",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Hatch Instruments Test", sender="Hatch", receiver="Hatch", 
          senderAdapterServiceId = "4", receiverAdapterServiceId = "4", interfaceServiceId = "4",
          senderProjectName="12345_000", senderApplicationName="PSPID", senderGraphName="Instruments",
          receiverProjectName="12345_000", receiverApplicationName="PSPID", receiverGraphName="Instruments",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Emerson Instruments Test", sender="Emerson", receiver="Emerson", 
          senderAdapterServiceId = "5", receiverAdapterServiceId = "5", interfaceServiceId = "5",
          senderProjectName="12345_000", senderApplicationName="EIO", senderGraphName="Instruments",
          receiverProjectName="12345_000", receiverApplicationName="EIO", receiverGraphName="Instruments",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "iRINGSandbox Test", sender="iRINGSandbox", receiver="iRINGSandbox", 
          senderAdapterServiceId = "1", receiverAdapterServiceId = "1", interfaceServiceId = "1",
          senderProjectName="12345_000", senderApplicationName="ABC", senderGraphName="Lines",
          receiverProjectName="12345_000", receiverApplicationName="ABC", receiverGraphName="Lines",
          exportEnabled=false, importEnabled=false},
        //new Scenario{scenarioName = "Bechtel iLab Line Test", sender="Bechtel", receiver="Bechtel", 
        //  senderAdapterServiceId = "12", receiverAdapterServiceId = "12", interfaceServiceId = "12",
        //  senderProjectName="12345_000", senderApplicationName="Inpsec", senderGraphName="LineList",
        //  receiverProjectName="12345_000", receiverApplicationName="Inpsec", receiverGraphName="LineList",
        //  exportEnabled=false, importEnabled=false},
      };
      
      config.scenarios = scenarios;
      config.adapterEndpoints = adapterEndPoints;
      config.interfaceEndpoints = interfaceEndPoints;

      Utility.Write<ConfiguredEndpoints>(config, @"..\..\ConfiguredEndpoints.xml", true);
    }
  }
}
