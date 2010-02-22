using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ServiceModel;
using ControlPanel.DemoService;

namespace ControlPanel
{
  public class ServiceUtility
  {
    public static DemoServiceClient GetDemoServiceClient() 
    {
      DemoServiceClient client = null;

      string uriScheme = Application.Current.Host.Source.Scheme;

      bool usingTransportSecurity = uriScheme.Equals("https", StringComparison.InvariantCultureIgnoreCase);

      BasicHttpSecurityMode securityMode;
      if (usingTransportSecurity)
        securityMode = BasicHttpSecurityMode.Transport;
      else
        securityMode = BasicHttpSecurityMode.None;

      BasicHttpBinding binding = new BasicHttpBinding(securityMode);
      binding.MaxReceivedMessageSize = int.MaxValue;
      binding.MaxBufferSize = int.MaxValue;
      TimeSpan timeout;
      TimeSpan.TryParse("00:10:00", out timeout);
      binding.OpenTimeout = timeout;
      binding.CloseTimeout = timeout;
      binding.ReceiveTimeout = timeout;
      binding.SendTimeout = timeout;

      Uri uri = new Uri(Application.Current.Host.Source, "../Service.svc");

      client = new DemoServiceClient(binding, new EndpointAddress(uri));
    
      return client;
    }
  }
}
