using System.Windows.Controls;
using System.Windows.Browser;
using System.Windows;

namespace RefDataEditor
{
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
            PageGrid.SizeChanged += new SizeChangedEventHandler(PageGrid_SizeChanged);
        }
        void PageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
          foreach (UserControl userControl in MainItem.Items)
          {
            userControl.Height = PageGrid.RowDefinitions[1].ActualHeight;
            //userControl.Width = PageGrid.ColumnDefinitions[1].ActualWidth;
          }

          foreach (UserControl userControl in OverlayItem.Items)
          {
            double d = 0;
              foreach (RowDefinition rowDef in PageGrid.RowDefinitions)
              {
                  d += rowDef.ActualHeight;
              }

              userControl.Height = d;
          }
        }
    }
}
