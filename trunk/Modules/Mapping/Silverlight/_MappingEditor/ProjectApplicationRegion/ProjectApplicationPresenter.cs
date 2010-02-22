using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;

using org.iringtools.modulelibrary.entities;
using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.extensions;
using org.iringtools.modulelibrary.types;

using org.iringtools.ontologyservice.presentation.presentationmodels;
using org.iringtools.informationmodel.events;
using PrismContrib.Base;

using InformationModel.Events;
using org.iringtools.library;


using org.iringtools.library.configuration;
using org.iringtools.modules.medatasourceregion;
using org.iringtools.modules.memappingregion;

namespace org.iringtools.modules.projectapplicationregion
{
  public class ProjectApplicationPresenter : PresenterBase<ProjectApplicationView>
  {
    private List<ScopeProject> _projects = null;
    private IEventAggregator _aggregator = null;
    private IAdapter _adapterProxy = null;
    private IIMPresentationModel _model = null;
    private IUnityContainer _container = null;

    private ComboBox prjCB { get { return ComboBoxCtrl("ProjectCombo"); } }
    private ComboBox appCB { get { return ComboBoxCtrl("AppCombo"); } }
    private Button btnGenerate { get { return ButtonCtrl("btnGenerate"); } }
    private Button btnRefresh { get { return ButtonCtrl("btnRefresh"); } }
    
    public ProjectApplicationPresenter(
      IProjectApplicationView view, 
      IIMPresentationModel model,
      IEventAggregator aggregator,
      IAdapter adapterProxy,
      IUnityContainer container) : base(view, model)
    {
      _model = model;
      _aggregator = aggregator;
      _adapterProxy = adapterProxy;
      _container = container;

      prjCB.SelectionChanged += new SelectionChangedEventHandler(prjCB_SelectionChanged);
      appCB.SelectionChanged += new SelectionChangedEventHandler(appCB_SelectionChanged);
      btnGenerate.Click += new RoutedEventHandler(btnGenerate_Click);
      btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

      _adapterProxy.OnDataArrived += new EventHandler<EventArgs>(adapterProxy_OnDataArrived);
      _adapterProxy.GetScopes();
    }

    void btnGenerate_Click(object sender, EventArgs e)
    {
      _adapterProxy.Generate((string)prjCB.SelectedItem, (string)appCB.SelectedItem);
    }

    void btnRefresh_Click(object sender, EventArgs e)
    {
      _adapterProxy.RefreshAll((string)prjCB.SelectedItem, (string)appCB.SelectedItem);
    }

    void adapterProxy_OnDataArrived(object sender, EventArgs e)
    {
      CompletedEventArgs args = e as CompletedEventArgs;

      if (args == null)
        return;

      if (args.CheckForType(CompletedEventType.GetScopes))
      {
        _projects = (List<ScopeProject>)args.Data;

        foreach (ScopeProject project in _projects)
        {
          prjCB.Items.Add(project.Name);
        }

        prjCB.IsEnabled = true;
        appCB.IsEnabled = true;
      }
      else if (args.CheckForType(CompletedEventType.Generate))
      {
        Response response = (Response)args.Data;
        string messages = String.Empty;

        foreach (string message in response)
        {
          messages += message + "\n";
        }

        MessageBox.Show(messages, "Generate DTO", MessageBoxButton.OK);
      }
      else if (args.CheckForType(CompletedEventType.RefreshAll))
      {
        Response response = (Response)args.Data;
        string messages = String.Empty;

        foreach (string message in response)
        {
          messages += message + "\n";
        }

        MessageBox.Show(messages, "Refresh Facade", MessageBoxButton.OK);
      }
    }

    void appCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ComboBox appCB = (ComboBox)sender;
      string projName = (string)prjCB.SelectedItem;
      string appName = (string)appCB.SelectedItem;

      if (!String.IsNullOrEmpty(projName) && !String.IsNullOrEmpty(appName))
      {
        _adapterProxy.GetDictionary(projName, appName);
        _adapterProxy.GetMapping(projName, appName);

        btnGenerate.IsEnabled = true;
        btnRefresh.IsEnabled = true;
      }
      else
      {
        btnGenerate.IsEnabled = false;
        btnRefresh.IsEnabled = false;
      }

      _aggregator.GetEvent<SelectionEvent>().Publish(new SelectionEventArgs
      {
          SelectedProject = (string)prjCB.SelectedItem,
          SelectedApplication = (string)appCB.SelectedItem
      });
    }

    void prjCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ComboBox prjCB = (ComboBox)sender;

      foreach (ScopeProject project in _projects)
      {
        if (project.Name == (string)prjCB.SelectedItem)
        {
          appCB.Items.Clear();
          btnGenerate.IsEnabled = false;
          btnRefresh.IsEnabled = false;

          foreach (ScopeApplication app in project.Applications)
          {
            appCB.Items.Add(app.Name);
          }

          return;
        }
      }
    }
  }
}
