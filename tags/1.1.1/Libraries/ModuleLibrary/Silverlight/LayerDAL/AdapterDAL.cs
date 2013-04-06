﻿using System.Linq;
using Library.Interface.Configuration;
using ModuleLibrary.Base;
using ModuleLibrary.Events; 
using System.Collections.Generic;
using System.Net;
using ModuleLibrary.Types;
using ModuleLibrary.Extensions;
using OntologyService.Interface.Entities;
using System;
using System.Text;
using org.ids_adi.iring;
using org.iringtools.utility;
using org.ids_adi.qxf;
using System.IO;
using System.ComponentModel;
using org.iringtools.library;

namespace ModuleLibrary.LayerDAL
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

    /// <summary>
    /// Adapter WCF Service
    /// </summary>
    private WebClient _dictionaryClient;
    private WebClient _generateClient;
    private WebClient _mappingClient;
    private WebClient _refreshClient;
    private WebClient _testClient;

    private string _adapterServiceUri;
    

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
       
        // Instantiate Adapter Service using baseclass 
        // properties
          _dictionaryClient = new WebClient();
          _generateClient = new WebClient();
          _mappingClient = new WebClient();
          _refreshClient = new WebClient();
          _testClient = new WebClient();

        #region Subscribe to client events
        // Async processing - specify event handlers
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

      //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
      #region       // GetDataDictionary data arrived event handler

      // Only handle GetDataDictionaryCompletedEventArgs 
      if (sender == _dictionaryClient)
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
      #endregion

      //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
      #region      // GetMapping data arrived event handler

      // Only handle GetMappingCompletedEventArgs 
      if (sender == _mappingClient && CheckClassTypeFor<DownloadStringCompletedEventArgs>(e))
      {
        // Cast e (AsyncCompletedEventArgs) to actual type so we can
        // retrieve the Result - assign to mappingResult
        string result = ((DownloadStringCompletedEventArgs)e).Result;

        Mapping mapping = result.DeserializeXml<Mapping>();

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
      #endregion

      //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
      #region // RefreshAll data arrived event handler
      // <Method> data arrived event handler 
      if (sender == _refreshClient)
      {
        string result = ((UploadStringCompletedEventArgs)e).Result;

        Response response = result.DeserializeXml<Response>();

        // Configure event argument
        args = new CompletedEventArgs
        {
          // Define your method in CompletedEventType and assign
          CompletedType = CompletedEventType.RefreshAll,
          Data = response.FirstOrDefault<string>()
        };
      }
      #endregion

      //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
      #region // Generate data arrived event handler 
      // <Method> data arrived event handler 
      if (sender == _generateClient)
      {

        var result = ((UploadStringCompletedEventArgs)e).Result;

        Response response = result.DeserializeXml<Response>();

        // Configure event argument
        args = new CompletedEventArgs
        {
          // Define your method in CompletedEventType and assign
          CompletedType = CompletedEventType.Generate,
          Data = response.FirstOrDefault<string>()

        };
      }
      #endregion

      //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
      #region // UpdateMapping data arrived event handler
      // <Method> data arrived event handler 
      if (sender == _mappingClient && CheckClassTypeFor<UploadStringCompletedEventArgs>(e))
      {
      var result = ((UploadStringCompletedEventArgs)e).Result;

      Response response = result.DeserializeXml<Response>();

      // Configure event argument
      args = new CompletedEventArgs
      {
        // Define your method in CompletedEventType and assign
        CompletedType = CompletedEventType.UpdateMapping,
        Data = response.FirstOrDefault<string>()
      };
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

    #region Generate( overloaded ) 
    /// <summary>
    /// Generates this instance.
    /// </summary>
    /// <returns></returns>
    public Response Generate()
    {
        _generateClient.DownloadStringAsync(new Uri(_adapterServiceUri + "/generate"));
        return null;
    }


    /// <summary>
    /// Generates this instance.
    /// </summary>
    /// <returns></returns>
    //public Response GenerateEDMX(DatabaseDictionary databaseDictionary)
    //{
    //  _generateClient.DownloadStringAsync(new Uri(_adapterServiceUri + "/dictionary/generate"));
    //  return null;
    //}

    /// <summary>
    /// Generates the specified user state.
    /// </summary>
    /// <param name="userState">State of the user.</param>
    /// <returns></returns>
    public Response Generate(object userState)
    {
      _generateClient.DownloadStringAsync(new Uri(_adapterServiceUri + "/generate"), userState);
      return null;
    }  
    #endregion
    
    #region GetMapping() 
    public Mapping GetMapping()
    {
      _mappingClient.DownloadStringAsync(new Uri(_adapterServiceUri + "/mapping"));
      return null;
    } 
    #endregion
    #region GetDictionary() 
    public DataDictionary GetDictionary()
    {
      _dictionaryClient.DownloadStringAsync(new Uri(_adapterServiceUri + "/dictionary"));
      return null;
    } 
    #endregion
    #region UpdateMapping(mapping) 
    public Response UpdateMapping(Mapping mapping)
    {
      string message = Utility.SerializeXml<Mapping>(mapping);

      _mappingClient.Headers["Content-type"] = "application/xml";
      _mappingClient.Encoding = Encoding.UTF8;
      _mappingClient.UploadStringAsync(new Uri(_adapterServiceUri + "/mapping"), "POST", message);
      
      return null;
    }

    #endregion
    #region RefreshAll( overloaded ) 
    /// <summary>
    /// Refreshes all.
    /// </summary>
    /// <returns></returns>
    public Response RefreshAll()
    {
      _refreshClient.DownloadStringAsync(new Uri(_adapterServiceUri + "/refresh"));
      
      return null;
    }

    /// <summary>
    /// Refreshes all.
    /// </summary>
    /// <param name="userState">State of the user.</param>
    /// <returns></returns>
    public Response RefreshAll(object userState)
    {
      _refreshClient.DownloadStringAsync(new Uri(_adapterServiceUri + "/refresh"), userState);
      
      return null;
    } 
    #endregion

    public void GetUnitTestString(string valueToReturn)
    {
      throw new NotImplementedException();
    }

    #region IAdapterService Members

    public Response RefreshDictionary()
    {
      throw new NotImplementedException();
    }

    public Response RefreshGraph(string graphName)
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

    public Response ClearStore()
    {
      throw new NotImplementedException();
    }

    #endregion

    public string GetAdapterServiceUri { get { return _adapterServiceUri; } }
  }
}