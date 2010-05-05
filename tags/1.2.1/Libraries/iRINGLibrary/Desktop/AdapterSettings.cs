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

    public AdapterSettings(NameValueCollection AppSettings)
    {
      this.BaseDirectoryPath = AppSettings["BaseDirectoryPath"];
      this.XmlPath = AppSettings["XmlPath"];
      this.EncryptedProxyToken = AppSettings["ProxyCredentialToken"];
      this.ProxyHost = AppSettings["ProxyHost"];
      this.ProxyPort = AppSettings["ProxyPort"];
      this.UseSemweb = Convert.ToBoolean(AppSettings["UseSemweb"]);
      this.TripleStoreConnectionString = AppSettings["TripleStoreConnectionString"];
      this.InterfaceServer = AppSettings["InterfaceService"];
      this.InterfaceServerPath = AppSettings["InterfaceServicePath"];
      this.EncryptedToken = AppSettings["TargetCredentialToken"];
      this.TrimData = Convert.ToBoolean(AppSettings["TrimData"]);
      this.BinaryPath = AppSettings["BinaryPath"];
      this.CodePath = AppSettings["CodePath"];
      this.PrepareCredentials();
    }

    public WebCredentials TargetCredentials { get; set; }

    public Mapping Mapping { get; set; }

    public override void PrepareCredentials()
    {
      base.PrepareCredentials();

      if (EncryptedToken == String.Empty)
      {
        this.TargetCredentials = new WebCredentials();
      }
      else
      {
        this.TargetCredentials = new WebCredentials(EncryptedToken);
        this.TargetCredentials.Decrypt();
      }
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
