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
using Ninject;
using Ninject.Extensions.Xml;

namespace iRINGTools.SDK.SPPIDDataLayer
{
    [TestFixture]
    public class SPPIDDataLayerTest
    {
        private string _baseDirectory = string.Empty;
        private IKernel _kernel = null;
        private NameValueCollection _settings;
        private AdapterSettings _adapterSettings;
        private IDataLayer2 _sppidDataLayer;

        public SPPIDDataLayerTest()
        {
            // N inject magic
            var ninjectSettings = new NinjectSettings { LoadExtensions = false };
            _kernel = new StandardKernel(ninjectSettings);

            _kernel.Load(new XmlExtensionModule());

            _kernel.Bind<AdapterSettings>().ToSelf().InSingletonScope();
            _adapterSettings = _kernel.Get<AdapterSettings>();

            // Start with some generic settings
            _settings = new NameValueCollection();

            _settings["XmlPath"] = @".\12345_000\";
            _settings["ProjectName"] = "12345_000";
            _settings["ApplicationName"] = "SPPID";

            _baseDirectory = Directory.GetCurrentDirectory();
            _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\bin"));
            _settings["BaseDirectoryPath"] = _baseDirectory;
            Directory.SetCurrentDirectory(_baseDirectory);

            _adapterSettings.AppendSettings(_settings);

            // Add our specific settings
            string appSettingsPath = String.Format("{0}12345_000.SPPID.config",
                _adapterSettings["XmlPath"]);

            if (File.Exists(appSettingsPath))
            {
                AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
                _adapterSettings.AppendSettings(appSettings);
            }

            // and run the thing
            string relativePath = String.Format(@"{0}BindingConfiguration.{1}.{2}.xml",
                _settings["XmlPath"],
                _settings["ProjectName"],
                _settings["ApplicationName"]);

            // Ninject Extension requires fully qualified path.
            string bindingConfigurationPath = Path.Combine(
                _settings["BaseDirectoryPath"],
                relativePath);

            _kernel.Load(bindingConfigurationPath);

            _sppidDataLayer = _kernel.Get<IDataLayer2>();
        }

        //[Test]
        //public void Create()
        //{
        //    IList<string> identifiers = new List<string>() { 
        //        "Equip-001", 
        //        "Equip-002",
        //        "Equip-003", 
        //        "Equip-004"
        //    };

        //    Random random = new Random();

        //    IList<IDataObject> dataObjects = _sppidDataLayer.Create("Equipment", identifiers);
        //    foreach (IDataObject dataObject in dataObjects)
        //    {
        //        dataObject.SetPropertyValue("PumpType", "PT-" + random.Next(2, 10));
        //        dataObject.SetPropertyValue("PumpDriverType", "PDT-" + random.Next(2, 10));
        //        dataObject.SetPropertyValue("DesignTemp", (double)random.Next(2, 10));
        //        dataObject.SetPropertyValue("DesignPressure", (double)random.Next(2, 10));
        //        dataObject.SetPropertyValue("Capacity", (double)random.Next(2, 10));
        //        dataObject.SetPropertyValue("SpecificGravity", (double)random.Next(2, 10));
        //        dataObject.SetPropertyValue("DifferentialPressure", (double)random.Next(2, 10));
        //    }
        //    Response actual = _sppidDataLayer.Post(dataObjects);

        //    if (actual.Level != StatusLevel.Success)
        //    {
        //        throw new AssertionException(Utility.SerializeDataContract<Response>(actual));
        //    }

        //    Assert.IsTrue(actual.Level == StatusLevel.Success);
        //}

        //[Test]
        //public void Read()
        //{
        //    IList<string> identifiers = new List<string>() 
        //    { 
        //        "Equip-001", 
        //        "Equip-002", 
        //        "Equip-003", 
        //        "Equip-004" 
        //    };

        //    IList<IDataObject> dataObjects = _sppidDataLayer.Get("Equipment", identifiers);

        //    if (!(dataObjects.Count() > 0))
        //    {
        //        throw new AssertionException("No Rows returned.");
        //    }

        //    foreach (IDataObject dataObject in dataObjects)
        //    {
        //        Assert.IsNotNull(dataObject.GetPropertyValue("PumpType"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("PumpDriverType"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("DesignTemp"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("DesignPressure"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("Capacity"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("SpecificGravity"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("DifferentialPressure"));
        //    }
        //}

        //[Test]
        //public void ReadWithFilter()
        //{
        //    DataFilter dataFilter = new DataFilter
        //    {
        //        Expressions = new List<Expression>
        //        {
        //            new Expression
        //            {
        //                PropertyName = "PumpDriverType",
        //                RelationalOperator = RelationalOperator.EqualTo,
        //                Values = new Values
        //                {
        //                    "PDT-8",
        //                }
        //            }
        //        }
        //    };

        //    IList<IDataObject> dataObjects = _sppidDataLayer.Get("Equipment", dataFilter, 2, 0);

        //    if (!(dataObjects.Count() > 0))
        //    {
        //        throw new AssertionException("No Rows returned.");
        //    }

        //    Assert.AreEqual(dataObjects.Count(), 2);

        //    foreach (IDataObject dataObject in dataObjects)
        //    {
        //        Assert.IsNotNull(dataObject.GetPropertyValue("PumpType"));
        //        Assert.AreEqual(dataObject.GetPropertyValue("PumpDriverType"), "PDT-8");
        //        Assert.IsNotNull(dataObject.GetPropertyValue("DesignTemp"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("DesignPressure"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("Capacity"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("SpecificGravity"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("DifferentialPressure"));
        //    }
        //}

        //[Test]
        //public void GetDictionary()
        //{
        //    DataDictionary dictionary = _sppidDataLayer.GetDictionary();

        //    Assert.IsNotNull(dictionary);

        //    string dictionaryPath = String.Format("{0}DataDictionary.xml",
        //          _adapterSettings["XmlPath"]
        //        );

        //    Utility.Write<DataDictionary>(dictionary, dictionaryPath, true);
        //}
    }
}
