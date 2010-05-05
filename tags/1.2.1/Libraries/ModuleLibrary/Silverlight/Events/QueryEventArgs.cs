
using org.iringtools.ontologyservice.presentation.types;
using System;

namespace org.iringtools.ontologyservice.presentation.events
{
    public class QueryEventArgs : EventArgs
    {
        public QueryType QueryType { get; set; }
        public string SearchFor { get; set; }
        public string Offset { get; set; }
    }
}
