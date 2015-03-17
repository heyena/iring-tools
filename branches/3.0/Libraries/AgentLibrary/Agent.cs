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
                jobclientinfos = new JobClientInfos();
            }

            [DataMember(Name = "job_Id", Order = 0)]
            public Guid Job_id { get; set; }

            [DataMember(Name = "isExchange", Order = 1, EmitDefaultValue = false)]
            public Byte Is_Exchange { get; set; }

            [DataMember(Name = "scope", Order = 2, EmitDefaultValue = false)]
            public string Scope { get; set; }

            [DataMember(Name = "app", Order = 3, EmitDefaultValue = false)]
            public string App { get; set; }

            [DataMember(Name = "dataObject", Order = 4, EmitDefaultValue = false)]
            public string DataObject { get; set; }

            [DataMember(Name = "xid", Order = 5, EmitDefaultValue = false)]
            public string Xid { get; set; }

            [DataMember(Name = "exchange_Url", Order = 6, EmitDefaultValue = false)]
            public string Exchange_Url { get; set; }

            [DataMember(Name = "cache_page_size", Order = 7, EmitDefaultValue = false)]
            public string Cache_Page_size { get; set; }

            [DataMember(Name = "jobclientinfos", Order = 8, EmitDefaultValue = false)]
            public JobClientInfos jobclientinfos { get; set; }

            [DataMember(Name = "schedules", Order = 9, EmitDefaultValue = false)]
            public Schedules schedules { get; set; }

        }

        [CollectionDataContract(Name = "jobclientinfos", Namespace = "http://www.iringtools.org/library", ItemName = "jobclientinfo")]
        public class JobClientInfos : List<JobClientInfo>
        {

        }

        [DataContract(Name = "jobclientinfo", Namespace = "http://www.iringtools.org/library")]
        public class JobClientInfo
        {
            [DataMember(Name = "Job_Id", Order = 0)]
            public Guid Job_Id { get; set; }

            [DataMember(Name = "SSo_Url", Order = 1, EmitDefaultValue = false)]
            public string SSo_Url { get; set; }

            [DataMember(Name = "Client_id", Order = 2, EmitDefaultValue = false)]
            public string Client_id { get; set; }

            [DataMember(Name = "Client_Secret", Order = 3, EmitDefaultValue = false)]
            public string Client_Secret { get; set; }

            [DataMember(Name = "Grant_Type", Order = 4, EmitDefaultValue = false)]
            public string Grant_Type { get; set; }

            [DataMember(Name = "Request_Timeout", Order = 5, EmitDefaultValue = false)]
            public int Request_Timeout { get; set; }
        }


        [CollectionDataContract(Name = "schedules", Namespace = "http://www.iringtools.org/library", ItemName = "schedule")]
        public class Schedules : List<Schedule>
        {

        }

        [DataContract(Name = "schedule", Namespace = "http://www.iringtools.org/library")]
        public class Schedule
        {
            [DataMember(Name = "Schedule_Id", Order = 0)]
            public Guid Schedule_Id { get; set; }

            [DataMember(Name = "Created_DateTime", Order = 1, EmitDefaultValue = false)]
            public string Created_DateTime { get; set; }

            [DataMember(Name = "Created_By", Order = 2, EmitDefaultValue = false)]
            public string Created_By { get; set; }

            [DataMember(Name = "Occurance", Order = 3, EmitDefaultValue = false)]
            public string Occurance { get; set; }

            [DataMember(Name = "Weekday", Order = 4, EmitDefaultValue = false)]
            public string Weekday { get; set; }

            [DataMember(Name = "Start_DateTime", Order = 5, EmitDefaultValue = false)]
            public string Start_DateTime { get; set; }

            [DataMember(Name = "End_DateTime", Order = 6, EmitDefaultValue = false)]
            public string End_DateTime { get; set; }

            [DataMember(Name = "Status", Order = 7, EmitDefaultValue = false)]
            public string Status { get; set; }

        }

        [CollectionDataContract(Name = "jobschedules", Namespace = "http://www.iringtools.org/library", ItemName = "jobschedule")]
        public class JobSchedules : List<JobSchedule>
        {

        }

        [DataContract(Name = "jobschedule", Namespace = "http://www.iringtools.org/library")]
        public class JobSchedule
        {
            [DataMember(Name = "Schedule_Id", Order = 0)]
            public Guid Schedule_Id { get; set; }

            [DataMember(Name = "Job_Id", Order = 1, EmitDefaultValue = false)]
            public Guid Job_Id { get; set; }

            [DataMember(Name = "Next_Start_DateTime", Order = 2, EmitDefaultValue = false)]
            public string Next_Start_DateTime { get; set; }

            [DataMember(Name = "Last_Start_DateTime", Order = 3, EmitDefaultValue = false)]
            public string Last_Start_DateTime { get; set; }

            [DataMember(Name = "Active", Order = 4, EmitDefaultValue = false)]
            public int Active { get; set; }

        }

    }
}
