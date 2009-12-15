using Microsoft.Practices.Composite.Events;
using OntologyService.Interface.PresentationModels;
using PrismContrib.Base;

namespace InformationModel.Views.MEPinnedRegion
{

    /// <summary>
    /// DEPRECATED....
    /// </summary>
    public class PinnedPresenter : PresenterBase<IPinnedView>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PinnedPresenter"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="model">The model.</param>
        /// <param name="aggregator">The aggregator.</param>
        public PinnedPresenter(IPinnedView view, IIMPresentationModel model,
          IEventAggregator aggregator)
            : base(view, model)
        {
        }
    }
}
