using System;
using System.Net;

namespace Library.Interface.Events
{
    public enum PopupProcessType { Show, Close };
    
    public class PopupEventArgs : EventArgs
    {
        public PopupProcessType Process { get; set; }
    }
}
