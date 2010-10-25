using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PrismContrib.Base;

namespace org.iringtools.modules.mainregion
{
  /// <summary>
  /// Interaction logic for MappingEditorView.xaml
  /// </summary>
  public partial class MappingEditorView : UserControl, IMappingEditorView
  {

    public MappingEditorView()
    {
      InitializeComponent();

      LayoutRoot.LayoutUpdated += new System.EventHandler(LayoutRoot_LayoutUpdated);

      LayoutRoot.SizeChanged += new System.Windows.SizeChangedEventHandler(LayoutRoot_SizeChanged);

      DataSource.Loaded += new System.Windows.RoutedEventHandler(DataSource_SizeChanged);
      Search.Loaded += new System.Windows.RoutedEventHandler(Search_SizeChanged);
      //Navigation.Loaded += new System.Windows.RoutedEventHandler(Navigation_SizeChanged);
      Details.Loaded += new System.Windows.RoutedEventHandler(Details_SizeChanged);
      Mapping.Loaded += new System.Windows.RoutedEventHandler(Mapping_SizeChanged);
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
      //Navigation_SizeChanged(sender, e);
      Details_SizeChanged(sender, e);
      Mapping_SizeChanged(sender, e);
    }

    void DataSource_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      if (DataSource.Items.Count > 0)
      {
        UserControl userControl = (UserControl)DataSource.Items[0];
        //double rowSpanHeight = LayoutRoot.RowDefinitions[0].ActualHeight + LayoutRoot.RowDefinitions[1].ActualHeight;
        userControl.Height = LayoutRoot.RowDefinitions[0].ActualHeight; //rowSpanHeight;
        //userControl.Width = LayoutRoot.ColumnDefinitions[0].ActualWidth;
      }
    }

    void Search_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      if (Search.Items.Count > 0)
      {
        UserControl userControl = (UserControl)Search.Items[0];
        userControl.Height = LayoutRoot.RowDefinitions[0].ActualHeight;
        //userControl.Width = LayoutRoot.ColumnDefinitions[1].ActualWidth;
      }
    }

    //void Navigation_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    //{
    //  if (Navigation.Items.Count > 0)
    //  {
    //    UserControl userControl = (UserControl)Navigation.Items[0];
    //    userControl.Height = LayoutRoot.RowDefinitions[1].ActualHeight;
    //    //userControl.Width = LayoutRoot.ColumnDefinitions[0].ActualWidth;
    //  }
    //}

    void Details_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      if (Details.Items.Count > 0)
      {
        UserControl userControl = (UserControl)Details.Items[0];
        userControl.Height = LayoutRoot.RowDefinitions[1].ActualHeight;
        //userControl.Width = LayoutRoot.ColumnDefinitions[0].ActualWidth;
      }
    }

    void Mapping_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      if (Mapping.Items.Count > 0)
      {
        UserControl userControl = (UserControl)Mapping.Items[0];
        userControl.Height = LayoutRoot.RowDefinitions[1].ActualHeight;
        //userControl.Width = LayoutRoot.ColumnDefinitions[1].ActualWidth;
      }
    }
    #endregion

    public IPresentationModel Model { get; set; }

  }
}
