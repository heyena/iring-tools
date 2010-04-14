using System;
using System.Windows;
using System.Windows.Controls;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;

using Library.Interface.Events;

using ModuleLibrary.Events;

namespace Modelling.ClassDefinition.ClassDefinitionEditor
{
    public partial class ClassDefinitionEditorView : UserControl, IClassDefinitionEditorView
    {
        public ClassDefinitionEditorView(IEventAggregator aggregator)
        {
            InitializeComponent();            
        }
                
        public IPresentationModel Model { get; set; }
    }
}
