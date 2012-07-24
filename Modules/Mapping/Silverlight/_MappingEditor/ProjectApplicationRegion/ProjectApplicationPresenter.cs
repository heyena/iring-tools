using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Composite.Logging;

using org.iringtools.modulelibrary.entities;
using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.extensions;
using org.iringtools.modulelibrary.types;

using org.iringtools.ontologyservice.presentation.presentationmodels;
using org.iringtools.informationmodel.events;
using PrismContrib.Base;

using org.iringtools.library;


using org.iringtools.library.configuration;
using org.iringtools.modules.medatasourceregion;
using org.iringtools.modules.memappingregion;

namespace org.iringtools.modules.projectapplicationregion
{
  public class ProjectApplicationPresenter : PresenterBase<ProjectApplicationView>
  {
    private ScopeProjects _scopes = null;
    private IEventAggregator _aggregator = null;
    private IAdapter _adapterProxy = null;
    private IIMPresentationModel _model = null;
    private IUnityContainer _container = null;

    private ComboBox prjCB { get { return ComboBoxCtrl("ProjectCombo"); } }
    private ComboBox appCB { get { return ComboBoxCtrl("AppCombo"); } }
   // private Button btnGenerate { get { return ButtonCtrl("btnGenerate"); } }
    private Button btnRefresh { get { return ButtonCtrl("btnRefresh"); } }
    
    public ProjectApplicationPresenter(
      IProjectApplicationView view, 
      IIMPresentationModel model,
      IEventAggregator aggregator,
      IAdapter adapterProxy,
      IUnityContainer container) : base(view, model)
    {
        try
        {
            _model = model;
            _aggregator = aggregator;
            _adapterProxy = adapterProxy;
            _container = container;

            prjCB.SelectionChanged += new SelectionChangedEventHandler(prjCB_SelectionChanged);
            appCB.SelectionChanged += new SelectionChangedEventHandler(appCB_SelectionChanged);
           // btnGenerate.Click += new RoutedEventHandler(btnGenerate_Click);
            btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            _adapterProxy.OnDataArrived += new EventHandler<EventArgs>(adapterProxy_OnDataArrived);
            _adapterProxy.GetScopes();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    void btnGenerate_Click(object sender, EventArgs e)
    {
        try
        {
            _adapterProxy.Generate((string)prjCB.SelectedItem, (string)appCB.SelectedItem);
        }
        catch (Exception ex)
        {
            
            throw ex;
        }
    }

    void btnRefresh_Click(object sender, EventArgs e)
    {
        try
        {
            _adapterProxy.RefreshAll((string)prjCB.SelectedItem, (string)appCB.SelectedItem);
        }
        catch (Exception ex)
        {
            
            throw ex;
        }
    }

    void adapterProxy_OnDataArrived(object sender, EventArgs e)
    {
        try
        {
            CompletedEventArgs args = e as CompletedEventArgs;

            if (args == null)
                return;

            if (args.CheckForType(CompletedEventType.GetScopes))
            {
                if (args.Error != null)
                {
                    MessageBox.Show(args.FriendlyErrorMessage, "Get Scopes Error", MessageBoxButton.OK);
                    return;
                }

                _scopes = (ScopeProjects)args.Data;

                foreach (ScopeProject project in _scopes)
                {
                    prjCB.Items.Add(project.Name);
                }

                prjCB.IsEnabled = true;
                appCB.IsEnabled = true;
            }
            else if (args.CheckForType(CompletedEventType.Generate))
            {
                if (args.Error != null)
                {
                    MessageBox.Show(args.FriendlyErrorMessage, "Generate DTO Error", MessageBoxButton.OK);
                    return;
                }

                Response response = (Response)args.Data;
                string messages = String.Empty;

                messages = response.ToString();

                MessageBox.Show(messages, "Generate DTO", MessageBoxButton.OK);
            }
            else if (args.CheckForType(CompletedEventType.RefreshAll))
            {
                if (args.Error != null)
                {
                    MessageBox.Show(args.FriendlyErrorMessage, "Refresh Facade Error", MessageBoxButton.OK);
                    return;
                }

                Response response = (Response)args.Data;
                string messages = String.Empty;

                messages = response.ToString();

                MessageBox.Show(messages, "Refresh Facade", MessageBoxButton.OK);
            }
        }
        catch (Exception ex)
        {
            Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace, 
                Category.Exception, Priority.High);
        }
    }

    void appCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            ComboBox appCB = (ComboBox)sender;
            string projName = (string)prjCB.SelectedItem;
            string appName = (string)appCB.SelectedItem;

            if (!String.IsNullOrEmpty(projName) && !String.IsNullOrEmpty(appName))
            {
                _adapterProxy.GetDictionary(projName, appName);
                _adapterProxy.GetMapping(projName, appName);

               // btnGenerate.IsEnabled = true;
                btnRefresh.IsEnabled = true;
            }
            else
            {
              //  btnGenerate.IsEnabled = false;
                btnRefresh.IsEnabled = false;
            }

            _aggregator.GetEvent<SelectionEvent>().Publish(new SelectionEventArgs
            {
                SelectedProject = (string)prjCB.SelectedItem,
                SelectedApplication = (string)appCB.SelectedItem
            });
        }
        catch (Exception ex)
        {
            Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace, 
                Category.Exception, Priority.High);
        }
    }

    void prjCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            ComboBox prjCB = (ComboBox)sender;

            foreach (ScopeProject project in _scopes)
            {
                if (project.Name == (string)prjCB.SelectedItem)
                {
                    appCB.Items.Clear();
                    //btnGenerate.IsEnabled = false;
                    btnRefresh.IsEnabled = false;

                    foreach (ScopeApplication app in project.Applications)
                    {
                        appCB.Items.Add(app.Name);
                    }

                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace, 
                Category.Exception, Priority.High);
        }
    }
  }
}
