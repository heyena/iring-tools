using System.Windows.Controls;
using PrismContrib.Base;

namespace InformationModel.Views.MESearchRegion
{
  public partial class MESearchView : UserControl, IMESearchView
  {
    public MESearchView()
    {
      InitializeComponent();
    }

    public IPresentationModel Model { get; set; }
  }
}
