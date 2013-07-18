using System.Windows.Controls;
using PrismContrib.Base;

namespace InformationModel.Views.StatusRegion
{
  public partial class StatusView : UserControl, IStatusView
  {
    public StatusView()
    {
      InitializeComponent();
    }

    public IPresentationModel Model { get; set; }


    #region IStatusView Members

    #region stsLeftMessage 
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
    #endregion
    #region stsRightMessage 
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
    #endregion
    #region stsMiddleMessage 
    public string stsMiddleMessage
    {
      get
      {
        return stsCenter1.Text;
      }
      set
      {
        stsCenter1.Text = value;
      }
    }
    
    #endregion

    #endregion
  }
}
