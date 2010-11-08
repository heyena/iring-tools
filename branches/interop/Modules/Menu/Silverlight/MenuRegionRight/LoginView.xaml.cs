using System.Windows.Controls;
using PrismContrib.Base;

namespace org.iringtools.modules.menu.menuregionright
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
