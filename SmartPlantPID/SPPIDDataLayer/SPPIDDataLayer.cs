using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Ciloci.Flee;
using log4net;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;


namespace iRINGTools.SDK.SPPIDDataLayer
{
    //NOTE: This DataLayer assumes that property "Tag" is identifier of data objects
    public class SPPIDDataLayer : IDataLayer
    {
        private AdapterSettings _settings = null;       // settings from web.config, [project].[app].config, thread state
        private string _dataDictionaryPath = String.Empty;
        private Llama.LMADataSource _projDatasource = null;        // SPPID DataSource

        //NOTE: This will enable logging.
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SPPIDDataLayer));

        //NOTE: This is required to deliver settings to constructor.
        //NOTE: Other objects could be requested on an as needed basis.
        [Inject]
        public SPPIDDataLayer(AdapterSettings settings)
        {
            _settings = settings;

            _dataDictionaryPath = String.Format(
              "{0}DataDictionary.{1}.{2}.xml",
              _settings["XmlPath"],
              _settings["ProjectName"],
              _settings["ApplicationName"]
            );

            // Connect to SPPID project
            string siteNode = _settings["SPPIDSiteNode"];
            string projectStr = _settings["SPPIDProjectNumber"];
            projectStr += "!" + projectStr;     // per TR-88021 in SPPID 2007 SP4
            
            _projDatasource = new Llama.LMADataSource();
            _projDatasource.ProjectNumber = projectStr;
            _projDatasource.set_SiteNode(siteNode);
        }

        public IList<IDataObject> Create(string objectType, IList<string> identifiers)
        {
            try
            {
                IList<IDataObject> dataObjects = new List<IDataObject>();

                //This may not be neccessary here, but is useful with generic APIs
                string typeName = String.Format(
                  "iRINGTools.SDK.SPPIDDataLayer.{0}DataObject",
                  objectType
                );

                Type type = Type.GetType(typeName);

                if (identifiers != null)
                {
                    foreach (string identifier in identifiers)
                    {
                        //Again, reflection is not neccessary with hard coded classes.
                        IDataObject dataObject = (IDataObject)Activator.CreateInstance(type);

                        if (!String.IsNullOrEmpty(identifier))
                        {
                            dataObject.SetPropertyValue("Tag", identifier);
                        }

                        dataObjects.Add(dataObject);
                    }
                }

                return dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in Create: " + ex);

                throw new Exception(
                  "Error while creating a list of data objects of type [" + objectType + "].",
                  ex
                );
            }
        }

        public long GetCount(string objectType, DataFilter filter)
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
                  ex
                );
            }
        }

        public IList<string> GetIdentifiers(string objectType, DataFilter filter)
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
                  ex
                );
            }
        }

        public IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
        {
            try
            {
                List<IDataObject> dataObjects = new List<IDataObject>();

                //This may not be neccessary here, but is useful with generic APIs
                string typeName = String.Format(
                  "iRINGTools.SDK.SPPIDDataLayer.{0}DataObject",
                  objectType
                );

                Type type = Type.GetType(typeName);

                // Load Internal Config xml 
                string uri = String.Format(
                  "{0}{1}.{2}.{3}.xml",
                  _settings["XmlPath"],
                  objectType,
                  _settings["ProjectName"],
                  _settings["ApplicationName"]
                );

                XDocument configDocument = XDocument.Load(uri);
                XElement commodityElement = configDocument.Element("commodity");

                //Get Path from Scope.config ({project}.{app}.config)
                string dataObjectPath = String.Format(
                  "{0}\\{1}",
                  _settings["SPPIDFolderPath"],
                  objectType
                );

                // Read all files (commodity rows) from commodity path of the application
                DirectoryInfo directory = new DirectoryInfo(dataObjectPath);
                FileInfo[] files = directory.GetFiles();

                foreach (FileInfo file in files)
                {
                    TextReader reader = new StreamReader(file.FullName);
                    string csvRow = reader.ReadLine();
                    reader.Close();

                    IDataObject dataObject = FormDataObject(commodityElement, type, csvRow);

                    if (dataObject != null)
                        dataObjects.Add(dataObject);
                }

                // Apply filter
                if (filter != null && filter.Expressions != null && filter.Expressions.Count > 0)
                {
                    string variable = "dataObject";

                    string linqExpression = filter.ToLinqExpression(type, variable);

                    if (linqExpression != String.Empty)
                    {
                        ExpressionContext context = new ExpressionContext();
                        context.Variables.DefineVariable(variable, type);

                        for (int i = 0; i < dataObjects.Count; i++)
                        {
                            context.Variables[variable] = dataObjects[i];
                            var expression = context.CompileGeneric<bool>(linqExpression);
                            if (!expression.Evaluate())
                            {
                                dataObjects.RemoveAt(i--);
                            }
                        }
                    }
                }

                if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
                {
                    throw new NotImplementedException("OrderExpressions are not supported by the SPPID DataLayer.");
                }

                //Page and Sort The Data
                dataObjects = dataObjects.GetRange(startIndex, pageSize);

                return dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetList: " + ex);

                throw new Exception(
                  "Error while getting a list of data objects of type [" + objectType + "].",
                  ex
                );
            }
        }

        public IList<IDataObject> Get(string objectType, IList<string> identifiers)
        {
            try
            {
                List<IDataObject> dataObjects = new List<IDataObject>();

                //This may not be neccessary here, but is useful with generic APIs
                string typeName = String.Format(
                  "iRINGTools.SDK.SPPIDDataLayer.{0}DataObject",
                  objectType
                );

                Type type = Type.GetType(typeName);

                // Load Internal Config xml 
                string uri = String.Format(
                  "{0}{1}.{2}.{3}.xml",
                  _settings["XmlPath"],
                  objectType,
                  _settings["ProjectName"],
                  _settings["ApplicationName"]
                );

                XDocument configDocument = XDocument.Load(uri);
                XElement commodityElement = configDocument.Element("commodity");

                //Get Path from Scope.config ({project}.{app}.config)
                string dataObjectPath = String.Format(
                  "{0}\\{1}",
                  _settings["SPPIDFolderPath"],
                  objectType
                );

                foreach (string identifier in identifiers)
                {
                    string path = String.Format(
                      "{0}\\{1}.csv",
                      dataObjectPath,
                      identifier
                    );

                    TextReader reader = new StreamReader(path);
                    string csvRow = reader.ReadLine();
                    reader.Close();

                    IDataObject dataObject = FormDataObject(commodityElement, type, csvRow);

                    if (dataObject != null)
                        dataObjects.Add(dataObject);
                }

                return dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetList: " + ex);
                throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
            }
        }

        public IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
        {
            throw new NotImplementedException();
        }

        public Response Post(IList<IDataObject> dataObjects)
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
                // Resolve the objectType
                string typeName = dataObjects.FirstOrDefault().GetType().FullName;
                string objectTypeName = typeName.Substring(0, typeName.Length - "DataObject".Length);
                objectType = objectTypeName.Substring(objectTypeName.LastIndexOf('.') + 1);

                // Load Internal Config xml 
                string uri = String.Format(
                  "{0}{1}.{2}.{3}.xml",
                  _settings["XmlPath"],
                  objectType,
                  _settings["ProjectName"],
                  _settings["ApplicationName"]
                );

                XDocument configDocument = XDocument.Load(uri);
                XElement commodityElement = configDocument.Element("commodity");

                //Get Path from Scope.config ({project}.{app}.config)
                string dataObjectPath = String.Format(
                  "{0}\\{1}",
                  _settings["SPPIDFolderPath"],
                  objectType
                );

                // Create data object directory in case it does not exist
                Directory.CreateDirectory(dataObjectPath);

                foreach (IDataObject dataObject in dataObjects)
                {
                    Status status = new Status();

                    try
                    {
                        string identifier = (string)dataObject.GetPropertyValue("Tag");
                        status.Identifier = identifier;

                        string path = String.Format(
                          "{0}\\{1}.csv",
                          dataObjectPath,
                          identifier
                        );

                        TextWriter writer = new StreamWriter(path);

                        IEnumerable<XElement> attributeElements = commodityElement.Element("attributes").Elements("attribute");

                        List<string> csvValues = new List<string>();

                        foreach (var attributeElement in attributeElements)
                        {
                            string name = attributeElement.Attribute("name").Value;
                            string value = Convert.ToString(dataObject.GetPropertyValue(name));
                            csvValues.Add(value);
                        }

                        writer.WriteLine(String.Join(", ", csvValues.ToArray()));
                        writer.Close();
                        status.Messages.Add("Record [" + identifier + "] has been saved successfully.");
                    }
                    catch (Exception ex)
                    {
                        status.Level = StatusLevel.Error;

                        string message = String.Format(
                          "Error while posting dataObject [{0}]. {1}",
                          dataObject.GetPropertyValue("Tag"),
                          ex.ToString()
                        );

                        status.Messages.Add(message);
                    }

                    response.Append(status);
                }

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

        public Response Delete(string objectType, IList<string> identifiers)
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
              _settings["SPPIDFolderPath"],
              objectType
            );

            foreach (string identifier in identifiers)
            {
                Status status = new Status();
                status.Identifier = identifier;

                try
                {
                    string path = String.Format(
                      "{0}\\{1}.csv",
                      dataObjectPath,
                      identifier
                    );

                    File.Delete(path);

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

        public Response Delete(string objectType, DataFilter filter)
        {
            try
            {
                IList<string> identifiers = new List<string>();

                //NOTE: pageSize of 0 indicates that all rows should be returned.
                IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

                foreach (IDataObject dataObject in dataObjects)
                {
                    identifiers.Add((string)dataObject.GetPropertyValue("Tag"));
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

        public DataDictionary GetDictionary()
        {

            // Create a DataDictionary instance
            DataDictionary dataDictionary = new DataDictionary()
            {
                dataObjects = new List<DataObject>()
        {
          new DataObject()
          {
            keyDelimeter = "",
            keyProperties = new List<KeyProperty>()
            {
              new KeyProperty()
              {
                keyPropertyName = "Tag"
              },
            },
            dataProperties = new List<DataProperty>()
            {
              new DataProperty()
              {
                dataLength = 255,
                dataType = DataType.String,
                propertyName = "PumpType",
                isNullable = true,
                showOnIndex = true,
              },
              new DataProperty()
              {
                dataLength = 255,
                dataType = DataType.String,
                propertyName = "PumpDriverType",
                isNullable = true,
              },
              new DataProperty()
              {
                dataLength = 16,
                dataType = DataType.Double,
                propertyName = "DesignTemp",
                isNullable = true,
              },
              new DataProperty()
              {
                dataLength = 16,
                dataType = DataType.Double,
                propertyName = "DesignPressure",
                isNullable = true,
              },
              new DataProperty()
              {
                dataLength = 16,
                dataType = DataType.Double,
                propertyName = "Capacity",
                isNullable = true,
              },
              new DataProperty()
              {
                dataLength = 16,
                dataType = DataType.Double,
                propertyName = "SpecificGravity",
                isNullable = true,
              },
              new DataProperty()
              {
                dataLength = 16,
                dataType = DataType.Double,
                propertyName = "DifferentialPressure",
                isNullable = true,
              },
            },
          objectName = "Equipment",
          }
        }
            };

            return dataDictionary;
        }

        private IDataObject FormDataObject(XElement commodityElement, Type type, string csvRow)
        {
            try
            {
                IDataObject dataObject = null;

                if (!String.IsNullOrEmpty(csvRow))
                {
                    //Again, reflection is not neccessary with hard coded classes.
                    dataObject = (IDataObject)Activator.CreateInstance(type);

                    IEnumerable<XElement> attributeElements = commodityElement.Element("attributes").Elements("attribute");

                    string[] csvValues = csvRow.Split(',');

                    int index = 0;
                    foreach (var attributeElement in attributeElements)
                    {
                        string name = attributeElement.Attribute("name").Value;
                        string dataType = attributeElement.Attribute("dataType").Value.ToLower();
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
                  "Error while forming a dataObject of type [" + type.Name + "] from a SPPID row.",
                  ex
                );
            }
        }

        private string GetDataObjectType(string objectType)
        {
            string dataLayerNamespace = "org.iringtools.adapter.datalayer";
            return dataLayerNamespace + ".proj_" + _settings["ProjectName"] + "." + _settings["ApplicationName"] + "." + objectType;
        }
    }
}
