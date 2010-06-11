
using Microsoft.Practices.Unity;
using org.iringtools.library.presentation.configuration;
using System;
using System.ServiceModel;
using System.Windows;
using PrismContrib.Errors;
using org.iringtools.modulelibrary.events;

namespace org.iringtools.modulelibrary.baseclass
{
    public class DALBase
    {
        /// <summary>
        /// Gets or sets the error.  Used by derived classes.
        /// </summary>
        /// <value>The error.</value>
        [Dependency]
        public IError Error { get; set; }
        
        private IServerConfiguration config;
        protected string ConfiguredURI { get; set; }
        protected BasicHttpBinding Binding { get; set; }
        protected EndpointAddress EndpointAddress { get; set; }

        protected string ConfiguredURIRefData { get; set; }
        protected BasicHttpBinding BindingRefData { get; set; }
        protected EndpointAddress EndpointAddressRefData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DALBase"/> class.
        /// <example>
        /// Derived class can use properties as follows:
        /// client = new wcf.AdapterProxyClient(Binding, EndpointAddress);
        /// </example>
        /// </summary>
        /// <param name="config">The config.</param>
        public DALBase(IServerConfiguration config)
        {
            this.config = config;

            // Configure and initialize WCF components so that they
            // can be used in derived classes
            ConfiguredURI = string.Format("{0}/{1}/{2}", 
                    config.WebServerURL,
                    config.WebSiteName,
                    config.AdapterProxy);

            ConfiguredURIRefData = string.Format("{0}/{1}/{2}",
                    config.WebServerURL,
                    config.WebSiteName,
                    config.GetParameter("ReferenceDataProxy"));
  
            // Don't build the path if the entire path already
            // is provided (stomp on above)
            if (config.WebServerURL.ToLower().Contains(".svc")) 
                ConfiguredURI = config.WebServerURL;

            #if SILVERLIGHT
                // Runtime
              string uriScheme = Application.Current.Host.Source.Scheme;
            #else           
                // Unit Test
                string uriScheme = "http";
            #endif

            bool usingTransportSecurity = uriScheme.Equals("https", StringComparison.InvariantCultureIgnoreCase);

            // Security
            BasicHttpSecurityMode securityMode;
            if (usingTransportSecurity)
                securityMode = BasicHttpSecurityMode.Transport;
            else
                securityMode = BasicHttpSecurityMode.None;

            // Binding
            Binding = new BasicHttpBinding(securityMode);
            Binding.MaxReceivedMessageSize = int.MaxValue;
            Binding.MaxBufferSize = int.MaxValue;
            TimeSpan timeout;
            TimeSpan.TryParse("00:10:00", out timeout);
            Binding.OpenTimeout = timeout;
            Binding.CloseTimeout = timeout;
            Binding.ReceiveTimeout = timeout;
            Binding.SendTimeout = timeout;

            // Endpoint Address
            Uri uri = new Uri(ConfiguredURI);
            EndpointAddress = new EndpointAddress(uri);

            uri = new Uri(ConfiguredURIRefData);
            EndpointAddressRefData = new EndpointAddress(uri);
        }

        /// <summary>
        /// Determines whether [is supported event] [the specified completed event arg].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="completedEventArg">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <returns>
        /// 	<c>true</c> if [is supported event] [the specified completed event arg]; otherwise, <c>false</c>.
        /// </returns>
        protected bool CheckClassTypeFor<T>(EventArgs completedEventArg)
        {
           return (completedEventArg is T);
        }

        /// <summary>
        /// Determines whether an exception is due to service unavailibility.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>
        /// 	<c>true</c> if exception is due to service unavailibility; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsServiceUnavailable(Exception ex)
        {
            if (
                ex.GetBaseException().Message.ToUpper().Contains("SECURITY ERROR") ||
                ex.GetBaseException() is System.Net.WebException
               )
            {
                return true;
            }
            else
                return false;
        }

    }
}
