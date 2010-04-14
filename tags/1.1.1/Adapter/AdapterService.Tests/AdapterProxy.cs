using System;
using org.iringtools.adapter;
using org.iringtools.utility;
using PrismContrib.Errors;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.iringtools.library;

namespace AdapterService.Tests
{
  [TestClass]
  public class AdapterProxy
  {
    private AdapterServiceProvider _adapterServiceProvider = null;   

    /// <summary>
    /// Gets or sets the error.
    /// </summary>
    /// <value>The error.</value>
    [Dependency]
    public IError Error { get; set; }

    [Dependency]
    public ILoggerFacade Logger { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdapterProxyProvider"/> class.
    /// </summary>
    /// <param name="container">The container.</param>
    public AdapterProxy()
    {
      ConfigSettings configSettings = new ConfigSettings();
      configSettings.BaseDirectoryPath = System.Configuration.ConfigurationManager.AppSettings["BasePath"];
      configSettings.XmlPath = System.Configuration.ConfigurationManager.AppSettings["XmlPath"];
      configSettings.TripleStoreConnectionString = System.Configuration.ConfigurationManager.AppSettings["TripleStoreConnectionString"];
      configSettings.ModelDTOPath = System.Configuration.ConfigurationManager.AppSettings["ModelDTOPath"];
      configSettings.IDataServicePath = System.Configuration.ConfigurationManager.AppSettings["IDataServicePath"];
      configSettings.InterfaceServer = System.Configuration.ConfigurationManager.AppSettings["InterfaceService"];
      configSettings.TrimData = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["TrimData"]);
      configSettings.UseSemweb = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["UseSemweb"]);
      configSettings.EncryptedToken = System.Configuration.ConfigurationManager.AppSettings["TargetCredentialToken"];
      configSettings.EncryptedProxyToken = System.Configuration.ConfigurationManager.AppSettings["ProxyCredentialToken"];
      configSettings.ProxyHost = System.Configuration.ConfigurationManager.AppSettings["ProxyHost"];
      configSettings.ProxyPort = System.Configuration.ConfigurationManager.AppSettings["ProxyPort"];
      configSettings.TransformPath = System.Configuration.ConfigurationManager.AppSettings["TransformPath"];
      configSettings.DataLayerConfigPath = System.Configuration.ConfigurationManager.AppSettings["DataLayerConfigPath"];
      _adapterServiceProvider = new AdapterServiceProvider(configSettings);
    }

    public Response Generate()
    {
      Response response = null;
      try
      {
        response = _adapterServiceProvider.Generate();
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }

    public Response RefreshDictionary()
    {
      Response response = null;
      try
      {
        response = _adapterServiceProvider.RefreshDictionary();
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }

    public Mapping GetMapping()
    {
      Mapping mapping = null;

      try
      {
        mapping = _adapterServiceProvider.GetMapping();
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return mapping;
    }

    public DataDictionary GetDictionary()
    {
      DataDictionary data = null;
      try
      {
        data = _adapterServiceProvider.GetDictionary();
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return data;
    }

    public Response UpdateMapping(Mapping mapping)
    {
      Response response = null;
      try
      {
        response = _adapterServiceProvider.UpdateMapping(mapping);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }

    public Envelope Get(string graphName, string identifier)
    {
      Envelope envelope = null;
      try
      {
        envelope = _adapterServiceProvider.Get(graphName, identifier);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return envelope;
    }

    public Envelope GetList(string graphName)
    {
      Envelope envelope = null;
      try
      {
        envelope = _adapterServiceProvider.GetList(graphName);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return envelope;
    }

    public Response ClearStore()
    {
      Response response = null;
      try
      {
        response = _adapterServiceProvider.ClearStore();
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }

    public Response RefreshGraph(string graphName)
    {
      Response response = null;
      try
      {
        response = _adapterServiceProvider.RefreshGraph(graphName);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }

    public Response RefreshAll()
    {
      Response response = null;
      try
      {
        response = _adapterServiceProvider.RefreshAll();
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return response;
    }   

    public Response Pull(Request request)
    {
       Response response = null;
       try
       {         
         response = _adapterServiceProvider.Pull(request);
       }
       catch (Exception ex)
       {
         Error.SetError(ex);
       }
       return response;
    }
  }
}
