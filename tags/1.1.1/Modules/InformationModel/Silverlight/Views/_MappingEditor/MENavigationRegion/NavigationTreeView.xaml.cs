using System.Windows.Controls;
using PrismContrib.Base;

namespace InformationModel.Views.MENavigationRegion
{
  public partial class NavigationTreeView : UserControl, INavigationTreeView
  {
    public NavigationTreeView()
    {
      InitializeComponent();
    }

    public IPresentationModel Model { get; set; }

  }
}
