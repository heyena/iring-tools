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
using LinqKit;

namespace org.iringtools.adapter.datalayer
{
  //NOTE: This CSVDataLayer assumes that property "Tag" is identifier of data objects
  public class CSVDataLayer : IDataLayer2
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(CSVDataLayer));
    private AdapterSettings _settings = null;
    private ApplicationSettings _appSettings = null;
    private string _dataDictionaryPath = String.Empty;

    [Inject]
    public CSVDataLayer(AdapterSettings settings, ApplicationSettings appSettings)
    {
      _dataDictionaryPath = settings.XmlPath + "DataDictionary." + appSettings.ProjectName + "." + appSettings.ApplicationName + ".xml";
      _settings = settings;
      _appSettings = appSettings;
    }

    public IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();
        Type type = Type.GetType(GetQualifiedObjectType(objectType) + "DataObject");

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
        string qualifiedObjectType = GetQualifiedObjectType(objectType) + "DataObject";
        Type type = Type.GetType(qualifiedObjectType);

        // Load config xml 
        string configFile = _settings.XmlPath + objectType + "." + _appSettings.ProjectName + "." + _appSettings.ApplicationName + ".xml";
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

              // if data type is not nullable, set default value for it
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
          var predicate = GenerateWherePredicate(qualifiedObjectType, filter);
          dataObjects = dataObjects.Where(predicate).ToList();          
        }

        // Apply paging
        if (pageSize > 0 && pageNumber > 0 && dataObjects.Count > (pageSize * (pageNumber - 1) + pageSize))
        {
          dataObjects = dataObjects.GetRange(pageSize * (pageNumber - 1), pageSize);
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
        //IList<IDataObject> allDataObjects = Get(objectType, null, 0, 0);

        //var dataObjects =
        //  (from dataObject in allDataObjects 
        //   where identifiers.Contains((string)dataObject.GetPropertyValue("Tag"))
        //   select dataObject).ToList<IDataObject>();

        //return dataObjects;

        List<IDataObject> dataObjects = new List<IDataObject>();
        string qualifiedObjectType = GetQualifiedObjectType(objectType) + "DataObject";
        Type type = Type.GetType(qualifiedObjectType);

        // Load config xml 
        string configFile = _settings.XmlPath + objectType + "." + _appSettings.ProjectName + "." + _appSettings.ApplicationName + ".xml";
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

                dataObject.SetPropertyValue(attrName, csvValue);
              }
            }
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
        response.Add("Nothing to update.");
        return response;
      }

      try
      {
        // Load config xml
        string dataObjectType = dataObjects.FirstOrDefault().GetType().Name;
        string objectType = dataObjectType.Substring(0, dataObjectType.Length - "DataObject".Length);
        string configFile = _settings.XmlPath + objectType + "." + _appSettings.ProjectName + "." + _appSettings.ApplicationName + ".xml";
        XDocument configDoc = XDocument.Load(configFile);
        XElement configRootElement = configDoc.Element("commodity");

        // Get path
        string dataObjectPath = GetDataObjectPath(objectType);

        // Create data object directory in case it does not exist
        Directory.CreateDirectory(dataObjectPath);

        foreach (IDataObject dataObject in dataObjects)
        {
          try
          {
            string identifier = (string)dataObject.GetPropertyValue("Tag");
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
            response.Add("Record [" + identifier + "] has been saved successfully.");
          }
          catch (Exception ex)
          {
            response.Add("Error while post data object [" + dataObject.GetPropertyValue("Tag") + ex);
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
        response.Add("Nothing to delete.");
        return response;
      }

      try
      {
        string dataObjectPath = GetDataObjectPath(objectType);
        foreach (string identifier in identifiers)
        {
          try
          {
            File.Delete(dataObjectPath + identifier + ".csv");
            response.Add("Data object [" + identifier + "] deleted successfully.");
          }
          catch (Exception ex)
          {
            _logger.Error("Error in Delete: " + ex);
            response.Add("Error while deleting data object [" + identifier + "]." + ex);
          }
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception("Error while deleting data objects of type [" + objectType + "].", ex);
      }
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
      return Utility.Read<DataDictionary>(_dataDictionaryPath);
    }

    private string GetQualifiedObjectType(string objectType)
    {
      string dataLayerNamespace = "org.iringtools.adapter.datalayer";
      return dataLayerNamespace + ".proj_" + _appSettings.ProjectName + "." + _appSettings.ApplicationName + "." + objectType;
    }

    private string GetDataObjectPath(string objectType)
    {
      // Load config xml 
      string configFile = _settings.XmlPath + objectType + "." + _appSettings.ProjectName + "." + _appSettings.ApplicationName + ".xml";
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

    //NOTE: this method currently does not support the following:
    //        - grouping
    //        - logical operator: OrNot, AndNot 
    //        - property data types that are not string and numeric
    private Func<IDataObject, bool> GenerateWherePredicate(string objectType, DataFilter filter)
    {
      var predicate = PredicateBuilder.True<IDataObject>();

      try
      {
        foreach (Expression expression in filter.Expressions)
        {
          string propertyType = Type.GetType(objectType).GetProperty(expression.PropertyName).PropertyType.Name.ToLower();

          if (expression.LogicalOperator == LogicalOperator.Or)
          {
            switch (expression.RelationalOperator)
            {
              case RelationalOperator.StartsWith:
                if (propertyType == "string")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).StartsWith(expression.Values.FirstOrDefault())));
                }
                else
                {
                  string error = "Error in GenerateWhereClause: BeginsWith operator used with non-string property";
                  _logger.Error(error);
                  throw new Exception(error);
                }
                break;

              case RelationalOperator.EndsWith:
                if (propertyType == "string")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => ((Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).EndsWith(expression.Values.FirstOrDefault()))));
                }
                else
                {
                  string error = "Error in GenerateWhereClause: EndsWith operator used with non-string property";
                  _logger.Error(error);
                  throw new Exception(error);
                }
                break;

              case RelationalOperator.Contains:
                if (propertyType == "string")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).Contains(expression.Values.FirstOrDefault())));
                }
                else
                {
                  string error = "Error in GenerateWhereClause: Contains operator used with non-string property";
                  _logger.Error(error);
                  throw new Exception(error);
                }
                break;

              case RelationalOperator.In:
                predicate = predicate.Or<IDataObject>(dataObject => expression.Values.Contains((Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)))));
                break;

              case RelationalOperator.EqualTo:
                if (propertyType == "string")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) == 0));
                }
                else if (propertyType.StartsWith("int") || propertyType == "single" || propertyType == "double" || propertyType == "decimal")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => Convert.ToDecimal(dataObject.GetPropertyValue(expression.PropertyName)) == Convert.ToDecimal(expression.Values.FirstOrDefault()));
                }
                break;

              case RelationalOperator.NotEqualTo:
                if (propertyType == "string")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) != 0));
                }
                break;

              case RelationalOperator.GreaterThan:
                if (propertyType == "string")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) > 0));
                }
                else if (propertyType.StartsWith("int") || propertyType == "single" || propertyType == "double" || propertyType == "decimal")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => (Convert.ToDecimal(dataObject.GetPropertyValue(expression.PropertyName)) > (Convert.ToDecimal(expression.Values.FirstOrDefault()))));
                }
                break;

              case RelationalOperator.GreaterThanOrEqual:
                if (propertyType == "string")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) >= 0));
                }
                else if (propertyType.StartsWith("int") || propertyType == "single" || propertyType == "double" || propertyType == "decimal")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => Convert.ToDecimal(dataObject.GetPropertyValue(expression.PropertyName)) >= Convert.ToDecimal(expression.Values.FirstOrDefault()));
                }
                break;

              case RelationalOperator.LesserThan:
                if (propertyType == "string")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) < 0));
                }
                else if (propertyType.StartsWith("int") || propertyType == "single" || propertyType == "double" || propertyType == "decimal")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => Convert.ToDecimal(dataObject.GetPropertyValue(expression.PropertyName)) < Convert.ToDecimal(expression.Values.FirstOrDefault()));
                }
                break;

              case RelationalOperator.LesserThanOrEqual:
                if (propertyType == "string")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) <= 0));
                }
                else if (propertyType.StartsWith("int") || propertyType == "single" || propertyType == "double" || propertyType == "decimal")
                {
                  predicate = predicate.Or<IDataObject>(dataObject => Convert.ToDecimal(dataObject.GetPropertyValue(expression.PropertyName)) <= Convert.ToDecimal(expression.Values.FirstOrDefault()));
                }
                break;
            }
          }
          else if (expression.LogicalOperator == LogicalOperator.None || expression.LogicalOperator == LogicalOperator.And)
          {
            switch (expression.RelationalOperator)
            {
              case RelationalOperator.StartsWith:
                if (propertyType == "string")
                {
                  predicate = predicate.And<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).StartsWith(expression.Values.FirstOrDefault())));
                }
                else
                {
                  string error = "Error in GenerateWhereClause: BeginsWith operator used with non-string property";
                  _logger.Error(error);
                  throw new Exception(error);
                }
                break;

              case RelationalOperator.EndsWith:
                if (propertyType == "string")
                {
                  predicate = predicate.And<IDataObject>(dataObject => ((Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).EndsWith(expression.Values.FirstOrDefault()))));
                }
                else
                {
                  string error = "Error in GenerateWhereClause: EndsWith operator used with non-string property";
                  _logger.Error(error);
                  throw new Exception(error);
                }
                break;

              case RelationalOperator.Contains:
                if (propertyType == "string")
                {
                  predicate = predicate.And<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).Contains(expression.Values.FirstOrDefault())));
                }
                else
                {
                  string error = "Error in GenerateWhereClause: Contains operator used with non-string property";
                  _logger.Error(error);
                  throw new Exception(error);
                }
                break;

              case RelationalOperator.In:
                predicate = predicate.And<IDataObject>(dataObject => expression.Values.Contains((Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)))));
                break;

              case RelationalOperator.EqualTo:
                if (propertyType == "string")
                {
                  predicate = predicate.And<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) == 0));
                }
                else if (propertyType.StartsWith("int") || propertyType == "single" || propertyType == "double" || propertyType == "decimal")
                {
                  predicate = predicate.And<IDataObject>(dataObject => Convert.ToDecimal(dataObject.GetPropertyValue(expression.PropertyName)) == Convert.ToDecimal(expression.Values.FirstOrDefault()));
                }
                break;

              case RelationalOperator.NotEqualTo:
                if (propertyType == "string")
                {
                  predicate = predicate.And<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) != 0));
                }
                break;

              case RelationalOperator.GreaterThan:
                if (propertyType == "string")
                {
                  predicate = predicate.And<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) > 0));
                }
                else if (propertyType.StartsWith("int") || propertyType == "single" || propertyType == "double" || propertyType == "decimal")
                {
                  predicate = predicate.And<IDataObject>(dataObject => (Convert.ToDecimal(dataObject.GetPropertyValue(expression.PropertyName)) > (Convert.ToDecimal(expression.Values.FirstOrDefault()))));
                }
                break;

              case RelationalOperator.GreaterThanOrEqual:
                if (propertyType == "string")
                {
                  predicate = predicate.And<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) >= 0));
                }
                else if (propertyType.StartsWith("int") || propertyType == "single" || propertyType == "double" || propertyType == "decimal")
                {
                  predicate = predicate.And<IDataObject>(dataObject => Convert.ToDecimal(dataObject.GetPropertyValue(expression.PropertyName)) >= Convert.ToDecimal(expression.Values.FirstOrDefault()));
                }
                break;

              case RelationalOperator.LesserThan:
                if (propertyType == "string")
                {
                  predicate = predicate.And<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) < 0));
                }
                else if (propertyType.StartsWith("int") || propertyType == "single" || propertyType == "double" || propertyType == "decimal")
                {
                  predicate = predicate.And<IDataObject>(dataObject => Convert.ToDecimal(dataObject.GetPropertyValue(expression.PropertyName)) < Convert.ToDecimal(expression.Values.FirstOrDefault()));
                }
                break;

              case RelationalOperator.LesserThanOrEqual:
                if (propertyType == "string")
                {
                  predicate = predicate.And<IDataObject>(dataObject => (Convert.ToString(dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) <= 0));
                }
                else if (propertyType.StartsWith("int") || propertyType == "single" || propertyType == "double" || propertyType == "decimal")
                {
                  predicate = predicate.And<IDataObject>(dataObject => Convert.ToDecimal(dataObject.GetPropertyValue(expression.PropertyName)) <= Convert.ToDecimal(expression.Values.FirstOrDefault()));
                }
                break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GenerateWherePredicate: " + ex);
        throw new Exception("Error while generating filter", ex);
      }

      return predicate.Compile();
    }
  }
}
