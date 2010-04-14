using System;
using InformationModel.UserControls;
using InformationModel.Types;

namespace InformationModel.Events
{
  public class CustomTabEventArgs : EventArgs
  {
    public CustomTabItem ActiveTab { get; set; }
    public CustomTabProcess Process { get; set; }
 
  }
}
