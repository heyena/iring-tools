using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.AgentLibrary
{
    public class Agent
    {
        /// <summary>
        /// These below defined classes are corresponding to the Agent configuration tables.
        /// </summary>
        /// 

        [CollectionDataContract(Name = "jobs", Namespace = "http://www.iringtools.org/library", ItemName = "job")]
        public class Jobs : List<Job>
        {

        }

        [DataContract(Name = "job", Namespace = "http://www.iringtools.org/library")]
        public class Job
        {
            public Job()
            {
                schedules = new Schedules();
            }

            [DataMember(Name = "jobId", Order = 0)]
            public Guid JobId { get; set; }

            [DataMember(Name = "scheduleId", Order = 1)]
            public Guid ScheduleId { get; set; }

            [DataMember(Name = "dataObjectId", Order = 2)]
            public string DataObjectId { get; set; }

            [DataMember(Name = "isExchange", Order = 3)]
            public Byte Is_Exchange { get; set; }

            [DataMember(Name = "xid", Order = 4, EmitDefaultValue = false)]
            public string Xid { get; set; }

            [DataMember(Name = "cache_page_size", Order = 5, EmitDefaultValue = false)]
            public string Cache_Page_size { get; set; }

            [DataMember(Name = "platformId", Order = 6, EmitDefaultValue = false)]
            public int PlatformId { get; set; }

            [DataMember(Name = "siteId", Order = 7, EmitDefaultValue = false)]
            public int SiteId { get; set; }

            [DataMember(Name = "next_Start_DateTime", Order = 8, EmitDefaultValue = false)]
            public string Next_Start_DateTime { get; set; }

            [DataMember(Name = "last_Start_DateTime", Order = 9, EmitDefaultValue = false)]
            public string Last_Start_DateTime { get; set; }

            [DataMember(Name = "totalRecords", Order = 10, EmitDefaultValue = false)]
            public int TotalRecords { get; set; }

            [DataMember(Name = "cachedRecords", Order = 11, EmitDefaultValue = false)]
            public int CachedRecords { get; set; }

            [DataMember(Name = "active", Order = 12, EmitDefaultValue = false)]
            public Byte Active { get; set; }

            [DataMember(Name = "schedules", Order = 13, EmitDefaultValue = false)]
            public Schedules schedules { get; set; }

            [DataMember(Name = "appname", Order = 14, EmitDefaultValue = false)]
            public string AppName { get; set; }

            [DataMember(Name = "contextname", Order = 15, EmitDefaultValue = false)]
            public string ContextName { get; set; }

        }

       
        [CollectionDataContract(Name = "schedules", Namespace = "http://www.iringtools.org/library", ItemName = "schedule")]
        public class Schedules : List<Schedule>
        {

        }

        [DataContract(Name = "schedule", Namespace = "http://www.iringtools.org/library")]
        public class Schedule
        {
            [DataMember(Name = "scheduleId", Order = 0)]
            public Guid ScheduleId { get; set; }

            [DataMember(Name = "created_DateTime", Order = 1, EmitDefaultValue = false)]
            public string Created_DateTime { get; set; }

            [DataMember(Name = "created_By", Order = 2, EmitDefaultValue = false)]
            public string Created_By { get; set; }

            [DataMember(Name = "occurance", Order = 3, EmitDefaultValue = false)]
            public string Occurance { get; set; }

            [DataMember(Name = "weekday", Order = 4, EmitDefaultValue = false)]
            public string Weekday { get; set; }

            [DataMember(Name = "start_DateTime", Order = 5, EmitDefaultValue = false)]
            public string Start_DateTime { get; set; }

            [DataMember(Name = "end_DateTime", Order = 6, EmitDefaultValue = false)]
            public string End_DateTime { get; set; }

            [DataMember(Name = "status", Order = 7, EmitDefaultValue = false)]
            public string Status { get; set; }

        }

    }
}
