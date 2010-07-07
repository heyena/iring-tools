using System;
using System.Net;
using org.iringtools.library;
using System.Text;
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
        //public event EventHandler<EventArgs> OnError;

        private WebClient _scopesClient;
        private WebClient _dbDictionaryClient;
        private WebClient _dbschemaClient;
        private WebClient _savedbdictionaryClient;
        private WebClient _dbdictionariesClient;
        private WebClient _providersClient;
        private WebClient _postdbdictionaryClient;
        private WebClient _clearClient;
        private WebClient _deleteClient;
        private WebClient _testClient;

        private string _dbDictionaryServiceUri;
        private string _adapterServiceUri;

        public ApplicationDAL() 
        {
            #region Webclients
            _scopesClient = new WebClient();
            _dbDictionaryClient = new WebClient();
            _dbschemaClient = new WebClient();
            _savedbdictionaryClient = new WebClient();
            _dbdictionariesClient = new WebClient();
            _providersClient = new WebClient();
            _postdbdictionaryClient = new WebClient();
            _clearClient = new WebClient();
            _deleteClient = new WebClient();
            _testClient = new WebClient();
            #endregion

            _scopesClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _dbDictionaryClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _dbschemaClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _savedbdictionaryClient.UploadStringCompleted += new UploadStringCompletedEventHandler(OnCompletedEvent);
            _dbdictionariesClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _providersClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _clearClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _postdbdictionaryClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _deleteClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);

            _dbDictionaryServiceUri = App.Current.Resources["ApplicationServiceURI"].ToString();
            _adapterServiceUri = App.Current.Resources["AdapterServiceUri"].ToString();
        }

        public DatabaseDictionary GetDbDictionary(string projectName, string applicationName)
        {
            if (!_dbDictionaryClient.IsBusy)
            {
              string relativeUri = String.Format("/{0}/{1}/dictionary",
                projectName,
                applicationName
              );

              Uri address = new Uri(new Uri(_dbDictionaryServiceUri), relativeUri);

              _dbDictionaryClient.DownloadStringAsync(address);
            }
            return null;
        }

        public void GetScopes()
        {
            
            string relativeUri = String.Format("/scopes");

            Uri address = new Uri(new Uri(_adapterServiceUri), relativeUri);

            _scopesClient.DownloadStringAsync(address);
            
        }

        public void SaveDatabaseDictionary(DatabaseDictionary databaseDictionary, string projectName, string applicationName)
        {
          string relativeUri = String.Format("/{0}/{1}/dictionary",
            projectName,
            applicationName
          );

          Uri address = new Uri(new Uri(_dbDictionaryServiceUri), relativeUri);
          string data = Utility.SerializeDataContract<DatabaseDictionary>(databaseDictionary);
            
          _savedbdictionaryClient.Headers["Content-type"] = "application/xml";
          _savedbdictionaryClient.Encoding = Encoding.UTF8;
          _savedbdictionaryClient.UploadStringAsync(address, "POST", data);
        }

        public void GetDatabaseSchema(string projectName, string applicationName)
        {
            string relativeUri = String.Format("/{0}/{1}/schema",
               projectName,
               applicationName
             );

          Uri address = new Uri(new Uri(_dbDictionaryServiceUri), relativeUri);
          _dbschemaClient.DownloadStringAsync(address);
        }

        //public void GetExistingDbDictionaryFiles()
        //{
        //  string relativeUri = "/dbdictionaries";

        //  Uri address = new Uri(new Uri(_dbDictionaryServiceUri), relativeUri);

        //  _dbdictionariesClient.DownloadStringAsync(address);
        //}

        public void GetProviders()
        {
          string relativeUri = "/providers";

          Uri address = new Uri(new Uri(_dbDictionaryServiceUri), relativeUri);

          _providersClient.DownloadStringAsync(address);
        }

        public void PostDictionaryToAdapterService(string projectName, string applicationName)
        {
          string relativeUri = String.Format("/{0}/{1}/generate",
            projectName,
            applicationName
          );

          Uri address = new Uri(new Uri(_dbDictionaryServiceUri), relativeUri);

          _postdbdictionaryClient.DownloadStringAsync(address);
          //string data = Utility.SerializeDataContract<DatabaseDictionary>(databaseDictionary);

          //_postdbdictionaryClient.Headers["Content-type"] = "application/xml";
          //_postdbdictionaryClient.Encoding = Encoding.UTF8;
          //_postdbdictionaryClient.UploadStringAsync(address, "POST", data);
        }

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

            if (sender == _scopesClient)
            {
                string result = ((DownloadStringCompletedEventArgs)e).Result;

                Collection<ScopeProject> scopes = result.DeserializeDataContract<Collection<ScopeProject>>();

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

            if (sender == _dbDictionaryClient)
            {
                try
                {
                    string result = ((DownloadStringCompletedEventArgs)e).Result;

                    DatabaseDictionary dbDictionary = result.DeserializeDataContract<DatabaseDictionary>();

                    // If the cast failed then return
                    if (dbDictionary == null)
                        return;

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDbDictionary,
                        Data = dbDictionary,
                    };
                }
                catch (Exception ex)
                {
                    string s = "Error Getting Database Dictionary from DBDictionaryService.";
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDbDictionary,
                        Error = ex,
                        FriendlyErrorMessage = 
                            ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
                            s + "\nPlease verify if the DBDictionary Service is available" :
                            s + "\nPlease review the log on the server.",
                    };
                }
            }

            if (sender == _dbschemaClient)
            {
                try
                {
                    string result = ((DownloadStringCompletedEventArgs)e).Result;

                    DatabaseDictionary dbSchema = result.DeserializeDataContract<DatabaseDictionary>();

                    // If the cast failed then return
                    if (dbSchema == null)
                        return;

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDatabaseSchema,
                        Data = dbSchema,
                    };
                }
                catch (Exception ex)
                {
                    string s = "Error Getting Database Schema from DBDictionaryService.";
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDatabaseSchema,
                        Error = ex,
                        FriendlyErrorMessage = 
                            ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
                            s + "\nPlease verify if the DBDictionary Service is available" :
                            s + "\nPlease review the log on the server.",
                    };
                }
            }

            if (sender == _savedbdictionaryClient)
            {
                try
                {
                    string result = ((UploadStringCompletedEventArgs)e).Result;

                    Response response = result.DeserializeDataContract<Response>();

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.SaveDatabaseDictionary,
                        Data = response,
                    };
                }
                catch (Exception ex)
                {
                    string s = "Error while saving Database Dictionary through DBDictionaryService.";
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.SaveDatabaseDictionary,
                        Error = ex,
                        FriendlyErrorMessage = 
                            ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
                            s + "\nPlease verify if the DBDictionary Service is available" :
                            s + "\nPlease review the log on the server.",
                    };
                }
            }

            if (sender == _dbdictionariesClient)
            {
                try
                {
                    string result = ((DownloadStringCompletedEventArgs)e).Result;

                    List<string> dbDictionaries = result.DeserializeDataContract<List<string>>();

                    // If the cast failed then return
                    if (dbDictionaries == null)
                        return;

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetExistingDbDictionaryFiles,
                        Data = dbDictionaries,
                    };
                }
                catch (Exception ex)
                {
                    string s = "Error Getting existing Database Dictionary Files from DBDictionaryService.";
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetExistingDbDictionaryFiles,
                        Error = ex,
                        FriendlyErrorMessage = 
                            ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
                            s + "\nPlease verify if the DBDictionary Service is available" :
                            s + "\nPlease review the log on the server.",
                    };
                }
            }

            if (sender == _providersClient)
            {
                try
                {
                    string result = ((DownloadStringCompletedEventArgs)e).Result;

                    string[] providers = result.DeserializeDataContract<string[]>();

                    // If the cast failed then return
                    if (providers == null)
                        return;

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetProviders,
                        Data = providers,
                    };
                }
                catch (Exception ex)
                {
                    string s = "Error Getting Provider Names from DBDictionaryService.";
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetProviders,
                        Error = ex,
                        FriendlyErrorMessage = 
                            ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
                            s + "\nPlease verify if the DBDictionary Service is available" :
                            s + "\nPlease review the log on the server.",
                    };
                }
            }

            if (sender == _postdbdictionaryClient)
            {
                try
                {
                    string result = ((UploadStringCompletedEventArgs)e).Result;

                    Response response = result.DeserializeDataContract<Response>();

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.PostDictionaryToAdapterService,
                        Data = response,
                    };
                }
                catch (Exception ex)
                {
                    string s = "Error while posting Database Dictionary to the AdapterService.";
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.PostDictionaryToAdapterService,
                        Error = ex,
                        FriendlyErrorMessage = 
                            ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
                            s + "\nPlease verify if the DBDictionary Service is available" :
                            s + "\nPlease review the log on the server.",
                    };
                }
            }

            if (sender == _clearClient)
            {
                try
                {
                    string result = ((DownloadStringCompletedEventArgs)e).Result;

                    Response response = result.DeserializeDataContract<Response>();

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.ClearTripleStore,
                        Data = response,
                    };
                }
                catch (Exception ex)
                {
                    string s = "Error while clearing triple store through the AdapterService.";
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.ClearTripleStore,
                        Error = ex,
                        FriendlyErrorMessage = 
                            ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
                            s + "\nPlease verify if the DBDictionary Service is available" :
                            s + "\nPlease review the log on the server.",
                    };
                }
            }

            if (sender == _deleteClient)
            {
                try
                {
                    string result = ((DownloadStringCompletedEventArgs)e).Result;

                    Response response = result.DeserializeDataContract<Response>();

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.DeleteApp,
                        Data = response,
                    };
                }
                catch (Exception ex)
                {
                    string s = "Error while deleting the app from AdapterService.";
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.DeleteApp,
                        Error = ex,
                        FriendlyErrorMessage = 
                            ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") || ex.GetBaseException() is System.Net.WebException ?
                            s + "\nPlease verify if the DBDictionary Service is available" :
                            s + "\nPlease review the log on the server.",
                    };
                }
            }

            if (args != null)
                OnDataArrived(sender, args);

        }

    }
}
