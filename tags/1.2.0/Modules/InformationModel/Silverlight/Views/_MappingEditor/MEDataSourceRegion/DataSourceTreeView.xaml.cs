using System.Windows.Controls;
using PrismContrib.Base;

namespace InformationModel.Views.MEDataSourceRegion
{
  public partial class DataSourceTreeView : UserControl, IDataSourceTreeView
  {
    public DataSourceTreeView()
    {
      InitializeComponent();
    }


    public IPresentationModel Model { get; set; }

  }
}
