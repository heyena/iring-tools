using System.Windows.Controls;
using PrismContrib.Base;

namespace InformationModel.Views.MEDataDetailRegion
{
  public partial class MappingView : UserControl, IMappingView
  {
    public MappingView()
    {
      InitializeComponent();
    }

    public IPresentationModel Model { get; set; }

  }
}
