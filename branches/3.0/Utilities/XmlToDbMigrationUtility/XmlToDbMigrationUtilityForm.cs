namespace XmlToDbMigrationUtility
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.IO;
    using log4net;
    using org.iringtools.utility;
    using org.iringtools.library;
    using org.iringtools.adapter;
    using StaticDust.Configuration;

    /// <summary>
    /// 
    /// </summary>
    public partial class XmlToDbMigrationUtilityForm : Form
    {
        enum LogType
        {
            Error,
            Info,
            Warn
        }

        ILog _logger = LogManager.GetLogger(typeof(XmlToDbMigrationUtilityForm));


        List<DatabaseDictionary> allDatabaseDictionaries = new List<DatabaseDictionary>();
        List<DataDictionary> allDataDictionaries = new List<DataDictionary>();
        List<AdapterSettings> allAdapterSettings = new List<AdapterSettings>();
        ScopeProjects allScopeProjects = new ScopeProjects();

        public XmlToDbMigrationUtilityForm()
        {
            InitializeComponent();


        }

        private void InputFolderButton_Click(object sender, EventArgs e)
        {
            DirectoryInfo currentDirectoryInfo = new DirectoryInfo("..\\..\\..\\..\\iRINGTools.Services\\App_Data");
            InputFolderBrowserDialog.SelectedPath = currentDirectoryInfo.FullName;
            InputFolderBrowserDialog.ShowDialog();

            InputFolderTextBox.Text = InputFolderBrowserDialog.SelectedPath;
        }

        private void StartMigrationButton_Click(object sender, EventArgs e)
        {
            ReadInputXMLs();
            GetPermissionInfoForTheUser();
            CreateObjectsForDatabase();
        }

        private void ReadInputXMLs()
        {
            MigrationProgressBar.Value = 0;

            if (Directory.Exists(InputFolderTextBox.Text))
            {
                CreateFolder("Processed");
                CreateFolder("Un-Processed");

                DirectoryInfo inputDirectory = new DirectoryInfo(InputFolderTextBox.Text);

                PopulateObjects<DataDictionary>(inputDirectory, ref allDataDictionaries);
                PopulateObjects<DatabaseDictionary>(inputDirectory, ref allDatabaseDictionaries);
                PopulateScopeProjects(inputDirectory, ref allScopeProjects);
                PopulateConfigObjects(inputDirectory, ref allAdapterSettings);
            }
            else
            {
                MigrationProgressBar.Value = 100;
                LogEntry("Input folder does not exist", LogType.Error);
            }
        }

        private void GetPermissionInfoForTheUser()
        {
        }

        private void CreateObjectsForDatabase()
        {
            
        }

        private void PopulateScopeProjects(DirectoryInfo inputDirectory, ref ScopeProjects allScopeProjects)
        {
            LogEntry("Reading Scopes.xml", LogType.Info);

            FileInfo xmlFile = inputDirectory.GetFiles("Scopes.xml")[0];
            try
            {
                allScopeProjects = Utility.Read<ScopeProjects>(xmlFile.FullName);
            }
            catch
            {
            }
        }

        private void PopulateObjects<T>(DirectoryInfo inputDirectory, ref List<T> objectsList)
        {
            LogEntry("Reading all " + typeof(T).Name, LogType.Info);

            foreach (FileInfo xmlFile in inputDirectory.EnumerateFiles(typeof(T).Name + ".*"))
            {
                try
                {
                    T tempObject = Utility.Read<T>(xmlFile.FullName);
                    objectsList.Add(tempObject);
                }
                catch
                {
                }
            }
        }

        private void PopulateConfigObjects(DirectoryInfo inputDirectory, ref List<AdapterSettings> objectsList)
        {
            LogEntry("Reading all datalayer config files", LogType.Info);

            foreach (FileInfo xmlFile in inputDirectory.EnumerateFiles("*.config"))
            {
                try
                {
                    AdapterSettings tempAdapterSettings = new AdapterSettings();
                    tempAdapterSettings.AppendSettings(new AppSettingsReader(xmlFile.FullName));
                    objectsList.Add(tempAdapterSettings);
                }
                catch
                {
                }
            }
        }

        private void CreateFolder(string folderName)
        {
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
                LogEntry("'" + folderName + "' folder created", LogType.Info);
            }
            else
            {
                LogEntry("'" + folderName + "' folder is present", LogType.Info);
            }
        }

        private void LogEntry(string message, LogType logType)
        {
            switch(logType)
            {
                case LogType.Error: _logger.Error(message);
                    break;
                case LogType.Info: _logger.Info(message);
                    break;
                case LogType.Warn: _logger.Warn(message);
                    break;
            }

            LogRichTextBox.Text += logType.ToString() + ":\t" + DateTime.Now.ToString() + "\t" + message + "\n";
        }
    }
}
