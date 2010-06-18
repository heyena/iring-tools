using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.utility;

namespace org.iringtools.library
{
  public class ServiceSettings
  {
    private string _xmlPath = String.Empty;
    private string _encryptedProxyToken = String.Empty;
    private string _proxyHost = String.Empty;
    private string _proxyPort = String.Empty;
    private string _baseDirectoryPath = String.Empty;
    private string _executingAssemblyName = String.Empty;

    public string BaseDirectoryPath 
    {
      get
      {
        return _baseDirectoryPath;
      }
      set
      {
        if (value == String.Empty || value == null)
        {
          _baseDirectoryPath = System.AppDomain.CurrentDomain.BaseDirectory;
        }
        else
        {
          _baseDirectoryPath = value;
        }
      }
    }

    public WebProxyCredentials ProxyCredentials { get; set; }

    public virtual void PrepareCredentials()
    {
      if (EncryptedProxyToken == String.Empty)
      {
        this.ProxyCredentials = new WebProxyCredentials();
      }
      else
      {
        int portNumber = 80;
        Int32.TryParse(this.ProxyPort, out portNumber);

        this.ProxyCredentials = new WebProxyCredentials(
          this.EncryptedProxyToken,
          this.ProxyHost, 
          portNumber);
        this.ProxyCredentials.Decrypt();
      }
    }

    public string ExecutingAssemblyName {
      get
      {
        return _executingAssemblyName;
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          _executingAssemblyName = "App_Code";
        else
          _executingAssemblyName = value;
      }
    }

    public string XmlPath
    {
      get
      {
        return _xmlPath;
      }
      set
      {
        if (value == String.Empty || value == null)
        {
          _xmlPath = @".\XML\";
        }
        else
        {
          _xmlPath = value;
        }
      }
    }

    public string EncryptedProxyToken
    {
      get
      {
        return _encryptedProxyToken;
      }
      set
      {
        if (value == null)
        {
          _encryptedProxyToken = String.Empty;
        }
        else
        {
          _encryptedProxyToken = value;
        }
      }
    }

    public string ProxyHost
    {
      get
      {
        return _proxyHost;
      }
      set
      {
        if (value == null)
        {
          _proxyHost = String.Empty;
        }
        else
        {
          _proxyHost = value;
        }
      }
    }

    public string ProxyPort
    {
      get
      {
        return _proxyPort;
      }
      set
      {
        if (value == null)
        {
          _proxyPort = String.Empty;
        }
        else
        {
          _proxyPort = value;
        }
      }
    }
  }
  
}
