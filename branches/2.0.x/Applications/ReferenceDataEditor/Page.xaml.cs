using System.Windows.Controls;
using System.Windows.Browser;
using System.Windows;

namespace ReferenceDataEditor
{
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
            LayoutRoot.SizeChanged += new SizeChangedEventHandler(PageGrid_SizeChanged);
        }

        void PageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
          Control c = null;

          c = (Control)HeaderItem.Items[0];
          c.Height = LayoutRoot.RowDefinitions[0].ActualHeight;

          c = (Control)MainItem.Items[0];
          c.Height = LayoutRoot.RowDefinitions[1].ActualHeight;

          c = (Control)StatusItem.Items[0];
          c.Height = LayoutRoot.RowDefinitions[2].ActualHeight;     
        }
    }
}
