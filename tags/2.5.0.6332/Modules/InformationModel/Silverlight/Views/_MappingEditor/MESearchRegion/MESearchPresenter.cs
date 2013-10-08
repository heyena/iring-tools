using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using ModuleLibrary.Events;
using ModuleLibrary.Extensions;
using ModuleLibrary.Types;
using OntologyService.Interface;
using PrismContrib.Base;
using System.Linq;
using OntologyService.Interface.PresentationModels;
using org.ids_adi.iring;

namespace InformationModel.Views.MESearchRegion
{
  /// <summary>
  /// Search View Presenter
  /// </summary>
  public class MESearchPresenter : PresenterBase<IMESearchView>
  {
    #region Controls (button and textbox references) 
    /// <summary>
    /// Gets a reference to the btnSearch control on View
    /// </summary>
    /// <value>Sear</value>
    Button btnSearch { get { return ButtonCtrl("btnSearch"); } }

    /// <summary>
    /// generate.
    /// </summary>
    /// <value>The BTN generate.</value>
    Button btnGenerate { get { return ButtonCtrl("btnGenerate"); } }

    /// <summary>
    /// refresh.
    /// </summary>
    /// <value>The BTN refresh.</value>
    Button btnRefresh { get { return ButtonCtrl("btnRefresh"); } }

    /// <summary>
    /// Gets a reference to the txtSearch control on View
    /// </summary>
    /// <value>The TXT search.</value>
    TextBox txtSearch { get { return TextCtrl("txtSearch"); } } 
    #endregion

    private IIMPresentationModel model = null;
    private IAdapter adapterProxy = null;
    private Mapping mapping = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="MESearchPresenter"/> class.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="model">The model.</param>
    public MESearchPresenter(IMESearchView view, IMPresentationModel model,
      IEventAggregator aggregator,
      IAdapter adapterProxy)
      : base(view, model)
    {
      try
      {
        // for class use
        this.adapterProxy = adapterProxy;
        this.model = model;

        // By default the button is disabled
        btnSearch.IsEnabled = false;

        #region TextChanged handler - enables/disables btnSearch
        // Handle textchanged event within dynamic delegate.
        // Enable button if user types in search criteria
        txtSearch.TextChanged += (object sender, TextChangedEventArgs e) =>
        {
          btnSearch.IsEnabled = ((TextBox)sender).Text.Length > 0;
        };
        #endregion

        #region Searches will be handled by the NavigationTreePresenter
        txtSearch.KeyDown += (object sender, KeyEventArgs e) =>
      {
        if (e.Key == Key.Enter)
        {
          ButtonEventArgs args = new ButtonEventArgs
          {
            ButtonClicked = btnSearch,
            Sender = this
          };

          // place the search request in the tag
          args.ButtonClicked.Tag = txtSearch.Text;

          Logger.Log(string.Format("SEARCH REQUEST FOR [{0}]", txtSearch.Text), Category.Debug, Priority.None);

          // Publish event
          // ButtonEvent and ButtonEventArgs defined in ModuleLibrary \ Events
          aggregator.GetEvent<ButtonEvent>().Publish(new ButtonEventArgs(this, btnSearch));
        }

      };

        btnSearch.Click += (object sender, RoutedEventArgs e) =>
        {
          ButtonEventArgs args = new ButtonEventArgs
          {
            ButtonClicked = btnSearch,
            Sender = this
          };

          // place the search request in the tag
          args.ButtonClicked.Tag = txtSearch.Text;

          Logger.Log(string.Format("SEARCH REQUEST FOR [{0}]", txtSearch.Text), Category.Debug, Priority.None);

          // Publish event
          // ButtonEvent and ButtonEventArgs defined in ModuleLibrary \ Events
          aggregator.GetEvent<ButtonEvent>().Publish(new ButtonEventArgs(this, btnSearch));
        };

        #endregion

        btnRefresh.Click += RefreshClickHandler;
        btnGenerate.Click += GenerateClickHandler;

        adapterProxy.OnDataArrived += OnDataArrivedHandler;
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    #region Button Click Handlers 


    /// <summary>
    /// Generates 
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void GenerateClickHandler(object sender, RoutedEventArgs e)
    {
      adapterProxy.Generate();
    }
    /// <summary>
    /// Refreshes 
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void RefreshClickHandler(object sender, RoutedEventArgs e)
    {
      adapterProxy.RefreshAll();
    }
    /// <summary>
    /// Saves 
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void SaveClickHandler(object sender, RoutedEventArgs e)
    {
      // TODO: Call Service and update OnDataArrivedHandler to
      //       execute SaveHandler with the applicable argument

      // The following is stubbed in until the above is completed
      SaveHandler(null);
    }


    #endregion

    /// <summary>
    /// Called when [data arrived handler].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnDataArrivedHandler(object sender, EventArgs e)
    {
      CompletedEventArgs args = e as CompletedEventArgs;
      if (args == null)
        return;

      if (args.CheckForType(CompletedEventType.RefreshAll))
        RefreshAllHandler(args);

      if (args.CheckForType(CompletedEventType.Generate))
        GenerateHandler(args);

      if (args.CheckForType(CompletedEventType.GetMapping))
        GetMappingHandler(args);

      if (args.CheckForType(CompletedEventType.UpdateMapping))
      {
        MessageBox.Show(args.Data.ToString(), "UPDATE MAPPING", MessageBoxButton.OK);
      }

    }


    /// <summary>
    /// Generates
    /// </summary>
    /// <param name="e">The <see cref="ModuleLibrary.Events.CompletedEventArgs"/> instance containing the event data.</param>
    public void GenerateHandler(CompletedEventArgs e)
    {
      MessageBox.Show(e.Data.ToString(),"GENERATE", MessageBoxButton.OK);
    }


    /// <summary>
    /// Refreshes
    /// </summary>
    /// <param name="e">The <see cref="ModuleLibrary.Events.CompletedEventArgs"/> instance containing the event data.</param>
    public void RefreshAllHandler(CompletedEventArgs e)
    {
      MessageBox.Show(e.Data.ToString(), "REFRESH", MessageBoxButton.OK);
    }


    /// <summary>
    /// Saves 
    /// </summary>
    /// <param name="e">The <see cref="ModuleLibrary.Events.CompletedEventArgs"/> instance containing the event data.</param>
    public void SaveHandler(CompletedEventArgs e)
    {
      TreeView tvwMapping = model.MappingTree;

      // TODO: Populate mapping object from tvwMapping object
      //       as applicable and send to back end
      //
      // For development purposes populated mapping object (from original load)
      // is used as stub data to test plumbing
      adapterProxy.UpdateMapping(mapping);
    }


    /// <summary>
    /// TEMPORARY: provide populated mapping object to test save
    /// plumbing and for reference to build mapping object from tvwMapping 
    /// </summary>
    /// <param name="e">The <see cref="ModuleLibrary.Events.CompletedEventArgs"/> instance containing the event data.</param>
    void GetMappingHandler(CompletedEventArgs e)
    {
      mapping = e.Data as Mapping;
    }
  }
}
