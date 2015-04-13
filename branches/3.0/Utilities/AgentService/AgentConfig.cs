using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using org.iringtools.utility;
using org.iringtools.library;
using System.Collections.ObjectModel;


namespace iRINGAgentService
{
    public class AgentConfig
    {

        public string JobId { get; set; }
        public int IsExchange { get; set; }
        public string Scope { get; set; }
        public string App { get; set; }
        public string DataObject { get; set; }
        public string ExchangeId { get; set; }
        public string ExchangeUrl { get; set; }
        public string CachePageSize { get; set; }
        public string SsoUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string GrantType { get; set; }
        public int RequestTimeout { get; set; }
        public string ScheduleId { get; set; }
        public string Occurance { get; set; }
        public string Weekday { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Status { get; set; }
        public DateTime NextStartDateTime { get; set; }
        public DateTime LastStartDateTime { get; set; }
        public int Active { get; set; }

    }
}
