using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InformationModel.UserControls;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;
using ModuleLibrary.Events;
using ModuleLibrary.LayerDAL;
using OntologyService.Interface.PresentationModels;
using PrismContrib.Base;

namespace InformationModel.Views.MENavigationRegion
{
    /// <summary>
    /// Model Browser View presenter
    /// </summary>
    public class NavigationTreePresenter : PresenterBase<INavigationTreeView>
    {
        // Fields for class
        IEventAggregator aggregator = null;
        IReferenceData referenceDataService = null;
        IUnityContainer container = null;
        IIMPresentationModel model = null;
        private TabControl tabCtrl = null;

        #region PROPERTY: itcModelBrowser (ItemsControl - container for treeview)
        /// <summary>
        /// Gets the ItemsControl reference where the model treeview will reside
        /// </summary>
        /// <value>The TVW model browser.</value>
        private ItemsControl itcModelBrowser { get { return GetControl<ItemsControl>("itcModelBrowser"); } }
        #endregion
        #region PROPERTY: itcSpinner (ItemsControl - container for spinner)
        // BillKrat.2009.05.27 - added spinner support
        private ItemsControl itcSpinner { get { return GetControl<ItemsControl>("itcSpinner"); } }

        #endregion

        #region CONSTRUCTOR - configuration / setup 
        public NavigationTreePresenter(INavigationTreeView view, IIMPresentationModel model,
        IWorkingSpinner spinner,
        IEventAggregator aggregator,
        IReferenceData referenceDataService,
        IUnityContainer container)
            : base(view, model)
        {
            try
            {
                // BillKrat.2009.05.27 Add spinner
                // Controlled by BLL's service call and OnDataArrivedHandler
                //itcSpinner.Items.Add(spinner);

                // For class use
                this.referenceDataService = referenceDataService;
                this.aggregator = aggregator;
                this.container = container;
                this.model = model;

                // setup tab control - we want to be notified when tab
                // selection changes occur
                tabCtrl = new TabControl();
                tabCtrl.SelectionChanged += tabSelectionChanged;

                //GvR please change here <----
                tabCtrl.Width = 700;
                tabCtrl.Height = 225;

                // set margins and dynamically add treeview control to 
                // itcModelBrowser ItemsControl
                itcModelBrowser.Margin = new System.Windows.Thickness { Bottom = 4, Left = 4, Right = 4, Top = 4 };
                itcModelBrowser.Items.Add(tabCtrl);


                // Subscribe to button click events (search) 
                aggregator.GetEvent<ButtonEvent>().Subscribe(ButtonClickHandler);

                // Subscribe to data events (reference Data Service calls)
                referenceDataService.OnDataArrived += OnDataArrivedHandler;

            }
            catch (Exception ex)
            {
                Error.SetError(ex);
            }
        } 
        #endregion

        #region EVENT HANDLER: ButtonClickHandler(ButtonEventArgs e)  -- Search Button 
        /// <summary>
        /// Button Click Handler
        /// </summary>
        /// <param name="e">The <see cref="ModuleLibrary.Events.ButtonEventArgs"/> instance containing the event data.</param>
        public void ButtonClickHandler(ButtonEventArgs e)
        {
            // We're only handling the search button click - we 
            // don't care about others
            if (!e.GetUniqueName().Contains("MESearchPresenter:btnSearch"))
                return;

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
            referenceDataService.Search(searchString, tabItem);

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
                
                ICommand command = args.GetUserState<ICommand>();
                if (command.CanExecute(args))
                {
                  command.Execute(args);
                }
                else
                {
                  Logger.Log("Could not execute concrete class for " + args.CompletedType.ToString(), Category.Warn, Priority.Medium);
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
                Error.SetError(ex);
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
        #endregion


    }
}
