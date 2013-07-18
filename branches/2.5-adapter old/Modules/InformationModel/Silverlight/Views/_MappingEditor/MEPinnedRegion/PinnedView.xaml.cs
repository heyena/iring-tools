using System.Windows.Controls;
using PrismContrib.Base;

namespace InformationModel.Views.MEPinnedRegion
{
  public partial class PinnedView : UserControl, IPinnedView
  {
    public PinnedView()
    {
      InitializeComponent();
    }

    public IPresentationModel Model { get; set; }

  }
}
