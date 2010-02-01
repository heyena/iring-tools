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
            LayoutRoot.SizeChanged += new SizeChangedEventHandler(PageGrid_SizeChanged);
        }

        void PageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
          Control c = (Control)MainItem.Items[0];
          c.Height = LayoutRoot.RowDefinitions[1].ActualHeight;     
        }
    }
}
