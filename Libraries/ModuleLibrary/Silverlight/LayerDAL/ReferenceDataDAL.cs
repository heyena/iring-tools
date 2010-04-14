using System;
using System.Text;
using System.Collections.Generic;
using Library.Interface.Configuration;
using ModuleLibrary.Base;
using ModuleLibrary.Events;
using ModuleLibrary.Types;
using System.Linq;
using System.Net;
using org.ids_adi.iring.referenceData;
using org.iringtools.utility;
using org.ids_adi.qmxf;
using ModuleLibrary.Extensions;
using System.ComponentModel;
using Library.Interface.Events;
using org.iringtools.library;

namespace ModuleLibrary.LayerDAL
{
    /// <summary>
    /// DATA ACCESS LAYER FOR ModuleLibrary.Desktop: Service References\REferenceDataProxy
    /// 
    /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// NO BUSINESS LOGIC SHOULD BE IN THIS CLASS - USE ReferenceDataBLL for
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
    public class ReferenceDataDAL : DALBase, IReferenceData
    {
        /// <summary>
        /// Occurs when [on data arrived].  The business logic layer
        /// will subscribe to this so it will be notified when data has
        /// arrived 
        /// </summary>
        public event EventHandler<EventArgs> OnDataArrived;

        
        private WebClient _searchClient;
        private WebClient _findClient;
        private WebClient _classClient;
        private WebClient _templateClient;
        private WebClient _subClassClient;
        private WebClient _classTemplatesClient;
        private WebClient _testClient;
        private WebClient _postClassClient;
        private WebClient _postTemplateClient;

        private string _referenceDataServiceUri;

        /// <summary>
        /// CONSTRUCTOR
        /// Initializes a new instance of the <see cref="ReferenceDataDAL"/> class.
        /// </summary>
        /// <param name="config">The config.</param>
        public ReferenceDataDAL(IServerConfiguration config)
            : base(config)
        {
            try
            {
              _referenceDataServiceUri = config.ReferenceDataServiceUri;

              _searchClient = new WebClient();
              _findClient = new WebClient();
              _classClient = new WebClient();
              _templateClient = new WebClient();
              _subClassClient = new WebClient();
              _classTemplatesClient = new WebClient();
              _testClient = new WebClient();
              _postClassClient = new WebClient();
              _postTemplateClient = new WebClient();

              #region // All Async data results will be handled by OnCompleteEventHandler
              _searchClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _findClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _classClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _templateClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _subClassClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _classTemplatesClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _testClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _postClassClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              #endregion

            }
            catch (Exception ex)
            {
                Error.SetError(ex);
            }
        }

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
            #region // Search data arrived event handler
            // SEARCH
            if (sender == _searchClient)
            {
                try
                {
                  // Cast e (AsyncCompletedEventArgs) to actual type so we can
                  // retrieve the Result - assign to dictionary
                  string result = ((DownloadStringCompletedEventArgs)e).Result;

                  RefDataEntities entities = result.DeserializeDataContract<RefDataEntities>();

                  // If the cast failed then return
                  if (entities == null)
                    return;

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        UserState = ((DownloadStringCompletedEventArgs)e).UserState,
                        CompletedType = CompletedEventType.Search,
                        Data = entities
                    };
                }
                catch (Exception ex)
                {
                    // filling args to stop spinner
                    args = new CompletedEventArgs
                    {                        
                        CompletedType = CompletedEventType.Search,
                    };                    
                    Error.SetError(ex);
                }
            }

            #endregion

            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            #region // Find data arrived event handler
            // SEARCH
            if (sender == _findClient)
            {
                try
                {
                    // Cast e (AsyncCompletedEventArgs) to actual type so we can
                    // retrieve the Result - assign to dictionary
                    string result = ((DownloadStringCompletedEventArgs)e).Result;

                    List<Entity> entities = result.DeserializeDataContract<List<Entity>>();

                    // If the cast failed then return
                    if (entities == null)
                        return;

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        UserState = ((DownloadStringCompletedEventArgs)e).UserState,
                        CompletedType = CompletedEventType.Find,
                        Data = entities
                    };
                }
                catch (Exception ex)
                {
                    // filling args to stop spinner
                    args = new CompletedEventArgs
                    {
                        CompletedType = CompletedEventType.Find,
                    };
                    Error.SetError(ex);
                }
            }

            #endregion
            
            ////:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            #region // GetClass data arrived event handler
            if (sender == _classClient || sender == _templateClient)
            {
              try
              {
                string result = ((DownloadStringCompletedEventArgs)e).Result;

                QMXF qmxf = result.DeserializeXml<QMXF>();

              // If the cast failed then return
              if (qmxf == null)
                return;

              bool isClass = true;
              if (sender == _classClient)
                isClass = true;
              else if (sender == _templateClient)
                isClass = false;
                       
                args = new CompletedEventArgs
                    {
                        UserState = ((DownloadStringCompletedEventArgs)e).UserState,
                        CompletedType = isClass ? CompletedEventType.GetClass 
                                                : CompletedEventType.GetTemplate,
                        Data = qmxf
                    };
                }
                catch (Exception ex)
                {
                    // filling args to stop spinner
                    if (sender == _classClient)
                        args = new CompletedEventArgs
                        {
                            CompletedType = CompletedEventType.GetClass,
                        };      
                    else if (sender == _templateClient)
                        args = new CompletedEventArgs
                        {
                            CompletedType = CompletedEventType.GetTemplate,
                        };
                    Error.SetError(ex);
                }
            }
            #endregion

            ////:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            #region // SubClasss data arrived event handler
          if (sender == _subClassClient)
            {

                try
                {
                    string result = ((DownloadStringCompletedEventArgs)e).Result;

                    List<Entity> entities = result.DeserializeDataContract<List<Entity>>();

                    // If the cast failed then return
                    if (entities == null)
                        return;

                    args = new CompletedEventArgs
                    {
                        UserState = ((DownloadStringCompletedEventArgs)e).UserState,
                        CompletedType = CompletedEventType.GetSubClasses,
                        Data = entities
                    };
                }
                catch (Exception ex)
                {
                    
                    // filling args to stop spinner
                    args = new CompletedEventArgs
                    {
                        CompletedType = CompletedEventType.GetSubClasses,
                    };
                    Error.SetError(ex);
                }              
            }
            #endregion
           
            ////:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            #region // Class Templates data arrived event handler
          if(sender == _classTemplatesClient)
          {
              try
              {
                  string result = ((DownloadStringCompletedEventArgs)e).Result;

                  List<Entity> entities = result.DeserializeDataContract<List<Entity>>();

                  // If the cast failed then return
                  if (entities == null)
                      return;

                  args = new CompletedEventArgs
                  {
                      UserState = ((DownloadStringCompletedEventArgs)e).UserState,
                      CompletedType = CompletedEventType.GetClassTemplates,
                      Data = entities
                  };
              }
              catch (Exception ex)
              {
                  // filling args to stop spinner
                    args = new CompletedEventArgs
                    {
                        CompletedType = CompletedEventType.GetClassTemplates,
                    };
                  Error.SetError(ex);
              }            
          }
            #endregion

            ////:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            #region // Post Class data arrived event handler
          if (sender == _postClassClient)
          {
              try
              {
                  //string result = ((DownloadStringCompletedEventArgs)e).Result;

                  //List<Entity> entities = result.DeserializeDataContract<List<Entity>>();

                  // If the cast failed then return
                  //if (entities == null)
                  //    return;

                  args = new CompletedEventArgs
                  {
                      UserState = ((DownloadStringCompletedEventArgs)e).UserState,
                      CompletedType = CompletedEventType.PostClass,
                      Data = null//entities
                  };
              }
              catch (Exception ex)
              {
                  // filling args to stop spinner
                  args = new CompletedEventArgs
                  {
                      CompletedType = CompletedEventType.PostClass,
                  };
                  Error.SetError(ex);
              }              
          }
            #endregion

            ////:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            #region // Post Template data arrived event handler
          if (sender == _postClassClient)
          {
              try
              {
                  string result = ((DownloadStringCompletedEventArgs)e).Result;

                  List<Entity> entities = result.DeserializeDataContract<List<Entity>>();

                  // If the cast failed then return
                  if (entities == null)
                      return;

                  args = new CompletedEventArgs
                  {
                      UserState = ((DownloadStringCompletedEventArgs)e).UserState,
                      CompletedType = CompletedEventType.PostTemplate,
                      Data = entities
                  };
              }
              catch (Exception ex)
              {
                  // filling args to stop spinner
                  args = new CompletedEventArgs
                  {
                      CompletedType = CompletedEventType.PostTemplate,
                  };
                  Error.SetError(ex);
              }              
          }
          #endregion
                   
            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            // Raise the event with the args
            if (OnDataArrived != null)
                OnDataArrived(this, args);
        }
        #endregion

        void ClassLabelCompletedEvent(object sender, AsyncCompletedEventArgs e)
        {
          try
          {
            WebClient classLabelClient = (WebClient)sender;
            string result = ((DownloadStringCompletedEventArgs)e).Result;

            string baseAddress = classLabelClient.BaseAddress;
            int keyIndex = baseAddress.LastIndexOf("?key=");
            int uriIndex = baseAddress.LastIndexOf("&uri=");

            string key = baseAddress.Substring(keyIndex + 5, uriIndex - keyIndex - 5);
            string uri = baseAddress.Substring(uriIndex + 5);
            string label = result.DeserializeDataContract<string>();

            CompletedEventArgs args = new CompletedEventArgs
            {
              UserState = ((DownloadStringCompletedEventArgs)e).UserState,
              CompletedType = CompletedEventType.GetClassLabel,
              Data = new string[] { key, uri, label }
            };

            OnDataArrived(this, args);
          }
          catch (Exception ex)
          {
            throw ex;
          }
        }

        //:::::[  ASYNC METHOD CALLS ]::::::::::::::::::::::::::::::::::::::
        #region IReferenceDataService Members

        public List<Repository> GetRepositories()
        {
            throw new System.NotImplementedException();
        }

        public RefDataEntities Search(string query)
        {
          _searchClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/search/" + query));
            return null;
        }

        public object Search(string query, object userState)
        {
          _searchClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/search/" + query), userState);           
            return userState;
        }


        public RefDataEntities SearchReset(string query)
        {
            throw new System.NotImplementedException();
        }

        public RefDataEntities SearchPage(string query, string page)
        {
            throw new System.NotImplementedException();
        }

        public RefDataEntities SearchPageReset(string query, string page)
        {
            throw new System.NotImplementedException();
        }

        public List<Entity> Find(string query)
        {
            _findClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/find/" + query));
            return null;
        }

        public object Find(string query, object userState)
        {
            _findClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/find/" + query), userState);
            return userState;
        }

        public org.ids_adi.qmxf.QMXF GetQMXF(string id)
        {
          throw new System.NotImplementedException();
        }

        public QMXF GetClass(string id)
        {
          _classClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/classes/" + id)); 
            return null;
        }

        public QMXF GetClass(string id, object userState)
        {
          _classClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/classes/" + id), userState);           
          return null;
        }

        public void GetClassLabel(string key, string uri, object userState)
        {
          WebClient classLabelClient = new WebClient();
          string id = uri.Substring(uri.LastIndexOf("#") + 1);

          classLabelClient.BaseAddress += "?key=" + key + "&uri=" + uri;
          classLabelClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(ClassLabelCompletedEvent);
          classLabelClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/classes/" + id + "/label"), userState);
        }

        public List<Entity> GetSubClasses(string id)
        {
          _subClassClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/classes/" + id + "/subclasses"));           
          return null;
        }

        public List<Entity> GetSubClasses(string id, object userState)
        {
          _subClassClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/classes/" + id + "/subclasses"), userState);           
          return null;
        }

        public List<Entity> GetSuperClasses(string id)
        {
          throw new System.NotImplementedException();
        }

        public List<Entity> GetSuperClasses(string id, object userState)
        {
          throw new System.NotImplementedException();
        }

        public List<Entity> GetClassTemplates(string id)
        {
          _classTemplatesClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/classes/" + id + "/templates"));           
          return null;
        }
        public List<Entity> GetClassTemplates(string id, object userState)
        {
          _classTemplatesClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/classes/" + id + "/templates"),userState);           
          return null;
        }

        public List<Entity> GetClassRelationships(string id)
        {
            throw new System.NotImplementedException();
        }

        public List<Entity> GetClassProperties(string id)
        {
            throw new System.NotImplementedException();
        }

        public org.ids_adi.qmxf.QMXF GetTemplate(string id)
        {
          _templateClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/templates/" + id));           
          return null;
        }

        public org.ids_adi.qmxf.QMXF GetTemplate(string id, object userState)
        {
          _templateClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/templates/" + id), userState);           
          return null;
        }

        public org.iringtools.library.Response PostTemplate(QMXF template)
        {
            string message = Utility.SerializeXml<QMXF>(template);

            _postTemplateClient.Headers["Content-type"] = "application/xml";
            _postTemplateClient.Encoding = Encoding.UTF8;
            _postTemplateClient.UploadStringAsync(new Uri(_referenceDataServiceUri + "/templates"), "POST", message);

            return null;
        }

        public org.iringtools.library.Response PostClass(QMXF @class)
        {
            string message = Utility.SerializeXml<QMXF>(@class);

            _postClassClient.Headers["Content-type"] = "application/xml";
            _postClassClient.Encoding = Encoding.UTF8;
            _postClassClient.UploadStringAsync(new Uri(_referenceDataServiceUri + "/classes"), "POST", message);

            return null;
        }

        public string GetReferenceDataServiceUri { get { return _referenceDataServiceUri; } }
        
        #endregion
    }
}
