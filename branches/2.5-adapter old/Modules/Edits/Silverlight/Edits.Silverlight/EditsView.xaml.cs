using System.Windows.Controls;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Regions;

namespace org.iringtools.modules.edits.editsregion
{
    public partial class EditsView : UserControl, IEditsView
    {
        public EditsView(IRegionManager regionManager)
        {
            InitializeComponent();
        }
        public IPresentationModel Model { get; set; }
    }
}
