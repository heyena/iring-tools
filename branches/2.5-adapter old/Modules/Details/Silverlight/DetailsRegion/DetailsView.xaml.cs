using System.Windows.Controls;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Regions;

namespace org.iringtools.modules.details.detailsregion
{
    public partial class DetailsView : UserControl, IDetailsView
    {

        public DetailsView(IRegionManager regionManager)
        {
            InitializeComponent();
        }

        public IPresentationModel Model { get; set; }

        //private void btnClipBoard_Click(object sender, System.Windows.RoutedEventArgs e)
        //{

        //}

    }
}
