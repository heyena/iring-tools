using System.Windows.Controls;
using PrismContrib.Base;

namespace InformationModel.Views.MenuRegionRight
{
  public partial class LoginView : UserControl, ILoginView
  {
    public LoginView()
    {
      InitializeComponent();
   
    }

    public IPresentationModel Model { get; set; }

  }
}
