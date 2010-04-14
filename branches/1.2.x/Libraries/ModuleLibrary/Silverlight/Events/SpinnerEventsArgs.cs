using System;
using System.Net;
using org.iringtools.modulelibrary.types;


namespace org.iringtools.informationmodel.events
{
  public enum SpinnerEventType { Started, Stopped };

  public class SpinnerEventArgs : EventArgs
  {
    public SpinnerEventType Active { get; set; }
    public string ActiveService { get; set; }
  }
}
