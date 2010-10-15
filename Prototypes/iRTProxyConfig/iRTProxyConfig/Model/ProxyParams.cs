using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRTProxyConfig.Model
{
    class ProxyParams
    {
        #region Properties

        public string AppName { get; set; }
        public string iRingToolsFolder { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public string ProxyCredentialToken { get; set; }
        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        
        #endregion //Properties
    }
}
