using System.Windows.Controls;
using PrismContrib.Base;

namespace org.iringtools.modelling.mainregion.refdatabrowser
{
  public partial class RefDataBrowserView : UserControl, IRefDataEditorView
  {
    public RefDataBrowserView()
    {
      InitializeComponent();

      LayoutRoot.LayoutUpdated += new System.EventHandler(LayoutRoot_Updated);
      LayoutRoot.SizeChanged += new System.Windows.SizeChangedEventHandler(LayoutRoot_SizeChanged);
      Search.Loaded += new System.Windows.RoutedEventHandler(Search_SizeChanged);
      Details.Loaded += new System.Windows.RoutedEventHandler(Details_SizeChanged);
      Edits.Loaded += new System.Windows.RoutedEventHandler(Edits_SizeChanged);
      ClassEditor.Loaded += new System.Windows.RoutedEventHandler(ClassEditor_SizeChanged);
      TemplateEditor.Loaded += new System.Windows.RoutedEventHandler(TemplateEditor_SizeChanged);
    }

    #region Resize Event Handlers
    void LayoutRoot_Updated(object sender, System.EventArgs e)
    {
      LayoutRoot_SizeChanged(sender, new System.Windows.RoutedEventArgs());
    }

    void LayoutRoot_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      Search_SizeChanged(sender, e);
      //Navigation_SizeChanged(sender, e);
      Details_SizeChanged(sender, e);
      Edits_SizeChanged(sender, e);
      ClassEditor_SizeChanged(sender, e);
      TemplateEditor_SizeChanged(sender, e);
    }

    void Edits_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      if (Edits.Items.Count > 0)
      {
        UserControl userControl = (UserControl)Edits.Items[0];
        userControl.Height = LayoutRoot.RowDefinitions[1].ActualHeight;
      }
    }

    void Search_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      if (Search.Items.Count > 0)
      {
        UserControl userControl = (UserControl)Search.Items[0];
        userControl.Height = LayoutRoot.RowDefinitions[0].ActualHeight +
            LayoutRoot.RowDefinitions[1].ActualHeight +
            LayoutRoot.RowDefinitions[2].ActualHeight; //rowSpanHeight;
        //userControl.Width = LayoutRoot.ColumnDefinitions[0].ActualWidth;
      }
    }

    void Details_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      if (Details.Items.Count > 0)
      {
        UserControl userControl = (UserControl)Details.Items[0];
        //double rowSpanHeight = LayoutRoot.RowDefinitions[0].ActualHeight + LayoutRoot.RowDefinitions[1].ActualHeight;
        userControl.Height = LayoutRoot.RowDefinitions[2].ActualHeight; //rowSpanHeight;
        //userControl.Width = LayoutRoot.ColumnDefinitions[1].ActualWidth;
      }
    }

    void ClassEditor_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      if (ClassEditor.Items.Count > 0)
      {
        UserControl userControl = (UserControl)ClassEditor.Items[0];
        //double rowSpanHeight = LayoutRoot.RowDefinitions[0].ActualHeight + LayoutRoot.RowDefinitions[1].ActualHeight;
        userControl.Height = LayoutRoot.RowDefinitions[1].ActualHeight; //rowSpanHeight;
        //userControl.Width = LayoutRoot.ColumnDefinitions[1].ActualWidth;
      }
    }

    void TemplateEditor_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      if (TemplateEditor.Items.Count > 0)
      {
        UserControl userControl = (UserControl)TemplateEditor.Items[0];
        //double rowSpanHeight = LayoutRoot.RowDefinitions[0].ActualHeight + LayoutRoot.RowDefinitions[1].ActualHeight;
        userControl.Height = LayoutRoot.RowDefinitions[1].ActualHeight; //rowSpanHeight;
        //userControl.Width = LayoutRoot.ColumnDefinitions[1].ActualWidth;
      }
    }
    #endregion

    public IPresentationModel Model { get; set; }
  }
}
