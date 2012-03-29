using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UtilitySample
{
    class Program
    {
        static void Main(string[] args)
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            ServiceSettings settings = new ServiceSettings();
            settings.AppendSettings(appSettings);

            WebProxyCredentials webProxyCredentials = settings.GetWebProxyCredentials();

            WebHttpClient client = new WebHttpClient(settings["BaseUri"],
                webProxyCredentials.userName,
                webProxyCredentials.password,
                webProxyCredentials.domain,
                webProxyCredentials.proxyHost,
                webProxyCredentials.proxyPort);

            client.AccessToken = settings["AccessToken"];

            Stream stream = client.GetStream(settings["RelativeUri"]);

            Utility.WriteStream(stream, @"..\..\Test.pdf");
        }
    }
}
