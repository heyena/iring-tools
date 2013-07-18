using System.Windows.Controls;
using PrismContrib.Base;

namespace MappingEditor.Views.Main
{
    public partial class MappingEditorView : UserControl, IMappingEditorView
    {
        public MappingEditorView()
        {
            InitializeComponent();
        }

        public IPresentationModel Model { get; set; }

    }
}
