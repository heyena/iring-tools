using System;
using System.Net;
using org.iringtools.modulelibrary.types;


namespace org.iringtools.informationmodel.events
{
    public class SelectionEventArgs : EventArgs
    {
      public  string SelectedProject { get; set; }
      public  string SelectedApplication { get; set; }
    }
}
