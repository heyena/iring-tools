using System.Windows.Controls;
using PrismContrib.Base;

namespace InformationModel.Views.MainRegion.Main
{
  public partial class MainView : UserControl, IMainView

  {
    public MainView()
    {
      InitializeComponent();
    }

    public IPresentationModel Model { get; set; }
  }
}
