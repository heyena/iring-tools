using System.Windows.Controls;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Regions;

namespace org.iringtools.modules.projectapplicationregion
{
    public partial class ProjectApplicationView : UserControl,IProjectApplicationView
    {
        public ProjectApplicationView()
        {
            InitializeComponent();
        }

        public IPresentationModel Model { get; set; }
    }
}
