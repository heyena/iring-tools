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
using PrismContrib.Base;

namespace InformationModel.Views.MainRegion.RefDataBrowser
{
  public partial class RefDataBrowserView : UserControl, IRefDataBrowserView 
  {
    public RefDataBrowserView()
    {
      InitializeComponent();
    }

    public IPresentationModel Model { get; set; }

  }
}
