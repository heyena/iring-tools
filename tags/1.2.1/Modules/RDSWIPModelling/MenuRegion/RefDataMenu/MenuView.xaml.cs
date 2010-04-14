using System.Windows;
using System.Windows.Controls;
using PrismContrib.Base;
using Microsoft.Practices.Composite.Regions;
using org.iringtools.modelling.mainregion.refdatabrowser;

namespace org.iringtools.menu.views.menuregion
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
