using System.Windows.Controls;
using PrismContrib.Base;

namespace Modules.Menu.MenuRegionRight
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
