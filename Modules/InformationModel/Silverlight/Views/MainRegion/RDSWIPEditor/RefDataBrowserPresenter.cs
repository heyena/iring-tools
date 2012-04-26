using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using PrismContrib.Base;
using OntologyService.Interface.PresentationModels;

namespace InformationModel.Views.MainRegion.RefDataBrowser
{
  public class RefDataBrowserPresenter : PresenterBase<IRefDataBrowserView>
  {
    public RefDataBrowserPresenter(IRefDataBrowserView view, IIMPresentationModel model)
      : base(view,model)
    {

    }

  }
}
