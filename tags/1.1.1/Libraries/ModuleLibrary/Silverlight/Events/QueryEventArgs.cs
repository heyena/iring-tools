
using OntologyService.Interface.Types;
using System;

namespace OntologyService.Interface.Events
{
    public class QueryEventArgs : EventArgs
    {
        public QueryType QueryType { get; set; }
        public string SearchFor { get; set; }
        public string Offset { get; set; }
    }
}
