using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;
using System.Configuration;
using System.Collections.Specialized;
using org.iringtools.utility;

namespace org.iringtools.adapter
{
  public class AdapterSettings : ServiceSettings
  {
    private string _encryptedToken = String.Empty;
    private string _tripleStoreConnectionString = String.Empty;
    private string _interfaceServerUri = String.Empty;
    private string _interfaceServerPath = String.Empty;
    private string _binaryPath = string.Empty;
    private string _codePath = string.Empty;
    private string _dotnetRDFDbName = string.Empty;
    private string _dotnetRDFDbServer = string.Empty;
    private string _dotnetRDFDbUser = string.Empty;
    private string _dotnetRDFDbPassword = string.Empty;
    private string _interfaceCredential = string.Empty;
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
      this.UseSemweb = Convert.ToBoolean(AppSettings["UseSemweb"]);
      this.UsedotnetRDF = Convert.ToBoolean(AppSettings["UsedotnetRDF"]);
      this.TripleStoreConnectionString = AppSettings["TripleStoreConnectionString"];
      this.InterfaceServer = AppSettings["InterfaceService"];
      this.InterfaceServerPath = AppSettings["InterfaceServicePath"];
      this.EncryptedToken = AppSettings["TargetCredentialToken"];
      this.TrimData = Convert.ToBoolean(AppSettings["TrimData"]);
      this.BinaryPath = AppSettings["BinaryPath"];
      this.CodePath = AppSettings["CodePath"];
      this.DBServer = AppSettings["DBServer"];
      this.DBname = AppSettings["DBName"];
      this.DBUser = AppSettings["DBUser"];
      this.DBPassword = AppSettings["DBPassword"];
      this.EndpointTimeout = Convert.ToInt32(AppSettings["EndpointTimeout"]);
      this.InterfaceCredentialToken = AppSettings["InterfaceCredentialToken"];
      this.GraphBaseUri = AppSettings["GraphBaseUri"];
      this.DefaultProjectionFormat = AppSettings["DefaultProjectionFormat"];
      this.PrepareCredentials();
    }    
  
    public string InterfaceCredentialToken 
    {
        get
        { return _interfaceCredential; }
        set
        { _interfaceCredential = value; }
    }

    public string GraphBaseUri
    {
        get
        { return _graphBaseUri; }
        set
        { this._graphBaseUri = value; }
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

    public WebCredentials InterfaceCredentials { get; set; }

    public WebCredentials TargetCredentials { get; set; }

    public Mapping Mapping { get; set; }

    public override void PrepareCredentials()
    {
      base.PrepareCredentials();

      if (String.IsNullOrEmpty(EncryptedToken))
      {
        this.TargetCredentials = new WebCredentials();
      }
      else
      {
        this.TargetCredentials = new WebCredentials(EncryptedToken);
        this.TargetCredentials.Decrypt();
      }
      if (String.IsNullOrEmpty(InterfaceCredentialToken))
      {
          this.InterfaceCredentials = new WebCredentials();
      }
      else
      {
          this.InterfaceCredentials = new WebCredentials(InterfaceCredentialToken);
          this.InterfaceCredentials.Decrypt();
      }
    }

    public int EndpointTimeout
    {
        get
        { return _endpointTimeout; }
        set
        { this._endpointTimeout = value; }
    }

    public string DBname
    {
        get
        { return _dotnetRDFDbName; }
        set
        { this._dotnetRDFDbName = value; }
    }

    public string DBServer
    {
        get
        { return _dotnetRDFDbServer; }
        set
        { this._dotnetRDFDbServer = value; }
    }

    public string DBUser
    {
        get
        { return _dotnetRDFDbUser; }
        set
        { this._dotnetRDFDbUser = value; }
    }

    public string DBPassword
    {
        get
        { return _dotnetRDFDbPassword; }
        set
        { this._dotnetRDFDbPassword = value; }
    }

    public string TripleStoreConnectionString 
    {
      get
      {
        return _tripleStoreConnectionString;
      }
      set
      {
        if (value == null)
        {
          _tripleStoreConnectionString = String.Empty;
        }
        else
        {
          _tripleStoreConnectionString = value;
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
          _interfaceServerUri = @"http://localhost/InterfaceService/sparql";
        }
        else
        {
          _interfaceServerUri = value;
        }
      }
    }

    //Booleans are handled by Convert
    public bool TrimData { get; set; }
    
    //Booleans are handled by Convert
    public bool UseSemweb { get; set; }

    //Booleans are handled by Convert
    public bool UsedotnetRDF { get; set; } 

    public string EncryptedToken 
    {
      get
      {
        return _encryptedToken;
      }
      set
      {
        if (value == null)
        {
          _encryptedToken = String.Empty;
        }
        else
        {
          _encryptedToken = value;
        }
      }
    }

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

    public string InterfaceServerPath
    {
      get
      {
        return _interfaceServerPath;
      }
      set
      {
        if (value == String.Empty || value == null)
        {
          _interfaceServerPath = @"..\InterfaceService\";
        }
        else
        {
          _interfaceServerPath = value;
        }
      }
    }
  }

  
}
