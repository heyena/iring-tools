using System;
using System.Collections.Generic;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;

using ModuleLibrary.Events;
using ModuleLibrary.Types;
using ModuleLibrary.LayerDAL;
using InformationModel.Events;

using OntologyService.Interface.PresentationModels;

using org.ids_adi.iring;
using org.ids_adi.qmxf;
using org.ids_adi.iring.referenceData;
using org.iringtools.library;

namespace Modules.Status.StatusRegion
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
            aggregator.GetEvent<StatusEvent>().Subscribe(StatusEventHandler);
            aggregator.GetEvent<NavigationEvent>().Subscribe(NavigationEventHandler);

            // Causes default status bar messsages to be cleared since
            // no Status property value is set
            StatusEventHandler(new StatusEventArgs());

            //View.stsLeftMessage = "Reference Data Service [" + referenceDataService.GetReferenceDataServiceUri + "]";

        }
        catch (Exception ex)
        {
            Error.SetError(ex);
        }
    }

    /// <summary>
    /// Status event handler.
    /// </summary>
    /// <param name="e">The <see cref="ModuleLibrary.Events.StatusEventArgs"/> 
    /// instance containing the event data.</param>
    public void StatusEventHandler(StatusEventArgs e)
    {
      StoryBoardCtrl("NoticeMe").Stop();

      switch (e.StatusPanel)
      {
        case StatusType.Left:
          View.stsLeftMessage = e.Message;
          break;

        case StatusType.Middle:
          View.stsMiddleMessage = e.Message;
          break;
        
        case StatusType.Right:
          View.stsRightMessage = e.Message;
          break;
        
        default: 
          // Reset status bar messages
          View.stsLeftMessage = "";
          View.stsRightMessage = "";
          View.stsMiddleMessage = "";
          break;
      }
    }

    public void NavigationEventHandler(NavigationEventArgs e)
    {
        Logger.Log(string.Format("{0} handled {1} event", ModuleFullName, e.DetailProcess),
            Category.Debug, Priority.None);

        // Update middle status with selected data source node info
        if (e.DetailProcess == DetailType.DataSource && e.SelectedNode.Tag is DataProperty)
        {
            DataProperty selectedDataSourceNode = (DataProperty)e.SelectedNode.Tag;
            StatusEventHandler(new StatusEventArgs
            {
                Message = string.Format("{0}:{1} Selected", selectedDataSourceNode.propertyName,
                   selectedDataSourceNode.dataType),
                StatusPanel = StatusType.Middle
            });
        }
        else if (e.DetailProcess == DetailType.DataSource && e.SelectedNode.Tag is DataObject)
        {
            StatusEventHandler(new StatusEventArgs
            {
                Message = string.Format("{0}", ""),                   
                StatusPanel = StatusType.Left
            });
        }

        if (e.DetailProcess == DetailType.Mapping && e.SelectedNode.Tag is GraphMap)
        {
            GraphMap selectedMappingNode = (GraphMap)e.SelectedNode.Tag;
            StatusEventHandler(new StatusEventArgs
            {
                Message = string.Format("Adapter Service {0}", adapterProxy.GetAdapterServiceUri),
                StatusPanel = StatusType.Left
            });
        }
        else if (e.DetailProcess == DetailType.Mapping && e.SelectedNode.Tag is TemplateMap)
        {
            TemplateMap selectedMappingNode = (TemplateMap)e.SelectedNode.Tag;
            StatusEventHandler(new StatusEventArgs
            {
                Message = string.Format("Adapter Service {0}", adapterProxy.GetAdapterServiceUri),
                StatusPanel = StatusType.Left
            });
        }
        else if (e.DetailProcess == DetailType.Mapping && e.SelectedNode.Tag is RoleMap)
        {
            RoleMap selectedMappingNode = (RoleMap)e.SelectedNode.Tag;
            StatusEventHandler(new StatusEventArgs
            {
                Message = string.Format("Adapter Service {0}", adapterProxy.GetAdapterServiceUri),
                StatusPanel = StatusType.Left
            });
        }
        // Update middle status with selected information model node info
        if ((e.DetailProcess == DetailType.NotDefined || e.DetailProcess==DetailType.InformationModel))
        {
          if (e.SelectedNode.Tag is Entity)
          {
            Entity selectedInformationModelNode = (Entity)e.SelectedNode.Tag;

            StatusEventHandler(new StatusEventArgs
            {
                Message = string.Format("{0}", selectedInformationModelNode.uri),
                StatusPanel = StatusType.Right
            });

            StatusEventHandler(new StatusEventArgs
            {
                Message = string.Format("ReferenceDataService: {0}", referenceDataService.GetReferenceDataServiceUri),
                StatusPanel = StatusType.Left
            });
          }
          else if (e.SelectedNode.Tag is ClassDefinition)
          {
            ClassDefinition selectedInformationModelNode = (ClassDefinition)e.SelectedNode.Tag;

            StatusEventHandler(new StatusEventArgs
            {
              Message = string.Format("{0}", selectedInformationModelNode.identifier),
              StatusPanel = StatusType.Right
            });

            StatusEventHandler(new StatusEventArgs
            {
                Message = string.Format("ReferenceDataService: {0}", referenceDataService.GetReferenceDataServiceUri),
                StatusPanel = StatusType.Left
            });
          }
          else if (e.SelectedNode.Tag is TemplateQualification)
          {
              TemplateQualification selectedInformationModelNode = (TemplateQualification)e.SelectedNode.Tag;

              StatusEventHandler(new StatusEventArgs
              {
                  Message = string.Format("{0}", selectedInformationModelNode.identifier),
                  StatusPanel = StatusType.Right
              });

              StatusEventHandler(new StatusEventArgs
              {
                  Message = string.Format("ReferenceDataService: {0}", referenceDataService.GetReferenceDataServiceUri),
                  StatusPanel = StatusType.Left
              });
          }          
        }
    }


    /// <summary>
    /// Called when [model property change].
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public override void OnModelPropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case "CurrentModelNode":
          // Show selected model node in middle status area
          StatusEventHandler(new StatusEventArgs
          {
            StatusPanel = StatusType.Middle,
            Message = "Selected: " + model.SelectedIMLabel
          });
          break;


      }
    }

  }
}
