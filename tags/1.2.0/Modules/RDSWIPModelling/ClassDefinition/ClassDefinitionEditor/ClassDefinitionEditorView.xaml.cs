using System;
using System.Windows;
using System.Windows.Controls;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;

using org.iringtools.library.presentation.events;

using org.iringtools.modulelibrary.events;

namespace org.iringtools.modelling.classdefinition.classdefinitioneditor
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
