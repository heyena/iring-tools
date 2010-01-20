
using System;
using org.iringtools.library.presentation.types;
namespace org.iringtools.library.events
{
    public class ProcessingEventArgs : EventArgs
    {
        public ProcessingType ProcessingType { get; set; }
    }
}
