using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Threading;


namespace iRINGTools.Web.Models
{
  public class FileRepository : IFileRepository
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(FileRepository));

    protected ServiceSettings _settings;
    protected string _proxyHost;
    protected string _proxyPort;
    protected string _adapterServiceUri = null;
    protected string _dataServiceUri = null;
    protected string _hibernateServiceUri = null;
    protected string _referenceDataServiceUri = null;

    public IDictionary<string, string> AuthHeaders { get; set; }

    public FileRepository()
    {
      NameValueCollection settings = ConfigurationManager.AppSettings;

      _settings = new ServiceSettings();
      _settings.AppendSettings(settings);

      _proxyHost = _settings["ProxyHost"];
      _proxyPort = _settings["ProxyPort"];

      _adapterServiceUri = _settings["AdapterServiceUri"];
      if (_adapterServiceUri.EndsWith("/"))
        _adapterServiceUri = _adapterServiceUri.Remove(_adapterServiceUri.Length - 1);

      _dataServiceUri = _settings["DataServiceUri"];
      if (_dataServiceUri.EndsWith("/"))
        _dataServiceUri = _dataServiceUri.Remove(_dataServiceUri.Length - 1);

      _hibernateServiceUri = _settings["HibernateServiceUri"];
      if (_hibernateServiceUri.EndsWith("/"))
        _hibernateServiceUri = _hibernateServiceUri.Remove(_hibernateServiceUri.Length - 1);

      _referenceDataServiceUri = _settings["RefDataServiceUri"];
      if (_referenceDataServiceUri.EndsWith("/"))
        _referenceDataServiceUri = _referenceDataServiceUri.Remove(_referenceDataServiceUri.Length - 1);
    }

    public HttpSessionStateBase Session { get; set; }

    protected WebHttpClient CreateWebClient(string baseUri)
    {
      WebHttpClient client = null;

      if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
      {
        WebProxy webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
        client = new WebHttpClient(baseUri, null, webProxy);
      }
      else
      {
        client = new WebHttpClient(baseUri);
      }

      if (AuthHeaders != null && AuthHeaders.Count > 0)
      {
        _logger.Debug("Injecting authorization [" + AuthHeaders.Count + "] headers.");
        client.Headers = AuthHeaders;
      }
      else
      {
        _logger.Debug("No authorization headers.");
      }

      return client;
    }

    protected T WaitForRequestCompletion<T>(string baseUri, string statusUrl)
    {
      T obj;

      try
      {
        long timeoutCount = 0;

        long asyncTimeout = 1800;  // seconds
        if (_settings["AsyncTimeout"] != null)
        {
          long.TryParse(_settings["AsyncTimeout"], out asyncTimeout);
        }
        asyncTimeout *= 1000;  // convert to milliseconds

        int asyncPollingInterval = 2;  // seconds
        if (_settings["AsyncPollingInterval"] != null)
        {
          int.TryParse(_settings["AsyncPollingInterval"], out asyncPollingInterval);
        }
        asyncPollingInterval *= 1000;  // convert to milliseconds

        WebHttpClient client = CreateWebClient(baseUri);
        RequestStatus requestStatus = null;

        while (timeoutCount < asyncTimeout)
        {
          requestStatus = client.Get<RequestStatus>(statusUrl);

          if (requestStatus.State != State.InProgress)
            break;

          Thread.Sleep(asyncPollingInterval);
          timeoutCount += asyncPollingInterval;
        }

        if (requestStatus.State != State.Completed)
        {
          throw new Exception(requestStatus.Message);
        }

        if (typeof(T) == typeof(string))
        {
          obj = (T)Convert.ChangeType(requestStatus.ResponseText, typeof(T));
        }
        else
        {
          obj = Utility.Deserialize<T>(requestStatus.ResponseText, true);
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message);
        throw ex;
      }

      return obj;
    }

    ///Done by: Ganesh Bisht: Polaris 
    /// <summary>
    /// Used for uploading the file to the server
    /// </summary>
    /// <param name="scope">scope name</param>
    /// <param name="application">application name</param>
    /// <param name="inputFile">input file stream</param>
    /// <param name="filename">filename</param>
    /// <returns>response with status and message</returns>
    public Response PostFile(string scope, string application, Stream inputFile, string filename)
      {
          try
          {
              var reluri = String.Format("/{0}/{1}/upload", scope, application);
             
              WebHttpClient client = CreateWebClient(_adapterServiceUri);
              client.Timeout = -1;
              Response response = Utility.Deserialize<Response>(client.PostStreamUpload(reluri, inputFile, filename), true);
              return response;
          }
          catch (Exception ex)
          {
              _logger.Error(ex.Message);

              Response response = new Response()
              {
                  Level = StatusLevel.Error,
                  Messages = new Messages { ex.Message }
              };

              return response;
          }
          
      }

      ///Done by: Ganesh Bisht: Polaris 
      /// <summary>
      /// Will get the file content in byte array
      /// </summary>
      /// <param name="scope"></param>
      /// <param name="application"></param>
      /// <param name="fileName"></param>
      /// <param name="ext"></param>
      /// <returns>byte array</returns>
      public byte[] getFile(string scope, string application, string fileName, string ext)
      {
          try
          {

              var reluri = String.Format("/{0}/{1}/File/{2}/{3}", scope, application,fileName,ext);
              
              WebHttpClient client = CreateWebClient(_adapterServiceUri);

              DocumentBytes pathObject = client.Get<DocumentBytes>(reluri, true);

              return pathObject.Content;
          }
          catch (Exception ioEx)
          {
              _logger.Error(ioEx.Message);
              throw ioEx;
          }
      }

      ///Done by: Ganesh Bisht: Polaris 
      /// <summary>
      /// List of all the available download files
      /// </summary>
      /// <param name="scope">scope name</param>
      /// <param name="application">application name</param>
      /// <returns> collection of file object</returns>   
      public List<Files> getDownloadedList(string scope,string application)
    {
 	     try
          {

              var reluri = String.Format("/{0}/{1}/Downloadlist", scope, application);
              
              WebHttpClient client = CreateWebClient(_adapterServiceUri);

              List<Files> files = client.Get<List<Files>>(reluri, true);

              return files;
          }
          catch (Exception ioEx)
          {
              _logger.Error(ioEx.Message);
              throw ioEx;
          }
    }
  
  }
}