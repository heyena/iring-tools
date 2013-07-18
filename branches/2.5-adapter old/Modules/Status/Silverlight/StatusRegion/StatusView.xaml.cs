using System.Windows.Controls;
using PrismContrib.Base;

namespace org.iringtools.modules.status.statusregion
{
  public partial class StatusView : UserControl, IStatusView
  {
    public StatusView()
    {
      InitializeComponent();
    }

    public IPresentationModel Model { get; set; }

    public string stsLeftMessage
    {
      get
      {
        return stsLeft1.Text;
      }
      set
      {
        stsLeft1.Text = value;
      }
    } 
    
    public string stsRightMessage
    {
      get
      {
        return stsRight1.Text;
      }
      set
      {
        stsRight1.Text = value;
      }
    } 
  }
}
