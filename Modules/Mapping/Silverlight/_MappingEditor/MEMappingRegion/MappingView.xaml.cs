using System.Windows.Controls;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Regions;

namespace Modules.MappingEditor.MEMappingRegion
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
