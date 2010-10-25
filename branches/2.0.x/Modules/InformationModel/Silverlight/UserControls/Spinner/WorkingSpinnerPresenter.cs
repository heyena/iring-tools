using Microsoft.Practices.Unity;
using OntologyService.Interface.PresentationModels;
using PrismContrib.Base;

namespace InformationModel.UserControls
{
  /// <summary>
  /// TODO: Move code out of code-behind into presenter
  /// </summary>
    public class WorkingSpinnerPresenter : PresenterBase<IWorkingSpinner>
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="WorkingSpinnerPresenter"/> class.
      /// </summary>
      /// <param name="view">The view.</param>
      /// <param name="model">The model.</param>
        public WorkingSpinnerPresenter(IWorkingSpinner view, IIMPresentationModel model)
            : base(view,model)
        {
            
        }
    }
}
