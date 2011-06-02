using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using org.iringtools.informationmodel.events;
using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;

using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.extensions;
using org.iringtools.modulelibrary.layerdal;
using org.iringtools.modulelibrary.types;

using org.iringtools.informationmodel.usercontrols;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Media;

namespace org.iringtools.modules.search.searchregion
{
    /// <summary>
    /// Information Model View Presenter
    /// </summary>robdeca
    public class SearchControlPresenter : PresenterBase<ISearchControl>
    {
        // Fields for class
        IEventAggregator aggregator = null;
        IReferenceData referenceDataService = null;
        IUnityContainer container = null;
        private TabControl tabCtrl = null;

        #region PROPERTY: itcModelBrowser (ItemsControl - container for treeview)
        /// <summary>
        /// Gets the ItemsControl reference where the model treeview will reside
        /// </summary>
        /// <value>The TVW model browser.</value>
        private ItemsControl itcModelBrowser { get { return GetControl<ItemsControl>("itcModelBrowser"); } }
        #endregion

        #region Controls (button and textbox references)
        /// <summary>
        /// Gets a reference to the chkReset control on View
        /// </summary>
        CheckBox chkReset { get { return CheckBoxCtrl("chkReset"); } }
        
        /// <summary>
        /// Gets a reference to the btnSearch control on View
        /// </summary>
        /// <value>Sear</value>
        Button btnSearch { get { return ButtonCtrl("btnSearch"); } }

        /// <summary>
        /// Gets a reference to the txtSearch control on View
        /// </summary>
        /// <value>The TXT search.</value>
        TextBox txtSearch { get { return TextCtrl("txtSearch"); } }
        #endregion

        public Image buttonImage { get; set; }

        private IIMPresentationModel model = null;
        //private IAdapter adapterProxy = null;
        //private Mapping mapping = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MESearchPresenter"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="model">The model.</param>
        public SearchControlPresenter(ISearchControl view, IMPresentationModel model,
          IEventAggregator aggregator,
          IReferenceData referenceDataService,
          IUnityContainer container
            )
            : base(view, model)
        {
            try
            {
                // For class use
                this.referenceDataService = referenceDataService;
                this.aggregator = aggregator;
                this.container = container;
                this.model = model;

                aggregator.GetEvent<ButtonEvent>().Subscribe(ButtonClickHandler);
                aggregator.GetEvent<SpinnerEvent>().Subscribe(SpinnerEventHandler);

                // setup tab control - we want to be notified when tab
                // selection changes occur
                tabCtrl = new TabControl();
                tabCtrl.BorderThickness = new Thickness(1);
                tabCtrl.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 143, 160, 174));
                tabCtrl.SelectionChanged += tabSelectionChanged;

                //tabCtrl.Width = itcModelBrowser.Width;
                //tabCtrl.Height = itcModelBrowser.Height;

                // set margins and dynamically add treeview control to 
                // itcModelBrowser ItemsControl
                itcModelBrowser.Margin = new System.Windows.Thickness { Bottom = 4, Left = 4, Right = 4, Top = 4 };
                itcModelBrowser.Items.Add(tabCtrl);

                // Subscribe to data events (reference Data Service calls)
                referenceDataService.OnDataArrived += OnDataArrivedHandler;

                // for class use
                //this.adapterProxy = adapterProxy;
                this.model = model;

                // By default the button is disabled
                btnSearch.IsEnabled = false;
                chkReset.IsEnabled = false;

                #region TextChanged handler - enables/disables btnSearch
                // Handle textchanged event within dynamic delegate.
                // Enable button if user types in search criteria
                txtSearch.TextChanged += (object sender, TextChangedEventArgs e) =>
                {
                    btnSearch.IsEnabled = ((TextBox)sender).Text.Length > 0;
                    chkReset.IsEnabled = ((TextBox)sender).Text.Length > 0;
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

                      ButtonClickHandler(new ButtonEventArgs(this, btnSearch));

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

                    ButtonClickHandler(new ButtonEventArgs(this, btnSearch));

                };

                #endregion

            }
            catch (Exception ex)
            {
                Error.SetError(ex);
            }
        }
        #region Spinner Event Handler (toggles Search button and model treeview control enables/disabled)
        public void SpinnerEventHandler(SpinnerEventArgs e)
        {
            try
            {
                switch (e.Active)
                {
                    case SpinnerEventType.Started:
                        btnSearch.IsEnabled = false;
                        this.itcModelBrowser.IsEnabled = false;
                        break;

                    case SpinnerEventType.Stopped:
                        btnSearch.IsEnabled = true;
                        this.itcModelBrowser.IsEnabled = true;
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region EVENT HANDLER: ButtonClickHandler(ButtonEventArgs e)  -- Search Button
        /// <summary>
        /// Button Click Handler
        /// </summary>
        /// <param name="e">The <see cref="org.iringtools.modulelibrary.events.ButtonEventArgs"/> instance containing the event data.</param>

        public void ButtonClickHandler(ButtonEventArgs e)
        {
            try
            {
                //// We're only handling the search button click - we 
                //// don't care about others

                //if (!e.GetUniqueName().Contains("SearchControlPresenter:btnSearch"))
                //  return;

                //// The ButtonEventArgs will contain the search string (stored in in the
                //// button's tag prior to publishing search event)
                //string searchString = e.GetTag<string>();

                //Logger.Log(string.Format("{0} is handling EVENT {1}", ModuleFullName, e.GetUniqueName()),
                //  Category.Debug, Priority.None);

                //// On search button click instantiate a new tab
                //SearchTabItem tabItem = Container.Resolve<SearchTabItem>();

                //// Set the tab header to the search content
                //tabItem.HeaderText = searchString;

                //// Subscribe to close click event
                //tabItem.OnCloseClick += OnTabCloseHandler;

                //// Add new search tab item to the tab control
                //tabCtrl.Items.Add(tabItem);

                //// Set it as the currently selected tab
                //tabCtrl.SelectedItem = tabItem;

                //// Perform the search - async response handled by the
                //// OnDataArrivedHandler(); we'll send the current tab
                //// as UserState for the async call
                //referenceDataService.Search(searchString, tabItem);

                //new code
                // We're only handling the search button click - we 
                // don't care about others

                if (e.GetUniqueName().Contains("SearchControlPresenter:btnSearch"))
                {

                    // The ButtonEventArgs will contain the search string (stored in in the
                    // button's tag prior to publishing search event)
                    string searchString = e.GetTag<string>();

                    Logger.Log(string.Format("{0} is handling EVENT {1}", ModuleFullName, e.GetUniqueName()),
                      Category.Debug, Priority.None);

                    // On search button click instantiate a new tab
                    SearchTabItem tabItem = Container.Resolve<SearchTabItem>();
                    
                    // Set the tab header to the search content
                    tabItem.HeaderText = searchString;

                    // Subscribe to close click event
                    tabItem.OnCloseClick += OnTabCloseHandler;

                    // Add new search tab item to the tab control
                    tabCtrl.Items.Add(tabItem);

                    // Set it as the currently selected tab
                    tabCtrl.SelectedItem = tabItem;

                    // Perform the search - async response handled by the
                    // OnDataArrivedHandler(); we'll send the current tab
                    // as UserState for the async call
                    if (chkReset.IsChecked == true)
                        referenceDataService.SearchReset(searchString, tabItem);
                    else
                        referenceDataService.Search(searchString, tabItem);
                }
                else if (e.GetUniqueName().Contains("btnPromoteItem"))
                {
                    // The ButtonEventArgs will contain the search string (stored in in the
                    // button's tag prior to publishing search event)
                    string searchString = e.GetTag<string>();

                    Logger.Log(string.Format("{0} is handling EVENT {1}", ModuleFullName, e.GetUniqueName()),
                      Category.Debug, Priority.None);

                    // On search button click instantiate a new tab
                    SearchTabItem tabItem = Container.Resolve<SearchTabItem>();
                    
                    // Set the tab header to the search content
                    tabItem.HeaderText = searchString;

                    // Subscribe to close click event
                    tabItem.OnCloseClick += OnTabCloseHandler;

                    // Add new search tab item to the tab control
                    tabCtrl.Items.Add(tabItem);

                    // Set it as the currently selected tab
                    tabCtrl.SelectedItem = tabItem;

                    // Perform the search - async response handled by the
                    // OnDataArrivedHandler(); we'll send the current tab
                    // as UserState for the async call
                    referenceDataService.Find(searchString, tabItem);
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
            }
        }
        #endregion

        #region EVENT HANDLER: OnTabCloseHandler(object sender, RoutedEventArgs e)
        void OnTabCloseHandler(object sender, RoutedEventArgs e)
        {
            CustomTabItem tabItem = (CustomTabItem)sender;
            tabCtrl.Items.Remove(tabItem);
        }

        #endregion

        #region EVENT HANDLER: OnDataArrivedHandler(object sender, System.EventArgs e)
        /// <summary>
        /// Called when [data arrived handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void OnDataArrivedHandler(object sender, System.EventArgs e)
        {
            try
            {
                CompletedEventArgs args = (CompletedEventArgs)e;
                if (args.CheckForType(CompletedEventType.GetClassLabel) || args.CheckForType(CompletedEventType.GetRepositories))
                { }
                else
                {

                    ICommand command = args.GetUserState<ICommand>();
                    if (command != null && command.CanExecute(args))
                    {
                        command.Execute(args);
                    }
                    else
                    {
                        Logger.Log("Could not execute concrete class for " + args.CompletedType.ToString(), Category.Warn, Priority.Medium);
                    }
                }
                /*
                // :::::::::::::::::::::::::::::
                // See code in Factories folder
                // :::::::::::::::::::::::::::::

                // Instantiate our factory class
                InformationModelTreeFactory factory = Container.Resolve<InformationModelTreeFactory>();

                // Get concrete class from factory
                IInformationModelConcrete concrete =  factory.GetConcreteClass(args);

                // If concrete class provided, execute it - otherwise notify developer via output window
                if (concrete != null && concrete.CanExecute())
                    concrete.Execute();
                else
                    Logger.Log("Could not execute concrete class for " + args.CompletedType.ToString(), Category.Warn, Priority.Medium);
                */

            }
            catch (Exception ex)
            {
                Error.SetError(ex, ex.Message);
            }
        }
        #endregion

        #region EVENT HANDLER: tabSelectionChanged(object sender, SelectionChangedEventArgs e)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        void tabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // If tab is being closed then return
                if (e.AddedItems.Count == 0)
                    return;

                CustomTabItem currentTab = (CustomTabItem)
                  e.AddedItems[0] as CustomTabItem;

                if (currentTab == null)
                    return;

                // Activate the Tab - executes any required business logic in control's Activate()
                currentTab.Activate();
            }
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
            }
        }
        #endregion

    }
}
