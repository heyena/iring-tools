using System.Windows.Controls;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Regions;

namespace org.iringtools.modules.medatasourceregion
{
  public partial class DataSourceTreeView : UserControl, IDataSourceTreeView
  {
    public DataSourceTreeView(IRegionManager regionManager)
    {
      InitializeComponent();

    }

    public IPresentationModel Model { get; set; }

  }
}
