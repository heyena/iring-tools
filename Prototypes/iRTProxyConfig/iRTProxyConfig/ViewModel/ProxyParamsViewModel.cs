using System;
using System.Windows;
using System.Xml;
using iRTProxyConfig.DataAccess;
using iRTProxyConfig.Model;
using System.Windows.Input;
using System.Windows.Media;

namespace iRTProxyConfig.ViewModel
{
    class ProxyParamsViewModel : ViewModelBase
    {
        // Private variables
        ProxyRepository _proxyRepository;
        RelayCommand _updateCommand;
        RelayCommand _resetCommand;
        RelayCommand _getIRTFolderCommand;

        public ProxyRepository ProxyParams
        {
            get
            {
                return _proxyRepository;
            }
        }

        public ProxyParamsViewModel(ProxyRepository proxyRepository)
        {
            if (proxyRepository == null)
            {
                throw new ArgumentNullException("proxyRepository");
            }

            _proxyRepository = proxyRepository;
            _proxyParams = new ProxyParams();
            _proxyParams.AppName = "Proxy Configuration";
            Reset();
        }

        public void Reset()
        {
            _proxyParams.iRingToolsFolder = "";
            _proxyParams.Username = "";
            _proxyParams.Password = "";
            _proxyRepository.Refresh();
            _proxyParams.ProxyHost = _proxyRepository.ProxyHost;
            _proxyParams.ProxyPort = _proxyRepository.ProxyPort;
        }
        
        ProxyParams _proxyParams;
        public ProxyParams ProxyParameters
        {
            get
            {
                return _proxyParams;
            }
            set
            {
                _proxyParams = value;
            }
        }

        public string AppName
        {
            get
            {
                return _proxyParams.AppName;
            }
            set
            {
                _proxyParams.AppName = value;
            }
        }

        public string iRingToolsFolder
        {
              get
              {
                  return _proxyParams.iRingToolsFolder;
              }
              set
              {
                  _proxyParams.iRingToolsFolder = value;
                  if (String.IsNullOrEmpty(value))
                  {
                      throw new ApplicationException("Folder location is required.");
                  }
              }
        }
          
        public string Username
        {
            get
            {
                return _proxyParams.Username;
            }
            set
            {
                _proxyParams.Username = value;
                if (String.IsNullOrEmpty(value))
                {
                    throw new ApplicationException("Username is required.");
                }
            }
        }

        public string Password
        {
            get
            {
                return _proxyParams.Password;
            }
            set
            {
                _proxyParams.Password = value;
                if (String.IsNullOrEmpty(value))
                {
                    throw new ApplicationException("Password is required.");
                }
            }
        }

        public string ProxyHost
        {
            get
            {
                return _proxyParams.ProxyHost;
            }
            set
            {
                _proxyParams.ProxyHost = value;
                if (String.IsNullOrEmpty(value))
                {
                    throw new ApplicationException("Proxy Host is required.");
                }
            }
        }

        public int ProxyPort
        {
            get
            {
                return _proxyParams.ProxyPort;
            }
            set
            {
                _proxyParams.ProxyPort = value;
            }
        }

        private Brush _bgBrush;
        public Brush BackgroundBrush
        {
            get
            {
                return _bgBrush;
            }
            set
            {
                _bgBrush = value;
                OnPropertyChanged("BackgroundBrush");
            }
        }

        public ICommand UpdateCommand
        {
            get
            {
                if (_updateCommand == null)
                {
                    _updateCommand = new RelayCommand(param => this.UpdateCommandExecute(), param => this.UpdateCommandCanExecute);
                }
                return _updateCommand;
            }
        }

        void UpdateCommandExecute()
        {
            try
            {
                bool isValid = true;
                if (_proxyParams.iRingToolsFolder == "")
                {
                    isValid = false;
                    OnPropertyChanged("iRingToolsFolder");
                }
                if (_proxyParams.Username == "")
                {
                    isValid = false;
                    OnPropertyChanged("Username");
                }
                if (_proxyParams.Password == "")
                {
                    isValid = false;
                    OnPropertyChanged("Username");
                }
                if (_proxyParams.ProxyHost == "")
                {
                    isValid = false;
                    OnPropertyChanged("Username");
                }

                if (!isValid)
                {
                    MessageBox.Show("One or more parameters invalid.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(_proxyParams.iRingToolsFolder + "\\Utilities\\EncryptCredentials.exe");
                psi.Arguments = _proxyParams.Username + " " + _proxyParams.Password;
                psi.RedirectStandardOutput = true;
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                System.Diagnostics.Process listFiles;
                listFiles = System.Diagnostics.Process.Start(psi);
                System.IO.StreamReader myOutput = listFiles.StandardOutput;
                listFiles.WaitForExit(1000);
                if (listFiles.HasExited)
                {
                    string output = myOutput.ReadToEnd();
                    // modify file
                    XmlDocument myXmlDocument = new XmlDocument();
                    myXmlDocument.Load(_proxyParams.iRingToolsFolder + "\\Services\\Web.config");
                    XmlNode nodeAppSettings = null;
                    XmlNode nodeModify = null;
                    nodeAppSettings = myXmlDocument.DocumentElement.SelectSingleNode("appSettings");
                    nodeModify = nodeAppSettings.SelectSingleNode("add[@key='ProxyCredentialToken']");
                    nodeModify.Attributes.GetNamedItem("value").Value = output;
                    nodeModify = nodeAppSettings.SelectSingleNode("add[@key='ProxyHost']");
                    nodeModify.Attributes.GetNamedItem("value").Value = _proxyParams.ProxyHost;
                    nodeModify = nodeAppSettings.SelectSingleNode("add[@key='ProxyPort']");
                    nodeModify.Attributes.GetNamedItem("value").Value = _proxyParams.ProxyPort.ToString();

                    myXmlDocument.Save(_proxyParams.iRingToolsFolder + "\\Services\\Web.config");
                    MessageBox.Show("Update complete.", "Update Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error occurred while getting updating proxy configuration. " + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            // sample code
            //bool isupdated = true;

            //if (_proxyParams.iRingToolsFolder!="")
            //{
            //    isupdated = false;
            //}
            //if (isupdated)
            //{
            //    BackgroundBrush = new SolidColorBrush(Colors.Green);
            //}
            //else
            //{
            //    BackgroundBrush = new SolidColorBrush(Colors.White);
            //}
        }

        bool UpdateCommandCanExecute
        {
            get
            {
                //if (_proxyParams.Username!="")
                //{
                //    return false;
                //}
                return true;
            }
        }

        public ICommand ResetCommand
        {
            get
            {
                if (_resetCommand == null)
                {
                    _resetCommand = new RelayCommand(param => this.ResetCommandExecute(), param => this.ResetCanExecute);
                }
                return _resetCommand;
            }
        }

        void ResetCommandExecute()
        {
            Reset();
            OnPropertyChanged("iRingToolsFolder");
            OnPropertyChanged("Username");
            OnPropertyChanged("Password");
            OnPropertyChanged("ProxyHost");
            OnPropertyChanged("ProxyPort");
        }

        bool ResetCanExecute
        {
            get
            {
                return true;
            }
        }

        public ICommand GetIRTFolder
        {
            get
            {
                if (_getIRTFolderCommand == null)
                {
                    _getIRTFolderCommand = new RelayCommand(param => this.GetIRTFolderExecute(), param => this.GetIRTFolderCanExecute);
                }
                return _getIRTFolderCommand;
            }
        }

        void GetIRTFolderExecute()
        {
            try
            {
                // Configure open file dialog box
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.FileName = "index.html"; // Default file name
                dlg.DefaultExt = ".html"; // Default file extension
                dlg.Filter = "HTML documents (.html)|*.html"; // Filter files by extension
                dlg.Filter = "File in iRINGTools folder|index.html"; // Filter files by extension
                dlg.Title = "Select the file index.html in the iRINGTools folder";
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    // Get folder path
                    _proxyParams.iRingToolsFolder = System.IO.Path.GetDirectoryName(dlg.FileName);
                    OnPropertyChanged("iRingToolsFolder"); 
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error occurred while getting folder. " + exc.Message,"Error",MessageBoxButton.OK,MessageBoxImage.Exclamation);
            }
        }

        bool GetIRTFolderCanExecute
        {
            get
            {
                return true;
            }
        }

        protected override void OnDispose()
        {
            //todo: clear something?
        }
    }
}
