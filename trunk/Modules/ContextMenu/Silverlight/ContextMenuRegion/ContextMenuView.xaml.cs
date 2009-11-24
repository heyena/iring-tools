using System.Windows.Controls;

using PrismContrib.Base;

using Library.Interface.Events;
using Microsoft.Practices.Composite.Events;

using Microsoft.Practices.Composite.Regions;

namespace Modules.ContextMenu.ContextMenuRegion
{
    public partial class ContextMenuView : UserControl, IContextMenuView
    {
      public ContextMenuView(IEventAggregator aggregator)
        {
            InitializeComponent();
        }

        public IPresentationModel Model { get; set; }
    }
}
