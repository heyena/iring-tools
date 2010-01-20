using System.Windows.Controls;

using PrismContrib.Base;

namespace org.iringtools.modules.menu.menuregion
{
  public partial class MenuView : UserControl, IMenuView
  {
    public MenuView()
    {
      InitializeComponent();
    }

    public IPresentationModel Model { get; set; }

  }    
}
