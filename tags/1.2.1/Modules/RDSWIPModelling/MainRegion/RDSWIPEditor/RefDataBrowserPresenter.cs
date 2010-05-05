using OntologyService.Interface;
using PrismContrib.Base;
using SpinnerModule;
using System.Windows.Controls;
using OntologyService.Interface.PresentationModels;

namespace Modelling.MainRegion.RefDataBrowser
{
    /// <summary>
    /// Information Model View Presenter
    /// </summary>
    public class RefDataBrowserPresenter : PresenterBase<IRefDataEditorView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InformationModelPresenter"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="model">The model.</param>         
        public RefDataBrowserPresenter(RefDataBrowserView view, IIMPresentationModel model)
            : base(view, model)
        {}
    }
}
