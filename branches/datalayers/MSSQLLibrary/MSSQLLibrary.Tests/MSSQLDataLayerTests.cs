using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using StaticDust.Configuration;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter.datalayer;

using System.Collections.Specialized;
using System.IO;
using System.Collections;


namespace iRINGTools.MSSQLLibrary
{
    [TestFixture]
    public class MSSQLLibraryTest
    {
        private string _baseDirectory = string.Empty;
        private NameValueCollection _settings = new NameValueCollection();
        private AdapterSettings _adapterSettings = new AdapterSettings();
        private MSSQLDataLayer _dataLayer;

        public MSSQLLibraryTest()
        {
            //_settings = new NameValueCollection();
            _settings["xmlPath"] = @"E:\iring-tools\branches\datalayers\MSSQLLibrary\MSSQLLibrary.Tests\XML\";
            _settings["ProjectName"] = "12345_000";
            _settings["ApplicationName"] = "DM";
            _settings["TestMode"] = "UseFiles"; //UseFiles/WriteFiles
            _settings["Scope"] = string.Format("{0}.{1}", "12345_000", "DM");
            _baseDirectory = Directory.GetCurrentDirectory();
            _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\bin"));
            _settings["BaseDirectoryPath"] = _baseDirectory;
            Directory.SetCurrentDirectory(_baseDirectory);

            _adapterSettings.AppendSettings(_settings);

            string appSettingsPath = String.Format("{0}mssql-configuration.12345_000.DM.xml",
                _adapterSettings["XmlPath"]);

            _dataLayer = new MSSQLDataLayer(_adapterSettings);
        }

        [Test]
        public void GetDictionary()
        {
            DataDictionary benchmark = null;

            DataDictionary dictionary = _dataLayer.GetDictionary();

            Assert.IsNotNull(dictionary);

            string path = String.Format("{0}DataDictionary.{1}.xml",
                  _adapterSettings["XmlPath"],
                  _adapterSettings["Scope"]
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
        public void Create()
        {
            IList<string> identifiers = new List<string>{ 
                "345-MV-9980", 
                "345-MV-9981", 
                "345-MV-9982",
                "345-MV-9983"
            };

            IList<IDataObject> dataObjects = _dataLayer.Create("ProjDbValve", identifiers);
            foreach (IDataObject dataObject in dataObjects)
            {
                dataObject.SetPropertyValue("DISPLAYED_TAG", string.Format("{0}-{1}-{2}",
                    dataObject.GetPropertyValue("VAREA"),
                    dataObject.GetPropertyValue("VTYP"),
                    dataObject.GetPropertyValue("VNUM"))
               );
                dataObject.SetPropertyValue("VSIZE", "40");
            }

            Response actual = _dataLayer.Post(dataObjects);

            if (actual.Level != StatusLevel.Success)
            {
                throw new AssertionException(Utility.SerializeDataContract<Response>(actual));
            }

            Assert.IsTrue(actual.Level == StatusLevel.Success);
        }

        [Test]
        public void Delete()
        {
            IList<string> identifiers = new List<string>{ 
                "345-MV-9980", 
                "345-MV-9981", 
                "345-MV-9982",
                "345-MV-9983"
            };

            Response actual = _dataLayer.Delete("ProjDbValve", identifiers);

            if (actual.Level != StatusLevel.Success)
            {
                throw new AssertionException(Utility.SerializeDataContract<Response>(actual));
            }

            Assert.IsTrue(actual.Level == StatusLevel.Success);
        }
    }
}
