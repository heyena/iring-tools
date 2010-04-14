using System.Windows.Controls;

using PrismContrib.Base;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

namespace org.iringtools.modelling.mainregion.refdatabrowser
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
