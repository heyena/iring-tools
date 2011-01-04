using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using org.iringtools.utility;
using System.Net;
using System.Collections.ObjectModel;
using org.iringtools.library;
using org.w3.sparql_results;
using System.Threading;
using VDS.RDF.Query;

namespace org.iringtools.utils.exchange
{
  /// <summary>
  /// Interaction logic for FacadeExchange.xaml
  /// </summary>
  public partial class FacadeExchange : Window
  {
    private ObservableCollection<StatusMessage> _messages = new ObservableCollection<StatusMessage>();
    Application _app = Application.Current;
    ScopeProject _project = null;
    ScopeApplication _application = null;
    GraphMap _graph = null;
    List<string> _graphUris = null;
    string _graphUri = String.Empty;

    public FacadeExchange()
    {
      InitializeComponent();

      listBoxResults.ItemsSource = _messages;
    }

    private void buttonPull_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = "Pulling Graph from remote Façade...", ImageName = "Resources/info_22.png" });

        WebClient client = new WebClient();

        client.Credentials = CredentialCache.DefaultCredentials;

        client.Proxy.Credentials = CredentialCache.DefaultCredentials;

        client.UploadStringCompleted += new UploadStringCompletedEventHandler(client_UploadStringCompleted);
        client.Headers["Content-type"] = "application/xml";
        client.Encoding = Encoding.UTF8;

        Uri pullURI = new Uri(
          textBoxAdapterURL.Text + "/" +
          _project.Name + "/" +
          _application.Name + "/" +
          _graph.name + "/pull");

        Request request = new Request();
        WebCredentials targetCredentials = new WebCredentials();
        string targetCredentialsXML = Utility.Serialize<WebCredentials>(targetCredentials, true);
        request.Add("targetUri", textBoxTargetURL.Text);
        request.Add("targetCredentials", targetCredentialsXML);
        request.Add("graphName", comboBoxGraphName.Text);
        request.Add("filter", "");

        string message = Utility.SerializeDataContract<Request>(request);

        client.UploadStringAsync(pullURI, message);
      }
      catch (Exception ex)
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
      }

    }

    private void buttonPublish_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = "Publishing Graph to own Façade...", ImageName = "Resources/info_22.png" });

        WebClient client = new WebClient();

        client.Credentials = CredentialCache.DefaultCredentials;

        client.Proxy.Credentials = CredentialCache.DefaultCredentials;

        WebProxy proxy = new WebProxy();
        proxy.UseDefaultCredentials = true;
        client.Proxy = proxy;

        client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);

        Uri refreshURI = new Uri(textBoxAdapterURL.Text + "/" + _project.Name + "/" + _application.Name + "/" + comboBoxGraphName.Text + "/refresh");

        client.DownloadStringAsync(refreshURI);
      }
      catch (Exception ex)
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
      }
    }

    void client_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
    {
      try
      {
        DisplayResults(e.Result);
      }
      catch (Exception ex)
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
      }
    }

    void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
      try
      {
        DisplayResults(e.Result);
      }
      catch (Exception ex)
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
      }
    }

    void DisplayResults(string result)
    {
      Response response = result.DeserializeDataContract<Response>();

      if (response != null && response.StatusList != null)
      {
        foreach (Status status in response.StatusList)
        {
          string imageName = "Resources/info_22.png";
          if (status.Level == StatusLevel.Error) imageName = "Resources/error_22.png";
          if (status.Level == StatusLevel.Success) imageName = "Resources/success_22.png";

          foreach (string message in status.Messages)
          {
            _messages.Add(new StatusMessage { Message = message, ImageName = imageName });
          }
        }

        listBoxResults.ScrollIntoView(listBoxResults.Items[listBoxResults.Items.Count - 1]);
      }
    }

    private void buttonConnect_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = "Fetching Scopes from Adapter...", ImageName = "Resources/info_22.png" });

        Uri scopesURI = new Uri(textBoxAdapterURL.Text + "/scopes");

        WebClient client = new WebClient();

        client.Credentials = CredentialCache.DefaultCredentials;

        client.Proxy.Credentials = CredentialCache.DefaultCredentials;

        client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_GetScopesCompleted);

        client.DownloadStringAsync(scopesURI);
      }
      catch (Exception ex)
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
      }
    }

    private void client_GetScopesCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
      try
      {
        ObservableCollection<ScopeProject> scopes = e.Result.DeserializeDataContract<ObservableCollection<ScopeProject>>();

        if (scopes != null && scopes.Count > 0)
        {
          comboBoxProjectName.DisplayMemberPath = "Name";
          comboBoxProjectName.ItemsSource = scopes;
          comboBoxProjectName.SelectionChanged += new SelectionChangedEventHandler(comboBoxProjectName_SelectionChanged);
          comboBoxProjectName.SelectedIndex = 0;

          comboBoxProjectName.IsEnabled = true;
          comboBoxAppName.IsEnabled = true;
        }

        _messages.Add(new StatusMessage
        {
          Message = "Successfully fetched " + scopes.Count +
            " scopes from Adapter.",
          ImageName = "Resources/success_22.png"
        });

        listBoxResults.ScrollIntoView(listBoxResults.Items[listBoxResults.Items.Count - 1]);
      }
      catch (Exception ex)
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
      }
    }

    private void client_GetMappingCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
      try
      {
        Mapping mapping = e.Result.DeserializeDataContract<Mapping>();

        if (mapping != null && mapping.graphMaps.Count > 0)
        {
          comboBoxGraphName.DisplayMemberPath = "name";
          comboBoxGraphName.ItemsSource = mapping.graphMaps;
          comboBoxGraphName.SelectionChanged += new SelectionChangedEventHandler(comboBoxGraphName_SelectionChanged);
          comboBoxGraphName.SelectedIndex = 0;
          comboBoxGraphName.IsEnabled = true;
        }
      }
      catch (Exception ex)
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
      }
    }

    private void comboBoxProjectName_SelectionChanged(object sender, RoutedEventArgs e)
    {
      try
      {
        if (comboBoxProjectName.SelectedIndex != -1)
        {
          _project = (ScopeProject)comboBoxProjectName.SelectedItem;
          comboBoxAppName.DisplayMemberPath = "Name";
          comboBoxAppName.ItemsSource = _project.Applications;
          comboBoxAppName.SelectionChanged += new SelectionChangedEventHandler(comboBoxAppName_SelectionChanged);
          comboBoxAppName.SelectedIndex = 0;
        }
      }
      catch (Exception ex)
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
      }
    }

    private void comboBoxAppName_SelectionChanged(object sender, RoutedEventArgs e)
    {
      try
      {
        _application = (ScopeApplication)comboBoxAppName.SelectedItem;

        if (_application != null && _application.Name != null && _application.Name != String.Empty)
        {
          WebClient client = new WebClient();

          client.Credentials = CredentialCache.DefaultCredentials;

          client.Proxy.Credentials = CredentialCache.DefaultCredentials;

          client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_GetMappingCompleted);

          Uri mappingURI = new Uri(textBoxAdapterURL.Text + "/" + _project.Name + "/" + _application.Name + "/mapping");

          client.DownloadStringAsync(mappingURI);
        }
      }
      catch (Exception ex)
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
      }
    }

    private void comboBoxGraphName_SelectionChanged(object sender, RoutedEventArgs e)
    {
      try
      {
        _graph = (GraphMap)comboBoxGraphName.SelectedItem;

        buttonPublish.IsEnabled = true;
      }
      catch (Exception ex)
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
      }
    }

    private void comboBoxGraphUri_SelectionChanged(object sender, RoutedEventArgs e)
    {
      try
      {
        _graphUri = comboBoxGraphUri.SelectedItem.ToString();

        buttonPull.IsEnabled = true;
      }
      catch (Exception ex)
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
      }
    }

    private void buttonExit_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _app.Shutdown();
      }
      catch (Exception ex)
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
      }
    }

    private void buttonFacadeConnect_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = "Fetching graphs from Façade...", ImageName = "Resources/info_22.png" });

        string sparql = "SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }";

        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(textBoxTargetURL.Text), "");

        SparqlResultSet results = endpoint.QueryWithResultSet(sparql);

        _graphUris = new List<string>();
        _graphUris.Add("[Default Graph]");
        foreach (SparqlResult result in results.Results)
        {
          string uri = result.Value("g").ToString();

          if (uri != null)
          {
            if (!uri.StartsWith("_:"))
            {
              _graphUris.Add(uri);
            }
          }
        }

        comboBoxGraphUri.ItemsSource = _graphUris;
        comboBoxGraphUri.SelectionChanged += new SelectionChangedEventHandler(comboBoxGraphUri_SelectionChanged);
        comboBoxGraphUri.SelectedIndex = 0;
        comboBoxGraphUri.IsEnabled = true;

        _messages.Add(new StatusMessage
        {
          Message = "Successfully fetched " + _graphUris.Count +
            " graphs from Façade.",
          ImageName = "Resources/success_22.png"
        });

        listBoxResults.ScrollIntoView(listBoxResults.Items[listBoxResults.Items.Count - 1]);
      }
      catch (Exception ex)
      {
        _messages.Clear();

        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
      }
    }

    public class StatusMessage
    {
      public string ImageName { get; set; }
      public string Message { get; set; }
    }
  }
}
