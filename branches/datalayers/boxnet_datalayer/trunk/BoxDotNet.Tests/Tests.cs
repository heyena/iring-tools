using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using System.Data;
using System.Text;
using log4net;
using NUnit.Framework;
using StaticDust.Configuration;
using System.Runtime.Serialization;

//namespace org.iringtools.adapter.datalayer.test
namespace BoxDotNetDataLayer.Test
{
    [TestFixture]
    public class Tests
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Tests));
        private IDataLayer2 _dataLayer;
        //private Bechtel.DataLayer.BoxDotNetDataLayer _dataLayer = null;
        private string _objectType;
        private string _objectTypeFolder;
        private string _objectTypeDocument;        
        private DataObject _objectDefinition;

        string fileName_Upload;
        string parentId_Upload;
        string sourceFilePath_Upload;

        public Tests()
        {
            string baseDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(baseDir.Substring(0, baseDir.LastIndexOf("\\bin")));

            AdapterSettings adapterSettings = new AdapterSettings();
            adapterSettings.AppendSettings(new AppSettingsReader("App.config"));

            string twConfigFile = String.Format("{0}{1}.{2}.config",
                  adapterSettings["AppDataPath"],
                  adapterSettings["ProjectName"],
                  adapterSettings["ApplicationName"]);

            AppSettingsReader twSettings = new AppSettingsReader(twConfigFile);
            adapterSettings.AppendSettings(twSettings);

            _dataLayer = new Bechtel.DataLayer.BoxDotNetDataLayer(adapterSettings);
            _objectType = adapterSettings["ObjectType"];
            _objectDefinition = GetObjectDefinition(_objectType);

            // NP Box Upload start            
            fileName_Upload = adapterSettings["fileName_Upload"];
            parentId_Upload = adapterSettings["parentId_Upload"];
            sourceFilePath_Upload = adapterSettings["filePath_upload"];

            _objectTypeFolder = Bechtel.DataLayer.Constants.ObjectName.Folders.ToString();
            _objectTypeDocument = Bechtel.DataLayer.Constants.ObjectName.Files.ToString();
        }

        [Test]
        public void Test_Dictionary_Creation()
        {
            DataDictionary dictionary = _dataLayer.GetDictionary();
            Assert.Greater(dictionary.dataObjects.Count, 0);
        }

        [Test]
        public void GetFoldersById()
        {
            IList<string> list = new List<string>();
            list.Add("755324410"); //0, , 786621487
            list.Add("786621487");

            IList<IDataObject> dataObjects = _dataLayer.Get(_objectTypeFolder.ToString(), list);

            Assert.IsNotNull(dataObjects);
            Assert.GreaterOrEqual(dataObjects.Count, 1);
        }

        //[Test]
        //public void GetDocumentById()
        //{
        //    IList<string> identifiers = new List<string>() 
        //    {
        //        "7280685398", "7446631335"
        //    }; //7280685398 - test.txt, 7446631335 - xyz.txt

        //    IList<IDataObject> dataObjects = _dataLayer.Get(_objectTypeDocument.ToString(), identifiers);

        //    Assert.IsNotNull(dataObjects);
        //    Assert.GreaterOrEqual(dataObjects.Count,1);
        //}

        //[Test]
        //public void SearchDocuments()
        //{
        //    string query = "test";

        //    IList<IDataObject> dataObjects = _dataLayer.Search(_objectTypeDocument.ToString(), query, null, 25, 0);

        //    Assert.IsNotNull(dataObjects);
        //    Assert.GreaterOrEqual(dataObjects.Count, 1);
        //}

        //[Test]
        //public void GetFoldersByFilters()
        //{
        //    DataFilter datafilter = new DataFilter
        //    {
        //        Expressions = new List<Expression>
        //        {
        //            new Expression
        //            {
        //                PropertyName = "name", 
        //                RelationalOperator=RelationalOperator.EqualTo, 
        //                Values = new Values{"test",}
        //            },
        //            new Expression
        //            {
        //                PropertyName = "createdBy", 
        //                RelationalOperator=RelationalOperator.EqualTo,
        //                LogicalOperator = LogicalOperator.And,
        //                Values = new Values{"Ved Prakash",}
        //            },
        //             new Expression
        //            {
        //                PropertyName = "modifiedBy", 
        //                RelationalOperator=RelationalOperator.EqualTo,
        //                LogicalOperator = LogicalOperator.And,
        //                Values = new Values{"naresh pandey",}
        //            }
        //        }
        //    };

        //    IList<IDataObject> dataObjects = _dataLayer.Get(_objectTypeFolder.ToString(), datafilter, 30, 0);

        //    Assert.IsNotNull(dataObjects);
        //    Assert.GreaterOrEqual(dataObjects.Count, 1);
        //}

        //[Test]
        //public void GetDocumentsByFilters()
        //{
        //    DataFilter datafilter = new DataFilter
        //    {
        //        Expressions = new List<Expression>
        //        {
        //            new Expression
        //            {
        //                PropertyName = "name", 
        //                RelationalOperator=RelationalOperator.EqualTo, 
        //                Values = new Values{"test",}
        //            },
        //            new Expression
        //            {
        //                PropertyName = "createdBy", 
        //                RelationalOperator=RelationalOperator.EqualTo,
        //                LogicalOperator = LogicalOperator.And,
        //                Values = new Values{"naresh pandey",}
        //            },
        //             new Expression
        //            {
        //                PropertyName = "modifiedBy", 
        //                RelationalOperator=RelationalOperator.EqualTo,
        //                LogicalOperator = LogicalOperator.And,
        //                Values = new Values{"naresh pandey",}
        //            }
        //        }
        //    };

        //    IList<IDataObject> dataObjects = _dataLayer.Get(_objectTypeDocument.ToString(), datafilter, 30, 0);

        //    Assert.IsNotNull(dataObjects);
        //    Assert.GreaterOrEqual(dataObjects.Count, 1);
        //}

        //[Test]
        //public void Test_GetCount()
        //{
        //    DataFilter datafilter = new DataFilter
        //    {
        //        Expressions = new List<Expression>
        //        {
        //            new Expression
        //            {
        //                PropertyName = "name", 
        //                RelationalOperator=RelationalOperator.EqualTo, 
        //                Values = new Values{"test",}
        //            }                    
        //        }
        //    };

        //    long count = _dataLayer.GetCount(_objectTypeDocument.ToString(), datafilter);
        //    Assert.Greater(count, 1);
        //}


        //[Test]
        //public void TestPost()
        //{
        //    //Response response = _dataLayer.Post(null); // Document/file to upload is specified in App.conifig
        //    IList<IDataObject> dataObjects = _dataLayer.Create(_objectTypeDocument.ToString(), null);
        //    dataObjects[0].SetPropertyValue("name", fileName_Upload);
        //    dataObjects[0].SetPropertyValue("folderId", parentId_Upload);
        //    dataObjects[0].SetPropertyValue("path", sourceFilePath_Upload);

        //    Response response = _dataLayer.Post(dataObjects);

        //    Assert.AreEqual(response.Level, StatusLevel.Success);
        //}

        [Test] // Get the folders under a specific folder.
        public void GetFoldersUnderFolder()
        {
            DataFilter datafilter = new DataFilter
            {
                Expressions = new List<Expression>
                {
                    new Expression
                    {
                        PropertyName = "parentId", 
                        RelationalOperator=RelationalOperator.EqualTo, 
                        Values = new Values{"0",}
                    }
                }
            };

            IList<IDataObject> dataObjects = _dataLayer.Get(_objectTypeFolder.ToString(), datafilter, 15, 0);
            Assert.IsNotNull(dataObjects);
        }

        [Test] // Get the files under a specific folder.
        public void GetFilessUnderFolder()
        {
            DataFilter datafilter = new DataFilter
            {
                Expressions = new List<Expression>
                {
                    new Expression
                    {
                        PropertyName = "parentId", 
                        RelationalOperator=RelationalOperator.EqualTo, 
                        Values = new Values{"0",}
                    }
                }
            };

            IList<IDataObject> dataObjects = _dataLayer.Get(_objectTypeDocument.ToString(), datafilter, 15, 0);
            Assert.IsNotNull(dataObjects);
        }

        //[Test]
        //public void GetDocuments()
        //{
        //    IList<IDataObject> dataObjects = _dataLayer.Get(_objectTypeDocument.ToString(), null, 75, 0);

        //    Assert.IsNotNull(dataObjects);
        //    Assert.AreEqual(75, dataObjects.Count);
        //}

        //[Test]
        //public void GetCountItemsUnderFolder()
        //{
        //    IList<string> list = new List<string>();
        //    list.Add("786621487"); //0, 755324410, 786621487

        //    DataDictionary dictionary = _dataLayer.GetDictionary();
        //    long count = _dataLayer.GetItemsCount(_objectTypeFolder.ToString(), "0");
        //    //long count = _dataLayer.GetCount(_objectType    , null);
        //    Assert.GreaterOrEqual(count, 0);
        //}



        //[Test]


        //public void TestGet()
        //{
        //    //To Do
        //    //Get the list of files from Box.Net
        //}
        ////[Test]
        ////REST URL: http://localhost:54321/data/pw/pilot/dtp_eng2/8bdd1237-7e3f-415b-9e21-ad18b5d64f2b?format=doc
        //public void TestGetContent()
        //{
        //    // To Do 
        //    // Get content of the file.
        //}
        ////[Test]
        //public void TestPost()
        //{
        //    //To Do
        //    //Post file on Box.Net
        //}
        ////[Test]
        //public void TestDelete()
        //{
        //    //To Do
        //    // Delete File from Box.Net
        //}
        ////[Test]
        //public void TestGetFolders()
        //{
        //    // To Do 
        //    // Download Uploaded Folder.
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