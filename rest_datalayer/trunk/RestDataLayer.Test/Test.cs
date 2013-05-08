using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using System.Xml.Linq;
using System.IO;
using System.Data;
using org.iringtools.library;
using log4net;
using org.iringtools.adapter;
using org.iringtools.utility;
using StaticDust.Configuration;

namespace RestDataLayer.Test
{
    [TestFixture]
    public class Tests
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Tests));
        private IDataLayer2 _dataLayer;
        private string _objectType;
        private string _objectId=string.Empty;  // NP
        private int _pageSize =100;  // NP
        private int _startIndex = 0;  // NP
        private string _ValueToSearch = string.Empty;  // NP
        IList<string> identifiers = new List<string>(); // NP
        private string _relatedDataObject;
        private string _modifiedProperty;
        private string _modifiedValue;
        private DataObject _objectDefinition;
        private DataFilter _filter;

        public Tests()
        {
            string baseDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(baseDir.Substring(0, baseDir.LastIndexOf("\\bin")));
            AdapterSettings adapterSettings = new AdapterSettings();
            adapterSettings.AppendSettings(new AppSettingsReader("App.config"));

            FileInfo log4netConfig = new FileInfo("Log4net.config");
            log4net.Config.XmlConfigurator.Configure(log4netConfig);

            string twConfigFile = String.Format("{0}{1}.{2}.config",
              adapterSettings["AppDataPath"],
              adapterSettings["ProjectName"],
              adapterSettings["ApplicationName"]
            );

            AppSettingsReader twSettings = new AppSettingsReader(twConfigFile); 
            adapterSettings.AppendSettings(twSettings);

            //_dataLayer = new Bechtel.DataLayer.RestDataLayer2(adapterSettings);
            _dataLayer = new Bechtel.DataLayer.RestDataLayer3(adapterSettings);
            
            _filter = Utility.Read<DataFilter>(adapterSettings["FilterPath"]);

            _objectType = adapterSettings["ObjectType"];

            _objectId = adapterSettings["ObjectId"];        // NP
            _pageSize = int.Parse(adapterSettings["pageSize"]);        // NP
            _startIndex = int.Parse(adapterSettings["startIndex"]);      // NP
            _ValueToSearch = adapterSettings["ValueToSearch"];   // NP
            

            _relatedDataObject = adapterSettings["RelatedObjectType"];
            _modifiedProperty = adapterSettings["ModifiedProperty"];
            _modifiedValue = adapterSettings["ModifiedValue"];
            _objectDefinition = GetObjectDefinition(_objectType);

            identifiers.Add(_objectId); //NP
        }

        [Test]
        public void Test_Dictionary_Creation() 
        {
            #region Test dictionary
            DataDictionary dictionary = _dataLayer.GetDictionary();
            Assert.Greater(dictionary.dataObjects.Count, 0);
            #endregion
        }

        [Test]
        public void Test_GetWithId()
        {
            DataDictionary dictionary = _dataLayer.GetDictionary();
            IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, identifiers);
            Assert.AreEqual(dataObjects.Count, 1);
        }

        [Test]
        public void Test_GetRelatedObjects_WithPaging()
        {
            //IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)            
            DataDictionary dictionary = _dataLayer.GetDictionary();
            
            IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, identifiers);
            Assert.AreEqual(dataObjects.Count, 1);

            IList<IDataObject> dataObject = _dataLayer.GetRelatedObjects(dataObjects[0], _relatedDataObject, _pageSize, _startIndex);

            //IList<IDataObject> dataObject = _dataLayer.GetRelatedObjects((IDataObject)_objectDefinition, relatedObjectType);

            Assert.GreaterOrEqual(dataObject.Count, 1);
        }

        [Test]
        public void Test_Search_WithPaging()
        {
            DataDictionary dictionary = _dataLayer.GetDictionary();

            IList<IDataObject> dataObject = _dataLayer.Search(_objectType, _ValueToSearch, null, _pageSize, _startIndex);  //(_objectType, identifiers);

            Assert.GreaterOrEqual(dataObject.Count, 1);
        }


        //[Test]
        //public void Test_GetCount()
        //{
        //    DataDictionary dictionary = _dataLayer.GetDictionary();
        //    long count = _dataLayer.GetCount(_objectType, null);
        //    Assert.Greater(count, 1);
        //}

        //[Test]
        //public void Test_Get_With_Filter()
        //{
        //    DataDictionary dictionary = _dataLayer.GetDictionary();
        //    IList<IDataObject> dataObject = _dataLayer.Get(_objectType, _filter, 10, 0);
        //    Assert.AreEqual(dataObject.Count, 1);
        //}

        //[Test]
        //public void TestCreate()
        //{
        //    IList<IDataObject> dataObjects = _dataLayer.Create(_objectType, null);
        //    Assert.AreNotEqual(dataObjects, null);
        //}

        //[Test]
        //public void TestPostWithUpdate()
        //{
        //    DataDictionary dictionary = _dataLayer.GetDictionary();
        //    // IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, null, 1, 0);
        //    IList<IDataObject> dataObjects = _dataLayer.Create(_objectType, null);

        //    dataObjects[0].SetPropertyValue(_modifiedProperty, _modifiedValue);

        //    Response response = _dataLayer.Post(dataObjects);
        //    Assert.AreEqual(response.Level, StatusLevel.Success);
        //}

        //[Test]
        //public void TestPostWithAddAndDeleteByIdentifier()
        //{
        //    //
        //    // create a new data object by getting an existing one and change its identifier
        //    //
        //    IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, new DataFilter(), 1, 1);
        //    string identifier = GetIdentifier(dataObjects[0]);

        //    string newIdentifier = GenerateStringValue();
        //    SetIdentifier(dataObjects[0], newIdentifier);

        //    // post the new data object
        //    Response response = _dataLayer.Post(dataObjects);
        //    Assert.AreEqual(response.Level, StatusLevel.Success);

        //    //
        //    // delete the new data object by its identifier
        //    //
        //    response = _dataLayer.Delete(_objectType, new List<string> { newIdentifier });
        //    Assert.AreEqual(response.Level, StatusLevel.Success);
        //}

        ////[Test]
        ////public void Test_Get_Data_With_Paging()
        ////{
        ////  DataDictionary  dictionary = _dataLayer.GetDictionary();
        //// IList<string> identifiers = new List<string>();
        ////  identifiers.Add("1");

        ////  IList<IDataObject> dataObject = _dataLayer.Get("Function", null, 10, 2);
        ////  Assert.AreEqual(dataObject.Count, 10);


        ////}

        ////[Test]
        ////public void Test_Get_Identifiers()
        ////{

        ////    IList<string> identifiers = _dataLayer.GetIdentifiers("Function", null);
        ////    Assert.Greater(identifiers.Count, 0);

        ////}

        //[Test]
        //public void TestGetWithIdentifiers()
        //{
        //    IList<string> identifiers = _dataLayer.GetIdentifiers("Function", new DataFilter());
        //    IList<string> identifier = ((List<string>)identifiers).GetRange(1, 1);
        //    IList<IDataObject> dataObjects = _dataLayer.Get("contacts", identifier);

        //    Assert.Greater(dataObjects.Count, 0);
        //}

        //[Test]
        //public void Test_GetDataTable()
        //{
        //    DataDictionary dictionary = _dataLayer.GetDictionary();
        //    IList<string> identifiers = new List<string>();
        //    identifiers.Add("99784x");

        //    IList<IDataObject> dataObject = _dataLayer.Get(_objectType, identifiers);

        //    Assert.AreEqual(dataObject.Count, 1);
        //}

        #region helper methods
        private string GenerateStringValue()
        {
            //return DateTime.Now.ToUniversalTime().Ticks.ToString();
            Random rnd = new Random();
            return rnd.Next(5, DateTime.Now.Second + 5).ToString();
        }

        private string GetIdentifier(IDataObject dataObject)
        {
            string[] identifierParts = new string[_objectDefinition.keyProperties.Count];

            int i = 0;
            foreach (KeyProperty keyProperty in _objectDefinition.keyProperties)
            {
                identifierParts[i] = dataObject.GetPropertyValue(keyProperty.keyPropertyName).ToString();
                i++;
            }

            return String.Join(_objectDefinition.keyDelimeter, identifierParts);
        }

        private void SetIdentifier(IDataObject dataObject, string identifier)
        {
            IList<string> keyProperties = GetKeyProperties();

            if (keyProperties.Count == 1)
            {
                dataObject.SetPropertyValue(keyProperties[0], identifier);
            }
            else if (keyProperties.Count > 1)
            {
                StringBuilder identifierBuilder = new StringBuilder();

                foreach (string keyProperty in keyProperties)
                {
                    dataObject.SetPropertyValue(keyProperty, identifier);

                    if (identifierBuilder.Length > 0)
                    {
                        identifierBuilder.Append(_objectDefinition.keyDelimeter);
                    }

                    identifierBuilder.Append(identifier);
                }

                identifier = identifierBuilder.ToString();
            }
        }

        private IList<string> GetKeyProperties()
        {
            IList<string> keyProperties = new List<string>();

            foreach (DataProperty dataProp in _objectDefinition.dataProperties)
            {
                foreach (KeyProperty keyProp in _objectDefinition.keyProperties)
                {
                    if (dataProp.propertyName == keyProp.keyPropertyName)
                    {
                        keyProperties.Add(dataProp.propertyName);
                    }
                }
            }
            return keyProperties;
        }

        private DataObject GetObjectDefinition(string objectType)
        {
            DataDictionary dictionary = _dataLayer.GetDictionary();

            if (dictionary.dataObjects != null)
            {
                foreach (DataObject dataObject in dictionary.dataObjects)
                {
                    if (dataObject.objectName.ToLower() == objectType.ToLower())
                    {
                        return dataObject;
                    }
                }
            }
            return null;
        }

        #endregion helper methods
    }
}
