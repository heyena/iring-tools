using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using log4net;

namespace ExchangeTask
{
  class Program
  {
    private static string _baseUrl = String.Empty;
    private static string _scope = String.Empty;
    private static string _commodityName = String.Empty;
    private static string _exchangeId = String.Empty;
    private static string _proxyHost = String.Empty;
    private static int _proxyPort =0;
    private static string _proxyCredentialToken = String.Empty;
    private static WebHttpClient _httpClient = null;

    private static ILog _logger = LogManager.GetLogger(typeof(Program));

    static void Main(string[] args)
    {
      log4net.Config.XmlConfigurator.Configure();
      RunExchange(args);

      Console.WriteLine("press any key to close this window...");
      Console.ReadKey();
    }

    private static void RunExchange(string[] args)
    {
      if (Initialize(args))
      {
        string relativeUrl = String.Format("?scope={0}&xid={1}", _scope, _exchangeId);

        if (String.IsNullOrWhiteSpace(_proxyCredentialToken))
        {
          _httpClient = new WebHttpClient(_baseUrl);
        }
        else
        {
          WebCredentials credential = new WebCredentials(_proxyCredentialToken);
          _httpClient = new WebHttpClient(_baseUrl, credential.userName, credential.password, credential.domain, _proxyHost, _proxyPort);
        }

        _logger.Debug("Send exchange request...");
        Console.WriteLine("Send exchange request...");
        string responseMessage = _httpClient.GetMessage(relativeUrl);
        Console.WriteLine("Response Message");
        _logger.Info("Response Message");
        Console.WriteLine(responseMessage);
        _logger.Info(responseMessage);

      }
    }

    private static bool  Initialize(string [] args)
    {
      _logger.Debug("Initialize ...");

      try
      {
      _baseUrl = System.Configuration.ConfigurationManager.AppSettings["BaseUrl"];
      _scope = System.Configuration.ConfigurationManager.AppSettings["Scope"];
      _commodityName = System.Configuration.ConfigurationManager.AppSettings["CommodityName"];
      _exchangeId = System.Configuration.ConfigurationManager.AppSettings["ExchangeId"];
      _proxyHost = System.Configuration.ConfigurationManager.AppSettings["ProxyHost"];
      _proxyCredentialToken = System.Configuration.ConfigurationManager.AppSettings["ProxyCredentialToken"];

      string proxyPortString = System.Configuration.ConfigurationManager.AppSettings["ProxyPort"];
      int.TryParse(proxyPortString,out _proxyPort);

      return true;
      }
      catch(Exception ex)
      {
        _logger.Error(ex.Message,ex);
        return false;
      }
    }
  }
}
