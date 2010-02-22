// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Xml;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using ControlPanel.DemoService;
using System.IO;

namespace ControlPanel
{
  public partial class ControlPanelPage : UserControl
  {
    DemoServiceClient _demoServiceClient = null;
    Popup _popUp = new Popup();
    Config _config = null;
    iRINGEndpoint _scenarioReceiverAdapterEndPoint = null;
    iRINGEndpoint _scenarioSenderAdapterEndPoint = null;
    iRINGEndpoint _adapterEndPoint = null;
    
    public ControlPanelPage()
    {
      InitializeComponent();

      _demoServiceClient = ServiceUtility.GetDemoServiceClient();
      _demoServiceClient.PullCompleted += 
        new EventHandler<PullCompletedEventArgs>(_demoServiceClient_PullCompleted);
      _demoServiceClient.QueryCompleted += 
        new EventHandler<QueryCompletedEventArgs>(_demoServiceClient_QueryCompleted);
      _demoServiceClient.UpdateCompleted += 
        new EventHandler<UpdateCompletedEventArgs>(_demoServiceClient_UpdateCompleted);
      _demoServiceClient.GetQXFCompleted += 
        new EventHandler<GetQXFCompletedEventArgs>(_demoServiceClient_GetQXFCompleted);
      _demoServiceClient.GetDictionaryCompleted +=
        new EventHandler<GetDictionaryCompletedEventArgs>(_demoServiceClient_GetDictionaryCompleted);
      _demoServiceClient.GetSenderDictionaryCompleted +=
      new EventHandler<GetSenderDictionaryCompletedEventArgs>(_demoServiceClient_GetSenderDictionaryCompleted);
      _demoServiceClient.GetReceiverDictionaryCompleted +=
        new EventHandler<GetReceiverDictionaryCompletedEventArgs>(_demoServiceClient_GetReceiverDictionaryCompleted);
      _demoServiceClient.RefreshCompleted +=
        new EventHandler<RefreshCompletedEventArgs>(_demoServiceClient_RefreshCompleted);
      _demoServiceClient.GetConfigCompleted +=
        new EventHandler<GetConfigCompletedEventArgs>(_demoServiceClient_GetConfigCompleted);
      _demoServiceClient.ResetCompleted += 
        new EventHandler<ResetCompletedEventArgs>(_demoServiceClient_ResetCompleted);
      _demoServiceClient.GenerateCompleted +=
        new EventHandler<GenerateCompletedEventArgs>(_demoServiceClient_GenerateCompleted);
      _demoServiceClient.ExportCompleted += 
        new EventHandler<ExportCompletedEventArgs>(_demoServiceClient_ExportCompleted);
      _demoServiceClient.ImportCompleted += 
        new EventHandler<ImportCompletedEventArgs>(_demoServiceClient_ImportCompleted);
        
      _demoServiceClient.GetConfigAsync();
    }

    void _demoServiceClient_ImportCompleted(object sender, ImportCompletedEventArgs e)
    {
      try
      {
        ResultPopup _rPopUp = new ResultPopup();
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
        btnReceiverExport.IsEnabled = true;
      }
    }

    void _demoServiceClient_ExportCompleted(object sender, ExportCompletedEventArgs e)
    {
      try
      {
        ResultPopup _rPopUp = new ResultPopup();
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
        btnSenderImport.IsEnabled = true;
      }
    }

    #region Demo Service Client events
    private void _demoServiceClient_GetConfigCompleted(object sender, GetConfigCompletedEventArgs e)
    {
        try
        {
            _config = e.Result;
            List<Scenario> scenarios = new List<Scenario>();
            foreach (Scenario scenario in _config.scenarios)
            {
                scenarios.Add(new Scenario
                {
                    scenarioName = scenario.scenarioName,
                    interfaceServiceId = scenario.interfaceServiceId,
                    senderAdapterServiceId = scenario.senderAdapterServiceId,
                    receiverAdapterServiceId = scenario.receiverAdapterServiceId,
                    sender = scenario.sender,
                    receiver = scenario.receiver
                });
            }
            cbxDemoScenarios.DisplayMemberPath = "scenarioName";
            cbxDemoScenarios.IsEnabled = false;
            cbxDemoScenarios.ItemsSource = scenarios;
            cbxDemoScenarios.SelectionChanged += new SelectionChangedEventHandler(cbxDemoScenarios_SelectionChanged);
            cbxDemoScenarios.IsEnabled = true;
            cbxDemoScenarios.SelectedIndex = 0;

            List<iRINGEndpoint> adapterServers = new List<iRINGEndpoint>();
            foreach (iRINGEndpoint endpoint in _config.adapterEndpoints)
            {
                adapterServers.Add(endpoint);
            }
            cbxAdapterServices.DisplayMemberPath = "name";
            cbxAdapterServices.IsEnabled = false;
            cbxAdapterServices.ItemsSource = adapterServers;            
            cbxAdapterServices.SelectionChanged += new SelectionChangedEventHandler(cbxAdapter_SelectionChanged);
            cbxAdapterServices.SelectedIndex = 0;
            cbxAdapterServices.IsEnabled = true;
            _adapterEndPoint = (iRINGEndpoint)cbxAdapterServices.SelectedItem;

            List<iRINGEndpoint> interfaceServers = new List<iRINGEndpoint>();
            foreach (iRINGEndpoint endpoint in _config.interfaceEndpoints)
            {
                interfaceServers.Add(endpoint);
            }
            cbxInterfaceServices.DisplayMemberPath = "name";
            cbxInterfaceServices.IsEnabled = false;
            cbxInterfaceServices.ItemsSource = interfaceServers;            
            cbxInterfaceServices.SelectionChanged += new SelectionChangedEventHandler(cbxInterface_SelectionChanged);
            cbxInterfaceServices.SelectedIndex = 0;
            cbxInterfaceServices.IsEnabled = true;
            iRINGEndpoint interfaceEndpoint = (iRINGEndpoint)cbxInterfaceServices.SelectedItem;

            cbInterfaceServices.DisplayMemberPath = "name";
            cbInterfaceServices.IsEnabled = false;
            cbInterfaceServices.ItemsSource = interfaceServers;            
            cbInterfaceServices.SelectionChanged += new SelectionChangedEventHandler(cbInterface_SelectionChanged);
            cbInterfaceServices.SelectedIndex = 0;
            cbInterfaceServices.IsEnabled = true;

            tabFunctions.SelectionChanged += new SelectionChangedEventHandler(tabFunctions_SelectionChanged);
            tabFunctions.SelectedIndex = 0;
            //Scenario selectedScenario = (Scenario)cbxDemoScenarios.SelectedItem;
            //tbSender.Text = selectedScenario.sender;
            //tbReceiver.Text = selectedScenario.receiver;
            //foreach (iRINGEndpoint endpoint in _config.adapterEndpoints)
            //{
            //    if (selectedScenario.receiverAdapterServiceId == endpoint.id)
            //    {
            //        _scenarioAdapterEndPoint = endpoint;
            //    }
            //}
            //tabFunctions.SelectionChanged += new SelectionChangedEventHandler(tabFunctions_SelectionChanged);
            //tabFunctions.SelectedIndex = 0;

            //_demoServiceClient.GetDictionaryAsync(_scenarioAdapterEndPoint);
        }
        catch (Exception ex)
        {
            System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
        }
        finally
        {
            this.Cursor = Cursors.Arrow;
        }
    }

    private void _demoServiceClient_GetDictionaryCompleted(object sender, GetDictionaryCompletedEventArgs e)
    {
        try
        {            
            List<GraphItems> graphs = new List<GraphItems>();

            foreach (String graph in e.Result)
            {
                graphs.Add(new GraphItems { GraphName = graph });
            }
            if (graphs.Count > 0)
            {
                cbxGraphs.DisplayMemberPath = "GraphName";
                cbxGraphs.IsEnabled = false;
                cbxGraphs.ItemsSource = graphs;
                cbxGraphs.SelectionChanged += new SelectionChangedEventHandler(cbxGraphs_SelectionChanged);
                cbxGraphs.IsEnabled = true;  
                cbxGraphs.SelectedIndex = 0;
            }

        }
        catch (Exception ex)
        {
            System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
        }
        finally
        {
            this.Cursor = Cursors.Arrow;
            cbxAdapterServices.IsEnabled = true;
        }
    }

    private void _demoServiceClient_GetSenderDictionaryCompleted(object sender, GetSenderDictionaryCompletedEventArgs e)
    {
        try
        {
            List<GraphItems> graphs = new List<GraphItems>();

            foreach (String graph in e.Result)
            {
                graphs.Add(new GraphItems { GraphName = graph });
            }
            if (graphs.Count > 0)
            {
                cbxSenderGraphName.DisplayMemberPath = "GraphName";
                cbxSenderGraphName.IsEnabled = false;
                cbxSenderGraphName.ItemsSource = graphs;
                cbxSenderGraphName.SelectionChanged += new SelectionChangedEventHandler(cbxDemoGraphName_SelectionChanged);
                cbxSenderGraphName.IsEnabled = true;
                cbxSenderGraphName.SelectedIndex = 0;                           
            }

        }
        catch (Exception ex)
        {
            System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
        }
        finally
        {
            this.Cursor = Cursors.Arrow;
            cbxDemoScenarios.IsEnabled = true;
        }
    }

    private void _demoServiceClient_GetReceiverDictionaryCompleted(object sender, GetReceiverDictionaryCompletedEventArgs e)
    {
        try
        {
            List<GraphItems> graphs = new List<GraphItems>();

            foreach (String graph in e.Result)
            {
                graphs.Add(new GraphItems { GraphName = graph });
            }
            if (graphs.Count > 0)
            {
                cbxReceiverGraphName.DisplayMemberPath = "GraphName";
                cbxReceiverGraphName.IsEnabled = false;
                cbxReceiverGraphName.ItemsSource = graphs;
                cbxReceiverGraphName.SelectionChanged += new SelectionChangedEventHandler(cbxDemoGraphName_SelectionChanged);
                cbxReceiverGraphName.IsEnabled = true;
                cbxReceiverGraphName.SelectedIndex = 0;
            }

        }
        catch (Exception ex)
        {
            System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
        }
        finally
        {
            this.Cursor = Cursors.Arrow;
            cbxDemoScenarios.IsEnabled = true;
        }
    }

    private void _demoServiceClient_PullCompleted(object sender, PullCompletedEventArgs e)
    {
        try
        {
            ResultPopup _rPopUp = new ResultPopup();

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

    private void _demoServiceClient_RefreshCompleted(object sender, RefreshCompletedEventArgs e)
    {
        try
        {
            ResultPopup _rPopUp = new ResultPopup();
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

    private void _demoServiceClient_QueryCompleted(object sender, QueryCompletedEventArgs e)
    {
      try
      {
        ResultPopup _rPopUp = new ResultPopup();
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

    private void _demoServiceClient_UpdateCompleted(object sender, UpdateCompletedEventArgs e)
    {
      try
      {
        ResultPopup _rPopUp = new ResultPopup();
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

    private void _demoServiceClient_ResetCompleted(object sender, ResetCompletedEventArgs e)
    {
        try
        {
            ResultPopup _rPopUp = new ResultPopup();
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

    private void _demoServiceClient_GenerateCompleted(object sender, GenerateCompletedEventArgs e)
    {
        try
        {
            ResultPopup _rPopUp = new ResultPopup();
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
            btnGenerate.IsEnabled = true;
        }
    }

    private void _demoServiceClient_GetQXFCompleted(object sender, GetQXFCompletedEventArgs e)
    {
      try
      {
        ResultPopup _rPopUp = new ResultPopup();
        foreach (string resultString in e.Result)
        {
          _rPopUp.tblResults.Text += resultString;
        }
        _rPopUp.FontSize = 12;
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
          btnUploadQXF.IsEnabled = true;
      }
    }
    
    #endregion

    #region Button click events

    #region Demo tab
    private void btnSenderRefresh_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            this.Cursor = Cursors.Wait;
            btnSenderRefresh.IsEnabled = false;            
            iRINGEndpoint adapterEndpoint = null;
            Scenario scenario = (Scenario)cbxDemoScenarios.SelectedItem;
            foreach (iRINGEndpoint endpoint in _config.adapterEndpoints)
            {
                if (scenario.senderAdapterServiceId == endpoint.id)
                {
                    adapterEndpoint = endpoint;
                }
            }
            GraphItems graph = (GraphItems)cbxSenderGraphName.SelectedItem;
            _demoServiceClient.RefreshAsync(adapterEndpoint, graph.GraphName, tbxIdentifier.Text);
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
                foreach (iRINGEndpoint endpoint in _config.adapterEndpoints)
                {
                    if (scenario.senderAdapterServiceId == endpoint.id)
                    {
                        adapterEndpoint = endpoint;
                    }
                }
                _demoServiceClient.ResetAsync(adapterEndpoint);
            }
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
        foreach (iRINGEndpoint endpoint in _config.adapterEndpoints)
        {
          if (scenario.senderAdapterServiceId == endpoint.id)
          {
            adapterEndpoint = endpoint;
          }
        }
        GraphItems graph = (GraphItems)cbxSenderGraphName.SelectedItem;
        _demoServiceClient.ExportAsync(adapterEndpoint, graph.GraphName);
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
            foreach (iRINGEndpoint endpoint in _config.adapterEndpoints)
            {
              if (scenario.receiverAdapterServiceId == endpoint.id)
              {
                adapterEndpoint = endpoint;
              }
            }  
            foreach (iRINGEndpoint endpoint in _config.interfaceEndpoints)
            {
              if (scenario.interfaceServiceId == endpoint.id)
              {
                  interfaceEndpoint = endpoint;
              }
            }
            GraphItems graph = (GraphItems)cbxReceiverGraphName.SelectedItem;
            _demoServiceClient.PullAsync(adapterEndpoint, interfaceEndpoint, graph.GraphName);
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
                foreach (iRINGEndpoint endpoint in _config.adapterEndpoints)
                {
                    if (scenario.receiverAdapterServiceId == endpoint.id)
                    {
                        adapterEndpoint = endpoint;
                    }
                }
                _demoServiceClient.ResetAsync(adapterEndpoint);
            }
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
        foreach (iRINGEndpoint endpoint in _config.adapterEndpoints)
        {
          if (scenario.receiverAdapterServiceId == endpoint.id)
          {
            adapterEndpoint = endpoint;
          }
        }
        _demoServiceClient.ImportAsync(adapterEndpoint);
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.ToString());
      }
    }
    #endregion

    #region Adapter Service tab
    private void btnPull_Click(object sender, RoutedEventArgs e)
    {
      try
      {
          this.Cursor = Cursors.Wait;
          btnPull.IsEnabled = false;
          cbxAdapterServices.IsEnabled = false;
          cbxGraphs.IsEnabled = false;
          cbInterfaceServices.IsEnabled = false;
        iRINGEndpoint adapterEndpoint = (iRINGEndpoint)cbxAdapterServices.SelectedItem;
        iRINGEndpoint targetEndpoint = (iRINGEndpoint)cbInterfaceServices.SelectedItem;
        GraphItems graph = (GraphItems)cbxGraphs.SelectedItem;
        _demoServiceClient.PullAsync(adapterEndpoint, targetEndpoint, graph.GraphName);
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
    }

    private void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            this.Cursor = Cursors.Wait;
            cbxAdapterServices.IsEnabled = false;
            cbxGraphs.IsEnabled = false;
            tbxIdentifier.IsEnabled = false;
            btnRefresh.IsEnabled = false;
            iRINGEndpoint adapterEndpoint = (iRINGEndpoint)cbxAdapterServices.SelectedItem;
            GraphItems graph = (GraphItems)cbxGraphs.SelectedItem;
            _demoServiceClient.RefreshAsync(adapterEndpoint, graph.GraphName, tbxIdentifier.Text);
        }
        catch (Exception ex)
        {
            System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
        }
    }

    private void btnUploadQXF_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            this.Cursor = Cursors.Wait;
            btnUploadQXF.IsEnabled = false;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "XML files (*.xml)|*.xml";
            if ((bool)dlg.ShowDialog())
            {
                FileStream fs = dlg.File.OpenRead();
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (Int32)(fs.Length));
                _demoServiceClient.GetQXFAsync(buffer);
            }
            else
            {
                //tbxStatus.Text = "No file selected...";
                ResultPopup _rPopUp = new ResultPopup();
                _rPopUp.tblResults.Text = "No file selected...";
                _rPopUp.btnSaveResults.Visibility = Visibility.Collapsed;
                _rPopUp.FontSize = 12;
                _popUp.Child = _rPopUp;
                _popUp.VerticalOffset = 64;
                _popUp.HorizontalOffset = 25;
                _popUp.IsOpen = true;

            }
        }
        catch (Exception ex)
        {
            System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
        }
    }

    private void btnDownloadQXF_Click(object sender, RoutedEventArgs e)
    {

    }

    private void btnBrowse_Click(object sender, RoutedEventArgs e)
    {
        String fileName = string.Empty;
        try
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "XML files (*.xml)|*.xml";
            if ((bool)dlg.ShowDialog())
            {
                fileName = dlg.File.Name;
                ////TODO
            }
            else
            {
                //tbxStatus.Text = "No file selected...";
                ResultPopup _rPopUp = new ResultPopup();
                _rPopUp.tblResults.Text = "No file selected...";
                _rPopUp.btnSaveResults.Visibility = Visibility.Collapsed;
                _rPopUp.FontSize = 12;
                _popUp.Child = _rPopUp;
                _popUp.VerticalOffset = 64;
                _popUp.HorizontalOffset = 25;
                _popUp.IsOpen = true;

            }
        }
        catch (Exception ex)
        {
            System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
        }

    }

    private void btnReset_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (System.Windows.Browser.HtmlPage.Window.Confirm("Are you sure you want to reset data?"))
            {
                this.Cursor = Cursors.Wait;
                btnReset.IsEnabled = false;
                iRINGEndpoint adapterEndpoint = (iRINGEndpoint)cbxAdapterServices.SelectedItem;
                GraphItems graph = (GraphItems)cbxGraphs.SelectedItem;
                _demoServiceClient.ResetAsync(adapterEndpoint);
            }
        }
        catch (Exception ex)
        {
            System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
        }
    }

    private void btnGenerate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            this.Cursor = Cursors.Wait;
            btnGenerate.IsEnabled = false;
            iRINGEndpoint adapterEndpoint = (iRINGEndpoint)cbxAdapterServices.SelectedItem;
            GraphItems graph = (GraphItems)cbxGraphs.SelectedItem;
            _demoServiceClient.GenerateAsync(adapterEndpoint);        
        }
        catch (Exception ex)
        {
            System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
        }
    }
    #endregion

    #region Interface Service tab
    private void btnQuery_Click(object sender, RoutedEventArgs e)
    {
      try
      {
          this.Cursor = Cursors.Wait;
          btnQuery.IsEnabled = false;
          cbxInterfaceServices.IsEnabled = false;
        string query = tbxQuery.Text;
        iRINGEndpoint interfaceEndpoint = (iRINGEndpoint)cbxInterfaceServices.SelectedItem;
        _demoServiceClient.QueryAsync(interfaceEndpoint, query);
        
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
        _demoServiceClient.UpdateAsync(interfaceEndpoint, query);
        
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
    }
    #endregion

    #endregion

    #region Selection change events
    private void cbxAdapter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
          this.Cursor = Cursors.Wait;
        cbxAdapterServices.IsEnabled = false;
        iRINGEndpoint adapterEndpoint = (iRINGEndpoint)cbxAdapterServices.SelectedItem;
        _demoServiceClient.GetDictionaryAsync(adapterEndpoint);
      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
    }

    private void cbxInterface_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void cbxGraphs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void cbxDemoGraphName_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
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

            if (scenario.scenarioName == "Data Flow 5A: DuPont -> Bechtel" || scenario.scenarioName == "Test Data Flow: Me -> Me")
            {
              btnReceiverExport.IsEnabled = true;
              btnSenderImport.IsEnabled = true;
            }
            else
            {
              btnReceiverExport.IsEnabled = false;
              btnSenderImport.IsEnabled = false;
            }

            string uri = String.Empty;
            foreach (iRINGEndpoint endpoint in _config.interfaceEndpoints)
            {
              if (scenario.interfaceServiceId == endpoint.id)
              {
                uri = endpoint.serviceUri;
              }
            }
            tbSenderUri.Text = uri;

            uri = String.Empty;
            foreach (iRINGEndpoint endpoint in _config.adapterEndpoints)
            {
              if (scenario.receiverAdapterServiceId == endpoint.id)
              {
                  _scenarioReceiverAdapterEndPoint = endpoint;
                  uri = _scenarioReceiverAdapterEndPoint.serviceUri;
              }
              if (scenario.senderAdapterServiceId == endpoint.id)
              {
                  _scenarioSenderAdapterEndPoint = endpoint;
              }
              tbReceiverUri.Text = uri;
            }
            _demoServiceClient.GetReceiverDictionaryAsync(_scenarioReceiverAdapterEndPoint);
            _demoServiceClient.GetSenderDictionaryAsync(_scenarioSenderAdapterEndPoint);

            if (scenario.scenarioName == "Data Flow 2C – Dow to Intergraph")
            {
                btnSenderImport.IsEnabled = true;
                btnReceiverExport.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
        }       
    }

    private void cbInterface_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void tabFunctions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (tabFunctions.SelectedIndex == 0)
            {
                _demoServiceClient.GetReceiverDictionaryAsync(_scenarioReceiverAdapterEndPoint);
                _demoServiceClient.GetSenderDictionaryAsync(_scenarioSenderAdapterEndPoint);
            }
            else if (tabFunctions.SelectedIndex == 2)
            {
                _demoServiceClient.GetDictionaryAsync(_adapterEndPoint);
            }
        }
        catch (Exception ex)
        {
            System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
        }
    }
    #endregion
  }
 
  public class GraphItems
  {
    public String GraphName { get; set; }
  }
 
}

