using System.Windows.Controls;

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

    public IPresentationModel Model
    {
      get;
      set;
    }
  }
}
