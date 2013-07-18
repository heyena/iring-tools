using System;
using Microsoft.Practices.Unity;
using org.iringtools.modulelibrary.types;
using org.iringtools.modulelibrary.events;
using System.Collections.ObjectModel;
using org.ids_adi.qxf;
using org.iringtools.modulelibrary.baseclass;
using Microsoft.Practices.Composite.Events;
using org.iringtools.library.presentation.events;
using org.iringtools.informationmodel.events;
using org.iringtools.library;
using org.iringtools.mapping;

namespace org.iringtools.modulelibrary.layerbll
{
  public class AdapterBLL : BLLBase, IAdapter
  {
    /// <summary>
    /// Occurs when [on data arrived].
    /// </summary>
    public event EventHandler<EventArgs> OnDataArrived;

    private IAdapter dal = null;
    private IEventAggregator aggregator = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdapterServiceProxyBLL"/> class.
    /// </summary>
    /// <param name="container">The container.</param>
    public AdapterBLL(IUnityContainer container, IEventAggregator aggregator)
    {
      this.aggregator = aggregator;

      // The BLL and DAL share the same interface so we 
      // can't use CONSTRUCTOR Injection to obtain the DAL
      // implementation - we have to provide the string parameter

      // Resolve the data access layer 
      dal = container.Resolve<IAdapter>("AdapterProxyDAL");

      // The BLL only has one event to subscribe to
      dal.OnDataArrived += new EventHandler<EventArgs>(dal_OnDataArrived);
    }

    //:::::::::::::::[ ASYNC Data Completed Events handled ]::::::::::::::::::::::

    #region dal_OnDataArrived(object sender, EventArgs e)
    /// <summary>
    /// Bubble event to consumer
    /// ------------------------- 
    /// Handles the OnDataArrived event of the dal control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> 
    /// instance containing the event data.</param>
    void dal_OnDataArrived(object sender, EventArgs e)
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

        case CompletedEventType.GetUnitTestString:
          break;

        case CompletedEventType.GetDataDictionary:
          break;

        case CompletedEventType.GetMapping:
          break;

        case CompletedEventType.Search:
          break;

        default:
          break;
      }


      // Raise the event after processing (bubble to subscribers)     
      if (OnDataArrived != null)
        OnDataArrived(sender, e);

      aggregator.GetEvent<SpinnerEvent>().Publish(new SpinnerEventArgs
      {
        Active = SpinnerEventType.Stopped,
        ActiveService = processType.ToString()
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
    /// Gets the unit test string.
    /// </summary>
    /// <param name="valueToReturn">The value to return.</param>
    public void GetUnitTestString(string valueToReturn)
    {
      dal.GetUnitTestString(valueToReturn);
    }



    /// <summary>
    /// Gets the mapping.
    /// </summary>
    public Mapping GetMapping(string projectName, string applicationName)
    {
      StartService("GetMapping");
      return dal.GetMapping(projectName, applicationName);
    }

    /// <summary>
    /// Gets the data dictionary.
    /// </summary>
    /// <returns></returns>
    public DataDictionary GetDictionary(string projectName, string applicationName)
    {
      StartService("GetDictionary");
      return dal.GetDictionary(projectName, applicationName);
    }


    public Response GetScopes()
    {
      StartService("GetScopes");
      return dal.GetScopes();
    }
    // THE FOLLOWING NOT IMPLEMENTED 



    /// <summary>
    /// Updates the mapping.
    /// </summary>
    /// <param name="mapping">The mapping.</param>
    /// <returns></returns>
    public Response UpdateMapping(string projectName, string applicationName, Mapping mapping)
    {
      StartService("UpdateMapping");
      return dal.UpdateMapping(projectName, applicationName, mapping);
    }

    public Response UpdateMapping(Mapping mapping)
    {
      StartService("UpdateMapping");
      return dal.UpdateMapping(mapping);
    }

    public Response RefreshAll(string projectName, string applicationName)
    {
      StartService("RefreshAll");
      return dal.RefreshAll(projectName, applicationName);
    }
    public Response RefreshAll(object userState)
    {
      StartService("RefreshAll");
      return dal.RefreshAll(userState);
    }


    public Response Generate(string projectName, string applicationName)
    {
      StartService("Generate");
      return dal.Generate(projectName, applicationName);
    }

    public Response Generate(object userState)
    {
      StartService("Generate");
      return dal.Generate(userState);
    }


    public Response RefreshGraph(string projectName, string applicationName, string graphName)
    {
      StartService("RefreshGraph");
      throw new NotImplementedException();
    }

    public QXF Get(string graphName, string identifier)
    {
      StartService("Get");
      throw new NotImplementedException();
    }

    public Response ClearStore(string projectName, string applicationName)
    {
      StartService("ClearStore");
      throw new NotImplementedException();
    }

    public QXF GetList(string graphName)
    {
      StartService("GetList");
      throw new NotImplementedException();
    }

    public Response Pull(Request request)
    {
      StartService("Pull");
      throw new NotImplementedException();
    }

    public string GetAdapterServiceUri
    {
      get { return dal.GetAdapterServiceUri; }
    }

    public string GetBaseAddress
    {
      get { return dal.GetBaseAddress; }
    }
    #endregion
  }
}
