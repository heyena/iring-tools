using System;
using System.Collections.Generic;
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
using TestService;
using org.iringtools.utility;
using SilverlightFaultsTest.ServiceReference;

namespace SilverlightFaultsTest
{
  public partial class MainPage : UserControl
  {
    public MainPage()
    {
      InitializeComponent();

      //WebClient webClient = new WebClient();
      //webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnDownloadStringCompleted);
      //webClient.DownloadStringAsync(new Uri("http://localhost:56758/Service.svc/fault"));
      ServiceClient serviceClient = new ServiceClient();
      serviceClient.GetFaultCompleted += new EventHandler<GetFaultCompletedEventArgs>(serviceClient_FaultCompleted);
      serviceClient.GetFaultAsync(new SilverlightFaultsTest.ServiceReference.Operation());
    }

    void serviceClient_FaultCompleted(object sender, GetFaultCompletedEventArgs e)
    {
      if (e.Error == null)
      {
        TextBox1.Text = Utility.Deserialize<string>((string)e.Result, true);
      }
      else if (e.Error is FaultException<SilverlightFaultsTest.ServiceReference.ArithmeticFault>)
      {
        FaultException<SilverlightFaultsTest.ServiceReference.ArithmeticFault> fault = e.Error as FaultException<SilverlightFaultsTest.ServiceReference.ArithmeticFault>;
        TextBox1.Text = "Error: " + fault.Detail.Operation + " - " + fault.Detail.Description;
      }
    }

    //private void OnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
    //{
    //  if (e.Error == null)
    //  {
    //    TextBox1.Text = Utility.Deserialize<string>((string)e.Result, true);
    //  }
    //  else if (e.Error is FaultException<SilverlightFaultsTest.ServiceReference.ArithmeticFault>)
    //  {
    //    FaultException<SilverlightFaultsTest.ServiceReference.ArithmeticFault> fault = e.Error as FaultException<SilverlightFaultsTest.ServiceReference.ArithmeticFault>;
    //    TextBox1.Text = "Error: " + fault.Detail.Operation + " - " + fault.Detail.Description;
    //  }
    //}
  }
}
