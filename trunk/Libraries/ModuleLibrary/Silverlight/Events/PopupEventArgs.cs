using System;
using System.Net;

namespace org.iringtools.library.presentation.events
{
    public enum PopupProcessType { Show, Close };
    
    public class PopupEventArgs : EventArgs
    {
        public PopupProcessType Process { get; set; }
    }
}
