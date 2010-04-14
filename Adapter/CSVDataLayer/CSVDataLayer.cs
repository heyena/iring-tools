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

    public object Create(string objectType, string identifier)
    {
      try
      {
        return CreateList(objectType, new List<string> { identifier }).FirstOrDefault();
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Create: " + exception);
        throw new Exception("Error while creating data of type " + objectType + ".", exception);
      }
    }

    public IList<object> CreateList(string objectType, List<string> identifiers)
    {
      try
      {
        IList<object> dataObjects = new List<object>();

        foreach (string identifier in identifiers)
        {
          Type type = Type.GetType(objectType);
          object dataObject = Activator.CreateInstance(type);

          ((IDataObject)dataObject).SetPropertyValue("Id", identifier);
          dataObjects.Add(dataObject);

          dataObjects.Add(dataObject);
        }

        return dataObjects;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in CreateList: " + exception);
        throw new Exception("Error while creating a list of data of type " + objectType + ".", exception);
      }
    }

    public object Get(string objectType, string identifier)
    {
      try
      {
        return null;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Get: " + exception);
        throw new Exception("Error while getting data of type " + objectType + ".", exception);
      }
    }

    public IList<object> GetList(string objectType, DataFilter filter, int pageSize, int pageNumber)
    {
      try
      {
        List<object> list = new List<object>();

        // Load config xml 
        string configFile = _settings.XmlPath + objectType + "." + _appSettings.ProjectName + "." + _appSettings.ApplicationName + ".xml";
        XDocument configDoc = XDocument.Load(configFile);
        XElement commodity = configDoc.Element("commodity");

        // Get commodity path
        string path = commodity.Element("location").Value;
        if (!path.EndsWith("\\"))
        {
          path += "\\";
        }

        // Read all files (commodity rows) from commodity path
        DirectoryInfo directory = new DirectoryInfo(path);
        FileInfo[] files = directory.GetFiles();

        foreach (FileInfo file in files)
        {
          TextReader reader = new StreamReader(path + file.Name);
          string csvRow = reader.ReadLine();
          reader.Close();

          if (!String.IsNullOrEmpty(csvRow))
          {
            string[] csvValues = csvRow.Split(',');
            int index = 0;

            XElement item = new XElement(objectType);
            IEnumerable<XElement> attrs = commodity.Element("attributesSequence").Elements("attribute");

            foreach (var attr in attrs)
            {
              string attrName = attr.Attribute("name").Value;
              string attrDataType = attr.Attribute("dataType").Value;
              string csvValue = csvValues[index++].Trim();

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
                       attrDataType.ToLower() == "float" ||
                       attrDataType.ToLower() == "decimal"))
              {
                csvValue = "0";
              }

              item.Add(new XElement(attrName, csvValue));
            }

            list.Add(Utility.Deserialize<IDataObject>(item.ToString(), false));
          }
        }

        return list;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetList: " + exception);
        throw new Exception("Error while getting a list of data of type " + objectType + ".", exception);
      }
    }

    public Response Post(object dataObject)
    {
      Response response = new Response();
      
      try
      {
        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Post: " + exception);
        throw new Exception("Error while posting data of type " + dataObject.GetType().Name + ".", exception);
      }
    }

    public Response PostList(List<object> dataObjects)
    {
      Response response = new Response();
      
      try
      {
        foreach (object dataObject in dataObjects)
        {
          try
          {
            Response responseGraph = Post(dataObject);
            response.Append(responseGraph);
          }
          catch (Exception exception)
          {
            response.Add(exception.ToString());
          }
        }
        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in PostList: " + exception);

        object sample = dataObjects.FirstOrDefault();
        string objectType = (sample != null) ? sample.GetType().Name : String.Empty;
        throw new Exception("Error while posting data of type " + objectType + ".", exception);
      }
    }

    public Response Delete(string objectType, string identifier)
    {
      Response response = new Response();

      try
      {
        
        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Delete: " + exception);
        throw new Exception("Error while deleting data of type " + Type.GetType(objectType).Name + ".", exception);
      }
    }

    public Response DeleteList(string objectType, DataFilter filter)
    {
      Response response = new Response();

      try
      {
        
        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in DeleteList: " + exception);
        throw new Exception("Error while deleting data of type " + Type.GetType(objectType).Name + ".", exception);
      }
    }

    public DataDictionary GetDictionary()
    {
      return Utility.Read<DataDictionary>(_dataDictionaryPath);
    }

    public Response RefreshDictionary()
    {
      throw new NotImplementedException();
    }

  }
}
