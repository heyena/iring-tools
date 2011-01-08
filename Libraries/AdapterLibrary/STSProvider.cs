﻿using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using Ninject;

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

    private AdapterSettings _settings = null;

    [Inject]
    public STSProvider(AdapterSettings settings)
    {
      _settings = settings;
    }

    public void Initialize()
    {
      HttpContext context = System.Web.HttpContext.Current;

      string header = context.Request.Headers["Authorization"];
      string restTokenAddress = _settings["STSAddress"];

      DataContractJsonSerializer dictionarySerializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
      WebClient client = new WebClient();
      client.UseDefaultCredentials = true;

      string xmlToken = client.DownloadString(restTokenAddress + "/JSON/DecodeOAuthHeader?header=" + header);
      MemoryStream str = new MemoryStream(Encoding.Unicode.GetBytes(xmlToken));
      str.Position = 0;

      Dictionary<string, string> result = (Dictionary<string, string>)dictionarySerializer.ReadObject(str);
      _settings["UserName"] = result.Values.ToString();
    }
  }
}
