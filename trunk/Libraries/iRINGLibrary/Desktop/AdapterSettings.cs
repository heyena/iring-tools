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
    private string _modelDTOPath = String.Empty;
    private string _idsPath = String.Empty;
    private string _dataLayerConfigPath = String.Empty;
    private string _transformPath = String.Empty;
    private string _mappingPath = String.Empty;
    private string _encryptedToken = String.Empty;
    private string _tripleStoreConnectionString = String.Empty;
    private string _interfaceServer = String.Empty;
    private string _projectListSource = string.Empty;

    public AdapterSettings(NameValueCollection AppSettings)
    {     
      this.XmlPath = AppSettings["XmlPath"];
      this.TripleStoreConnectionString = AppSettings["TripleStoreConnectionString"];
      this.ModelDTOPath = AppSettings["ModelDTOPath"];
      this.IdsPath = AppSettings["IDataServicePath"];
      this.InterfaceServer = AppSettings["InterfaceService"];
      this.TrimData = Convert.ToBoolean(AppSettings["TrimData"]);
      this.UseSemweb = Convert.ToBoolean(AppSettings["UseSemweb"]);
      this.EncryptedToken = AppSettings["TargetCredentialToken"];
      this.EncryptedProxyToken = AppSettings["ProxyCredentialToken"];
      this.ProxyHost = AppSettings["ProxyHost"];
      this.ProxyPort = AppSettings["ProxyPort"];
      this.TransformPath = AppSettings["TransformPath"];
      this.DataLayerConfigPath = AppSettings["DataLayerConfigPath"];
      this.BaseDirectoryPath = AppSettings["BaseDirectoryPath"];
      this.ProjectListSource = AppSettings["ProjectListSource"];

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

    public string ModelDTOPath
    {
      get
      {
        return _modelDTOPath;
      }
      set
      {
        if (value == String.Empty || value == null)
        {
          _modelDTOPath = @".\App_Code\ModelDTO.cs";
        }
        else
        {
          _modelDTOPath = value;
        }
      }
    }

    public string IdsPath
    {
      get
      {
        return _idsPath;
      }
      set
      {
        if (value == String.Empty || value == null)
        {
          _idsPath = @".\App_Code\IDataService.cs";
        }
        else
        {
          _idsPath = value;
        }
      }
    }

    public string InterfaceServer 
    {
      get
      {
        return _interfaceServer;
      }
      set
      {
        if (value == String.Empty || value == null)
        {
          _interfaceServer = @"http://localhost:2222/iring";
        }
        else
        {
          _interfaceServer = value;
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

    public string TransformPath
    {
      get
      {
        return _transformPath;
      }
      set
      {
        if (value == String.Empty || value == null)
        {
          _transformPath = @".\Transforms\";
        }
        else
        {
          _transformPath = value;
        }
      }
    }

    public string DataLayerConfigPath
    {
      get
      {
        return _dataLayerConfigPath;
      }
      set
      {
        if (value == String.Empty || value == null)
        {
          _dataLayerConfigPath = @".\App_Data\Model.csdl";
        }
        else
        {
          _dataLayerConfigPath = value;
        }
      }
    }

      public string ProjectListSource
      {
          get
          {
              return _projectListSource;
          }
          set
          {
              if (value==String.Empty || value==null)
              {
                  _projectListSource = "Project.xml";
              }
              else
              {
                  _projectListSource = value;
              }
          }
      }
  }

  
}
