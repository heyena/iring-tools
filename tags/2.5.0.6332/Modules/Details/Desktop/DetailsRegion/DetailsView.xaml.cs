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
using Microsoft.Practices.Composite.Regions;
using PrismContrib.Base;
namespace org.iringtools.modules.details.detailsregion
{
  /// <summary>
  /// Interaction logic for DetailsView.xaml
  /// </summary>
  public partial class DetailsView : UserControl, IDetailsView
  {
     public DetailsView(IRegionManager regionManager)
        {
            InitializeComponent();
        }

        public IPresentationModel Model { get; set; }
  }
}
