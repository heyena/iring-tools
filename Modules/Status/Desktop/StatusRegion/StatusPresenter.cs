using System;
using System.Collections.Generic;
using org.iringtools.informationmodel.events;
using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;

using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.types;
using org.iringtools.modulelibrary.layerdal;

using org.iringtools.ontologyservice.presentation.presentationmodels;

using org.ids_adi.qmxf;
using org.iringtools.library;
using org.iringtools.mapping;

namespace org.iringtools.modules.status.statusregion
{
  public class StatusPresenter : PresenterBase<IStatusView>
  {
    private IEventAggregator aggregator;
    private IIMPresentationModel model;
    private IReferenceData referenceDataService = null;
    private IAdapter adapterProxy = null;

    public StatusPresenter(IStatusView view,
        IIMPresentationModel model,
        IEventAggregator aggregator,
        IReferenceData referenceDataService,
        IAdapter adapterProxy)
      : base(view, model)
    {
      try
      {
        this.aggregator = aggregator;
        this.model = model;
        this.adapterProxy = adapterProxy;
        this.referenceDataService = referenceDataService;

        // Subscribe to status events
        //aggregator.GetEvent<StatusEvent>().Subscribe(StatusEventHandler);
        aggregator.GetEvent<NavigationEvent>().Subscribe(NavigationEventHandler);

        // Causes default status bar messsages to be cleared since
        // no Status property value is set
        //StatusEventHandler(new StatusEventArgs());

        if (!String.IsNullOrEmpty(referenceDataService.GetReferenceDataServiceUri))
        {
          View.stsLeftMessage = "Reference Data Service: " + referenceDataService.GetReferenceDataServiceUri;
        }

        if (!String.IsNullOrEmpty(adapterProxy.GetAdapterServiceUri))
        {
          View.stsRightMessage = "Adapter Service: " + adapterProxy.GetAdapterServiceUri;
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    public void NavigationEventHandler(NavigationEventArgs e)
    {
      try
      {
        Logger.Log(string.Format("{0} handled {1} event", ModuleFullName, e.DetailProcess),
          Category.Debug, Priority.None);

        //if (e.DetailProcess == DetailType.DataSource && e.SelectedNode.Tag is DataObject)
        //{
        //    StatusEventHandler(new StatusEventArgs
        //    {
        //        Message = string.Format("{0}", ""),                   
        //        StatusPanel = StatusType.Left
        //    });
        //}

        if (e.DetailProcess == DetailType.Mapping && e.SelectedNode.Tag is GraphMap)
        {
          GraphMap selectedMappingNode = (GraphMap)e.SelectedNode.Tag;
        }
        else if (e.DetailProcess == DetailType.Mapping && e.SelectedNode.Tag is TemplateMap)
        {
          TemplateMap selectedMappingNode = (TemplateMap)e.SelectedNode.Tag;
        }
        else if (e.DetailProcess == DetailType.Mapping && e.SelectedNode.Tag is RoleMap)
        {
          RoleMap selectedMappingNode = (RoleMap)e.SelectedNode.Tag;
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }
  }
}
