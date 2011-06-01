using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Ciloci.Flee;
using log4net;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using System.Diagnostics;

using SPPIDDataLayerVB;

namespace iRINGTools.SDK.SPPIDDataLayer
{
    public class SPPIDDataLayer : BaseDataLayer, IDataLayer2
    {
        private List<IDataObject> _dataObjects = null;
        //private ILMADataSource _projDatasource = null;        // SPPID DataSource
        private Llama.LMADataSource _projDatasource = null;        // SPPID DataSource
        private Llama.LMAFilter _lmFilters = null;
        private Llama.LMACriterion _lmCriterion = null;

        private const String CONST_SPID_Vessel = "C76EF274525A4345A6ACE1D179362899";

        //NOTE: This is required to deliver settings to constructor.
        //NOTE: Other objects could be requested on an as needed basis.
        [Inject]
        public SPPIDDataLayer(AdapterSettings settings, IKernel kernel)
        {
            _settings = settings;

            // Connect to SPPID project
            string siteNode = _settings["SPPIDSiteNode"];
            string projectStr = _settings["SPPIDProjectNumber"];
            projectStr += "!" + projectStr;     // per TR-88021 in SPPID 2007 SP4

            //_projDatasource = kernel.Get<ILMADataSource>();
            _projDatasource = new Llama.LMADataSource();
            
            _projDatasource.ProjectNumber = projectStr;
            _projDatasource.set_SiteNode(siteNode);

            //Example of Llama code extraction to VB project
            var datasource = LamaFactory.CreateDataSource(siteNode, projectStr);


        }

        public override DataDictionary GetDictionary()
        {
            DataDictionary dataDictionary = new DataDictionary();

            LoadConfiguration();

            List<DataObject> dataObjects = new List<DataObject>();
            foreach (XElement commodity in _configuration.Elements("commodities").Elements("commodity")) //commodity
            {

                string name = commodity.FirstAttribute.Value;
                // string name = commodity.Element("name").Value;

                DataObject dataObject = new DataObject
                {
                    objectName = name,
                    keyDelimeter = "_"
                };

                List<KeyProperty> keyProperties = new List<KeyProperty>();
                List<DataProperty> dataProperties = new List<DataProperty>();

                foreach (XElement attribute in commodity.Element("attributes").Elements("attribute"))
                {
                    // Name
                    string attributeName = attribute.Attribute("name").Value;

                    // is key
                    bool isKey = false;
                    if (attribute.Attribute("isKey") != null)
                    {
                        Boolean.TryParse(attribute.Attribute("isKey").Value, out isKey);
                    }

                    // Data type: String, Integer, Real, DateTime, Picklist, Boolean
                     string dataTypeName = attribute.Attribute("datatype").Value;
                    // string dataTypeName = attribute.Attribute("dataType").Value;

                    DataType dataType = DataType.String;
                    //Enum.TryParse<DataType>(attribute.Attribute("dataType").Value, out dataType);
                    switch (dataTypeName)
                    {
                        case "String":
                            dataType = DataType.String;
                            break;
                        case "Integer":
                            dataType = DataType.Int32;
                            break;
                        case "Real":
                            dataType = DataType.Double;
                            break;
                        case "DateTime":
                            dataType = DataType.DateTime;
                            break;
                        case "Picklist":
                            dataType = DataType.String;
                            break;
                        case "Boolean":
                            dataType = DataType.Boolean;
                            break;
                        default:
                            dataType = DataType.String;
                            break;
                    }

                    // Data length
                    int dataLength = 0;
                    if (attribute.Attribute("length") != null)
                    {
                        Int32.TryParse(attribute.Attribute("length").Value, out dataLength);
                    }

                    if (dataLength == 0 && dataTypeName == "Picklist")
                    {
                         Int32.TryParse(_settings["PicklistDataLength"], out dataLength);
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

                dataObject.keyProperties = keyProperties;
                dataObject.dataProperties = dataProperties;

                dataObjects.Add(dataObject);
            }

            dataDictionary.dataObjects = dataObjects;

            return dataDictionary;
        }

        public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
        {
            try
            {

                LoadDataDictionary(objectType);

                IList<IDataObject> allDataObjects = LoadDataObjects(objectType);

                var expressions = FormMultipleKeysPredicate(identifiers);

                if (expressions != null)
                {
                    _dataObjects = allDataObjects.AsQueryable().Where(expressions).ToList();
                }

                return _dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetList: " + ex);
                throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
            }
        }

        public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
        {
            try
            {
                LoadDataDictionary(objectType);

                IList<IDataObject> allDataObjects = LoadDataObjects(objectType);

                _lmFilters = new Llama.LMAFilter();
                //_lmCriterion = new Llama.LMACriterion();

                //_lmCriterion.SourceAttributeName = "TagSuffix";
                //_lmCriterion.set_ValueAttribute("P");
                //_lmCriterion.Operator = "=";
                _lmFilters.ItemType = "Drawing";
                string criteriaName = "Test";
                _lmFilters.get_Criteria().AddNew(criteriaName);
                _lmFilters.get_Criteria().get_Item(criteriaName).SourceAttributeName = "DrawingNumber";
                _lmFilters.get_Criteria().get_Item(criteriaName).set_ValueAttribute("d");
                _lmFilters.get_Criteria().get_Item(criteriaName).Operator = "!=";

                //_lmFilters.set_Criteria(_lmCriterion);  //TO DO Error when trying to add criteria. 

                // Error Retrieving the COM class factory for 
               //component with CLSID  failed due to the following error: 80040154 
               //Class not registered (Exception from HRESULT: 0x80040154 (REGDB_E_CLASSNOTREG)).

                Llama.LMDrawings drawings = new Llama.LMDrawings();

                drawings.Collect(_projDatasource, null, null, _lmFilters);

                Debug.WriteLine("Number of Piperuns retrieved = " + drawings.Count);

                Llama.LMAAttribute attr = new Llama.LMAAttribute();

               

                #region Commented code

                //// Apply filter
                //if (filter != null && filter.Expressions != null && filter.Expressions.Count > 0)
                //{
                //    var predicate = filter.ToPredicate(_dataObjectDefinition);

                //    if (predicate != null)
                //    {
                //        _dataObjects = allDataObjects.AsQueryable().Where(predicate).ToList();
                //    }
                //}

                //if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
                //{
                //    throw new NotImplementedException("OrderExpressions are not supported by the SPPID DataLayer.");
                //}

                ////Page and Sort The Data
                //if (pageSize > _dataObjects.Count())
                //    pageSize = _dataObjects.Count();
                //_dataObjects = _dataObjects.GetRange(startIndex, pageSize);

             //   return _dataObjects;
                #endregion       
       
                return new List<IDataObject>();
                
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetList: " + ex);

                throw new Exception(
                  "Error while getting a list of data objects of type [" + objectType + "].",
                  ex);
            }
        }

        public override long GetCount(string objectType, DataFilter filter)
        {
            try
            {
                //NOTE: pageSize of 0 indicates that all rows should be returned.
                IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

                return dataObjects.Count();
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetIdentifiers: " + ex);

                throw new Exception(
                  "Error while getting a count of type [" + objectType + "].",
                  ex);
            }
        }

        public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
        {
            try
            {
                List<string> identifiers = new List<string>();

                //NOTE: pageSize of 0 indicates that all rows should be returned.
                IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

                foreach (IDataObject dataObject in dataObjects)
                {
                    identifiers.Add((string)dataObject.GetPropertyValue("Tag"));
                }

                return identifiers;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetIdentifiers: " + ex);

                throw new Exception(
                  "Error while getting a list of identifiers of type [" + objectType + "].",
                  ex);
            }
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

                IList<IDataObject> existingDataObjects = LoadDataObjects(objectType);

                foreach (IDataObject dataObject in dataObjects)
                {
                    IDataObject existingDataObject = null;

                    string identifier = GetIdentifier(dataObject);
                    var predicate = FormKeyPredicate(identifier);

                    if (predicate != null)
                    {
                        existingDataObject = existingDataObjects.AsQueryable().Where(predicate).FirstOrDefault();
                    }

                    if (existingDataObject != null)
                    {
                        existingDataObjects.Remove(existingDataObject);
                    }

                    //TODO: Should this be per property?  Will it matter?
                    existingDataObjects.Add(dataObject);
                }

                response = SaveDataObjects(objectType, existingDataObjects);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in Post: " + ex);

                throw new Exception(
                  "Error while posting dataObjects of type [" + objectType + "].",
                  ex);
            }
        }

        public override Response Delete(string objectType, IList<string> identifiers)
        {
            // Not gonna do it. Wouldn't be prudent.
            Response response = new Response();
            Status status = new Status();
            status.Level = StatusLevel.Error;
            status.Messages.Add("Delete not supported by the SPPID DataLayer.");
            response.Append(status);
            return response;
        }

        public override Response Delete(string objectType, DataFilter filter)
        {
            // Not gonna do it. Wouldn't be prudent with a filter either.
            Response response = new Response();
            Status status = new Status();
            status.Level = StatusLevel.Error;
            status.Messages.Add("Delete not supported by the SPPID DataLayer.");
            response.Append(status);
            return response;
        }

        private void LoadConfiguration()
        {
            if (_configuration == null)
            {
                string uri = String.Format(
                    "{0}Configuration.{1}.xml",
                    _settings["XmlPath"],
                    _settings["ApplicationName"]);

                XDocument configDocument = XDocument.Load(uri);
                _configuration = configDocument.Element("configuration");
            }
        }

        private XElement GetCommodityConfig(string objectType)
        {
            if (_configuration == null)
            {
                LoadConfiguration();
            }

            XElement commodityConfig = _configuration.Elements("commodities").Elements("commodity").Where(o => o.FirstAttribute.Value == objectType).First();
           // XElement commodityConfig = _configuration.Elements("commodities").Where(o => o.Element("name").Value == objectType).First();

            return commodityConfig;
        }

        private IList<IDataObject> LoadDataObjects(string objectType)
        {
            try
            {
                IList<IDataObject> dataObjects = new List<IDataObject>();

                //NOTE: This will use the VB project to load the objects. Hopefully it will work.
                //var comConfig = GetCommodityConfig(objectType);
                //var lamaFactory = new LamaFactory();
                //dataObjects = lamaFactory.LoadDataObjects(objectType, comConfig, _projDatasource);

                //Get Path from Scope.config ({project}.{app}.config)
                //string path = String.Format(
                //    "{0}\\{1}.csv",
                //    _settings["SPPIDFolderPath"],
                //    objectType);
                string path = String.Format(
                  "{0}\\{1}.csv",
                   _settings["XMLPath"],
                  objectType);
               

                IDataObject dataObject = null;
                TextReader reader = new StreamReader(path);
                while (reader.Peek() >= 0)
                {
                    string csvRow = reader.ReadLine();

                    dataObject = FormDataObject(objectType, csvRow);

                    if (dataObject != null)
                        dataObjects.Add(dataObject);
                }
                reader.Close();

                return dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in LoadDataObjects: " + ex);
                throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
            }
        }

        private IDataObject FormDataObject(string objectType, string csvRow)
        {
            try
            {
                IDataObject dataObject = new GenericDataObject
                {
                    ObjectType = objectType,
                };

                XElement commodityElement = GetCommodityConfig(objectType);

                if (!String.IsNullOrEmpty(csvRow))
                {
                    IEnumerable<XElement> attributeElements = commodityElement.Element("attributes").Elements("attribute");

                    string[] csvValues = csvRow.Split(',');
                    int index = 0;
                    foreach (var attributeElement in attributeElements)
                    {
                        
                            string name = attributeElement.Attribute("name").Value;
                            string dataType = attributeElement.Attribute("datatype").Value.ToLower();
                            //string dataType = attributeElement.Attribute("dataType").Value.ToLower();
                            
                            string value = csvValues[index++].Trim();

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
                  "Error while forming a dataObject of type [" + objectType + "] from SPPID.",
                  ex);
            }
        }

        private Response SaveDataObjects(string objectType, IList<IDataObject> dataObjects)
        {
            try
            {
                Response response = new Response();

                // Create data object directory in case it does not exist
                Directory.CreateDirectory(_settings["XMLPath"]);
                //Directory.CreateDirectory(_settings["SPPIDFolderPath"]);

                string path = String.Format(
                  "{0}\\{1}.csv",
                  _settings["XMLPath"],
                  objectType);

                //TODO: Need to update file, not replace it!
                TextWriter writer = new StreamWriter(path);

                foreach (IDataObject dataObject in dataObjects)
                {
                    Status status = new Status();

                    try
                    {
                        string identifier = GetIdentifier(dataObject);
                        status.Identifier = identifier;

                        List<string> csvRow = FormCSVRow(objectType, dataObject);

                        writer.WriteLine(String.Join(", ", csvRow.ToArray()));
                        status.Messages.Add("Record [" + identifier + "] has been saved successfully.");
                    }
                    catch (Exception ex)
                    {
                        status.Level = StatusLevel.Error;

                        string message = String.Format(
                          "Error while posting dataObject [{0}]. {1}",
                          dataObject.GetPropertyValue("Tag"),
                          ex.ToString());

                        status.Messages.Add(message);
                    }

                    response.Append(status);
                }

                writer.Close();

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in LoadDataObjects: " + ex);
                throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
            }
        }

        private List<string> FormCSVRow(string objectType, IDataObject dataObject)
        {
            try
            {
                List<string> csvRow = new List<string>();

                XElement commodityElement = GetCommodityConfig(objectType);

                IEnumerable<XElement> attributeElements = commodityElement.Element("attributes").Elements("attribute");
                string value = string.Empty;
                foreach (var attributeElement in attributeElements)
                {
                    string name = attributeElement.Attribute("name").Value;
                    value = Convert.ToString(dataObject.GetPropertyValue(name));
                    csvRow.Add(value);
                }

                return csvRow;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in FormSPPIDRow: " + ex);

                throw new Exception(
                  "Error while forming a CSV row of type [" + objectType + "] from a DataObject.",
                  ex);
            }
        }
    }
}
