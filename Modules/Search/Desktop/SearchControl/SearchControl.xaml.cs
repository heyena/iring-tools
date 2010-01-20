using System.Windows.Controls;
using PrismContrib.Base;
using Microsoft.Practices.Composite.Regions;

namespace org.iringtools.modules.search.searchregion
{
    public partial class SearchControl : UserControl, ISearchControl
    {
        public SearchControl()
        {
          InitializeComponent();

          itcModelBrowser.SizeChanged += new System.Windows.SizeChangedEventHandler(itcModelBrowser_SizeChanged);
          itcModelBrowser.Loaded += new System.Windows.RoutedEventHandler(itcModelBrowser_SizeChanged);
        }

        void itcModelBrowser_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
        {
          if (itcModelBrowser.Items.Count > 0)
          {
            System.Windows.Controls.Control userControl = (System.Windows.Controls.Control)itcModelBrowser.Items[0];
            userControl.Height = itcModelBrowser.ActualHeight;
            //userControl.Width = itcModelBrowser.ActualWidth;
          }
        }

        public IPresentationModel Model { get; set; }

    }
}
