using OntologyService.Interface.PresentationModels;
using PrismContrib.Base;
using Microsoft.Practices.Composite.Events;
using ModuleLibrary.Events;
using ModuleLibrary.Types;
using System;
using Microsoft.Practices.Composite.Logging;
using InformationModel.Events;
using org.ids_adi.iring;
using org.ids_adi.qmxf;
using org.ids_adi.iring.referenceData;
using System.Collections.Generic;

namespace InformationModel.Views.StatusRegion
{
  public class StatusPresenter : PresenterBase<IStatusView>
  {
    private IEventAggregator aggregator;
    private IIMPresentationModel model;

    public StatusPresenter(IStatusView view, IIMPresentationModel model,
      IEventAggregator aggregator)
      : base(view, model)
    {
      try
      {
        this.aggregator = aggregator;
        this.model = model;

        // Subscribe to status events
        aggregator.GetEvent<StatusEvent>().Subscribe(StatusEventHandler);
        aggregator.GetEvent<NavigationEvent>().Subscribe(NavigationEventHandler);

        // Causes default status bar messsages to be cleared since
        // no Status property value is set
        StatusEventHandler(new StatusEventArgs());
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
          }
          else if (e.SelectedNode.Tag is ClassDefinition)
          {
            ClassDefinition selectedInformationModelNode = (ClassDefinition)e.SelectedNode.Tag;

            StatusEventHandler(new StatusEventArgs
            {
              Message = string.Format("{0}", selectedInformationModelNode.identifier),
              StatusPanel = StatusType.Right
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
