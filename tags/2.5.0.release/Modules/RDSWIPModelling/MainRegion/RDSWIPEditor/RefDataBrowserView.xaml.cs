using System.Windows.Controls;
using PrismContrib.Base;

namespace Modelling.MainRegion.RefDataBrowser
{
    public partial class RefDataBrowserView : UserControl, IRefDataEditorView
    {
        public RefDataBrowserView()
        {
            InitializeComponent();
        }

        public IPresentationModel Model { get; set; }

    }
}
