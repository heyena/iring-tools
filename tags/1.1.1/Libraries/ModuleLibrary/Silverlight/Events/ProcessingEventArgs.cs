
using System;
using Library.Interface.Types;
namespace Library.Events
{
    public class ProcessingEventArgs : EventArgs
    {
        public ProcessingType ProcessingType { get; set; }
    }
}
