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

namespace Bechtel.CSVDataLayer
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
      _dataDictionaryPath = settings.XmlPath + "DataDictionary." + appSettings.ApplicationName + ".xml";
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
      throw new NotImplementedException();
    }

    public Response Post<T>(T graph)
    {
      Response response;
      try
      {
        response = new Response();

        CSVObject csvObject = (CSVObject)(object)graph;

        

        response.Add("Records of type " + typeof(T).Name + " have been updated successfully");
      
        return response;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Post<T>: " + exception);
        throw new Exception("Error while posting data of type " + typeof(T).Name + ".", exception);
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
