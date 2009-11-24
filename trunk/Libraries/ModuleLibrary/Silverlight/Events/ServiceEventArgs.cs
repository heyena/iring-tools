using System;
using System.Net;


namespace  Library.Interface.Events
{
    public enum ServiceProcessType { NotAssigned, Starting, Stopped };

    public class ServiceEventArgs : EventArgs
    {
        public ServiceProcessType Process { get; set; } 
        public string ServiceName { get; set; }
    }
}
