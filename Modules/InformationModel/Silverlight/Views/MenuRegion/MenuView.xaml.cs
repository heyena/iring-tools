using System.Windows.Controls;
using InformationModel.Views.MainRegion.Main;
using PrismContrib.Base;

namespace InformationModel.Views.MenuRegion
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
