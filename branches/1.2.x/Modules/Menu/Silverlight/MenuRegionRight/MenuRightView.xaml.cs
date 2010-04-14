using System.Windows.Controls;
using PrismContrib.Base;

namespace org.iringtools.modules.menu.menuregionright
{
  public partial class MenuRightView : UserControl, IMenuRightView
  {
    public MenuRightView()
    {
      InitializeComponent();
   
    }

    public IPresentationModel Model { get; set; }

  }
}
