using System;
using org.iringtools.library;
using System.Xml;
using System.IO;
using System.Configuration;
using Ninject;
using log4net;
using System.Collections.Generic;
using org.iringtools.adapter;
using org.iringtools.utility;
using System.Xml.Linq;
using System.Linq;
using Bechtel.CSVDataLayer.API;

namespace Bechtel.CSVDataLayer
{
  public class DataLayer : IDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DataLayer));
    private AdapterSettings _settings = null;
    private ApplicationSettings _appSettings = null;
    private string _dataDictionaryPath = String.Empty;

    [Inject]
    public DataLayer(AdapterSettings settings, ApplicationSettings appSettings)
    {
      _settings = settings;
      _appSettings = appSettings;
      _dataDictionaryPath = settings.XmlPath + "DataDictionary." + _appSettings.ProjectName + "." + _appSettings.ApplicationName + ".xml";      
    }

    public T Get<T>(Dictionary<string, object> queryProperties)
    {
      try
      {
        return default(T);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while getting data of type " + typeof(T).Name + ".", exception);
      }
    }

    public IList<T> GetList<T>()
    {
      return GetList<T>(null);
    }

    public IList<T> GetList<T>(Dictionary<string, object> queryProperties)
    {
      try
      {
        List<T> list = new List<T>();

        // Load config xml 
        string configFile = _settings.XmlPath + typeof(T).Name + "." + _appSettings.ProjectName + "." + _appSettings.ApplicationName + ".xml";
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

            XElement item = new XElement(typeof(T).Name);
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

            list.Add(Utility.Deserialize<T>(item.ToString(), false));
          }
        }

        return list;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetList<T>: " + exception);
        throw new Exception("Error while getting data of type " + typeof(T).Name + ".", exception);
      }
    }

    public Response PostList<T>(System.Collections.Generic.List<T> graphList)
    {
      throw new NotImplementedException();
    }

    public Response Post<T>(T graph)
    {
      throw new NotImplementedException();
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
