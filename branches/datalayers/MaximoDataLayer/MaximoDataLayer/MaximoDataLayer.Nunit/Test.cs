using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using System.Collections.Specialized;
using System.IO;
using StaticDust.Configuration;

namespace iRingTools.MaximoDataLayer
{
    [TestFixture]
    public class MaximoDataLayerTest
    {
        private string _baseDirectory = string.Empty;
        private NameValueCollection _settings;
        private AdapterSettings _adapterSettings;
        private MaximoDataLayer _dataLayer;

        public MaximoDataLayerTest()
        {
            _settings = new NameValueCollection();

            _settings["ProjectName"] = "12345Maximo";
            _settings["XmlPath"] = @"..\MaximoDataLayer.NUnit\12345Maximo\";
            _settings["ApplicationName"] = "Maximo";
            _settings["TestMode"] = "UseFiles"; //UseFiles/WriteFiles

            _baseDirectory = Directory.GetCurrentDirectory();
            _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\bin"));
            _settings["BaseDirectoryPath"] = _baseDirectory;
            //_settings["BaseURL"] = "http://asbs50126.amers.ibechtel.com/maxrest/rest/os/mxasset?_lid=larvin&_lpwd=Ephesians01";
            _settings["BaseURL"] = "http://asbs50126.amers.ibechtel.com/maxrest/rest/mbo/asset?_lid=larvin&_lpwd=Ephesians01";
            _settings["BaseCredentials"] = "_lid=larvin&_lpwd=Ephesians01";

            Directory.SetCurrentDirectory(_baseDirectory);

            _adapterSettings = new AdapterSettings();
            _adapterSettings.AppendSettings(_settings);

            string appSettingsPath = String.Format("{0}12345Maximo.Maximo.config",
                _adapterSettings["XmlPath"]
            );

            if (File.Exists(appSettingsPath))
            {
                AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
                _adapterSettings.AppendSettings(appSettings);
            }

            _dataLayer = new MaximoDataLayer(_adapterSettings);
        }

        [Test]
        public void Create()
        {
            IList<string> identifiers = new List<string>() { 
                "Asset-040",
            };

            Random random = new Random();

            IList<IDataObject> dataObjects = _dataLayer.Create("Asset", identifiers);
            foreach (IDataObject dataObject in dataObjects)
            {
                //dataObject.SetPropertyValue("AssetNum", "ASSET-" + random.Next(2, 10));
                dataObject.SetPropertyValue("SiteId", "BLG");
                dataObject.SetPropertyValue("OrgId", "PMACWA");
                dataObject.SetPropertyValue("Description", "TEST LINDA DESC-1");
               // dataObject.SetPropertyValue("acwa_assettag", "LINDATEST-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Status", "ACTIVE");
                dataObject.SetPropertyValue("Location", "10-X9157");
                dataObject.SetPropertyValue("Manufacturer", "WALCHEM");
                dataObject.SetPropertyValue("acwa_assetsystem", "AFS");
                dataObject.SetPropertyValue("classstructureid", "1519");
                dataObject.SetPropertyValue("acwa_eng_tag", "Asset-033");
                dataObject.SetPropertyValue("acwa_qclass", "1");
                dataObject.SetPropertyValue("islinear", "0");
                dataObject.SetPropertyValue("acwa_futurelocation", "00001-A1");
                dataObject.SetPropertyValue("pluscmodelnum", "99");
                dataObject.SetPropertyValue("vendor", "GOLDER");
                dataObject.SetPropertyValue("serialnum", "99");
                dataObject.SetPropertyValue("description_longdescription", "TEST LINDA DESC-1");
                
                // dataObject.SetPropertyValue("HierarchyPath", "VALVE");


            }
            Response actual = _dataLayer.Post(dataObjects);


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
                "BG00690"              
            };

            IList<IDataObject> dataObjects = _dataLayer.Get("Asset", identifiers);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }
            if (dataObjects.Count() > 0)
            {
                foreach (IDataObject dataObject in dataObjects)
                {
                    //  Assert.IsNotNull(dataObject.GetPropertyValue("AssetNum"));
                    Assert.IsNotNull(dataObject.GetPropertyValue("OrgId"));
                    Assert.IsNotNull(dataObject.GetPropertyValue("SiteId"));
                    Assert.IsNotNull(dataObject.GetPropertyValue("Description"));
                    Assert.IsNotNull(dataObject.GetPropertyValue("acwa_assettag"));
                    Assert.IsNotNull(dataObject.GetPropertyValue("Status"));
                    Console.WriteLine(dataObject.GetPropertyValue("acwa_assettag"));
                }
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
                        PropertyName = "OrgId",
                        RelationalOperator = RelationalOperator.EqualTo,
                        Values = new Values
                        {
                            "PMACWA"
                        }
                    },
                  new Expression
                    {
                        PropertyName = "SiteId",
                        RelationalOperator = RelationalOperator.EqualTo,
                        Values = new Values
                        {
                            "BLG"
                        }
                    }  
                }
            };

            IList<IDataObject> dataObjects = _dataLayer.Get("Asset", dataFilter, 25, 0);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

            Assert.AreEqual(dataObjects.Count(), 25);

            foreach (IDataObject dataObject in dataObjects)
            {
                //Assert.IsNotNull(dataObject.GetPropertyValue("AssetNum"));
                Assert.AreEqual(dataObject.GetPropertyValue("OrgId"), "PMACWA");
                Assert.IsNotNull(dataObject.GetPropertyValue("SiteId"));
                Assert.IsNotNull(dataObject.GetPropertyValue("Description"));
                Assert.IsNotNull(dataObject.GetPropertyValue("acwa_assettag"));
                Assert.IsNotNull(dataObject.GetPropertyValue("Status"));

            }
        }

        [Test]
        public void GetDictionary()
        {
            DataDictionary benchmark = null;

            DataDictionary dictionary = _dataLayer.GetDictionary();

            Assert.IsNotNull(dictionary);

            string path = String.Format("{0}DataDictionary.{1}.xml",
                  _adapterSettings["XmlPath"],
                  _adapterSettings["ApplicationName"]
                );

            if (_settings["TestMode"].ToLower() != "usefiles")
            {
                Utility.Write<DataDictionary>(dictionary, path);
                Assert.AreNotEqual(null, dictionary);
            }
            else
            {
                benchmark = Utility.Read<DataDictionary>(path);
                Assert.AreEqual(
                  Utility.SerializeDataContract<DataDictionary>(benchmark),
                  Utility.SerializeDataContract<DataDictionary>(dictionary)
                );
            }
        }
        [Test]
        public void Delete()
        {
            IList<string> identifiers = new List<string>() 
        { 
           "Asset-038",
            "Asset-037",
             "Asset-036",
              "Asset-035",
               "Asset-034",
        };

            Response response = _dataLayer.Delete("Asset", identifiers);

            if (response.Level != StatusLevel.Success)
            {
                throw new AssertionException(Utility.SerializeDataContract<Response>(response));
            }

            Assert.IsTrue(response.Level == StatusLevel.Success);


        }
        [Test]
        public void DeletewithFilter()
        {
            DataFilter dataFilter = new DataFilter
            {
                Expressions = new List<Expression>
        {
          new Expression
          {
            PropertyName = "acwa_assettag",
            RelationalOperator = RelationalOperator.EqualTo,
            Values = new Values
            {
              "Asset-033",
            }
          },
        }
            };

            Response response = _dataLayer.Delete("Asset", dataFilter);

            if (response.Level != StatusLevel.Success)
            {
                throw new AssertionException(Utility.SerializeDataContract<Response>(response));
            }

            Assert.IsTrue(response.Level == StatusLevel.Success);


        }
    }
}