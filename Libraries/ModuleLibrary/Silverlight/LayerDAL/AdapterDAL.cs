using System.Linq;
using org.iringtools.library.presentation.configuration;
using org.iringtools.modulelibrary.baseclass;
using org.iringtools.modulelibrary.events;
using System.Collections.Generic;
using System.Net;
using org.iringtools.modulelibrary.types;
using org.iringtools.modulelibrary.extensions;
using System;
using System.Text;
using org.iringtools.utility;
using org.ids_adi.qxf;
using System.IO;
using System.ComponentModel;
using org.iringtools.library;
using org.iringtools.mapping;

namespace org.iringtools.modulelibrary.layerdal
{
  /// <summary>
  /// DATA ACCESS LAYER FOR ModuleLibrary.Desktop: Service References\AdapterServiceProxy
  /// 
  /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
  /// NO BUSINESS LOGIC SHOULD BE IN THIS CLASS - USE AdapterBLL for
  ///  + Preprocessing
  ///  + Postprocessing
  ///  + Validation
  ///  + Special error handling
  /// 
  /// This way if we ever have to replace the Data Layer we only have to
  /// implement the interface without breaking any business logic
  /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
  /// 
  /// </summary>
  public class AdapterDAL : DALBase, IAdapter
  {
    /// <summary>
    /// Occurs when [on data arrived].
    /// </summary>
    public event EventHandler<EventArgs> OnDataArrived;
    //public event EventHandler<EventArgs> OnError;

    /// <summary>
    /// Adapter WCF Service
    /// </summary>
    private WebClient _scopeListClient;
    private WebClient _dictionaryClient;
    private WebClient _generateClient;
    private WebClient _mappingClient;
    private WebClient _refreshClient;
    private WebClient _testClient;

    private string _adapterServiceUri;
    private string _graphBaseUri;
    private string _facadeServiceUri;


    #region Configure WebClient (client)
    /// <summary>
    /// Initializes a new instance of the <see cref="AdapterServiceDAL"/> class.
    /// </summary>
    /// <param name="config">The config.</param>
    public AdapterDAL(IServerConfiguration config)
      : base(config)
    {
      try
      {

        _adapterServiceUri = config.AdapterServiceUri;
        _graphBaseUri = config.BaseAddress;
        _facadeServiceUri = config.FacadeServiceUri;

        // Instantiate Adapter Service using baseclass 
        // properties
        _scopeListClient = new WebClient();
        _dictionaryClient = new WebClient();
        _generateClient = new WebClient();
        _mappingClient = new WebClient();
        _refreshClient = new WebClient();
        _testClient = new WebClient();

        #region Subscribe to client events
        // Async processing - specify event handlers
        _scopeListClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
        _dictionaryClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
        _generateClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
        _mappingClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
        _mappingClient.UploadStringCompleted += new UploadStringCompletedEventHandler(OnCompletedEvent);
        _refreshClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
        _testClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
        #endregion

      }
      catch (Exception ex)
      {
        Error.Exception = ex;
      }
    }

    #endregion

    //:::::[  ASYNC HANDLER ]:::::::::::::::::::::::::::::::::::::::::::

    #region OnCompletedEvent(object sender, AsyncCompletedEventArgs e)
    /// <summary>
    /// Handles the GetUnitTestStringCompleted - all events arrive here (single entry point)  
    /// with e representing the result type being returned
    /// 
    /// note: OndataArrive is Bubbled to subscribers with populated CompletedEventArgs data
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ModuleLibrary.AdapterProxy.GetUnitTestStringCompletedEventArgs"/> instance containing the event data.</param>
    void OnCompletedEvent(object sender, AsyncCompletedEventArgs e)
    {
      if (OnDataArrived == null)
        return;

      // Our event argument
      CompletedEventArgs args = null;

      //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
      #region // Your Handler HERE (template to copy/paste)
      // <Method> data arrived event handler 
      if (sender == _testClient)
      {
        // Configure event argument
        args = new CompletedEventArgs
        {
          // Define your method in CompletedEventType and assign
          CompletedType = CompletedEventType.NotDefined,
          Data = "Assign the expected result here"
        };
      }
      #endregion
      #region // Get ScopeList
      // <Method> data arrived event handler 
      if (sender == _scopeListClient)
      {
        try
        {
          string result = ((DownloadStringCompletedEventArgs)e).Result;

          ScopeProjects scopes = result.DeserializeDataContract<ScopeProjects>();

          // If the cast failed then return
          if (scopes == null)
            return;

          // Configure event argument
          args = new CompletedEventArgs
          {
            // Define your method in CompletedEventType and assign
            CompletedType = CompletedEventType.GetScopes,
            Data = scopes,
          };
        }
        catch (Exception ex)
        {
          string s = "Error Getting Scope List from Adapter Service.";
          // Configure event argument
          args = new CompletedEventArgs
          {
            // Define your method in CompletedEventType and assign
            CompletedType = CompletedEventType.GetScopes,
            Error = ex,
            FriendlyErrorMessage =
              IsServiceUnavailable(ex) ?
              s + "\nPlease verify if the Adapter Service is available" :
              s + "\nPlease review the log on the server.",
          };
        }
      }
      #endregion
      //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
      #region       // GetDataDictionary data arrived event handler

      // Only handle GetDataDictionaryCompletedEventArgs 
      if (sender == _dictionaryClient)
      {

        try
        {
          // Cast e (AsyncCompletedEventArgs) to actual type so we can
          // retrieve the Result - assign to dictionary
          string result = ((DownloadStringCompletedEventArgs)e).Result;

          DataDictionary dictionary = result.DeserializeDataContract<DataDictionary>();

          // If the cast failed then return
          if (dictionary == null)
            return;

          // Populate our event argument 
          args = new CompletedEventArgs
          {
            CompletedType = CompletedEventType.GetDataDictionary,
            Data = dictionary,
          };
        }
        catch (Exception ex)
        {
          string s = "Error Getting DataDictionary from Adapter Service.";
          // Configure event argument
          args = new CompletedEventArgs
          {
            // Define your method in CompletedEventType and assign
            CompletedType = CompletedEventType.GetDataDictionary,
            Error = ex,
            FriendlyErrorMessage =
              IsServiceUnavailable(ex) ?
              s + "\nPlease verify if the Adapter Service is available" :
              s + "\nPlease review the log on the server.",
          };
        }
      }
      #endregion

      //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
      #region      // GetMapping data arrived event handler

      // Only handle GetMappingCompletedEventArgs 
      if (sender == _mappingClient && CheckClassTypeFor<DownloadStringCompletedEventArgs>(e))
      {
        try
        {
          // Cast e (AsyncCompletedEventArgs) to actual type so we can
          // retrieve the Result - assign to mappingResult
          string result = ((DownloadStringCompletedEventArgs)e).Result;

          Mapping mapping = result.DeserializeDataContract<Mapping>();

          // If the cast failed then return
          if (mapping == null)
            return;

          // Populate event argument data object
          args = new CompletedEventArgs
          {
            CompletedType = CompletedEventType.GetMapping,
            Data = mapping,
          };
        }
        catch (Exception ex)
        {
          string s = "Error getting Mapping from Adapter Service.";
          // Configure event argument
          args = new CompletedEventArgs
          {
            // Define your method in CompletedEventType and assign
            CompletedType = CompletedEventType.GetMapping,
            Error = ex,
            FriendlyErrorMessage =
              IsServiceUnavailable(ex) ?
              s + "\nPlease verify if the Adapter Service is available" :
              s + "\nPlease review the log on the server.",
          };
        }
      }
      #endregion

      //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
      #region // RefreshAll data arrived event handler
      // <Method> data arrived event handler 
      if (sender == _refreshClient)
      {
        try
        {
          string result = ((DownloadStringCompletedEventArgs)e).Result;
          Response response = result.DeserializeDataContract<Response>();

          // Configure event argument
          args = new CompletedEventArgs
          {
            // Define your method in CompletedEventType and assign
            CompletedType = CompletedEventType.RefreshAll,
            Data = response
          };
        }
        catch (Exception ex)
        {
          string s = "Adapter Service returned an error while refreshing the facade.";
          // Configure event argument
          args = new CompletedEventArgs
          {
            // Define your method in CompletedEventType and assign
            CompletedType = CompletedEventType.RefreshAll,
            Error = ex,
            FriendlyErrorMessage =
              IsServiceUnavailable(ex) ?
              s + "\nPlease verify if the Adapter Service is available" :
              s + "\nPlease review the log on the server.",
          };
        }
      }
      #endregion

      //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
      #region // Generate data arrived event handler
      // <Method> data arrived event handler 
      if (sender == _generateClient)
      {
        try
        {
          string result = ((DownloadStringCompletedEventArgs)e).Result;
          Response response = result.DeserializeDataContract<Response>();

          // Configure event argument
          args = new CompletedEventArgs
          {
            // Define your method in CompletedEventType and assign
            CompletedType = CompletedEventType.Generate,
            Data = response
          };
        }
        catch (Exception ex)
        {
          string s = "Adapter Service returned an error while generating DTO.";
          // Configure event argument
          args = new CompletedEventArgs
          {
            // Define your method in CompletedEventType and assign
            CompletedType = CompletedEventType.Generate,
            Error = ex,
            FriendlyErrorMessage =
              IsServiceUnavailable(ex) ?
              s + "\nPlease verify if the Adapter Service is available" :
              s + "\nPlease review the log on the server.",
          };
        }
      }
      #endregion

      //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
      #region // UpdateMapping data arrived event handler
      // <Method> data arrived event handler 
      if (sender == _mappingClient && CheckClassTypeFor<UploadStringCompletedEventArgs>(e))
      {
        try
        {
          var result = ((UploadStringCompletedEventArgs)e).Result;
          Response response = result.DeserializeDataContract<Response>();

          // Configure event argument
          args = new CompletedEventArgs
          {
            // Define your method in CompletedEventType and assign
            CompletedType = CompletedEventType.UpdateMapping,
            Data = response.ToString()
          };
        }
        catch (Exception ex)
        {
          string s = "Adapter Service returned an error while updating the mapping.";
          // Configure event argument
          args = new CompletedEventArgs
          {
            // Define your method in CompletedEventType and assign
            CompletedType = CompletedEventType.UpdateMapping,
            Error = ex,
            FriendlyErrorMessage =
              IsServiceUnavailable(ex) ?
              s + "\nPlease verify if the Adapter Service is available" :
              s + "\nPlease review the log on the server.",
          };
        }
      }
      #endregion

      //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
      // Raise the data event
      if (args != null)
        OnDataArrived(sender, args);
    }
    #endregion

    //:::::[  ASYNC METHOD CALLS ]::::::::::::::::::::::::::::::::::::::

    // Note: IAdapterService interface is used by the actual service
    //       (as well as this proxy); the servie will return an actual
    //       result where we are making an Async call here so no values
    //       will be returned (we'll default to null so we can compile)

    public Mapping GetMapping(string projectName, string applicationName)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(_adapterServiceUri);
      sb.Append("/");
      sb.Append(projectName);
      sb.Append("/");
      sb.Append(applicationName);
      sb.Append("/mapping");
      _mappingClient.DownloadStringAsync(new Uri(sb.ToString()));
      return null;
    }

    public DataDictionary GetDictionary(string projectName, string applicationName)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(_adapterServiceUri);
      sb.Append("/");
      sb.Append(projectName);
      sb.Append("/");
      sb.Append(applicationName);
      sb.Append("/dictionary");
      _dictionaryClient.DownloadStringAsync(new Uri(sb.ToString()));
      return null;
    }

    public Response UpdateMapping(string projectName, string applicationName, Mapping mapping)
    {
      string message = Utility.SerializeDataContract<Mapping>(mapping);

      StringBuilder sb = new StringBuilder();
      sb.Append(_adapterServiceUri);
      sb.Append("/");
      sb.Append(projectName);
      sb.Append("/");
      sb.Append(applicationName);
      sb.Append("/mapping");

      _mappingClient.Headers["Content-type"] = "application/xml";
      _mappingClient.Encoding = Encoding.UTF8;
      _mappingClient.UploadStringAsync(new Uri(sb.ToString()), "POST", message);

      return null;
    }

    public Response GetScopes()
    {
      _scopeListClient.DownloadStringAsync(new Uri(_adapterServiceUri + "/scopes"));
      return null;
    }

    public Response Generate(string projectName, string applicationName)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(_adapterServiceUri);
      sb.Append("/");
      sb.Append(projectName);
      sb.Append("/");
      sb.Append(applicationName);
      sb.Append("/generate");
      _generateClient.DownloadStringAsync(new Uri(sb.ToString()));
      return null;
    }

    public Response RefreshAll(string projectName, string applicationName)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(_facadeServiceUri);
      sb.Append("/");
      sb.Append(projectName);
      sb.Append("/");
      sb.Append(applicationName);
      sb.Append("/refresh");
      _refreshClient.DownloadStringAsync(new Uri(sb.ToString()));
      return null;
    }

    public Response RefreshAll(object userState)
    {
      throw new NotImplementedException();
    }

    public Response Generate(object userState)
    {
      throw new NotImplementedException();
    }

    public Response UpdateMapping(Mapping mapping)
    {
      throw new NotImplementedException();
    }

    public void GetUnitTestString(string valueToReturn)
    {
      throw new NotImplementedException();
    }

    #region IAdapterService Members

    public Response RefreshGraph(string projectName, string applicationName, string graphName)
    {
      throw new NotImplementedException();
    }

    public QXF Get(string graphName, string identifier)
    {
      throw new NotImplementedException();
    }

    public QXF GetList(string graphName)
    {
      throw new NotImplementedException();
    }

    public Response Pull(Request request)
    {
      throw new NotImplementedException();
    }

    public Response ClearStore(string projectName, string applicationName)
    {
      throw new NotImplementedException();
    }

    #endregion

    public string GetAdapterServiceUri { get { return _adapterServiceUri; } }

    public string GetBaseAddress { get { return _graphBaseUri; } }
  }
}
