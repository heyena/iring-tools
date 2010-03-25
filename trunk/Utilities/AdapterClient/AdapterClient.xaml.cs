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

namespace IDS_ADI.iRING.Adapter
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class AdapterClient : Window
  {
    private ObservableCollection<StatusMessage> _messages = new ObservableCollection<StatusMessage> 
    { 
      //new StatusMessage { Message = "iRING Adapter Client 1.01.00", ImageName = "iring_22.png" },
    };

    ScopeProject _project = null;
    ScopeApplication _application = null;

    public AdapterClient()
    {
      InitializeComponent();

      listBoxResults.ItemsSource = _messages;

      
    }

    private void buttonPull_Click(object sender, RoutedEventArgs e)
    {
      _messages.Add(new StatusMessage { Message = "Posting Pull Request...", ImageName = "info_22.png" });

      WebClient client = new WebClient();
      client.UploadStringCompleted += new UploadStringCompletedEventHandler(client_UploadStringCompleted);
      client.Headers["Content-type"] = "application/xml";
      client.Encoding = Encoding.UTF8;
      
      Uri pullURI = new Uri(textBoxAdapterURL.Text + "/" + _project.Name + "/" + _application.Name + "/pull");

      Request request = new Request();
      WebCredentials targetCredentials = new WebCredentials();
      string targetCredentialsXML = Utility.Serialize<WebCredentials>(targetCredentials, true);
      request.Add("targetUri", textBoxTargetURL.Text + "/" + _project.Name + "/" + _application.Name + "/sparql");
      request.Add("targetCredentials", targetCredentialsXML);
      request.Add("graphName", textBoxGraphName.Text);
      request.Add("filter", "");

      string message = Utility.SerializeDataContract<Request>(request);

      client.UploadStringAsync(pullURI, message);
    }

    private void buttonClear_Click(object sender, RoutedEventArgs e)
    {
      _messages.Add(new StatusMessage { Message = "Posting Clear Request...", ImageName = "info_22.png" });

      WebClient client = new WebClient();
      client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);

      Uri clearURI = new Uri(textBoxAdapterURL.Text + "/" + _project.Name + "/" + _application.Name + "/clear");

      client.DownloadStringAsync(clearURI);
    }

    private void buttonClearLog_Click(object sender, RoutedEventArgs e)
    {
      _messages.Clear();
    }

    private void buttonRefresh_Click(object sender, RoutedEventArgs e)
    {
      WebClient client = new WebClient();
      client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);

      Uri refreshURI = new Uri(textBoxAdapterURL.Text + "/" + _project.Name + "/" + _application.Name + "/" + textBoxGraphName.Text + "/refresh");

      client.DownloadStringAsync(refreshURI);
    }

    void client_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
    {
      try
      {
        DisplayResults(e.Result);
      }
      catch(Exception ex)
      {
        _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "error_22.png" });
      }
    }

    void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
      DisplayResults(e.Result);
    }

    void DisplayResults(string result)
    {
      Response response = result.DeserializeDataContract<Response>();

      foreach (string message in response)
      {
        string imageName = "info_22.png";
        if (message.Contains("error")) imageName = "error_22.png";
        if (message.Contains("success")) imageName = "success_22.png";

        _messages.Add(new StatusMessage { Message = message, ImageName = imageName });
      }

      listBoxResults.ScrollIntoView(listBoxResults.Items[listBoxResults.Items.Count - 1]);
    }

    private void buttonGetScopes_Click(object sender, RoutedEventArgs e)
    {
      WebClient client = new WebClient();
      client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadScopesCompleted);

      Uri scopesURI = new Uri(textBoxAdapterURL.Text + "/scopes");

      client.DownloadStringAsync(scopesURI);      
      
    }

    private void client_DownloadScopesCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
      ObservableCollection<ScopeProject> scopes = e.Result.DeserializeDataContract<ObservableCollection<ScopeProject>>();

      if (scopes != null && scopes.Count > 0)
      {
        comboBoxProjectName.DisplayMemberPath = "Name";
        comboBoxProjectName.ItemsSource = scopes;
        comboBoxProjectName.SelectionChanged += new SelectionChangedEventHandler(comboBoxProjectName_SelectionChanged);
        comboBoxProjectName.SelectedIndex = 0;
      }
    }

    private void comboBoxProjectName_SelectionChanged(object sender, RoutedEventArgs e)
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

    private void comboBoxAppName_SelectionChanged(object sender, RoutedEventArgs e)
    {
      _application = (ScopeApplication)comboBoxAppName.SelectedItem;
    }


    //private void buttonDictionaryGenerate_Click(object sender, RoutedEventArgs e)
    //{
    //  try
    //  {
    //    _messages.Add(new StatusMessage { Message = "Posting Generate Dictionary Request...", ImageName = "info_22.png" });

    //    WebClient client = new WebClient();
    //    client.UploadStringCompleted += new UploadStringCompletedEventHandler(client_UploadStringCompleted);
    //    client.Headers["Content-type"] = "application/xml";
    //    client.Encoding = Encoding.UTF8;

    //    Uri dictionaryGenerateURI = new Uri(textBoxAdapterURL.Text + "/dictionary/generate");

    //    #region make it
    //    //DatabaseDictionary myDatabaseDictionary = new DatabaseDictionary();

    //    //myDatabaseDictionary.providerName = "CrazyProvider";
    //    //myDatabaseDictionary.dataObjects = new List<DatabaseObject>
    //    //{
    //    //  new DatabaseObject 
    //    //  {
    //    //    dataProperties = new List<DatabaseProperty>
    //    //    {
    //    //      new DatabaseProperty
    //    //      {
    //    //        dataLength = "32",
    //    //        dataType = "string",
    //    //        isPropertyKey = true,
    //    //        isRequired = true,
    //    //        propertyName = "szTag",
    //    //      },
    //    //    },
    //    //    objectName = "I_Line",
    //    //    objectNamespace = "Crazy",
    //    //  },
    //    //  new DatabaseObject 
    //    //  {
    //    //    dataProperties = new List<DatabaseProperty>
    //    //    {
    //    //      new DatabaseProperty
    //    //      {
    //    //        dataLength = "32",
    //    //        dataType = "string",
    //    //        isPropertyKey = true,
    //    //        isRequired = true,
    //    //        propertyName = "szTag",
    //    //      },
    //    //    },
    //    //    objectName = "I_Valve",
    //    //    objectNamespace = "Crazy",
    //    //  },
    //    //};
    //    #endregion

    //    //Utility.Write<DatabaseDictionary>(myDatabaseDictionary, "MyDatabasedictioanry.xml", true);

    //    DatabaseDictionary databaseDictionary = Utility.Read<DatabaseDictionary>("databaseDictionary.xml", true);

    //    string message = Utility.Serialize<DatabaseDictionary>(databaseDictionary, true);

    //    client.UploadStringAsync(dictionaryGenerateURI, message);
    //  }
    //  catch (Exception ex)
    //  {
    //    _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "error_22.png" });
    //  }
    //}
  }

  public class StatusMessage
  {
    public string ImageName { get; set; }
    public string Message { get; set; }
  }
}
