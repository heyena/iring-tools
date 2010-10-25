using System.Windows.Controls;

using PrismContrib.Base;

using org.iringtools.library.presentation.events;
using Microsoft.Practices.Composite.Events;

using Microsoft.Practices.Composite.Regions;

namespace org.iringtools.modules.contextmenu.contextmenuregion
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
