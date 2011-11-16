using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Events;
using org.iringtools.library.presentation.events;
using PrismContrib.Base;
using org.iringtools.modules.templateeditor;

namespace org.iringtools.modules.templateeditor.editorregion
{
    public partial class TemplateEditorView : UserControl, ITemplateEditorView
    {
        public TemplateEditorView()
        {
            InitializeComponent();
        }

        public IPresentationModel Model { get; set; }
    }
}
