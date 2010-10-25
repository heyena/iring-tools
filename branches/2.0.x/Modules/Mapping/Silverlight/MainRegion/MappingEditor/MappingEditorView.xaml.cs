using System.Windows.Controls;

using PrismContrib.Base;

namespace org.iringtools.modules.mainregion
{
  public partial class MappingEditorView : UserControl, IMappingEditorView
  {

    public MappingEditorView()
    {
      InitializeComponent();

      LayoutRoot.LayoutUpdated += new System.EventHandler(LayoutRoot_LayoutUpdated);

      LayoutRoot.SizeChanged += new System.Windows.SizeChangedEventHandler(LayoutRoot_SizeChanged);
      ProjectApplication.Loaded += new System.Windows.RoutedEventHandler(ProjectApplication_SizeChanged);
      DataSource.Loaded += new System.Windows.RoutedEventHandler(DataSource_SizeChanged);
      Search.Loaded += new System.Windows.RoutedEventHandler(Search_SizeChanged);
      Details.Loaded += new System.Windows.RoutedEventHandler(Details_SizeChanged);
      Mapping.Loaded += new System.Windows.RoutedEventHandler(Mapping_SizeChanged);
    }

    void ProjectApplication_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ProjectApplication.Items.Count > 0)
        {
            UserControl userControl = (UserControl)ProjectApplication.Items[0];
            userControl.Height = LayoutRoot.RowDefinitions[0].ActualHeight;
        }
    }

    void LayoutRoot_LayoutUpdated(object sender, System.EventArgs e)
    {
      LayoutRoot_SizeChanged(sender, new System.Windows.RoutedEventArgs());
    }

    #region Resize Event Handlers
    void LayoutRoot_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      DataSource_SizeChanged(sender, e);
      Search_SizeChanged(sender, e);
      ProjectApplication_SizeChanged(sender, e);
      Details_SizeChanged(sender, e);
      Mapping_SizeChanged(sender, e);
    }

    void DataSource_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      if (DataSource.Items.Count > 0)
      {
        UserControl userControl = (UserControl)DataSource.Items[0];
        userControl.Height = LayoutRoot.RowDefinitions[1].ActualHeight;
      }
    }

    void Search_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      if (Search.Items.Count > 0)
      {
        UserControl userControl = (UserControl)Search.Items[0];
        userControl.Height = LayoutRoot.RowDefinitions[1].ActualHeight;
      }
    }

    void Details_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      if (Details.Items.Count > 0)
      {
        UserControl userControl = (UserControl)Details.Items[0];
        userControl.Height = LayoutRoot.RowDefinitions[2].ActualHeight;
      }
    }

    void Mapping_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      if (Mapping.Items.Count > 0)
      {
        UserControl userControl = (UserControl)Mapping.Items[0];
        userControl.Height = LayoutRoot.RowDefinitions[2].ActualHeight;
      }
    }
    #endregion

    public IPresentationModel Model { get; set; }

  }
}
