using System;
using System.Text;
using System.Collections.Generic;
using org.iringtools.library.presentation.configuration;
using org.iringtools.modulelibrary.baseclass;
using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.types;
using System.Linq;
using System.Net;
using org.ids_adi.iring.referenceData;
using org.iringtools.utility;
using org.ids_adi.qmxf;
using org.iringtools.modulelibrary.extensions;
using System.ComponentModel;
using org.iringtools.library.presentation.events;
using org.iringtools.library;

namespace org.iringtools.modulelibrary.layerdal
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
        private WebClient _searchResetClient;
        private WebClient _findClient;
        private WebClient _classClient;
        private WebClient _templateClient;
        private WebClient _subClassClient;
        private WebClient _classTemplatesClient;
        private WebClient _testClient;
        private WebClient _postClassClient;
        private WebClient _postTemplateClient;
        private WebClient _getRepositoriesClient;
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
              _searchResetClient = new WebClient();
              _findClient = new WebClient();
              _classClient = new WebClient();
              _templateClient = new WebClient();
              _subClassClient = new WebClient();
              _classTemplatesClient = new WebClient();
              _testClient = new WebClient();
              _postClassClient = new WebClient();
              _postTemplateClient = new WebClient();
              _getRepositoriesClient = new WebClient();

              #region // All Async data results will be handled by OnCompleteEventHandler
              _searchClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _searchResetClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _findClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _classClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _templateClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _subClassClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _classTemplatesClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _testClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
              _postClassClient.UploadStringCompleted += new UploadStringCompletedEventHandler(OnCompletedEvent);
              _postTemplateClient.UploadStringCompleted += new UploadStringCompletedEventHandler(OnCompletedEvent);
              _getRepositoriesClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
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

          #region // GetRepositories
          // <Method> data arrived event handler 
          if (sender == _getRepositoriesClient)
          {
              try
              {
                  string result = ((DownloadStringCompletedEventArgs)e).Result;
                  List<Repository> repositories = result.DeserializeDataContract<List<Repository>>();

                  if (repositories == null)
                      return;

                  args = new CompletedEventArgs
                  {
                      // Define your method in CompletedEventType and assign
                      CompletedType = CompletedEventType.GetRepositories,
                      Data = repositories
                  };
              }
              catch (Exception ex)
              {
                  args = new CompletedEventArgs
                  {
                      CompletedType = CompletedEventType.GetRepositories,
                      Error = ex,
                      FriendlyErrorMessage = "Reference Data Service returned an error while getting repositories.",
                  };
                  Error.SetError(ex);
              }
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
                        Error = ex,
                        FriendlyErrorMessage = "Reference Data Service returned an error while performing search.",
                    };                    
                    //Error.SetError(ex);
                }
            }

            #endregion

            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            #region // Search Reset data arrived event handler
            // SEARCH RESET
            if (sender == _searchResetClient)
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
                        Error = ex,
                        FriendlyErrorMessage = "Reference Data Service returned an error while performing search.",
                    };
                    //Error.SetError(ex);
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
                        Error = ex,
                        FriendlyErrorMessage = "Reference Data Service returned an error while performing \"Find\".",
                    };
                    //Error.SetError(ex);
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
                            Error = ex,
                            FriendlyErrorMessage = "Reference Data Service returned an error while performing GetClass.",
                        };      
                    else if (sender == _templateClient)
                        args = new CompletedEventArgs
                        {
                            CompletedType = CompletedEventType.GetTemplate,
                            Error = ex,
                            FriendlyErrorMessage = "Reference Data Service returned an error while performing GetTemplate.",
                        };
                    //Error.SetError(ex);
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
                        Error = ex,
                        FriendlyErrorMessage = "Reference Data Service returned an error while performing GetSubClasses.",
                    };
                    //Error.SetError(ex);
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
                        Error = ex,
                        FriendlyErrorMessage = "Reference Data Service returned an error while performing GetClassTemplates.",
                    };
                  //Error.SetError(ex);
              }            
          }
            #endregion

            ////:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            #region // Post Class data arrived event handler
          if (sender == _postClassClient)
          {
              try
              {
                  string result = ((DownloadStringCompletedEventArgs)e).Result;

                  Response response = result.DeserializeDataContract<Response>();

                  args = new CompletedEventArgs
                  {
                      UserState = ((UploadStringCompletedEventArgs)e).UserState,
                      CompletedType = CompletedEventType.PostClass,
                      Data = response
                  };
              }
              catch (Exception ex)
              {
                  // filling args to stop spinner
                  args = new CompletedEventArgs
                  {
                      CompletedType = CompletedEventType.PostClass,
                      Error = ex,
                      FriendlyErrorMessage = "Reference Data Service returned an error while posting the class.",
                  };
                  //Error.SetError(ex);
              }              
          }
            #endregion

            ////:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            #region // Post Template data arrived event handler
          if (sender == _postTemplateClient)
          {
              try
              {
                  string result = ((DownloadStringCompletedEventArgs)e).Result;

                  Response response = result.DeserializeDataContract<Response>();

                  args = new CompletedEventArgs
                  {
                      UserState = ((UploadStringCompletedEventArgs)e).UserState,
                      CompletedType = CompletedEventType.PostTemplate,
                      Data = response
                  };
              }
              catch (Exception ex)
              {
                  // filling args to stop spinner
                  args = new CompletedEventArgs
                  {
                      CompletedType = CompletedEventType.PostTemplate,
                      Error = ex,
                      FriendlyErrorMessage = "Reference Data Service returned an error while posting the template.",
                  };
                  //Error.SetError(ex);
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
            int tagIndex = baseAddress.LastIndexOf("?tag=");
            int idIndex = baseAddress.LastIndexOf("&id=");

            string tag = baseAddress.Substring(tagIndex + 5, idIndex - tagIndex - 5);
            string id = baseAddress.Substring(idIndex + 4);
            string label = result.DeserializeDataContract<string>();

            CompletedEventArgs args = new CompletedEventArgs
            {
              UserState = ((DownloadStringCompletedEventArgs)e).UserState,
              CompletedType = CompletedEventType.GetClassLabel,
              Data = new string[] { tag, id, label }
            };

            OnDataArrived(this, args);
          }
          catch (Exception ex)
          {
            throw ex;
          }
        }

        void TemplateLabelCompletedEvent(object sender, AsyncCompletedEventArgs e)
        {
          try
          {
            WebClient classLabelClient = (WebClient)sender;
            string result = ((DownloadStringCompletedEventArgs)e).Result;

            string baseAddress = classLabelClient.BaseAddress;
            int tagIndex = baseAddress.LastIndexOf("?tag=");
            int idIndex = baseAddress.LastIndexOf("&id=");

            string tag = baseAddress.Substring(tagIndex + 5, idIndex - tagIndex - 5);
            string id = baseAddress.Substring(idIndex + 4);

            string label = String.Empty;
            QMXF qmxf = result.DeserializeXml<QMXF>();

            if (qmxf.templateDefinitions != null && qmxf.templateDefinitions.Count > 0)
            {
              if (qmxf.templateDefinitions.FirstOrDefault().name != null &&
                  qmxf.templateDefinitions.FirstOrDefault().name.Count > 0)
              {
                label = qmxf.templateDefinitions.FirstOrDefault().name.FirstOrDefault().value;
              }
            }
            else if (qmxf.templateQualifications != null && qmxf.templateQualifications.Count > 0)
            {
              if (qmxf.templateQualifications.FirstOrDefault().name != null &&
                  qmxf.templateQualifications.FirstOrDefault().name.Count > 0)
              {
                label = qmxf.templateQualifications.FirstOrDefault().name.FirstOrDefault().value;
              }
            }
            
            CompletedEventArgs args = new CompletedEventArgs
            {
              UserState = ((DownloadStringCompletedEventArgs)e).UserState,
              CompletedType = CompletedEventType.GetClassLabel,
              Data = new string[] { tag, id, label }
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
            _getRepositoriesClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/repositories"));
            return null;
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

        public object SearchReset(string query, object userState)
        {
            _searchResetClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/search/" + query + "/reset"), userState);
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

        public void GetClassLabel(string tag, string id, object userState)
        {
          id = Utility.GetIdFromURI(id);

          if (!String.IsNullOrEmpty(id))
          {
            WebClient classLabelClient = new WebClient();
            classLabelClient.BaseAddress += "?tag=" + tag + "&id=" + id;
            classLabelClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(ClassLabelCompletedEvent);
            classLabelClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/classes/" + id + "/label"), userState);
          }
        }

        public void GetTemplateLabel(string tag, string id, object userState)
        {
          id = Utility.GetIdFromURI(id);

          if (!String.IsNullOrEmpty(id))
          {
            WebClient classLabelClient = new WebClient();
            classLabelClient.BaseAddress += "?tag=" + tag + "&id=" + id;
            classLabelClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(TemplateLabelCompletedEvent);
            classLabelClient.DownloadStringAsync(new Uri(_referenceDataServiceUri + "/templates/" + id), userState);
          }
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
            _postTemplateClient.UploadStringAsync(new Uri(template.targetRepository + "/templates"), "POST", message);

            return null;
        }

        public org.iringtools.library.Response PostClass(QMXF @class)
        {
            string message = Utility.SerializeXml<QMXF>(@class);
           
            _postClassClient.Headers["Content-type"] = "application/xml";
            _postClassClient.Encoding = Encoding.UTF8;
            _postClassClient.UploadStringAsync(new Uri(@class.targetRepository + "/classes"), "POST", message);

            return null;
        }

        public string GetReferenceDataServiceUri { get { return _referenceDataServiceUri; } }
        
        #endregion
    }
}
