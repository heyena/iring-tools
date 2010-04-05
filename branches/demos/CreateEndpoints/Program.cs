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
        new iRINGEndpoint{id="4", name = "Hatch AdapterService", serviceUri = "http://iring.hatch.com.au/AdapterService/Service.svc"}, 
          //credentials = new WebCredentials("zlNbMav8dBXUREoOR6ctMZ8aRcOiKmUtgU9mmyhyNqxJ0EAePsEoRq5C71fEde6GtBAGg/JcGoE3bucNdqLZDg==") },
        new iRINGEndpoint{id="5", name = "Emerson AdapterService", serviceUri = "http://122.248.16.198/AdapterService/Service.svc"},
        new iRINGEndpoint{id="10", name = "TCS AdapterService", serviceUri = "http://125.20.35.60/AdapterService/Service.svc",
          credentials = new WebCredentials("7Q8aGXYHbwVtkqTbiYF9tsvcUa76udrgwFiYZQ7ixHsgaevNQAryCAOZQtJ3pV6pDniCTzJGm3B78clDFbtFFA==") },
        new iRINGEndpoint{id="12", name = "Bechtel Staging AdapterService", serviceUri = "https://façade.staging.becpsn.com/AdapterService/Service.svc", 
          credentials = new WebCredentials("8stUFBRIGcYR0Nl8v25rm5+qecZZD6i6itQUBD8zM0bODWZk1TysqXLBvMkc4GVUSCGqS4I9G99Wcynf7Qdqssj5NgAqU7m16EguxwWAScfq8olyMFfjd271RNc35Nv9")},
        new iRINGEndpoint{id="22", name = "Bechtel iLab AdapterService", serviceUri = "http://labs98142/AdapterService/Service.svc"},
        new iRINGEndpoint{id="27", name = "Dow AdapterService", serviceUri = "http://usmdlsdoww915/AdapterService/Service.svc"},
        new iRINGEndpoint{id="80", name = "Local AdapterService", serviceUri = "http://localhost/AdapterService/Service.svc"},
        new iRINGEndpoint{id="90", name = "Local Debug AdapterService", serviceUri = "http://localhost:52786/Service.svc"},
      };

      iRINGEndpoints interfaceEndPoints = new iRINGEndpoints
      {
        new iRINGEndpoint{id="1", name = "iRINGSandbox InterfaceService", serviceUri = "http://www.iringsandbox.org/InterfaceService"},
        new iRINGEndpoint{id="2", name = "Bechtel InterfaceService", serviceUri = "https://facade.becpsn.com/InterfaceService", 
          credentials = new WebCredentials("8stUFBRIGcYR0Nl8v25rm5+qecZZD6i6itQUBD8zM0bODWZk1TysqXLBvMkc4GVUSCGqS4I9G99Wcynf7Qdqssj5NgAqU7m16EguxwWAScfq8olyMFfjd271RNc35Nv9")},
        new iRINGEndpoint{id="3", name = "CCC InterfaceService", serviceUri = "http://c3d.ccc.gr/InterfaceService"},
        new iRINGEndpoint{id="4", name = "Hatch InterfaceService", serviceUri = "http://iring.hatch.com.au/InterfaceService"}, 
          //credentials = new WebCredentials("zlNbMav8dBXUREoOR6ctMZ8aRcOiKmUtgU9mmyhyNqxJ0EAePsEoRq5C71fEde6GtBAGg/JcGoE3bucNdqLZDg==") },
        new iRINGEndpoint{id="5", name = "Emerson InterfaceService", serviceUri = "http://122.248.16.198/InterfaceService"},
        new iRINGEndpoint{id="10", name = "TCS InterfaceService", serviceUri = "http://125.20.35.60/InterfaceService",
          credentials = new WebCredentials("7Q8aGXYHbwVtkqTbiYF9tsvcUa76udrgwFiYZQ7ixHsgaevNQAryCAOZQtJ3pV6pDniCTzJGm3B78clDFbtFFA==") },
        new iRINGEndpoint{id="12", name = "Bechtel Staging InterfaceService", serviceUri = "https://façade.staging.becpsn.com/InterfaceService", 
          credentials = new WebCredentials("8stUFBRIGcYR0Nl8v25rm5+qecZZD6i6itQUBD8zM0bODWZk1TysqXLBvMkc4GVUSCGqS4I9G99Wcynf7Qdqssj5NgAqU7m16EguxwWAScfq8olyMFfjd271RNc35Nv9")},
        new iRINGEndpoint{id="22", name = "Bechtel iLab InterfaceService", serviceUri = "http://labs98142/InterfaceService"},
        new iRINGEndpoint{id="80", name = "Local InterfaceService", serviceUri = "http://localhost/InterfaceService"},
      };

      ConfiguredEndpoints config = new ConfiguredEndpoints();
      Scenarios scenarios = new Scenarios
      {
        new Scenario{scenarioName = "Bentley (PDx) -> Bechtel (Inspec) [Lines]", sender="Bentley", receiver="Bechtel", 
          senderAdapterServiceId = "N/A", receiverAdapterServiceId = "2", interfaceServiceId = "1",
          senderProjectName="12345_000", senderApplicationName="PDx", senderGraphName="",
          receiverProjectName="12345_000", receiverApplicationName="Inspec", receiverGraphName="LineList",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Hatch (PlantSpace P&ID) -> CCC (C3D) [Lines]", sender="Hatch", receiver="CCC", 
          senderAdapterServiceId = "4", receiverAdapterServiceId = "3", interfaceServiceId = "4",
          senderProjectName="12345_000", senderApplicationName="PSPID", senderGraphName="Lines",
          receiverProjectName="CCC", receiverApplicationName="C3D", receiverGraphName="Lines",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Hatch (PlantSpace P&ID) -> Dow (SPPID) [Lines]", sender="Hatch", receiver="Dow", 
          senderAdapterServiceId = "4", receiverAdapterServiceId = "80", interfaceServiceId = "4",
          senderProjectName="12345_000", senderApplicationName="PSPID", senderGraphName="Lines",
          receiverProjectName="CrackingComplex", receiverApplicationName="SPPID", receiverGraphName="LINE",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Emerson (Enter It Once) -> CH2M Hill (Project Database) [Instruments]", sender="Emerson", receiver="CH2M Hill", 
          senderAdapterServiceId = "5", receiverAdapterServiceId = "80", interfaceServiceId = "5",
          senderProjectName="12345_000", senderApplicationName="EIO", senderGraphName="INSTRUMENTS",
          receiverProjectName="PDB", receiverApplicationName="Inst", receiverGraphName="InstrumentList",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "Bechtel (Inspec) -> Intergraph (SmartPlant Foundation) [Equipment]", sender="Bechtel", receiver="Intergraph", 
          senderAdapterServiceId = "2", receiverAdapterServiceId = "80", interfaceServiceId = "2",
          senderProjectName="12345_000", senderApplicationName="Inspec", senderGraphName="EquipmentList",
          receiverProjectName="12345_000", receiverApplicationName="SPF", receiverGraphName="ProcessEquipment",
          exportEnabled=false, importEnabled=false},
        new Scenario{scenarioName = "TCS (SmartPlant Instrumentation) -> Emersion (Enter It Once) [Instruments]", sender="TCS", receiver="Emerson", 
          senderAdapterServiceId = "10", receiverAdapterServiceId = "5", interfaceServiceId = "10",
          senderProjectName="12345_000", senderApplicationName="SPI", senderGraphName="InstrumentList",
          receiverProjectName="12345_000", receiverApplicationName="EIO", receiverGraphName="INSTRUMENTS",
          exportEnabled=false, importEnabled=false},
      };
      
      config.scenarios = scenarios;
      config.adapterEndpoints = adapterEndPoints;
      config.interfaceEndpoints = interfaceEndPoints;

      Utility.Write<ConfiguredEndpoints>(config, @"..\..\ConfiguredEndpoints.xml", true);
    }
  }
}
