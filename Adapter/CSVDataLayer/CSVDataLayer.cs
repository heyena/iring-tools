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
              //NOTE: the key property can be hardcoded if it is known or can be looked up in data dictionary
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

    //TODO: handle filter
    public IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {
        List<string> identifiers = new List<string>();
        string dataObjectPath = GetDataObjectPath(objectType);

        // Read all files (commodity rows) from commodity path of the application
        DirectoryInfo directory = new DirectoryInfo(dataObjectPath);
        FileInfo[] files = directory.GetFiles();

        foreach (FileInfo file in files)
        {
          string identifier = file.Name.Substring(0, ".csv".Length);
          identifiers.Add(identifier);
        }

        if (filter != null && filter.Expressions.Count > 0)
        {
          string nhWhereClause = GenerateWhereClause(objectType, filter);
        }

        return identifiers;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception("Error while getting a list of identifiers of type [" + objectType + "].", ex);
      }
    }

    //TODO: handle filter, pageSize, and pageNumber
    public IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int pageNumber)
    {
      try
      {
        List<IDataObject> dataObjects = new List<IDataObject>();
        Type type = Type.GetType(GetQualifiedObjectType(objectType) + "DataObject");
            
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
              string attrDataType = attr.Attribute("dataType").Value;
              string csvValue = csvValues[index++].Trim();

              // if data type is not nullable, make sure it has a value 
              if (!(attrDataType.EndsWith("?") && csvValue == String.Empty))
              {
                if (attrDataType.ToLower().Contains("bool"))
                {
                  if (csvValue.ToLower() == "true" || csvValue.ToLower() == "yes")
                  {
                    csvValue = "1";
                  }
                  else
                  {
                    csvValue = "0";
                  }
                }
                else if (csvValue == String.Empty && (
                         attrDataType.ToLower().Contains("int") ||
                         attrDataType.ToLower() == "double" ||
                         attrDataType.ToLower() == "single" ||
                         attrDataType.ToLower() == "float" ||
                         attrDataType.ToLower() == "decimal"))
                {
                  csvValue = "0";
                }

                dataObject.SetPropertyValue(attrName, csvValue);
              }
            }

            dataObjects.Add(dataObject);
          }
        }

        if (filter != null && filter.Expressions.Count > 0)
        {
          string whereClause = GenerateWhereClause(GetQualifiedObjectType(objectType), filter);

          //TODO: use predicate builder to build whereLamdaClause
          var predicate = PredicateBuilder.False<IDataObject>();
          predicate = predicate.Or<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue("Tag")).Contains("abc"));
          dataObjects = dataObjects.Where(predicate.Compile()).ToList();
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
        Type type = Type.GetType(GetQualifiedObjectType(objectType) + "DataObject");

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
              string attrDataType = attr.Attribute("dataType").Value;
              string csvValue = csvValues[index++].Trim();

              // if data type is not nullable, make sure it has a value 
              if (!(attrDataType.EndsWith("?") && csvValue == String.Empty))
              {
                if (attrDataType.ToLower().Contains("bool"))
                {
                  if (csvValue.ToLower() == "true" || csvValue.ToLower() == "yes")
                  {
                    csvValue = "1";
                  }
                  else
                  {
                    csvValue = "0";
                  }
                }
                else if (csvValue == String.Empty && (
                         attrDataType.ToLower().Contains("int") ||
                         attrDataType.ToLower() == "double" ||
                         attrDataType.ToLower() == "single" ||
                         attrDataType.ToLower() == "float" ||
                         attrDataType.ToLower() == "decimal"))
                {
                  csvValue = "0";
                }

                dataObject.SetPropertyValue(attrName, csvValue);
              }
            }
            ////////////////
            string exp = string.Empty;
            var p = System.Linq.Expressions.Expression.Parameter(typeof(IDataObject), "dataObject");
            //var e = System.Linq.Expressions.Expression.DynamicExpression.ParseLambda(new[] { p }, null, exp);
            //var result = e.Compile().DynamicInvoke(dataObjects);   
            ////////////////
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

    //TODO: handle filter
    public Response Delete(string objectType, DataFilter filter)
    {
      Response response = new Response();

      try
      {
        if (filter != null && filter.Expressions.Count > 0)
        {

        }
        else
        {
          string dataObjectPath = GetDataObjectPath(objectType);
          Directory.Delete(dataObjectPath);
        }

        return response;
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

    private string GenerateWhereClause(string objectType, DataFilter filter)
    {
      if (filter == null || filter.Expressions.Count == 0)
      {
        return String.Empty;
      }

      try
      {
        var predicate = PredicateBuilder.False<IDataObject>();
        
        foreach (Expression expression in filter.Expressions)
        {
          Type propertyType = Type.GetType(objectType).GetProperty(expression.PropertyName).PropertyType;
          bool isString = (propertyType == typeof(string));

          switch (expression.LogicalOperator)
          {
            case LogicalOperator.None | LogicalOperator.Or:
              switch (expression.RelationalOperator)
              {
                case RelationalOperator.BeginsWith:
                  if (isString)
                  {
                    predicate = predicate.Or<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).StartsWith(expression.Values.FirstOrDefault()));
                  }
                  else
                  {
                    string error = "Error in GenerateWhereClause: BeginsWith operator used with non-string property";
                    _logger.Error(error);
                    throw new Exception(error);
                  }
                  break;

                case RelationalOperator.EndsWith:
                  if (isString)
                  {
                    predicate = predicate.Or<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).EndsWith(expression.Values.FirstOrDefault()));
                  }
                  else
                  {
                    string error = "Error in GenerateWhereClause: EndsWith operator used with non-string property";
                    _logger.Error(error);
                    throw new Exception(error);
                  }
                  break;

                case RelationalOperator.Contains:
                  if (isString)
                  {
                    predicate = predicate.Or<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).Contains(expression.Values.FirstOrDefault()));
                  }
                  else
                  {
                    string error = "Error in GenerateWhereClause: Contains operator used with non-string property";
                    _logger.Error(error);
                    throw new Exception(error);
                  }
                  break;
                case RelationalOperator.EqualTo:
                  if (isString)
                  {
                    predicate = predicate.Or<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) == 0);
                  }
                  break;

                case RelationalOperator.NotEqualTo:
                  if (isString)
                  {
                    predicate = predicate.Or<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) != 0);
                  }
                  break;

                case RelationalOperator.GreaterThan:
                  if (isString)
                  {
                    predicate = predicate.Or<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) > 0);
                  }
                  break;

                case RelationalOperator.GreaterThanOrEqual:
                  if (isString)
                  {
                    predicate = predicate.Or<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) >= 0);
                  }
                  break;

                case RelationalOperator.In:
                  if (isString)
                  {
                    predicate = predicate.Or<IDataObject>(dataObject => expression.Values.Contains(((string)dataObject.GetPropertyValue(expression.PropertyName))));
                  }
                  break;

                case RelationalOperator.LesserThan:
                  if (isString)
                  {
                    predicate = predicate.Or<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) < 0);
                  }
                  break;

                case RelationalOperator.LesserThanOrEqual:
                  if (isString)
                  {
                    predicate = predicate.Or<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) <= 0);
                  }
                  break;
              }
              break;

            case LogicalOperator.And:
              switch (expression.RelationalOperator)
              {
                case RelationalOperator.BeginsWith:
                  if (isString)
                  {
                    predicate = predicate.And<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).StartsWith(expression.Values.FirstOrDefault()));
                  }
                  else
                  {
                    string error = "Error in GenerateWhereClause: BeginsWith operator used with non-string property";
                    _logger.Error(error);
                    throw new Exception(error);
                  }
                  break;

                case RelationalOperator.EndsWith:
                  if (isString)
                  {
                    predicate = predicate.And<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).EndsWith(expression.Values.FirstOrDefault()));
                  }
                  else
                  {
                    string error = "Error in GenerateWhereClause: EndsWith operator used with non-string property";
                    _logger.Error(error);
                    throw new Exception(error);
                  }
                  break;

                case RelationalOperator.Contains:
                  if (isString)
                  {
                    predicate = predicate.And<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).Contains(expression.Values.FirstOrDefault()));
                  }
                  else
                  {
                    string error = "Error in GenerateWhereClause: Contains operator used with non-string property";
                    _logger.Error(error);
                    throw new Exception(error);
                  }
                  break;

                case RelationalOperator.EqualTo:
                  if (isString)
                  {
                    predicate = predicate.And<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) == 0);
                  }
                  break;

                case RelationalOperator.NotEqualTo:
                  if (isString)
                  {
                    predicate = predicate.And<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) != 0);
                  }
                  break;

                case RelationalOperator.GreaterThan:
                  if (isString)
                  {
                    predicate = predicate.And<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) > 0);
                  }
                  break;

                case RelationalOperator.GreaterThanOrEqual:
                  if (isString)
                  {
                    predicate = predicate.And<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) >= 0);
                  }
                  break;

                case RelationalOperator.In:
                  if (isString)
                  {
                    predicate = predicate.And<IDataObject>(dataObject => expression.Values.Contains(((string)dataObject.GetPropertyValue(expression.PropertyName))));
                  }
                  break;

                case RelationalOperator.LesserThan:
                  if (isString)
                  {
                    predicate = predicate.And<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) < 0);
                  }
                  break;

                case RelationalOperator.LesserThanOrEqual:
                  if (isString)
                  {
                    predicate = predicate.And<IDataObject>(dataObject => ((string)dataObject.GetPropertyValue(expression.PropertyName)).CompareTo(expression.Values.FirstOrDefault()) <= 0);
                  }
                  break;
              }
              break;
          }          
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GenerateWhereClause: " + ex);
      }

      return null;
    }

    private string GenerateWhereClause1(string objectType, DataFilter filter)
    {
      if (filter == null || filter.Expressions.Count == 0)
      {
        return String.Empty;
      }

      try
      {
        StringBuilder whereClause = new StringBuilder();
        whereClause.Append("dataObject => ");

        foreach (Expression expression in filter.Expressions)
        {
          if (expression.LogicalOperator != LogicalOperator.None)
          {
            whereClause.Append(ResolveLogicalOperator(expression.LogicalOperator));
          }

          for (int i = 0; i < expression.OpenGroupCount; i++)
          {
            whereClause.Append("(");
          }

          string propertyName = expression.PropertyName;
          whereClause.Append(propertyName);

          string relationalOperator = ResolveRelationalOperator(expression.RelationalOperator);
          whereClause.Append(relationalOperator);

          Type propertyType = Type.GetType(objectType).GetProperty(propertyName).PropertyType;
          bool isString = propertyType == typeof(string);

          if (expression.RelationalOperator == RelationalOperator.In)
          {
            whereClause.Append("(");

            foreach (string value in expression.Values)
            {
              if (whereClause.ToString() != "(")
              {
                whereClause.Append(", ");
              }

              if (isString)
              {
                whereClause.Append("'" + value + "'");
              }
              else
              {
                whereClause.Append(value);
              }
            }

            whereClause.Append(")");
          }
          else
          {
            string value = String.Empty;

            if (expression.RelationalOperator == RelationalOperator.BeginsWith ||
                expression.RelationalOperator == RelationalOperator.Contains ||
                expression.RelationalOperator == RelationalOperator.EndsWith)
            {
              if (isString)
              {
                value = "\"(" + expression.Values.FirstOrDefault() + "\")";
              }
              else
              {
                _logger.Error("Error in GenerateNHFilter: like operator used with non-string property");
                throw new Exception("Error while generating an NHibernate filter. Like operator used with non-string property");
              }
            }
            else
            {
              if (isString)
              {
                value = "'" + expression.Values.FirstOrDefault() + "'";
              }
              else
              {
                value = expression.Values.FirstOrDefault();
              }
            }
            
            whereClause.Append(value);
          }

          for (int i = 0; i < expression.CloseGroupCount; i++)
          {
            whereClause.Append(")");
          }
        }

        return whereClause.ToString();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GenerateNHFilter: " + ex);
        throw new Exception("Error while generating an NHibernate filter.", ex);
      }
    }

    private string ResolveRelationalOperator(RelationalOperator relationalOperator)
    {
      string linqRelationalOperator = String.Empty;
      
      switch (relationalOperator)
      {
        case RelationalOperator.BeginsWith:
          linqRelationalOperator = ".BeginsWith";
          break;

        case RelationalOperator.Contains:
          linqRelationalOperator = ".Contains";
          break;

        case RelationalOperator.EndsWith:
          linqRelationalOperator = ".EndsWith";
          break;

        case RelationalOperator.EqualTo:
          linqRelationalOperator = " == ";
          break;

        case RelationalOperator.GreaterThan:
          linqRelationalOperator = " > ";
          break;

        case RelationalOperator.GreaterThanOrEqual:
          linqRelationalOperator = " >= ";
          break;

        case RelationalOperator.In:
          linqRelationalOperator = " in ";
          break;

        case RelationalOperator.LesserThan:
          linqRelationalOperator = " < ";
          break;

        case RelationalOperator.LesserThanOrEqual:
          linqRelationalOperator = " <= ";
          break;

        case RelationalOperator.NotEqualTo:
          linqRelationalOperator = " != ";
          break;
      }

      return linqRelationalOperator;
    }

    private string ResolveLogicalOperator(LogicalOperator logicalOperator)
    {
      string linqLogicalOperator = String.Empty;

      switch (logicalOperator)
      {
        case LogicalOperator.And:
          linqLogicalOperator = " && ";
          break;

        case LogicalOperator.AndNot:
          linqLogicalOperator = " && !";
          break;

        case LogicalOperator.Not:
          linqLogicalOperator = " !";
          break;

        case LogicalOperator.Or:
          linqLogicalOperator = " || ";
          break;

        case LogicalOperator.OrNot:
          linqLogicalOperator = " || !";
          break;
      }

      return linqLogicalOperator;
    }
  }
}
