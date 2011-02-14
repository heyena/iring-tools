using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;


namespace iRINGTools.SDK.CSVDataLayer
{
    [TestFixture]
    public class CSVDataLayerTest
    {
        private static CustomDataLayer _csvDataLayer;
        private static AdapterSettings _adapterSettings;

        [SetUp]
        public void BeforeTest()
        {
            NameValueCollection settings = new NameValueCollection();

            settings["BaseDirectoryPath"] = "C:\\iring-tools-2.0.x\\iRINGTools.SDK\\CSVDataLayer";
            settings["XmlPath"] = ".\\";
            settings["ProjectName"] = "12345_000";
            settings["ApplicationName"] = "CSV";

            _adapterSettings = new AdapterSettings();
            _adapterSettings.AppendSettings(settings);

            Directory.SetCurrentDirectory(_adapterSettings["BaseDirectoryPath"]);

            string appSettingsPath = String.Format("{0}12345_000.CSV.config",
                  _adapterSettings["XmlPath"]
                );

            if (File.Exists(appSettingsPath))
            {
                AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
                _adapterSettings.AppendSettings(appSettings);
            }

            _csvDataLayer = new CustomDataLayer(_adapterSettings);
        }

        [Test]
        public void Create()
        {
            IList<string> identifiers = new List<string>() { 
                "Equip-001", 
                "Equip-002",
                "Equip-003", 
                "Equip-004"
            };

            Random random = new Random();

            IList<IDataObject> dataObjects = _csvDataLayer.Create("Equipment", identifiers);
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
            Response actual = _csvDataLayer.Post(dataObjects);

            if (actual.Level != StatusLevel.Success)
            {
                throw new AssertionException(Utility.SerializeDataContract<Response>(actual));
            }

            Assert.IsTrue(actual.Level == StatusLevel.Success);
        }

        [Test]
        public void Read()
        {
            IList<string> identifiers = new List<string>() 
            { 
                "Equip-001", 
                "Equip-002", 
                "Equip-003", 
                "Equip-004" 
            };

            IList<IDataObject> dataObjects = _csvDataLayer.Get("Equipment", identifiers);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

            foreach (IDataObject dataObject in dataObjects)
            {
                Assert.IsNotNull(dataObject.GetPropertyValue("PumpType"));
                Assert.IsNotNull(dataObject.GetPropertyValue("PumpDriverType"));
                Assert.IsNotNull(dataObject.GetPropertyValue("DesignTemp"));
                Assert.IsNotNull(dataObject.GetPropertyValue("DesignPressure"));
                Assert.IsNotNull(dataObject.GetPropertyValue("Capacity"));
                Assert.IsNotNull(dataObject.GetPropertyValue("SpecificGravity"));
                Assert.IsNotNull(dataObject.GetPropertyValue("DifferentialPressure"));
            }
        }

        [Test]
        public void ReadWithFilter()
        {
            DataFilter dataFilter = new DataFilter
            {
                Expressions = new List<Expression>
                {
                    new Expression
                    {
                        PropertyName = "PumpDriverType",
                        RelationalOperator = RelationalOperator.EqualTo,
                        Values = new Values
                        {
                            "PDT-8",
                        }
                    }
                }
            };

            IList<IDataObject> dataObjects = _csvDataLayer.Get("Equipment", dataFilter, 2, 0);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

            Assert.AreEqual(dataObjects.Count(), 2);

            foreach (IDataObject dataObject in dataObjects)
            {
                Assert.IsNotNull(dataObject.GetPropertyValue("PumpType"));
                Assert.AreEqual(dataObject.GetPropertyValue("PumpDriverType"), "PDT-8");
                Assert.IsNotNull(dataObject.GetPropertyValue("DesignTemp"));
                Assert.IsNotNull(dataObject.GetPropertyValue("DesignPressure"));
                Assert.IsNotNull(dataObject.GetPropertyValue("Capacity"));
                Assert.IsNotNull(dataObject.GetPropertyValue("SpecificGravity"));
                Assert.IsNotNull(dataObject.GetPropertyValue("DifferentialPressure"));
            }
        }

        [Test]
        public void GetDictionary()
        {
            DataDictionary dictionary = _csvDataLayer.GetDictionary();

            Assert.IsNotNull(dictionary);

            string dictionaryPath = String.Format("{0}DataDictionary.xml",
                  _adapterSettings["XmlPath"]
                );

            Utility.Write<DataDictionary>(dictionary, dictionaryPath, true);
        }
    }
}
