using System.Windows;
using System.Windows.Controls;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Regions;

using Modelling.MainRegion.RefDataBrowser;

namespace Menu.Views.MenuRegion
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
