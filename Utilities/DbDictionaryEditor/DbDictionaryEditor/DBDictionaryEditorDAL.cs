﻿using System;
using System.Net;
using org.iringtools.library;
using System.Text;
using org.iringtools.utility;
using System.Collections.Generic;
using System.ComponentModel;
using org.iringtools.modulelibrary.events;
using System.Collections.ObjectModel;

namespace DbDictionaryEditor
{
    public class DBDictionaryEditorDAL
    {
        public event EventHandler<EventArgs> OnDataArrived;

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

        public DBDictionaryEditorDAL() 
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
            _dbschemaClient.UploadStringCompleted += new UploadStringCompletedEventHandler(OnCompletedEvent);
            _savedbdictionaryClient.UploadStringCompleted += new UploadStringCompletedEventHandler(OnCompletedEvent);
            _dbdictionariesClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _providersClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _clearClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _postdbdictionaryClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _deleteClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);

            _dbDictionaryServiceUri = App.Current.Resources["DBDictionaryServiceURI"].ToString();
            _adapterServiceUri = App.Current.Resources["AdapterServiceUri"].ToString();
        }

        //public Collection<ScopeProject> GetScopes()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append(_adapterServiceUri);
        //    sb.Append("/scopes");
        //    _scopesClient.DownloadStringAsync(new Uri(sb.ToString()));
        //    return null;
        //}

        public DatabaseDictionary GetDbDictionary(string project, string application)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_dbDictionaryServiceUri);
            sb.Append("/");
            sb.Append(project);
            sb.Append("/");
            sb.Append(application);
            sb.Append("/dbdictionary");
            _dbDictionaryClient.DownloadStringAsync(new Uri(sb.ToString()));
            return null;
        }

        public Response SaveDatabaseDictionary(DatabaseDictionary dict, string project, string application)
        {
            string message = Utility.SerializeDataContract<DatabaseDictionary>(dict);

            StringBuilder sb = new StringBuilder();
            sb.Append(_dbDictionaryServiceUri);
            sb.Append("/");
            sb.Append(project);
            sb.Append("/");
            sb.Append(application);
            sb.Append("/savedbdictionary");
            
            _savedbdictionaryClient.Headers["Content-type"] = "application/xml";
            _savedbdictionaryClient.Encoding = Encoding.UTF8;
            _savedbdictionaryClient.UploadStringAsync(new Uri(sb.ToString()), "POST", message);

            return null;
        }

        public DatabaseDictionary GetDatabaseSchema(string connString, string dbProvider)
        {
            Request request = new Request();
            request.Add("connectionString", connString);
            request.Add("dbProvider", dbProvider);
            string message = Utility.SerializeDataContract<Request>(request);

            StringBuilder sb = new StringBuilder();
            sb.Append(_dbDictionaryServiceUri);
            sb.Append("/dbschema");

            _dbschemaClient.Headers["Content-type"] = "application/xml";
            _dbschemaClient.Encoding = Encoding.UTF8;
            _dbschemaClient.UploadStringAsync(new Uri(sb.ToString()), "POST", message);

            return null;            
        }

        public List<string> GetExistingDbDictionaryFiles()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_dbDictionaryServiceUri);
            sb.Append("/dbdictionaries");
            _dbdictionariesClient.DownloadStringAsync(new Uri(sb.ToString()));
            return null;
        }

        public String[] GetProviders()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_dbDictionaryServiceUri);
            sb.Append("/providers");
            _providersClient.DownloadStringAsync(new Uri(sb.ToString()));
            return null;              
        }

        public Response PostDictionaryToAdapterService(string projectName, string applicationName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_dbDictionaryServiceUri);
            sb.Append("/");
            sb.Append(projectName);
            sb.Append("/");
            sb.Append(applicationName);
            sb.Append("/postdbdictionary");
            _postdbdictionaryClient.DownloadStringAsync(new Uri(sb.ToString()));
            return null;  
        }

        public Response ClearTripleStore(string projectName, string applicationName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_adapterServiceUri);
            sb.Append("/");
            sb.Append(projectName);
            sb.Append("/");
            sb.Append(applicationName);
            sb.Append("/clear");
            _clearClient.DownloadStringAsync(new Uri(sb.ToString()));
            return null;            
        }

        public Response DeleteApp(string projectName, string applicationName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_adapterServiceUri);
            sb.Append("/");
            sb.Append(projectName);
            sb.Append("/");
            sb.Append(applicationName);
            sb.Append("/delete");
            _deleteClient.DownloadStringAsync(new Uri(sb.ToString()));
            return null;
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
            
            //if (sender == _scopesClient)
            //{
            //    string result = ((DownloadStringCompletedEventArgs)e).Result;

            //    Collection<ScopeProject> scopes = result.DeserializeDataContract<Collection<ScopeProject>>();

            //    // If the cast failed then return
            //    if (scopes == null)
            //        return;

            //    // Configure event argument
            //    args = new CompletedEventArgs
            //    {
            //        // Define your method in CompletedEventType and assign
            //        CompletedType = CompletedEventType.GetScopes,
            //        Data = scopes,
            //    };
            //}

            if (sender == _dbDictionaryClient)
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

            if (sender == _dbschemaClient)
            {
                string result = ((UploadStringCompletedEventArgs)e).Result;

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

            if (sender == _savedbdictionaryClient)
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

            if (sender == _dbdictionariesClient)
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

            if (sender == _providersClient)
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

            if (sender == _postdbdictionaryClient)
            {
                string result = ((DownloadStringCompletedEventArgs)e).Result;

                Response response = result.DeserializeDataContract<Response>();

                // Configure event argument
                args = new CompletedEventArgs
                {
                    // Define your method in CompletedEventType and assign
                    CompletedType = CompletedEventType.PostDictionaryToAdapterService,
                    Data = response,
                };
            }

            if (sender == _clearClient)
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

            if (sender == _deleteClient)
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

            if (args != null)
                OnDataArrived(sender, args);

        }

    }
}
