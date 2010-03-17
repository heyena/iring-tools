using System;
using System.IO;
using System.Collections.Generic;
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
using org.iringtools.library;
using org.iringtools.utility;
using DbDictionaryEditor.ServiceRef;
using System.Windows.Interactivity;
using System.Windows.Controls.Primitives;
using System.Text;


namespace DbDictionaryEditor
{
    public partial class Page : UserControl
    {
        DbDictionaryServiceClient proxy = null;
        ObservableCollection<ScopeProject> _project = null;
        private NewDbDictionary newDbDictionary;
        private ResultsList resultsList;
        private EditTreeNode editTreeNode;
        ObservableCollection<string> dbDictionaries;
        public string newProvider;
        public string newProject;
        public string newApplication;
        public string newDataSourceName;
        public string newDatabaseName;
        public string newDatabaseUserName;
        public string newDatabaseUserPassword;

        public Page()
        {
            InitializeComponent();
            string uriScheme = Application.Current.Host.Source.Scheme;
            bool usingTransportSecurity = uriScheme.Equals("https", StringComparison.InvariantCultureIgnoreCase);
            newDbDictionary = new NewDbDictionary();
            newDbDictionary.Closed += new EventHandler(newDbDictionary_Closed);
            resultsList = new ResultsList();
            resultsList.Closed += new EventHandler(results_Closed);
            editTreeNode = new EditTreeNode();
            editTreeNode.Closed += new EventHandler(editTreeNode_Closed);
            BasicHttpSecurityMode secirityMode;
            if (usingTransportSecurity)
                secirityMode = BasicHttpSecurityMode.Transport;
            else
                secirityMode = BasicHttpSecurityMode.None;


            BasicHttpBinding binding = new BasicHttpBinding(secirityMode);
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.MaxBufferSize = int.MaxValue;
            TimeSpan timeout;
            TimeSpan.TryParse("00:10:00", out timeout);
            binding.OpenTimeout = timeout;
            binding.CloseTimeout = timeout;
            binding.ReceiveTimeout = timeout;
            binding.SendTimeout = timeout;

            Uri uri = new Uri(Application.Current.Host.Source, "../DbDictionaryService.svc");

            proxy = new DbDictionaryServiceClient(binding, new EndpointAddress(uri));
            proxy.GetScopesCompleted += new EventHandler<GetScopesCompletedEventArgs>(proxy_GetScopesCompleted);
            proxy.GetDbDictionaryCompleted += new EventHandler<GetDbDictionaryCompletedEventArgs>(proxy_GetDbDictionaryCompleted);
            proxy.GetDatabaseSchemaCompleted += new EventHandler<GetDatabaseSchemaCompletedEventArgs>(proxy_GetDatabaseSchemaCompleted);
            proxy.SaveDabaseDictionaryCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(proxy_SaveDabaseDictionaryCompleted);
            proxy.GetExistingDbDictionaryFilesCompleted += new EventHandler<GetExistingDbDictionaryFilesCompletedEventArgs>(proxy_GetExistingDbDictionaryFilesCompleted);
            proxy.GetProvidersCompleted += new EventHandler<GetProvidersCompletedEventArgs>(proxy_GetProvidersCompleted);
            proxy.ClearTripleStoreCompleted += new EventHandler<ClearTripleStoreCompletedEventArgs>(proxy_ClearTripleStoreCompleted);
            proxy.PostDictionaryToAdapterServiceCompleted += new EventHandler<PostDictionaryToAdapterServiceCompletedEventArgs>(proxy_PostDictionaryToAdapterServiceCompleted);
            proxy.DeleteAppCompleted += new EventHandler<DeleteAppCompletedEventArgs>(proxy_DeleteAppCompleted);

            LayoutRoot.SizeChanged += new SizeChangedEventHandler(LayoutRoot_SizeChanged);

            MouseScrollBehavior mouseScrollBehaviorSource = new MouseScrollBehavior();
            Interaction.GetBehaviors(tvwSource).Add(mouseScrollBehaviorSource);
            
            MouseScrollBehavior mouseScrollBehaviorDest = new MouseScrollBehavior();
            Interaction.GetBehaviors(tvwDestination).Add(mouseScrollBehaviorDest);
            
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
            StackPanel sp;
            CheckBox cb;
            TreeViewItem tvi = new TreeViewItem();
            foreach (TreeViewItem table in root.Items)
            {
                sp = (StackPanel)table.Header;
                cb = (CheckBox)sp.Children[0];
                if (cb.IsChecked.Value.Equals(true))
                    tvi = table;
                else
                {
                    foreach (TreeViewItem column in table.Items)
                    {
                        sp = (StackPanel)column.Header;
                        cb = (CheckBox)sp.Children[0];
                        if (cb.IsChecked.Value.Equals(true))
                            tvi = column;
                    }
                }
            }
            return tvi;
        }

        void proxy_DeleteAppCompleted(object sender, DeleteAppCompletedEventArgs e)
        {
            resultsList.lbResult.Items.Clear();
            ServiceRef.Response resp = e.Result;
            string dictionaries = cbDictionary.SelectedItem.ToString();
            string project = dictionaries.Split('.')[1];
            string application = dictionaries.Split('.')[2];

            ListBoxItem lbi;
            foreach (string res in resp)
            {
                lbi = new ListBoxItem() { Content = res };
                resultsList.lbResult.Items.Add(lbi);
            }
            proxy.PostDictionaryToAdapterServiceAsync(project, application);
        }

        void results_Closed(object sender, EventArgs e)
        {
            if ((bool)resultsList.DialogResult)
            {

            }
        }

        void proxy_PostDictionaryToAdapterServiceCompleted(object sender, PostDictionaryToAdapterServiceCompletedEventArgs e)
        {
            ServiceRef.Response response = e.Result;
            ListBoxItem lbi;
            resultsList.lbResult.Items.Clear();
            foreach (string res in response)
            {
                lbi = new ListBoxItem() { Content = res };
                resultsList.lbResult.Items.Add(lbi);
            }
            biBusyWindow.IsBusy = false;
            resultsList.Show();
        }

        void proxy_ClearTripleStoreCompleted(object sender, ClearTripleStoreCompletedEventArgs e)
        {
            resultsList.lbResult.Items.Clear();
            ServiceRef.Response resp = e.Result;
            string dictionary = cbDictionary.SelectedItem.ToString();
            string project = dictionary.Split('.')[1];
            string application = dictionary.Split('.')[2];
            ListBoxItem lbi;
            foreach (string res in resp)
            {
                lbi = new ListBoxItem() { Content = res };
                resultsList.lbResult.Items.Add(lbi);
            }
            proxy.PostDictionaryToAdapterServiceAsync(project, application);
        }

        void proxy_GetProvidersCompleted(object sender, GetProvidersCompletedEventArgs e)
        {
            newDbDictionary.cbProvider.ItemsSource = e.Result;
        }

        void proxy_GetExistingDbDictionaryFilesCompleted(object sender, GetExistingDbDictionaryFilesCompletedEventArgs e)
        {
            dbDictionaries = e.Result;
            
            List<string> dbDict = dbDictionaries.ToList<string>();
            dbDict.Sort();

            cbDictionary.IsEnabled = false;
            cbDictionary.ItemsSource = dbDict;

            if (!cbDictionary.Items.Count.Equals(0))
                cbDictionary.SelectedIndex = 0;
            cbDictionary.IsEnabled = true;
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
            StringBuilder newDictionary = new StringBuilder();
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
            if (!cbDictionary.Items.Contains(newDictionary))
                cbDictionary.Items.Add(newDictionary);
            cbDictionary.SelectedItem = cbDictionary.Items.IndexOf(newDictionary);
            proxy.SaveDabaseDictionaryAsync(dict, newProject, newApplication);
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
                connString = "";
            } 
            else if (newProvider.Contains("MYSQL"))
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

        void proxy_SaveDabaseDictionaryCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            string proj = cbDictionary.SelectedItem.ToString().Split('.')[1];
            string app = cbDictionary.SelectedItem.ToString().Split('.')[2];
           
            tvwItemDestinationRoot.Items.Clear();
            proxy.GetDbDictionaryAsync(proj, app);
        }

        void proxy_GetDatabaseSchemaCompleted(object sender, GetDatabaseSchemaCompletedEventArgs e)
        {
            TreeViewItem stab;
            TreeViewItem dtab;
      
            tvwItemSourceRoot.Items.Clear();
            DatabaseDictionary dict = e.Result;
            ConstructTreeView(dict, tvwItemSourceRoot);
            for (int sourceTables = 0; sourceTables < tvwItemSourceRoot.Items.Count; sourceTables++)
            {
                stab = (TreeViewItem)tvwItemSourceRoot.Items[sourceTables];
                StackPanel ssp = (StackPanel)stab.Header;
                TextBlock stb = (TextBlock)ssp.Children[1];
                TreeViewItem sourceParent = stab.Parent as TreeViewItem;
                for (int destTables = 0; destTables < tvwItemDestinationRoot.Items.Count; destTables++)
                {
                    dtab = (TreeViewItem)tvwItemDestinationRoot.Items[destTables];
                    StackPanel dsp = (StackPanel)dtab.Header;
                    TextBlock dtb = (TextBlock)dsp.Children[1];
                    if (stb.Text == dtb.Text)
                    {
                        RemoveTreeItem(sourceParent, stab);
                        sourceTables--;
                        break;
                    }
                }
            }
            biBusyWindow.IsBusy = false;
        }

        void proxy_GetDbDictionaryCompleted(object sender, GetDbDictionaryCompletedEventArgs e)
        {
            
            tvwItemDestinationRoot.Items.Clear();
            DatabaseDictionary dict = e.Result;
            proxy.GetDatabaseSchemaAsync(dict.connectionString, dict.provider.ToString());
            ConstructTreeView(dict, tvwItemDestinationRoot);
        }
        
        void ConstructTreeView(DatabaseDictionary dict, TreeViewItem root)
        {
            TreeViewItem itmTable = null;
            TreeViewItem itmColumn = null;
            bool enableCheckBox = false;
            if (root.Name != "tvwItemSourceRoot")
                enableCheckBox = true;
            try
            {
                root.Tag = dict.connectionString + "~" + dict.provider;
                foreach (Table tab in dict.tables)
                {
                    itmTable = new TreeViewItem() { Header = tab.tableName };
                    itmTable.Tag = tab;
                    root.IsExpanded = true;

                    foreach (org.iringtools.library.Key k in tab.keys)
                    {
                        itmColumn = new TreeViewItem();
                        itmColumn.Tag = k;
                        AddTreeItem(itmTable, itmColumn, k.columnName, "Magenta", false);
                        AddTreeItem(itmColumn, new TreeViewItem(), "Data Length = " + k.dataLength.ToString(), null, false);
                        AddTreeItem(itmColumn, new TreeViewItem(), "Data Type = " + k.dataType.ToString(), null, false);
                        AddTreeItem(itmColumn, new TreeViewItem(), "Is Nullable = " + k.isNullable, null, false);
                        AddTreeItem(itmColumn, new TreeViewItem(), "Key Type = " + k.keyType, null, false);
                        AddTreeItem(itmColumn, new TreeViewItem(), "Property Name = " + k.propertyName, null, false);
                    }
                    foreach (Column col in tab.columns)
                    {
                        itmColumn = new TreeViewItem();
                        itmColumn.Tag = col;
                        AddTreeItem(itmTable, itmColumn, col.columnName, null, enableCheckBox);
                        AddTreeItem(itmColumn, new TreeViewItem(), "Data Length = " + col.dataLength.ToString(), null, false);
                        AddTreeItem(itmColumn, new TreeViewItem(), "Data Type = " + col.dataType.ToString(), null, false);
                        AddTreeItem(itmColumn, new TreeViewItem(), "Is Nullable = " + col.isNullable, null, false);
                        AddTreeItem(itmColumn, new TreeViewItem(), "Property Name = " + col.propertyName, null, false);
                    }
                    AddTreeItem(root, itmTable, tab.tableName, null, enableCheckBox);
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
            
            StackPanel sp = new StackPanel(){ Orientation = Orientation.Horizontal };
            CheckBox cb = new CheckBox();
            
            cb.Checked += new RoutedEventHandler(itm_Checked);
            TextBlock tb = null;
            if (!string.IsNullOrEmpty(color))
                tb = new TextBlock() { Text = headerText, Foreground = new SolidColorBrush(Colors.Magenta) };
            else
                tb = new TextBlock() { Text = headerText };
            if (checkBox)
                cb.IsEnabled = true;
            else
                cb.IsEnabled = false;
            if (child.Tag is Table && !checkBox)
                cb.IsEnabled = true;
            sp.Children.Add(cb);
            sp.Children.Add(tb);
            child.Header = sp;
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


        void proxy_GetScopesCompleted(object sender, GetScopesCompletedEventArgs e)
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
            proxy.GetProvidersAsync();
            newDbDictionary.tbMessages.Text = string.Empty;
            newDbDictionary.Show();  
        }

        private void btnGetScopes_Click(object sender, RoutedEventArgs e)
        {
            biBusyWindow.IsBusy = true;
            tvwItemSourceRoot.Items.Clear();
            tvwItemSourceRoot.Visibility = Visibility.Collapsed;
            tvwItemDestinationRoot.Items.Clear();
            tvwItemDestinationRoot.Visibility = Visibility.Collapsed;
            proxy.GetExistingDbDictionaryFilesAsync();
        }


        private void cbProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDictionary.SelectedIndex != -1)
            {
                tvwItemSourceRoot.Items.Clear();
                tvwItemSourceRoot.Visibility = Visibility.Collapsed;
                tvwItemDestinationRoot.Items.Clear();
                tvwItemDestinationRoot.Visibility = Visibility.Collapsed;
            }
        }


        private void btnSaveDbDictionary_Click(object sender, RoutedEventArgs e)
        {
            biBusyWindow.IsBusy = true;
            string proj = cbDictionary.SelectedItem.ToString().Split('.')[1];
            string app = cbDictionary.SelectedItem.ToString().Split('.')[2];
            
            DatabaseDictionary dict = new DatabaseDictionary();
            object currentObject = null;
            Table tab;
            dict.tables = new List<Table>();
            dict.connectionString = tvwItemDestinationRoot.Tag.ToString().Split('~')[0];
            string prov = tvwItemDestinationRoot.Tag.ToString().Split('~')[1];
            dict.provider = (Provider)Enum.Parse(typeof(Provider), prov, true);
            foreach (TreeViewItem titm in tvwItemDestinationRoot.Items)
            {
                tab = new Table();
                currentObject = titm.Tag;
                if (currentObject is Table)
                {
                    tab.entityName = ((Table)currentObject).entityName;
                    tab.tableName = ((Table)currentObject).tableName;
                    tab.keys = new List<org.iringtools.library.Key>();
                    tab.associations = new List<Association>();
                    tab.columns = new List<Column>();
                }
                foreach (TreeViewItem citm in titm.Items)
                {
                    currentObject = citm.Tag;
                    if (currentObject is org.iringtools.library.Key)
                    { 
                        DataType dt =  (DataType)Enum.Parse(typeof(DataType),((org.iringtools.library.Key)currentObject).dataType.ToString(),true);
                        org.iringtools.library.Key key = new org.iringtools.library.Key();
                        key.columnName = ((org.iringtools.library.Key)currentObject).columnName;
                        key.dataLength = ((org.iringtools.library.Key)currentObject).dataLength;
                        key.dataType = dt;
                        key.isNullable = ((org.iringtools.library.Key)currentObject).isNullable;
                        key.propertyName = ((org.iringtools.library.Key)currentObject).propertyName;
                        tab.keys.Add(key);        
                    }
                    else
                    {
                        DataType dt = (DataType)Enum.Parse(typeof(DataType), ((Column)currentObject).dataType.ToString(), true);
                        Column col = new Column();
                        col.columnName = ((Column)currentObject).columnName;
                        col.dataLength = ((Column)currentObject).dataLength;
                        col.dataType = dt;
                        col.isNullable = ((Column)currentObject).isNullable;
                        col.propertyName = ((Column)currentObject).propertyName;
                        tab.columns.Add(col); 
                    }

                }
                dict.tables.Add(tab);
            }
            proxy.SaveDabaseDictionaryAsync(dict, proj, app);
        }

        private void btnPostDictionary_Click(object sender, RoutedEventArgs e)
        {
            biBusyWindow.IsBusy = true;
            string project = cbDictionary.SelectedItem.ToString().Split('.')[1];
            string application = cbDictionary.SelectedItem.ToString().Split('.')[2];
            proxy.DeleteAppAsync(project, application);
        }

        private void clearComboBox(ComboBox combox)
        {
            if (combox.ItemsSource != null)
            {
                combox.ItemsSource = null;
            }

            combox.IsEnabled = false;

        }

        private void btnLoadDictionary_Click(object sender, RoutedEventArgs e)
        {
            biBusyWindow.IsBusy = true;
            if (cbDictionary.SelectedIndex != -1)
            {
                tvwItemDestinationRoot.Items.Clear();
                string proj = cbDictionary.SelectedItem.ToString().Split('.')[1];
                string app = cbDictionary.SelectedItem.ToString().Split('.')[2];
                proxy.GetDbDictionaryAsync(proj, app);
            }
        }

        private void btnAddColumnToDict_Click(object sender, RoutedEventArgs e)
        {
            StackPanel sp;
            CheckBox cb;
            TextBlock tb;
            TreeViewItem sourceRoot = tvwItemSourceRoot;
            TreeViewItem destRoot = tvwItemDestinationRoot;
            TreeViewItem tableItem = new TreeViewItem();
            TreeViewItem columnItem = new TreeViewItem();

            for (int i = 0; i < sourceRoot.Items.Count; i++)
            {
                tableItem = (TreeViewItem)sourceRoot.Items[i];
                TreeViewItem parent = tableItem.Parent as TreeViewItem;
                sp = (StackPanel)tableItem.Header;
                tb = (TextBlock)sp.Children[1];
                cb = (CheckBox)sp.Children[0];
                if (cb.IsChecked.Value.Equals(true))
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
         
                int i = parentItem.Items.IndexOf(child);
                TreeViewItem parent = child.Parent as TreeViewItem;
                parent.Items.Remove(child);

        }

        private void btnDelColFromDict_Click(object sender, RoutedEventArgs e)
        {
            StackPanel sp;
            CheckBox cb;
            TreeViewItem root = tvwItemDestinationRoot;
            TreeViewItem tableItem;
            TreeViewItem columnItem;
            for (int i = 0; i < root.Items.Count; i++)
            {
                tableItem = (TreeViewItem)root.Items[i];
                TreeViewItem parent = tableItem.Parent as TreeViewItem;
                sp = (StackPanel)tableItem.Header;
                cb = (CheckBox)sp.Children[0];
                if (cb.IsChecked.Value.Equals(true))
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
                        sp = (StackPanel)columnItem.Header;
                        cb = (CheckBox)sp.Children[0];
                        if (cb.IsChecked.Value.Equals(true))
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

            StackPanel sp;

            TextBlock tblk;
            TextBox tbx;
            TreeViewItem selectedItem = FindFirstCheckedTreeItem(tvwItemDestinationRoot);
             
         
                TreeViewItem tvi = (TreeViewItem)selectedItem;
                if (selectedItem.Tag is Table)
                {
                    sp = new StackPanel() { Orientation = Orientation.Horizontal };
                    tblk = CreateTextBlock("      --==  Edit Table  ==--    ");
                    tblk.FontSize = 14;
                    sp.Children.Add(tblk);
                    editTreeNode.spContainer.Children.Add(sp);

                    sp = new StackPanel() { Orientation = Orientation.Horizontal };
                    tblk = CreateTextBlock("Entity Name: ");
                    tbx = CreateTextBox(((Table)selectedItem.Tag).entityName, "entityName");
                    sp.Children.Add(tblk);
                    sp.Children.Add(tbx);
                    editTreeNode.spContainer.Children.Add(sp);
                    sp = new StackPanel() { Orientation = Orientation.Horizontal };
                    tblk = CreateTextBlock("Table Name: ");
                    tbx = CreateTextBox(((Table)selectedItem.Tag).tableName,"tableName");
                    sp.Children.Add(tblk);
                    sp.Children.Add(tbx);
                    editTreeNode.spContainer.Children.Add(sp);
                    editTreeNode.Show();
                } 
                else if (selectedItem.Tag is Column)
                {
                     sp = new StackPanel() { Orientation = Orientation.Horizontal };
                     tblk = CreateTextBlock("      --==  Edit Column  ==--    ");
                     tblk.FontSize = 14;
                     sp.Children.Add(tblk);
                     editTreeNode.spContainer.Children.Add(sp);
                     sp = new StackPanel() { Orientation = Orientation.Horizontal };
                     tblk = CreateTextBlock("Column Name: ");
                     tbx = CreateTextBox(((Column)selectedItem.Tag).columnName,"columnName");
                      sp.Children.Add(tblk);
                      sp.Children.Add(tbx);
                      editTreeNode.spContainer.Children.Add(sp);

                      sp = new StackPanel() { Orientation = Orientation.Horizontal };
                      tblk = CreateTextBlock("   Data Length: ");
                      tbx = CreateTextBox(((Column)selectedItem.Tag).dataLength.ToString(),"dataLength");
                      sp.Children.Add(tblk);
                      sp.Children.Add(tbx);
                      editTreeNode.spContainer.Children.Add(sp);
                            
                      sp = new StackPanel() { Orientation = Orientation.Horizontal };
                      tblk = CreateTextBlock("      Data Type: ");
                      tbx = CreateTextBox(Enum.GetName(typeof(DataType), ((Column)selectedItem.Tag).dataType),"dataType");
                      sp.Children.Add(tblk);
                      sp.Children.Add(tbx);
                      editTreeNode.spContainer.Children.Add(sp);

                      sp = new StackPanel() { Orientation = Orientation.Horizontal };
                      tblk = CreateTextBlock("       IsNullable: ");
                      tbx = CreateTextBox(((Column)selectedItem.Tag).isNullable.ToString(), "isNullable");
                      sp.Children.Add(tblk);
                      sp.Children.Add(tbx);
                      editTreeNode.spContainer.Children.Add(sp);

                      sp = new StackPanel() { Orientation = Orientation.Horizontal };
                      tblk = CreateTextBlock("Property Name :");
                      tbx = CreateTextBox(((Column)selectedItem.Tag).propertyName, "propertyName");
                      sp.Children.Add(tblk);
                      sp.Children.Add(tbx);
                      editTreeNode.spContainer.Children.Add(sp);

                      editTreeNode.Show();
                }
                         
        }
    
               

          
     

        private TextBox CreateTextBox(string text, string tag)
        {
            TextBox tb = new TextBox() { Text = text };
            tb.Tag = tag;
            tb.Width = 100;
            tb.Height = 24;
            return tb;
        }

        private TextBlock CreateTextBlock(string text)
        {
            TextBlock tb = new TextBlock() { Text = text };
            tb.Height = 24;
            tb.Margin = new Thickness() { Top = 5, Left = 5 };
            return tb;
        }


    }
}