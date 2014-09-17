using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using org.iringtools.utility;
using org.iringtools.library;
using System.Collections.ObjectModel;


namespace iRINGAgentService
{
    public class AgentExchange
    {
        public string ScheduleExchangeId { get; set; }
        public string TaskName { get; set; }
        public string Scope { get; set; }
        public string BaseUrl { get; set; }
        public string ExchangeId { get; set; }
        public string SsoUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string GrantType { get; set; }
        public int RequestTimeout { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public int Active { get; set; }

    }
}
