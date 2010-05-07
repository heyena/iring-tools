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
using System.Windows.Interactivity;
using System.Text;
using System.Net;
using org.iringtools.utility;
using org.iringtools.modulelibrary.events;


namespace DbDictionaryEditor
{
    public partial class Page : UserControl
    {
        private NewDbDictionary newDbDictionary;
        private ResultsList resultsList;
        private EditTreeNode editTreeNode;
        List<string> dbDictionaries;
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

        private DBDictionaryEditorDAL _dal;

        public event System.EventHandler<System.EventArgs> OnDataArrived;

        public Page()
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

            _dal = new DBDictionaryEditorDAL();

            _dal.OnDataArrived += dal_OnDataArrived;

            LayoutRoot.SizeChanged += new SizeChangedEventHandler(LayoutRoot_SizeChanged);
            
            _dal.GetExistingDbDictionaryFiles();

            isPosting = false;
        }

        void dal_OnDataArrived(object sender, System.EventArgs e)
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

                case CompletedEventType.GetExistingDbDictionaryFiles:
                    getdbdictionariesComplete(args);
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

                case CompletedEventType.SaveDatabaseDictionary:
                    savedbdictionaryComplete(args);
                    break;

                default:
                    break;
            }
        }

        void editTreeNode_Closed(object sender, EventArgs e)
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
                                if (selectedItem.Tag is Table)
                                {
                                    if (tbox.Tag == "entityName")
                                    {
                                        ((Table)selectedItem.Tag).entityName = tbox.Text;
                                        textBlock.Text = tbox.Text;
                                    }
                                    else if (tbox.Tag == "tableName")
                                      ((Table)selectedItem.Tag).tableName = tbox.Text;
                                }
                                else if (selectedItem.Tag is Column)
                                {
                                    if (tbox.Tag == "columnName")
                                    {
                                        ((Column)selectedItem.Tag).columnName = tbox.Text;
                                        textBlock.Text = tbox.Text;
                                    }
                                    else if (tbox.Tag == "propertyName")
                                        ((Column)selectedItem.Tag).propertyName = tbox.Text;
                                }
                            }
                        }
                    }
                }
            }
        }

        TreeViewItem FindFirstCheckedTreeItem(TreeViewItem root)
        {
            StackPanel stackPanel;
            CheckBox checkBox;
            TreeViewItem treeViewItem = new TreeViewItem();
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
            return treeViewItem;
        }

        void deleteComplete(CompletedEventArgs args)
        {
            Response response = (Response)args.Data;
         
            string dictionaries = cbDictionary.SelectedItem.ToString();
            string project = dictionaries.Split('.')[1];
            string application = dictionaries.Split('.')[2];

            resultsList.lbResult.ItemsSource = response;

            _dal.GetDbDictionary(project, application);            
        }

        void results_Closed(object sender, EventArgs e)
        {
            if ((bool)resultsList.DialogResult)
            {

            }
        }

        void postdbdictionaryComplete(CompletedEventArgs args)
        {
            Response response = (Response)args.Data;
            
            resultsList.lbResult.ItemsSource = response;
            
            biBusyWindow.IsBusy = false;
            isPosting = false;
            resultsList.Show();
        }

        void clearTripleStoreComplete(CompletedEventArgs args)
        {
            Response resp = (Response)args.Data;

            string dictionary = cbDictionary.SelectedItem.ToString();
            
            string project = dictionary.Split('.')[1];
            string application = dictionary.Split('.')[2];
           
            resultsList.lbResult.ItemsSource = resp;

            //_dal.PostDictionaryToAdapterService(project, application);
        }

        void getProvidersComplete(CompletedEventArgs args)
        {
            newDbDictionary.cbProvider.ItemsSource = (string[])args.Data;
        }

        void getdbdictionariesComplete(CompletedEventArgs args)
        {
            dbDictionaries = (List<string>)args.Data;
            cbDictionary.IsEnabled = true;
           
            List<string> dbDict = dbDictionaries.ToList<string>();
            dbDict.Sort();

            
            cbDictionary.ItemsSource = dbDict;
            if (cbDictionary.Items.Count > 0)
            {
                if (newDictionary != null)
                {
                    if (cbDictionary.Items.Contains(newDictionary.ToString()))
                        cbDictionary.SelectedIndex = cbDictionary.Items.IndexOf(newDictionary.ToString());

                    _dal.GetDbDictionary(newDictionary.ToString().Split('.')[1], newDictionary.ToString().Split('.')[2]);
                    newDictionary = null;
                }
                else
                {
                    if (string.IsNullOrEmpty(selectedCBItem))
                    {
                        cbDictionary.SelectedIndex = 0;
                        selectedCBItem = cbDictionary.SelectedItem.ToString();
                    }
                    cbDictionary.SelectedIndex = cbDictionary.Items.IndexOf(selectedCBItem);
                }
                
                _dal.GetDbDictionary(selectedCBItem.Split('.')[1], selectedCBItem.Split('.')[2]);
            }
            biBusyWindow.IsBusy = false;
        }

        void newDbDictionary_Closed(object sender, EventArgs e)
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

        private void BuildNewDbDictionary(string newProvider, string newProject, string newApplication, string newDataSourceName, string newDatabaseName, string newDatabaseUserName, string newDatabaseUserPassword)
        {
            newDictionary = new StringBuilder();
            newDictionary.Append("DatabaseDictionary.");
            newDictionary.Append(newProject);
            newDictionary.Append(".");
            newDictionary.Append(newApplication);
            newDictionary.Append(".xml");

           string connectionstring = BuildConnectionString(newProvider, newDataSourceName, newDatabaseName, newDatabaseUserName, newDatabaseUserPassword);
            Table table = new Table();
            DatabaseDictionary dict = new DatabaseDictionary()
            {
                connectionString = connectionstring,
                provider = (Provider)Enum.Parse(typeof(Provider), newProvider, true),
                tables = new List<Table>()
            };
            
            _dal.SaveDatabaseDictionary(dict, newProject, newApplication);
        }

        private string BuildConnectionString(string newProvider, string newDataSourceName, string newDatabaseName, string newDatabaseUserName, string newDatabaseUserPassword)
        {
            newProvider = newProvider.ToUpper();
            string connString = string.Empty;
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
                connString = "";// TODO
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
            return connString;
        }

        void savedbdictionaryComplete(CompletedEventArgs args)
        {
            _dal.GetExistingDbDictionaryFiles();
            //tvwItemDestinationRoot.Items.Clear();
        }

        void getdbschemaComplete(CompletedEventArgs args)
        {
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

            biBusyWindow.IsBusy = false;
        }

        void getdbDictionaryComplete(CompletedEventArgs args)
        {
            DatabaseDictionary dict = (DatabaseDictionary)args.Data;
            if (isPosting)
            {
                 string project = cbDictionary.SelectedItem.ToString().Split('.')[1];
                 string application = cbDictionary.SelectedItem.ToString().Split('.')[2];
                 _dal.PostDictionaryToAdapterService(project, application, dict);                
            }
            else
            {
                tvwItemDestinationRoot.Items.Clear();                

                _dal.GetDatabaseSchema(dict.connectionString, dict.provider.ToString());
                ConstructTreeView(dict, tvwItemDestinationRoot);            
            }
        }
        
        void ConstructTreeView(DatabaseDictionary dict, TreeViewItem root)
        {
            TreeViewItem tableTreeViewItem = null;
            TreeViewItem columnTreeViewItem = null;
            bool enableCheckBox = false;
            if (root.Name != "tvwItemSourceRoot")
                enableCheckBox = true;
            try
            {
                root.Tag = dict.connectionString + "~" + dict.provider;
                if (dict.tables == null)
                    dict.tables = new List<Table>();
                foreach (Table table in dict.tables)
                {
                    tableTreeViewItem = new TreeViewItem() { Header = table.tableName };
                    tableTreeViewItem.Tag = table;
                    root.IsExpanded = true;

                    foreach (org.iringtools.library.Key key in table.keys)
                    {
                        columnTreeViewItem = new TreeViewItem();
                        columnTreeViewItem.Tag = key;
                        AddTreeItem(tableTreeViewItem, columnTreeViewItem, key.columnName, "Magenta", false);
                        AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Data Length = " + key.dataLength.ToString(), null, false);
                        AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Column Type = " + key.columnType.ToString(), null, false);
                      //  AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Data Type = " + key.dataType.ToString(), null, false);
                        AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Is Nullable = " + key.isNullable, null, false);
                        AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Key Type = " + key.keyType, null, false);
                        AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Property Name = " + key.propertyName, null, false);
                    }
                    foreach (Column column in table.columns)
                    {
                        columnTreeViewItem = new TreeViewItem();
                        columnTreeViewItem.Tag = column;
                        AddTreeItem(tableTreeViewItem, columnTreeViewItem, column.columnName, null, enableCheckBox);
                        AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Data Length = " + column.dataLength.ToString(), null, false);
                        AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Column Type = " + column.columnType.ToString(), null, false);
                     //   AddTreeItem(columnTreeViewItem, new TreeViewItem(), "Data Type = " + column.dataType.ToString(), null, false);
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
            
            StackPanel stackpanel = new StackPanel(){ Orientation = Orientation.Horizontal };
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
            if (child.Tag is Table && !checkBox)
                checkbox.IsEnabled = true;
            stackpanel.Children.Add(checkbox);
            stackpanel.Children.Add(textblock);
            child.Header = stackpanel;
            //child.FontSize = 12;
            child.Expanded += new RoutedEventHandler(itm_Expanded);
            
            parent.Items.Add(child);
        }

        void itm_Checked(object sender, RoutedEventArgs e)
        {
        }
        
        void itm_Expanded(object sender, RoutedEventArgs e)
        {
        }

        void getScopesComplete(CompletedEventArgs args)
        {
            try
            {
                cbDictionary.IsEnabled = false;
  
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
            _dal.GetProviders();
            newDbDictionary.tbMessages.Text = string.Empty;
            newDbDictionary.Show();  
        }

        private void cbProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDictionary.SelectedIndex != -1)
            {
                tvwItemSourceRoot.Items.Clear();
                tvwItemSourceRoot.Visibility = Visibility.Collapsed;
                tvwItemDestinationRoot.Items.Clear();
                tvwItemDestinationRoot.Visibility = Visibility.Collapsed;

                selectedCBItem = cbDictionary.SelectedItem.ToString();
                
                _dal.GetDbDictionary(selectedCBItem.Split('.')[1], selectedCBItem.Split('.')[2]); 
            }
        }

        private void btnSaveDbDictionary_Click(object sender, RoutedEventArgs e)
        {
            biBusyWindow.IsBusy = true;
            selectedCBItem = cbDictionary.SelectedItem.ToString();
            string projectName = cbDictionary.SelectedItem.ToString().Split('.')[1];
            string applicationName = cbDictionary.SelectedItem.ToString().Split('.')[2];
            
            DatabaseDictionary databaseDictionary = new DatabaseDictionary();
            object currentObject = null;
            Table table;
            databaseDictionary.tables = new List<Table>();
            databaseDictionary.connectionString = tvwItemDestinationRoot.Tag.ToString().Split('~')[0];
            string provider = tvwItemDestinationRoot.Tag.ToString().Split('~')[1];
            databaseDictionary.provider = (Provider)Enum.Parse(typeof(Provider), provider, true);
            foreach (TreeViewItem tableTreeViewItem in tvwItemDestinationRoot.Items)
            {
                table = new Table();
                currentObject = tableTreeViewItem.Tag;
                if (currentObject is Table)
                {
                    table.entityName = ((Table)currentObject).entityName;
                    table.tableName = ((Table)currentObject).tableName;
                    table.keys = new List<org.iringtools.library.Key>();
                    table.associations = new List<Association>();
                    table.columns = new List<Column>();
                }
                foreach (TreeViewItem columnTreeViewItem in tableTreeViewItem.Items)
                {
                    currentObject = columnTreeViewItem.Tag;
                    if (currentObject is org.iringtools.library.Key)
                    { 
                    //    DataType dataType =  (DataType)Enum.Parse(typeof(DataType),((org.iringtools.library.Key)currentObject).dataType.ToString(),true);
                        org.iringtools.library.Key key = new org.iringtools.library.Key();
                        key.columnName = ((org.iringtools.library.Key)currentObject).columnName;
                        key.dataLength = ((org.iringtools.library.Key)currentObject).dataLength;
                    //    key.dataType = dataType;
                        key.isNullable = ((org.iringtools.library.Key)currentObject).isNullable;
                        key.propertyName = ((org.iringtools.library.Key)currentObject).propertyName;
                        table.keys.Add(key);        
                    }
                    else
                    {
                    //    DataType dataType = (DataType)Enum.Parse(typeof(DataType), ((Column)currentObject).dataType.ToString(), true);
                        Column column = new Column();
                        column.columnName = ((Column)currentObject).columnName;
                        column.dataLength = ((Column)currentObject).dataLength;
                   //     column.dataType = dataType;
                        column.isNullable = ((Column)currentObject).isNullable;
                        column.propertyName = ((Column)currentObject).propertyName;
                        table.columns.Add(column); 
                    }

                }
                databaseDictionary.tables.Add(table);
            }
            
            _dal.SaveDatabaseDictionary(databaseDictionary, projectName, applicationName);
        }

        private void btnPostDictionary_Click(object sender, RoutedEventArgs e)
        {
            biBusyWindow.IsBusy = true;
            isPosting = true;

            string project = cbDictionary.SelectedItem.ToString().Split('.')[1];
            string application = cbDictionary.SelectedItem.ToString().Split('.')[2];
            
            _dal.DeleteApp(project, application);
        }

        private void clearComboBox(ComboBox combox)
        {
            if (combox.ItemsSource != null)
            {
                combox.ItemsSource = null;
            }

            combox.IsEnabled = false;

        }

        private void btnAddColumnToDict_Click(object sender, RoutedEventArgs e)
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
        
        private void RemoveTreeItem(TreeViewItem parentItem, TreeViewItem child)
        {
            TreeViewItem parent = child.Parent as TreeViewItem;
            parent.Items.Remove(child);
        }

        private void btnDelColFromDict_Click(object sender, RoutedEventArgs e)
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

        private void btnEditNode_Click(object sender, RoutedEventArgs e)
        {
            editTreeNode.spContainer.Children.Clear();

            StackPanel stackPanel;

            TextBlock textBlock;
            TextBox textBox;
            TreeViewItem selectedItem = FindFirstCheckedTreeItem(tvwItemDestinationRoot);
             
         
                TreeViewItem treeViewItem = (TreeViewItem)selectedItem;
                if (selectedItem.Tag is Table)
                {
                    stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                    textBlock = CreateTextBlock("      --==  Edit Table  ==--    ");
                    textBlock.FontSize = 14;
                    stackPanel.Children.Add(textBlock);
                    editTreeNode.spContainer.Children.Add(stackPanel);

                    stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                    textBlock = CreateTextBlock("Entity Name: ");
                    textBox = CreateTextBox(((Table)selectedItem.Tag).entityName, "entityName");
                    stackPanel.Children.Add(textBlock);
                    stackPanel.Children.Add(textBox);
                    editTreeNode.spContainer.Children.Add(stackPanel);
                    stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                    textBlock = CreateTextBlock("Table Name: ");
                    textBox = CreateTextBox(((Table)selectedItem.Tag).tableName,"tableName");
                    stackPanel.Children.Add(textBlock);
                    stackPanel.Children.Add(textBox);
                    editTreeNode.spContainer.Children.Add(stackPanel);
                    editTreeNode.Show();
                } 
                else if (selectedItem.Tag is Column)
                {
                     stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                     textBlock = CreateTextBlock("      --==  Edit Column  ==--    ");
                     textBlock.FontSize = 14;
                     stackPanel.Children.Add(textBlock);
                     editTreeNode.spContainer.Children.Add(stackPanel);
                     stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                     textBlock = CreateTextBlock("Column Name: ");
                     textBox = CreateTextBox(((Column)selectedItem.Tag).columnName,"columnName");
                      stackPanel.Children.Add(textBlock);
                      stackPanel.Children.Add(textBox);
                      editTreeNode.spContainer.Children.Add(stackPanel);

                      stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                      textBlock = CreateTextBlock("   Data Length: ");
                      textBox = CreateTextBox(((Column)selectedItem.Tag).dataLength.ToString(),"dataLength");
                      stackPanel.Children.Add(textBlock);
                      stackPanel.Children.Add(textBox);
                      editTreeNode.spContainer.Children.Add(stackPanel);
                            
                      stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                      textBlock = CreateTextBlock("      Column Type: ");
                      textBox = CreateTextBox(Enum.GetName(typeof(ColumnType), ((Column)selectedItem.Tag).columnType),"ColumnType");
                   //   textBlock = CreateTextBlock("      Data Type: ");
                   //   textBox = CreateTextBox(Enum.GetName(typeof(DataType), ((Column)selectedItem.Tag).dataType),"dataType");
                      stackPanel.Children.Add(textBlock);
                      stackPanel.Children.Add(textBox);
                      editTreeNode.spContainer.Children.Add(stackPanel);

                      stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                      textBlock = CreateTextBlock("       IsNullable: ");
                      textBox = CreateTextBox(((Column)selectedItem.Tag).isNullable.ToString(), "isNullable");
                      stackPanel.Children.Add(textBlock);
                      stackPanel.Children.Add(textBox);
                      editTreeNode.spContainer.Children.Add(stackPanel);

                      stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                      textBlock = CreateTextBlock("Property Name :");
                      textBox = CreateTextBox(((Column)selectedItem.Tag).propertyName, "propertyName");
                      stackPanel.Children.Add(textBlock);
                      stackPanel.Children.Add(textBox);
                      editTreeNode.spContainer.Children.Add(stackPanel);

                      editTreeNode.Show();
                }                         
        }

        private TextBox CreateTextBox(string text, string tag)
        {
            TextBox textBox = new TextBox() { Text = text };
            textBox.Tag = tag;
            textBox.Width = 100;
            textBox.Height = 24;
            return textBox;
        }

        private TextBlock CreateTextBlock(string text)
        {
            TextBlock textBlock = new TextBlock() { Text = text };
            textBlock.Height = 24;
            textBlock.Margin = new Thickness() { Top = 5, Left = 5 };
            return textBlock;
        }
    }
}
