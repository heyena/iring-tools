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

        public string TaskId { get; set; }
        public int TaskType { get; set; }
        public string Project { get; set; }
        public string App { get; set; }
        public string Scope { get; set; }
        public string BaseUrl { get; set; }
        public string ExchangeId { get; set; }
        public string CachePageSize { get; set; }
        public string SsoUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string GrantType { get; set; }
        public string AppKey { get; set; }
        public string AccessToken { get; set; }
        public int RequestTimeout { get; set; }
        public string Status { get; set; }
        public int Active { get; set; }

    }
}
