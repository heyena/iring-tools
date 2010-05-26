using System.Windows.Controls;
using PrismContrib.Base;

namespace Modelling.ClassDefinition.ClassDefinitionEditor
{
    public partial class ClassDefinitionEditorView : UserControl, IClassDefinitionEditorView
    {
        public ClassDefinitionEditorView()
        {
            InitializeComponent();            
        }

        public IPresentationModel Model { get; set; }
    }
}
