﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ServiceModel;
using org.iringtools.library;
using System.Text;
using System.Net;
using org.iringtools.utility;
using org.iringtools.modulelibrary.events;


namespace ApplicationEditor
{
  public partial class Page : UserControl
  {
    //  private NewDbDictionary newDbDictionary;
    private ResultsList resultsList;
    private EditTreeNode editTreeNode;
    private CompositeKeys compositeKeys;
    private Relations relations;
    Collection<ScopeProject> _scopes;

    TreeViewItem _editProject;
    TreeViewItem _editApplication;

    ScopeProject _currentProject;
    ScopeApplication _currentApplication;

    public string newProvider;
    public string newProject;
    public string newApplication;
    public string newDataSourceName;
    public string newDatabaseName;
    public string newDatabaseUserName;
    public string newDatabaseUserPassword;
    public StringBuilder newDictionary;
    public string selectedCBItem = string.Empty;

    private bool isPosting;

    private ApplicationDAL _dal;

    //public event System.EventHandler<System.EventArgs> OnDataArrived;

    public Page()
    {
      try
      {
        InitializeComponent();

        string uriScheme = Application.Current.Host.Source.Scheme;
        bool usingTransportSecurity = uriScheme.Equals("https", StringComparison.InvariantCultureIgnoreCase);
        //initialize child windows
        compositeKeys = new CompositeKeys();
        compositeKeys.Closed += new EventHandler(compositeKeys_Closed);
        relations = new Relations();
        relations.Closed += new EventHandler(relations_Closed);
        resultsList = new ResultsList();
        resultsList.Closed += new EventHandler(results_Closed);
        editTreeNode = new EditTreeNode();
        editTreeNode.Closed += new EventHandler(editTreeNode_Closed);

        _dal = new ApplicationDAL();

        _dal.OnDataArrived += dal_OnDataArrived;

        LayoutRoot.SizeChanged += new SizeChangedEventHandler(LayoutRoot_SizeChanged);

        biBusyWindow.IsBusy = true;
        _dal.GetScopes();
        _dal.GetProviders();
        isPosting = false;
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
    }

    void relations_Closed(object sender, EventArgs e)
    {
      try
      {
        if ((bool)relations.DialogResult && !compositeKeys.CancelButton.IsPressed)
        {
        }
        else
        {
        }
      }
      catch (Exception ex)
      {
      }
    }

    void compositeKeys_Closed(object sender, EventArgs e)
    {
      try
      {
        if ((bool)compositeKeys.DialogResult && !compositeKeys.CancelButton.IsPressed)
        {
          TreeViewItem child;
          object selectedItem = tvwDestination.SelectedItem;
          TreeViewItem dataObjectItem = findObjectParent(selectedItem as TreeViewItem);
          org.iringtools.library.DataObject dataObject = dataObjectItem.Tag as org.iringtools.library.DataObject;
          dataObject.keyProperties.Clear();

          foreach (TreeViewItem objectItem in dataObjectItem.Items)
          {
            if (objectItem.Header.ToString() == "Keys")
            {
              objectItem.Items.Clear();
              foreach (String lbItem in compositeKeys.lbKeys.Items)
              {
                KeyProperty key = new KeyProperty { keyPropertyName = lbItem };
                dataObject.keyProperties.Add(key);
                child = new TreeViewItem { Tag = key };
                AddTreeItem(objectItem, child, lbItem, null, false);
              }
            }
          }
          compositeKeys._dataItems.Clear();
          compositeKeys._keyItems.Clear();
        }
        else if (!(bool)compositeKeys.DialogResult && compositeKeys.CancelButton.IsPressed)
        {
          compositeKeys._dataItems.Clear();
          compositeKeys._keyItems.Clear();
        }
        else
          compositeKeys.Show();
      }
      catch
      { }
    }

    public string currentProject
    {
      get
      {
        ScopeProject project = (ScopeProject)((ComboBoxItem)cmbProject.SelectedItem).Tag;
        return project.Name;
      }
    }

    public string selectedApplication
    {
      get
      {
        ScopeApplication application = (ScopeApplication)((ComboBoxItem)cmbApp.SelectedItem).Tag;
        return application.Name;
      }
    }

    void dal_OnDataArrived(object sender, System.EventArgs e)
    {
      try
      {
        // Only handle properly populated event arguments
        CompletedEventArgs args = e as CompletedEventArgs;
        if (args == null)
          return;

        // CompletedEventArgs is a generic class that handles multiple
        // services.  We have to cast the CompletedType for our service.
        CompletedEventType processType = (CompletedEventType)
          Enum.Parse(typeof(CompletedEventType), args.CompletedType.ToString(), false);

        switch (processType)
        {
          case CompletedEventType.NotDefined:
            break;

          case CompletedEventType.DeleteApp:
            deleteComplete(args);
            break;

          case CompletedEventType.GetDatabaseSchema:
            getdbschemaComplete(args);
            break;

          case CompletedEventType.GetDbDictionary:
            getdbDictionaryComplete(args);
            break;

          case CompletedEventType.GetProviders:
            getProvidersComplete(args);
            break;

          case CompletedEventType.GetScopes:
            getScopesComplete(args);
            break;

          case CompletedEventType.PostDictionaryToAdapterService:
            postdbdictionaryComplete(args);
            break;

          case CompletedEventType.PostScopes:
            postScopesComplete(args);
            break;

          case CompletedEventType.SaveDatabaseDictionary:
            savedbdictionaryComplete(args);
            break;

          default:
            break;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
    }

    void editTreeNode_Closed(object sender, EventArgs e)
    {
      try
      {
        if ((bool)editTreeNode.DialogResult)
        {
          StackPanel stackPanel;
          TextBlock textBlock;
          TreeViewItem selectedItem = FindFirstCheckedTreeItem(tvwItemDestinationRoot);
          stackPanel = (StackPanel)selectedItem.Header;
          textBlock = (TextBlock)stackPanel.Children[1];
          foreach (UIElement uiElement1 in editTreeNode.spContainer.Children)
          {
            if (uiElement1 is StackPanel)
            {
              StackPanel stkpnl = (StackPanel)uiElement1 as StackPanel;
              foreach (UIElement uiElement2 in stkpnl.Children)
              {
                if (uiElement2 is TextBox)
                {
                  TextBox tbox = (TextBox)uiElement2 as TextBox;
                  if (selectedItem.Tag is org.iringtools.library.DataObject)
                  {
                    if (tbox.Tag.ToString() == "entityName")
                    {
                      ((org.iringtools.library.DataObject)selectedItem.Tag).objectName = tbox.Text;
                      textBlock.Text = tbox.Text;
                    }
                    else if (tbox.Tag.ToString() == "tableName")
                      ((org.iringtools.library.DataObject)selectedItem.Tag).tableName = tbox.Text;
                  }
                  else if (selectedItem.Tag is DataProperty)
                  {
                    if (tbox.Tag.ToString() == "columnName")
                    {
                      ((DataProperty)selectedItem.Tag).columnName = tbox.Text;
                      textBlock.Text = tbox.Text;
                    }
                    else if (tbox.Tag.ToString() == "propertyName")
                      ((DataProperty)selectedItem.Tag).propertyName = tbox.Text;
                  }
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
    }

    TreeViewItem FindFirstCheckedTreeItem(TreeViewItem root)
    {
      TreeViewItem treeViewItem = new TreeViewItem();
      try
      {
        StackPanel stackPanel;
        CheckBox checkBox;

        foreach (TreeViewItem table in root.Items)
        {
          stackPanel = (StackPanel)table.Header;
          checkBox = (CheckBox)stackPanel.Children[0];
          if (checkBox.IsChecked.Value.Equals(true))
            treeViewItem = table;
          else
          {
            foreach (TreeViewItem column in table.Items)
            {
              stackPanel = (StackPanel)column.Header;
              checkBox = (CheckBox)stackPanel.Children[0];
              if (checkBox.IsChecked.Value.Equals(true))
                treeViewItem = column;
            }
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
      return treeViewItem;
    }

    void deleteComplete(CompletedEventArgs args)
    {
      try
      {
        if (args.Error != null)
        {
          MessageBox.Show(args.FriendlyErrorMessage, "Delete Application Error", MessageBoxButton.OK);
          biBusyWindow.IsBusy = false;
          return;
        }

        Response response = (Response)args.Data;

        resultsList.lbResult.ItemsSource = response.StatusList[0].Messages;

        _dal.GetDbDictionary(_currentProject.Name, _currentApplication.Name);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
      finally
      {
        biBusyWindow.IsBusy = false;
      }
    }

    void results_Closed(object sender, EventArgs e)
    {
      try
      {
        if ((bool)resultsList.DialogResult)
        {

        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
    }

    void postdbdictionaryComplete(CompletedEventArgs args)
    {
      try
      {
        if (args.Error != null)
        {
          MessageBox.Show(args.FriendlyErrorMessage, "Post Database Dictionary Error", MessageBoxButton.OK);
          return;
        }

        Response response = (Response)args.Data;

        resultsList.lbResult.ItemsSource = response.StatusList[0].Messages;

        isPosting = false;
        resultsList.Show();
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
      finally
      {
        biBusyWindow.IsBusy = false;
      }
    }

    void postScopesComplete(CompletedEventArgs args)
    {
      try
      {
        if (args.Error != null)
        {
          MessageBox.Show(args.FriendlyErrorMessage, "Post Scopes Error", MessageBoxButton.OK);
          return;
        }

        Response response = (Response)args.Data;

        resultsList.lbResult.ItemsSource = response.StatusList[0].Messages;

        isPosting = false;
        resultsList.Show();
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
      finally
      {
        biBusyWindow.IsBusy = false;
      }
    }

    void getProvidersComplete(CompletedEventArgs args)
    {
      try
      {
        if (args.Error != null)
        {
          MessageBox.Show(args.FriendlyErrorMessage, "Get Providers Error", MessageBoxButton.OK);
          return;
        }
        cbProvider.ItemsSource = (string[])args.Data;
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
    }

    private void BuildNewDbDictionary(string newProvider, string newProject, string newApplication, string newDataSourceName, string newDatabaseName, string newDatabaseUserName, string newDatabaseUserPassword)
    {
      try
      {
        newDictionary = new StringBuilder();
        newDictionary.Append("DatabaseDictionary.");
        newDictionary.Append(newProject);
        newDictionary.Append(".");
        newDictionary.Append(newApplication);
        newDictionary.Append(".xml");

        string connectionstring = BuildConnectionString(newProvider, newDataSourceName, newDatabaseName, newDatabaseUserName, newDatabaseUserPassword);

        DatabaseDictionary dict = new DatabaseDictionary()
        {
          connectionString = connectionstring,
          provider = (Provider)Enum.Parse(typeof(Provider), newProvider, true),
          dataObjects = new List<org.iringtools.library.DataObject>()
        };

        _dal.SaveDatabaseDictionary(dict, newProject, newApplication);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
    }

    private string BuildConnectionString(string newProvider, string newDataSourceName, string newDatabaseName, string newDatabaseUserName, string newDatabaseUserPassword)
    {
      newProvider = newProvider.ToUpper();
      string connString = string.Empty;
      try
      {
        if (newProvider.Contains("MSSQL"))
        {
          connString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};",
              newDataSourceName,
              newDatabaseName,
              newDatabaseUserName,
              newDatabaseUserPassword);
        }
        else if (newProvider.Contains("ORACLE"))
        {
          connString = string.Format("Data Source={0};User ID={1};Password={2};",
              newDataSourceName,
              newDatabaseUserName,
              newDatabaseUserPassword);
        }
        else if (newProvider.Contains("MYSQL"))//using default port
        {
          connString = string.Format("Server={0};Database={1};Uid={2};Pwd={3};",
              newDataSourceName,
              newDatabaseName,
              newDatabaseUserName,
              newDatabaseUserPassword);
        }
        else if (newProvider.Contains("POSTGRES"))
        {
          connString = string.Format("Server={0}; Initial Catalog={1}; User Id={2}; Password={3};",
              newDataSourceName,
              newDatabaseName,
              newDatabaseUserName,
              newDatabaseUserPassword);
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
      return connString;
    }

    void savedbdictionaryComplete(CompletedEventArgs args)
    {
      try
      {
        if (args.Error != null)
        {
          MessageBox.Show(args.FriendlyErrorMessage, "Save Database Dictionary Error", MessageBoxButton.OK);
          return;
        }

        //_dal.GetScopes();

      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
      finally
      {
        biBusyWindow.IsBusy = false;
      }
    }

    void getdbschemaComplete(CompletedEventArgs args)
    {
      try
      {
        if (args.Error != null)
        {

          MessageBox.Show(args.FriendlyErrorMessage, "Get Database Schema Error", MessageBoxButton.OK);
          biBusyWindow.IsBusy = false;
          return;
        }

        tvwItemSourceRoot.Items.Clear();
        DatabaseDictionary databaseDictionary = (DatabaseDictionary)args.Data;
        string connString = databaseDictionary.connectionString;

        if (string.IsNullOrEmpty(connString) && cbProvider.SelectedItem.ToString() != "MsSql2000")
        {
          databaseDictionary.connectionString = BuildConnectionString(cbProvider.SelectedItem.ToString(),
            tbNewDataSource.Text,
            tbNewDatabase.Text,
            tbUserID.Text,
            tbPassword.Password);
          databaseDictionary.provider = (Provider)Enum.Parse(typeof(Provider), cbProvider.SelectedItem.ToString(), true);
          _dal.SaveDatabaseDictionary(databaseDictionary, _currentProject.Name, _currentApplication.Name);
          _dal.GetDatabaseSchema(_currentProject.Name, _currentApplication.Name);

        }
        constructTreeView(databaseDictionary, tvwItemSourceRoot);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
      finally
      {
        biBusyWindow.IsBusy = false;
      }
    }

    void getdbDictionaryComplete(CompletedEventArgs args)
    {
      try
      {
        if (args.Error != null)
        {
          DatabaseDictionary dictionary = new DatabaseDictionary();
          _dal.SaveDatabaseDictionary(dictionary, _currentProject.Name, _currentApplication.Name);
          return;
        }

        DatabaseDictionary dict = (DatabaseDictionary)args.Data;
        if (dict.connectionString == null) return;

        tbNewDataSource.Text = String.Empty;
        tbNewDatabase.Text = String.Empty;
        tbUserID.Text = String.Empty;
        tbPassword.Password = String.Empty;

        string[] tokens = dict.connectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        string[] token = null;
        foreach (string part in tokens)
        {
          token = part.Split('=');
          switch (token[0].ToUpper())
          {
            case "DATA SOURCE":
              tbNewDataSource.Text = token[1];
              break;
            case "INITIAL CATALOG":
              tbNewDatabase.Text = token[1];
              break;
            //case "USER ID":
            //  tbUserID.Text = token[1];
            //  break;
            //case "PASSWORD":
            //  tbPassword.Password = token[1];
            //  break;
          }
        }
        cbProvider.SelectedItem = dict.provider.ToString();
        string project = string.Empty;
        string application = string.Empty;

        if (_currentProject != null && _currentApplication != null)
        {
          project = _currentProject.Name;
          application = _currentApplication.Name;
        }
        else
        {
          project = newProject;
          application = newApplication;
        }

        if (isPosting)
        {
          _dal.PostDictionaryToApplicationService(project, application);
        }
        else
        {
          tvwItemDestinationRoot.Items.Clear();

          _dal.GetDatabaseSchema(project, application);
          constructTreeView(dict, tvwItemDestinationRoot);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
      finally
      {
        biBusyWindow.IsBusy = false;
      }
    }

    void constructObjectTree(TreeViewItem selectedItem, TreeViewItem destinationRoot)
    {
      TreeViewItem tableTreeViewItem = null;
      TreeViewItem columnTreeViewItem = null;
      TreeViewItem keysTreeViewItem = new TreeViewItem() { Header = "Keys" };
      TreeViewItem propertiesTreeViewItem = new TreeViewItem() { Header = "Properties" };
      TreeViewItem relationshipsTreeViewItem = new TreeViewItem() { Header = "Relationships" };
      org.iringtools.library.DataObject dataObject;
      bool enableCheckBox = false;
      try
      {
        if (selectedItem.Tag is org.iringtools.library.DataObject)
        {
          dataObject = selectedItem.Tag as org.iringtools.library.DataObject;
          tableTreeViewItem = new TreeViewItem { Header = dataObject.objectName, Tag = dataObject };
          //constructObjectTree(tableTreeViewItem);
          tableTreeViewItem.Items.Add(keysTreeViewItem);
          tableTreeViewItem.Items.Add(propertiesTreeViewItem);
          tableTreeViewItem.Items.Add(relationshipsTreeViewItem);
          foreach (org.iringtools.library.KeyProperty keyName in dataObject.keyProperties)
          {
            DataProperty key = dataObject.getKeyProperty(keyName.keyPropertyName);
            if (key == null) continue;
            columnTreeViewItem = new TreeViewItem();
            columnTreeViewItem.Tag = key;
            AddTreeItem(keysTreeViewItem, columnTreeViewItem, key.propertyName, null, false);
          }

          foreach (DataProperty column in dataObject.dataProperties)
          {
            columnTreeViewItem = new TreeViewItem();
            columnTreeViewItem.Tag = column;
            AddTreeItem(propertiesTreeViewItem, columnTreeViewItem, column.propertyName, null, enableCheckBox);
          }

          foreach (DataRelationship relation in dataObject.dataRelationships)
          {
            //columnTreeViewItem = new TreeViewItem{ Header = relation;
          }
          AddTreeItem(destinationRoot, tableTreeViewItem, dataObject.objectName, null, false);
        }
      }
      catch (Exception ex)
      {

      }
    }

    void constructTreeView(DatabaseDictionary dict, TreeViewItem root)
    {
      TreeViewItem tableTreeViewItem = null;

      bool enableCheckBox = false;
      if (root.Name != "tvwItemSourceRoot")
        enableCheckBox = true;
      try
      {
        root.Tag = dict;

        if (dict.dataObjects == null)
          dict.dataObjects = new List<org.iringtools.library.DataObject>();

        foreach (org.iringtools.library.DataObject table in dict.dataObjects)
        {
          tableTreeViewItem = new TreeViewItem() { Header = table.tableName };
          tableTreeViewItem.Tag = table;
          root.IsExpanded = true;
          constructObjectTree(tableTreeViewItem, root);

        }

        root.Visibility = Visibility.Visible;
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message);
      }
    }

    void AddTreeItem(TreeViewItem parent, TreeViewItem child, string headerText, string color, bool checkBox)
    {

      try
      {
        StackPanel stackpanel = new StackPanel() { Orientation = Orientation.Horizontal };
        CheckBox checkbox = new CheckBox();

        checkbox.Checked += new RoutedEventHandler(itm_Checked);
        TextBlock textblock = null;
        //if (!string.IsNullOrEmpty(color))
        //  textblock = new TextBlock() { Text = headerText, Foreground = new SolidColorBrush(Colors.Magenta) };
        //else
        textblock = new TextBlock() { Text = headerText };

        checkbox.IsEnabled = true;
        if (child.Tag is org.iringtools.library.DataObject && !checkBox)
          checkbox.IsEnabled = true;
        stackpanel.Children.Add(checkbox);
        stackpanel.Children.Add(textblock);
        child.Header = stackpanel;
        //child.FontSize = 12;
        child.Expanded += new RoutedEventHandler(itm_Expanded);

        parent.Items.Add(child);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    void itm_Checked(object sender, RoutedEventArgs e)
    {
    }

    void itm_Expanded(object sender, RoutedEventArgs e)
    {
    }

    void tvwItemScopeProject_Selected(object sender, RoutedEventArgs e)
    {
      if (sender != null)
      {
        TreeViewItem tvwItem = (TreeViewItem)sender;
        if (tvwItem.Tag != null)
        {
          _editProject = tvwItem;
          ScopeProject project = (ScopeProject)_editProject.Tag;
          tbNewPrjName.Text = project.Name;
          tbNewPrjDesc.Text = (project.Description == null) ? string.Empty : project.Description;
        }
      }
    }

    void tvwItemScopeApplication_Selected(object sender, RoutedEventArgs e)
    {
      if (sender != null)
      {
        TreeViewItem tvwItem = (TreeViewItem)sender;
        TreeViewItem tvwParent = (TreeViewItem)tvwItem.Parent;

        if (tvwParent.Tag != null)
        {
          _editProject = tvwParent;
          ScopeProject project = (ScopeProject)_editProject.Tag;
          tbNewPrjName.Text = project.Name;
          tbNewPrjDesc.Text = (project.Description == null) ? string.Empty : project.Description;
        }

        if (tvwItem.Tag != null)
        {
          _editApplication = tvwItem;
          ScopeApplication application = (ScopeApplication)_editApplication.Tag;
          tbNewAppName.Text = application.Name;
          tbNewAppDesc.Text = (application.Description == null) ? string.Empty : application.Description;
        }
      }
    }

    void getScopesComplete(CompletedEventArgs args)
    {
      try
      {

        if (args != null && args.Data != null)
        {

          _scopes = (Collection<ScopeProject>)args.Data;

          tvwScopesItemRoot.Items.Clear();
          foreach (ScopeProject project in _scopes)
          {
            cmbProject.Items.Add(new ComboBoxItem { Content = project.Name, Tag = project });

            TreeViewItem tvwItemProject = new TreeViewItem { Header = project.Name, Tag = project };
            tvwItemProject.Selected += new RoutedEventHandler(tvwItemScopeProject_Selected);

            TreeViewItem tvwItemScope = null;

            foreach (ScopeApplication application in project.Applications)
            {
              tvwItemScope = new TreeViewItem { Header = application.Name, Tag = application };
              tvwItemScope.Selected += new RoutedEventHandler(tvwItemScopeApplication_Selected);
              tvwItemProject.Items.Add(tvwItemScope);
            }

            tvwScopesItemRoot.Items.Add(tvwItemProject);

          }

          tvwScopesItemRoot.Tag = _scopes;
          tvwScopesItemRoot.IsExpanded = true;
          tvwScopes.Visibility = Visibility.Visible;

        }

      }
      catch (Exception ex)
      {
        System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
      }
      finally
      {
        biBusyWindow.IsBusy = false;
      }
    }

    void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
    {

    }

    //private void btnNewDictionary_Click(object sender, RoutedEventArgs e)
    //{
    //    try
    //    {
    //       // _dal.GetProviders();
    //        newDbDictionary.tbMessages.Text = string.Empty;
    //        newDbDictionary.Show();
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
    //    }
    //}

    private void cmbProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        ComboBoxItem cmbItem = (ComboBoxItem)cmbProject.SelectedItem;
        _currentProject = (ScopeProject)cmbItem.Tag;


        foreach (ScopeApplication application in _currentProject.Applications)
        {
          cmbApp.Items.Add(new ComboBoxItem { Content = application.Name, Tag = application });
        }

      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
    }

    private void cmbApp_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

      try
      {
        tvwItemSourceRoot.Items.Clear();
        tvwItemSourceRoot.Visibility = Visibility.Collapsed;
        tvwItemDestinationRoot.Items.Clear();
        tvwItemDestinationRoot.Visibility = Visibility.Collapsed;

        _currentApplication = (ScopeApplication)((ComboBoxItem)cmbApp.SelectedItem).Tag;

        _dal.GetDbDictionary(_currentProject.Name, _currentApplication.Name);
      }
      catch (Exception ex)
      {
        throw ex;
      }

    }

    private void btnSaveDbDictionary_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        biBusyWindow.IsBusy = true;
        TreeViewItem dbdictRoot = tvwItemDestinationRoot;
        DatabaseDictionary dbDict = dbdictRoot.Tag as DatabaseDictionary;
        dbDict.dataObjects.Clear();
        foreach (TreeViewItem dataObjectItem in dbdictRoot.Items)
        {
          dbDict.dataObjects.Add(dataObjectItem.Tag as org.iringtools.library.DataObject);
        }
        _dal.SaveDatabaseDictionary(dbDict, _currentProject.Name, _currentApplication.Name);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
        biBusyWindow.IsBusy = false;
      }
    }

    private void btnPostDictionary_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        DatabaseDictionary dbdict = new DatabaseDictionary();
        biBusyWindow.IsBusy = true;
        isPosting = true;

        _dal.PostDictionaryToApplicationService(_currentProject.Name, _currentApplication.Name);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
        biBusyWindow.IsBusy = false;
      }
    }

    private void clearComboBox(ComboBox combox)
    {
      try
      {
        if (combox.ItemsSource != null)
        {
          combox.ItemsSource = null;
        }

        combox.IsEnabled = false;
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }

    }

    private void btnAddColumnToDict_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        TreeViewItem sourceRoot = tvwItemSourceRoot;
        TreeViewItem destRoot = tvwItemDestinationRoot;
        TreeViewItem tableItem = new TreeViewItem();

        for (int i = 0; i < sourceRoot.Items.Count; i++)
        {
          tableItem = (TreeViewItem)sourceRoot.Items[i];
          TreeViewItem parent = tableItem.Parent as TreeViewItem;
          if (((CheckBox)((StackPanel)tableItem.Header).Children[0]).IsChecked.Value.Equals(true))
          {
            if (!destRoot.Items.Contains(tableItem))
            {
              if ((!destRoot.Items.Contains(parent)) &
                  (!parent.Header.ToString().Equals("Available Database Schema Items")))
              {
                TreeViewItem parentParent = parent.Parent as TreeViewItem;
                parentParent.Items.Add(parent);
              }
              constructObjectTree(tableItem, destRoot);
            }
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
    }

    private void RemoveTreeItem(TreeViewItem child)
    {
      try
      {
        TreeViewItem parent = child.Parent as TreeViewItem;
        parent.Items.Remove(child);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private void btnDelColFromDict_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        TreeViewItem selectedItem = tvwDestination.SelectedItem as TreeViewItem;
        TreeViewItem parent = selectedItem.Parent as TreeViewItem;

        DatabaseDictionary dbDict = tvwItemDestinationRoot.Tag as DatabaseDictionary;

        TreeViewItem objectItem = findObjectParent(selectedItem);
        org.iringtools.library.DataObject dataObject = objectItem.Tag as org.iringtools.library.DataObject;

        if (selectedItem == null || selectedItem.Header is String)
        {
          MessageBox.Show("Please select a node to delete", "DELETE NODE", MessageBoxButton.OK);
          return;
        }
        if (selectedItem.Tag is org.iringtools.library.DataObject)
        {
          parent.Items.Remove(selectedItem);
          dbDict.dataObjects.Remove(selectedItem.Tag as org.iringtools.library.DataObject);
        }
        else if (selectedItem.Tag is KeyProperty)
        {
          parent.Items.Remove(selectedItem);
          dataObject.keyProperties.Remove((KeyProperty)selectedItem.Tag);
        }
        else if (selectedItem.Tag is DataProperty)
        {

          dataObject.deleteProperty((DataProperty)selectedItem.Tag);
          TreeViewItem key = findKeyItem(selectedItem, ((TextBlock)((StackPanel)selectedItem.Header).Children[1]).Text);
          if (key != null)
          {
            TreeViewItem keyParent = key.Parent as TreeViewItem;
            keyParent.Items.Remove(key);
          }
          parent.Items.Remove(selectedItem);
        }
        else if (selectedItem.Tag is DataRelationship)
        {
          parent.Items.Remove(selectedItem);
          dataObject.dataRelationships.Remove((DataRelationship)selectedItem.Tag);

        }
        _dal.GetDatabaseSchema(_currentProject.Name, _currentApplication.Name);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
    }

    private TreeViewItem findKeyItem(TreeViewItem selectedItem, String itemName)
    {
      TreeViewItem found = null;
      TreeViewItem objectItem = findObjectParent(selectedItem);
      foreach (TreeViewItem item in objectItem.Items)
      {
        if (item.Header.ToString() == "Keys")
        {
          foreach (TreeViewItem keyItem in item.Items)
          {
            if (((TextBlock)((StackPanel)keyItem.Header).Children[1]).Text == itemName)
              found = keyItem;
          }
        }
      }
      return found;
    }
    private void btnEditNode_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        editTreeNode.spContainer.Children.Clear();

        StackPanel stackPanel;

        TextBlock textBlock;
        TextBox textBox;
        TreeViewItem selectedItem = FindFirstCheckedTreeItem(tvwItemDestinationRoot);


        TreeViewItem treeViewItem = (TreeViewItem)selectedItem;
        if (selectedItem.Tag is org.iringtools.library.DataObject)
        {
          stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
          textBlock = CreateTextBlock("      --==  Edit Table  ==--    ");
          textBlock.FontSize = 14;
          stackPanel.Children.Add(textBlock);
          editTreeNode.spContainer.Children.Add(stackPanel);

          stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
          textBlock = CreateTextBlock("Entity Name: ");
          textBox = CreateTextBox(((org.iringtools.library.DataObject)selectedItem.Tag).objectName, "entityName");
          stackPanel.Children.Add(textBlock);
          stackPanel.Children.Add(textBox);
          editTreeNode.spContainer.Children.Add(stackPanel);
          stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
          textBlock = CreateTextBlock("Table Name: ");
          textBox = CreateTextBox(((org.iringtools.library.DataObject)selectedItem.Tag).tableName, "tableName");
          stackPanel.Children.Add(textBlock);
          stackPanel.Children.Add(textBox);
          editTreeNode.spContainer.Children.Add(stackPanel);
          editTreeNode.Show();
        }
        else if (selectedItem.Tag is DataProperty)
        {
          stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
          textBlock = CreateTextBlock("      --==  Edit Column  ==--    ");
          textBlock.FontSize = 14;
          stackPanel.Children.Add(textBlock);
          editTreeNode.spContainer.Children.Add(stackPanel);
          stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
          textBlock = CreateTextBlock("Column Name: ");
          textBox = CreateTextBox(((DataProperty)selectedItem.Tag).columnName, "columnName");
          stackPanel.Children.Add(textBlock);
          stackPanel.Children.Add(textBox);
          editTreeNode.spContainer.Children.Add(stackPanel);

          stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
          textBlock = CreateTextBlock("   Data Length: ");
          textBox = CreateTextBox(((DataProperty)selectedItem.Tag).dataLength.ToString(), "dataLength");
          stackPanel.Children.Add(textBlock);
          stackPanel.Children.Add(textBox);
          editTreeNode.spContainer.Children.Add(stackPanel);

          stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
          textBlock = CreateTextBlock("      Column Type: ");
          textBox = CreateTextBox(Enum.GetName(typeof(DataType), ((DataProperty)selectedItem.Tag).dataType), "ColumnType");
          //   textBlock = CreateTextBlock("      Data Type: ");
          //   textBox = CreateTextBox(Enum.GetName(typeof(DataType), ((Column)selectedItem.Tag).dataType),"dataType");
          stackPanel.Children.Add(textBlock);
          stackPanel.Children.Add(textBox);
          editTreeNode.spContainer.Children.Add(stackPanel);

          stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
          textBlock = CreateTextBlock("       IsNullable: ");
          textBox = CreateTextBox(((DataProperty)selectedItem.Tag).isNullable.ToString(), "isNullable");
          stackPanel.Children.Add(textBlock);
          stackPanel.Children.Add(textBox);
          editTreeNode.spContainer.Children.Add(stackPanel);

          stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
          textBlock = CreateTextBlock("Property Name :");
          textBox = CreateTextBox(((DataProperty)selectedItem.Tag).propertyName, "propertyName");
          stackPanel.Children.Add(textBlock);
          stackPanel.Children.Add(textBox);
          editTreeNode.spContainer.Children.Add(stackPanel);

          editTreeNode.Show();
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
    }

    private TextBox CreateTextBox(string text, string tag)
    {
      try
      {
        TextBox textBox = new TextBox() { Text = text };
        textBox.Tag = tag;
        textBox.Width = 100;
        textBox.Height = 24;
        return textBox;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private TextBlock CreateTextBlock(string text)
    {
      try
      {
        TextBlock textBlock = new TextBlock() { Text = text };
        textBlock.Height = 24;
        textBlock.Margin = new Thickness() { Top = 5, Left = 5 };
        return textBlock;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private void btnAddScope_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (tbNewPrjName.Text != null && tbNewAppName.Text != null)
        {
          TreeViewItem tvwPrj = null;

          foreach (TreeViewItem tvwItem in tvwScopesItemRoot.Items)
          {
            if (tvwItem.Header.ToString() == tbNewPrjName.Text)
            {
              tvwPrj = tvwItem;
              break;
            }
          }

          if (tvwPrj == null)
          {
            ScopeProject project = new ScopeProject { Name = tbNewPrjName.Text, Applications = new List<ScopeApplication>() };
            tvwPrj = new TreeViewItem { Header = tbNewPrjName.Text, Tag = project };
            _scopes.Add(project);            
            tvwScopesItemRoot.Items.Add(tvwPrj);
            cmbProject.Items.Add(new ComboBoxItem { Content = project.Name, Tag = project });
          }

          TreeViewItem tvwApp = null;

          foreach (TreeViewItem tvwItem in tvwPrj.Items)
          {
            if (tvwItem.Header.ToString() == tbNewAppName.Text)
            {
              tvwApp = tvwItem;
              break;
            }
          }

          if (tvwApp == null)
          {
            ScopeProject project = (ScopeProject)tvwPrj.Tag;
            ScopeApplication application = new ScopeApplication { Name = tbNewAppName.Text, Description = tbNewAppDesc.Text };
            tvwApp = new TreeViewItem { Header = tbNewAppName.Text, Tag = application };
            project.Applications.Add(application);
            tvwPrj.Items.Add(tvwApp);

            if (cmbProject.SelectedIndex > -1)
            {
              ComboBoxItem selectedItem = (ComboBoxItem)cmbProject.SelectedItem;
              ScopeProject cbPrj = (ScopeProject)selectedItem.Tag;
              if (project.Equals(cbPrj))
              {
                cmbApp.Items.Add(new ComboBoxItem { Content = application.Name, Tag = application });
              }
            }

          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private void btnSaveScope_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (tbNewPrjName.Text != null && tbNewAppName.Text != null)
        {
          if (_editProject != null)
          {
            ScopeProject project = (ScopeProject)_editProject.Tag;
            _editProject.Header = tbNewPrjName.Text;
            project.Name = tbNewPrjName.Text;
            project.Description = tbNewPrjDesc.Text;
          }

          if (_editApplication != null)
          {
            ScopeApplication application = (ScopeApplication)_editApplication.Tag;
            _editApplication.Header = tbNewAppName.Text;
            application.Name = tbNewAppName.Text;
            application.Description = tbNewAppDesc.Text;
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private void btnCreateCompKey_Click(object sender, RoutedEventArgs e)
    {
      ListBoxItem lbItem;
      object selectedItem = tvwDestination.SelectedItem;
      TreeViewItem treeViewItem = (TreeViewItem)selectedItem;
      TreeViewItem dataObjectparent;

      try
      {
        if (selectedItem == null)
        {
          MessageBox.Show("Please select a tree node from treeview", "COMPOSITE KEYS", MessageBoxButton.OK);
          return;
        }
        else if (treeViewItem.Tag is org.iringtools.library.DatabaseDictionary)
        {
          MessageBox.Show("Please at least select a table node in treeview", "COMPOSITE KEYS", MessageBoxButton.OK);
          return;
        }
        else
        {
          dataObjectparent = findObjectParent(treeViewItem);
          compositeKeys._dataItems = new ObservableCollection<String>();
          compositeKeys._keyItems = new ObservableCollection<String>();
          compositeKeys.lbSourceProperties.ItemsSource = compositeKeys._dataItems;
          compositeKeys.lbKeys.ItemsSource = compositeKeys._keyItems;
          org.iringtools.library.DataObject dataObject = dataObjectparent.Tag as org.iringtools.library.DataObject;
          compositeKeys.lblObjectName.Content = dataObject.objectName;

          foreach (DataProperty dataProperty in dataObject.dataProperties)
          {

            lbItem = new ListBoxItem { Content = dataProperty.propertyName };
            compositeKeys._dataItems.Add(dataProperty.propertyName);
          }
          foreach (KeyProperty keyProperty in dataObject.keyProperties)
          {

            lbItem = new ListBoxItem { Content = keyProperty.keyPropertyName };
            compositeKeys._keyItems.Add(keyProperty.keyPropertyName);

            //var result = from _dataProperties
            if (compositeKeys._dataItems.Contains(keyProperty.keyPropertyName))
              compositeKeys._dataItems.Remove(keyProperty.keyPropertyName);
          }

        }
        compositeKeys.Show();
      }

      catch (Exception ex)
      {
        MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
      }
    }


    private TreeViewItem findObjectParent(TreeViewItem selectedItem)
    {
      TreeViewItem parent = null;
      if (selectedItem.Tag is org.iringtools.library.DataObject)
        parent = selectedItem;
      else if (selectedItem.Tag is org.iringtools.library.DataProperty)
      {
        parent = findObjectParent(selectedItem.Parent as TreeViewItem);
      }
      else if (selectedItem.Header is String)
      {
        parent = findObjectParent(selectedItem.Parent as TreeViewItem);
      }
      else if (selectedItem.Header is StackPanel)
      {
        parent = findObjectParent(selectedItem.Parent as TreeViewItem);
      }

      return parent;
    }

    private void btnCreateRalation_Click(object sender, RoutedEventArgs e)
    {
      relations.Show();
    }

    private void tvwDestination_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {

    }
    private void btnDelScope_Click(object sender, RoutedEventArgs e)
    {
      try
      {

        if (tvwScopes.SelectedItem != null)
        {
          TreeViewItem selectedItem = (TreeViewItem)tvwScopes.SelectedItem;
          object oItem = selectedItem.Tag;

          if (oItem is ScopeProject)
          {
            _scopes.Remove((ScopeProject)oItem);
            tvwScopes.Items.Remove(selectedItem);

            _editProject = null;
            tbNewPrjName.Text = String.Empty;
            tbNewPrjDesc.Text = String.Empty;

            _editApplication = null;
            tbNewAppName.Text = String.Empty;
            tbNewAppDesc.Text = String.Empty;
          }
          else if (oItem is ScopeApplication)
          {
            TreeViewItem parentItem = (TreeViewItem)selectedItem.Parent;
            ScopeProject project = (ScopeProject)parentItem.Tag;

            project.Applications.Remove((ScopeApplication)oItem);
            parentItem.Items.Remove(selectedItem);

            _editApplication = null;
            tbNewAppName.Text = String.Empty;
            tbNewAppDesc.Text = String.Empty;
          }
        }

      }
      catch (Exception ex)
      {
        throw ex;
      }
    }


    private void btnMoveUp_Click(object sender, RoutedEventArgs e)
    {
      int treeIndex;
      int dataIndex;
      object selectedItem = tvwDestination.SelectedItem;
      TreeViewItem treeViewItem = (TreeViewItem)selectedItem;
      TreeViewItem parent = treeViewItem.Parent as TreeViewItem;
      TreeViewItem dataObjectItem = findObjectParent(treeViewItem);
      org.iringtools.library.DataObject dataObject = dataObjectItem.Tag as org.iringtools.library.DataObject;
      if (selectedItem == null)
        return;
      else if (treeViewItem.Tag is org.iringtools.library.DataObject || treeViewItem.Tag == null)
      {
        MessageBox.Show("Please select keys, properties or relationships to move up", "MOVE UP", MessageBoxButton.OK);
        return;
      }
      else if (treeViewItem.Tag is DataProperty)
      {
        treeIndex = parent.Items.IndexOf(treeViewItem);
        dataIndex = dataObject.dataProperties.IndexOf(treeViewItem.Tag as DataProperty);
        if (treeIndex > 0)
        {
          parent.Items.Remove(treeViewItem);
          parent.Items.Insert(treeIndex - 1, treeViewItem);
        }
        if (dataIndex > 0)
        {
          dataObject.dataProperties.Remove(treeViewItem.Tag as DataProperty);
          dataObject.dataProperties.Insert(dataIndex - 1, treeViewItem.Tag as DataProperty);
        }
      }
      else if (treeViewItem.Tag is KeyProperty)
      {
        treeIndex = parent.Items.IndexOf(treeViewItem);
        dataIndex = dataObject.keyProperties.IndexOf(treeViewItem.Tag as KeyProperty);
        if (treeIndex > 0)
        {
          parent.Items.Remove(treeViewItem);
          parent.Items.Insert(treeIndex - 1, treeViewItem);
        }
        if (dataIndex > 0)
        {
          dataObject.keyProperties.Remove(treeViewItem.Tag as KeyProperty);
          dataObject.keyProperties.Insert(dataIndex - 1, treeViewItem.Tag as KeyProperty);
        }
      }

      else if (treeViewItem.Tag is DataRelationship)
      {
        treeIndex = parent.Items.IndexOf(treeViewItem);
        dataIndex = dataObject.dataRelationships.IndexOf(treeViewItem.Tag as DataRelationship);
        if (treeIndex > 0)
        {
          parent.Items.Remove(treeViewItem);
          parent.Items.Insert(treeIndex - 1, treeViewItem);
        }
        if (dataIndex > 0)
        {
          dataObject.dataRelationships.Remove(treeViewItem.Tag as DataRelationship);
          dataObject.dataRelationships.Insert(dataIndex - 1, treeViewItem.Tag as DataRelationship);
        }
      }

      treeViewItem.Focus();

    }

    private void btnMoveDown_Click(object sender, RoutedEventArgs e)
    {
      int treeIndex;
      int dataIndex;
      object selectedItem = tvwDestination.SelectedItem;
      TreeViewItem treeViewItem = (TreeViewItem)selectedItem;
      TreeViewItem parent = treeViewItem.Parent as TreeViewItem;
      TreeViewItem dataObjectItem = findObjectParent(treeViewItem);
      org.iringtools.library.DataObject dataObject = dataObjectItem.Tag as org.iringtools.library.DataObject;
      if (selectedItem == null)
        return;
      else if (treeViewItem.Tag is org.iringtools.library.DataObject || treeViewItem.Tag == null)
      {
        MessageBox.Show("Please select keys, properties or relationships to move down", "MOVE DOWN", MessageBoxButton.OK);
        return;
      }
      else if (treeViewItem.Tag is DataProperty)
      {
        treeIndex = parent.Items.IndexOf(treeViewItem);
        dataIndex = dataObject.dataProperties.IndexOf(treeViewItem.Tag as DataProperty);
        if (treeIndex != parent.Items.Count - 1)
        {
          parent.Items.Remove(treeViewItem);
          parent.Items.Insert(treeIndex + 1, treeViewItem);
        }
        if (dataIndex != dataObject.dataProperties.Count - 1)
        {
          dataObject.dataProperties.Remove(treeViewItem.Tag as DataProperty);
          dataObject.dataProperties.Insert(dataIndex + 1, treeViewItem.Tag as DataProperty);
        }
      }
      else if (treeViewItem.Tag is KeyProperty)
      {
        treeIndex = parent.Items.IndexOf(treeViewItem);
        dataIndex = dataObject.keyProperties.IndexOf(treeViewItem.Tag as KeyProperty);
        if (treeIndex != parent.Items.Count - 1)
        {
          parent.Items.Remove(treeViewItem);
          parent.Items.Insert(treeIndex + 1, treeViewItem);
        }
        if (dataIndex != dataObject.keyProperties.Count - 1)
        {
          dataObject.keyProperties.Remove(treeViewItem.Tag as KeyProperty);
          dataObject.keyProperties.Insert(dataIndex + 1, treeViewItem.Tag as KeyProperty);
        }
      }

      else if (treeViewItem.Tag is DataRelationship)
      {
        treeIndex = parent.Items.IndexOf(treeViewItem);
        dataIndex = dataObject.dataRelationships.IndexOf(treeViewItem.Tag as DataRelationship);
        if (treeIndex != parent.Items.Count - 1)
        {
          parent.Items.Remove(treeViewItem);
          parent.Items.Insert(treeIndex + 1, treeViewItem);
        }
        if (dataIndex != dataObject.dataRelationships.Count - 1)
        {
          dataObject.dataRelationships.Remove(treeViewItem.Tag as DataRelationship);
          dataObject.dataRelationships.Insert(dataIndex + 1, treeViewItem.Tag as DataRelationship);
        }
      }

      treeViewItem.Focus();

    }

    private void btnPostScope_Click(object sender, RoutedEventArgs e)
    {
      biBusyWindow.IsBusy = true;
      _dal.UpdateScopes(_scopes);
    }

    private void btnFetch_Click(object sender, RoutedEventArgs e)
    {
      biBusyWindow.IsBusy = true;
      if (cbProvider.SelectedIndex == -1)
      {
        MessageBox.Show("Please select a provider", "GET SCHEMA", MessageBoxButton.OK);
        cbProvider.Focus();
        return;
      }
      if (string.IsNullOrEmpty(tbNewDataSource.Text))
      {
        MessageBox.Show("Please enter a data source name", "GET SCHEMA", MessageBoxButton.OK);
        tbNewDataSource.Focus();
        return;
      }
      if (string.IsNullOrEmpty(tbNewDatabase.Text))
      {
        MessageBox.Show("Please enter a database name", "GET SCHEMA", MessageBoxButton.OK);
        tbNewDatabase.Focus();
        return;
      }
      if (string.IsNullOrEmpty(tbUserID.Text))
      {
        MessageBox.Show("Please enter a sql server user id", "GET SCHEMA", MessageBoxButton.OK);
        tbUserID.Focus();
        return;
      }
      if (string.IsNullOrEmpty(tbPassword.Password))
      {
        MessageBox.Show("Please enter a sql server user password", "GET SCHEMA", MessageBoxButton.OK);
        tbPassword.Focus();
        return;
      }
      _dal.GetDatabaseSchema(_currentProject.Name, _currentApplication.Name);
    }

  }
}
