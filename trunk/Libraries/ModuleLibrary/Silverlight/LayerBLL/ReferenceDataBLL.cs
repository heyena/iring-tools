
using org.iringtools.modulelibrary.layerdal;
using Microsoft.Practices.Unity;
using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.baseclass;
using org.iringtools.modulelibrary.types;
using System;
using System.Collections.Generic;
using org.ids_adi.qmxf;
using Microsoft.Practices.Composite.Events;
using org.iringtools.library.presentation.events;
using org.iringtools.informationmodel.events;
using org.iringtools.library;


namespace org.iringtools.modulelibrary.layerbll
{
  public class ReferenceDataBLL : BLLBase, IReferenceData
  {
    /// <summary>
    /// Occurs when [on data arrived].
    /// </summary>
    public event System.EventHandler<System.EventArgs> OnDataArrived;

    private IReferenceData dal = null;
    private IEventAggregator aggregator = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceDataBLL"/> class.
    /// </summary>
    /// <param name="container">The container.</param>
    public ReferenceDataBLL(IUnityContainer container, IEventAggregator aggregator)
    {
      this.aggregator = aggregator;

      // The BLL and DAL share the same interface so we 
      // can't use CONSTRUCTOR Injection to obtain the DAL
      // implementation - we have to provide the string parameter

      // Resolve the data access layer 
      dal = container.Resolve<IReferenceData>("ReferenceDataDAL");
      
      // All dal events will be handled by dal_OnDataArrived
      dal.OnDataArrived += dal_OnDataArrived;
    }

    //:::::::::::::::[ ASYNC Data Completed Events handled ]::::::::::::::::::::::

    #region dal_OnDataArrived(object sender, System.EventArgs e) 
    /// <summary>
    /// Bubble event to consumer
    /// ------------------------- 
    /// Handles the OnDataArrived event of the dal control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void dal_OnDataArrived(object sender, System.EventArgs e)
    {
      // Only handle properly populated event arguments
      CompletedEventArgs args = e as CompletedEventArgs;
      if (args == null)
        return;

      // CompletedEventArgs is a generic class that handles multiple
      // services.  We have to cast the CompletedType for our service.
      CompletedEventType processType = (CompletedEventType)
        Enum.Parse(typeof(CompletedEventType), args.CompletedType.ToString(), false);

      // Publish service stopped event
      aggregator.GetEvent<ServiceEvent>().Publish(new ServiceEventArgs
      {
        Process = ServiceProcessType.Stopped,
        ServiceName = processType.ToString()
      });

      // POST Processing (if applicable)
      switch (processType)
      {
        case CompletedEventType.NotDefined:
          break;

        case CompletedEventType.GetClass:
          break;

        case CompletedEventType.GetClassLabel:
          break;

        case CompletedEventType.GetUnitTestString:
          break;

        case CompletedEventType.GetDataDictionary:
          break;

        case CompletedEventType.GetMapping:
          break;

        case CompletedEventType.Search:
          break;

        case CompletedEventType.Find:
          break;

        case CompletedEventType.GetSubClasses:
          break;

        case CompletedEventType.GetClassTemplates:
          break;

        case CompletedEventType.GetTemplate:
          break;

        case CompletedEventType.GetSuperClasses:
          break;

        case CompletedEventType.GetQMXF:
          break;

        default:
          break;
      }


      // Raise event with updates 
      if (OnDataArrived != null)
          //if (((CompletedEventArgs)e).Data!=null)
            OnDataArrived(sender, e);

      aggregator.GetEvent<SpinnerEvent>().Publish(new SpinnerEventArgs
      {
          Active = SpinnerEventType.Stopped,
          ActiveService = Enum.GetName(typeof(CompletedEventType), processType)
      });
    } 
    #endregion

    //:::::::::[ ASYNC Data Layer calls ]::::::::::::::::::::::::::

    /// <summary>
    /// Starts the service (Spinner).  The Start and Stop Service 
    /// methods have to be in sync so that the spinner doesn't stop
    /// while other services are still running
    /// </summary>
    /// <param name="serviceName">Name of the service.</param>
    public void StartService(string serviceName)
    {
      aggregator.GetEvent<ServiceEvent>().Publish(new ServiceEventArgs
      {
        Process = ServiceProcessType.Starting,
        ServiceName = serviceName
      });

      aggregator.GetEvent<SpinnerEvent>().Publish(new SpinnerEventArgs
      {
        Active = SpinnerEventType.Started,
        ActiveService = serviceName
      });
    }

    // IMPORTANT NOTE:  All calls are Async so we don't actually
    // return anything; since we use the same interface as the 
    // actual service we have to implement the return.   All 
    // Data will arrive at dal_OnDataArrived (above)

    #region Data Layer Calls 
    /// <summary>
    /// get configured repositories
    /// </summary>
    /// <returns></returns>
    public Repositories GetRepositories()
    {        
       StartService("GetRepositories");
       return dal.GetRepositories();
    }

    public Entities GetEntityTypes()
    {
      StartService("GetEntityTypes");
      return dal.GetEntityTypes();
    }
    /// <summary>
    /// do a search
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    //public RefDataEntities Search(string query)
    //{
    //  StartService("Search");
    //  return dal.Search(query);
    //}

    public object Search(string query, object userState)
    {
        StartService("Search");
        return dal.Search(query, userState);
    }

    public object SearchReset(string query, object userState)
    {
        StartService("Search");
        return dal.SearchReset(query, userState);
    }
    /// <summary>
    /// do a search
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    //public RefDataEntities Search(string query)
    //{
    //  StartService("Search");
    //  return dal.Search(query);
    //}

    public object Find(string query, object userState)
    {
        StartService("Search");
        return dal.Find(query, userState);
    }

    /// <summary>
    /// do a search, ignoring cached results
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    //public RefDataEntities SearchReset(string query)
    //{
    //  StartService("SearchReset");
    //  return dal.SearchReset(query);
    //}

    /// <summary>
    /// do a search and return a specific page
    /// </summary>
    /// <param name="query"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    //public RefDataEntities SearchPage(string query, string page)
    //{
    //  StartService("SearchPage");
    //  return dal.SearchPage(query, page);
    //}

    /// <summary>
    /// do a search, ignoring cached results, and return a specific page
    /// </summary>
    /// <param name="query"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    //public RefDataEntities SearchPageReset(string query, string page)
    //{
    //  StartService("SearchPageReset");
    //  return dal.SearchPageReset(query, page);
    //}

    /// <summary>
    /// find a sepcific item by label
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    //public List<Entity> Find(string query)
    //{
    //  StartService("Find");
    //  return dal.Find(query);
    //}

    /// <summary>
    /// Gets the class.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns></returns>
    //public QMXF GetQMXF(string id)
    //{
    //  StartService("GetQMXF");
    //  return dal.GetQMXF(id);
    //}

    /// <summary>
    /// Gets the class.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns></returns>
    //public QMXF GetClass(string id)
    //{
    //  StartService("GetClass");
    //  return dal.GetClass(id);
    //}

    /// <summary>
    /// Gets the class.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns></returns>
    public QMXF GetClass(string id, object userState)
    {
        StartService("GetClass");
        return dal.GetClass(id, userState);
    }

    /// <summary>
    /// Gets the sub classes.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns></returns>
    //public List<Entity> GetSubClasses(string id)
    //{
    //  StartService("GetSubClasses");
    //  return dal.GetSubClasses(id);
    //}

    /// <summary>
    /// Resolves class uri id to label.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="uri">The uri.</param>
    /// <param name="userState">The ClassTreeItem.</param>
    public void GetClassLabel(string key, string uri, object userState)
    {
      StartService("GetClassLabel");
      dal.GetClassLabel(key, uri, userState);
    }
    
    /// <summary>
    /// Resolves template uri id to label.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="uri"></param>
    /// <param name="userState"></param>
    public void GetTemplateLabel(string key, string uri, object userState)
    {
      StartService("GetTemplateLabel");
      dal.GetTemplateLabel(key, uri, userState);
    }

    //// <summary>
    /// Gets the sub classes.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns></returns>
    public List<Entity> GetSubClasses(string id, object userState)
    {
      StartService("GetSubClasses");
      return dal.GetSubClasses(id, userState);
    }

    //public List<Entity> GetSuperClasses(string id)
    //{
    //  return dal.GetSuperClasses(id);
    //}

    public List<Entity> GetSuperClasses(string id, object userState)
    {
      return dal.GetSuperClasses(id, userState);
    }

    //public List<Entity> GetClassTemplates(string id)
    //{
    //  StartService("GetClassTemplates");
    //  return dal.GetClassTemplates(id);
    //}

    public List<Entity> GetClassTemplates(string id, object userState)
    {
        StartService("GetClassTemplates");        
        return dal.GetClassTemplates(id, userState);
    }

    /// <summary>
    /// Gets the template.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns></returns>
    //public QMXF GetTemplate(string id)
    //{
    //  StartService("GetTemplate");
    //  return dal.GetTemplate(id);
    //}
    public QMXF GetTemplate(string id, object userState)
    {
        StartService("GetTemplate");
        return dal.GetTemplate(id, userState);
    }

    /// <summary>
    /// Gets the part8 template.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns></returns>
    public QMXF GetPart8Template(string id, object userState)
    {
        StartService("GetPart8Template");
        return dal.GetPart8Template(id, userState);
    }

    /// <summary>
    /// Posts the template.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <returns></returns>
    public Response PostTemplate(QMXF template)
    {
      StartService("PostTemplate");
      return dal.PostTemplate(template);
    }

    /// <summary>
    /// Posts the part8 template.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <returns></returns>
    public Response PostPart8Template(QMXF template)
    {
        StartService("PostTemplate");
        return dal.PostPart8Template(template);
    }

    /// <summary>
    /// Posts the class.
    /// </summary>
    /// <param name="class">The @class.</param>
    /// <returns></returns>
    public Response PostClass(QMXF @class, object userState)
    {
      StartService("PostClass");
      return dal.PostClass(@class, userState);
    }

    public string GetReferenceDataServiceUri 
    { 
        get { return dal.GetReferenceDataServiceUri; }
    }
    
    #endregion


    public List<Entity> GetClassMembers(string id, object userState)
    {
      StartService("GetMembers");
      return dal.GetClassMembers(id, userState);
    }
  }
}
