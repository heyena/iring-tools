using System.Windows.Controls;
using PrismContrib.Base;

namespace RDSWIPModelling.MainRegion.RDSWIPEditor
{
    public partial class RDSWIPEditorView : UserControl, IRDSWIPEditorView
    {
        public RDSWIPEditorView()
        {
            InitializeComponent();
        }

        public IPresentationModel Model { get; set; }

    }
}
