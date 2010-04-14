using OntologyService.Interface.PresentationModels;
using PrismContrib.Base;

namespace InformationModel.Views.MainRegion.Main
{
  public class MainPresenter : PresenterBase<IMainView>
  {
    public MainPresenter(IMainView view, IIMPresentationModel model)
      :base(view,model)
    {

    }

  }
}
