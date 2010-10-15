using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace iRTProxyConfig.DataAccess
{
    class ProxyRepository
    {
        public ProxyRepository()
        {
            //Get proxy host and port
            Refresh();
        }

        public void Refresh()
        {
            //Refresh proxy host and port
            _proxyHost = Convert.ToString(HttpWebRequest.DefaultWebProxy.GetProxy(new Uri("http://www.bechtel.com")).Host);
            _proxyPort = Convert.ToInt32(HttpWebRequest.DefaultWebProxy.GetProxy(new Uri("http://www.bechtel.com")).Port);
       }

        string _proxyHost;
        public string ProxyHost
        {
            get
            {
                return _proxyHost;
            }
            set
            {
                _proxyHost = value;
            }
        }

        int _proxyPort;
        public int ProxyPort
        {
            get
            {
                return _proxyPort;
            }
            set
            {
                _proxyPort = value;
            }
        }
    }
}
