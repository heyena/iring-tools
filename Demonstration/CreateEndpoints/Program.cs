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
            ConfiguredEndpoints config = new ConfiguredEndpoints();
            Scenarios scenarios = new Scenarios
            { 
                new Scenario{
                    scenarioName = "Test", 
                    sender="BechtelSender", 
                    receiver="BechtelReceiver", 
                    senderAdapterServiceId = "1", 
                    receiverAdapterServiceId = "1", 
                    interfaceServiceId = "1",
                    senderProjectName="12345_000",
                    senderApplicationName="ABC",
                    senderGraphName="Lines",
                    receiverProjectName="12345_000",
                    receiverApplicationName="ABC",
                    receiverGraphName="Lines",
                    exportEnabled=false,
                    importEnabled=false
                },
                new Scenario{scenarioName = "Data flow 1A – Hatch to Dow", sender="Hatch", receiver="Dow", senderAdapterServiceId = "5", receiverAdapterServiceId = "7", interfaceServiceId = "5"},
                new Scenario{scenarioName = "Data Flow 1B – Bechtel to Dow", sender="Bechtel", receiver="Dow", senderAdapterServiceId = "1", receiverAdapterServiceId = "7", interfaceServiceId = "1"},
                new Scenario{scenarioName = "Data flow 2A – Hatch to Intergraph", sender="Hatch", receiver="Intergraph", senderAdapterServiceId = "5", receiverAdapterServiceId = "8", interfaceServiceId = "5"},
                new Scenario{scenarioName = "Data Flow 2B – Bechtel to Intergraph", sender="Bechtel", receiver="Intergraph", senderAdapterServiceId = "1", receiverAdapterServiceId = "8", interfaceServiceId = "1"},
                new Scenario{scenarioName = "Data Flow 2C – Dow to Intergraph", sender="Dow", receiver="Intergraph", senderAdapterServiceId = "", receiverAdapterServiceId = "", interfaceServiceId = ""},
                new Scenario{scenarioName = "Data flow 3A – Hatch to CCC", sender="Hatch", receiver="CCC", senderAdapterServiceId = "5", receiverAdapterServiceId = "3", interfaceServiceId = "5"},
                new Scenario{scenarioName = "Data Flow 3B – Bechtel to CCC", sender="Bechtel", receiver="CCC", senderAdapterServiceId = "1", receiverAdapterServiceId = "3", interfaceServiceId = "1"},
                new Scenario{scenarioName = "Data flow 4A – Hatch to Emerson", sender="Hatch", receiver="Emerson", senderAdapterServiceId = "5", receiverAdapterServiceId = "9", interfaceServiceId = "5"},
                new Scenario{scenarioName = "Data Flow 4B – Bechtel to Emerson", sender="Bechtel", receiver="Emerson", senderAdapterServiceId = "1", receiverAdapterServiceId = "9", interfaceServiceId = "1"},
                new Scenario{scenarioName = "Data Flow 5A – DuPont to Bechtel", sender="Dupont", receiver="Bechtel", senderAdapterServiceId = "10", receiverAdapterServiceId = "1", interfaceServiceId = "7"}
            };
            iRINGEndpoints adapterEndPoints = new iRINGEndpoints
        {
            new iRINGEndpoint{id="1", name = "My Adapter Service", serviceUri = "http://iring.hatch.com.au/AdapterService/Service.svc", credentials = new WebCredentials()},
            new iRINGEndpoint{id="2", name = "Bechtel (karthur) Adapter Service", serviceUri = "https://iring.becpsn.com/AdapterService/Service.svc", credentials = new WebCredentials("IKmr723/1qmzmrRzIhJWZezzpAU0XoV6QkXRoKOBRQhMFt7iNAShgc/tGU0Lypt3TTdBG5MHCTxEjb3btLriN9ZVVXRHzDQPqmoyGLq/xvY=")},
            new iRINGEndpoint{id="3", name = "Bechtel (local) Adapter Service", serviceUri = "http://localhost/AdapterService/Service.svc", credentials = new WebCredentials()},
            new iRINGEndpoint{id="4", name = "CCC (karthur) Adapter Service", serviceUri = "", credentials = new WebCredentials()},
            new iRINGEndpoint{id="5", name = "CCC (local) Adapter Service", serviceUri = "http://localhost/AdapterService/Service.svc", credentials = new WebCredentials()},
            new iRINGEndpoint{id="6", name = "Hatch (karthur) Adapter Service", serviceUri = "http://iring.hatch.com.au/AdapterService/Service.svc", credentials = new WebCredentials("zlNbMav8dBXUREoOR6ctMZ8aRcOiKmUtgU9mmyhyNqxJ0EAePsEoRq5C71fEde6GtBAGg/JcGoE3bucNdqLZDg==")},
            new iRINGEndpoint{id="7", name = "Hatch (local) Adapter Service", serviceUri = "http://localhost/AdapterService/Service.svc", credentials = new WebCredentials()},
            new iRINGEndpoint{id="8", name = "Dow (local) Adapter Service", serviceUri = "http://localhost/AdapterService/Service.svc", credentials = new WebCredentials()},
            new iRINGEndpoint{id="9", name = "Intergraph (local) Adapter Service", serviceUri = "http://localhost/AdapterService/Service.svc", credentials = new WebCredentials()},
            new iRINGEndpoint{id="10", name = "Emerson (local) Adapter Service", serviceUri = "http://localhost/AdapterService/Service.svc", credentials = new WebCredentials()},
            new iRINGEndpoint{id="11", name = "Dupont (local) Adapter Service", serviceUri = "http://localhost/AdapterService/Service.svc", credentials = new WebCredentials()}
        };

            iRINGEndpoints interfaceEndPoints = new iRINGEndpoints
        {
            new iRINGEndpoint{id="1", name = "My Interface Service", serviceUri = "http://localhost:2020", credentials = new WebCredentials()},
            new iRINGEndpoint{id="2", name = "Bechtel (karthur) Interface Service", serviceUri = "https://iring.becpsn.com/InterfaceService", credentials = new WebCredentials("IKmr723/1qmzmrRzIhJWZezzpAU0XoV6QkXRoKOBRQhMFt7iNAShgc/tGU0Lypt3TTdBG5MHCTxEjb3btLriN9ZVVXRHzDQPqmoyGLq/xvY=")},
            new iRINGEndpoint{id="3", name = "Bechtel (local) Interface Service", serviceUri = "http://localhost:2020", credentials = new WebCredentials()},
            new iRINGEndpoint{id="4", name = "CCC (karthur) Interface Service", serviceUri = "", credentials = new WebCredentials()},
            new iRINGEndpoint{id="5", name = "CCC (local) Interface Service", serviceUri = "http://localhost:2020", credentials = new WebCredentials()},
            new iRINGEndpoint{id="6", name = "Hatch (karthur) Interface Service", serviceUri = "http://iring.hatch.com.au/InterfaceService", credentials = new WebCredentials("zlNbMav8dBXUREoOR6ctMZ8aRcOiKmUtgU9mmyhyNqxJ0EAePsEoRq5C71fEde6GtBAGg/JcGoE3bucNdqLZDg==")},
            new iRINGEndpoint{id="7", name = "Hatch (local) Interface Service", serviceUri = "http://localhost:2020", credentials = new WebCredentials()},
            new iRINGEndpoint{id="8", name = "Dupont (local Interface Service", serviceUri = "http://localhost:2020", credentials = new WebCredentials()}            
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
