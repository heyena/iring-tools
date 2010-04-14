using System.Windows.Controls;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Regions;

namespace Modules.Details.DetailsRegion
{
    public partial class DetailsView : UserControl, IDetailsView
    {

        public DetailsView(IRegionManager regionManager)
        {
            InitializeComponent();
        }

        public IPresentationModel Model { get; set; }

    }
}
