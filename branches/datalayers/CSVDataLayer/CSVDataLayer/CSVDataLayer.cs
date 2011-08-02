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

namespace iRINGTools.SDK.CSVDataLayer
{
  public class CSVDataLayer : BaseDataLayer, IDataLayer2
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(CSVDataLayer));
    private List<IDataObject> _dataObjects = null;

    //NOTE: This is required to deliver settings to constructor.
    //NOTE: Other objects could be requested on an as needed basis.
    [Inject]
    public CSVDataLayer(AdapterSettings settings) : base(settings) {}

    public override DataDictionary GetDictionary()
    {
      DataDictionary dataDictionary = new DataDictionary();

      LoadConfiguration();

      List<DataObject> dataObjects = new List<DataObject>();
      foreach (XElement commodity in _configuration.Elements("commodity"))
      {
        string name = commodity.Element("name").Value;

        DataObject dataObject = new DataObject
        {
          objectName = name,
          keyDelimeter = "_",
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

        //Page and Sort The Data
        if (pageSize > _dataObjects.Count())
          pageSize = _dataObjects.Count();
        _dataObjects = _dataObjects.GetRange(startIndex, pageSize);

        return _dataObjects;
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
          ex
        );
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
        _settings["CSVFolderPath"],
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

    public override Response Delete(string objectType, DataFilter filter)
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

    private XElement GetCommodityConfig(string objectType)
    {
      if (_configuration == null)
      {
        LoadConfiguration();
      }

      XElement commodityConfig = _configuration.Elements("commodity").Where(o => o.Element("name").Value == objectType).First();

      return commodityConfig;
    }

    private IList<IDataObject> LoadDataObjects(string objectType)
    {
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();

        //Get Path from Scope.config ({project}.{app}.config)
        string path = String.Format(
            "{0}\\{1}.csv",
            _settings["CSVFolderPath"],
            objectType
        );

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
          "Error while forming a dataObject of type [" + objectType + "] from a CSV row.",
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
        Directory.CreateDirectory(_settings["CSVFolderPath"]);

        string path = String.Format(
          "{0}\\{1}.csv",
          _settings["CSVFolderPath"],
          objectType
        );

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
              ex.ToString()
            );

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

        foreach (var attributeElement in attributeElements)
        {
          string name = attributeElement.Attribute("name").Value;
          string value = Convert.ToString(dataObject.GetPropertyValue(name));
          csvRow.Add(value);
        }

        return csvRow;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in FormCSVRow: " + ex);

        throw new Exception(
          "Error while forming a CSV row of type [" + objectType + "] from a DataObject.",
          ex
        );
      }
    }
  }
}
