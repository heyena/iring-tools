using System;
using System.Net;
using org.iringtools.library;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using org.iringtools.modulelibrary.events;
using org.iringtools.utility;
using System.Collections.ObjectModel;

namespace ApplicationEditor
{
  public class ApplicationDAL
  {
    public event EventHandler<EventArgs> OnDataArrived;
    private string _applicationServiceUri;
    private string _adapterServiceUri;


    public ApplicationDAL()
    {
      _applicationServiceUri = App.Current.Resources["NHibernateServiceURI"].ToString();
      _adapterServiceUri = App.Current.Resources["AdapterServiceUri"].ToString();
    }

    public DatabaseDictionary GetDbDictionary(string projectName, string applicationName)
    {
      string relativeUri = String.Format("/{0}/{1}/dictionary",
         projectName,
         applicationName
       );

      Uri address = new Uri(_applicationServiceUri + relativeUri);

      WebClient webClient = new WebClient();      
      webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnGetDbDictionaryCompletedEvent);
      webClient.DownloadStringAsync(address);

      return null;
    }

    public void SaveDatabaseDictionary(DatabaseDictionary databaseDictionary, string projectName, string applicationName)
    {
      string relativeUri = String.Format("/{0}/{1}/dictionary",
        projectName,
        applicationName
      );

      Uri address = new Uri(_applicationServiceUri + relativeUri);
      string data = Utility.SerializeDataContract<DatabaseDictionary>(databaseDictionary);

      WebClient webClient = new WebClient();
      webClient.UploadStringCompleted += new UploadStringCompletedEventHandler(OnSaveDbDictionaryCompletedEvent);
      webClient.Headers["Content-type"] = "application/xml";
      webClient.Encoding = Encoding.UTF8;
      webClient.UploadStringAsync(address, "POST", data);
    }

    public void GetDatabaseSchema(string projectName, string applicationName)
    {
      string relativeUri = String.Format("/{0}/{1}/schema",
         projectName,
         applicationName
       );

      Uri address = new Uri(_applicationServiceUri + relativeUri);

      WebClient webClient = new WebClient();
      webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnGetDbSchemaCompletedEvent);
      webClient.DownloadStringAsync(address);
    }

    public void GetSchemaObjects(string projectName, string applicationName)
    {
      string relativeUri = String.Format("/{0}/{1}/objects",
        projectName,
        applicationName
        );
      Uri address = new Uri(_applicationServiceUri + relativeUri);

      WebClient webClient = new WebClient();
      webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnGetSchemaObjectsCompletedEvent);
      webClient.DownloadStringAsync(address);
    }

    public void GetSchemaObjectsSchma(string projectName, string applicationName, string objectName)
    {
        string relativeUri = String.Format("/{0}/{1}/objects/{2}",
        projectName,
        applicationName,
        objectName
        );
      Uri address = new Uri(_applicationServiceUri + relativeUri);

      WebClient webClient = new WebClient();
      webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnGetSchemaObjectSchemaCompletedEvent);
      webClient.DownloadStringAsync(address);

    }
    
    public void GetProviders()
    {
      string relativeUri = "/providers";
      Uri address = new Uri(_applicationServiceUri + relativeUri);

      WebClient webClient = new WebClient();
      webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnGetProvidersCompletedEvent);
      webClient.DownloadStringAsync(address);
    }

    public void GetRelationshipTypes()
    {
      string relativeUri = "/relationships";
      Uri address = new Uri(_applicationServiceUri + relativeUri);

      WebClient webClient = new WebClient();
      webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnGetRelationshipCompletedEvent);
      webClient.DownloadStringAsync(address);
    }

    public void GetScopes()
    {
      string relativeUri = "/scopes";
      Uri address = new Uri(_adapterServiceUri + relativeUri);

      WebClient webClient = new WebClient();
      webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnGetScopesCompletedEvent);
      webClient.DownloadStringAsync(address);
    }

    public void PostDictionaryToApplicationService(string projectName, string applicationName)
    {
      string relativeUri = String.Format("/{0}/{1}/generate",
        projectName,
        applicationName
      );

      Uri address = new Uri(_applicationServiceUri + relativeUri);

      WebClient webClient = new WebClient();
      webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnPostDbDictionaryCompletedEvent);
      webClient.DownloadStringAsync(address);

    }

    public void UpdateScopes(ScopeProjects scopes)
    {
      string relativeUri = String.Format("/scopes");
      Uri address = new Uri(_adapterServiceUri + relativeUri);
      string data = Utility.SerializeDataContract<ScopeProjects>(scopes);

      WebClient webClient = new WebClient();
      webClient.UploadStringCompleted += new UploadStringCompletedEventHandler(OnUpdateScopesCompletedEvent);
      webClient.Headers["Content-type"] = "application/xml";
      webClient.Encoding = Encoding.UTF8;
      webClient.UploadStringAsync(address, "POST", data);
    }

    void OnGetScopesCompletedEvent(object sender, AsyncCompletedEventArgs e)
    {
      string result = ((DownloadStringCompletedEventArgs)e).Result;
      ScopeProjects scopes = result.DeserializeDataContract<ScopeProjects>();

      // If the cast failed then return
      if (scopes == null)
        return;

      CompletedEventArgs args = new CompletedEventArgs
      {
        CompletedType = CompletedEventType.GetScopes,
        Data = scopes,
      };

      OnDataArrived(sender, args);
    }

    void OnUpdateScopesCompletedEvent(object sender, AsyncCompletedEventArgs e)
    {
      CompletedEventArgs args;

      try
      {
        string result = ((UploadStringCompletedEventArgs)e).Result;
        Response response = result.DeserializeDataContract<Response>();
        
        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.PostScopes,
          Data = response,
        };
      }
      catch (Exception ex)
      {
        string s = "Error while posting Scopes to the AdapterService.";

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.PostScopes,
          Error = ex,
          FriendlyErrorMessage =
              ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
              s + "\nPlease verify if the Application Service is available" :
              s + "\nPlease review the log on the server.",
        };
      }

      OnDataArrived(sender, args);
    }

    void OnGetDbDictionaryCompletedEvent(object sender, AsyncCompletedEventArgs e)
    {
      CompletedEventArgs args;
      DatabaseDictionary dbDictionary = null;
      try
      {
        string result = ((DownloadStringCompletedEventArgs)e).Result;
        if (result != string.Empty)
        {
          dbDictionary = result.DeserializeDataContract<DatabaseDictionary>();
        }

        // If the cast failed then return
        if (dbDictionary == null)
          return;

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.GetDbDictionary,
          Data = dbDictionary,
        };
      }
      catch (Exception ex)
      {
        string s = "Error Getting Database Dictionary from ApplicationService.";

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.GetDbDictionary,
          Error = ex,
          FriendlyErrorMessage =
              ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
              s + "\nPlease verify if the Application Service is available" :
              s + "\nPlease review the log on the server.",
        };
      }

      OnDataArrived(sender, args);
    }

    void OnGetDbSchemaCompletedEvent(object sender, AsyncCompletedEventArgs e)
    {
      //CompletedEventArgs args;

      //try
      //{
      //  string result = ((DownloadStringCompletedEventArgs)e).Result;
      //  DatabaseDictionary dbSchema = result.DeserializeDataContract<DatabaseDictionary>();

      //  // If the cast failed then return
      //  if (dbSchema == null)
      //    return;

      //  args = new CompletedEventArgs
      //  {
      //    CompletedType = CompletedEventType.GetDatabaseSchema,
      //    Data = dbSchema,
      //  };
      //}
      //catch (Exception ex)
      //{
      //  string s = "Error Getting Database Schema from ApplicationService.";

      //  args = new CompletedEventArgs
      //  {
      //    CompletedType = CompletedEventType.GetDatabaseSchema,
      //    Error = ex,
      //    FriendlyErrorMessage =
      //        ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
      //        s + "\nPlease verify if the Application Service is available" :
      //        s + "\nPlease review the log on the server.",
      //  };
      //}

      //OnDataArrived(sender, args);
    }

    void OnGetSchemaObjectSchemaCompletedEvent(object sender, DownloadStringCompletedEventArgs e)
    {
      CompletedEventArgs args;
      try
      {
        string result = ((DownloadStringCompletedEventArgs)e).Result;
        org.iringtools.library.DataObject dataObject = result.DeserializeDataContract<org.iringtools.library.DataObject>();

        if (dataObject == null) return;

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.GetSchemaObjectsSchema,
          Data = dataObject
        };

      }
      catch (Exception ex)
      {
        string s = "Error getting data object from data source";
        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.GetDatabaseSchema,
          Error = ex,
          FriendlyErrorMessage =
              ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
              s + "\nPlease verify if the Application Service is available" :
              s + "\nPlease review the log on the server.",
        };
      }
      OnDataArrived(sender, args);
    }

    void OnSaveDbDictionaryCompletedEvent(object sender, AsyncCompletedEventArgs e)
    {
      CompletedEventArgs args;

      try
      {
        string result = ((UploadStringCompletedEventArgs)e).Result;
        Response response = result.DeserializeDataContract<Response>();

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.SaveDatabaseDictionary,
          Data = response,
        };
      }
      catch (Exception ex)
      {
        string s = "Error while saving Database Dictionary through ApplicationService.";

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.SaveDatabaseDictionary,
          Error = ex,
          FriendlyErrorMessage =
              ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
              s + "\nPlease verify if the Application Service is available" :
              s + "\nPlease review the log on the server.",
        };
      }

      OnDataArrived(sender, args);
    }

    void OnGetProvidersCompletedEvent(object sender, AsyncCompletedEventArgs e)
    {
      CompletedEventArgs args;

      try
      {
        string result = ((DownloadStringCompletedEventArgs)e).Result;
        DataProviders providers = result.DeserializeDataContract<DataProviders>();

        // If the cast failed then return
        if (providers == null)
          return;

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.GetProviders,
          Data = providers,
        };
      }
      catch (Exception ex)
      {
        string s = "Error Getting Provider Names from ApplicationService.";

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.GetProviders,
          Error = ex,
          FriendlyErrorMessage =
              ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
              s + "\nPlease verify if the Application Service is available" :
              s + "\nPlease review the log on the server.",
        };
      }

      OnDataArrived(sender, args);
    }

    void OnGetRelationshipCompletedEvent(object sender, AsyncCompletedEventArgs e)
    {
      CompletedEventArgs args;

      try
      {
        string result = ((DownloadStringCompletedEventArgs)e).Result;
        DataRelationships relationships = result.DeserializeDataContract<DataRelationships>();

        if (relationships == null)
          return;

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.GetRelationships,
          Data = relationships,
        };
      }
      catch (Exception ex)
      {
        string s = "Error Getting relationships Names from Application Service.";

        args = new CompletedEventArgs
        {
          // Define your method in CompletedEventType and assign
          CompletedType = CompletedEventType.GetRelationships,
          Error = ex,
          FriendlyErrorMessage =
              ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
              s + "\nPlease verify if the Application Service is available" :
              s + "\nPlease review the log on the server.",
        };
      }

      OnDataArrived(sender, args);
    }

    void OnPostDbDictionaryCompletedEvent(object sender, AsyncCompletedEventArgs e)
    {
      CompletedEventArgs args;

      try
      {
        string result = ((DownloadStringCompletedEventArgs)e).Result;
        Response response = result.DeserializeDataContract<Response>();

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.PostDictionaryToAdapterService,
          Data = response,
        };
      }
      catch (Exception ex)
      {
        string s = "Error while posting Database Dictionary to the AdapterService.";

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.PostDictionaryToAdapterService,
          Error = ex,
          FriendlyErrorMessage =
              ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
              s + "\nPlease verify if the Application Service is available" :
              s + "\nPlease review the log on the server.",
        };
      }

      OnDataArrived(sender, args);
    }

    //void OnClearTripleStoreCompletedEvent(object sender, AsyncCompletedEventArgs e)
    //{
    //  CompletedEventArgs args;

    //  try
    //  {
    //    string result = ((DownloadStringCompletedEventArgs)e).Result;
    //    Response response = result.DeserializeDataContract<Response>();

    //    args = new CompletedEventArgs
    //    {
    //      CompletedType = CompletedEventType.ClearTripleStore,
    //      Data = response,
    //    };
    //  }
    //  catch (Exception ex)
    //  {
    //    string s = "Error while clearing triple store through the AdapterService.";

    //    args = new CompletedEventArgs
    //    {
    //      // Define your method in CompletedEventType and assign
    //      CompletedType = CompletedEventType.ClearTripleStore,
    //      Error = ex,
    //      FriendlyErrorMessage =
    //          ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
    //          s + "\nPlease verify if the Application Service is available" :
    //          s + "\nPlease review the log on the server.",
    //    };
    //  }

    //  OnDataArrived(sender, args);
    //}

    void OnDeleteAppCompletedEvent(object sender, AsyncCompletedEventArgs e)
    {
      CompletedEventArgs args;

      try
      {
        string result = ((DownloadStringCompletedEventArgs)e).Result;
        Response response = result.DeserializeDataContract<Response>();

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.DeleteApp,
          Data = response,
        };
      }
      catch (Exception ex)
      {
        string s = "Error while deleting the app from AdapterService.";

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.DeleteApp,
          Error = ex,
          FriendlyErrorMessage =
              ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
              s + "\nPlease verify if the Application Service is available" :
              s + "\nPlease review the log on the server.",
        };
      }

      OnDataArrived(sender, args);
    }

    void OnGetSchemaObjectsCompletedEvent(object sender, AsyncCompletedEventArgs e)
    {
      CompletedEventArgs args;
      DataObjects schemaObjects = null;
      try
      {
        string result = ((DownloadStringCompletedEventArgs)e).Result;
        if (result != string.Empty)
        {
          schemaObjects = result.DeserializeDataContract<DataObjects>();
        }
        else 
        {
          return;
        }
        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.GetSchemaObjects,
          Data = schemaObjects,
        };
      }
      catch (Exception ex)
      {
        string s = "Error while getting schema objects.";

        args = new CompletedEventArgs
        {
          CompletedType = CompletedEventType.GetSchemaObjects,
          Error = ex,
          FriendlyErrorMessage =
              ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
              s + "\nPlease verify if the Application Service is available" :
              s + "\nPlease review the log on the server.",
        };
      }
      OnDataArrived(sender, args);
    }
  }
}
