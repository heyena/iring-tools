using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Events;
using Library.Interface.Events;
using PrismContrib.Base;
using Modules.TemplateEditor;

namespace Modules.TemplateEditor.EditorRegion
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
