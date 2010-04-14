using System.Windows.Controls;
using PrismContrib.Base;

namespace Modules.Menu.MenuRegionRight
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
