using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PrismContrib.Base;
using Microsoft.Practices.Composite.Regions;

namespace org.iringtools.modules.memappingregion
{
  public partial class MappingView : UserControl, IMappingView
  {
    public MappingView()
    {
      InitializeComponent();
    }

    public IPresentationModel Model { get; set; }

  }
}
