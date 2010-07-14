using System;
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
        private NewDbDictionary newDbDictionary;
        private ResultsList resultsList;
        private EditTreeNode editTreeNode;
        Collection<ScopeProject> _scopes;
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
                newDbDictionary = new NewDbDictionary();
                newDbDictionary.Closed += new EventHandler(newDbDictionary_Closed);
                resultsList = new ResultsList();
                resultsList.Closed += new EventHandler(results_Closed);
                editTreeNode = new EditTreeNode();
                editTreeNode.Closed += new EventHandler(editTreeNode_Closed);

                _dal = new ApplicationDAL();

                _dal.OnDataArrived += dal_OnDataArrived;

                LayoutRoot.SizeChanged += new SizeChangedEventHandler(LayoutRoot_SizeChanged);

                _dal.GetScopes();                
                //_dal.GetExistingDbDictionaryFiles();

                isPosting = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);
            }
        }

        public string _project
        {
          get
          {
            ScopeProject project = (ScopeProject)((ComboBoxItem)cmbProject.SelectedItem).Tag;
            return project.Name;
          }
        }

        public string _application
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

                    case CompletedEventType.ClearTripleStore:
                        clearTripleStoreComplete(args);
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

                _dal.GetDbDictionary(_project, _application);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);                
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
            biBusyWindow.IsBusy = false;
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
          biBusyWindow.IsBusy = false;
        }

        void clearTripleStoreComplete(CompletedEventArgs args)
        {
            try
            {
                if (args.Error != null)
                {
                    MessageBox.Show(args.FriendlyErrorMessage, "Clear Triple Store Error", MessageBoxButton.OK);
                    return;
                }
                Response response = (Response)args.Data;

                resultsList.lbResult.ItemsSource = response.StatusList[0].Messages;

                //_dal.PostDictionaryToAdapterService(_project, _application);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);                
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
                newDbDictionary.cbProvider.ItemsSource = (string[])args.Data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);                
            }
        }        

        void newDbDictionary_Closed(object sender, EventArgs e)
        {
            try
            {
                if ((bool)newDbDictionary.DialogResult && !newDbDictionary.btnCancle.IsPressed)
                {
                    newProject = newDbDictionary.tbProject.Text;
                    newProvider = newDbDictionary.cbProvider.SelectedItem.ToString();
                    newApplication = newDbDictionary.tbApp.Text;
                    newDataSourceName = newDbDictionary.tbNewDataSource.Text;
                    newDatabaseName = newDbDictionary.tbNewDatabase.Text;
                    newDatabaseUserName = newDbDictionary.tbUserID.Text;
                    newDatabaseUserPassword = newDbDictionary.tbPassword.Text;
                    BuildNewDbDictionary(newProvider, newProject, newApplication,
                        newDataSourceName, newDatabaseName, newDatabaseUserName, newDatabaseUserPassword);
                }
                else if ((bool)newDbDictionary.DialogResult && newDbDictionary.btnCancle.IsPressed)
                { }
                else
                    newDbDictionary.Show();
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
                org.iringtools.library.DataObject table = new org.iringtools.library.DataObject();
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
                //string dictionaries = cbDictionary.SelectedItem.ToString();
                //string project = dictionaries.Split('.')[1];
                //string application = dictionaries.Split('.')[2];
                //_dal.GetDbDictionary(project, application);
                _dal.GetScopes();
                //_dal.GetExistingDbDictionaryFiles();
                //tvwItemDestinationRoot.Items.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);                
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

                TreeViewItem sourceTable;
                TreeViewItem destinationTable;

                tvwItemSourceRoot.Items.Clear();
                DatabaseDictionary databaseDictionary = (DatabaseDictionary)args.Data;
                ConstructTreeView(databaseDictionary, tvwItemSourceRoot);
                for (int sourceTables = 0; sourceTables < tvwItemSourceRoot.Items.Count; sourceTables++)
                {
                    sourceTable = (TreeViewItem)tvwItemSourceRoot.Items[sourceTables];
                    StackPanel sourceStackPanel = (StackPanel)sourceTable.Header;
                    TextBlock sourceTextBlock = (TextBlock)sourceStackPanel.Children[1];
                    TreeViewItem sourceParent = sourceTable.Parent as TreeViewItem;
                    for (int destTables = 0; destTables < tvwItemDestinationRoot.Items.Count; destTables++)
                    {
                        destinationTable = (TreeViewItem)tvwItemDestinationRoot.Items[destTables];
                        StackPanel destinationStackPanel = (StackPanel)destinationTable.Header;
                        TextBlock destinationTextBlock = (TextBlock)destinationStackPanel.Children[1];
                        if (sourceTextBlock.Text == destinationTextBlock.Text)
                        {
                            RemoveTreeItem(sourceParent, sourceTable);
                            sourceTables--;
                            break;
                        }
                    }
                }
                

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);                
            }
            biBusyWindow.IsBusy = false;
        }

        void getdbDictionaryComplete(CompletedEventArgs args)
        {
            try
            {
                if (args.Error != null)
                {
                    MessageBox.Show(args.FriendlyErrorMessage, "Get Database Dictionary Error", MessageBoxButton.OK);
                    return;
                }

                DatabaseDictionary dict = (DatabaseDictionary)args.Data;
                string project = string.Empty;
                string application = string.Empty;

                if (cmbProject.SelectedItem != null && cmbApp.SelectedItem != null)
                {
                    project = _project;
                    application = _application;
                }
                else
                {
                    project = newProject;
                    application = newApplication;
                }

                if (isPosting)
                {                   
                    _dal.PostDictionaryToAdapterService(project, application);
                }
                else
                {
                    tvwItemDestinationRoot.Items.Clear();

                    _dal.GetDatabaseSchema(project, application);
                    ConstructTreeView(dict, tvwItemDestinationRoot);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);                
            }
        }

        void ConstructTreeView(DatabaseDictionary dict, TreeViewItem root)
        {
          TreeViewItem tableTreeViewItem = null;
          TreeViewItem keysTreeViewItem = null;
          TreeViewItem propertiesTreeViewItem = null;
          TreeViewItem relationshipsTreeViewItem = null;
          TreeViewItem columnTreeViewItem = null;

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

              keysTreeViewItem = new TreeViewItem() { Header = "Keys" };
              propertiesTreeViewItem = new TreeViewItem() { Header = "Properties" };
              relationshipsTreeViewItem = new TreeViewItem() { Header = "Relationships" };

              tableTreeViewItem.Items.Add(keysTreeViewItem);
              tableTreeViewItem.Items.Add(propertiesTreeViewItem);
              tableTreeViewItem.Items.Add(relationshipsTreeViewItem);

              foreach (org.iringtools.library.KeyProperty keyName in table.keyProperties)
              {
                DataProperty key = table.getKeyProperty(keyName.keyPropertyName);

                columnTreeViewItem = new TreeViewItem();
                columnTreeViewItem.Tag = key;
                AddTreeItem(keysTreeViewItem, columnTreeViewItem, key.columnName, "Magenta", false);
                AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Data Length = " + key.dataLength.ToString(), null, false);
                AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Column Type = " + key.dataType.ToString(), null, false);
                //AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Data Type = " + key.dataType.ToString(), null, false);
                AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Is Nullable = " + key.isNullable, null, false);
                AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Key Type = " + key.keyType, null, false);
                AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Property Name = " + key.propertyName, null, false);
              }

              foreach (DataProperty column in table.dataProperties)
              { 
                  columnTreeViewItem = new TreeViewItem();
                  columnTreeViewItem.Tag = column;
                  AddTreeItem(propertiesTreeViewItem, columnTreeViewItem, column.columnName, null, enableCheckBox);
                  AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Data Length = " + column.dataLength.ToString(), null, false);
                  AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Column Type = " + column.dataType.ToString(), null, false);
                  //AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Data Type = " + column.dataType.ToString(), null, false);
                  AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Is Nullable = " + column.isNullable, null, false);
                  AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Property Name = " + column.propertyName, null, false);                
              }

              AddTreeItem(root, tableTreeViewItem, table.tableName, null, enableCheckBox);

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
                if (!string.IsNullOrEmpty(color))
                    textblock = new TextBlock() { Text = headerText, Foreground = new SolidColorBrush(Colors.Magenta) };
                else
                    textblock = new TextBlock() { Text = headerText };
                if (checkBox)
                    checkbox.IsEnabled = true;
                else
                    checkbox.IsEnabled = false;
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

        void tvwItemProject_Selected(object sender, RoutedEventArgs e)
        {
          if (sender != null)
          {
            TreeViewItem tvwItem = (TreeViewItem)sender;
            if (tvwItem.Tag != null)
            {
              ScopeProject oPrj = (ScopeProject)tvwItem.Tag;
              tbNewPrjName.Text = oPrj.Name;
              tbNewPrjDesc.Text = (oPrj.Description == null) ? string.Empty : oPrj.Description;
            }
          }
        }

        void tvwItemScope_Selected(object sender, RoutedEventArgs e)
        {
          if (sender != null)
          {
            TreeViewItem tvwItem = (TreeViewItem)sender;
            TreeViewItem tvwParent = (TreeViewItem)tvwItem.Parent;

            if (tvwParent.Tag != null)
            {
              ScopeProject oPrj = (ScopeProject)tvwParent.Tag;
              tbNewPrjName.Text = oPrj.Name;
              tbNewPrjDesc.Text = (oPrj.Description == null) ? string.Empty : oPrj.Description;
            }
            
            if (tvwItem.Tag != null)
            {
              ScopeApplication oApp = (ScopeApplication)tvwItem.Tag;
              tbNewAppName.Text = oApp.Name;
              tbNewAppDesc.Text = (oApp.Description == null) ? string.Empty : oApp.Description;
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

                foreach (ScopeProject project in _scopes)
                {
                  cmbProject.Items.Add(new ComboBoxItem { Content = project.Name, Tag = project });

                  TreeViewItem tvwItemProject = new TreeViewItem { Header = project.Name, Tag = project };
                  tvwItemProject.Selected += new RoutedEventHandler(tvwItemProject_Selected);

                  TreeViewItem tvwItemScope = null;

                  foreach (ScopeApplication application in project.Applications) 
                  {
                    tvwItemScope = new TreeViewItem { Header = application.Name, Tag = application };
                    tvwItemScope.Selected += new RoutedEventHandler(tvwItemScope_Selected);
                    tvwItemProject.Items.Add( tvwItemScope );
                  }

                  tvwScopesItemRoot.Items.Add( tvwItemProject );

                }

                tvwScopesItemRoot.Tag = _scopes;
                tvwScopes.Visibility = Visibility.Visible;
              
              }             
  
            }
            catch (Exception ex)
            {
                System.Windows.Browser.HtmlPage.Window.Alert(ex.Message);
            }
            finally
            {
            }
        }

        void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        private void btnNewDictionary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _dal.GetProviders();
                newDbDictionary.tbMessages.Text = string.Empty;
                newDbDictionary.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);                
            }
        }

        private void cmbProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          try
          {
            ComboBoxItem cmbItem = (ComboBoxItem)cmbProject.SelectedItem;
            ScopeProject project = (ScopeProject)cmbItem.Tag;

            foreach (ScopeApplication application in project.Applications ) 
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

            ScopeProject project = (ScopeProject)((ComboBoxItem)cmbProject.SelectedItem).Tag;
            ScopeApplication application = (ScopeApplication)((ComboBoxItem)cmbApp.SelectedItem).Tag;

            _dal.GetDbDictionary(project.Name, application.Name);
          }
          catch (Exception ex)
          {
            
          }
          
        }

        private void btnSaveDbDictionary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                biBusyWindow.IsBusy = true;
                string projectName = _project;
                string applicationName = _application;

                DatabaseDictionary databaseDictionary = new DatabaseDictionary();
                object currentObject = null;
                org.iringtools.library.DataObject table;
                databaseDictionary.dataObjects = new List<org.iringtools.library.DataObject>();
                databaseDictionary.connectionString = ((DatabaseDictionary)tvwItemDestinationRoot.Tag).connectionString;
                
                databaseDictionary.provider = ((DatabaseDictionary)tvwItemDestinationRoot.Tag).provider;
                foreach (TreeViewItem tableTreeViewItem in tvwItemDestinationRoot.Items)
                {
                  table = new org.iringtools.library.DataObject();
                    currentObject = tableTreeViewItem.Tag;
                    if (currentObject is org.iringtools.library.DataObject)
                    {
                      table.objectName = ((org.iringtools.library.DataObject)currentObject).objectName;
                      table.tableName = ((org.iringtools.library.DataObject)currentObject).tableName;
                        table.keyProperties = new List<KeyProperty>();
                        table.dataRelationships = new List<DataRelationship>();
                        table.dataProperties = new List<DataProperty>();
                    }
                    foreach (TreeViewItem columnTreeViewItem in tableTreeViewItem.Items)
                    {
                        currentObject = columnTreeViewItem.Tag;
                        if (((DataProperty)currentObject).keyType != KeyType.unassigned)
                        {
                          org.iringtools.library.KeyProperty key = new org.iringtools.library.KeyProperty();
                          key.keyPropertyName = ((DataProperty)currentObject).propertyName;
                          table.keyProperties.Add(key);
                        }

                        DataProperty column = new DataProperty();
                        column.columnName = ((DataProperty)currentObject).columnName;
                        column.dataLength = ((DataProperty)currentObject).dataLength;
                        column.dataType = ((DataProperty)currentObject).dataType;
                        column.isNullable = ((DataProperty)currentObject).isNullable;
                        column.keyType = ((DataProperty)currentObject).keyType;
                        column.propertyName = ((DataProperty)currentObject).propertyName;
                        table.dataProperties.Add(column);
                    }
                    databaseDictionary.dataObjects.Add(table);
                }

                _dal.SaveDatabaseDictionary(databaseDictionary, projectName, applicationName);
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
                dbdict = (DatabaseDictionary)tvwItemDestinationRoot.Tag;
                biBusyWindow.IsBusy = true;
                isPosting = true;
                              
                _dal.PostDictionaryToAdapterService(_project, _application);
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
                StackPanel stackPanel;
                CheckBox checkBox;
                TextBlock textBlock;
                TreeViewItem sourceRoot = tvwItemSourceRoot;
                TreeViewItem destRoot = tvwItemDestinationRoot;
                TreeViewItem tableItem = new TreeViewItem();
                TreeViewItem columnItem = new TreeViewItem();

                for (int i = 0; i < sourceRoot.Items.Count; i++)
                {
                    tableItem = (TreeViewItem)sourceRoot.Items[i];
                    TreeViewItem parent = tableItem.Parent as TreeViewItem;
                    stackPanel = (StackPanel)tableItem.Header;
                    textBlock = (TextBlock)stackPanel.Children[1];
                    checkBox = (CheckBox)stackPanel.Children[0];
                    if (checkBox.IsChecked.Value.Equals(true))
                    {
                        if (!destRoot.Items.Contains(tableItem))
                        {
                            if ((!destRoot.Items.Contains(parent)) &
                                (!parent.Header.ToString().Equals("Available Database Schema Items")))
                            {
                                TreeViewItem parentParent = parent.Parent as TreeViewItem;
                                parentParent.Items.Add(parent);
                            }
                            RemoveTreeItem(parent, tableItem);
                            destRoot.Items.Add(tableItem);
                            i--;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred... \r\n" + ex.Message + ex.StackTrace, "Application Editor Error", MessageBoxButton.OK);                
            }
        }
        
        private void RemoveTreeItem(TreeViewItem parentItem, TreeViewItem child)
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
                StackPanel stackPanel;
                CheckBox checkBox;
                TreeViewItem root = tvwItemDestinationRoot;
                TreeViewItem tableItem;
                TreeViewItem columnItem;
                for (int i = 0; i < root.Items.Count; i++)
                {
                    tableItem = (TreeViewItem)root.Items[i];
                    TreeViewItem parent = tableItem.Parent as TreeViewItem;
                    stackPanel = (StackPanel)tableItem.Header;
                    checkBox = (CheckBox)stackPanel.Children[0];
                    if (checkBox.IsChecked.Value.Equals(true))
                    {
                        RemoveTreeItem(parent, tableItem);//.Items.Remove(tableItem);
                        i--;
                    }
                    else
                    {
                        for (int j = 0; j < tableItem.Items.Count; j++)
                        {
                            columnItem = (TreeViewItem)tableItem.Items[j];
                            TreeViewItem colParent = columnItem.Parent as TreeViewItem;
                            stackPanel = (StackPanel)columnItem.Header;
                            checkBox = (CheckBox)stackPanel.Children[0];
                            if (checkBox.IsChecked.Value.Equals(true))
                            {
                                RemoveTreeItem(colParent, columnItem);// .Items.Remove(columnItem);
                                j--;
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

        private void btnDelScope_Click(object sender, RoutedEventArgs e)
        {
          try
          {
            
            if (tvwScopes.SelectedItem != null) {

              TreeViewItem selectedItem = (TreeViewItem)tvwScopes.SelectedItem;
              object oItem = selectedItem.Tag;

              if (oItem is ScopeProject)
              {
                _scopes.Remove((ScopeProject)oItem);
                tvwScopes.Items.Remove(selectedItem);
              }
              else if (oItem is ScopeApplication)
              {
                TreeViewItem parentItem = (TreeViewItem)selectedItem.Parent;
                ScopeProject project = (ScopeProject)parentItem.Tag;

                project.Applications.Remove((ScopeApplication)oItem);
                parentItem.Items.Remove(selectedItem);
              }

            }

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
            if (tbNewPrjName.Text != null && tbNewAppName != null)
            {
              Collection<ScopeProject> scopes = (Collection<ScopeProject>)tvwScopesItemRoot.Tag;

              TreeViewItem tvwPrj = null;

              foreach (TreeViewItem tvwItem in tvwScopesItemRoot.Items ) {
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
                scopes.Add(project);
                tvwScopesItemRoot.Items.Add(tvwPrj);
              }

              TreeViewItem tvwApp = null;

              foreach (TreeViewItem tvwItem in tvwPrj.Items ) {
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
              }
            }
          }
          catch
          {
          }
        }

        private void btnPostScope_Click(object sender, RoutedEventArgs e)
        {
          Collection<ScopeProject> scopes = (Collection<ScopeProject>)tvwScopesItemRoot.Tag;
          _dal.UpdateScopes(scopes);
        }
        
    }
}
