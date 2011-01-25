using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using Ninject;
using System.Collections;
using System;
using org.iringtools.utility;
using log4net;

namespace org.iringtools.adapter.identity
{
  public class STSProvider : IIdentityLayer
  {
    /// <summary>
    /// Classes for dealing with STS for web services.
    /// 
    /// Requires a web operation context...wcf use only
    /// To enable add app settings:
    /// <add key="restTokenAddress" value will be listed shortly/>
    /// </summary>
    /// 

    private static readonly ILog _logger = LogManager.GetLogger(typeof(STSProvider));
    private AdapterSettings _settings = null;

    [Inject]
    public STSProvider(AdapterSettings settings)
    {
      _settings = settings;
    }

    public IDictionary GetKeyRing()
    {
      string restTokenAddress = String.Empty;

      try
      {

        IDictionary keyRing = new Dictionary<string, string>();

        HttpContext context = System.Web.HttpContext.Current;

        string header = context.Request.Headers["Authorization"];
        string proxyHost = _settings["ProxyHost"];
        string proxyPort = _settings["ProxyPort"];
        
        restTokenAddress = _settings["STSAddress"];
     
        DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
        WebHttpClient webHttpClient = null;

        if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
        {
          WebProxy webProxy = new WebProxy(proxyHost, Int32.Parse(proxyPort)); 
          
          webProxy.Credentials = _settings.GetProxyCredential();
        
          webHttpClient = new WebHttpClient(restTokenAddress, null, webProxy);
        }
        else
        {
          webHttpClient = new WebHttpClient(restTokenAddress);
        }

        string relativeUrl = "/JSON/DecodeOAuthHeader?header=" + header;
        string json = webHttpClient.GetMessage(relativeUrl);
        MemoryStream str = new MemoryStream(Encoding.Unicode.GetBytes(json));
        str.Position = 0;

        _logger.Debug(json);

        if (!String.IsNullOrWhiteSpace(json))
        {
          keyRing = (IDictionary)jsonSerializer.ReadObject(str);
          keyRing.Add("Provider", "SecureTokenProvider");
        }

        return keyRing;
      }
      catch (Exception ex)
      {
        throw new Exception("Error while trying to get the KeyRing from: " + restTokenAddress, ex);
      }
    }
  }
}
