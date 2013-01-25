using System;
using System.Windows;
using System.Windows.Controls;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;

using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.types;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

namespace org.iringtools.modules.menu.menuregion
{
  public class MenuPresenter : PresenterBase<IMenuView>
  {

    #region btnMappingEditor 
    private Button btnMappingEditor
    {
      get { return ButtonCtrl("btnMappingEditor1"); }
    }
    
    #endregion
    #region btnRefDataEditor 
    private Button btnRefDataEditor
    {
      get { return ButtonCtrl("btnRefDataEditor1"); }
    } 
    #endregion

    private IEventAggregator aggregator;

    private IMPresentationModel model;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuPresenter"/> class.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="model">The model.</param>
    /// <param name="regionManager">The region manager.</param>
    public MenuPresenter(IMenuView view, IIMPresentationModel model,
      IEventAggregator aggregator)
      : base(view, model)
    {
      try
      {
        this.model = (IMPresentationModel) model;
        this.aggregator = aggregator;

        // Since both buttons access the same event handler we'll hook into the
        // event delegate and do some preprocessing - in this case enable the
        // buttons (clicked button will be disabled in event handler).
        //btnMappingEditor.Click += (object sender, RoutedEventArgs e)
        //  => {EnableButtons(); buttonClickHandler(sender, e); };
        //btnRDSWIPEditor.Click += (object sender, RoutedEventArgs e)
        //  => { EnableButtons(); buttonClickHandler(sender, e); };
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    void adapterService_OnDataArrived(object sender, EventArgs e)
    {
      
    }


    /// <summary>
    /// Handles the Button Click - activates the view specified in the button's Tag 
    /// for the "MainRegion"
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void buttonClickHandler(object sender, System.Windows.RoutedEventArgs e)
    {
      // Cast sender to button so we can access it easier
      Button button = sender as Button;

      Logger.Log(string.Format("Button Click: {0} for View: {1}",
        button.Name,button.Tag), Category.Debug, Priority.None);

      //aggregator.GetEvent<StatusEvent>().Publish(new StatusEventArgs
      //{
      //  Message = string.Format("View {0} selected", button.Tag),
      //  StatusPanel = StatusType.Left  // Left status bar
      //});

      // Disable the button just clicked
      button.IsEnabled = false;
    }

    /// <summary>
    /// Enables the buttons.
    /// </summary>
    private void EnableButtons()
    {
      btnRefDataEditor.IsEnabled = true;
      btnMappingEditor.IsEnabled = true;
    }
  
  }
}
