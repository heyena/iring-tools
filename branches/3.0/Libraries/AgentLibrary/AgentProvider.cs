using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.adapter;
using log4net;
using System.Collections.Specialized;
using Ninject;
using System.Runtime.Serialization;
using System.IO;
using org.iringtools.library;
using System.Xml.Linq;
using org.iringtools.utility;
using System.ServiceModel.Web;
using System.Data.Linq;
using System.Web;
using System.Data;
using org.iringtools.mapping;
using org.iringtools.AgentLibrary;

namespace org.iringtools.AgentLibrary
{
    public class AgentProvider : BaseProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));
        private string _connSecurityDb;
        
        [Inject]
        public AgentProvider(NameValueCollection settings)
            : base(settings)
        {
            try
            {
                // We have _settings collection available here.
                _connSecurityDb = settings["SecurityConnection"];
              
            }
            catch (Exception e)
            {
                _logger.Error("Error initializing adapter provider: " + e.Message);
            }
        }

        public org.iringtools.AgentLibrary.Agent.Jobs GetAllJobs(int platformId, int siteId)
        {
            org.iringtools.AgentLibrary.Agent.Jobs jobs = new org.iringtools.AgentLibrary.Agent.Jobs();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@PlatformId", Value = Convert.ToString(platformId) });
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(siteId) });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgAllJobs", nvl);
                jobs = utility.Utility.Deserialize<org.iringtools.AgentLibrary.Agent.Jobs>(xmlString, true);

                //List<org.iringtools.AgentLibrary.Agent.Job> lstJob = new List<org.iringtools.AgentLibrary.Agent.Job>();

                //using (var dc = new DataContext(_connSecurityDb))
                //{
                //    lstJob = dc.ExecuteQuery<org.iringtools.AgentLibrary.Agent.Job>("spgAllJobs").ToList();
                //}

                //jobs.AddRange(lstJob);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Jobs: " + ex);
            }
            return jobs;
        }

        public org.iringtools.AgentLibrary.Agent.Job GetJob(int platformId, int siteId, string scope, string app)
        {
           
            org.iringtools.AgentLibrary.Agent.Job job = new org.iringtools.AgentLibrary.Agent.Job();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@PlatformId", Value = Convert.ToString(platformId) });
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(siteId) });
                nvl.Add(new ListItem() { Name = "@ScopeName", Value = Convert.ToString(scope) });
                nvl.Add(new ListItem() { Name = "@AppName", Value = Convert.ToString(app) });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgJob", nvl);
                job = utility.Utility.Deserialize<org.iringtools.AgentLibrary.Agent.Job>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Job: " + ex);
            }
            return job;
        }

        public Response InsertJob(int platformId, int siteId, XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            string cachePageSize = "";

            try
            {               
                org.iringtools.AgentLibrary.Agent.Job job = Utility.DeserializeDataContract<org.iringtools.AgentLibrary.Agent.Job>(xml.ToString());

                string schedulesXml = job.schedules.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (job == null)
                        PrepareErrorResponse(response, "Please enter valid Job!");
                    else
                    {
                        if (string.IsNullOrEmpty(_settings["CachePageSize"]))
                            cachePageSize = "1000";
                        else
                            cachePageSize = _settings["CachePageSize"];

                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@Is_exchange", Value = Convert.ToString(job.Is_Exchange) });
                        nvl.Add(new ListItem() { Name = "@Scope", Value = job.Scope });
                        nvl.Add(new ListItem() { Name = "@App", Value = job.App });
                        nvl.Add(new ListItem() { Name = "@DataObject", Value = job.DataObject });
                        nvl.Add(new ListItem() { Name = "@Xid", Value = job.Xid });
                        nvl.Add(new ListItem() { Name = "@Exchange_Url", Value = job.Exchange_Url });
                        nvl.Add(new ListItem() { Name = "@Cache_Page_Size", Value = cachePageSize});
                        nvl.Add(new ListItem() { Name = "@PlatformId", Value = platformId });
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = siteId });
                        nvl.Add(new ListItem() { Name = "@Schedules", Value = schedulesXml });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiJob", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "jobadded");
                                break;
                            case "0":
                                PrepareSuccessResponse(response, "duplicatejob");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Job: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateJob(string jobId, XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            string cachePageSize = "";
            try
            {
                org.iringtools.AgentLibrary.Agent.Job job = Utility.DeserializeDataContract<org.iringtools.AgentLibrary.Agent.Job>(xml.ToString());
                string schedulesXml = job.schedules.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                string jobSchedulesXml = job.jobschedules.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
              
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (job == null)
                        PrepareErrorResponse(response, "Please enter Job!");
                    else
                    {
                        if (string.IsNullOrEmpty(_settings["CachePageSize"]))
                            cachePageSize = "1000";
                        else
                            cachePageSize = _settings["CachePageSize"];

                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@Job_Id", Value = jobId });
                        nvl.Add(new ListItem() { Name = "@Is_Exchange", Value = Convert.ToString(job.Is_Exchange) });
                        nvl.Add(new ListItem() { Name = "@Scope", Value = job.Scope });
                        nvl.Add(new ListItem() { Name = "@App", Value = job.App });
                        nvl.Add(new ListItem() { Name = "@DataObject", Value = job.DataObject });
                        nvl.Add(new ListItem() { Name = "@Xid", Value = job.Xid });
                        nvl.Add(new ListItem() { Name = "@Exchange_Url", Value = job.Exchange_Url });
                        nvl.Add(new ListItem() { Name = "@Cache_Page_Size", Value = cachePageSize });
                        nvl.Add(new ListItem() { Name = "@Schedules", Value = schedulesXml });
                        nvl.Add(new ListItem() { Name = "@JobSchedules", Value = jobSchedulesXml });
                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuJob", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "jobupdated");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating Job: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteJob(string jobId)
        {
            Response response = new Response();
            response.Messages = new Messages();

        try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (string.IsNullOrEmpty(jobId))
                        PrepareErrorResponse(response, "Please enter jobid!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@JobId", Value = jobId });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdJob", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "jobdeleted");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Job: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public org.iringtools.AgentLibrary.Agent.JobClientInfos GetJobClientInfo(string Job_Id)
        {
            org.iringtools.AgentLibrary.Agent.JobClientInfos jobClientInfos = new org.iringtools.AgentLibrary.Agent.JobClientInfos();
            try
            {

                List<org.iringtools.AgentLibrary.Agent.JobClientInfo> lstJobClient = new List<org.iringtools.AgentLibrary.Agent.JobClientInfo>();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    lstJobClient = dc.ExecuteQuery<org.iringtools.AgentLibrary.Agent.JobClientInfo>("spgJob_Client_Info @Job_Id = {0}", Job_Id).ToList();
                }

                jobClientInfos.AddRange(lstJobClient);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Job Client Info: " + ex);
            }
            return jobClientInfos;
        }

        public Response InsertJobClientInfo(XDocument xml)
        {
            Response response = new Response();

            try
            {

                org.iringtools.AgentLibrary.Agent.JobClientInfo jobClientInfo = Utility.DeserializeDataContract<org.iringtools.AgentLibrary.Agent.JobClientInfo>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    NameValueList nvl = new NameValueList();
                    nvl.Add(new ListItem() { Name = "@Job_Id", Value = Convert.ToString(jobClientInfo.Job_Id) });
                    nvl.Add(new ListItem() { Name = "@SSo_Url", Value = jobClientInfo.SSo_Url });
                    nvl.Add(new ListItem() { Name = "@Client_id", Value = jobClientInfo.Client_id });
                    nvl.Add(new ListItem() { Name = "@Client_Secret", Value = jobClientInfo.Client_Secret });
                    nvl.Add(new ListItem() { Name = "@Grant_Type", Value = jobClientInfo.Grant_Type });
                    nvl.Add(new ListItem() { Name = "@Request_Timeout", Value = Convert.ToString(jobClientInfo.Request_Timeout) });

                    string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiJob_client_Info", nvl);
                    switch (output)
                    {
                        case "1":
                            PrepareSuccessResponse(response, "jobClientInfoadded");
                            break;
                        case "0":
                            PrepareSuccessResponse(response, "duplicatejobClientInfo");
                            break;
                        default:
                            PrepareErrorResponse(response, output);
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Job Client Info: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateJobClientInfo(string jobId, XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                string rawXml = xml.ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                org.iringtools.AgentLibrary.Agent.JobClientInfo jobClientInfo = Utility.DeserializeDataContract<org.iringtools.AgentLibrary.Agent.JobClientInfo>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (jobClientInfo == null)
                        PrepareErrorResponse(response, "Please enter Job!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@Job_Id", Value = jobId });
                        nvl.Add(new ListItem() { Name = "@SSo_Url", Value = jobClientInfo.SSo_Url });
                        nvl.Add(new ListItem() { Name = "@Client_id", Value = jobClientInfo.Client_id });
                        nvl.Add(new ListItem() { Name = "@Client_Secret", Value = jobClientInfo.Client_Secret });
                        nvl.Add(new ListItem() { Name = "@Grant_Type", Value = jobClientInfo.Grant_Type });
                        nvl.Add(new ListItem() { Name = "@Request_Timeout", Value = Convert.ToString(jobClientInfo.Request_Timeout) });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuJob_client_Info", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "jobclientinfoupdated");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error updating Job Client Info: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteJobClientInfo(string jobId)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (string.IsNullOrEmpty(jobId))
                        PrepareErrorResponse(response, "Please enter jobid!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@JobId", Value = jobId });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdJobClientInfo", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "jobdeclientinfoleted");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Job Client Info: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public org.iringtools.AgentLibrary.Agent.Schedule GetSchedule(string schedule_id)
        {
            List<org.iringtools.AgentLibrary.Agent.Schedule> lstSchedule = new List<org.iringtools.AgentLibrary.Agent.Schedule>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstSchedule = dc.ExecuteQuery<org.iringtools.AgentLibrary.Agent.Schedule>("spgSchedule @Schedule_Id = {0}", schedule_id).ToList();
            }

            org.iringtools.AgentLibrary.Agent.Schedule schedule = new org.iringtools.AgentLibrary.Agent.Schedule();
            if (lstSchedule.Count > 0)
                schedule = lstSchedule.First();

            return schedule;
        }

        public Response InsertSchedule(XDocument xml)
        {
            Response response = new Response();

            try
            {
                org.iringtools.AgentLibrary.Agent.Schedule schedule = Utility.DeserializeDataContract<org.iringtools.AgentLibrary.Agent.Schedule>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    NameValueList nvl = new NameValueList();
                    //nvl.Add(new ListItem() { Name = "@Schedule_Id", Value = Convert.ToString(schedule.Schedule_Id) });
                    nvl.Add(new ListItem() { Name = "@Created_DateTime", Value = schedule.Created_DateTime});
                    nvl.Add(new ListItem() { Name = "@Created_By", Value = schedule.Created_By });
                    nvl.Add(new ListItem() { Name = "@Occurance", Value = schedule.Occurance });
                    nvl.Add(new ListItem() { Name = "@Weekday", Value = schedule.Weekday });
                    nvl.Add(new ListItem() { Name = "@Start_DateTime", Value = schedule.Start_DateTime});
                    nvl.Add(new ListItem() { Name = "@End_DateTime", Value = schedule.End_DateTime});
                    nvl.Add(new ListItem() { Name = "@Status", Value = schedule.Status });
                 
                    string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiSchedule", nvl);
                    switch (output)
                    {
                        case "1":
                            PrepareSuccessResponse(response, "scheduleAdded");
                            break;
                        case "0":
                            PrepareSuccessResponse(response, "duplicateSchedule");
                            break;
                        default:
                            PrepareErrorResponse(response, output);
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Schedule: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateSchedule(string scheduleId, XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                string rawXml = xml.ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                org.iringtools.AgentLibrary.Agent.Schedule schedule = Utility.DeserializeDataContract<org.iringtools.AgentLibrary.Agent.Schedule>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (schedule == null)
                        PrepareErrorResponse(response, "Please enter schedule!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@Created_DateTime", Value = scheduleId });
                        nvl.Add(new ListItem() { Name = "@Created_By", Value = schedule.Created_By });
                        nvl.Add(new ListItem() { Name = "@Occurance", Value = schedule.Occurance });
                        nvl.Add(new ListItem() { Name = "@Weekday", Value = schedule.Weekday });
                        nvl.Add(new ListItem() { Name = "@Start_DateTime", Value = schedule.Start_DateTime});
                        nvl.Add(new ListItem() { Name = "@End_DateTime", Value = schedule.End_DateTime});
                        nvl.Add(new ListItem() { Name = "@Status", Value = schedule.Status });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuSchedule", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "scheduleupdated");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error updating Schedule: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteSchedule(string scheduleId)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (string.IsNullOrEmpty(scheduleId))
                        PrepareErrorResponse(response, "Please enter scheduleId!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@ScheduleId", Value = scheduleId });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdSchedule", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "scheduledeleted");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Schedule: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public org.iringtools.AgentLibrary.Agent.JobSchedule GetJobSchedule(string job_id, string schedule_id)
        {
            org.iringtools.AgentLibrary.Agent.JobSchedule jobSchedule = null;
            try
            {
                List<org.iringtools.AgentLibrary.Agent.JobSchedule> lstJobSchedule = new List<org.iringtools.AgentLibrary.Agent.JobSchedule>();

                using (var dc = new DataContext(_connSecurityDb))
                {
                    lstJobSchedule = dc.ExecuteQuery<org.iringtools.AgentLibrary.Agent.JobSchedule>("spgJobSchedule @Job_Id = {0}, @Schedule_Id = {1} ", job_id, schedule_id).ToList();
                }

                jobSchedule = new org.iringtools.AgentLibrary.Agent.JobSchedule();
                if (lstJobSchedule.Count > 0)
                    jobSchedule = lstJobSchedule.First();
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Job Schedule: " + ex);
            }
            return jobSchedule;
        }

        public Response InsertJobSchedule(XDocument xml)
        {
            Response response = new Response();

            try
            {

                string rawXml = xml.ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                org.iringtools.AgentLibrary.Agent.JobSchedule jobschedule = Utility.DeserializeDataContract<org.iringtools.AgentLibrary.Agent.JobSchedule>(rawXml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    NameValueList nvl = new NameValueList();
                    nvl.Add(new ListItem() { Name = "@Schedule_Id", Value = Convert.ToString(jobschedule.Schedule_Id) });
                    nvl.Add(new ListItem() { Name = "@Job_Id", Value = Convert.ToString(jobschedule.Job_Id) });
                    nvl.Add(new ListItem() { Name = "@PlatformId", Value = Convert.ToString(jobschedule.PlatformId) });
                    nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(jobschedule.SiteId) });
                    nvl.Add(new ListItem() { Name = "@Next_Start_DateTime", Value = jobschedule.Next_Start_DateTime});
                    nvl.Add(new ListItem() { Name = "@Last_Start_DateTime", Value = jobschedule.Last_Start_DateTime});
                    nvl.Add(new ListItem() { Name = "@TotalRecords", Value = Convert.ToString(jobschedule.TotalRecords) });
                    nvl.Add(new ListItem() { Name = "@CachedRecords", Value = Convert.ToString(jobschedule.CachedRecords) });
                    nvl.Add(new ListItem() { Name = "@Active", Value = Convert.ToString(jobschedule.Active) });
                   
                    string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiJobSchedule", nvl);
                    switch (output)
                    {
                        case "1":
                            PrepareSuccessResponse(response, "jobScheduleAdded");
                            break;
                        case "0":
                            PrepareSuccessResponse(response, "duplicateJobSchedule");
                            break;
                        default:
                            PrepareErrorResponse(response, output);
                            break;
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Job Schedule: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateJobSchedule(string jobId, string scheduleId, XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                string rawXml = xml.ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                org.iringtools.AgentLibrary.Agent.JobSchedule jobschedule = Utility.DeserializeDataContract<org.iringtools.AgentLibrary.Agent.JobSchedule>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (jobschedule == null)
                        PrepareErrorResponse(response, "Please enter job schedule!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@Schedule_Id", Value = scheduleId });
                        nvl.Add(new ListItem() { Name = "@Job_Id", Value = jobId });
                        nvl.Add(new ListItem() { Name = "@PlatformId", Value = Convert.ToString(jobschedule.PlatformId) });
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(jobschedule.SiteId) });
                        nvl.Add(new ListItem() { Name = "@Next_Start_DateTime", Value = jobschedule.Next_Start_DateTime });
                        nvl.Add(new ListItem() { Name = "@Last_Start_DateTime", Value = jobschedule.Last_Start_DateTime });
                        nvl.Add(new ListItem() { Name = "@TotalRecords", Value = Convert.ToString(jobschedule.TotalRecords) });
                        nvl.Add(new ListItem() { Name = "@CachedRecords", Value = Convert.ToString(jobschedule.CachedRecords) });
                        nvl.Add(new ListItem() { Name = "@Active", Value = Convert.ToString(jobschedule.Active) });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuJobSchedule", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "jobscheduleupdated");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error updating Job Schedule: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteJobSchedule(string jobId, string scheduleId)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (string.IsNullOrEmpty(jobId) || string.IsNullOrEmpty(scheduleId))
                        PrepareErrorResponse(response, "Missing jobId or scheduleId!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@JobId", Value = jobId });
                        nvl.Add(new ListItem() { Name = "@ScheduleId", Value = scheduleId });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdJobSchedule", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "jobScheduledeleted");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Job Schedule: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public XElement FormatIncomingMessage<T>(Stream stream, string format, bool isBase64encoded = false)
        {
            XElement xElement = null;

            if (format != null && (format.ToLower().Contains("xml") || format.ToLower().Contains("rdf") ||
              format.ToLower().Contains("dto")))
            {
                xElement = XElement.Load(stream);
            }
            else
            {
                T dataItems = FormatIncomingMessage<T>(stream);

                if (isBase64encoded)
                    xElement = dataItems.Serialize<T>();
                else
                    xElement = dataItems.ToXElement<T>();
            }

            return xElement;
        }

        public T FormatIncomingMessage<T>(Stream stream)
        {
            T dataItems;

            DataItemSerializer serializer = new DataItemSerializer(
                _settings["JsonIdField"], _settings["JsonLinksField"], bool.Parse(_settings["DisplayLinks"]));
            string json = Utility.ReadString(stream);
            dataItems = serializer.Deserialize<T>(json, true);
            stream.Close();

            return dataItems;
        }

        public void FormatOutgoingMessage<T>(T graph, string format, bool useDataContractSerializer)
        {
            if (format.ToUpper() == "JSON")
            {
                string json = Utility.SerializeJson<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
                HttpContext.Current.Response.Write(json);
            }
            else
            {
                string xml = Utility.Serialize<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/xml";
                HttpContext.Current.Response.Write(xml);
            }
        }



        #region Private Methods
        private void PrepareErrorResponse(Response response, string errMsg)
        {
            Status status = new Status { Level = StatusLevel.Error };
            status.Messages = new Messages { errMsg };
            response.DateTimeStamp = DateTime.Now;
            response.Level = StatusLevel.Error;
            response.StatusList.Add(status);

        }
        private void PrepareSuccessResponse(Response response, string errMsg)
        {
            Status status = new Status { Level = StatusLevel.Success };
            status.Messages = new Messages { errMsg };
            response.DateTimeStamp = DateTime.Now;
            response.Level = StatusLevel.Success;
            response.StatusList.Add(status);
        }
        #endregion

    }
}
