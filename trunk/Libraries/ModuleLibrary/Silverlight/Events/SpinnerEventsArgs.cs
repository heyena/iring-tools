using System;
using System.Net;
using ModuleLibrary.Types;


namespace InformationModel.Events
{
  public enum SpinnerEventType { Started, Stopped };

  public class SpinnerEventArgs : EventArgs
  {
    public SpinnerEventType Active { get; set; }
    public string ActiveService { get; set; }
  }
}
