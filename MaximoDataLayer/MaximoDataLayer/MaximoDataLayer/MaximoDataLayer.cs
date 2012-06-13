using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using log4net;
using Ninject;
using org.iringtools.library;
using org.iringtools.adapter;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Helpers;
using StaticDust.Configuration;

namespace iRingTools.MaximoDataLayer
{
  
    public class MaximoDataLayer : BaseDataLayer
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MaximoDataLayer));
        private List<IDataObject> _dataObjects = null;
        protected XmlDocument _dataXmlDocument;
        private string _dataPath = string.Empty;
        private string _scope = string.Empty;
        private string _dictionaryPath = string.Empty;
        private DataDictionary _dictionary = null;

        private string _server = string.Empty;
        private string _dataSource = string.Empty;
        private string _userName = string.Empty;
        private string _password = string.Empty;
        private string _communityName = string.Empty;
        private string _classObjects = string.Empty;
        private string _keyDelimiter = string.Empty;
      
        //private Dictionary<string, Configuration> _configs = null;
        
        [Inject]
        public MaximoDataLayer(AdapterSettings settings) : base(settings)
        {
            try
            {
                //_settings = settings;
                _dataPath = settings["DataLayerPath"];
                if (_dataPath == null)
                {
                    _dataPath = settings["AppDataPath"];
                }

                _scope = _settings["ProjectName"] + "." + _settings["ApplicationName"];

                //
                // Load AppSettings
                //
                string appSettingsPath = string.Format("{0}{1}.config", _dataPath, _scope);
                if (!System.IO.File.Exists(appSettingsPath))
                {
                    _dataPath += "App_Data\\";
                    appSettingsPath = string.Format("{0}{1}.config", _dataPath, _scope);
                }
                _settings.AppendSettings(new AppSettingsReader(appSettingsPath));

                _dictionaryPath = string.Format("{0}DataDictionary.{1}.xml", _dataPath, _scope);

                //_server = _settings["ebServer"];
                //_dataSource = _settings["ebDataSource"];
                //_userName = _settings["ebUserName"];
                //_password = _settings["ebPassword"];
                //_classObjects = _settings["ebClassObjects"];

                //_keyDelimiter = _settings["ebKeyDelimiter"];
                //if (_keyDelimiter == null)
                //{
                //    _keyDelimiter = ";";
                //}

                //_communityName = _settings["ebCommunityName"];
                //string[] configFiles = Directory.GetFiles(_dataPath, "*" + _communityName + ".xml");
                //string ruleFile = _dataPath + "Rules_" + _communityName + ".xml";

                //
                // Load configuration files
                //
                //_configs = new Dictionary<string, Configuration>(StringComparer.OrdinalIgnoreCase);

                //foreach (string configFile in configFiles)
                //{
                //    if (configFile.ToLower() != ruleFile.ToLower())
                //    {
                //        string fileName = Path.GetFileName(configFile);
                //        Configuration config = Utility.Read<Configuration>(configFile, false);
                //        _configs[fileName] = config;
                //    }
                //}

                // Load rule file
                //_rules = Utility.Read<Rules>(ruleFile, false);
            }
            catch (Exception e)
            {
                _logger.Error("Error initializing MaximoDataLayer: " + e.Message);
            }
        }
  
        public override Response Delete(string objectType, IList<string> identifiers)
        {
            Response response = new Response();

            if (identifiers == null || identifiers.Count == 0)
            {
                Status status = new Status();
                status.Level = StatusLevel.Warning;
                status.Messages.Add("Nothing to delete.");
                response.Append(status);
                return response;
            }

            //Get Path from Scope.config ({project}.{app}.config)
            string dataObjectPath = String.Format(
              "{0}\\{1}",
              _settings["MaximoFolderPath"],
              objectType
            );

            foreach (string identifier in identifiers)
            {
                Status status = new Status();
                status.Identifier = identifier;
                string value = "";
                string assetseq = "";
                try
                {
                   
                    //check if it exists first
                    //WebRequest wRequest1 = WebRequest.Create("http://asbs50126.amers.ibechtel.com/maxrest/rest/os/mxasset?_lid=larvin&_lpwd=Ephesians01" + "&assetid=~eq~101008" + "&assetnum = ~eq~ " + identifier);
                    //WebRequest wRequest1 = WebRequest.Create("http://asbs50126.amers.ibechtel.com/maxrest/rest/os/mxasset?_lid=larvin&_lpwd=Ephesians01&assetid=~eq~101008&siteid=~eq~BLG");
                    //WebRequest wRequest1 = WebRequest.Create("http://asbs50126.amers.ibechtel.com/maxrest/rest/os/mxasset/101008?_lid=larvin&_lpwd=Ephesians01");
                    //above only works if Asset exists already - otherwise it doesn't work
                    WebRequest wRequest1 = WebRequest.Create("http://asbs50126.amers.ibechtel.com/maxrest/rest/os/mxasset?_lid=larvin&_lpwd=Ephesians01&acwa_assettag=~eq~" + identifier);
                     
                    //if QueryMXASSETResponse attribute rsCount = 0 then nothing to delete
                    WebResponse wResponse1 = wRequest1.GetResponse();
                    //Console.WriteLine(((HttpWebResponse)wResponse1).StatusDescription);

                    Stream dataStream1 = wResponse1.GetResponseStream();
                    StreamReader reader1 = new StreamReader(dataStream1);
                    var responsefromServer = reader1.ReadToEnd();
           
                    //Console.WriteLine(responsefromServer);
                    reader1.Close();
                    dataStream1.Close();
                    wResponse1.Close();

                    using (XmlReader reader = XmlReader.Create(new StringReader(responsefromServer)))
                    {
                        
                        reader.ReadToFollowing("QueryMXASSETResponse");
                        reader.MoveToAttribute("rsCount");

                        value = reader.Value;
                        //Console.WriteLine(value);
                        if (Convert.ToInt32(value) == 1)
                        {
                            reader.ReadToFollowing(objectType.ToUpper());
                            reader.ReadToFollowing("ASSETID");
                            assetseq = reader.ReadElementContentAsString();
                        }
                    }
                    if (Convert.ToInt32(value)==1)
                    {
                        // WebRequest wRequest = WebRequest.Create("http://asbs50126.amers.ibechtel.com/maxrest/rest/os/mxasset/" + identifier + "?_lid=larvin&_lpwd=Ephesians01");
                        WebRequest wRequest = WebRequest.Create("http://asbs50126.amers.ibechtel.com/maxrest/rest/mbo/asset/" + assetseq + "?_lid=larvin&_lpwd=Ephesians01");

                        //WebRequest wRequest = WebRequest.Create(_settings["BaseURL"] + "/" + identifier);
                        wRequest.Method = "DELETE";
                        //wRequest.Timeout = 5000; 
                        //string postData = identifier;
                        //byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                        //wRequest.ContentType = "application/x-www-form-urlencoded";
                        //wRequest.ContentLength = byteArray.Length;

                        //Stream dataStream1 = wRequest.GetRequestStream();
                        // Write the data to the request stream.
                        //dataStream1.Write(byteArray, 0, byteArray.Length);
                        // Close the Stream object.
                        //dataStream1.Close();

                        WebResponse wResponse = wRequest.GetResponse();
                        //Console.WriteLine(((HttpWebResponse)wResponse).StatusDescription);

                        Stream dataStream = wResponse.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        responsefromServer = reader.ReadToEnd();
                        //Console.WriteLine(responsefromServer);
                        reader.Close();
                        dataStream.Close();
                        wResponse.Close();
                    }
                    string message = String.Format(
                      "DataObject [{0}] deleted successfully.",
                      identifier
                    );

                    status.Messages.Add(message);
                }
                catch (Exception ex)
                {
                    _logger.Error("Error in Delete: " + ex);

                    status.Level = StatusLevel.Error;

                    string message = String.Format(
                      "Error while deleting dataObject [{0}]. {1}",
                      identifier,
                      ex
                    );

                    status.Messages.Add(message);
                }

                response.Append(status);
            }

            return response;
        }
        public override Response Delete(string objectType, DataFilter filter)
        {
            try
            {
                IList<string> identifiers = new List<string>();

                //NOTE: pageSize of 0 indicates that all rows should be returned.
                IList<IDataObject> dataObjects = Get(objectType, filter, 1, 0);
                DataObject objDef = GetObjectDefinition(objectType);
                foreach (IDataObject dataObject in dataObjects)
                {
                    identifiers.Add(Convert.ToString(dataObject.GetPropertyValue(objDef.keyProperties.First().keyPropertyName)));
                }

                return Delete(objectType, identifiers);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in Delete: " + ex);

                throw new Exception(
                  "Error while deleting data objects of type [" + objectType + "].",
                  ex
                );
            }
        }
        public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
        {
            string exMessage = null;
            try
            {
                
                LoadDataDictionary(objectType);

                IList<IDataObject> allDataObjects = LoadDataObjectsfromREST(objectType, pageSize, startIndex);
                
                // Apply filter
                if (filter != null && filter.Expressions != null && filter.Expressions.Count > 0)
                {
                    
                    var predicate = filter.ToPredicate(_dataObjectDefinition);

                    if (predicate != null)
                    {
                        
                        _dataObjects = allDataObjects.AsQueryable().Where(predicate).ToList();
                    }
                }
                
                if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
                {
                    throw new NotImplementedException("OrderExpressions are not supported by the CSV DataLayer.");
                }
               
               // Page and Sort The Data
                if (pageSize > allDataObjects.Count())
                    pageSize = allDataObjects.Count();
               // _dataObjects = _dataObjects.GetRange(startIndex, pageSize);
                exMessage += "step 5" + startIndex;
                _dataObjects = allDataObjects.ToList();
                return _dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetList: " + ex);

                throw new Exception(
                  "Error while getting a list of data objects of type [" + objectType + "]." + exMessage,
                  ex
                );
            }
        }
        public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
        {
            try
            {
                LoadDataDictionary(objectType);

                IList<IDataObject> allDataObjects = LoadDataObjectsfromREST(objectType, 25, 0);

                var expressions = FormMultipleKeysPredicate(identifiers);
                //Console.WriteLine("expressions " + expressions.Body);
                //if (expressions != null)
                //{
                //    _dataObjects = allDataObjects.AsQueryable().Where(expressions).ToList();
                //    Console.WriteLine("dataobjects " + _dataObjects.Count);
                //}
                
                _dataObjects = allDataObjects.ToList();
                //_dataObjects = (List <IDataObject>) allDataObjects;
                //Console.WriteLine("dataobjects " + _dataObjects.Count);
                return _dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetList: " + ex);
                throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
            }
        }
        public XmlDocument  DataXMLDocument
         {
            get
        {
            return _dataXmlDocument;
        }
        }

        private IList<IDataObject> LoadDataObjectsfromREST(string objectType, int pageSize, int startIndex)
        {
            try
            {
                IList<IDataObject> dataObjects = new List<IDataObject>();            
                
                IDataObject dataObject = null;
                
                Int32 rsTotal = 0;
                //need to loop through maxitems - changing rsstart and looking at rsTotal-_usc
                //WebRequest wRequest = WebRequest.Create("http://asbs50126.amers.ibechtel.com/maxrest/rest/os/mxasset?_lid=larvin&_lpwd=Ephesians01" + "&_dropnulls=0&_maxItems=" + pageSize + "&_rsStart=" + startIndex);
                WebRequest wRequest = WebRequest.Create(_settings["BaseURL"] + "&_lpwd=Ephesians01" + "&_dropnulls=0&_maxItems=" + pageSize + "&_rsStart=" + startIndex);
           
                // WebRequest wRequest = WebRequest.Create(_settings["BaseURL"] + "&_dropnulls=0&_maxItems=" + pageSize + "&_rsStart=" + startIndex + "&_format=json&_compact=1");                            

                WebResponse wResponse = wRequest.GetResponse();
                
                //Console.WriteLine(((HttpWebResponse)wResponse).StatusCode);               
                //Console.WriteLine(((HttpWebResponse)wResponse).StatusDescription);

                Stream dataStream = wResponse.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                var responsefromServer = reader.ReadToEnd();

                //DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(WebResponse));
                //object objResponse = jsonSerializer.ReadObject(dataStream);
                //WebResponse jsonResponse = objResponse as WebResponse;
                //var dynamicObject = Json.Decode(responsefromServer); 
                //for(var i=0; i < json.Super.length; i++) {
   // alert("Name: "+json.Super[i].Name + " Location: "+json.Super[i].Location);


                //IDictionary<string, object> deserializedJson = jsonResponse.ToDictionary(); 


                reader.Close();
                dataStream.Close();
                wResponse.Close();
                //Console.WriteLine(responsefromServer);
                //need to iterate through the whole response to get all assets
                using (XmlReader reader2 = XmlReader.Create(new StringReader(responsefromServer)))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(reader2);
                    _dataXmlDocument = new XmlDocument(); 
                    _dataXmlDocument.Load(reader2);
                    //Console.WriteLine(xmlDoc.SelectNodes("//*"));
                    XmlNode oNode = xmlDoc.DocumentElement;
                    //Console.WriteLine(oNode.OuterXml);
                    //XmlNodeList xnList = xmlDoc.SelectNodes("QueryMXASSETResponse/MXASSETSet/ASSET");
                   //rsTotal = Convert.ToInt32(oNode.Attributes["rsTotal"].Value);
                    //Console.WriteLine(rsTotal);
                    XmlNodeList xnList = xmlDoc.GetElementsByTagName(objectType.ToUpper());
                    //Console.WriteLine(xnList.Count);
                    foreach (XmlNode xn in xnList)
                    {
                       // Console.WriteLine(xn.OuterXml);
                        dataObject = FormRestDataObject(objectType, xn.OuterXml);
                        if (dataObject != null)
                            dataObjects.Add(dataObject);
                    }


                    //  dataObject = FormRestDataObject(objectType, responsefromServer);

                    
                }
                //startIndex = startIndex + 200;
                //while (startIndex < 6000)
                //{
                //    wRequest = WebRequest.Create(_settings["BaseURL"] + "&_dropnulls=0&_maxItems=" + pageSize + "&_rsStart=" + startIndex);
                //    wResponse = wRequest.GetResponse();

                //    Console.WriteLine(((HttpWebResponse)wResponse).StatusCode);
                //    Console.WriteLine(((HttpWebResponse)wResponse).StatusDescription);

                //    dataStream = wResponse.GetResponseStream();
                //    reader = new StreamReader(dataStream);
                //    responsefromServer = reader.ReadToEnd();
                //    reader.Close();
                //    dataStream.Close();
                //    wResponse.Close();

                //    //need to iterate through the whole response to get all assets
                //    using (XmlReader reader2 = XmlReader.Create(new StringReader(responsefromServer)))
                //    {
                //        XmlDocument xmlDoc = new XmlDocument();
                //        xmlDoc.Load(reader2);
                //        //Console.WriteLine(xmlDoc.SelectNodes("//*"));
                //        XmlNode oNode = xmlDoc.DocumentElement;

                //        //XmlNodeList xnList = xmlDoc.SelectNodes("QueryMXASSETResponse/MXASSETSet/ASSET");
                //        Int32 rsCount = Convert.ToInt32(oNode.Attributes["rsCount"].Value);
                //        Console.WriteLine(rsCount);
                //        XmlNodeList xnList = xmlDoc.GetElementsByTagName("ASSET");
                //        Console.WriteLine(xnList.Count);
                //        foreach (XmlNode xn in xnList)
                //        {
                //            // Console.WriteLine(xn.OuterXml);
                //            dataObject = FormRestDataObject(objectType, xn.OuterXml);
                //        }


                //        //  dataObject = FormRestDataObject(objectType, responsefromServer);

                //        if (dataObject != null)
                //            dataObjects.Add(dataObject);
                //    }
                //    startIndex = startIndex + 200;
                //}
                //Console.WriteLine(dataObjects.Count);
                return dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in LoadDataObjectsREST: " + ex);
                throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
            }
        }
 
        private IDataObject FormRestDataObject(string objectType, string xmlRow)
        {
            try
            {
                IDataObject dataObject = new GenericDataObject
                {
                    ObjectType = objectType,
                };

                XElement commodityElement = GetConfig(objectType);

                if (!String.IsNullOrEmpty(xmlRow))
                {
                    IEnumerable<XElement> attributeElements = commodityElement.Element("attributes").Elements("attribute");

                    //we need to split out the xml tags that are returned
                   // string[] xmlValues = xmlRow.Split(',');
  
                    //int index = 0;
                    string value = "";
                    foreach (var attributeElement in attributeElements)
                    {
              
                        string name = attributeElement.Attribute("name").Value;
                        string dataType = attributeElement.Attribute("dataType").Value.ToLower();

                       // XmlReader rdr = XmlReader.Create(new System.IO.StringReader(xmlRow));
                        using (XmlReader reader = XmlReader.Create(new StringReader(xmlRow)))
                        {
                            reader.ReadToFollowing(objectType.ToUpper());
                            reader.ReadToFollowing(name.ToUpper());
                            //Console.WriteLine(name.ToUpper());
                            value = reader.ReadElementContentAsString();
                            //Console.WriteLine(value);
                        }
                                            

                        // if data type is not nullable, make sure it has a value
                        if (!(dataType.EndsWith("?") && value == String.Empty))
                        {
                            if (dataType.Contains("bool"))
                            {
                                if (value.ToUpper() == "TRUE" || value.ToUpper() == "YES")
                                {
                                    value = "1";
                                }
                                else
                                {
                                    value = "0";
                                }
                            }
                            else if (value == String.Empty && (
                                     dataType.StartsWith("int") ||
                                     dataType == "double" ||
                                     dataType == "single" ||
                                     dataType == "float" ||
                                     dataType == "decimal"))
                            {
                                value = "0";
                            }
                        }

                        dataObject.SetPropertyValue(name, value);
                    }
                }

                return dataObject;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in FormDataObject: " + ex);

                throw new Exception(
                  "Error while forming a dataObject of type [" + objectType + "] from a XML row.",
                  ex
                );
            }
        }
        private XElement GetConfig(string objectType)
        {
            if (_configuration == null)
            {
                LoadConfiguration();
            }

            XElement commodityConfig = _configuration.Elements("commodity").Where(o => o.Element("name").Value == objectType).First();

            return commodityConfig;
        }
        public override DataDictionary GetDictionary()
        {
            DataDictionary dataDictionary = new DataDictionary();

            LoadConfiguration();

            List<DataObject> dataObjects = new List<DataObject>();
            List<PicklistObject> picklists = new List<PicklistObject>();
            foreach (XElement commodity in _configuration.Elements("commodity"))
            {
                string name = commodity.Element("name").Value;

                DataObject dataObject = new DataObject
                {
                    objectName = name,
                    tableName = name,
                    keyDelimeter = "_",
                };


                List<KeyProperty> keyProperties = new List<KeyProperty>();
                List<DataProperty> dataProperties = new List<DataProperty>();
                List<DataRelationship> dataRelations = new List<DataRelationship>();

                foreach (XElement attribute in commodity.Element("attributes").Elements("attribute"))
                {
                    bool isKey = false;
                    if (attribute.Attribute("isKey") != null)
                    {
                        Boolean.TryParse(attribute.Attribute("isKey").Value, out isKey);
                    }

                    string attributeName = attribute.Attribute("name").Value;
                   // string attributekeyType = attribute.Attribute("keyType").Value;
                    KeyType keyType = KeyType.unassigned;
                    Enum.TryParse<KeyType>(attribute.Attribute("keyType").Value, out keyType);
                    //string dataRelationship = attribute.Attribute("dataRelationship").Value;

                    DataType dataType = DataType.String;
                    Enum.TryParse<DataType>(attribute.Attribute("dataType").Value, out dataType);

                    int dataLength = 0;
                    if (DataDictionary.IsNumeric(dataType))
                    {
                        dataLength = 16;
                    }
                    else
                    {
                        dataLength = 255;
                    }

                    DataProperty dataProperty = new DataProperty
                    {
                        propertyName = attributeName,
                        dataType = dataType,
                        dataLength = dataLength,
                        isNullable = true,
                        showOnIndex = false,
                        keyType = keyType,
                    };

                    if (isKey)
                    {
                        dataProperty.isNullable = false;
                        dataProperty.showOnIndex = true;

                        KeyProperty keyProperty = new KeyProperty
                        {
                            keyPropertyName = attributeName,                          
                        };

                        keyProperties.Add(keyProperty);
                    }

                    dataProperties.Add(dataProperty);

                  //  RelationshipType relationshipType = RelationshipType.OneToMany;
                   // DataRelationship dataRelation = new DataRelationship
                   // {
                       // relationshipName = dataRelationship,
                        //Enum.TryParse<RelationshipType>(dataRelationship, out relationshipType),
                   // };
                    //dataRelations.Add(dataRelation);
                }

                dataObject.keyProperties = keyProperties;
                dataObject.dataProperties = dataProperties;
               // dataObject.dataRelationships = dataRelations;
                dataObjects.Add(dataObject);
            }



            dataDictionary.dataObjects = dataObjects;
            foreach (XElement commodity in _configuration.Elements("lookup"))
            {
                string name = commodity.Element("name").Value;

               

                PicklistObject picklist = new PicklistObject
                {
                    name = name,
                    tableName = name,
                };

                List<KeyProperty> keyProperties = new List<KeyProperty>();
                List<DataProperty> dataProperties = new List<DataProperty>();
               
                foreach (XElement attribute in commodity.Element("attributes").Elements("attribute"))
                {
                    bool isKey = false;
                    if (attribute.Attribute("isKey") != null)
                    {
                        Boolean.TryParse(attribute.Attribute("isKey").Value, out isKey);
                    }

                    string attributeName = attribute.Attribute("name").Value;

                    DataType dataType = DataType.String;
                    Enum.TryParse<DataType>(attribute.Attribute("dataType").Value, out dataType);

                    int dataLength = 0;
                    if (DataDictionary.IsNumeric(dataType))
                    {
                        dataLength = 16;
                    }
                    else
                    {
                        dataLength = 255;
                    }

                    DataProperty dataProperty = new DataProperty
                    {
                        propertyName = attributeName,
                        dataType = dataType,
                        dataLength = dataLength,
                        isNullable = true,
                        showOnIndex = false,
                    };

                    if (isKey)
                    {
                        dataProperty.isNullable = false;
                        dataProperty.showOnIndex = true;

                        KeyProperty keyProperty = new KeyProperty
                        {
                            keyPropertyName = attributeName,
                        };

                        keyProperties.Add(keyProperty);
                    }

                    dataProperties.Add(dataProperty);
                }
                picklist.pickListProperties = dataProperties;
                picklists.Add(picklist);
            }
            dataDictionary.picklists = picklists;

            return dataDictionary;
        }

        private void LoadConfiguration()
        {
            if (_configuration == null)
            {
                string uri = String.Format(
                    "{0}Configuration.{1}.xml",
                    _settings["XmlPath"],
                    _settings["ApplicationName"]
                );

                XDocument configDocument = XDocument.Load(uri);
                _configuration = configDocument.Element("configuration");
            }
        }
        public override long GetCount(string objectType, DataFilter filter)
        {
            try
            {
                //NOTE: pageSize of 0 indicates that all rows should be returned.
                IList<IDataObject> dataObjects = Get(objectType, filter, 25, 0);

                return dataObjects.Count();
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetIdentifiers: " + ex);

                throw new Exception(
                  "Error while getting a count of type [" + objectType + "].",
                  ex
                );
            }
        }
        public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
        {
            try
            {
                List<string> identifiers = new List<string>();

                //NOTE: pageSize of 0 indicates that all rows should be returned.
                IList<IDataObject> dataObjects = Get(objectType, filter, 25, 0);
               // IList<IDataObject> dataObjects = Get(objectType, filter, 0, -1);
                DataObject objDef = GetObjectDefinition(objectType);
               
                foreach (IDataObject dataObject in dataObjects)
                {
                    identifiers.Add(Convert.ToString(dataObject.GetPropertyValue(objDef.keyProperties.First().keyPropertyName)));
                }


                return identifiers;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetIdentifiers: " + ex);

                throw new Exception(
                  "Error while getting a list of identifiers of type [" + objectType + "].",
                  ex
                );
            }
        }
        protected DataObject GetObjectDefinition(string objectType)
        {
            DataDictionary dictionary = GetDictionary();
            DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
            return objDef;
        }
        public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
        {
            throw new NotImplementedException();
        }
      

        public override Response Post(IList<IDataObject> dataObjects)
        {
            Response response = new Response();
            string objectType = String.Empty;

            if (dataObjects == null || dataObjects.Count == 0)
            {
                Status status = new Status();
                status.Level = StatusLevel.Warning;
                status.Messages.Add("Nothing to update.");
                response.Append(status);
                return response;
            }

            try
            {
                objectType = ((GenericDataObject)dataObjects.FirstOrDefault()).ObjectType;

                LoadDataDictionary(objectType);

               // IList<IDataObject> existingDataObjects = new List<IDataObject>();
               // IList<IDataObject> existingDataObjects = LoadDataObjectsfromREST(objectType, 1, 0);

                foreach (IDataObject dataObject in dataObjects)
                {
                 //   IDataObject existingDataObject = null;

                    string identifier = GetIdentifier(dataObject);
                   
                    var predicate = FormKeyPredicate(identifier);
                    
                //    if (predicate != null)
                //    {
                //        existingDataObject = existingDataObjects.AsQueryable().Where(predicate).FirstOrDefault();
                        
                //    }
                    

                    //if (existingDataObject != null)
                    //{
                    //    existingDataObjects.Remove(existingDataObject);
                    //    Console.WriteLine("Remove");
                    //}

                    //TODO: Should this be per property?  Will it matter?
                  //  existingDataObjects.Add(dataObject);
                  //  Console.WriteLine(existingDataObjects.Count());
                }
                
                response = SaveDataObjects(objectType, dataObjects);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in Post: " + ex);

                throw new Exception(
                  "Error while posting dataObjects of type [" + objectType + "].",
                  ex
                );
            }
        }
        private Response SaveDataObjects(string objectType, IList<IDataObject> dataObjects)
        {
            try
            {
                Response response = new Response();

                // Create data object directory in case it does not exist
              //  Directory.CreateDirectory(_settings["MaximoFolderPath"]);

                //string path = String.Format(
                //  "{0}\\{1}.csv",
                //  _settings["MaximoFolderPath"],
                //  objectType
                //);

                //TODO: Need to update file, not replace it!
              //  TextWriter writer = new StreamWriter(path);

                foreach (IDataObject dataObject in dataObjects)
                {
                    Status status = new Status();

                    try
                    {
                        string identifier = GetIdentifier(dataObject);
                        status.Identifier = identifier;
                        //Console.WriteLine(identifier);
                        //List<string> csvRow = FormCSVRow(objectType, dataObject);
                        string xmlRow = FormCreateXMLRow(objectType, dataObject);

                        //writer.WriteLine(String.Join(", ", csvRow.ToArray()));
                       
                        //Call Create Rest API for Asset
                        //Console.WriteLine(xmlRow);

                        WebRequest wRequest = WebRequest.Create(_settings["BaseURL"]);
                        wRequest.Method = "POST";
                        //wRequest.Timeout = 5000; 
                        //string postData = "&AssetNum=~eq~Asset-028&SiteId=~eq~BLG&OrgId=~eq~PMACWA&Description=~eq~TEST LINDA DESC-6&acwa_assettag=~eq~LINDATEST-2&Status=~eq~ACTIVE&Location=~eq~10-X9157&Manufacturer=~eq~WALCHEM&acwa_assetsystem=~eq~AFS&classstructureid=~eq~1519";
                        string postData =  xmlRow;
                        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                        wRequest.ContentType = "application/x-www-form-urlencoded";
                        wRequest.ContentLength = byteArray.Length;

                        Stream dataStream1 = wRequest.GetRequestStream();
                        // Write the data to the request stream.
                        dataStream1.Write(byteArray, 0, byteArray.Length);
                        // Close the Stream object.
                        dataStream1.Close();
                       // Console.WriteLine(wRequest.RequestUri.ToString());
                        WebResponse wResponse = wRequest.GetResponse();
                        //Console.WriteLine(((HttpWebResponse)wResponse).StatusDescription);

                        Stream dataStream = wResponse.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        var responsefromServer = reader.ReadToEnd();
                        //Console.WriteLine(responsefromServer);
                        reader.Close();
                        dataStream.Close();
                        wResponse.Close();
                        status.Messages.Add("Record [" + identifier + "] has been saved successfully.");

                    }
                    catch (Exception ex)
                    {
                        status.Level = StatusLevel.Error;

                        string message = String.Format(
                          "Error while posting dataObject [{0}]. {1}",
                          objectType + " " + status.Identifier,
                          ex.ToString()
                        );

                        status.Messages.Add(message);
                    }

                    response.Append(status);
                }

               // writer.Close();

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in LoadDataObjects: " + ex);
                throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
            }
        }
 

        private string FormCreateXMLRow(string objectType, IDataObject dataObject)
        {
            try
            {
                string xmlRow = "";
               
                XElement commodityElement = GetConfig(objectType);

                IEnumerable<XElement> attributeElements = commodityElement.Element("attributes").Elements("attribute");

                foreach (var attributeElement in attributeElements)
                {
                    string name = attributeElement.Attribute("name").Value;
                    string value = Convert.ToString(dataObject.GetPropertyValue(name));
                    
                    xmlRow +=("&" + name + "=" + value);
                   
                }
               // Console.WriteLine(xmlRow.ToString());
                return xmlRow;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in FormXMLRow: " + ex);

                throw new Exception(
                  "Error while forming a XML row of type [" + objectType + "] from a DataObject.",
                  ex
                );
            }
        }
        private string FormGetXMLRow(string objectType, IDataObject dataObject)
        {
            try
            {
                string xmlRow = "";

                XElement commodityElement = GetConfig(objectType);

                IEnumerable<XElement> attributeElements = commodityElement.Element("attributes").Elements("attribute");

                foreach (var attributeElement in attributeElements)
                {
                    string name = attributeElement.Attribute("name").Value;
                    string value = Convert.ToString(dataObject.GetPropertyValue(name));

                    xmlRow += ("&" + name + "=~eq~" + value);

                }
                // Console.WriteLine(xmlRow.ToString());
                return xmlRow;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in FormXMLRow: " + ex);

                throw new Exception(
                  "Error while forming a XML row of type [" + objectType + "] from a DataObject.",
                  ex
                );
            }
        }

    }
}
