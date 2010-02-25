using System;
using System.Collections.Generic;
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
                cbxInterfaceServices.SelectionChanged += new SelectionChangedEventHandler(cbxInterface_SelectionChanged);
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

        private void cbxAdapter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                cbxAdapterServices.IsEnabled = false;
                iRINGEndpoint adapterEndpoint = (iRINGEndpoint)cbxAdapterServices.SelectedItem;

                cbxProjects.Items.Clear();
                cbxProjects.Items.Add(adapterEndpoint.projectName);
                cbxProjects.SelectedIndex = 0;

                cbxApplication.Items.Clear();
                cbxApplication.Items.Add(adapterEndpoint.applicationName);
                cbxApplication.SelectedIndex = 0;

                _client.GetManifestAsync(adapterEndpoint);
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

        private void cbxInterface_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
            }
        }

        private void cbxDemoScenarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                cbxDemoScenarios.IsEnabled = false;
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

                if ( senderEndpoint != null && receiverEndpoint != null ) {

                    tbReceiverProjectName.Text = receiverEndpoint.projectName;
                    tbReceiverAppName.Text = receiverEndpoint.applicationName;

                    tbSenderProjectName.Text = senderEndpoint.projectName;
                    tbSenderAppName.Text = senderEndpoint.applicationName;
                    
                    tbSenderUri.Text = receiverEndpoint.serviceUri;
                    tbReceiverUri.Text = senderEndpoint.serviceUri;

                    btnReceiverExport.IsEnabled = receiverEndpoint.exportEnabled;
                    btnSenderImport.IsEnabled = senderEndpoint.importEnabled;

                    _client.GetReceiverManifestAsync(receiverEndpoint);
                    _client.GetSenderManifestAsync(senderEndpoint);
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
                cbxGraphs.IsEnabled = true;
            }
        }

        private void client_GetReceiverManifestCompleted(object sender, GetReceiverManifestCompletedEventArgs eventArgs)
        {
            try
            {
                this.Cursor = Cursors.Wait;                
                cbxReceiverGraphName.IsEnabled = false;
                                                                
                foreach (string graphName in eventArgs.Result)
                {
                    cbxReceiverGraphName.Items.Add(graphName);
                }

                cbxReceiverGraphName.SelectedIndex = 0;                
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
                cbxSenderGraphName.IsEnabled = false;
                
                foreach (string graphName in eventArgs.Result)
                {
                    cbxSenderGraphName.Items.Add(graphName);
                }

                cbxSenderGraphName.SelectedIndex = 0;
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
                btnPull.IsEnabled = true;
                cbxAdapterServices.IsEnabled = true;
                cbxGraphs.IsEnabled = true;
                cbInterfaceServices.IsEnabled = true;
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
                List<List<SPARQLBinding>> sparqlResults;

                sparqlResults = e.Result;
                foreach (List<SPARQLBinding> bindings in sparqlResults)
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
                btnReset.IsEnabled = true;
                btnSenderReset.IsEnabled = true;
                btnReceiverReset.IsEnabled = true;
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
                _client.RefreshAsync(adapterEndpoint, graphName, "");
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
                btnSenderImport.IsEnabled = false;
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
                _client.ExportAsync(adapterEndpoint, graphName);
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
                    btnSenderReset.IsEnabled = false;
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
                    _client.ResetAsync(adapterEndpoint);
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
                string graphName = (string)cbxReceiverGraphName.SelectedItem;
                _client.PullAsync(adapterEndpoint, interfaceEndpoint, graphName);
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
                btnReceiverExport.IsEnabled = false;
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
                _client.ImportAsync(adapterEndpoint);
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
                    btnReceiverReset.IsEnabled = false;
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
                    _client.ResetAsync(adapterEndpoint);
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
                this.Cursor = Cursors.Wait;
                btnQuery.IsEnabled = false;
                cbxInterfaceServices.IsEnabled = false;
                string query = tbxQuery.Text;
                iRINGEndpoint interfaceEndpoint = (iRINGEndpoint)cbxInterfaceServices.SelectedItem;
                _client.QueryAsync(interfaceEndpoint, query);

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
                this.Cursor = Cursors.Wait;
                btnUpdate.IsEnabled = false;
                cbxInterfaceServices.IsEnabled = false;
                string query = tbxQuery.Text;
                iRINGEndpoint interfaceEndpoint = (iRINGEndpoint)cbxInterfaceServices.SelectedItem;
                _client.UpdateAsync(interfaceEndpoint, query);

            }
            catch (Exception ex)
            {
                System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
            }
        }        
    }        
}
