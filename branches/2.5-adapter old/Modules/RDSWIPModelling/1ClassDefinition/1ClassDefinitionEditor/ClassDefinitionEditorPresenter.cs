using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using ModuleLibrary.Events;
using ModuleLibrary.Types;
using OntologyService.Interface.PresentationModels;
using PrismContrib.Base;
using OntologyService.Interface;

namespace Modelling.ClassDefinition.ClassDefinitionEditor
{
    public class ClassDefinitionEditorPresenter : PresenterBase<IClassDefinitionEditorView>
    {
        private IEventAggregator aggregator;

        private IMPresentationModel model;

        public ClassDefinitionEditorPresenter(IClassDefinitionEditorView view, IIMPresentationModel model,
            IEventAggregator aggregator)
            : base(view, model)
        {
            try
            {
                this.model = (IMPresentationModel)model;
                this.aggregator = aggregator;
                
            }
            catch (Exception ex)
            {
                Error.SetError(ex);
            }
        }
    }
}
