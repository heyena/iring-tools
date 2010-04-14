using System.Windows.Controls;

using PrismContrib.Base;

namespace Modules.Menu.MenuRegion
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
