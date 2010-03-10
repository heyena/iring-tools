using System;
//using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ServiceModel;
using System.Windows.Controls.Primitives;
using org.w3.sparql_results;
using org.iringtools.library;
using DemoControlPanel.DemoControlPanelService;
using System.Collections.Generic;

namespace DemoControlPanel
{
  public partial class MainPage : UserControl
  {
    Popup _popUp = new Popup();
    DemoControlPanelServiceClient _client = null;
    ConfiguredEndpoints _configEndpoints = null;

    public MainPage()
    {
      InitializeComponent();

      string uriScheme = Application.Current.Host.Source.Scheme;

      bool usingTransportSecurity = uriScheme.Equals("https", StringComparison.InvariantCultureIgnoreCase);

      BasicHttpSecurityMode securityMode;
      if (usingTransportSecurity)
        securityMode = BasicHttpSecurityMode.Transport;
      else
        securityMode = BasicHttpSecurityMode.None;

      BasicHttpBinding binding = new BasicHttpBinding(securityMode);
      binding.MaxReceivedMessageSize = int.MaxValue;
      binding.MaxBufferSize = int.MaxValue;
      TimeSpan timeout;
      TimeSpan.TryParse("00:10:00", out timeout);
      binding.OpenTimeout = timeout;
      binding.CloseTimeout = timeout;
      binding.ReceiveTimeout = timeout;
      binding.SendTimeout = timeout;

      Uri uri = new Uri(Application.Current.Host.Source, "../DemoControlPanelService.svc");

      _client = new DemoControlPanelServiceClient(binding, new EndpointAddress(uri));

      // Use the 'client' variable to call operations on the service.

      _client.GetConfiguredEndpointsCompleted += new EventHandler<GetConfiguredEndpointsCompletedEventArgs>(client_GetConfiguredEndpointsCompleted);
      _client.GetManifestCompleted += new EventHandler<GetManifestCompletedEventArgs>(client_GetManifestCompleted);
      _client.GetReceiverManifestCompleted += new EventHandler<GetReceiverManifestCompletedEventArgs>(client_GetReceiverManifestCompleted);
      _client.GetSenderManifestCompleted += new EventHandler<GetSenderManifestCompletedEventArgs>(client_GetSenderManifestCompleted);
      _client.RefreshCompleted += new EventHandler<RefreshCompletedEventArgs>(client_RefreshCompleted);
      _client.PullCompleted += new EventHandler<PullCompletedEventArgs>(client_PullCompleted);
      _client.QueryCompleted += new EventHandler<QueryCompletedEventArgs>(client_QueryCompleted);
      _client.ResetCompleted += new EventHandler<ResetCompletedEventArgs>(client_ResetCompleted);
      _client.UpdateCompleted += new EventHandler<UpdateCompletedEventArgs>(client_UpdateCompleted);
      _client.GetScopesCompleted += new EventHandler<GetScopesCompletedEventArgs>(client_GetScopesCompleted);
      //_client.GetInterfaceScopesCompleted += new EventHandler<GetInterfaceScopesCompletedEventArgs>(client_GetInterfaceScopesCompleted);

      _client.GetConfiguredEndpointsAsync();
    }

    private void client_GetConfiguredEndpointsCompleted(object sender, GetConfiguredEndpointsCompletedEventArgs eventArgs)
    {
      try
      {
        this.Cursor = Cursors.Wait;

        _configEndpoints = eventArgs.Result;

        cbxDemoScenarios.DisplayMemberPath = "scenarioName";
        cbxDemoScenarios.IsEnabled = false;
        cbxDemoScenarios.ItemsSource = _configEndpoints.scenarios;
        cbxDemoScenarios.SelectionChanged += new SelectionChangedEventHandler(cbxDemoScenarios_SelectionChanged);
        cbxDemoScenarios.IsEnabled = true;
        cbxDemoScenarios.SelectedIndex = 0;

        cbxAdapterServices.DisplayMemberPath = "name";
        cbxAdapterServices.IsEnabled = false;
        cbxAdapterServices.ItemsSource = _configEndpoints.adapterEndpoints;
        cbxAdapterServices.SelectionChanged += new SelectionChangedEventHandler(cbxAdapter_SelectionChanged);
        cbxAdapterServices.SelectedIndex = 0;
        cbxAdapterServices.IsEnabled = true;

        cbxInterfaceServices.DisplayMemberPath = "name";
        cbxInterfaceServices.IsEnabled = false;
        cbxInterfaceServices.ItemsSource = _configEndpoints.interfaceEndpoints;
        //cbxInterfaceServices.SelectionChanged += new SelectionChangedEventHandler(cbxInterfaceServices_SelectionChanged);
        cbxInterfaceServices.SelectedIndex = 0;
        cbxInterfaceServices.IsEnabled = true;

      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
      finally
      {
        this.Cursor = Cursors.Arrow;
      }
    }

    private void clearComboBox(ComboBox combox)
    {
      combox.IsEnabled = true;

      if (combox.ItemsSource != null)
      {
        combox.ItemsSource = null;
      }

      combox.IsEnabled = false;
    }

    private void cbxAdapter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        this.Cursor = Cursors.Wait;
        clearComboBox(cbxProjects);
        clearComboBox(cbxApplication);
        clearComboBox(cbxGraphs);
        
        iRINGEndpoint adapterEndpoint = (iRINGEndpoint)cbxAdapterServices.SelectedItem;

        _client.GetScopesAsync(adapterEndpoint);

      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
      finally
      {
        this.Cursor = Cursors.Arrow;
        cbxAdapterServices.IsEnabled = true;
      }
    }
          
    private void cbxDemoScenarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        cbxReceiverGraphName.Items.Clear();
        cbxSenderGraphName.Items.Clear();

        this.Cursor = Cursors.Wait;
        cbxDemoScenarios.IsEnabled = false;
        cbxSenderGraphName.IsEnabled = false;
        cbxReceiverGraphName.IsEnabled = false;
        Scenario scenario = (Scenario)cbxDemoScenarios.SelectedItem;
        tbSender.Text = scenario.sender;
        tbReceiver.Text = scenario.receiver;

        iRINGEndpoint receiverEndpoint = null;
        iRINGEndpoint senderEndpoint = null;
        iRINGEndpoint interfaceEndpoint = null;

        foreach (iRINGEndpoint endpoint in _configEndpoints.interfaceEndpoints)
        {
          if (scenario.interfaceServiceId == endpoint.id)
          {
            interfaceEndpoint = endpoint;
            break;
          }
        }

        foreach (iRINGEndpoint endpoint in _configEndpoints.adapterEndpoints)
        {
          if (scenario.receiverAdapterServiceId == endpoint.id)
          {
            receiverEndpoint = endpoint;
          }
          if (scenario.senderAdapterServiceId == endpoint.id)
          {
            senderEndpoint = endpoint;
          }
        }

        if (senderEndpoint != null && receiverEndpoint != null)
        {

          tbReceiverProjectName.Text = scenario.receiverProjectName;
          tbReceiverAppName.Text = scenario.receiverApplicationName;

          tbSenderProjectName.Text = scenario.senderProjectName;
          tbSenderAppName.Text = scenario.senderApplicationName;

          tbSenderUri.Text = senderEndpoint.serviceUri;
          tbReceiverUri.Text = receiverEndpoint.serviceUri;
          
          //btnReceiverExport.IsEnabled = scenario.exportEnabled;
          //btnSenderImport.IsEnabled = scenario.importEnabled;

          _client.GetReceiverManifestAsync(receiverEndpoint, scenario.receiverProjectName, scenario.receiverApplicationName);
          _client.GetSenderManifestAsync(senderEndpoint, scenario.senderProjectName, scenario.senderApplicationName);
        }

      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
      finally
      {
        this.Cursor = Cursors.Arrow;
        cbxDemoScenarios.IsEnabled = true;
      }
    }

    private void client_GetManifestCompleted(object sender, GetManifestCompletedEventArgs eventArgs)
    {
      try
      {
        this.Cursor = Cursors.Wait;
        cbxGraphs.IsEnabled = false;
        cbxGraphs.Items.Clear();

        foreach (string graphName in eventArgs.Result)
        {
          cbxGraphs.Items.Add(graphName);
        }

        cbxGraphs.SelectedIndex = 0;

      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
      }
      finally
      {
        this.Cursor = Cursors.Arrow;
        cbxAdapterServices.IsEnabled = true;
        cbxProjects.IsEnabled = true;
        cbxApplication.IsEnabled = true;
        cbxGraphs.IsEnabled = true;
      }
    }

    private void client_GetReceiverManifestCompleted(object sender, GetReceiverManifestCompletedEventArgs eventArgs)
    {
      try
      {
        this.Cursor = Cursors.Wait;
        Scenario scenario = (Scenario)cbxDemoScenarios.SelectedItem;

        DemoControlPanel.DemoControlPanelService.Response graphNames = eventArgs.Result;
        clearComboBox(cbxReceiverGraphName);
        cbxReceiverGraphName.Items.Clear();

        foreach (string graphName in eventArgs.Result)
        {
          cbxReceiverGraphName.Items.Add(graphName);
        }

        int idx = cbxReceiverGraphName.Items.IndexOf(scenario.senderGraphName);

        if (idx > -1)
        {
          cbxReceiverGraphName.SelectedIndex = idx;
        }
        else
        {
          cbxReceiverGraphName.IsEnabled = false;
        }

        if (cbxReceiverGraphName.Items.Contains(scenario.receiverGraphName))
        {
          cbxReceiverGraphName.SelectedItem = scenario.receiverGraphName;
        }

        //cbxReceiverGraphName.SelectedIndex = 0;
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
      }
      finally
      {
        this.Cursor = Cursors.Arrow;
        cbxReceiverGraphName.IsEnabled = true;
      }
    }

    private void client_GetSenderManifestCompleted(object sender, GetSenderManifestCompletedEventArgs eventArgs)
    {
      try
      {
        this.Cursor = Cursors.Wait;
        Scenario scenario = (Scenario)cbxDemoScenarios.SelectedItem;

        DemoControlPanel.DemoControlPanelService.Response graphNames = eventArgs.Result;
        clearComboBox(cbxSenderGraphName);
        cbxSenderGraphName.Items.Clear();

        foreach (string graphName in graphNames)
        {
          cbxSenderGraphName.Items.Add(graphName);
        }

        int idx = cbxSenderGraphName.Items.IndexOf(scenario.senderGraphName);

        if (idx > -1)
        {
          cbxSenderGraphName.SelectedIndex = idx;
        }
        else
        {
          cbxSenderGraphName.IsEnabled = false;
        }

        if (cbxSenderGraphName.Items.Contains(scenario.senderGraphName))
        {
          cbxSenderGraphName.SelectedItem = scenario.senderGraphName;
        }

      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
      }
      finally
      {
        this.Cursor = Cursors.Arrow;
        cbxSenderGraphName.IsEnabled = true;
      }
    }

    private void client_PullCompleted(object sender, PullCompletedEventArgs e)
    {
      try
      {
        Results _rPopUp = new Results();

        foreach (String resultString in e.Result)
        {
          _rPopUp.tblResults.Text += resultString + Environment.NewLine;
        }

        _rPopUp.FontSize = 12;
        _rPopUp.btnSaveResults.Visibility = Visibility.Collapsed;
        _popUp.Child = _rPopUp;
        _popUp.VerticalOffset = 64;
        _popUp.HorizontalOffset = 25;
        _popUp.IsOpen = true;
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
      finally
      {
        this.Cursor = Cursors.Arrow;
        //btnPull.IsEnabled = true;
        cbxAdapterServices.IsEnabled = true;
        cbxGraphs.IsEnabled = true;
        //cbInterfaceServices.IsEnabled = true;
        btnReceiverPull.IsEnabled = true;
      }
    }

    private void client_RefreshCompleted(object sender, RefreshCompletedEventArgs e)
    {
      try
      {
        Results _rPopUp = new Results();
        foreach (String resultString in e.Result)
        {
          _rPopUp.tblResults.Text += resultString + Environment.NewLine;
        }
        _rPopUp.FontSize = 12;
        _rPopUp.btnSaveResults.Visibility = Visibility.Collapsed;
        _popUp.Child = _rPopUp;
        _popUp.VerticalOffset = 64;
        _popUp.HorizontalOffset = 25;
        _popUp.IsOpen = true;
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
      }
      finally
      {
        this.Cursor = Cursors.Arrow;
        btnRefresh.IsEnabled = true;
        cbxAdapterServices.IsEnabled = true;
        cbxGraphs.IsEnabled = true;
        tbxIdentifier.IsEnabled = true;
        btnSenderRefresh.IsEnabled = true;
      }
    }

    private void client_QueryCompleted(object sender, QueryCompletedEventArgs e)
    {
      try
      {
        Results _rPopUp = new Results();
        ObservableCollection<ObservableCollection<SPARQLBinding>> sparqlResults;

        sparqlResults = e.Result;
        foreach (ObservableCollection<SPARQLBinding> bindings in sparqlResults)
        {
          foreach (SPARQLBinding binding in bindings)
          {
            if (binding.bnode != null)
            {
              _rPopUp.tblResults.Text += binding.bnode + " = " + binding.name + Environment.NewLine;
            }
            else if (binding.literal != null)
            {
              _rPopUp.tblResults.Text += binding.name + " = " + binding.literal.value + Environment.NewLine;
            }
            else if (binding.literal == null)
            {
              _rPopUp.tblResults.Text += binding.name + " = " + binding.uri + Environment.NewLine;
            }
          }
        }
        _rPopUp.FontSize = 12;
        _rPopUp.btnSaveResults.Visibility = Visibility.Collapsed;
        _popUp.Child = _rPopUp;
        _popUp.VerticalOffset = 64;
        _popUp.HorizontalOffset = 25;
        _popUp.IsOpen = true;
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
      finally
      {
        this.Cursor = Cursors.Arrow;
        btnQuery.IsEnabled = true;
        cbxInterfaceServices.IsEnabled = true;
      }
    }

    private void client_ResetCompleted(object sender, ResetCompletedEventArgs e)
    {
      try
      {
        Results _rPopUp = new Results();
        foreach (String resultString in e.Result)
        {
          _rPopUp.tblResults.Text += resultString + Environment.NewLine;
        }

        _rPopUp.FontSize = 12;
        _rPopUp.btnSaveResults.Visibility = Visibility.Collapsed;
        _popUp.Child = _rPopUp;
        _popUp.VerticalOffset = 64;
        _popUp.HorizontalOffset = 25;
        _popUp.IsOpen = true;
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
      finally
      {
        this.Cursor = Cursors.Arrow;
        //btnReset.IsEnabled = true;
        //btnSenderReset.IsEnabled = true;
        //btnReceiverReset.IsEnabled = true;
      }
    }

    private void client_UpdateCompleted(object sender, UpdateCompletedEventArgs e)
    {
      try
      {
        Results _rPopUp = new Results();
        foreach (String resultString in e.Result)
        {
          _rPopUp.tblResults.Text += resultString + Environment.NewLine;
        }

        _rPopUp.FontSize = 12;
        _rPopUp.btnSaveResults.Visibility = Visibility.Collapsed;
        _popUp.Child = _rPopUp;
        _popUp.VerticalOffset = 64;
        _popUp.HorizontalOffset = 25;
        _popUp.IsOpen = true;
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
      finally
      {
        this.Cursor = Cursors.Arrow;
        btnUpdate.IsEnabled = true;
        cbxInterfaceServices.IsEnabled = true;
      }
    }

    private void client_GetScopesCompleted(object sender, GetScopesCompletedEventArgs e)
    {
      try
      {
        cbxProjects.IsEnabled = false;
        cbxApplication.IsEnabled = false;

        ObservableCollection<ScopeProject> scopes = e.Result;

        if (scopes != null && scopes.Count > 0)
        {
          cbxProjects.DisplayMemberPath = "Name";
          cbxProjects.ItemsSource = scopes;
          cbxProjects.SelectionChanged += new SelectionChangedEventHandler(cbxProjects_SelectionChanged);
          cbxProjects.SelectedIndex = 0;
        }
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
      finally
      {
        this.Cursor = Cursors.Arrow;
        cbxProjects.IsEnabled = true;
        cbxApplication.IsEnabled = true;
      }
    }

    //private void client_GetInterfaceScopesCompleted(object sender, GetInterfaceScopesCompletedEventArgs e)
    //{
    //    try
    //    {
    //        cbxInterfaceProjects.IsEnabled = false;
    //        cbxInterfaceApplication.IsEnabled = false;

    //        ObservableCollection<ScopeProject> scopes = e.Result;

    //        if (scopes != null && scopes.Count > 0)
    //        {
    //            cbxInterfaceProjects.DisplayMemberPath = "Name";
    //            cbxInterfaceProjects.ItemsSource = scopes;
    //            cbxInterfaceProjects.SelectionChanged += new SelectionChangedEventHandler(cbxInterfaceProjects_SelectionChanged);
    //            cbxInterfaceProjects.SelectedIndex = 0;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
    //    }
    //    finally
    //    {
    //        this.Cursor = Cursors.Arrow;
    //        cbxInterfaceProjects.IsEnabled = true;
    //        cbxInterfaceApplication.IsEnabled = true;
    //    }
    //}

    private void cbxProjects_SelectionChanged(object sender, RoutedEventArgs e)
    {
      clearComboBox(cbxApplication);
      clearComboBox(cbxGraphs);

      if (cbxProjects.SelectedIndex != -1)
      {
        ScopeProject project = (ScopeProject)cbxProjects.SelectedItem;
        cbxApplication.DisplayMemberPath = "Name";
        cbxApplication.ItemsSource = project.Applications;
        cbxApplication.SelectionChanged += new SelectionChangedEventHandler(cbxApplication_SelectionChanged);
        cbxApplication.SelectedIndex = 0;
      }
    }

    //private void cbxInterfaceProjects_SelectionChanged(object sender, RoutedEventArgs e)
    //{
    //    clearComboBox(cbxInterfaceApplication);        

    //    if (cbxProjects.SelectedIndex != -1)
    //    {
    //        ScopeProject project = (ScopeProject)cbxInterfaceProjects.SelectedItem;
    //        cbxInterfaceApplication.DisplayMemberPath = "Name";
    //        cbxInterfaceApplication.ItemsSource = project.Applications;            
    //        cbxInterfaceApplication.SelectedIndex = 0;
    //    }
    //}

    private void cbxApplication_SelectionChanged(object sender, RoutedEventArgs e)
    {
      clearComboBox(cbxGraphs);

      if (cbxApplication.SelectedIndex != -1)
      {
        iRINGEndpoint adapterEndPoint = (iRINGEndpoint)cbxAdapterServices.SelectedItem;
        ScopeProject project = (ScopeProject)cbxProjects.SelectedItem;
        ScopeApplication application = (ScopeApplication)cbxApplication.SelectedItem;

        this.Cursor = Cursors.Wait;
        _client.GetManifestAsync(adapterEndPoint, project.Name, application.Name);
      }
    }

    private void btnSenderRefresh_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        this.Cursor = Cursors.Wait;
        btnSenderRefresh.IsEnabled = false;
        iRINGEndpoint adapterEndpoint = null;
        Scenario scenario = (Scenario)cbxDemoScenarios.SelectedItem;

        foreach (iRINGEndpoint endpoint in _configEndpoints.adapterEndpoints)
        {
          if (scenario.senderAdapterServiceId == endpoint.id)
          {
            adapterEndpoint = endpoint;
            break;
          }
        }

        string graphName = (string)cbxSenderGraphName.SelectedItem;
        _client.RefreshAsync(adapterEndpoint, scenario.senderProjectName, scenario.senderApplicationName, graphName, "");
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
      }
    }

    private void btnSenderImport_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        this.Cursor = Cursors.Wait;
        //btnSenderImport.IsEnabled = false;
        iRINGEndpoint adapterEndpoint = null;
        Scenario scenario = (Scenario)cbxDemoScenarios.SelectedItem;
        foreach (iRINGEndpoint endpoint in _configEndpoints.adapterEndpoints)
        {
          if (scenario.senderAdapterServiceId == endpoint.id)
          {
            adapterEndpoint = endpoint;
            break;
          }
        }
        string graphName = (string)cbxSenderGraphName.SelectedItem;
        _client.ExportAsync(adapterEndpoint, scenario.senderProjectName, scenario.senderApplicationName, graphName);
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
      }
    }

    private void btnSenderReset_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (System.Windows.Browser.HtmlPage.Window.Confirm("Are you sure you want to reset data at sender's endpoint?"))
        {
          this.Cursor = Cursors.Wait;
          //btnSenderReset.IsEnabled = false;
          iRINGEndpoint adapterEndpoint = null;
          Scenario scenario = (Scenario)cbxDemoScenarios.SelectedItem;
          foreach (iRINGEndpoint endpoint in _configEndpoints.adapterEndpoints)
          {
            if (scenario.senderAdapterServiceId == endpoint.id)
            {
              adapterEndpoint = endpoint;
              break;
            }
          }
          _client.ResetAsync(adapterEndpoint, scenario.senderProjectName, scenario.senderApplicationName);
        }
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
      }
    }

    private void btnReceiverPull_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        this.Cursor = Cursors.Wait;
        btnReceiverPull.IsEnabled = false;
        iRINGEndpoint adapterEndpoint = null;
        iRINGEndpoint interfaceEndpoint = null;
        Scenario scenario = (Scenario)cbxDemoScenarios.SelectedItem;
        foreach (iRINGEndpoint endpoint in _configEndpoints.adapterEndpoints)
        {
          if (scenario.receiverAdapterServiceId == endpoint.id)
          {
            adapterEndpoint = endpoint;
            break;
          }
        }
        foreach (iRINGEndpoint endpoint in _configEndpoints.interfaceEndpoints)
        {
          if (scenario.interfaceServiceId == endpoint.id)
          {
            interfaceEndpoint = endpoint;
            break;
          }
        }
                  
        //interfaceEndpoint.ProjectName = scenario.senderProjectName;
        //interfaceEndpoint.ApplicationName = scenario.senderApplicationName;
        string graphName = (string)cbxReceiverGraphName.SelectedItem;
        _client.PullAsync(scenario, adapterEndpoint, interfaceEndpoint, graphName);
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
      }
    }

    private void btnReceiverExport_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        this.Cursor = Cursors.Wait;
        //btnReceiverExport.IsEnabled = false;
        iRINGEndpoint adapterEndpoint = null;
        Scenario scenario = (Scenario)cbxDemoScenarios.SelectedItem;
        foreach (iRINGEndpoint endpoint in _configEndpoints.adapterEndpoints)
        {
          if (scenario.receiverAdapterServiceId == endpoint.id)
          {
            adapterEndpoint = endpoint;
            break;
          }
        }
        _client.ImportAsync(adapterEndpoint, scenario.receiverProjectName, scenario.receiverApplicationName);
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
      }
    }

    private void btnReceiverReset_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (System.Windows.Browser.HtmlPage.Window.Confirm("Are you sure you want to reset data at sender's endpoint?"))
        {
          this.Cursor = Cursors.Wait;
          //btnReceiverReset.IsEnabled = false;
          iRINGEndpoint adapterEndpoint = null;
          Scenario scenario = (Scenario)cbxDemoScenarios.SelectedItem;
          foreach (iRINGEndpoint endpoint in _configEndpoints.adapterEndpoints)
          {
            if (scenario.receiverAdapterServiceId == endpoint.id)
            {
              adapterEndpoint = endpoint;
              break;
            }
          }
          _client.ResetAsync(adapterEndpoint, scenario.receiverProjectName, scenario.receiverApplicationName);
        }
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
      }
    }

    private void btnQuery_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Scenario scenario = (Scenario)cbxDemoScenarios.SelectedItem;
        this.Cursor = Cursors.Wait;
        btnQuery.IsEnabled = false;
        cbxInterfaceServices.IsEnabled = false;
        string query = tbxQuery.Text;
        iRINGEndpoint interfaceEndpoint = (iRINGEndpoint)cbxInterfaceServices.SelectedItem;
        _client.QueryAsync(interfaceEndpoint, "", "", query);
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
    }

    private void btnUpdate_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Scenario scenario = (Scenario)cbxDemoScenarios.SelectedItem;
        this.Cursor = Cursors.Wait;
        btnUpdate.IsEnabled = false;
        cbxInterfaceServices.IsEnabled = false;
        string query = tbxQuery.Text;
        iRINGEndpoint interfaceEndpoint = (iRINGEndpoint)cbxInterfaceServices.SelectedItem;
        _client.UpdateAsync(interfaceEndpoint, scenario.receiverProjectName, scenario.receiverApplicationName, query);

      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
    }

    //private void cbxInterfaceServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //    try
    //    {
    //        this.Cursor = Cursors.Wait;
    //        clearComboBox(cbxInterfaceProjects);
    //        clearComboBox(cbxInterfaceApplication);            

    //        iRINGEndpoint interfaceEndpoint = (iRINGEndpoint)cbxInterfaceServices.SelectedItem;

    //        _client.GetInterfaceScopesAsync(interfaceEndpoint);

    //    }
    //    catch (Exception ex)
    //    {
    //        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
    //    }
    //    finally
    //    {
    //        this.Cursor = Cursors.Arrow;
    //        cbxInterfaceServices.IsEnabled = true;
    //    }
    //}
  }
}
