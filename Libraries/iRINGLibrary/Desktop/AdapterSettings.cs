using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;
using System.Configuration;
using System.Collections.Specialized;
using org.iringtools.utility;
using System.ServiceModel;

namespace org.iringtools.adapter
{
  public class AdapterSettings : ServiceSettings
  {
    private string _encryptedToken = String.Empty;
    private string _interfaceServerUri = String.Empty;
    private string _binaryPath = string.Empty;
    private string _codePath = string.Empty;
    private string _dotnetRDFDbName = string.Empty;
    private string _dotnetRDFDbServer = string.Empty;
    private string _dotnetRDFDbUser = string.Empty;
    private string _dotnetRDFDbPassword = string.Empty;
    private string _graphBaseUri = string.Empty;
    private string _defaultProjectionFormat = string.Empty;
    private int _endpointTimeout = 0;

    public AdapterSettings(NameValueCollection AppSettings)
    {
      this.BaseDirectoryPath = AppSettings["BaseDirectoryPath"];
      this.ExecutingAssemblyName = AppSettings["ExecutingAssemblyName"];
      this.XmlPath = AppSettings["XmlPath"];
      this.EncryptedProxyToken = AppSettings["ProxyCredentialToken"];
      this.ProxyHost = AppSettings["ProxyHost"];
      this.ProxyPort = AppSettings["ProxyPort"];
      this.InterfaceServer = AppSettings["InterfaceService"];
      this.TrimData = Convert.ToBoolean(AppSettings["TrimData"]);
      this.BinaryPath = AppSettings["BinaryPath"];
      this.CodePath = AppSettings["CodePath"];
      this.DBServer = AppSettings["DBServer"];
      this.DBname = AppSettings["DBName"];
      this.DBUser = AppSettings["DBUser"];
      this.DBPassword = AppSettings["DBPassword"];
      this.EndpointTimeout = Convert.ToInt32(AppSettings["EndpointTimeout"]);
      this.GraphBaseUri = AppSettings["GraphBaseUri"];
      this.DefaultProjectionFormat = AppSettings["DefaultProjectionFormat"];
      this.PrepareCredentials();
    }    

    public string GraphBaseUri
    {
        get
        { return _graphBaseUri; }
        set
        {
          if (value == null)
          {
            var baseAddress = OperationContext.Current.Host.BaseAddresses[0];
            _graphBaseUri = baseAddress.ToString();
          }
          else
          {
            this._graphBaseUri = value;
          }
        }
    }

    public string DefaultProjectionFormat
    {
      get
      { return _defaultProjectionFormat; }
      set
      {
        if (value == null)
        {
          _defaultProjectionFormat = "xml";
        }
        else
        {
          _defaultProjectionFormat = value;
        }
      }
    }

    public Mapping Mapping { get; set; }

    public int EndpointTimeout
    {
        get
        { return _endpointTimeout; }
        set
        {
          if (value == 0)
          {
            _endpointTimeout = 30000;
          }
          else
          {
            this._endpointTimeout = value;
          }
        }
    }

    public string DBname
    {
        get
        { return _dotnetRDFDbName; }
        set
        {
          if (value == null)
          {
            _dotnetRDFDbName = "dotNetRdf";
          }
          else
          {
            _dotnetRDFDbName = value;
          }
        }
    }

    public string DBServer
    {
        get
        { return _dotnetRDFDbServer; }
        set
        {
          if (value == null)
          {
            _dotnetRDFDbServer = ".\\SQLEXPRESS";
          }
          else
          {
            _dotnetRDFDbServer = value;
          }
       }
    }

    public string DBUser
    {
        get
        { return _dotnetRDFDbUser; }
        set
        {
          if (value == null)
          {
            _dotnetRDFDbUser = "dotNetRDF";
          }
          else
          {
            _dotnetRDFDbUser = value;
          }
        }
    }

    public string DBPassword
    {
        get
        { return _dotnetRDFDbPassword; }
        set
        {
          if (value == null)
          {
            _dotnetRDFDbPassword = "dotNetRDF";
          }
          else
          {
              _dotnetRDFDbPassword = value;
          }
        }
    }

    public string InterfaceServer 
    {
      get
      {
        return _interfaceServerUri;
      }
      set
      {
        if (value == String.Empty || value == null)
        {
          _interfaceServerUri = @"http://localhost/InterfaceService/sparql/";
        }
        else
        {
          _interfaceServerUri = value;
        }
      }
    }

    //Booleans are handled by Convert
    public bool TrimData { get; set; }

    public string BinaryPath
    {
      get
      {
        return _binaryPath;
      }
      set
      {
        if (value == String.Empty || value == null)
        {
          _binaryPath = @".\Bin\";
        }
        else
        {
          _binaryPath = value;
        }
      }
    }

    public string CodePath
    {
      get
      {
        return _codePath;
      }
      set
      {
        if (value == String.Empty || value == null)
        {
          _codePath = @".\App_Code\";
        }
        else
        {
          _codePath = value;
        }
      }
    }
  }

  
}
