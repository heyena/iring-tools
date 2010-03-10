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

namespace Bechtel.SPPIDDataLayer
{
  /// <remarks>
  /// The DataLayer class implements Get, Post, Update and Delete methods.
  /// This class uses .net entity framework to perform operations.
  /// The methods implemented in this class are generic methods dealing with type T.
  /// </remarks>
  public class DataLayer : IDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DataLayer));
    private AdapterSettings _settings = null;
    private ApplicationSettings _appSettings = null;
    private string _dataDictionaryPath = String.Empty;
    private string _optionsXmlPath = String.Empty;

    [Inject]
    public DataLayer(AdapterSettings settings, ApplicationSettings appSettings)
    {
      _dataDictionaryPath = settings.XmlPath + "DataDictionary." + 
        appSettings.ProjectName + "." + appSettings.ApplicationName + ".xml";

      _optionsXmlPath = settings.XmlPath + "Options." + 
        appSettings.ProjectName + "." + appSettings.ApplicationName + ".xml";
      
      _settings = settings;
      _appSettings = appSettings;
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
        return new List<T>();
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetList<T>: " + exception);
        throw new Exception("Error while getting data of type " + typeof(T).Name + ".", exception);
      }
    }

    public Response PostList<T>(System.Collections.Generic.List<T> graphList)
    {
      Response response;
      try
      {
        response = new Response();

        int numberOfRecords = 0;

        foreach (T graph in graphList)
        {
          numberOfRecords += IntergraphPost(graph);
        }

        response.Add(numberOfRecords + " records of type " + typeof(T).Name + " have been updated successfully");

        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Post<T>: " + exception);
        throw new Exception("Error while posting data of type " + typeof(T).Name + ".", exception);
      }
    }

    public Response Post<T>(T graph)
    {
      Response response;
      try
      {
        response = new Response();

        int numberOfRecords = IntergraphPost(graph);

        response.Add("Records of type " + typeof(T).Name + " have been updated successfully");
      
        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Post<T>: " + exception);
        throw new Exception("Error while posting data of type " + typeof(T).Name + ".", exception);
      }
    }

    private int IntergraphPost(object graph)
    {
      string updatesXml = String.Empty;
      string keysXml = String.Empty;

      try
      {
        SPPIDObject obj = (SPPIDObject)graph;
        string commodityName = obj.CommodityName;
        updatesXml = obj.GetUpdatesXml();
        keysXml = obj.GetKeysXml();

        int errorCode = 0;
        string errorMsg = string.Empty;
        XmlDocument doc = new XmlDocument();
        doc.Load(_optionsXmlPath);
        string optionsXml = doc.InnerXml;
        BechSPPIDBridge.Bridge bridgeClass = new BechSPPIDBridge.Bridge();
        bridgeClass.init(optionsXml, ref errorCode, ref errorMsg);

        _logger.Info(String.Format("Init - errorCode:{0} errorMsg:{1}", errorCode, errorMsg));

        int recordsUpdated = 0;
        if (errorCode == 0)
        {
          recordsUpdated = bridgeClass.updateAttributes(commodityName, keysXml, updatesXml, ref errorCode, ref errorMsg);

          _logger.Info(String.Format("UpdateAttributes - errorCode:{0} errorMsg:{1}", errorCode, errorMsg));
        }

        return recordsUpdated;
      }
      catch (Exception ex)
      {
        if (keysXml != String.Empty) Utility.WriteString(keysXml, _settings.XmlPath + "Keys.xml");
        if (updatesXml != String.Empty) Utility.WriteString(updatesXml, _settings.XmlPath + "Updates.xml");
        _logger.Error(ex);
        throw ex;
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
