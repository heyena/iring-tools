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
                new Scenario{scenarioName = "iRINGSandbox Test", sender="iRINGSandbox", receiver="iRINGSandbox", 
                    senderAdapterServiceId = "1", receiverAdapterServiceId = "1", interfaceServiceId = "1",
                    senderProjectName="12345_000", senderApplicationName="ABC", senderGraphName="Lines",
                    receiverProjectName="12345_000", receiverApplicationName="ABC", receiverGraphName="Lines",
                    exportEnabled=false, importEnabled=false},
                new Scenario{scenarioName = "Bechtel Test", sender="Bechtel", receiver="Bechtel", 
                    senderAdapterServiceId = "2", receiverAdapterServiceId = "2", interfaceServiceId = "2",
                    senderProjectName="12345_000", senderApplicationName="ABC", senderGraphName="Lines",
                    receiverProjectName="12345_000", receiverApplicationName="ABC", receiverGraphName="Lines",
                    exportEnabled=false, importEnabled=false},
                new Scenario{scenarioName = "Bechtel Test", sender="Bechtel", receiver="Bechtel", 
                    senderAdapterServiceId = "2", receiverAdapterServiceId = "2", interfaceServiceId = "2",
                    senderProjectName="12345_000", senderApplicationName="ABC", senderGraphName="Lines",
                    receiverProjectName="12345_000", receiverApplicationName="ABC", receiverGraphName="Lines",
                    exportEnabled=false, importEnabled=false},
                new Scenario{scenarioName = "Bechtel iLab Test", sender="Bechtel", receiver="Bechtel", 
                    senderAdapterServiceId = "12", receiverAdapterServiceId = "12", interfaceServiceId = "12",
                    senderProjectName="12345_000", senderApplicationName="Inpsec", senderGraphName="LineList",
                    receiverProjectName="12345_000", receiverApplicationName="Inpsec", receiverGraphName="LineList",
                    exportEnabled=false, importEnabled=false},
            };
            
            config.scenarios = scenarios;
            config.adapterEndpoints = adapterEndPoints;
            config.interfaceEndpoints = interfaceEndPoints;

            Utility.Write<ConfiguredEndpoints>(config, @"..\..\ConfiguredEndpoints.xml", true);

            //Scenario scenario = new Scenario();

            //iRINGEndpoints adapterEndpoints = new iRINGEndpoints();
            //iRINGEndpoint adapterEndpoint = new iRINGEndpoint();

            //adapterEndpoint.id = "1";
            //adapterEndpoint.name = "My iRING Adapter Service";
            //adapterEndpoint.serviceUri = "http://localhost/AdapterService/Service.svc";
            //adapterEndpoint.credentials = new RequestCredentials();
            //adapterEndpoints.Add(adapterEndpoint);

            //iRINGEndpoints interfaceEndpoints = new iRINGEndpoints();
            //iRINGEndpoint interfaceEndpoint = new iRINGEndpoint();

            //interfaceEndpoint.id = "1";
            //interfaceEndpoint.name = "My iRING Interface Service";
            //interfaceEndpoint.serviceUri = "http://localhost:2020";
            //interfaceEndpoint.credentials = new RequestCredentials();
            //interfaceEndpoints.Add(interfaceEndpoint);

            //scenario.scenarioName = "Demo 1A";
            //scenario.receiverAdapterServiceId = "1";
            //scenario.senderAdapterServiceId = "1";
            //scenario.interfaceServiceId = "1";

            //scenarios.Add(scenario);

            //config.scenarios = scenarios;
            //config.interfaceEndpoints = interfaceEndpoints;
            //config.adapterEndpoints = adapterEndpoints;

            //Utility.Write<Config>(config, @"..\..\Config.xml", true);


            //RequestCredentials kArthurCreds = new RequestCredentials
            //{
            //    userName = "karthur",
            //    password = "",
            //    domain = "eBechtel"
            //};
            //  kArthurCreds.Encrypt();

            //  RequestCredentials robBenCreds = new RequestCredentials
            //  {
            //      userName = "robben@robenj.com",
            //      password = "",
            //      domain = ""
            //  };
            //  robBenCreds.Encrypt();

            //iRINGEndpoint myEndpoint = new iRINGEndpoint
            //{
            //  name = "My iRING Interface",
            //  adapterServiceUri = "http://localhost/AdapterService/Service.svc",
            //  interfaceServiceUri = "http://localhost:2020",
            //  credentials = new RequestCredentials()
            //};

            //iRINGEndpoint bechtelEndpointKArthur = new iRINGEndpoint
            //{
            //  name = "Bechtel iRING Interface (KArthur)",
            //  adapterServiceUri = "https://iring.becpsn.com/AdapterService/Service.svc",
            //  interfaceServiceUri = "https://iring.becpsn.com/InterfaceService",
            //  credentials = kArthurCreds
            //};

            //iRINGEndpoint bechtelEndpointRobBen = new iRINGEndpoint
            //{
            //  name = "Bechtel iRING Interface (robben@robenj.com)",
            //  adapterServiceUri = "https://iring.becpsn.com/AdapterService/Service.svc",
            //  interfaceServiceUri = "https://iring.becpsn.com/InterfaceService",
            //  credentials = robBenCreds
            //};

            //iRINGEndpoints endpoints = new iRINGEndpoints
            //{
            //  myEndpoint,
            //  bechtelEndpointKArthur,
            //  bechtelEndpointRobBen
            //};

            //Utility.Write<iRINGEndpoints>(endpoints, @"..\..\Endpoints.xml", true);
        }
    }
}
