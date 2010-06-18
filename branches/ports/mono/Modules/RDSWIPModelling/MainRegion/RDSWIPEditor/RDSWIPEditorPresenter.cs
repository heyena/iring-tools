using OntologyService.Interface;
using PrismContrib.Base;
using SpinnerModule;
using System.Windows.Controls;
using OntologyService.Interface.PresentationModels;

namespace RDSWIPModelling.MainRegion.RDSWIPEditor
{
    /// <summary>
    /// Information Model View Presenter
    /// </summary>
    public class RDSWIPEditorPresenter : PresenterBase<IRDSWIPEditorView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InformationModelPresenter"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="model">The model.</param>         
        public RDSWIPEditorPresenter(RDSWIPEditorView view, IIMPresentationModel model)
            : base(view, model)
        {}
    }
}
