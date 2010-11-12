using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using log4net;
using Ninject;
using org.iringtools.utility;
using System.Xml.Linq;
using System.IO;
using Ciloci.Flee;
using org.iringtools.adapter;

namespace Bechtel.CSVDataLayer.API
{
  //NOTE: This CSVDataLayer assumes that property "Tag" is identifier of data objects
  public class CSVDataLayer : IDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(CSVDataLayer));
    private AdapterSettings _settings = null;
    private string _dataDictionaryPath = String.Empty;

    [Inject]
    public CSVDataLayer(AdapterSettings settings)
    {
      _dataDictionaryPath = _settings["XmlPath"] + "DataDictionary." + _settings["ProjectName"] + "." + _settings["ApplicationName"] + ".xml";
      _settings = settings;
    }

    public IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();
        Type type = Type.GetType("Bechtel.CSVDataLayer.API." + objectType + "DataObject");

        objectType = objectType.Substring(objectType.LastIndexOf('.') + 1);

        if (identifiers != null && identifiers.Count > 0)
        {
          foreach (string identifier in identifiers)
          {
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
        _logger.Error("Error in CreateList: " + ex);
        throw new Exception("Error while creating a list of data objects of type [" + objectType + "].", ex);
      }
    }

    public IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {
        List<string> identifiers = new List<string>();
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
        throw new Exception("Error while getting a list of identifiers of type [" + objectType + "].", ex);
      }
    }

    public IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int pageNumber)
    {
      try
      {
        List<IDataObject> dataObjects = new List<IDataObject>();
        string dataObjectType = objectType + "DataObject";
        Type type = Type.GetType("Bechtel.CSVDataLayer.API." + objectType + "DataObject");

        // Load config xml 
        string configFile = _settings["XmlPath"] + objectType + "." + _settings["ProjectName"] + "." + _settings["ApplicationName"] + ".xml";
        XDocument configDoc = XDocument.Load(configFile);
        XElement configRootElement = configDoc.Element("commodity");

        // Get path
        string dataObjectPath = GetDataObjectPath(objectType);

        // Read all files (commodity rows) from commodity path of the application
        DirectoryInfo directory = new DirectoryInfo(dataObjectPath);
        FileInfo[] files = directory.GetFiles();

        foreach (FileInfo file in files)
        {
          TextReader reader = new StreamReader(dataObjectPath + file.Name);
          string csvRow = reader.ReadLine();
          reader.Close();

          if (!String.IsNullOrEmpty(csvRow))
          {
            // Create an instance of IDataObject
            IDataObject dataObject = (IDataObject)Activator.CreateInstance(type);

            IEnumerable<XElement> attrs = configRootElement.Element("attributesSequence").Elements("attribute");
            string[] csvValues = csvRow.Split(',');
            int index = 0;

            foreach (var attr in attrs)
            {
              string attrName = attr.Attribute("name").Value;
              string attrDataType = attr.Attribute("dataType").Value.ToLower();
              string csvValue = csvValues[index++].Trim();

              // if data type is not nullable, make sure it has a value
              if (!(attrDataType.EndsWith("?") && csvValue == String.Empty))
              {
                if (attrDataType.Contains("bool"))
                {
                  if (csvValue == "true" || csvValue == "yes")
                  {
                    csvValue = "1";
                  }
                  else
                  {
                    csvValue = "0";
                  }
                }
                else if (csvValue == String.Empty && (
                         attrDataType.StartsWith("int") ||
                         attrDataType == "double" ||
                         attrDataType == "single" ||
                         attrDataType == "float" ||
                         attrDataType == "decimal"))
                {
                  csvValue = "0";
                }
              }

              dataObject.SetPropertyValue(attrName, csvValue);
            }

            dataObjects.Add(dataObject);
          }
        }

        // Apply filter
        if (filter != null && filter.Expressions.Count > 0)
        {
          string variable = "dataObject";
          string linqExpression = string.Empty;
          switch (objectType)
            {
            case "Equipment":
                linqExpression = filter.ToLinqExpression<Equipment>(variable);
                break;
              default:
                linqExpression = string.Empty;
                break;
            }


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

        // Apply paging
        if (pageSize > 0 && pageNumber > 0)
        {
          if (dataObjects.Count > (pageSize * (pageNumber - 1) + pageSize))
          {
            dataObjects = dataObjects.GetRange(pageSize * (pageNumber - 1), pageSize);
          }
          else if (pageSize * (pageNumber - 1) > dataObjects.Count)
          {
            dataObjects = dataObjects.GetRange(pageSize * (pageNumber - 1), dataObjects.Count);
          }
          else
          {
            return null;
          }
        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
      }
    }
    
    public IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      try
      {
        List<IDataObject> dataObjects = new List<IDataObject>();
        objectType = objectType.Substring(objectType.LastIndexOf('.') + 1);
        string dataObjectType = objectType + "DataObject";
        Type type = Type.GetType("Bechtel.CSVDataLayer.API." + objectType + "DataObject");

        // Load config xml 
        string configFile = _settings["XmlPath"] + objectType + "." + _settings["ProjectName"] + "." + _settings["ApplicationName"] + ".xml";
        XDocument configDoc = XDocument.Load(configFile);
        XElement configRootElement = configDoc.Element("commodity");

        // Get path
        string dataObjectPath = GetDataObjectPath(objectType);

        foreach (string identifier in identifiers)
        {
          TextReader reader = new StreamReader(dataObjectPath + identifier + ".csv");
          string csvRow = reader.ReadLine();
          reader.Close();

          if (!String.IsNullOrEmpty(csvRow))
          {
            // Create an instance of IDataObject
            IDataObject dataObject = (IDataObject)Activator.CreateInstance(type);

            IEnumerable<XElement> attrs = configRootElement.Element("attributesSequence").Elements("attribute");
            string[] csvValues = csvRow.Split(',');
            int index = 0;

            foreach (var attr in attrs)
            {
              string attrName = attr.Attribute("name").Value;
              string attrDataType = attr.Attribute("dataType").Value.ToLower();
              string csvValue = csvValues[index++].Trim();

              // if data type is not nullable, make sure it has a value
              if (!(attrDataType.EndsWith("?") && csvValue == String.Empty))
              {
                if (attrDataType.Contains("bool"))
                {
                  if (csvValue == "true" || csvValue == "yes")
                  {
                    csvValue = "1";
                  }
                  else
                  {
                    csvValue = "0";
                  }
                }
                else if (csvValue == String.Empty && (
                         attrDataType.Contains("int") ||
                         attrDataType == "double" ||
                         attrDataType == "single" ||
                         attrDataType == "float" ||
                         attrDataType == "decimal"))
                {
                  csvValue = "0";
                }
              }

              dataObject.SetPropertyValue(attrName, csvValue);
            }

            dataObjects.Add(dataObject);
          }
        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
      }
    }

    public Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();

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
        // Load config xml
        string dataObjectType = dataObjects.FirstOrDefault().GetType().FullName;
        
        string objectType = dataObjectType.Substring(0, dataObjectType.Length - "DataObject".Length);
        objectType = objectType.Substring(objectType.LastIndexOf('.') + 1);
        
        string configFile = _settings["XmlPath"] + objectType + "." + _settings["ProjectName"] + "." + _settings["ApplicationName"] + ".xml";
        XDocument configDoc = XDocument.Load(configFile);
        XElement configRootElement = configDoc.Element("commodity");

        // Get path
        string dataObjectPath = GetDataObjectPath(objectType);

        // Create data object directory in case it does not exist
        Directory.CreateDirectory(dataObjectPath);

        foreach (IDataObject dataObject in dataObjects)
        {
          Status status = new Status();

          try
          {
            string identifier = (string)dataObject.GetPropertyValue("Tag");
            status.Identifier = identifier;

            TextWriter writer = new StreamWriter(dataObjectPath + identifier + ".csv");
            IEnumerable<XElement> attrs = configRootElement.Element("attributesSequence").Elements("attribute");
            List<string> csvValues = new List<string>();

            foreach (var attr in attrs)
            {
              string attrName = attr.Attribute("name").Value;
              csvValues.Add(Convert.ToString(dataObject.GetPropertyValue(attrName)));
            }

            writer.WriteLine(String.Join(", ", csvValues.ToArray()));
            writer.Close();
            status.Messages.Add("Record [" + identifier + "] has been saved successfully.");
          }
          catch (Exception ex)
          {
            status.Level = StatusLevel.Error;
            status.Messages.Add("Error while post data object [" + dataObject.GetPropertyValue("Tag") + ex);
          }
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in PostList: " + ex);

        object sample = dataObjects.FirstOrDefault();
        string objectType = (sample != null) ? sample.GetType().Name : String.Empty;
        throw new Exception("Error while posting data objects of type [" + objectType + "].", ex);
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

      string dataObjectPath = GetDataObjectPath(objectType);
      foreach (string identifier in identifiers)
      {
        Status status = new Status();
        status.Identifier = identifier;

        try
        {
            
          File.Delete(dataObjectPath + identifier + ".csv");
          status.Messages.Add("Data object [" + identifier + "] deleted successfully.");
            
        }
        catch (Exception ex)
        {
          _logger.Error("Error in Delete: " + ex);
          status.Level = StatusLevel.Error;
          status.Messages.Add("Error while deleting data object [" + identifier + "]." + ex);
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
        throw new Exception("Error while deleting data objects of type [" + objectType + "].", ex);
      }
    }

    public DataDictionary GetDictionary()
    {

      // Create a DataDictionary instance
      DataDictionary dataDictionary = new DataDictionary()
      {
        DataObjects = new List<DataObject>()
        {
          new DataObject()
          {
            KeyDelimeter = "",
            KeyProperties = new List<KeyProperty>()
            {
              new KeyProperty()
              {
                KeyPropertyName = "Tag"
              },
            },
            DataProperties = new List<DataProperty>()
            {
              new DataProperty()
              {
                DataLength = 255,
                DataType = DataType.String,
                PropertyName = "PumpType",
                IsNullable = true,
              },
              new DataProperty()
              {
                DataLength = 255,
                DataType = DataType.String,
                PropertyName = "PumpDriverType",
                IsNullable = true,
              },
              new DataProperty()
              {
                DataLength = 16,
                DataType = DataType.Double,
                PropertyName = "DesignTemp",
                IsNullable = true,
              },
              new DataProperty()
              {
                DataLength = 16,
                DataType = DataType.Double,
                PropertyName = "DesignPressure",
                IsNullable = true,
              },
              new DataProperty()
              {
                DataLength = 16,
                DataType = DataType.Double,
                PropertyName = "Capacity",
                IsNullable = true,
              },
              new DataProperty()
              {
                DataLength = 16,
                DataType = DataType.Double,
                PropertyName = "SpecificGravity",
                IsNullable = true,
              },
              new DataProperty()
              {
                DataLength = 16,
                DataType = DataType.Double,
                PropertyName = "DifferentialPressure",
                IsNullable = true,
              },
            },
          ObjectName = "Equipment",
          }
        }
      };

      return dataDictionary;
    }

    private string GetdataObjectType(string objectType)
    {
      string dataLayerNamespace = "org.iringtools.adapter.datalayer";
      return dataLayerNamespace + ".proj_" + _settings["ProjectName"] + "." + _settings["ApplicationName"] + "." + objectType;
    }

    private string GetDataObjectPath(string objectType)
    {
      // Load config xml 
      string configFile = _settings["XmlPath"] + objectType + "." + _settings["ProjectName"] + "." + _settings["ApplicationName"] + ".xml";
      XDocument configDoc = XDocument.Load(configFile);
      XElement configRootElement = configDoc.Element("commodity");

      // Get path
      string dataObjectPath = configRootElement.Element("location").Value;
      if (!dataObjectPath.EndsWith("\\"))
      {
        dataObjectPath += "\\";
      }

      return dataObjectPath;
    }

    #region IDataLayer Members

    public IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
