using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using org.iringtools.library;
using org.iringtools.adapter;
using System.Collections.Specialized;

namespace org.iringtools.adapter.proj_12345_000.API
{
    class Program
    {
        static void Main(string[] args)
        {
            NameValueCollection settings = new NameValueCollection();
            settings["BaseDirectoryPath"] = String.Empty;
            settings["XmlPath"] = @"C:\iring-tools\Adapter\CSVDataLayer\";
            settings["ProxyCredentialToken"] = String.Empty;
            settings["ProxyHost"] = String.Empty;
            settings["ProxyPort"] = String.Empty;
            settings["UseSemweb"] = "False";
            settings["TripleStoreConnectionString"] = String.Empty;
            settings["InterfaceService"] = String.Empty;
            settings["InterfaceServicePath"] = String.Empty;
            settings["TargetCredentialToken"] = String.Empty;
            settings["TrimData"] = "False";
            settings["BinaryPath"] = String.Empty;
            settings["CodePath"] = String.Empty;

            Random random = new Random();
            AdapterSettings adapterSettings = new AdapterSettings(settings);
            ApplicationSettings appSettings = new ApplicationSettings("12345_000", "API");
            CSVDataLayer csvDataLayer = new CSVDataLayer(adapterSettings, appSettings);

            // create
            Console.WriteLine("Testing CREATE...");
            IList<string> identifiers = new List<string>() { "Equip-001", "Equip-002", "Equip-003", "Equip-004" };
            IList<IDataObject> dataObjects = csvDataLayer.Create("org.iringtools.adapter.datalayer.proj_12345_000.CSV.Equipment", identifiers);
            foreach (IDataObject dataObject in dataObjects)
            {
                dataObject.SetPropertyValue("PumpType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("PumpDriverType", "PDT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("DesignTemp", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("DesignPressure", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("Capacity", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("SpecificGravity", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("DifferentialPressure", (double)random.Next(2, 10));
            }
            Response response = csvDataLayer.Post(dataObjects);
            foreach (string line in response)
            {
                Console.WriteLine(line);
            }

            // read
            Console.WriteLine("\nTesting READ...");
            //dataObjects = csvDataLayer.Get("Equipment", null, 0, 0);
            dataObjects = csvDataLayer.Get("org.iringtools.adapter.datalayer.proj_12345_000.CSV.Equipment", identifiers);
            foreach (IDataObject dataObject in dataObjects)
            {
                Console.WriteLine(
                  dataObject.GetPropertyValue("Tag") + ", " +
                  dataObject.GetPropertyValue("PumpType") + ", " +
                  dataObject.GetPropertyValue("PumpDriverType") + ", " +
                  dataObject.GetPropertyValue("DesignTemp") + ", " +
                  dataObject.GetPropertyValue("DesignPressure") + ", " +
                  dataObject.GetPropertyValue("Capacity") + ", " +
                  dataObject.GetPropertyValue("SpecificGravity") + ", " +
                  dataObject.GetPropertyValue("DifferentialPressure"));
            }

            // update
            Console.WriteLine("\nTesting UPDATE - updating pump type...");
            foreach (IDataObject dataObject in dataObjects)
            {
                dataObject.SetPropertyValue("PumpType", "PT2-" + (double)random.Next(2, 10));
            }
            response = csvDataLayer.Post(dataObjects);
            foreach (string line in response)
            {
                Console.WriteLine(line);
            }
            dataObjects = csvDataLayer.Get("org.iringtools.adapter.datalayer.proj_12345_000.CSV.Equipment", identifiers);
            foreach (IDataObject dataObject in dataObjects)
            {
                Console.WriteLine(
                  dataObject.GetPropertyValue("Tag") + ", " +
                  dataObject.GetPropertyValue("PumpType") + ", " +
                  dataObject.GetPropertyValue("PumpDriverType") + ", " +
                  dataObject.GetPropertyValue("DesignTemp") + ", " +
                  dataObject.GetPropertyValue("DesignPressure") + ", " +
                  dataObject.GetPropertyValue("Capacity") + ", " +
                  dataObject.GetPropertyValue("SpecificGravity") + ", " +
                  dataObject.GetPropertyValue("DifferentialPressure"));
            }

            // data filter
            Console.WriteLine("\nTesting FILTER...");
            DataFilter filter = new DataFilter()
            {
                Expressions = new List<Expression>()
      {
        new Expression()
        {
          //OpenGroupCount = 1,
          //LogicalOperator = LogicalOperator.And,            
          PropertyName = "DesignTemp",
          RelationalOperator = RelationalOperator.GreaterThan,
          Values = new List<string>() { "2.5" }
        },
        new Expression()
        {
          LogicalOperator = LogicalOperator.And,            
          PropertyName = "DesignTemp",
          RelationalOperator = RelationalOperator.LesserThan,
          Values = new List<string>() { "7.5" },
          CloseGroupCount = 1
        }
      },
            };
            dataObjects = csvDataLayer.Get("org.iringtools.adapter.datalayer.proj_12345_000.CSV.Equipment", filter, 0, 0);
            foreach (IDataObject dataObject in dataObjects)
            {
                Console.WriteLine(
                  dataObject.GetPropertyValue("Tag") + ", " +
                  dataObject.GetPropertyValue("PumpType") + ", " +
                  dataObject.GetPropertyValue("PumpDriverType") + ", " +
                  dataObject.GetPropertyValue("DesignTemp") + ", " +
                  dataObject.GetPropertyValue("DesignPressure") + ", " +
                  dataObject.GetPropertyValue("Capacity") + ", " +
                  dataObject.GetPropertyValue("SpecificGravity") + ", " +
                  dataObject.GetPropertyValue("DifferentialPressure"));
            }

            // delete
            Console.WriteLine("\nTesting DELETE...");
            response = csvDataLayer.Delete("org.iringtools.adapter.datalayer.proj_12345_000.CSV.Equipment", identifiers);
            foreach (string line in response)
            {
                Console.WriteLine(line);
            }


            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}

